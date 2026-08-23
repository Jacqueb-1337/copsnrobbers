#pragma once

#include <string>

struct ElfGuestProbeResult {
    bool ok = false;
    std::string report;
};

// Loads the untouched ARM32 CNR libmain.so as a guest ELF image and executes
// one of its exported self-contained helper functions through Dynarmic.
ElfGuestProbeResult RunOriginalLibMainProbe(const std::string& path);
