#include "unity_guest_probe.h"

#include <cstdint>
#include <cstring>
#include <fstream>
#include <optional>
#include <sstream>
#include <string>
#include <vector>

#include "dynarmic/interface/A32/a32.h"

namespace {
constexpr std::uint32_t kGuestBase = 0x01000000;
constexpr std::uint32_t kReturnStub = 0x00008000;
constexpr std::uint32_t kAttachStub = 0x00008100;
constexpr std::uint32_t kFindClassStub = 0x00008110;
constexpr std::uint32_t kRegisterNativesStub = 0x00008120;
constexpr std::uint32_t kFatalErrorStub = 0x00008130;
constexpr std::uint32_t kVmObject = 0x00009000;
constexpr std::uint32_t kVmTable = 0x00009100;
constexpr std::uint32_t kEnvObject = 0x00009200;
constexpr std::uint32_t kEnvTable = 0x00009300;
constexpr std::uint32_t kFakeClassHandle = 0x0000a000;
constexpr std::uint32_t kStackTop = 0x01f00000;
constexpr std::size_t kGuestMemorySize = 32u * 1024u * 1024u;

constexpr std::uint32_t kSvcReturn = 0;
constexpr std::uint32_t kSvcAttach = 1;
constexpr std::uint32_t kSvcFindClass = 2;
constexpr std::uint32_t kSvcRegisterNatives = 3;
constexpr std::uint32_t kSvcFatalError = 4;
constexpr std::uint32_t kJniVersion16 = 0x00010006;

constexpr std::uint16_t kElfTypeSharedObject = 3;
constexpr std::uint16_t kElfMachineArm = 40;
constexpr std::uint32_t kProgramTypeLoad = 1;
constexpr std::uint32_t kSectionTypeRel = 9;
constexpr std::uint32_t kSectionTypeDynsym = 11;
constexpr std::uint32_t kRelArmRelative = 23;

#pragma pack(push, 1)
struct Elf32Ehdr {
    unsigned char ident[16];
    std::uint16_t type;
    std::uint16_t machine;
    std::uint32_t version;
    std::uint32_t entry;
    std::uint32_t phoff;
    std::uint32_t shoff;
    std::uint32_t flags;
    std::uint16_t ehsize;
    std::uint16_t phentsize;
    std::uint16_t phnum;
    std::uint16_t shentsize;
    std::uint16_t shnum;
    std::uint16_t shstrndx;
};

struct Elf32Phdr {
    std::uint32_t type;
    std::uint32_t offset;
    std::uint32_t vaddr;
    std::uint32_t paddr;
    std::uint32_t filesz;
    std::uint32_t memsz;
    std::uint32_t flags;
    std::uint32_t align;
};

struct Elf32Shdr {
    std::uint32_t name;
    std::uint32_t type;
    std::uint32_t flags;
    std::uint32_t addr;
    std::uint32_t offset;
    std::uint32_t size;
    std::uint32_t link;
    std::uint32_t info;
    std::uint32_t addralign;
    std::uint32_t entsize;
};

struct Elf32Sym {
    std::uint32_t name;
    std::uint32_t value;
    std::uint32_t size;
    std::uint8_t info;
    std::uint8_t other;
    std::uint16_t shndx;
};

struct Elf32Rel {
    std::uint32_t offset;
    std::uint32_t info;
};
#pragma pack(pop)

static_assert(sizeof(Elf32Ehdr) == 52);
static_assert(sizeof(Elf32Phdr) == 32);
static_assert(sizeof(Elf32Shdr) == 40);
static_assert(sizeof(Elf32Sym) == 16);
static_assert(sizeof(Elf32Rel) == 8);

bool RangeFits(std::size_t offset, std::size_t size, std::size_t total) {
    return offset <= total && size <= total - offset;
}

template <typename T>
bool ReadStruct(const std::vector<std::uint8_t>& bytes, std::size_t offset, T& out) {
    if (!RangeFits(offset, sizeof(T), bytes.size())) return false;
    std::memcpy(&out, bytes.data() + offset, sizeof(T));
    return true;
}

bool LoadFile(const std::string& path, std::vector<std::uint8_t>& bytes, std::string& error) {
    std::ifstream file(path, std::ios::binary | std::ios::ate);
    if (!file) {
        error = "Could not open guest ELF: " + path;
        return false;
    }
    const std::streamsize size = file.tellg();
    if (size <= 0) {
        error = "Guest ELF is empty.";
        return false;
    }
    file.seekg(0, std::ios::beg);
    bytes.resize(static_cast<std::size_t>(size));
    if (!file.read(reinterpret_cast<char*>(bytes.data()), size)) {
        error = "Could not read guest ELF.";
        return false;
    }
    return true;
}

bool GetSection(const std::vector<std::uint8_t>& file, const Elf32Ehdr& ehdr,
                std::uint32_t index, Elf32Shdr& out) {
    if (index >= ehdr.shnum || ehdr.shentsize < sizeof(Elf32Shdr)) return false;
    return ReadStruct(file,
                      static_cast<std::size_t>(ehdr.shoff) +
                          static_cast<std::size_t>(index) * ehdr.shentsize,
                      out);
}

std::string ReadString(const std::vector<std::uint8_t>& file,
                       const Elf32Shdr& strtab,
                       std::uint32_t offset) {
    if (offset >= strtab.size || !RangeFits(strtab.offset, strtab.size, file.size())) return {};
    const std::size_t start = static_cast<std::size_t>(strtab.offset) + offset;
    const std::size_t limit = static_cast<std::size_t>(strtab.offset) + strtab.size;
    std::size_t end = start;
    while (end < limit && file[end] != 0) ++end;
    return std::string(reinterpret_cast<const char*>(file.data() + start), end - start);
}

bool FindDynamicSymbol(const std::vector<std::uint8_t>& file,
                       const Elf32Ehdr& ehdr,
                       const char* preferred,
                       const char* fallback,
                       std::uint32_t& value,
                       std::string& foundName) {
    for (std::uint32_t i = 0; i < ehdr.shnum; ++i) {
        Elf32Shdr symtab{};
        if (!GetSection(file, ehdr, i, symtab) ||
            symtab.type != kSectionTypeDynsym ||
            symtab.entsize < sizeof(Elf32Sym)) {
            continue;
        }
        Elf32Shdr strtab{};
        if (!GetSection(file, ehdr, symtab.link, strtab)) continue;
        if (!RangeFits(symtab.offset, symtab.size, file.size())) continue;
        const std::size_t count = symtab.size / symtab.entsize;
        for (std::size_t n = 0; n < count; ++n) {
            Elf32Sym sym{};
            if (!ReadStruct(file,
                            static_cast<std::size_t>(symtab.offset) + n * symtab.entsize,
                            sym)) {
                continue;
            }
            const std::string name = ReadString(file, strtab, sym.name);
            if (name == preferred || (fallback && name == fallback)) {
                value = sym.value;
                foundName = name;
                return true;
            }
        }
    }
    return false;
}

bool Read32(const std::vector<std::uint8_t>& memory,
            std::uint32_t address,
            std::uint32_t& value) {
    if (address > memory.size() || sizeof(value) > memory.size() - address) return false;
    std::memcpy(&value, memory.data() + address, sizeof(value));
    return true;
}

bool Write32(std::vector<std::uint8_t>& memory,
             std::uint32_t address,
             std::uint32_t value) {
    if (address > memory.size() || sizeof(value) > memory.size() - address) return false;
    std::memcpy(memory.data() + address, &value, sizeof(value));
    return true;
}

std::string ReadGuestCString(const std::vector<std::uint8_t>& memory,
                             std::uint32_t address,
                             std::size_t maxLen = 256) {
    if (address >= memory.size()) return {};
    std::string result;
    for (std::size_t i = 0; i < maxLen && address + i < memory.size(); ++i) {
        const char ch = static_cast<char>(memory[address + i]);
        if (ch == '\0') break;
        result.push_back(ch);
    }
    return result;
}

bool WriteTrapStub(std::vector<std::uint8_t>& memory,
                   std::uint32_t address,
                   std::uint32_t svc) {
    return Write32(memory, address, 0xef000000u | (svc & 0x00ffffffu)) &&
           Write32(memory, address + 4, 0xe12fff1eu); // bx lr
}

class UnityEnvironment final : public Dynarmic::A32::UserCallbacks {
public:
    explicit UnityEnvironment(std::vector<std::uint8_t>& memory) : memory_(memory) {}

