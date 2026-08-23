#include "cnr64_hotpatch_api.h"
#include <cstdint>
namespace {
constexpr std::uint32_t kUnityMonoJitInitGot=0x01a8924cu;
constexpr std::uint32_t kExistingRootDomain=0x05038ee0u;
}
extern "C" const char* cnr64_hotpatch_name_v1(){return "ProjectV7 root reuse simple";}
extern "C" int cnr64_hotpatch_dispatch_v1(Cnr64HotpatchContextV1* c){
    if(!c||c->abi_version!=CNR64_HOTPATCH_ABI_VERSION||!c->guest_memory||!c->allocate_guest) return CNR64_HOTPATCH_PASS;
    static bool done=false; if(done) return CNR64_HOTPATCH_PASS;
    const std::uint32_t s=c->allocate_guest(c->host_context,12u,4u);
    if(!s||s+12u>c->guest_memory_size||kUnityMonoJitInitGot+4u>c->guest_memory_size) return CNR64_HOTPATCH_PASS;
    auto w=[&](std::uint32_t a,std::uint32_t v){c->guest_memory[a]=(std::uint8_t)v;c->guest_memory[a+1]=(std::uint8_t)(v>>8);c->guest_memory[a+2]=(std::uint8_t)(v>>16);c->guest_memory[a+3]=(std::uint8_t)(v>>24);};
    w(s,0xe59f0000u); w(s+4u,0xe12fff1eu); w(s+8u,kExistingRootDomain); w(kUnityMonoJitInitGot,s);
    done=true; if(c->log_message)c->log_message(c->host_context,"root-domain reuse GOT patch applied");
    return CNR64_HOTPATCH_PASS;
}
