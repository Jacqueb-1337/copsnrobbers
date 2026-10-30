#pragma once
#include <cstdint>
#include <span>
#include <string>
#include <string_view>
#include <unordered_map>
#include <vector>

namespace pv7::compat::cocos2dx {
struct BitmapTextRequest {
    std::string text;
    std::string font;
    std::int32_t font_size=0;
    std::int32_t alignment=0;
    std::int32_t width=0;
    std::int32_t height=0;
};
// Decode the original ARM32 AAPCS va_list backing memory. The guest owns all
// pointers. No guest pointer is dereferenced as a host pointer.
class BitmapJniMarshaller {
public:
    bool Decode(std::span<const std::uint8_t> guest_memory,
                std::uint32_t arm_va_list,
                const std::unordered_map<std::uint32_t,std::string>& strings,
                BitmapTextRequest& result,
                std::string& error) const;
};
}
