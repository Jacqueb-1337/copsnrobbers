#include <jni.h>
#include <android/native_window_jni.h>

#include <algorithm>
#include <string>
#include <utility>
#include <vector>

#include "a32_test_core.h"
#include "shared_guest_linker.h"
#include "unity_guest_probe.h"

namespace {
std::string ToString(JNIEnv* env, jstring value) {
    if (!value) return {};
    const char* chars = env->GetStringUTFChars(value, nullptr);
    const std::string result = chars ? chars : "";
    if (chars) env->ReleaseStringUTFChars(value, chars);
    return result;
}

jstring NativeRunSelfTest(JNIEnv* env, jclass,
                          jstring originalLibMainPath,
                          jstring originalLibUnityPath,
                          jstring originalLibMonoPath,
                          jstring managedDirPath,
                          jstring packageCodePath,
                          jobject renderSurface,
                          jint hostDisplayId) {
    ANativeWindow* hostWindow = renderSurface ? ANativeWindow_fromSurface(env, renderSurface) : nullptr;
    const A32SelfTestResult result = RunA32SelfTest(ToString(env, originalLibMainPath),
                                                    ToString(env, originalLibUnityPath),
                                                    ToString(env, originalLibMonoPath),
                                                    ToString(env, managedDirPath),
                                                    ToString(env, packageCodePath),
                                                    hostWindow,
                                                    env,
                                                    static_cast<std::uint32_t>(hostDisplayId));
    if (hostWindow) ANativeWindow_release(hostWindow);
    return env->NewStringUTF(result.report.c_str());
}

jstring NativeRunUnityJniStage(JNIEnv* env, jclass,
                               jstring originalLibUnityPath,
                               jint eventLimit) {
    const UnityGuestProbeResult result = RunOriginalLibUnityProbe(ToString(env, originalLibUnityPath),
                                                                  static_cast<int>(eventLimit));
    return env->NewStringUTF(result.report.c_str());
}

void NativeSetGameLoopRunning(JNIEnv*, jclass, jboolean running) {
    SetGuestGameLoopRunning(running == JNI_TRUE);
}

void NativeQueueTouch(JNIEnv* env, jclass,
                      jint action,
                      jint metaState,
                      jint buttonState,
                      jint source,
                      jint deviceId,
                      jlong downTimeNanos,
                      jlong eventTimeNanos,
                      jintArray pointerIds,
                      jfloatArray xs,
                      jfloatArray ys,
                      jfloatArray pressures,
                      jfloatArray sizes) {
    if (!pointerIds || !xs || !ys || !pressures || !sizes) return;
    jsize count = env->GetArrayLength(pointerIds);
    count = std::min(count, env->GetArrayLength(xs));
    count = std::min(count, env->GetArrayLength(ys));
    count = std::min(count, env->GetArrayLength(pressures));
    count = std::min(count, env->GetArrayLength(sizes));
    count = std::min<jsize>(count, 16);
    if (count <= 0) return;

    std::vector<jint> ids(static_cast<std::size_t>(count));
    std::vector<jfloat> x_values(static_cast<std::size_t>(count));
    std::vector<jfloat> y_values(static_cast<std::size_t>(count));
    std::vector<jfloat> pressure_values(static_cast<std::size_t>(count));
    std::vector<jfloat> size_values(static_cast<std::size_t>(count));
    env->GetIntArrayRegion(pointerIds, 0, count, ids.data());
    env->GetFloatArrayRegion(xs, 0, count, x_values.data());
    env->GetFloatArrayRegion(ys, 0, count, y_values.data());
    env->GetFloatArrayRegion(pressures, 0, count, pressure_values.data());
    env->GetFloatArrayRegion(sizes, 0, count, size_values.data());
    if (env->ExceptionCheck()) {
        env->ExceptionClear();
        return;
    }

    HostTouchEvent event;
    event.action = static_cast<std::int32_t>(action);
    event.metaState = static_cast<std::int32_t>(metaState);
    event.buttonState = static_cast<std::int32_t>(buttonState);
    event.source = static_cast<std::int32_t>(source);
    event.deviceId = static_cast<std::int32_t>(deviceId);
    event.downTimeNanos = static_cast<std::int64_t>(downTimeNanos);
    event.eventTimeNanos = static_cast<std::int64_t>(eventTimeNanos);
    event.points.reserve(static_cast<std::size_t>(count));
    for (jsize index = 0; index < count; ++index) {
        HostTouchPoint point;
        point.id = static_cast<std::int32_t>(ids[static_cast<std::size_t>(index)]);
        point.x = x_values[static_cast<std::size_t>(index)];
        point.y = y_values[static_cast<std::size_t>(index)];
        point.pressure = pressure_values[static_cast<std::size_t>(index)];
        point.size = size_values[static_cast<std::size_t>(index)];
        event.points.push_back(point);
    }
    QueueHostTouchEvent(std::move(event));
}

const JNINativeMethod kMainActivityMethods[] = {
        {
                const_cast<char*>("nativeRunSelfTest"),
                const_cast<char*>("(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Landroid/view/Surface;I)Ljava/lang/String;"),
                reinterpret_cast<void*>(NativeRunSelfTest),
        },
        {
                const_cast<char*>("nativeRunUnityJniStage"),
                const_cast<char*>("(Ljava/lang/String;I)Ljava/lang/String;"),
                reinterpret_cast<void*>(NativeRunUnityJniStage),
        },
        {
                const_cast<char*>("nativeSetGameLoopRunning"),
                const_cast<char*>("(Z)V"),
                reinterpret_cast<void*>(NativeSetGameLoopRunning),
        },
        {
                const_cast<char*>("nativeQueueTouch"),
                const_cast<char*>("(IIIIIJJ[I[F[F[F[F)V"),
                reinterpret_cast<void*>(NativeQueueTouch),
        },
};

bool TryRegisterMainActivity(JNIEnv* env, const char* className) {
    jclass clazz = env->FindClass(className);
    if (!clazz) {
        if (env->ExceptionCheck()) env->ExceptionClear();
        return false;
    }

    const jint result = env->RegisterNatives(
            clazz,
            kMainActivityMethods,
            static_cast<jint>(sizeof(kMainActivityMethods) / sizeof(kMainActivityMethods[0])));
    env->DeleteLocalRef(clazz);
    if (result != JNI_OK) {
        if (env->ExceptionCheck()) env->ExceptionClear();
        return false;
    }
    return true;
}
} // namespace

extern "C" JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM* vm, void*) {
    JNIEnv* env = nullptr;
    if (!vm || vm->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6) != JNI_OK || !env) {
        return JNI_ERR;
    }

    // The generic Project V7 template always uses this Java class namespace.
    // The final Android application/package ID remains freely configurable in the manifest.
    if (TryRegisterMainActivity(env, "app/projectv7/wrapper/MainActivity")) {
        return JNI_VERSION_1_6;
    }

    // Transitional compatibility for the existing CNR wrapper while it is migrated
    // onto the generic template. Remove after the legacy wrapper is retired.
    if (TryRegisterMainActivity(env, "me/jacqueb/cnr64poc/MainActivity")) {
        return JNI_VERSION_1_6;
    }

    return JNI_ERR;
}
