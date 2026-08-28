#pragma once

#include <cstddef>
#include <cstdint>

#ifdef __cplusplus
extern "C" {
#endif

#define CNR64_HOTPATCH_ABI_VERSION 1u

// Plugin return codes.
enum Cnr64HotpatchResultV1 : int {
    CNR64_HOTPATCH_PASS = 0,
    CNR64_HOTPATCH_HANDLED = 1,
    CNR64_HOTPATCH_RETRY_GUEST = 2,
    CNR64_HOTPATCH_HALT = 3,
};

struct Cnr64HotpatchContextV1 {
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

    // Useful host-side state for advanced bridge plugins.
    void* host_native_window;
    const char* managed_dir;

    // Opaque environment plus helpers. Plugins should use these rather than
    // assuming the wrapper's internal C++ object layout.
    void* host_context;
    std::uint32_t (*allocate_guest)(void* host_context, std::uint32_t size, std::uint32_t alignment);
    void (*log_message)(void* host_context, const char* message);

    // Developer scheduler controls. These are appended so existing V1 plugins
    // remain binary-compatible; older plugins simply never read these fields.
    std::uint32_t (*find_cond_waiter_thread)(void* host_context, std::uint32_t cond);
    void (*prefer_guest_thread)(void* host_context, std::uint32_t thread_id);
};

typedef int (*Cnr64HotpatchDispatchV1)(Cnr64HotpatchContextV1* context);
typedef const char* (*Cnr64HotpatchNameV1)();

#ifdef __cplusplus
}
#endif
