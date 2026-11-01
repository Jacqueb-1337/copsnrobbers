#include "cocos2dx_guest_jni_arrays.h"
#include <algorithm>
#include <cstring>
#include <limits>
namespace pv7::compat::cocos2dx {
bool GuestJniByteArrays::Create(const BitmapPixels& bitmap,
                                std::uint32_t& handle, std::string& error) {
    if (!ValidateBitmapPixels(bitmap,error)) return false;
    if (next_handle_ >= 0x000ffffcu) {
        error = "JNI byte array reference table exhausted"; return false;
    }
    auto candidate = next_handle_;
    auto [it, inserted] = arrays_.emplace(candidate,bitmap.bytes);
    if (!inserted) {
        error = "JNI byte array handle conflict"; return false;
    }
    next_handle_ += 4;
    handle = candidate;
    error.clear();
    return true;
}
bool GuestJniByteArrays::Length(std::uint32_t handle, std::int32_t& length) const {
    auto it=arrays_.find(handle);
    if(it==arrays_.end() || it->second.size()>static_cast<std::size_t>(INT32_MAX))
        return false;
    length=static_cast<std::int32_t>(it->second.size());
    return true;
}
bool GuestJniByteArrays::CopyRegion(std::uint32_t handle,std::int32_t offset,
                                    std::int32_t count,std::uint8_t* destination) const {
    auto it=arrays_.find(handle);
    if(it==arrays_.end() || offset<0 || count<0 ||
       static_cast<std::size_t>(offset)>it->second.size() ||
       static_cast<std::size_t>(count)>it->second.size()-offset ||
       (count>0 && destination==nullptr)) return false;
    if(count) std::memcpy(destination,it->second.data()+offset,static_cast<std::size_t>(count));
    return true;
}
bool GuestJniByteArrays::Release(std::uint32_t handle) {
    return arrays_.erase(handle)==1;
}
const std::vector<std::uint8_t>* GuestJniByteArrays::Find(std::uint32_t handle) const {
    auto it=arrays_.find(handle);
    return it==arrays_.end()?nullptr:&it->second;
}
}
