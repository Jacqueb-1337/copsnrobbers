#include "arm32_guest_image.h"
#include "dynarmic/interface/A32/a32.h"

#include <algorithm>
#include <cstdint>
#include <cstring>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <iterator>
#include <map>
#include <optional>
#include <string>
#include <string_view>
#include <vector>

// PV7 development-only execution probe: the first actual ARM instructions
// of the owner's original Cocos ELF run under Dynarmic. All unknown imports
// halt at a named SVC, not at fabricated successful return values.
// This does NOT implement Android, rendering, or a complete JNI environment.
namespace {
constexpr std::uint32_t kImageBase = 0x01000000;
constexpr std::uint32_t kImportsBase = 0x01800000;
constexpr std::uint32_t kObjectsBase = 0x01900000;
constexpr std::uint32_t kReturnTrap = 0x00008000;
constexpr std::uint32_t kGetEnvStub = 0x00008100;
constexpr std::uint32_t kAttachStub = 0x00008110;
constexpr std::uint32_t kFindClassStub = 0x00008120;
constexpr std::uint32_t kRegisterStub = 0x00008130;
constexpr std::uint32_t kGetVersionStub = 0x00008140;
constexpr std::uint32_t kVM = 0x00009000;
constexpr std::uint32_t kVMTable = 0x00009100;
constexpr std::uint32_t kEnv = 0x00009200;
constexpr std::uint32_t kEnvTable = 0x00009300;
constexpr std::uint32_t kStack = 0x01f00000;
constexpr std::size_t kMemorySize = 32 * 1024 * 1024;
constexpr std::uint32_t kImportSvcBase = 0x00001000;
constexpr std::uint32_t kJniGetEnv = 20;
constexpr std::uint32_t kJniAttach = 21;
constexpr std::uint32_t kJniFindClass = 22;
constexpr std::uint32_t kJniRegister = 23;
constexpr std::uint32_t kJniGetVersion = 24;

bool Fits(std::size_t address, std::size_t size, std::size_t capacity) {
    return address <= capacity && size <= capacity - address;
}
bool Put32(std::vector<std::uint8_t>& mem, std::uint32_t address, std::uint32_t val) {
    if (!Fits(address, sizeof(val), mem.size())) return false;
    std::memcpy(mem.data() + address, &val, sizeof(val));
    return true;
}
bool Trap(std::vector<std::uint8_t>& mem, std::uint32_t address, std::uint32_t id) {
    return Put32(mem, address, 0xef000000u | id) &&
           Put32(mem, address + 4, 0xe12fff1eu); // BX LR
}
std::string CString(const std::vector<std::uint8_t>& mem, std::uint32_t addr) {
    std::string out;
    if (addr >= mem.size()) return out;
    for (std::size_t i = 0; i < 128 && addr + i < mem.size(); ++i) {
        if (!mem[addr + i]) break;
        out.push_back(static_cast<char>(mem[addr + i]));
    }
    return out;
}
struct Import {
    std::string name;
    std::uint32_t address;
    bool data = false;
};
class Bridge final : public Dynarmic::A32::UserCallbacks {
public:
    explicit Bridge(std::vector<std::uint8_t>& memory,
                    const std::vector<Import>& imports)
        : mem_(memory), imports_(imports) {}
    Dynarmic::A32::Jit* jit = nullptr;
    bool returned = false;
    bool faulted = false;
    std::uint32_t fault_address = 0;
    std::string reason;
    std::uint64_t ticks_remaining = 1000000;

