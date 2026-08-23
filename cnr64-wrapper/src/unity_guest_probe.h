#pragma once

#include <string>

struct UnityGuestProbeResult {
    bool ok = false;
    std::string report;
};

UnityGuestProbeResult RunOriginalLibUnityProbe(const std::string& path, int jniEventLimit = 0);
UnityGuestProbeResult RunOriginalLibMonoProbe(const std::string& path);
