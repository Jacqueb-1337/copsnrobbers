#include "cnr64_hotpatch_api.h"

#include <cstdio>
#include <cstring>

extern "C" void* eglGetProcAddress(const char* procname);
using GLuint = unsigned int;

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "gl-create-program-proc";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION ||
        !ctx->symbol || !ctx->regs || !ctx->log_message ||
        std::strcmp(ctx->symbol, "glCreateProgram") != 0) {
        return CNR64_HOTPATCH_PASS;
    }

    using Proc = GLuint (*)();
    static Proc proc = reinterpret_cast<Proc>(eglGetProcAddress("glCreateProgram"));
    if (!proc) {
        ctx->log_message(ctx->host_context, "glCreateProgram eglGetProcAddress returned null");
        ctx->regs[0] = 0;
        return CNR64_HOTPATCH_HANDLED;
    }

    ctx->log_message(ctx->host_context, "glCreateProgram direct-proc enter");
    const GLuint program = proc();
    char line[160];
    std::snprintf(line, sizeof(line), "glCreateProgram direct-proc returned %u", program);
    ctx->log_message(ctx->host_context, line);
    ctx->regs[0] = static_cast<std::uint32_t>(program);
    return CNR64_HOTPATCH_HANDLED;
}