    std::optional<std::uint32_t> MemoryReadCode(std::uint32_t a) override {
        if (!Fits(a, 4, mem_.size())) { Fault(a, "code_fetch_out_of_range"); return std::nullopt; }
        return Read<std::uint32_t>(a);
    }
    std::uint8_t MemoryRead8(std::uint32_t a) override { return Read<std::uint8_t>(a); }
    std::uint16_t MemoryRead16(std::uint32_t a) override { return Read<std::uint16_t>(a); }
    std::uint32_t MemoryRead32(std::uint32_t a) override { return Read<std::uint32_t>(a); }
    std::uint64_t MemoryRead64(std::uint32_t a) override { return Read<std::uint64_t>(a); }
    void MemoryWrite8(std::uint32_t a, std::uint8_t v) override { Write(a,v); }
    void MemoryWrite16(std::uint32_t a, std::uint16_t v) override { Write(a,v); }
    void MemoryWrite32(std::uint32_t a, std::uint32_t v) override { Write(a,v); }
    void MemoryWrite64(std::uint32_t a, std::uint64_t v) override { Write(a,v); }
    void InterpreterFallback(std::uint32_t pc, std::size_t) override { Fault(pc, "interpreter_fallback"); }
    void ExceptionRaised(std::uint32_t pc, Dynarmic::A32::Exception) override { Fault(pc, "guest_exception"); }
    void AddTicks(std::uint64_t ticks) override {
        ticks_remaining = ticks >= ticks_remaining ? 0 : ticks_remaining - ticks;
    }
    std::uint64_t GetTicksRemaining() override { return ticks_remaining; }
    void CallSVC(std::uint32_t svc) override {
        if (!jit) return;
        auto& regs = jit->Regs();
        if (svc == 0) {
            returned = true;
            jit->HaltExecution();
            return;
        }
        if (svc == kJniGetEnv || svc == kJniAttach) {
            if (!Put32(mem_, regs[1], kEnv)) { Fault(regs[1], "jni_out_pointer_invalid"); return; }
            regs[0] = 0; // JNI_OK only for these two specifically modeled operations
            return;
        }
        if (svc == kJniGetVersion) { regs[0] = 0x00010006; return; }
        if (svc == kJniFindClass) {
            reason = "JNI_FindClass:" + CString(mem_, regs[1]);
            Fault(regs[15], reason); // fail closed until genuine Java class bridge exists
            return;
        }
        if (svc == kJniRegister) {
            reason = "JNI_RegisterNatives_count:" + std::to_string(regs[3]);
            Fault(regs[15], reason);
            return;
        }
        if (svc >= kImportSvcBase && svc < kImportSvcBase + imports_.size()) {
            const auto& item = imports_[svc - kImportSvcBase];
            reason = "UNIMPLEMENTED_IMPORT:" + item.name;
            std::cout << "IMPORT_TRAP " << item.name
                      << " pc=0x" << std::hex << regs[15]
                      << " lr=0x" << regs[14]
                      << " r0=0x" << regs[0] << " r1=0x" << regs[1]
                      << " r2=0x" << regs[2] << " r3=0x" << regs[3]
                      << std::dec << "\n";
            Fault(regs[15], reason);
            return;
        }
        Fault(regs[15], "unhandled_svc:" + std::to_string(svc));
    }
private:
    template<typename T> T Read(std::uint32_t a) {
        T v{};
        if (!Fits(a, sizeof(T), mem_.size())) { Fault(a, "read_out_of_range"); return v; }
        std::memcpy(&v, mem_.data() + a, sizeof(T));
        return v;
    }
    template<typename T> void Write(std::uint32_t a, T v) {
        if (!Fits(a, sizeof(T), mem_.size())) { Fault(a, "write_out_of_range"); return; }
        std::memcpy(mem_.data() + a, &v, sizeof(T));
    }
    void Fault(std::uint32_t address, std::string why) {
        if (faulted) return;
        faulted = true;
        fault_address = address;
        reason = std::move(why);
        if (jit) jit->HaltExecution();
    }
    std::vector<std::uint8_t>& mem_;
    const std::vector<Import>& imports_;
};
} // namespace

