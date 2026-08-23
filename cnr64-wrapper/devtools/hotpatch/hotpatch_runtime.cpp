#include "hotpatch_runtime.h"

#include <algorithm>
#include <cctype>
#include <fstream>
#include <iterator>
#include <sstream>

#if !defined(_WIN32)
#include <dlfcn.h>
#include <setjmp.h>
#include <signal.h>
#endif

namespace {
std::string Trim(std::string value) {
    auto not_space = [](unsigned char c) { return !std::isspace(c); };
    value.erase(value.begin(), std::find_if(value.begin(), value.end(), not_space));
    value.erase(std::find_if(value.rbegin(), value.rend(), not_space).base(), value.end());
    return value;
}

bool ParseU32(const std::string& text, std::uint32_t& value) {
    try {
        std::size_t used = 0;
        const unsigned long long parsed = std::stoull(text, &used, 0);
        if (used != text.size() || parsed > 0xffffffffull) return false;
        value = static_cast<std::uint32_t>(parsed);
        return true;
    } catch (...) {
        return false;
    }
}

#if !defined(_WIN32)
thread_local sigjmp_buf* g_hotpatch_jump = nullptr;
thread_local int g_hotpatch_signal = 0;

void HotpatchSignalHandler(int signal_number) {
    if (g_hotpatch_jump) {
        g_hotpatch_signal = signal_number;
        siglongjmp(*g_hotpatch_jump, 1);
    }
    ::signal(signal_number, SIG_DFL);
    ::raise(signal_number);
}

class ScopedHotpatchSignalGuard {
public:
    ScopedHotpatchSignalGuard() {
        const int signals[] = {SIGSEGV, SIGBUS, SIGILL, SIGFPE, SIGABRT};
        struct sigaction action{};
        action.sa_handler = HotpatchSignalHandler;
        sigemptyset(&action.sa_mask);
        action.sa_flags = 0;
        for (std::size_t i = 0; i < std::size(signals); ++i) {
            sigaction(signals[i], &action, &old_[i]);
        }
    }

    ~ScopedHotpatchSignalGuard() {
        const int signals[] = {SIGSEGV, SIGBUS, SIGILL, SIGFPE, SIGABRT};
        for (std::size_t i = 0; i < std::size(signals); ++i) {
            sigaction(signals[i], &old_[i], nullptr);
        }
    }

private:
    struct sigaction old_[5]{};
};
#endif
} // namespace

HotpatchRuntime::HotpatchRuntime(std::string directory)
    : directory_(std::move(directory)) {
    if (!directory_.empty()) {
        rules_path_ = (std::filesystem::path(directory_) / "rules.txt").string();
        control_path_ = (std::filesystem::path(directory_) / "control.txt").string();
        std::error_code ec;
        std::filesystem::create_directories(directory_, ec);
    }
    Reload();
}

HotpatchRuntime::~HotpatchRuntime() {
#if !defined(_WIN32)
    for (auto& plugin : plugins_) {
        if (plugin.handle) dlclose(plugin.handle);
    }
#endif
}

void HotpatchRuntime::Reload() {
    if (directory_.empty()) return;
    const auto now = std::chrono::steady_clock::now();
    if (now < next_scan_) return;
    next_scan_ = now + std::chrono::milliseconds(250);
    ReloadControl();
    ReloadRules();
    ReloadPlugins();
}

void HotpatchRuntime::ReloadControl() {
    std::error_code ec;
    if (!std::filesystem::exists(control_path_, ec)) return;
    const auto mtime = std::filesystem::last_write_time(control_path_, ec);
    if (ec || (have_control_mtime_ && mtime == control_mtime_)) return;

    control_mtime_ = mtime;
    have_control_mtime_ = true;
    std::ifstream input(control_path_);
    std::string line;
    while (std::getline(input, line)) {
        line = Trim(line);
        if (line.empty() || line[0] == '#') continue;
        const auto eq = line.find('=');
        if (eq == std::string::npos) continue;
        const std::string key = Trim(line.substr(0, eq));
        const std::string value_text = Trim(line.substr(eq + 1));
        std::uint32_t value = 0;
        if (key == "trap_wait_ms" && ParseU32(value_text, value)) {
            trap_wait_ms_ = std::min<std::uint32_t>(value, 600000u);
        }
    }
}

void HotpatchRuntime::ReloadRules() {
    std::error_code ec;
    if (!std::filesystem::exists(rules_path_, ec)) {
        if (have_rules_mtime_) {
            rules_.clear();
            have_rules_mtime_ = false;
        }
        return;
    }
    const auto mtime = std::filesystem::last_write_time(rules_path_, ec);
    if (ec || (have_rules_mtime_ && mtime == rules_mtime_)) return;

    std::unordered_map<std::string, HotpatchRule> fresh;
    std::ifstream input(rules_path_);
    std::string line;
    std::size_t line_number = 0;
    while (std::getline(input, line)) {
        ++line_number;
        line = Trim(line);
        if (line.empty() || line[0] == '#') continue;

        std::istringstream parser(line);
        std::string action;
        std::string symbol;
        parser >> action >> symbol;
        if (action.empty() || symbol.empty()) continue;

        HotpatchRule rule;
        if (action == "noop") {
            rule.kind = HotpatchRule::Kind::ReturnU32;
            rule.value = 0;
        } else if (action == "return") {
            std::string value_text;
            parser >> value_text;
            if (!ParseU32(value_text, rule.value)) {
                last_error_ = "rules.txt invalid return value at line " + std::to_string(line_number);
                continue;
            }
            rule.kind = HotpatchRule::Kind::ReturnU32;
        } else if (action == "alias") {
            parser >> rule.target;
            if (rule.target.empty()) {
                last_error_ = "rules.txt missing alias target at line " + std::to_string(line_number);
                continue;
            }
            rule.kind = HotpatchRule::Kind::Alias;
        } else {
            last_error_ = "rules.txt unknown action at line " + std::to_string(line_number);
            continue;
        }
        fresh[symbol] = std::move(rule);
    }

    rules_ = std::move(fresh);
    rules_mtime_ = mtime;
    have_rules_mtime_ = true;
}