    Dynarmic::A32::Jit* jit = nullptr;
    bool saw_return_trap = false;
    bool failed = false;
    std::uint32_t fault_pc = 0;
    std::uint64_t ticks_left = 1000000;

    bool jni_mode = false;
    bool saw_attach = false;
    bool saw_fatal_error = false;
    int jni_event_limit = 0;
    int jni_event_count = 0;
    bool halted_at_jni_limit = false;
    std::vector<std::string> requested_classes;
    std::vector<std::uint32_t> registered_native_counts;

    std::optional<std::uint32_t> MemoryReadCode(std::uint32_t address) override {
        if (address > memory_.size() || 4 > memory_.size() - address) return std::nullopt;
        return Read<std::uint32_t>(address);
    }

    std::uint8_t MemoryRead8(std::uint32_t address) override { return Read<std::uint8_t>(address); }
    std::uint16_t MemoryRead16(std::uint32_t address) override { return Read<std::uint16_t>(address); }
    std::uint32_t MemoryRead32(std::uint32_t address) override { return Read<std::uint32_t>(address); }
    std::uint64_t MemoryRead64(std::uint32_t address) override { return Read<std::uint64_t>(address); }

    void MemoryWrite8(std::uint32_t address, std::uint8_t value) override { Write(address, value); }
    void MemoryWrite16(std::uint32_t address, std::uint16_t value) override { Write(address, value); }
    void MemoryWrite32(std::uint32_t address, std::uint32_t value) override { Write(address, value); }
    void MemoryWrite64(std::uint32_t address, std::uint64_t value) override { Write(address, value); }

