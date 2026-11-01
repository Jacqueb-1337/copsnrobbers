#pragma once

#include <cstdint>
#include <string>
#include <vector>

struct SharedGuestLinkResult {
    bool ok = false;
    std::string report;
};

struct HostTouchPoint {
    std::int32_t id = 0;
    float x = 0.0f;
    float y = 0.0f;
    float pressure = 0.0f;
    float size = 0.0f;
};

struct HostTouchEvent {
    std::int32_t action = 0;
    std::int32_t metaState = 0;
    std::int32_t buttonState = 0;
    std::int32_t source = 0;
    std::int32_t deviceId = 0;
    std::int64_t downTimeNanos = 0;
    std::int64_t eventTimeNanos = 0;
    std::vector<HostTouchPoint> points;
};

void QueueHostTouchEvent(HostTouchEvent event);
void ClearHostTouchEvents();
void SetGuestGameLoopRunning(bool running);
bool IsGuestGameLoopRunning();
bool ConsumeGuestRestartRequest();

SharedGuestLinkResult RunSharedGuestLinkProbe(const std::string& libMainPath,
                                              const std::string& libUnityPath,
                                              const std::string& libMonoPath,
                                              const std::string& managedDirPath,
                                              const std::string& packageCodePath,
                                              void* hostNativeWindow,
                                              void* hostJniEnv = nullptr);

void QueueHostTextEvent(std::string text,bool closed,bool canceled);
