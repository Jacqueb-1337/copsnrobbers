#include "cnr64_hotpatch_api.h"

#include <cstdint>
#include <cstdio>
#include <cstring>

namespace {

bool read_u32(const Cnr64HotpatchContextV1* ctx, std::uint32_t addr, std::uint32_t& out) {
    if (!ctx || !ctx->guest_memory || addr > ctx->guest_memory_size ||
        ctx->guest_memory_size - addr < sizeof(std::uint32_t)) {
        return false;
    }
    std::memcpy(&out, ctx->guest_memory + addr, sizeof(out));
    return true;
}

void read_cstr(const Cnr64HotpatchContextV1* ctx, std::uint32_t addr,
               char* out, std::size_t out_size) {
    if (!out || out_size == 0) return;
    out[0] = '\0';
    if (!ctx || !ctx->guest_memory || addr >= ctx->guest_memory_size) return;

    std::size_t i = 0;
    while (i + 1 < out_size && addr + i < ctx->guest_memory_size) {
        const char c = static_cast<char>(ctx->guest_memory[addr + i]);
        out[i++] = c;
        if (c == '\0') return;
    }
    out[i < out_size ? i : out_size - 1] = '\0';
}

} // namespace

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "ProjectV7 fatal assertion probe";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION ||
        !ctx->symbol || !ctx->regs || !ctx->guest_memory) {
        return CNR64_HOTPATCH_PASS;
    }

    if (std::strcmp(ctx->symbol, "raise") != 0 || ctx->regs[0] != 11u) {
        return CNR64_HOTPATCH_PASS;
    }

    const std::uint32_t sp = ctx->regs[13];
    std::uint32_t caller_fp = 0u;
    if (!read_u32(ctx, sp, caller_fp) || caller_fp < 36u) {
        return CNR64_HOTPATCH_PASS;
    }

    std::uint32_t log_level = 0u;
    std::uint32_t log_format = 0u;
    std::uint32_t log_args = 0u;
    read_u32(ctx, caller_fp - 28u, log_level);
    read_u32(ctx, caller_fp - 32u, log_format);
    read_u32(ctx, caller_fp - 36u, log_args);

    std::uint32_t arg0 = 0u;
    std::uint32_t arg1 = 0u;
    if (log_args) {
        read_u32(ctx, log_args, arg0);
        read_u32(ctx, log_args + 4u, arg1);
    }

    char format[192]{};
    char arg0_text[256]{};
    read_cstr(ctx, log_format, format, sizeof(format));
    read_cstr(ctx, arg0, arg0_text, sizeof(arg0_text));

    char message[640]{};
    std::snprintf(message, sizeof(message),
                  "PV7HOTFATAL thread=%u fp=0x%08x level=0x%08x format=0x%08x args=0x%08x arg0=0x%08x arg1=%u format='%s' arg0str='%s'",
                  ctx->thread_id, caller_fp, log_level, log_format, log_args,
                  arg0, arg1, format, arg0_text);
    if (ctx->log_message) ctx->log_message(ctx->host_context, message);

    return CNR64_HOTPATCH_PASS;
}
