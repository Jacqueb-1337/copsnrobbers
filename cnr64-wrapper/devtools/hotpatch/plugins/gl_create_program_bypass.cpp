#include "cnr64_hotpatch_api.h"

#include <cstdio>
#include <cstring>

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "gl-create-program-bypass";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION ||
        !ctx->symbol || !ctx->regs || !ctx->log_message ||
        std::strcmp(ctx->symbol, "glCreateProgram") != 0) {
        return CNR64_HOTPATCH_PASS;
    }

    static std::uint32_t next_fake_program = 0x60000000u;
    const std::uint32_t fake = next_fake_program++;
    ctx->regs[0] = fake;

    char line[192];
    std::snprintf(line, sizeof(line),
                  "DIAG bypass glCreateProgram thread=%u fake=0x%08x lr=0x%08x",
                  ctx->thread_id, fake, ctx->regs[14]);
    ctx->log_message(ctx->host_context, line);
    return CNR64_HOTPATCH_HANDLED;
}
