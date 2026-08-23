#include <jni.h>
#include <android/native_window_jni.h>

#include <string>

#include "a32_test_core.h"
#include "unity_guest_probe.h"

namespace {
std::string ToString(JNIEnv* env, jstring value) {
    if (!value) return {};
    const char* chars = env->GetStringUTFChars(value, nullptr);
    const std::string result = chars ? chars : "";
    if (chars) env->ReleaseStringUTFChars(value, chars);
    return result;
}
} // namespace

extern "C" JNIEXPORT jstring JNICALL
Java_me_jacqueb_cnr64poc_MainActivity_nativeRunSelfTest(JNIEnv* env, jclass,
                                                         jstring originalLibMainPath,
                                                         jstring originalLibUnityPath,
                                                         jstring originalLibMonoPath,
                                                         jstring managedDirPath,
                                                         jstring packageCodePath,
                                                         jobject renderSurface) {
    ANativeWindow* hostWindow = renderSurface ? ANativeWindow_fromSurface(env, renderSurface) : nullptr;
    const A32SelfTestResult result = RunA32SelfTest(ToString(env, originalLibMainPath),
                                                    ToString(env, originalLibUnityPath),
                                                    ToString(env, originalLibMonoPath),
                                                    ToString(env, managedDirPath),
                                                    ToString(env, packageCodePath),
                                                    hostWindow);
    if (hostWindow) ANativeWindow_release(hostWindow);
    return env->NewStringUTF(result.report.c_str());
}

extern "C" JNIEXPORT jstring JNICALL
Java_me_jacqueb_cnr64poc_MainActivity_nativeRunUnityJniStage(JNIEnv* env, jclass,
                                                             jstring originalLibUnityPath,
                                                             jint eventLimit) {
    const UnityGuestProbeResult result = RunOriginalLibUnityProbe(ToString(env, originalLibUnityPath),
                                                                  static_cast<int>(eventLimit));
    return env->NewStringUTF(result.report.c_str());
}
