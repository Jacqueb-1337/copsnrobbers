#pragma once

#include <string>

struct SharedGuestLinkResult {
    bool ok = false;
    std::string report;
};

SharedGuestLinkResult RunSharedGuestLinkProbe(const std::string& libMainPath,
                                              const std::string& libUnityPath,
                                              const std::string& libMonoPath,
                                              const std::string& managedDirPath,
                                              const std::string& packageCodePath,
                                              void* hostNativeWindow);
