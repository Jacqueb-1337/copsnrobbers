#include "cnr64_hotpatch_api.h"

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <unordered_map>
#include <unordered_set>

namespace {
constexpr std::uint32_t kUnityMainCondWaitReturn = 0x013ce68cu;
std::unordered_map<std::uint32_t, std::uint32_t> pre_signals;
std::unordered_set<std::uint32_t> seen_main_waits;
}

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "ProjectV7 pre-signal diagnostic";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION ||
        !ctx->symbol || !ctx->regs) {
        return CNR64_HOTPATCH_PASS;
    }

    if (std::strcmp(ctx->symbol, "pthread_cond_signal") == 0) {
        const std::uint32_t cond = ctx->regs[0];
        if (seen_main_waits.find(cond) == seen_main_waits.end()) {
            ++pre_signals[cond];
        }
        return CNR64_HOTPATCH_PASS;
    }

    if (std::strcmp(ctx->symbol, "pthread_cond_wait") != 0 ||
        ctx->regs[14] != kUnityMainCondWaitReturn) {
        return CNR64_HOTPATCH_PASS;
    }

    const std::uint32_t cond = ctx->regs[0];
    const bool first_main_wait = seen_main_waits.insert(cond).second;
    auto token = pre_signals.find(cond);
    if (!first_main_wait || token == pre_signals.end() || token->second == 0)
        return CNR64_HOTPATCH_PASS;

    --token->second;
    ctx->regs[0] = 0;
    if (ctx->log_message) {
        char message[256]{};
        std::snprintf(message, sizeof(message),
                      "PV7PRESIGNAL consumed cond=0x%08x mutex=0x%08x lr=0x%08x remaining=%u thread=%u",
                      cond, ctx->regs[1], ctx->regs[14], token->second, ctx->thread_id);
        ctx->log_message(ctx->host_context, message);
    }
    return CNR64_HOTPATCH_HANDLED;
}
