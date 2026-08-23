#include "cnr64_hotpatch_api.h"

#include <cstring>

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "CNR64 smoke override";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION || !ctx->symbol || !ctx->regs)
        return CNR64_HOTPATCH_PASS;

    // Harmless proof that a newly deployed ARM64 extension can replace a guest
    // import without rebuilding or reinstalling the wrapper APK.
    if (std::strcmp(ctx->symbol, "__aeabi_atexit") == 0) {
        static bool logged = false;
        if (!logged && ctx->log_message) {
            ctx->log_message(ctx->host_context, "smoke extension intercepted __aeabi_atexit");
            logged = true;
        }
        ctx->regs[0] = 0;
        return CNR64_HOTPATCH_HANDLED;
    }

    return CNR64_HOTPATCH_PASS;
}
