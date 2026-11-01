#include "pv7_compat_module_registry.h"

#include <algorithm>
#include <cctype>
#include <cstdint>
#include <map>
#include <sstream>
#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
#include "../compat_modules/cnr/cnr_compat_module.h"
#endif
#if defined(PV7_COMPAT_MODULE_COCOS2DX) && PV7_COMPAT_MODULE_COCOS2DX
#include "../compat_modules/cocos2dx/cocos2dx_compat_module.h"
#endif

namespace pv7::compat {
namespace {

struct RuntimePathAlias {
    std::string from;
    std::string to_files_relative;
};

struct RuntimePatch {
    std::string name;
    std::uint32_t address = 0;
    std::uint32_t expected = 0;
    std::uint32_t replacement = 0;
    bool complete = false;
};

struct RuntimeConfig {
    std::string module_id;
    std::string version;
    std::string guest_package;
    bool rewrite_private_paths = false;
    std::vector<RuntimePathAlias> aliases;
    std::unordered_map<std::string, std::string> properties;
    std::vector<RuntimePatch> patches;
    std::unordered_map<std::string, std::uint32_t> addresses;
};

RuntimeConfig g_runtime;

std::string Trim(std::string value) {
    const auto not_space = [](unsigned char c) { return !std::isspace(c); };
    value.erase(value.begin(), std::find_if(value.begin(), value.end(), not_space));
    value.erase(std::find_if(value.rbegin(), value.rend(), not_space).base(), value.end());
    return value;
}

bool ParseBool(const std::string& value) {
    std::string lower = value;
    std::transform(lower.begin(), lower.end(), lower.begin(),
                   [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
    return lower == "1" || lower == "true" || lower == "yes" || lower == "on";
}

bool ParseU32(const std::string& text, std::uint32_t& value) {
    try {
        std::size_t consumed = 0;
        const unsigned long parsed = std::stoul(text, &consumed, 0);
        if (consumed != text.size() || parsed > 0xfffffffful) return false;
        value = static_cast<std::uint32_t>(parsed);
        return true;
    } catch (...) {
        return false;
    }
}

bool PrefixRewrite(const std::string& path,
                   const std::string& from,
                   const std::string& to,
                   std::string& result) {
    if (from.empty()) return false;
    if (path == from) {
        result = to;
        return true;
    }
    if (path.size() > from.size() &&
        path.compare(0, from.size(), from) == 0 &&
        path[from.size()] == '/') {
        result = to + path.substr(from.size());
        return true;
    }
    return false;
}

bool ParseIndexedKey(const std::string& key,
                     const std::string& prefix,
                     int& index,
                     std::string& field) {
    if (key.rfind(prefix, 0) != 0) return false;
    const std::size_t number_start = prefix.size();
    const std::size_t dot = key.find('.', number_start);
    if (dot == std::string::npos) return false;
    try {
        index = std::stoi(key.substr(number_start, dot - number_start));
    } catch (...) {
        return false;
    }
    field = key.substr(dot + 1);
    return !field.empty();
}

} // namespace

void ConfigureRuntimeModule(const std::string& config_text) {
    RuntimeConfig parsed;
    std::map<int, RuntimePathAlias> aliases;
    std::map<int, RuntimePatch> patches;

    std::istringstream stream(config_text);
    std::string line;
    while (std::getline(stream, line)) {
        line = Trim(line);
        if (line.empty() || line[0] == '#' || line[0] == ';') continue;

        const std::size_t equals = line.find('=');
        if (equals == std::string::npos) continue;
        const std::string key = Trim(line.substr(0, equals));
        const std::string value = Trim(line.substr(equals + 1));
        if (key.empty()) continue;

        if (key == "module.id") {
            parsed.module_id = value;
        } else if (key == "module.version") {
            parsed.version = value;
        } else if (key == "guest.package") {
            parsed.guest_package = value;
        } else if (key == "path.rewritePrivate") {
            parsed.rewrite_private_paths = ParseBool(value);
        } else if (key.rfind("property.", 0) == 0) {
            parsed.properties[key.substr(9)] = value;
        } else if (key.rfind("address.", 0) == 0) {
            std::uint32_t address = 0;
            if (ParseU32(value, address)) {
                parsed.addresses[key.substr(8)] = address;
            }
        } else {
            int index = -1;
            std::string field;
            if (ParseIndexedKey(key, "path.alias.", index, field)) {
                auto& alias = aliases[index];
                if (field == "from") alias.from = value;
                else if (field == "toFilesRelative") alias.to_files_relative = value;
            } else if (ParseIndexedKey(key, "patch.", index, field)) {
                auto& patch = patches[index];
                if (field == "name") {
                    patch.name = value;
                } else if (field == "address") {
                    ParseU32(value, patch.address);
                } else if (field == "expected") {
                    ParseU32(value, patch.expected);
                } else if (field == "replacement") {
                    ParseU32(value, patch.replacement);
                }
            }
        }
    }

    for (auto& entry : aliases) {
        if (!entry.second.from.empty()) parsed.aliases.push_back(std::move(entry.second));
    }
    for (auto& entry : patches) {
        RuntimePatch patch = std::move(entry.second);
        patch.complete = !patch.name.empty() && patch.address != 0u;
        if (patch.complete) parsed.patches.push_back(std::move(patch));
    }

    g_runtime = std::move(parsed);
}

const char* GuestPackageName() {
    if (!g_runtime.guest_package.empty()) return g_runtime.guest_package.c_str();
#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
    if (const char* value = cnr::GuestPackageName(); value && *value) return value;
#endif
    return nullptr;
}

const char* GuestVersionName() {
    const auto version = g_runtime.properties.find("pv7.guestVersionName");
    if (version != g_runtime.properties.end()) return version->second.c_str();
#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
    return cnr::GuestVersionName();
#else
    return nullptr;
#endif
}
bool SupportsHostRestart() {
    const auto flag = g_runtime.properties.find("pv7.hostRestart");
    if (flag != g_runtime.properties.end()) return ParseBool(flag->second);
#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
    return cnr::SupportsHostRestart();
#else
    return false;
#endif
}

bool NormalizeGuestPath(const std::string& path,
                        const std::string& managed_dir,
                        std::string& result) {
    const std::size_t files_marker = managed_dir.find("/files/");
    if (files_marker != std::string::npos) {
        const std::string files_dir = managed_dir.substr(0, files_marker + 6u);
        const std::string app_private_dir = managed_dir.substr(0, files_marker);

        if (g_runtime.rewrite_private_paths && !g_runtime.guest_package.empty()) {
            const std::string data_data = "/data/data/" + g_runtime.guest_package;
            const std::string user_zero = "/data/user/0/" + g_runtime.guest_package;
            if (PrefixRewrite(path, data_data, app_private_dir, result) ||
                PrefixRewrite(path, user_zero, app_private_dir, result)) {
                return true;
            }
        }

        for (const auto& alias : g_runtime.aliases) {
            std::string target = files_dir;
            if (!alias.to_files_relative.empty()) {
                target += "/";
                target += alias.to_files_relative;
            }
            if (PrefixRewrite(path, alias.from, target, result)) return true;
        }
    }

#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
    if (cnr::NormalizeGuestPath(path, managed_dir, result)) return true;
#endif
    return false;
}

bool GetSystemProperty(const std::string& name, std::string& value) {
    const auto runtime = g_runtime.properties.find(name);
    if (runtime != g_runtime.properties.end()) {
        value = runtime->second;
        return true;
    }
#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
    if (cnr::GetSystemProperty(name, value)) return true;
#endif
    return false;
}

std::vector<Pv7GuestInstructionPatchV1> StartupInstructionPatches() {
    std::vector<Pv7GuestInstructionPatchV1> result;
    result.reserve(g_runtime.patches.size() + 2u);
    for (const auto& patch : g_runtime.patches) {
        result.push_back({
            patch.name.c_str(),
            patch.address,
            patch.expected,
            patch.replacement,
        });
    }
#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
    auto cnr_patches = cnr::StartupInstructionPatches();
    result.insert(result.end(), cnr_patches.begin(), cnr_patches.end());
#endif
    return result;
}

bool LookupGuestAddress(const std::string& name, std::uint32_t& address) {
    const auto runtime = g_runtime.addresses.find(name);
    if (runtime != g_runtime.addresses.end()) {
        address = runtime->second;
        return true;
    }
#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
    if (cnr::LookupGuestAddress(name, address)) return true;
#endif
    return false;
}

std::string EmbeddedModuleSummary() {
    std::string result;
    if (!g_runtime.module_id.empty()) {
        result += g_runtime.module_id;
        if (!g_runtime.version.empty()) {
            result += "@";
            result += g_runtime.version;
        }
    }
#if defined(PV7_COMPAT_MODULE_CNR) && PV7_COMPAT_MODULE_CNR
    if (!result.empty()) result += ",";
    result += "cnr@";
    result += cnr::Version();
#endif
#if defined(PV7_COMPAT_MODULE_COCOS2DX) && PV7_COMPAT_MODULE_COCOS2DX
    if (!result.empty()) result += ",";
    result += "cocos2dx@";
    result += cocos2dx::Version();
#endif
    return result.empty() ? "(none)" : result;
}

} // namespace pv7::compat
