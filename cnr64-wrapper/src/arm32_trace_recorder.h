#pragma once

#include <array>
#include <cstddef>
#include <cstdint>
#include <mutex>
#include <string>
#include <unordered_map>
#include <vector>

namespace pv7 {

// Passive sink for ARM32 guest execution callbacks. The loader does not yet
// call this: a future Dynarmic backend must explicitly emit real events.
// Never fabricate "executed" events from static ELF imports.
enum class Arm32TraceKind { Call, Return, Import, Fallback, Fault };

struct Arm32TraceEvent {
    Arm32TraceKind kind = Arm32TraceKind::Call;
    std::uint32_t pc = 0;
    std::uint32_t target = 0;
    std::uint32_t thread_id = 0;
    std::array<std::uint32_t, 4> args{}; // Guest r0-r3 snapshots; not decoded types
    std::string symbol;
    std::string detail;
};

class Arm32TraceRecorder {
public:
    explicit Arm32TraceRecorder(std::size_t max_unique_events = 8192);
    void Record(Arm32TraceEvent event);
    // Thread-safe snapshot. One event per unique call-site signature plus
    // repetitions, followed by a summary line with truncation information.
    std::string ToJsonLines() const;
    std::size_t UniqueEvents() const;
    std::uint64_t TotalEvents() const;
    std::uint64_t DroppedEvents() const;
private:
    struct Stored {
        Arm32TraceEvent event;
        std::uint64_t count = 1;
    };
    static std::string Key(const Arm32TraceEvent& event);
    mutable std::mutex mutex_;
    const std::size_t capacity_;
    std::uint64_t total_ = 0;
    std::uint64_t dropped_ = 0;
    std::vector<Stored> events_;
    std::unordered_map<std::string, std::size_t> keys_;
};

} // namespace pv7
