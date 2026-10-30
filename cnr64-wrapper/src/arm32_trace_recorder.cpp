#include "arm32_trace_recorder.h"

#include <iomanip>
#include <sstream>
#include <utility>

namespace pv7 {
namespace {

const char* KindName(Arm32TraceKind kind) {
    switch (kind) {
    case Arm32TraceKind::Call: return "call";
    case Arm32TraceKind::Return: return "return";
    case Arm32TraceKind::Import: return "import";
    case Arm32TraceKind::Fallback: return "fallback";
    case Arm32TraceKind::Fault: return "fault";
    }
    return "unknown";
}

void WriteString(std::ostream& out, const std::string& value) {
    out << '"';
    for (const unsigned char c : value) {
        switch (c) {
        case '"': out << "\\\""; break;
        case '\\': out << "\\\\"; break;
        case '\n': out << "\\n"; break;
        case '\r': out << "\\r"; break;
        case '\t': out << "\\t"; break;
        default:
            if (c < 0x20) {
                out << "\\u00" << std::hex << std::setw(2)
                    << std::setfill('0') << static_cast<unsigned>(c) << std::dec;
            } else {
                out << static_cast<char>(c);
            }
        }
    }
    out << '"';
}

std::string Hex(std::uint32_t value) {
    std::ostringstream out;
    out << "0x" << std::hex << std::setw(8) << std::setfill('0') << value;
    return out.str();
}

} // namespace

Arm32TraceRecorder::Arm32TraceRecorder(std::size_t max_unique_events)
    : capacity_(max_unique_events > 100000 ? 100000 : max_unique_events) {}

std::string Arm32TraceRecorder::Key(const Arm32TraceEvent& event) {
    // Aggregation is by call site and outcome. Argument registers are samples
    // only; changing arguments do not create infinitely many event keys.
    return std::to_string(static_cast<int>(event.kind)) + ":" +
           std::to_string(event.pc) + ":" +
           std::to_string(event.target) + ":" +
           std::to_string(event.thread_id) + ":" +
           event.symbol + ":" + event.detail;
}

void Arm32TraceRecorder::Record(Arm32TraceEvent event) {
    // Never retain arbitrary-size user-provided diagnostic text.
    if (event.symbol.size() > 160) event.symbol.resize(160);
    if (event.detail.size() > 240) event.detail.resize(240);
    std::lock_guard<std::mutex> lock(mutex_);
    ++total_;
    const auto key = Key(event);
    const auto found = keys_.find(key);
    if (found != keys_.end()) {
        ++events_[found->second].count;
        return;
    }
    if (events_.size() >= capacity_) {
        ++dropped_;
        return;
    }
    keys_.emplace(key, events_.size());
    events_.push_back(Stored{std::move(event), 1});
}

std::size_t Arm32TraceRecorder::UniqueEvents() const {
    std::lock_guard<std::mutex> lock(mutex_);
    return events_.size();
}

std::uint64_t Arm32TraceRecorder::TotalEvents() const {
    std::lock_guard<std::mutex> lock(mutex_);
    return total_;
}

std::uint64_t Arm32TraceRecorder::DroppedEvents() const {
    std::lock_guard<std::mutex> lock(mutex_);
    return dropped_;
}

std::string Arm32TraceRecorder::ToJsonLines() const {
    std::lock_guard<std::mutex> lock(mutex_);
    std::ostringstream out;
    for (const auto& stored : events_) {
        const auto& event = stored.event;
        out << "{\"schema\":\"pv7.arm32.trace.v1\",\"kind\":";
        WriteString(out, KindName(event.kind));
        out << ",\"pc\":";
        WriteString(out, Hex(event.pc));
        out << ",\"target\":";
        WriteString(out, Hex(event.target));
        out << ",\"thread_id\":" << event.thread_id
            << ",\"registers_r0_r3\":[";
        for (std::size_t i = 0; i < event.args.size(); ++i) {
            if (i) out << ',';
            out << event.args[i];
        }
        out << "],\"symbol\":";
        WriteString(out, event.symbol);
        out << ",\"detail\":";
        WriteString(out, event.detail);
        out << ",\"repetitions\":" << stored.count
            << ",\"origin\":\"runtime_callback\"}\n";
    }
    out << "{\"schema\":\"pv7.arm32.trace.v1\","
        << "\"kind\":\"summary\","
        << "\"unique_events\":" << events_.size() << ','
        << "\"total_events\":" << total_ << ','
        << "\"dropped_events\":" << dropped_ << "}\n";
    return out.str();
}

} // namespace pv7
