#pragma once
#include <cstdint>
#include <map>
#include <optional>
#include <string>
namespace pv7::compat::cocos2dx {
enum class JniRefKind { Class, String, ByteArray, IntArray, FloatArray };
enum class JniRefScope { Local, Global };
class GuestJniRefs {
public:
    bool RegisterLocal(std::uint32_t handle, JniRefKind kind);
    bool Duplicate(std::uint32_t source, JniRefScope scope,
                   std::uint32_t& result, std::string& error);
    bool Delete(std::uint32_t handle, JniRefScope scope);
    std::optional<std::uint32_t> Resolve(std::uint32_t handle) const;
    std::optional<JniRefKind> Kind(std::uint32_t handle) const;
    bool IsSame(std::uint32_t a, std::uint32_t b) const;
    // JNI_INVALID_REF_TYPE=0, JNI_LOCAL_REF_TYPE=1, JNI_GLOBAL_REF_TYPE=2.
    int RefType(std::uint32_t handle) const;
private:
    struct Entry {
        std::uint32_t canonical;
        JniRefKind kind;
        JniRefScope scope;
    };
    std::uint32_t next_alias_ = 0x00400000u;
    std::map<std::uint32_t,Entry> references_;
};
}