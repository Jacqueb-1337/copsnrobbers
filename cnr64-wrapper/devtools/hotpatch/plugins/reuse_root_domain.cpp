#include "cnr64_hotpatch_api.h"
#include <cstdint>
#include <cstring>
#include <cstdio>
namespace {
constexpr std::uint32_t kUnityMonoJitInitGot = 0x01a8924cu;
constexpr std::uint32_t kExistingRootDomain = 0x05038ee0u;
bool write_u32(Cnr64HotpatchContextV1* ctx, std::uint32_t address, std::uint32_t value) {
    if (!ctx || !ctx->guest_memory || address > ctx->guest_memory_size - 4u) return false;
    std::memcpy(ctx->guest_memory + address, &value, sizeof(value));
    return true;
}
}
extern "C" const char* cnr64_hotpatch_name_v1() { return "ProjectV7 reuse existing Mono root domain"; }
extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION) return CNR64_HOTPATCH_PASS;
    static bool applied = false;
    if (applied || !ctx->allocate_guest || !ctx->guest_memory) return CNR64_HOTPATCH_PASS;
    const std::uint32_t stub = ctx->allocate_guest(ctx->host_context, 12u, 4u);
    if (!stub || stub > ctx->guest_memory_size - 12u) return CNR64_HOTPATCH_PASS;
    if (!write_u32(ctx, stub + 0u, 0xe59f0000u) ||
        !write_u32(ctx, stub + 4u, 0xe12fff1eu) ||
        !write_u32(ctx, stub + 8u, kExistingRootDomain) ||
        !write_u32(ctx, kUnityMonoJitInitGot, stub)) return CNR64_HOTPATCH_PASS;
    applied = true;
    if (ctx->log_message) {
        char message[160]{};
        std::snprintf(message, sizeof(message), "patched Unity mono_jit_init_version GOT 0x%08x -> stub 0x%08x returning root 0x%08x", kUnityMonoJitInitGot, stub, kExistingRootDomain);
        ctx->log_message(ctx->host_context, message);
    }
    return CNR64_HOTPATCH_PASS;
}