    void InterpreterFallback(std::uint32_t pc, std::size_t) override {
        failed = true;
        fault_pc = pc;
        if (jit) jit->HaltExecution();
    }

    void CallSVC(std::uint32_t swi) override {
        if (!jit) return;
        auto& regs = jit->Regs();

        if (swi == kSvcReturn) {
            saw_return_trap = true;
            jit->HaltExecution();
            return;
        }

        if (!jni_mode) {
            failed = true;
            fault_pc = regs[15];
            jit->HaltExecution();
            return;
        }

        switch (swi) {
        case kSvcAttach:
            saw_attach = true;
            Write(regs[1], kEnvObject);
            regs[0] = 0; // JNI_OK
            FinishJniEvent();
            break;
        case kSvcFindClass:
            requested_classes.push_back(ReadGuestCString(memory_, regs[1]));
            regs[0] = kFakeClassHandle;
            FinishJniEvent();
            break;
        case kSvcRegisterNatives:
            registered_native_counts.push_back(regs[3]);
            regs[0] = 0; // JNI_OK
            FinishJniEvent();
            break;
        case kSvcFatalError:
            saw_fatal_error = true;
            failed = true;
            jit->HaltExecution();
            break;
        default:
            failed = true;
            fault_pc = regs[15];
            jit->HaltExecution();
            break;
        }
    }

    void ExceptionRaised(std::uint32_t pc, Dynarmic::A32::Exception) override {
        failed = true;
        fault_pc = pc;
        if (jit) jit->HaltExecution();
    }

    void AddTicks(std::uint64_t ticks) override {
        ticks_left = ticks >= ticks_left ? 0 : ticks_left - ticks;
    }

