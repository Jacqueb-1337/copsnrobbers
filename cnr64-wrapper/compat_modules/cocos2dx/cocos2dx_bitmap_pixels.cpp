#include "cocos2dx_bitmap_pixels.h"
#include <cstdint>
#include <limits>

namespace pv7::compat::cocos2dx {
namespace {
constexpr std::uint64_t kMaxPixelBytes = 64ull * 1024 * 1024;
}
bool ValidateBitmapPixels(const BitmapPixels& bitmap, std::string& error) {
    if (bitmap.width <= 0 || bitmap.height <= 0) {
        error = "bitmap dimensions must be positive"; return false;
    }
    const auto required = static_cast<std::uint64_t>(bitmap.width) *
                          static_cast<std::uint64_t>(bitmap.height) * 4u;
    if (required > kMaxPixelBytes) {
        error = "bitmap exceeds pixel budget"; return false;
    }
    if (bitmap.bytes.size() != required) {
        error = "bitmap byte count mismatch"; return false;
    }
    error.clear();
    return true;
}
bool ReadBitmapPixels(std::int32_t width, std::int32_t height,
                      std::span<const std::uint8_t> pixels,
                      BitmapPixels& result, std::string& error) {
    if (width <= 0 || height <= 0) {
        error = "bitmap dimensions must be positive"; return false;
    }
    const auto required = static_cast<std::uint64_t>(width) *
                          static_cast<std::uint64_t>(height) * 4u;
    if (required > kMaxPixelBytes || required != pixels.size()) {
        error = "invalid bitmap payload size"; return false;
    }
    BitmapPixels candidate;
    candidate.width = width;
    candidate.height = height;
    candidate.bytes.assign(pixels.begin(), pixels.end());
    if (!ValidateBitmapPixels(candidate,error)) return false;
    result = std::move(candidate);
    return true;
}
}