void HotpatchRuntime::ReloadPlugins() {
#if defined(_WIN32)
    return;
#else
    std::error_code ec;
    if (!std::filesystem::exists(directory_, ec)) return;

    static std::uint64_t generation = 1;
    for (const auto& entry : std::filesystem::directory_iterator(directory_, ec)) {
        if (ec || !entry.is_regular_file()) continue;
        const auto path = entry.path();
        if (path.extension() != ".so") continue;
        const std::string filename = path.filename().string();
        if (filename.rfind(".cnr64-loaded-", 0) == 0) continue;

        const std::string source_path = path.string();
        const auto mtime = entry.last_write_time(ec);
        if (ec) continue;
        const auto known = plugin_mtimes_.find(source_path);
        if (known != plugin_mtimes_.end() && known->second == mtime) continue;
        // Record the observed version before attempting to load it. A broken
        // plugin should not be retried every scan; changing the file mtime
        // explicitly arms the next retry.
        plugin_mtimes_[source_path] = mtime;

        // dlopen caches by path. Load a unique shadow copy so replacing a plugin
        // while the harness is running actually loads the new machine code.
        const auto shadow = path.parent_path() /
            (".cnr64-loaded-" + std::to_string(generation++) + "-" + filename);
        std::filesystem::copy_file(path, shadow,
                                   std::filesystem::copy_options::overwrite_existing, ec);
        if (ec) {
            last_error_ = "hotpatch copy failed: " + ec.message();
            ec.clear();
            continue;
        }

        void* handle = dlopen(shadow.string().c_str(), RTLD_NOW | RTLD_LOCAL);
        if (!handle) {
            const char* error_text = dlerror();
            last_error_ = std::string("hotpatch dlopen failed: ") + (error_text ? error_text : "unknown");
            continue;
        }
        auto dispatch = reinterpret_cast<Cnr64HotpatchDispatchV1>(
            dlsym(handle, "cnr64_hotpatch_dispatch_v1"));
        if (!dispatch) {
            last_error_ = "hotpatch missing cnr64_hotpatch_dispatch_v1: " + source_path;
            dlclose(handle);
            continue;
        }

        std::string display_name = filename;
        auto name_fn = reinterpret_cast<Cnr64HotpatchNameV1>(
            dlsym(handle, "cnr64_hotpatch_name_v1"));
        if (name_fn) {
            const char* supplied = name_fn();
            if (supplied && *supplied) display_name = supplied;
        }

        plugins_.push_back({source_path, display_name, mtime, handle, dispatch});
        plugin_mtimes_[source_path] = mtime;
    }
#endif
}

HotpatchRule HotpatchRuntime::LookupRule(const std::string& symbol) {
    Reload();
    const auto it = rules_.find(symbol);
    return it == rules_.end() ? HotpatchRule{} : it->second;
}

int HotpatchRuntime::DispatchPlugins(Cnr64HotpatchContextV1& context, std::string& detail) {
    Reload();
#if defined(_WIN32)
    (void)context;
    detail.clear();
    return CNR64_HOTPATCH_PASS;
#else
    for (auto it = plugins_.rbegin(); it != plugins_.rend(); ++it) {
        if (!it->dispatch) continue;
        int result = CNR64_HOTPATCH_PASS;
        int trapped_signal = 0;
        {
            ScopedHotpatchSignalGuard guard;
            sigjmp_buf jump{};
            g_hotpatch_signal = 0;
            g_hotpatch_jump = &jump;
            if (sigsetjmp(jump, 1) == 0) {
                result = it->dispatch(&context);
            } else {
                trapped_signal = g_hotpatch_signal;
                result = CNR64_HOTPATCH_PASS;
            }
            g_hotpatch_jump = nullptr;
        }
        if (trapped_signal != 0) {
            detail = it->display_name + " trapped host signal " + std::to_string(trapped_signal);
            continue;
        }
        if (result != CNR64_HOTPATCH_PASS) {
            detail = it->display_name;
            return result;
        }
    }
    detail.clear();
    return CNR64_HOTPATCH_PASS;
#endif
}

std::uint32_t HotpatchRuntime::TrapWaitMilliseconds() {
    Reload();
    return trap_wait_ms_;
}

std::string HotpatchRuntime::StatusLine() const {
    std::ostringstream out;
    out << "hotpatch dir=" << (directory_.empty() ? "(disabled)" : directory_)
        << " rules=" << rules_.size()
        << " plugins=" << plugins_.size()
        << " trap_wait_ms=" << trap_wait_ms_;
    if (!last_error_.empty()) out << " last_error=\"" << last_error_ << "\"";
    return out.str();
}
