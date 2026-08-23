#include "a32_test_core.h"

#include <cstdint>
#include <cstring>
#include <optional>
#include <sstream>
#include <vector>

#include "dynarmic/interface/A32/a32.h"
#include "elf_guest_probe.h"
#include "unity_guest_probe.h"
#include "shared_guest_linker.h"

namespace {
constexpr std::uint32_t kCodeBase = 0x1000;
constexpr std::size_t kGuestMemorySize = 64 * 1024;

class TestEnvironment final : public Dynarmic::A32::UserCallbacks {
public:
    explicit TestEnvironment(std::vector<std::uint8_t>& memory) : memory_(memory) {}

    Dynarmic::A32::Jit* jit = nullptr;
    bool saw_svc = false;
    bool failed = false;
    std::uint64_t ticks_left = 1000;

    std::optional<std::uint32_t> MemoryReadCode(std::uint32_t address) override {
        if (address + 4 > memory_.size()) return std::nullopt;
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

    void InterpreterFallback(std::uint32_t, std::size_t) override {
        failed = true;
        if (jit) jit->HaltExecution();
    }

    void CallSVC(std::uint32_t) override {
        saw_svc = true;
        if (jit) jit->HaltExecution();
    }

    void ExceptionRaised(std::uint32_t, Dynarmic::A32::Exception) override {
        failed = true;
        if (jit) jit->HaltExecution();
    }

    void AddTicks(std::uint64_t ticks) override {
        ticks_left = ticks >= ticks_left ? 0 : ticks_left - ticks;
    }

    std::uint64_t GetTicksRemaining() override { return ticks_left; }

private:
    template <typename T>
    T Read(std::uint32_t address) const {
        T value{};
        if (address + sizeof(T) > memory_.size()) return value;
        std::memcpy(&value, memory_.data() + address, sizeof(T));
        return value;
    }

    template <typename T>
    void Write(std::uint32_t address, T value) {
        if (address + sizeof(T) > memory_.size()) {
            failed = true;
            return;
        }
        std::memcpy(memory_.data() + address, &value, sizeof(T));
    }

    std::vector<std::uint8_t>& memory_;
};

void WriteInstruction(std::vector<std::uint8_t>& memory, std::uint32_t address, std::uint32_t instruction) {
    std::memcpy(memory.data() + address, &instruction, sizeof(instruction));
}

bool RunProgram(Dynarmic::A32::Jit& jit, TestEnvironment& env, std::uint32_t expected, std::uint32_t& actual) {
    env.saw_svc = false;
    env.failed = false;
    env.ticks_left = 1000;
    jit.ClearHalt();
    jit.Regs().fill(0);
    jit.Regs()[15] = kCodeBase;
    jit.SetCpsr(0);
    jit.Run();
    actual = jit.Regs()[0];
    return !env.failed && env.saw_svc && actual == expected;
}
} // namespace

A32SelfTestResult RunA32SelfTest(const std::string& originalLibMainPath,
                                const std::string& originalLibUnityPath,
                                const std::string& originalLibMonoPath,
                                const std::string& managedDirPath,
                                const std::string& packageCodePath,
                                void* hostNativeWindow) {
    std::ostringstream report;
#if defined(__aarch64__)
    report << "Host ISA: AArch64\n";
#else
    report << "Host ISA: NOT AARCH64\n";
#endif
    report << "Guest ISA: ARMv7 A32\n\n";

    std::vector<std::uint8_t> memory(kGuestMemorySize, 0);
    TestEnvironment env(memory);

    Dynarmic::A32::UserConfig config;
    config.callbacks = &env;
    config.arch_version = Dynarmic::A32::ArchVersion::v7;
    config.always_little_endian = true;
    config.enable_cycle_counting = true;
    config.code_cache_size = 16 * 1024 * 1024;

    Dynarmic::A32::Jit jit(config);
    env.jit = &jit;

    WriteInstruction(memory, kCodeBase + 0, 0xE3A00028); // mov r0,#40
    WriteInstruction(memory, kCodeBase + 4, 0xE2800002); // add r0,r0,#2
    WriteInstruction(memory, kCodeBase + 8, 0xEF000000); // svc #0

    std::uint32_t actual1 = 0;
    const bool first_ok = RunProgram(jit, env, 42, actual1);
    report << "Synthetic ARMv7 execution: " << (first_ok ? "PASS" : "FAIL")
           << " (r0=" << actual1 << ")\n";

    WriteInstruction(memory, kCodeBase + 0, 0xE3A00064); // mov r0,#100
    WriteInstruction(memory, kCodeBase + 4, 0xE2800017); // add r0,r0,#23
    jit.InvalidateCacheRange(kCodeBase, 12);

    std::uint32_t actual2 = 0;
    const bool rewrite_ok = RunProgram(jit, env, 123, actual2);
    report << "Runtime code rewrite + cache invalidate: " << (rewrite_ok ? "PASS" : "FAIL")
           << " (r0=" << actual2 << ")\n";

    const ElfGuestProbeResult elfResult = RunOriginalLibMainProbe(originalLibMainPath);
    report << elfResult.report << "\n";

    const UnityGuestProbeResult unityResult = RunOriginalLibUnityProbe(originalLibUnityPath, -1);
    report << unityResult.report << "\n";

    const UnityGuestProbeResult monoResult = RunOriginalLibMonoProbe(originalLibMonoPath);
    report << monoResult.report << "\n";

    const SharedGuestLinkResult sharedResult = RunSharedGuestLinkProbe(originalLibMainPath,
                                                                       originalLibUnityPath,
                                                                       originalLibMonoPath,
                                                                       managedDirPath,
                                                                       packageCodePath,
                                                                       hostNativeWindow);
    report << sharedResult.report << "\n";

    const bool ok = first_ok && rewrite_ok && elfResult.ok && unityResult.ok && monoResult.ok && sharedResult.ok;
    report << "\nCNR64 Dynarmic PoC: " << (ok ? "PASS" : "FAIL") << "\n";
    report << "Android process is 64-bit; original CNR ARM32 code is treated as guest data.";

    return {ok, report.str()};
}
