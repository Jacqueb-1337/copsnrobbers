#include "cnr64_hotpatch_api.h"

#include <cstring>

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "CNR64 live reload probe";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION || !ctx->symbol)
        return CNR64_HOTPATCH_PASS;

    if (std::strcmp(ctx->symbol, "pthread_getspecific") == 0) {
        static bool logged = false;
        if (!logged && ctx->log_message) {
            ctx->log_message(ctx->host_context,
                             "same-process hot reload confirmed on pthread_getspecific");
            logged = true;
        }
    }
    return CNR64_HOTPATCH_PASS;
}