int main(int argc, char** argv) {
    if (argc != 2 && argc != 3) {
        std::cerr << "Usage: pv7-cocos-firstcall libgame.so [JNI_OnLoad|nativeRender]\n";
        return 2;
    }
    const std::string requested = argc == 3 ? argv[2] : "JNI_OnLoad";
    const char* entry_name = nullptr;
    if (requested == "JNI_OnLoad") entry_name = "JNI_OnLoad";
    else if (requested == "nativeRender") entry_name = "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender";
    else { std::cerr << "Unknown entrypoint\n"; return 2; }

    std::ifstream source(argv[1], std::ios::binary);
    if (!source) { std::cerr << "Cannot read ELF file\n"; return 2; }
    std::vector<std::uint8_t> elf(std::istreambuf_iterator<char>{source}, {});
    pv7::Arm32GuestImage image;
    std::string err;
    if (!image.Load(elf, kImageBase, err)) {
        std::cerr << "LOAD_FAILED " << err << "\n"; return 2;
    }
    const auto entry = image.FindExport(entry_name);
    if (!entry) { std::cerr << "ENTRY_MISSING " << entry_name << "\n"; return 2; }
    if (kImageBase + image.span() >= kImportsBase) {
        std::cerr << "IMAGE_OVERLAPS_THUNKS\n"; return 2;
    }

    std::map<std::string, bool> data_imports;
    for (const auto& i : image.imported_symbols())
        if (i.symbol_type == 1) data_imports[i.name] = true;
    std::map<std::string, std::uint32_t> addresses;
    std::vector<Import> imports;
    unsigned data_count=0;
    auto resolver = [&](std::string_view value) -> std::optional<std::uint32_t> {
        const std::string name(value);
        if (auto it = addresses.find(name); it != addresses.end()) return it->second;
        const bool is_data = data_imports.find(name) != data_imports.end();
        if (imports.size() >= 4096) return std::nullopt;
        const std::uint32_t addr = is_data
            ? kObjectsBase + data_count++ * 128u
            : kImportsBase + static_cast<std::uint32_t>(imports.size()) * 16u;
        addresses[name] = addr;
        imports.push_back({name, addr, is_data});
        return addr;
    };
    if (!image.Relocate(resolver, err)) {
        std::cerr << "RELOCATE_FAILED " << err << "\n"; return 2;
    }

    std::vector<std::uint8_t> memory(kMemorySize, 0);
    if (!image.CopyToGuestMemory(memory, err)) {
        std::cerr << "MAP_FAILED " << err << "\n"; return 2;
    }
    for (std::size_t i = 0; i < imports.size(); ++i) {
        if (!imports[i].data &&
            !Trap(memory, imports[i].address, kImportSvcBase + static_cast<std::uint32_t>(i))) {
            std::cerr << "THUNK_FAILED " << imports[i].name << "\n"; return 2;
        }
    }
    // Minimal invocation interface scaffolding. Unknown JNI operations halt.
    if (!Trap(memory, kReturnTrap, 0) ||
        !Trap(memory, kGetEnvStub, kJniGetEnv) ||
        !Trap(memory, kAttachStub, kJniAttach) ||
        !Trap(memory, kFindClassStub, kJniFindClass) ||
        !Trap(memory, kRegisterStub, kJniRegister) ||
        !Trap(memory, kGetVersionStub, kJniGetVersion) ||
        !Put32(memory, kVM, kVMTable) ||
        !Put32(memory, kVMTable + 4 * 4, kAttachStub) ||
        !Put32(memory, kVMTable + 6 * 4, kGetEnvStub) ||
        !Put32(memory, kEnv, kEnvTable) ||
        !Put32(memory, kEnvTable + 6 * 4, kFindClassStub) ||
        !Put32(memory, kEnvTable + 215 * 4, kRegisterStub) ||
        !Put32(memory, kEnvTable + 4 * 4, kGetVersionStub)) {
        std::cerr << "JNI_SCAFFOLD_FAILED\n"; return 2;
    }
    Bridge bridge(memory, imports);
    Dynarmic::A32::UserConfig conf;
    conf.callbacks = &bridge;
    conf.arch_version = Dynarmic::A32::ArchVersion::v7;
    conf.always_little_endian = true;
    conf.enable_cycle_counting = true;
    conf.code_cache_size = 16 * 1024 * 1024;
    Dynarmic::A32::Jit jit(conf);
    bridge.jit = &jit;
    jit.Regs().fill(0);
    jit.Regs()[0] = kVM; // first argument of JNI_OnLoad
    jit.Regs()[13] = kStack;
    jit.Regs()[14] = kReturnTrap;
    jit.Regs()[15] = *entry & ~1u;
    jit.SetCpsr((*entry & 1u) ? 0x20u : 0u);
    std::cout << "EXECUTING_REAL_ARM32 entry=" << entry_name
              << " pc=0x" << std::hex << *entry << std::dec
              << " imports=" << imports.size()
              << " image=" << image.span()
              << " constructors_not_run=" << image.constructors().size()
              << "\n";
    jit.Run();
    if (bridge.faulted) {
        std::cout << "GUEST_STOP " << bridge.reason
                  << " fault_address=0x" << std::hex << bridge.fault_address << std::dec
                  << "\n";
        return 3; // expected actionable missing service, not success
    }
    if (bridge.returned) {
        std::cout << "GUEST_RETURN r0=0x" << std::hex << jit.Regs()[0]
                  << std::dec << " (constructors and Android services are still unavailable)\n";
        return 0;
    }
    std::cout << "GUEST_STOP instruction_budget_exhausted_or_stalled pc=0x"
              << std::hex << jit.Regs()[15] << std::dec << "\n";
    return 4;
}
