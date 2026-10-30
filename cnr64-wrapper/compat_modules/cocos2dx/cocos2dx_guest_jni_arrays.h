#pragma once
#include "cocos2dx_bitmap_pixels.h"
#include <cstdint>
#include <map>
#include <string>
#include <vector>
namespace pv7::compat::cocos2dx {
class GuestJniByteArrays {
public:
    bool Create(const BitmapPixels& bitmap, std::uint32_t& handle, std::string& error);
    bool Length(std::uint32_t handle, std::int32_t& length) const;
    bool CopyRegion(std::uint32_t handle, std::int32_t offset,
                    std::int32_t count, std::uint8_t* destination) const;
    bool Release(std::uint32_t handle);
    const std::vector<std::uint8_t>* Find(std::uint32_t handle) const;
private:
    std::uint32_t next_handle_ = 0x000c0000;
    std::map<std::uint32_t, std::vector<std::uint8_t>> arrays_;
};
}