    std::uint64_t GetTicksRemaining() override { return ticks_left; }

private:
    void FinishJniEvent() {
        ++jni_event_count;
        if (jni_event_limit > 0 && jni_event_count >= jni_event_limit) {
            halted_at_jni_limit = true;
            // Do not halt Dynarmic from inside the JNI callback. Instead,
            // redirect the guest stub's BX LR to the same return trap used by
            // the already-proven-safe execution probes. This lets the current
            // ARM32 stub finish normally before the JIT stops.
            if (jit) jit->Regs()[14] = kReturnStub;
        }
    }

    template <typename T>
    T Read(std::uint32_t address) const {
        T value{};
        if (address > memory_.size() || sizeof(T) > memory_.size() - address) return value;
        std::memcpy(&value, memory_.data() + address, sizeof(T));
        return value;
    }

    template <typename T>
    void Write(std::uint32_t address, T value) {
        if (address > memory_.size() || sizeof(T) > memory_.size() - address) {
            failed = true;
            fault_pc = address;
            return;
        }
        std::memcpy(memory_.data() + address, &value, sizeof(T));
    }

    std::vector<std::uint8_t>& memory_;
};
} // namespace

UnityGuestProbeResult RunOriginalLibUnityProbe(const std::string& path, int jniEventLimit) {
    std::ostringstream report;
    report << "\nOriginal CNR libunity.so ELF probe:\n";

    std::vector<std::uint8_t> file;
    std::string error;
    if (!LoadFile(path, file, error)) {
        report << "  ELF file load: FAIL (" << error << ")";
        return {false, report.str()};
    }

    Elf32Ehdr ehdr{};
    if (!ReadStruct(file, 0, ehdr) ||
        ehdr.ident[0] != 0x7f || ehdr.ident[1] != 'E' ||
        ehdr.ident[2] != 'L' || ehdr.ident[3] != 'F' ||
        ehdr.ident[4] != 1 || ehdr.ident[5] != 1 ||
        ehdr.type != kElfTypeSharedObject || ehdr.machine != kElfMachineArm) {
        report << "  ELF validation: FAIL (not ARM32 little-endian ET_DYN)";
        return {false, report.str()};
    }
    report << "  ELF validation: PASS (ELF32 ARM ET_DYN, " << file.size() << " bytes)\n";

    std::vector<std::uint8_t> memory(kGuestMemorySize, 0);
    std::size_t loadSegments = 0;
    std::uint32_t highestGuestAddress = 0;
    for (std::uint32_t i = 0; i < ehdr.phnum; ++i) {
        Elf32Phdr phdr{};
        if (!ReadStruct(file,
                        static_cast<std::size_t>(ehdr.phoff) +
                            static_cast<std::size_t>(i) * ehdr.phentsize,
                        phdr)) {
            report << "  PT_LOAD mapping: FAIL (bad program header)";
            return {false, report.str()};
        }
        if (phdr.type != kProgramTypeLoad) continue;

        const std::uint64_t guestAddress = static_cast<std::uint64_t>(kGuestBase) + phdr.vaddr;
        const std::uint64_t guestEnd = guestAddress + phdr.memsz;
        if (!RangeFits(phdr.offset, phdr.filesz, file.size()) || guestEnd > memory.size()) {
            report << "  PT_LOAD mapping: FAIL (segment outside 32 MB guest memory)";
            return {false, report.str()};
        }

        if (phdr.filesz > 0) {
            std::memcpy(memory.data() + guestAddress, file.data() + phdr.offset, phdr.filesz);
        }
        if (phdr.memsz > phdr.filesz) {
            std::memset(memory.data() + guestAddress + phdr.filesz, 0, phdr.memsz - phdr.filesz);
        }
        highestGuestAddress = static_cast<std::uint32_t>(guestEnd);
        ++loadSegments;
    }

    if (loadSegments == 0) {
        report << "  PT_LOAD mapping: FAIL (no loadable segments)";
        return {false, report.str()};
    }
    report << "  PT_LOAD mapping: PASS (" << loadSegments << " segments, base=0x"
           << std::hex << kGuestBase << ", end=0x" << highestGuestAddress << std::dec << ")\n";

    std::size_t relativeRelocations = 0;
    std::size_t deferredRelocations = 0;
    for (std::uint32_t i = 0; i < ehdr.shnum; ++i) {
        Elf32Shdr relsec{};
        if (!GetSection(file, ehdr, i, relsec) ||
            relsec.type != kSectionTypeRel ||
            relsec.entsize < sizeof(Elf32Rel)) {
            continue;
        }
        if (!RangeFits(relsec.offset, relsec.size, file.size())) {
            report << "  Relocations: FAIL (bad relocation section)";
            return {false, report.str()};
        }

        const std::size_t count = relsec.size / relsec.entsize;
        for (std::size_t n = 0; n < count; ++n) {
            Elf32Rel rel{};
            if (!ReadStruct(file,
                            static_cast<std::size_t>(relsec.offset) + n * relsec.entsize,
                            rel)) {
                continue;
            }
            const std::uint32_t type = rel.info & 0xffu;
            if (type == kRelArmRelative) {
                const std::uint32_t target = kGuestBase + rel.offset;
                std::uint32_t addend = 0;
                if (!Read32(memory, target, addend) ||
                    !Write32(memory, target, addend + kGuestBase)) {
                    report << "  Relocations: FAIL (R_ARM_RELATIVE target out of range)";
                    return {false, report.str()};
                }
                ++relativeRelocations;
            } else {
                ++deferredRelocations;
            }
        }
    }
    report << "  R_ARM_RELATIVE applied: PASS (" << relativeRelocations << ")\n";
    report << "  Deferred external relocations: " << deferredRelocations
           << " (not needed by this probe)\n";

    std::uint32_t symbolValue = 0;
    std::string symbolName;
    if (!FindDynamicSymbol(file, ehdr, "__aeabi_idiv", "__divsi3", symbolValue, symbolName)) {
        report << "  Dynamic symbol lookup: FAIL (__aeabi_idiv/__divsi3 not found)";
        return {false, report.str()};
    }
    const std::uint32_t functionAddress = kGuestBase + (symbolValue & ~1u);
    report << "  Dynamic symbol lookup: PASS (" << symbolName << " @ guest 0x"
           << std::hex << functionAddress << std::dec << ")\n";

    if (!WriteTrapStub(memory, kReturnStub, kSvcReturn) ||
        !WriteTrapStub(memory, kAttachStub, kSvcAttach) ||
        !WriteTrapStub(memory, kFindClassStub, kSvcFindClass) ||
        !WriteTrapStub(memory, kRegisterNativesStub, kSvcRegisterNatives) ||
        !WriteTrapStub(memory, kFatalErrorStub, kSvcFatalError)) {
        report << "  Bridge trap setup: FAIL";
        return {false, report.str()};
    }

    UnityEnvironment env(memory);
    Dynarmic::A32::UserConfig config;
    config.callbacks = &env;
    config.arch_version = Dynarmic::A32::ArchVersion::v7;
    config.always_little_endian = true;
    config.enable_cycle_counting = true;
    config.code_cache_size = 32u * 1024u * 1024u;

    Dynarmic::A32::Jit jit(config);
    env.jit = &jit;
    jit.Regs().fill(0);
    jit.Regs()[0] = 100;
    jit.Regs()[1] = 7;
    jit.Regs()[13] = kStackTop;
    jit.Regs()[14] = kReturnStub;
    jit.Regs()[15] = functionAddress;
    jit.SetCpsr((symbolValue & 1u) ? 0x20u : 0u);
    jit.Run();

    const std::uint32_t quotient = jit.Regs()[0];
    const bool executionOk = !env.failed && env.saw_return_trap && quotient == 14;
    report << "  Execute untouched Unity ARM code: " << (executionOk ? "PASS" : "FAIL")
           << " (100 / 7 = " << quotient << ")";
    if (env.failed) {
        report << " fault_pc=0x" << std::hex << env.fault_pc << std::dec;
    }
    report << "\n";

    if (!executionOk) {
        return {false, report.str()};
    }
    report << "  Result: original libunity.so code executed inside the ARM64 process.\n";

    if (jniEventLimit < 0) {
        report << "  Unity JNI_OnLoad: SKIPPED (use staged JNI test buttons).";
        return {true, report.str()};
    }

    // Now exercise Unity's real JNI_OnLoad. The function stores the JavaVM in
    // Unity global state, attaches the current thread, finds three Java classes,
    // and registers three native method tables (25 + 6 + 2 methods).
    Write32(memory, kVmObject, kVmTable);
    Write32(memory, kVmTable + 0x10, kAttachStub);             // AttachCurrentThread
    Write32(memory, kEnvObject, kEnvTable);
    Write32(memory, kEnvTable + 0x18, kFindClassStub);         // FindClass
    Write32(memory, kEnvTable + 0x35c, kRegisterNativesStub);  // RegisterNatives
    Write32(memory, kEnvTable + 0x48, kFatalErrorStub);        // FatalError

    std::uint32_t jniSymbol = 0;
    std::string jniName;
    if (!FindDynamicSymbol(file, ehdr, "JNI_OnLoad", nullptr, jniSymbol, jniName)) {
        report << "  Unity JNI_OnLoad symbol lookup: FAIL";
        return {false, report.str()};
    }
    const std::uint32_t jniAddress = kGuestBase + (jniSymbol & ~1u);
    report << "  Unity JNI_OnLoad symbol lookup: PASS (@ guest 0x"
           << std::hex << jniAddress << std::dec << ")\n";

    env.failed = false;
    env.fault_pc = 0;
    env.saw_return_trap = false;
    env.ticks_left = 1000000;
    env.jni_mode = true;
    env.saw_attach = false;
    env.saw_fatal_error = false;
    env.jni_event_limit = jniEventLimit;
    env.jni_event_count = 0;
    env.halted_at_jni_limit = false;
    env.requested_classes.clear();
    env.registered_native_counts.clear();
    jit.ClearHalt();
    jit.Regs().fill(0);
    jit.Regs()[0] = kVmObject;
    jit.Regs()[1] = 0;
    jit.Regs()[13] = kStackTop;
    jit.Regs()[14] = kReturnStub;
    jit.Regs()[15] = jniAddress;
    jit.SetCpsr((jniSymbol & 1u) ? 0x20u : 0u);
    jit.Run();

    if (jniEventLimit > 0) {
        const bool stageOk = !env.failed && env.halted_at_jni_limit && env.saw_return_trap &&
                             env.jni_event_count == jniEventLimit;
        report << "  Unity JNI staged checkpoint: " << (stageOk ? "PASS" : "FAIL")
               << " (events=" << env.jni_event_count << "/" << jniEventLimit << ")\n";
        report << "    AttachCurrentThread: " << (env.saw_attach ? "SEEN" : "not yet") << "\n";
        report << "    FindClass calls: " << env.requested_classes.size() << "\n";
        for (std::size_t i = 0; i < env.requested_classes.size(); ++i) {
            report << "      [" << i << "] " << env.requested_classes[i] << "\n";
        }
        report << "    RegisterNatives calls: " << env.registered_native_counts.size() << "\n";
        for (std::size_t i = 0; i < env.registered_native_counts.size(); ++i) {
            report << "      [" << i << "] count=" << env.registered_native_counts[i] << "\n";
        }
        if (env.failed) {
            report << "    fault_pc=0x" << std::hex << env.fault_pc << std::dec << "\n";
        }
        return {stageOk, report.str()};
    }

    const std::uint32_t jniVersion = jit.Regs()[0];
    const std::vector<std::string> expectedClasses = {
        "com/unity3d/player/UnityPlayer",
        "org/fmod/FMODAudioDevice",
        "com/unity3d/player/ReflectionHelper"
    };
    const std::vector<std::uint32_t> expectedCounts = {25, 6, 2};
    const bool classesOk = env.requested_classes == expectedClasses;
    const bool countsOk = env.registered_native_counts == expectedCounts;
    const bool jniOk = !env.failed && env.saw_return_trap && env.saw_attach &&
                       !env.saw_fatal_error && classesOk && countsOk &&
                       jniVersion == kJniVersion16;

    report << "  Unity ARM32->ARM64 JNI trap bridge: " << (jniOk ? "PASS" : "FAIL") << "\n";
    report << "    AttachCurrentThread: " << (env.saw_attach ? "PASS" : "MISS") << "\n";
    report << "    FindClass sequence: " << (classesOk ? "PASS" : "FAIL") << "\n";
    for (std::size_t i = 0; i < env.requested_classes.size(); ++i) {
        report << "      [" << i << "] " << env.requested_classes[i] << "\n";
    }
    report << "    RegisterNatives counts: " << (countsOk ? "PASS" : "FAIL") << " (";
    for (std::size_t i = 0; i < env.registered_native_counts.size(); ++i) {
        if (i) report << ",";
        report << env.registered_native_counts[i];
    }
    report << ")\n";
    report << "    JNI_OnLoad return: 0x" << std::hex << jniVersion << std::dec
           << (jniVersion == kJniVersion16 ? " (JNI_VERSION_1_6)" : "") << "\n";
    if (env.failed) {
        report << "    fault_pc=0x" << std::hex << env.fault_pc << std::dec << "\n";
    }
    if (jniOk) {
        report << "  Result: original Unity JNI_OnLoad crossed the 32->64 ABI bridge successfully.";
    }

    return {jniOk, report.str()};
}

