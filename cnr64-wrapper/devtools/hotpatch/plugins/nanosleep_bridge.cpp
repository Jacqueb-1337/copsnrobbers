#include "cnr64_hotpatch_api.h"

#include <algorithm>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#if !defined(__ANDROID__)
#include <chrono>
#include <thread>
#endif

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "CNR64 nanosleep bridge";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION || !ctx->symbol || !ctx->regs)
        return CNR64_HOTPATCH_PASS;
    if (std::strcmp(ctx->symbol, "nanosleep") != 0)
        return CNR64_HOTPATCH_PASS;
    // Keep worker-thread sleeps on the wrapper's cooperative scheduler. This
    // extension only provides host-time pacing for Unity's guest main thread.
    if (ctx->thread_id != 1u)
        return CNR64_HOTPATCH_PASS;

    const std::uint32_t guest_req = ctx->regs[0];
    if (!ctx->guest_memory || guest_req > ctx->guest_memory_size ||
        ctx->guest_memory_size - guest_req < 8u) {
        ctx->regs[0] = static_cast<std::uint32_t>(-1);
        return CNR64_HOTPATCH_HANDLED;
    }

    std::uint32_t sec32 = 0;
    std::uint32_t nsec32 = 0;
    std::memcpy(&sec32, ctx->guest_memory + guest_req, 4);
    std::memcpy(&nsec32, ctx->guest_memory + guest_req + 4, 4);
    if (nsec32 >= 1000000000u) nsec32 = 999999999u;

    static bool logged = false;
    if (!logged && ctx->log_message) {
        char text[160]{};
        std::snprintf(text, sizeof(text),
                      "live nanosleep bridge active: requested=%u.%09u sec",
                      sec32, nsec32);
        ctx->log_message(ctx->host_context, text);
        logged = true;
    }

    // Keep the development harness responsive even if old Unity asks for an
    // unexpectedly long sleep. Normal frame pacing requests are much smaller.
    std::uint64_t total_ns = static_cast<std::uint64_t>(sec32) * 1000000000ull + nsec32;
    total_ns = std::min<std::uint64_t>(total_ns, 1000000ull);
    timespec host_req{};
    host_req.tv_sec = static_cast<time_t>(total_ns / 1000000000ull);
    host_req.tv_nsec = static_cast<long>(total_ns % 1000000000ull);
#if defined(__ANDROID__)
    ::nanosleep(&host_req, nullptr);
#else
    std::this_thread::sleep_for(std::chrono::nanoseconds(total_ns));
#endif

    if (ctx->regs[1] != 0 && ctx->regs[1] <= ctx->guest_memory_size &&
        ctx->guest_memory_size - ctx->regs[1] >= 8u) {
        const std::uint64_t zero = 0;
        std::memcpy(ctx->guest_memory + ctx->regs[1], &zero, 8);
    }
    ctx->regs[0] = 0;
    return CNR64_HOTPATCH_HANDLED;
}
