#pragma once

#include <chrono>
#include <cstdint>
#include <filesystem>
#include <string>
#include <unordered_map>
#include <vector>

#include "cnr64_hotpatch_api.h"

struct HotpatchRule {
    enum class Kind {
        None,
        ReturnU32,
        Alias,
    };

    Kind kind = Kind::None;
    std::uint32_t value = 0;
    std::string target;
};

class HotpatchRuntime {
public:
    explicit HotpatchRuntime(std::string directory);
    ~HotpatchRuntime();

    const std::string& Directory() const { return directory_; }
    void Reload();
    HotpatchRule LookupRule(const std::string& symbol);
    int DispatchPlugins(Cnr64HotpatchContextV1& context, std::string& detail);
    std::uint32_t TrapWaitMilliseconds();
    std::string StatusLine() const;

private:
    struct LoadedPlugin {
        std::string path;
        std::string display_name;
        std::filesystem::file_time_type mtime{};
        void* handle = nullptr;
        Cnr64HotpatchDispatchV1 dispatch = nullptr;
    };

    void ReloadRules();
    void ReloadPlugins();
    void ReloadControl();

    std::string directory_;
    std::string rules_path_;
    std::string control_path_;
    std::filesystem::file_time_type rules_mtime_{};
    std::filesystem::file_time_type control_mtime_{};
    bool have_rules_mtime_ = false;
    bool have_control_mtime_ = false;
    std::unordered_map<std::string, HotpatchRule> rules_;
    std::unordered_map<std::string, std::filesystem::file_time_type> plugin_mtimes_;
    std::vector<LoadedPlugin> plugins_;
    std::uint32_t trap_wait_ms_ = 120000;
    std::chrono::steady_clock::time_point next_scan_{};
    std::string last_error_;
};
