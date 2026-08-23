#include "cnr64_hotpatch_api.h"

#include <cstdint>
#include <cstdio>
#include <cstring>

namespace {
struct WaitCounter {
    std::uint32_t thread_id = 0;
    std::uint32_t cond = 0;
    std::uint32_t calls = 0;
};

WaitCounter g_waits[64]{};

std::uint32_t BumpWait(std::uint32_t thread_id, std::uint32_t cond) {
    WaitCounter* empty = nullptr;
    for (auto& slot : g_waits) {
        if (slot.thread_id == thread_id && slot.cond == cond) return ++slot.calls;
        if (!empty && slot.thread_id == 0) empty = &slot;
    }
    if (!empty) return 1;
    empty->thread_id = thread_id;
    empty->cond = cond;
    empty->calls = 1;
    return 1;
}

void Log(Cnr64HotpatchContextV1* ctx, const char* event, std::uint32_t cond,
         std::uint32_t mutex, std::uint32_t count) {
    if (!ctx || !ctx->log_message) return;
    char line[192]{};
    std::snprintf(line, sizeof(line),
                  "PV7CONDTRACE %s thread=%u cond=0x%08x mutex=0x%08x lr=0x%08x count=%u",
                  event, ctx->thread_id, cond, mutex,
                  ctx->regs ? ctx->regs[14] : 0u, count);
    ctx->log_message(ctx->host_context, line);
}
} // namespace

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "ProjectV7 condvar trace";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION || !ctx->symbol || !ctx->regs)
        return CNR64_HOTPATCH_PASS;

    const std::uint32_t cond = ctx->regs[0];
    if (std::strcmp(ctx->symbol, "pthread_cond_signal") == 0) {
        Log(ctx, "signal", cond, 0u, 1u);
    } else if (std::strcmp(ctx->symbol, "pthread_cond_broadcast") == 0) {
        Log(ctx, "broadcast", cond, 0u, 1u);
    } else if (std::strcmp(ctx->symbol, "pthread_cond_wait") == 0 ||
               std::strcmp(ctx->symbol, "pthread_cond_timedwait") == 0 ||
               std::strcmp(ctx->symbol, "pthread_cond_timedwait_monotonic_np") == 0) {
        const std::uint32_t count = BumpWait(ctx->thread_id, cond);
        if (count <= 8u || (count % 64u) == 0u)
            Log(ctx, "wait", cond, ctx->regs[1], count);
    }
    return CNR64_HOTPATCH_PASS;
}
