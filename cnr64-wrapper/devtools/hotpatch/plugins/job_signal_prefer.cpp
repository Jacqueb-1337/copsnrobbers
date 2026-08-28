#include "cnr64_hotpatch_api.h"

#include <cstdint>
#include <cstdio>
#include <cstring>

namespace {
constexpr std::uint32_t kGlobalJobCond = 0x01ba30acu;
constexpr std::uint32_t kWorkerCompleteLr = 0x013c4c7cu;
std::uint32_t g_log_count = 0;
}

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "ProjectV7 Unity job signal preference";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION ||
        !ctx->symbol || !ctx->regs || !ctx->find_cond_waiter_thread ||
        !ctx->prefer_guest_thread) {
        return CNR64_HOTPATCH_PASS;
    }
    if (std::strcmp(ctx->symbol, "pthread_cond_signal") != 0)
        return CNR64_HOTPATCH_PASS;

    const std::uint32_t cond = ctx->regs[0];
    const bool global_job_wake = cond == kGlobalJobCond;
    const bool worker_job_complete =
        ctx->thread_id == 2u && ctx->regs[14] == kWorkerCompleteLr;
    if (!global_job_wake && !worker_job_complete)
        return CNR64_HOTPATCH_PASS;

    const std::uint32_t waiter = ctx->find_cond_waiter_thread(ctx->host_context, cond);
    if (waiter == 0u || waiter == ctx->thread_id)
        return CNR64_HOTPATCH_PASS;

    // The global queue condition should wake the dedicated Unity job worker.
    if (global_job_wake && waiter != 2u)
        return CNR64_HOTPATCH_PASS;

    ctx->prefer_guest_thread(ctx->host_context, waiter);
    if (ctx->log_message && g_log_count < 32u) {
        char text[192]{};
        std::snprintf(text, sizeof(text),
                      "job signal preference: signaler=%u cond=0x%08x waiter=%u kind=%s",
                      ctx->thread_id, cond, waiter,
                      global_job_wake ? "dispatch" : "complete");
        ctx->log_message(ctx->host_context, text);
        ++g_log_count;
    }
    return CNR64_HOTPATCH_PASS;
}