UnityGuestProbeResult RunOriginalLibMonoProbe(const std::string& path) {
    std::ostringstream report;
    report << "\nOriginal CNR libmono.so ELF probe:\n";

    std::vector<std::uint8_t> file;
    std::string error;
    if (!LoadFile(path, file, error)) {
        report << "  ELF file load: FAIL (" << error << ")";
        return {false, report.str()};
    }

    Elf32Ehdr ehdr{};
    if (!ReadStruct(file, 0, ehdr) ||
        ehdr.ident[0] != 0x7f || ehdr.ident[1] != 'E' ||
        ehdr.ident[2] != 'L' || ehdr.ident[3] != 'F' ||
        ehdr.ident[4] != 1 || ehdr.ident[5] != 1 ||
        ehdr.type != kElfTypeSharedObject || ehdr.machine != kElfMachineArm) {
        report << "  ELF validation: FAIL (not ARM32 little-endian ET_DYN)";
        return {false, report.str()};
    }
    report << "  ELF validation: PASS (ELF32 ARM ET_DYN, " << file.size() << " bytes)\n";

    std::vector<std::uint8_t> memory(kGuestMemorySize, 0);
    std::size_t loadSegments = 0;
    std::uint32_t highestGuestAddress = 0;
    for (std::uint32_t i = 0; i < ehdr.phnum; ++i) {
        Elf32Phdr phdr{};
        if (!ReadStruct(file,
                        static_cast<std::size_t>(ehdr.phoff) +
                            static_cast<std::size_t>(i) * ehdr.phentsize,
                        phdr)) {
            report << "  PT_LOAD mapping: FAIL (bad program header)";
            return {false, report.str()};
        }
        if (phdr.type != kProgramTypeLoad) continue;

        const std::uint64_t guestAddress = static_cast<std::uint64_t>(kGuestBase) + phdr.vaddr;
        const std::uint64_t guestEnd = guestAddress + phdr.memsz;
        if (!RangeFits(phdr.offset, phdr.filesz, file.size()) || guestEnd > memory.size()) {
            report << "  PT_LOAD mapping: FAIL (segment outside 32 MB guest memory)";
            return {false, report.str()};
        }
        if (phdr.filesz > 0) {
            std::memcpy(memory.data() + guestAddress, file.data() + phdr.offset, phdr.filesz);
        }
        if (phdr.memsz > phdr.filesz) {
            std::memset(memory.data() + guestAddress + phdr.filesz, 0, phdr.memsz - phdr.filesz);
        }
        highestGuestAddress = static_cast<std::uint32_t>(guestEnd);
        ++loadSegments;
    }
    if (loadSegments == 0) {
        report << "  PT_LOAD mapping: FAIL (no loadable segments)";
        return {false, report.str()};
    }
    report << "  PT_LOAD mapping: PASS (" << loadSegments << " segments, base=0x"
           << std::hex << kGuestBase << ", end=0x" << highestGuestAddress << std::dec << ")\n";

    std::size_t relativeRelocations = 0;
    std::size_t deferredRelocations = 0;
    for (std::uint32_t i = 0; i < ehdr.shnum; ++i) {
        Elf32Shdr relsec{};
        if (!GetSection(file, ehdr, i, relsec) ||
            relsec.type != kSectionTypeRel ||
            relsec.entsize < sizeof(Elf32Rel)) {
            continue;
        }
        if (!RangeFits(relsec.offset, relsec.size, file.size())) {
            report << "  Relocations: FAIL (bad relocation section)";
            return {false, report.str()};
        }
        const std::size_t count = relsec.size / relsec.entsize;
        for (std::size_t n = 0; n < count; ++n) {
            Elf32Rel rel{};
            if (!ReadStruct(file,
                            static_cast<std::size_t>(relsec.offset) + n * relsec.entsize,
                            rel)) {
                continue;
            }
            const std::uint32_t type = rel.info & 0xffu;
            if (type == kRelArmRelative) {
                const std::uint32_t target = kGuestBase + rel.offset;
                std::uint32_t addend = 0;
                if (!Read32(memory, target, addend) ||
                    !Write32(memory, target, addend + kGuestBase)) {
                    report << "  Relocations: FAIL (R_ARM_RELATIVE target out of range)";
                    return {false, report.str()};
                }
                ++relativeRelocations;
            } else {
                ++deferredRelocations;
            }
        }
    }
    report << "  R_ARM_RELATIVE applied: PASS (" << relativeRelocations << ")\n";
    report << "  Deferred external relocations: " << deferredRelocations
           << " (not needed by this probe)\n";

    std::uint32_t symbolValue = 0;
    std::string symbolName;
    if (!FindDynamicSymbol(file, ehdr, "mono_get_root_domain", nullptr, symbolValue, symbolName)) {
        report << "  Dynamic symbol lookup: FAIL (mono_get_root_domain not found)";
        return {false, report.str()};
    }
    const std::uint32_t functionAddress = kGuestBase + (symbolValue & ~1u);
    report << "  Dynamic symbol lookup: PASS (" << symbolName << " @ guest 0x"
           << std::hex << functionAddress << std::dec << ")\n";

    if (!WriteTrapStub(memory, kReturnStub, kSvcReturn)) {
        report << "  Return trap setup: FAIL";
        return {false, report.str()};
    }

    UnityEnvironment env(memory);
    Dynarmic::A32::UserConfig config;
    config.callbacks = &env;
    config.arch_version = Dynarmic::A32::ArchVersion::v7;
    config.always_little_endian = true;
    config.enable_cycle_counting = true;
    config.code_cache_size = 16u * 1024u * 1024u;

    Dynarmic::A32::Jit jit(config);
    env.jit = &jit;
    jit.Regs().fill(0);
    jit.Regs()[13] = kStackTop;
    jit.Regs()[14] = kReturnStub;
    jit.Regs()[15] = functionAddress;
    jit.SetCpsr((symbolValue & 1u) ? 0x20u : 0u);
    jit.Run();

    const std::uint32_t rootDomain = jit.Regs()[0];
    const bool executionOk = !env.failed && env.saw_return_trap && rootDomain == 0;
    report << "  Execute untouched Mono ARM code: " << (executionOk ? "PASS" : "FAIL")
           << " (mono_get_root_domain before init = 0x" << std::hex << rootDomain << std::dec << ")";
    if (env.failed) {
        report << " fault_pc=0x" << std::hex << env.fault_pc << std::dec;
    }
    report << "\n";
    if (executionOk) {
        report << "  Result: original libmono.so code executed inside the ARM64 process.";
    }
    return {executionOk, report.str()};
}
