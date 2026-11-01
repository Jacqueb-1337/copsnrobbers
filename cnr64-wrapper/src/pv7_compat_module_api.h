#pragma once

#include <cstddef>
#include <cstdint>

#ifdef __cplusplus
extern "C" {
#endif

#define PV7_COMPAT_MODULE_ABI_VERSION 1u

// Generic compatibility-module return codes. Modules may observe a bridge call,
// handle it completely, request that the guest instruction be retried after
// modifying state, or halt the current guest execution.
enum Pv7CompatModuleResultV1 : int {
    PV7_COMPAT_PASS = 0,
    PV7_COMPAT_HANDLED = 1,
    PV7_COMPAT_RETRY_GUEST = 2,
    PV7_COMPAT_HALT = 3,
};

struct Pv7GuestInstructionPatchV1 {
    const char* name;
    std::uint32_t address;
    std::uint32_t expected_instruction;
    std::uint32_t replacement_instruction;
};

struct Pv7CompatModuleContextV1 {
    std::uint32_t abi_version;
    const char* symbol;
    std::uint32_t svc_id;
    std::uint32_t thread_id;

    // ARM32 guest state. regs points to r0-r15 and may be modified in-place.
    std::uint32_t* regs;
    std::uint32_t* ext_regs;
    std::uint32_t ext_reg_count;
    std::uint32_t cpsr;
    std::uint32_t fpscr;

    // Direct access to the emulated ARM32 address space.
    std::uint8_t* guest_memory;
    std::uint32_t guest_memory_size;

    // Host-side state exposed through stable helpers rather than PV7 internals.
    void* host_native_window;
    const char* managed_dir;
    void* host_context;
    std::uint32_t (*allocate_guest)(void* host_context, std::uint32_t size, std::uint32_t alignment);
    void (*log_message)(void* host_context, const char* message);

    // Cooperative scheduler helpers. These are intentionally generic so a
    // compatibility module can resolve timing/synchronization quirks without
    // hard-coding a game into PV7 core.
    std::uint32_t (*find_cond_waiter_thread)(void* host_context, std::uint32_t cond);
    void (*prefer_guest_thread)(void* host_context, std::uint32_t thread_id);
    std::uint32_t (*find_guest_thread_by_name)(void* host_context, const char* name);
    void (*configure_guest_thread_sampling)(void* host_context, std::uint32_t thread_id,
                                            std::uint64_t ticks_per_slice, std::uint32_t sample_limit);
};

typedef int (*Pv7CompatModuleDispatchV1)(Pv7CompatModuleContextV1* context);
typedef const char* (*Pv7CompatModuleNameV1)();

#ifdef __cplusplus
}
#endif
