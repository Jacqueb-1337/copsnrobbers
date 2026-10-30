#pragma once
#include <cstdint>
#include <span>
#include <string>
#include <vector>

namespace pv7::compat::cocos2dx {
// Native-order 4-byte-per-pixel bitmap delivered by the original
// Cocos2dxBitmap.java nativeInitBitmapDC(width,height,byte[]).
struct BitmapPixels {
    std::int32_t width = 0;
    std::int32_t height = 0;
    std::vector<std::uint8_t> bytes;
};

// Host-to-guest handoff contract. A valid packet may be forwarded to the
// real ARM32 JNI callback; never execute the callback with fake pixels.
bool ValidateBitmapPixels(const BitmapPixels& bitmap, std::string& error);
bool ReadBitmapPixels(std::int32_t width, std::int32_t height,
                      std::span<const std::uint8_t> pixels,
                      BitmapPixels& result, std::string& error);
}
