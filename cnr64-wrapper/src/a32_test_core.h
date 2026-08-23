#pragma once

#include <string>

struct A32SelfTestResult {
    bool ok = false;
    std::string report;
};

A32SelfTestResult RunA32SelfTest(const std::string& originalLibMainPath,
                                const std::string& originalLibUnityPath,
                                const std::string& originalLibMonoPath,
                                const std::string& managedDirPath,
                                const std::string& packageCodePath,
                                void* hostNativeWindow);
