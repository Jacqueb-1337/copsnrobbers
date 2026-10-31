#pragma once

// Bounded, diagnostic-only formatting of ARM32 guest arguments. Never pass a
// guest va_list, guest pointer, or unvalidated format to host varargs.
#include <algorithm>
#include <array>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <functional>
#include <optional>
#include <string>
#include <string_view>
#include <vector>

namespace pv7::cocos_probe {
using GuestWord = std::function<std::optional<std::uint32_t>()>;
inline bool ReadGuestText(const std::vector<std::uint8_t>& memory,
                          std::uint32_t address, std::string& result,
                          std::string& error) {
    if (!address || address >= memory.size()) {
        error = "guest string pointer invalid"; return false;
    }
    const auto length = std::min<std::size_t>(4096, memory.size() - address);
    const auto* begin = memory.data() + address;
    const auto* end = static_cast<const std::uint8_t*>(
        std::memchr(begin, 0, length));
    if (!end) { error = "guest string unterminated"; return false; }
    result.assign(reinterpret_cast<const char*>(begin),
                  static_cast<std::size_t>(end - begin));
    return true;
}

inline bool FormatGuest32(const std::vector<std::uint8_t>& memory,
                          std::uint32_t formatAddress, const GuestWord& next,
                          std::string& output, std::string& error) {
    std::string format;
    if (!ReadGuestText(memory, formatAddress, format, error)) return false;
    output.clear();
    for (std::size_t i = 0; i < format.size();) {
        if (format[i] != '%') {
            output.push_back(format[i++]);
        } else {
            ++i;
            if (i < format.size() && format[i] == '%') {
                output.push_back('%'); ++i; continue;
            }
            std::string flags;
            while (i < format.size() &&
                   std::string_view("-+ #0").find(format[i]) != std::string_view::npos)
                flags.push_back(format[i++]);
            auto parseCount = [&](std::string& count) -> bool {
                if (i < format.size() && format[i] == '*') {
                    ++i;
                    auto word = next();
                    if (!word) { error = "missing dynamic format width"; return false; }
                    auto value = static_cast<std::int32_t>(*word);
                    if (value < -2048 || value > 2048) {
                        error = "dynamic format width exceeds cap"; return false;
                    }
                    if (value < 0) {
                        if (count.empty()) flags.push_back('-');
                        value = -value;
                    }
                    count = std::to_string(value);
                } else {
                    while (i < format.size() && format[i] >= '0' && format[i] <= '9') {
                        count.push_back(format[i++]);
                        if (count.size() > 4) {
                            error = "format width precision too long"; return false;
                        }
                    }
                    if (!count.empty() && std::stoul(count) > 2048) {
                        error = "format width precision exceeds cap"; return false;
                    }
                }
                return true;
            };
            std::string width;
            if (!parseCount(width)) return false;
            std::string precision;
            if (i < format.size() && format[i] == '.') {
                ++i;
                std::string p;
                if (!parseCount(p)) return false;
                precision = "." + (p.empty() ? "0" : p);
            }
            if (i < format.size() && format[i] == 'h') {
                ++i;
                if (i < format.size() && format[i] == 'h') ++i;
            } else if (i < format.size() &&
                       (format[i] == 'l' || format[i] == 'z' || format[i] == 't')) {
                const char length = format[i++];
                if (length == 'l' && i < format.size() && format[i] == 'l') {
                    error = "64-bit vararg alignment not supported"; return false;
                }
            } else if (i < format.size() && (format[i] == 'j' || format[i] == 'L')) {
                error = "64-bit or extended vararg not supported"; return false;
            }
            if (i == format.size()) { error = "incomplete format conversion"; return false; }
            const char conversion = format[i++];
            if (std::string_view("diuxXocs").find(conversion) == std::string_view::npos) {
                error = std::string("unsupported format conversion: ") + conversion;
                return false; // explicitly reject %n, %p, floats, extensions
            }
            const auto argument = next();
            if (!argument) { error = "guest format arguments exhausted"; return false; }
            const std::string spec = "%" + flags + width + precision + conversion;
            std::array<char, 8192> piece{};
            int written = -1;
            switch (conversion) {
                case 'd': case 'i':
                    written = std::snprintf(piece.data(), piece.size(), spec.c_str(),
                                            static_cast<std::int32_t>(*argument)); break;
                case 'u': case 'x': case 'X': case 'o':
                    written = std::snprintf(piece.data(), piece.size(), spec.c_str(),
                                            static_cast<std::uint32_t>(*argument)); break;
                case 'c':
                    written = std::snprintf(piece.data(), piece.size(), spec.c_str(),
                                            static_cast<int>(*argument)); break;
                case 's': {
                    std::string value;
                    if (!ReadGuestText(memory, *argument, value, error)) return false;
                    written = std::snprintf(piece.data(), piece.size(), spec.c_str(),
                                            value.c_str());
                    break;
                }
                default: break;
            }
            if (written < 0 || static_cast<std::size_t>(written) >= piece.size()) {
                error = "formatted conversion too large"; return false;
            }
            output.append(piece.data(), static_cast<std::size_t>(written));
        }
        if (output.size() > 4096) { error = "formatted output exceeds 4096 bytes"; return false; }
    }
    return true;
}
} // namespace pv7::cocos_probe