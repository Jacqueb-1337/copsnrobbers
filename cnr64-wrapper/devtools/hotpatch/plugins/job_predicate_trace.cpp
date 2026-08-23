#include "cnr64_hotpatch_api.h"

#include <cstdint>
#include <cstdio>
#include <cstring>

namespace {
bool Read32(const Cnr64HotpatchContextV1* ctx, std::uint32_t address, std::uint32_t& value) {
    if (!ctx || !ctx->guest_memory || address > ctx->guest_memory_size - 4u) return false;
    const auto* p = ctx->guest_memory + address;
    value = static_cast<std::uint32_t>(p[0]) |
            (static_cast<std::uint32_t>(p[1]) << 8) |
            (static_cast<std::uint32_t>(p[2]) << 16) |
            (static_cast<std::uint32_t>(p[3]) << 24);
    return true;
}

void LogJob(Cnr64HotpatchContextV1* ctx, const char* event) {
    if (!ctx || !ctx->regs || !ctx->log_message) return;
    const std::uint32_t cond = ctx->regs[0];
    if (cond < 8u) return;
    const std::uint32_t job = cond - 8u;
    std::uint32_t f0=0, f4=0, fc=0, f10=0, f14=0;
    if (!Read32(ctx, job + 0x00u, f0) || !Read32(ctx, job + 0x04u, f4) ||
        !Read32(ctx, job + 0x0cu, fc) || !Read32(ctx, job + 0x10u, f10) ||
        !Read32(ctx, job + 0x14u, f14)) return;
    char line[320]{};
    std::snprintf(line, sizeof(line),
                  "PV7JOB %s thread=%u job=0x%08x cond=0x%08x lr=0x%08x sp=0x%08x f0=0x%08x f4=0x%08x fc=0x%08x f10=0x%08x f14=0x%08x r1=0x%08x r2=0x%08x r3=0x%08x",
                  event, ctx->thread_id, job, cond, ctx->regs[14], ctx->regs[13],
                  f0, f4, fc, f10, f14, ctx->regs[1], ctx->regs[2], ctx->regs[3]);
    ctx->log_message(ctx->host_context, line);
}
} // namespace

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "ProjectV7 job predicate trace";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION || !ctx->symbol || !ctx->regs)
        return CNR64_HOTPATCH_PASS;

    if (std::strcmp(ctx->symbol, "pthread_cond_signal") == 0 && ctx->regs[14] == 0x013c4c7cu) {
        LogJob(ctx, "worker-signal");
    } else if ((std::strcmp(ctx->symbol, "pthread_cond_wait") == 0 ||
                std::strcmp(ctx->symbol, "pthread_cond_timedwait") == 0 ||
                std::strcmp(ctx->symbol, "pthread_cond_timedwait_monotonic_np") == 0) &&
               ctx->regs[14] == 0x013ce68cu) {
        LogJob(ctx, "main-wait");
    }
    return CNR64_HOTPATCH_PASS;
}
