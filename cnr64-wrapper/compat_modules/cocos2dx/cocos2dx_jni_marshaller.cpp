#include "cocos2dx_jni_marshaller.h"
#include <array>

namespace pv7::compat::cocos2dx {
namespace {
bool Read32(std::span<const std::uint8_t> bytes, std::uint32_t address,
            std::uint32_t& result) {
    if (address > bytes.size() || bytes.size()-address < 4) return false;
    result = static_cast<std::uint32_t>(bytes[address]) |
             (static_cast<std::uint32_t>(bytes[address+1]) << 8) |
             (static_cast<std::uint32_t>(bytes[address+2]) << 16) |
             (static_cast<std::uint32_t>(bytes[address+3]) << 24);
    return true;
}
}
bool BitmapJniMarshaller::Decode(
    std::span<const std::uint8_t> guest_memory,
    std::uint32_t arm_va_list,
    const std::unordered_map<std::uint32_t,std::string>& strings,
    BitmapTextRequest& result, std::string& error) const {
    if (!arm_va_list || (arm_va_list & 3u)) {
        error="invalid or unaligned ARM32 va_list"; return false;
    }
    std::array<std::uint32_t,6> arguments{};
    for (std::size_t i=0;i<arguments.size();++i) {
        if (arm_va_list > UINT32_MAX - static_cast<std::uint32_t>(i*4) ||
            !Read32(guest_memory,arm_va_list+static_cast<std::uint32_t>(i*4),
                    arguments[i])) {
            error="ARM32 va_list outside guest memory"; return false;
        }
    }
    const auto text=strings.find(arguments[0]);
    const auto font=strings.find(arguments[1]);
    if (text==strings.end() || font==strings.end()) {
        error="unresolved JNI string handle"; return false;
    }
    BitmapTextRequest request;
    request.text=text->second;
    request.font=font->second;
    request.font_size=static_cast<std::int32_t>(arguments[2]);
    request.alignment=static_cast<std::int32_t>(arguments[3]);
    request.width=static_cast<std::int32_t>(arguments[4]);
    request.height=static_cast<std::int32_t>(arguments[5]);
    if (request.font_size <= 0 || request.font_size > 4096 ||
        request.width < 0 || request.height < 0 ||
        request.width > 16384 || request.height > 16384) {
        error="invalid text bitmap dimensions"; return false;
    }
    result=std::move(request);
    error.clear();
    return true;
}
}
