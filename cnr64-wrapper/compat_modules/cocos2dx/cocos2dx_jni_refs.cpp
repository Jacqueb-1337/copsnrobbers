#include "cocos2dx_jni_refs.h"
namespace pv7::compat::cocos2dx {
bool GuestJniRefs::RegisterLocal(std::uint32_t handle,JniRefKind kind) {
    if (!handle) return false;
    auto [it,inserted]=references_.emplace(handle,Entry{handle,kind,JniRefScope::Local});
    return inserted || (it->second.canonical==handle &&
                        it->second.kind==kind &&
                        it->second.scope==JniRefScope::Local);
}
bool GuestJniRefs::Duplicate(std::uint32_t source,JniRefScope scope,
                             std::uint32_t& result,std::string& error) {
    if (!source) {result=0; error.clear();return true;}
    const auto it=references_.find(source);
    if (it==references_.end()) {error="invalid JNI reference";return false;}
    if(next_alias_>=0x007ffffcu) {error="JNI reference space exhausted";return false;}
    const auto alias=next_alias_;
    auto [added,inserted]=references_.emplace(alias,
        Entry{it->second.canonical,it->second.kind,scope});
    if(!inserted) {error="JNI reference collision";return false;}
    next_alias_+=4;
    result=alias;
    error.clear();
    return true;
}
bool GuestJniRefs::Delete(std::uint32_t handle,JniRefScope scope) {
    if (!handle) return true;
    auto it=references_.find(handle);
    if(it==references_.end()||it->second.scope!=scope) return false;
    references_.erase(it);
    return true;
}
std::optional<std::uint32_t> GuestJniRefs::Resolve(std::uint32_t handle) const {
    if(!handle) return std::uint32_t{0};
    const auto it=references_.find(handle);
    if(it==references_.end()) return std::nullopt;
    return it->second.canonical;
}
std::optional<JniRefKind> GuestJniRefs::Kind(std::uint32_t handle) const {
    const auto it=references_.find(handle);
    if(it==references_.end()) return std::nullopt;
    return it->second.kind;
}
bool GuestJniRefs::IsSame(std::uint32_t a,std::uint32_t b) const {
    const auto one=Resolve(a),two=Resolve(b);
    return one && two && *one==*two;
}
int GuestJniRefs::RefType(std::uint32_t handle) const {
    const auto it=references_.find(handle);
    if(it==references_.end()) return 0;
    return it->second.scope==JniRefScope::Local ? 1 : 2;
}
}