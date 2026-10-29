#include "arm32_trace_recorder.h"

#include <cassert>
#include <iostream>
#include <string>

int main() {
    pv7::Arm32TraceRecorder trace(2);
    pv7::Arm32TraceEvent import;
    import.kind = pv7::Arm32TraceKind::Import;
    import.pc = 0x10012;
    import.target = 0x70001;
    import.thread_id = 7;
    import.args = {1, 2, 3, 4};
    import.symbol = "fopen";
    import.detail = "missing implementation";
    trace.Record(import);
    import.args = {5, 6, 7, 8};
    trace.Record(import); // same site: count increases, first args are a sample
    assert(trace.UniqueEvents() == 1);
    assert(trace.TotalEvents() == 2);
    assert(trace.DroppedEvents() == 0);
    auto output = trace.ToJsonLines();
    assert(output.find("\"symbol\":\"fopen\"") != std::string::npos);
    assert(output.find("\"repetitions\":2") != std::string::npos);
    assert(output.find("\"registers_r0_r3\":[1,2,3,4]") != std::string::npos);

    pv7::Arm32TraceEvent fault;
    fault.kind = pv7::Arm32TraceKind::Fault;
    fault.pc = 0x88;
    fault.detail = "bad \"guest\"\naddress";
    trace.Record(fault);
    assert(trace.UniqueEvents() == 2);
    trace.Record({pv7::Arm32TraceKind::Fallback, 0x90});
    assert(trace.DroppedEvents() == 1);
    output = trace.ToJsonLines();
    assert(output.find("bad \\\"guest\\\"\\naddress") != std::string::npos);
    assert(output.find("\"dropped_events\":1") != std::string::npos);
    assert(output.find("\"total_events\":4") != std::string::npos);
    assert(output.find("\"origin\":\"runtime_callback\"") != std::string::npos);

    pv7::Arm32TraceRecorder disabled(0);
    disabled.Record(import);
    assert(disabled.UniqueEvents() == 0);
    assert(disabled.DroppedEvents() == 1);
    std::cout << "PASS ARM32 bounded runtime trace sink\n";
    return 0;
}
