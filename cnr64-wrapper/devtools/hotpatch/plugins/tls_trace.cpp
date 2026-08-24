#include "cnr64_hotpatch_api.h"

#include <cstdint>
#include <cstdio>
#include <cstring>

namespace {
struct TlsSlot {
    std::uint32_t thread_id = 0;
    std::uint32_t key = 0;
    std::uint32_t value = 0;
    std::uint32_t gets = 0;
    std::uint32_t sets = 0;
};

TlsSlot g_slots[256]{};
std::uint32_t g_key3_entries_thread36 = 0;

TlsSlot* FindSlot(std::uint32_t thread_id, std::uint32_t key) {
    TlsSlot* empty = nullptr;
    for (auto& slot : g_slots) {
        if (slot.thread_id == thread_id && slot.key == key) return &slot;
        if (!empty && slot.thread_id == 0) empty = &slot;
    }
    if (!empty) return nullptr;
    empty->thread_id = thread_id;
    empty->key = key;
    return empty;
}

void CopyGuestAscii(Cnr64HotpatchContextV1* ctx, std::uint32_t ptr, char* out, std::size_t out_size) {
    if (!out || out_size == 0) return;
    out[0] = '\0';
    if (!ctx || !ctx->guest_memory || ptr >= ctx->guest_memory_size) return;
    const std::size_t max_read = out_size - 1;
    std::size_t i = 0;
    for (; i < max_read && ptr + i < ctx->guest_memory_size; ++i) {
        const unsigned char c = ctx->guest_memory[ptr + i];
        if (c == 0) break;
        out[i] = (c >= 32 && c < 127) ? static_cast<char>(c) : '.';
    }
    out[i] = '\0';
}

void Log(Cnr64HotpatchContextV1* ctx, const char* op, const TlsSlot& slot) {
    if (!ctx || !ctx->log_message) return;
    char line[224]{};
    std::snprintf(line, sizeof(line),
                  "PV7TLSTRACE %s thread=%u key=%u value=0x%08x gets=%u sets=%u lr=0x%08x pc=0x%08x",
                  op, ctx->thread_id, slot.key, slot.value, slot.gets, slot.sets,
                  ctx->regs ? ctx->regs[14] : 0u, ctx->regs ? ctx->regs[15] : 0u);
    ctx->log_message(ctx->host_context, line);
}

void LogKey3Payload(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || !ctx->regs || !ctx->log_message) return;
    const std::uint32_t index = ++g_key3_entries_thread36;
    if (index > 12u && (index % 1024u) != 0u) return;

    char r10_text[96]{};
    char r8_text[96]{};
    CopyGuestAscii(ctx, ctx->regs[10], r10_text, sizeof(r10_text));
    CopyGuestAscii(ctx, ctx->regs[8], r8_text, sizeof(r8_text));

    char line[512]{};
    std::snprintf(line, sizeof(line),
                  "PV7KEY3 entry=%u thread=%u r10=0x%08x text10='%s' r8=0x%08x text8='%s' r9=0x%08x r11=0x%08x sp=0x%08x lr=0x%08x",
                  index, ctx->thread_id, ctx->regs[10], r10_text, ctx->regs[8], r8_text,
                  ctx->regs[9], ctx->regs[11], ctx->regs[13], ctx->regs[14]);
    ctx->log_message(ctx->host_context, line);
}
} // namespace

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "ProjectV7 pthread TLS trace";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* ctx) {
    if (!ctx || ctx->abi_version != CNR64_HOTPATCH_ABI_VERSION || !ctx->symbol || !ctx->regs)
        return CNR64_HOTPATCH_PASS;

    if (std::strcmp(ctx->symbol, "pthread_setspecific") == 0) {
        TlsSlot* slot = FindSlot(ctx->thread_id, ctx->regs[0]);
        if (slot) {
            const std::uint32_t old_value = slot->value;
            slot->value = ctx->regs[1];
            ++slot->sets;
            if (ctx->thread_id == 36u && slot->key == 3u && slot->value == 1u)
                LogKey3Payload(ctx);
            if (slot->sets <= 4u || (slot->sets % 1024u) == 0u)
                Log(ctx, "set", *slot);
            (void)old_value;
        }
    } else if (std::strcmp(ctx->symbol, "pthread_getspecific") == 0) {
        TlsSlot* slot = FindSlot(ctx->thread_id, ctx->regs[0]);
        if (slot) {
            ++slot->gets;
            if (slot->gets <= 2u || (slot->gets % 4096u) == 0u)
                Log(ctx, "get", *slot);
        }
    } else if (std::strcmp(ctx->symbol, "pthread_key_delete") == 0) {
        TlsSlot* slot = FindSlot(ctx->thread_id, ctx->regs[0]);
        if (slot) Log(ctx, "delete", *slot);
    }

    return CNR64_HOTPATCH_PASS;
}
