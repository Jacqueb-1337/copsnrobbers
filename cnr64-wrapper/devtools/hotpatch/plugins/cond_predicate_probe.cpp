#include "cnr64_hotpatch_api.h"

#include <cstdint>
#include <cstdio>
#include <cstring>

namespace {
constexpr std::uint32_t kTargetCond = 0x06410508u;
constexpr std::uint32_t kRenderWaitLr = 0x013ce68cu;

bool read_u32(Cnr64HotpatchContextV1* c, std::uint32_t a, std::uint32_t& v) {
    if (!c || !c->guest_memory || a > c->guest_memory_size - 4u) return false;
    std::memcpy(&v, c->guest_memory + a, sizeof(v));
    return true;
}

void logf(Cnr64HotpatchContextV1* c, const char* tag) {
    if (!c || !c->regs || !c->log_message) return;
    const auto* r = c->regs;
    const std::uint32_t sp = r[13];
    const std::uint32_t r4 = r[4];
    const std::uint32_t r5 = r[5];
    std::uint32_t sp8 = 0, r5p24 = 0, base0 = 0, base20 = 0, base24 = 0;
    read_u32(c, sp + 8u, sp8);
    read_u32(c, r5 + 24u, r5p24);
    read_u32(c, kTargetCond - 8u, base0);
    read_u32(c, kTargetCond + 12u, base20);
    read_u32(c, kTargetCond + 16u, base24);
    char msg[320]{};
    std::snprintf(msg, sizeof(msg),
                  "PV7PRED %s tid=%u lr=0x%08x sp=0x%08x r4=0x%08x r5=0x%08x sp8=0x%08x r5p24=0x%08x condBase0=0x%08x condBase20=0x%08x condBase24=0x%08x",
                  tag, c->thread_id, r[14], sp, r4, r5, sp8, r5p24, base0, base20, base24);
    c->log_message(c->host_context, msg);
}
}

extern "C" const char* cnr64_hotpatch_name_v1() {
    return "ProjectV7 render cond predicate probe";
}

extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* c) {
    if (!c || c->abi_version != CNR64_HOTPATCH_ABI_VERSION || !c->symbol || !c->regs)
        return CNR64_HOTPATCH_PASS;

    if (std::strcmp(c->symbol, "pthread_cond_signal") == 0 && c->regs[0] == kTargetCond) {
        logf(c, "signal-target");
    } else if (std::strcmp(c->symbol, "pthread_cond_wait") == 0 &&
               c->regs[0] == kTargetCond && c->regs[14] == kRenderWaitLr) {
        logf(c, "wait-target");
    }
    return CNR64_HOTPATCH_PASS;
}
