#include "cnr64_hotpatch_api.h"

#include <cstdio>
#include <cstring>

namespace {
bool StartsWith(const char* value, const char* prefix) {
    if (!value || !prefix) return false;
    return std::strncmp(value, prefix, std::strlen(prefix)) == 0;
}

bool IsInteresting(const char* name) {
    if (!name) return false;
    if (StartsWith(name, "egl") || StartsWith(name, "gl") ||
        StartsWith(name, "ANativeWindow") || StartsWith(name, "AAsset")) {
        return true;
    }
    static const char* const exact[] = {
        "open", "open64", "openat", "read", "pread", "pread64", "fread",
        "mmap", "mmap64", "munmap", "mprotect", "msync", "ioctl",
        "poll", "ppoll", "select", "pselect", "epoll_wait",
        "sem_wait", "sem_timedwait", "pthread_join", "pthread_cond_wait",
        "pthread_cond_timedwait", "nanosleep", "usleep", "sleep"
    };
    for (const char* candidate : exact) {
        if (std::strcmp(name, candidate) == 0) return true;
    }
    return false;
}
} // namespace

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "blocking-call-trace";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION ||
        !ctx->regs || !ctx->log_message || !IsInteresting(ctx->symbol)) {
        return CNR64_HOTPATCH_PASS;
    }

    static unsigned long long sequence = 0;
    char line[320];
    std::snprintf(line, sizeof(line),
                  "BRIDGE_ENTER #%llu thread=%u symbol=%s pc=0x%08x lr=0x%08x r0=0x%08x r1=0x%08x r2=0x%08x r3=0x%08x",
                  ++sequence, ctx->thread_id, ctx->symbol ? ctx->symbol : "?",
                  ctx->regs[15], ctx->regs[14], ctx->regs[0], ctx->regs[1],
                  ctx->regs[2], ctx->regs[3]);
    ctx->log_message(ctx->host_context, line);
    return CNR64_HOTPATCH_PASS;
}
