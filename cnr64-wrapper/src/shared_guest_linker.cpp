#include "shared_guest_linker.h"
#include "guest_memory.h"
#include "pv7_compat_module_registry.h"
#include "pv7_binary_trace.hpp"

#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
#include "hotpatch_runtime.h"
#endif

#include <algorithm>
#include <array>
#include <atomic>
#include <cerrno>
#include <chrono>
#include <cctype>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <ctime>
#include <deque>
#include <filesystem>
#include <fstream>
#include <map>
#include <memory>
#include <mutex>
#include <optional>
#include <sstream>
#include <string>
#include <string_view>
#include <thread>
#include <unordered_map>
#include <unordered_set>
#include <utility>
#include <vector>

#if defined(__ANDROID__)
#include <jni.h>
#include <android/log.h>
#include <sys/mman.h>
#include <android/native_window.h>
#include <EGL/egl.h>
#include <GLES2/gl2.h>
#include <SLES/OpenSLES.h>
#include <SLES/OpenSLES_Android.h>
#include <zlib.h>
#endif

#if !defined(_WIN32)
#include <arpa/inet.h>
#include <fcntl.h>
#include <netdb.h>
#include <netinet/in.h>
#include <poll.h>
#include <sys/epoll.h>
#include <sys/ioctl.h>
#include <sys/socket.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/file.h>
#include <sys/uio.h>
#include <utime.h>
#include <unistd.h>
#endif

#if defined(_WIN32)
extern "C" int _access(const char*, int);
#else
#include <dirent.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <unistd.h>
extern "C" int access(const char*, int);
#endif

#include "dynarmic/interface/A32/a32.h"
#include "dynarmic/interface/exclusive_monitor.h"

namespace {
#if defined(PROJECTV7_FAST_SYMBOL_DISPATCH) && PROJECTV7_FAST_SYMBOL_DISPATCH
using Pv7DispatchNameKey = std::string_view;
#else
using Pv7DispatchNameKey = std::string;
#endif
std::mutex g_host_touch_mutex;
std::deque<HostTouchEvent> g_host_touch_queue;
std::atomic<bool> g_guest_game_loop_running{false};
std::atomic<bool> g_guest_restart_requested{false};
struct HostTextEvent { std::string text; bool closed=false,canceled=false; };
std::mutex g_host_text_mutex;
std::optional<HostTextEvent> g_host_text_event;
std::optional<HostTextEvent> PopHostTextEvent() {
    std::lock_guard<std::mutex> lock(g_host_text_mutex);
    auto event=std::move(g_host_text_event);g_host_text_event.reset();return event;
}

std::atomic<std::uint32_t> g_net_trace_count{0};
std::atomic<std::uint32_t> g_thread_trace_count{0};
std::atomic<std::uint32_t> g_thread_snapshot_count{0};

bool PopHostTouchEvent(HostTouchEvent& event) {
    std::lock_guard<std::mutex> lock(g_host_touch_mutex);
    if (g_host_touch_queue.empty()) return false;
    event = std::move(g_host_touch_queue.front());
    g_host_touch_queue.pop_front();
    return true;
}

#if defined(__ANDROID__)
class AndroidAudioOutput final {
public:
    AndroidAudioOutput() = default;
    ~AndroidAudioOutput() { Shutdown(); }

    bool Configure(std::uint32_t sample_rate) {
        Shutdown();
        if (sample_rate < 8000u || sample_rate > 192000u) return false;

        SLresult rc = slCreateEngine(&engine_object_, 0, nullptr, 0, nullptr, nullptr);
        if (rc != SL_RESULT_SUCCESS || !engine_object_) return false;
        rc = (*engine_object_)->Realize(engine_object_, SL_BOOLEAN_FALSE);
        if (rc != SL_RESULT_SUCCESS) { Shutdown(); return false; }
        rc = (*engine_object_)->GetInterface(engine_object_, SL_IID_ENGINE, &engine_);
        if (rc != SL_RESULT_SUCCESS || !engine_) { Shutdown(); return false; }

        rc = (*engine_)->CreateOutputMix(engine_, &output_mix_object_, 0, nullptr, nullptr);
        if (rc != SL_RESULT_SUCCESS || !output_mix_object_) { Shutdown(); return false; }
        rc = (*output_mix_object_)->Realize(output_mix_object_, SL_BOOLEAN_FALSE);
        if (rc != SL_RESULT_SUCCESS) { Shutdown(); return false; }

        SLDataLocator_AndroidSimpleBufferQueue queue_locator{
            SL_DATALOCATOR_ANDROIDSIMPLEBUFFERQUEUE,
            static_cast<SLuint32>(buffers_.size())
        };
        SLDataFormat_PCM pcm_format{
            SL_DATAFORMAT_PCM,
            2,
            static_cast<SLuint32>(sample_rate * 1000u),
            SL_PCMSAMPLEFORMAT_FIXED_16,
            SL_PCMSAMPLEFORMAT_FIXED_16,
            SL_SPEAKER_FRONT_LEFT | SL_SPEAKER_FRONT_RIGHT,
            SL_BYTEORDER_LITTLEENDIAN
        };
        SLDataSource source{&queue_locator, &pcm_format};
        SLDataLocator_OutputMix output_locator{SL_DATALOCATOR_OUTPUTMIX, output_mix_object_};
        SLDataSink sink{&output_locator, nullptr};
        const SLInterfaceID ids[] = {SL_IID_ANDROIDSIMPLEBUFFERQUEUE};
        const SLboolean required[] = {SL_BOOLEAN_TRUE};
        rc = (*engine_)->CreateAudioPlayer(engine_, &player_object_, &source, &sink,
                                           1, ids, required);
        if (rc != SL_RESULT_SUCCESS || !player_object_) { Shutdown(); return false; }
        rc = (*player_object_)->Realize(player_object_, SL_BOOLEAN_FALSE);
        if (rc != SL_RESULT_SUCCESS) { Shutdown(); return false; }
        rc = (*player_object_)->GetInterface(player_object_, SL_IID_PLAY, &play_);
        if (rc != SL_RESULT_SUCCESS || !play_) { Shutdown(); return false; }
        rc = (*player_object_)->GetInterface(player_object_, SL_IID_ANDROIDSIMPLEBUFFERQUEUE,
                                             &queue_);
        if (rc != SL_RESULT_SUCCESS || !queue_) { Shutdown(); return false; }
        rc = (*queue_)->RegisterCallback(queue_, &AndroidAudioOutput::BufferQueueCallback, this);
        if (rc != SL_RESULT_SUCCESS) { Shutdown(); return false; }
        rc = (*play_)->SetPlayState(play_, SL_PLAYSTATE_PLAYING);
        if (rc != SL_RESULT_SUCCESS) { Shutdown(); return false; }

        queued_.store(0, std::memory_order_release);
        write_index_ = 0;
        sample_rate_ = sample_rate;
        configured_ = true;
        return true;
    }

    bool CanAccept() const {
        return configured_ && queue_ && queued_.load(std::memory_order_acquire) < buffers_.size();
    }

    bool Enqueue(const std::uint8_t* data, std::size_t size) {
        if (!data || size == 0 || !CanAccept()) return false;
        auto& buffer = buffers_[write_index_];
        buffer.assign(data, data + size);
        const SLresult rc = (*queue_)->Enqueue(queue_, buffer.data(), static_cast<SLuint32>(buffer.size()));
        if (rc != SL_RESULT_SUCCESS) return false;
        write_index_ = (write_index_ + 1u) % buffers_.size();
        queued_.fetch_add(1u, std::memory_order_acq_rel);
        return true;
    }

    void Shutdown() {
        configured_ = false;
        queued_.store(0, std::memory_order_release);
        if (play_) (*play_)->SetPlayState(play_, SL_PLAYSTATE_STOPPED);
        if (queue_) (*queue_)->Clear(queue_);
        if (player_object_) (*player_object_)->Destroy(player_object_);
        if (output_mix_object_) (*output_mix_object_)->Destroy(output_mix_object_);
        if (engine_object_) (*engine_object_)->Destroy(engine_object_);
        player_object_ = nullptr;
        output_mix_object_ = nullptr;
        engine_object_ = nullptr;
        play_ = nullptr;
        queue_ = nullptr;
        engine_ = nullptr;
        sample_rate_ = 0;
        write_index_ = 0;
        for (auto& buffer : buffers_) buffer.clear();
    }

    std::uint32_t sample_rate() const { return sample_rate_; }
    std::size_t queued() const { return queued_.load(std::memory_order_acquire); }

private:
    static void BufferQueueCallback(SLAndroidSimpleBufferQueueItf, void* context) {
        auto* self = static_cast<AndroidAudioOutput*>(context);
        if (!self) return;
        std::size_t current = self->queued_.load(std::memory_order_acquire);
        while (current != 0u &&
               !self->queued_.compare_exchange_weak(current, current - 1u,
                                                     std::memory_order_acq_rel,
                                                     std::memory_order_acquire)) {}
    }

    SLObjectItf engine_object_ = nullptr;
    SLEngineItf engine_ = nullptr;
    SLObjectItf output_mix_object_ = nullptr;
    SLObjectItf player_object_ = nullptr;
    SLPlayItf play_ = nullptr;
    SLAndroidSimpleBufferQueueItf queue_ = nullptr;
    std::array<std::vector<std::uint8_t>, 4> buffers_{};
    std::atomic<std::size_t> queued_{0};
    std::size_t write_index_ = 0;
    std::uint32_t sample_rate_ = 0;
    bool configured_ = false;
};
#endif

constexpr std::uint32_t kMainBase = 0x00100000;
constexpr std::uint32_t kUnityBase = 0x01000000;
constexpr std::uint32_t kCnrPreloadTrylockCaller = kUnityBase + 0x00313620u;
constexpr std::uint32_t kMonoBase = 0x03000000;
constexpr std::uint32_t kMonoMetadataDecodeRow = kMonoBase + 0x001c4310u;
constexpr std::uint32_t kMonoMetadataDecodeRowCol = kMonoBase + 0x001c4520u;
 constexpr std::uint32_t kReturnStub = 0x00008000;
constexpr std::uint32_t kJniDetachStub = 0x000087f0;
constexpr std::uint32_t kJniAttachStub = 0x00008100;
constexpr std::uint32_t kJniFindClassStub = 0x00008110;
constexpr std::uint32_t kJniRegisterNativesStub = 0x00008120;
constexpr std::uint32_t kJniFatalErrorStub = 0x00008130;
constexpr std::uint32_t kJniGetJavaVmStub = 0x00008140;
constexpr std::uint32_t kJniGetEnvStub = 0x00008150;
constexpr std::uint32_t kJniNewGlobalRefStub = 0x00008160;
constexpr std::uint32_t kJniDeleteGlobalRefStub = 0x00008170;
constexpr std::uint32_t kJniGetObjectClassStub = 0x00008180;
constexpr std::uint32_t kJniExceptionCheckStub = 0x00008190;
constexpr std::uint32_t kJniPushLocalFrameStub = 0x000081a0;
constexpr std::uint32_t kJniPopLocalFrameStub = 0x000081b0;
constexpr std::uint32_t kJniGetStaticFieldIdStub = 0x000081c0;
constexpr std::uint32_t kJniGetStaticObjectFieldStub = 0x000081d0;
constexpr std::uint32_t kJniNewStringUtfStub = 0x000081e0;
constexpr std::uint32_t kJniGetMethodIdStub = 0x000081f0;
constexpr std::uint32_t kJniCallObjectMethodStub = 0x00008200;
constexpr std::uint32_t kJniCallObjectMethodVStub = 0x00008210;
constexpr std::uint32_t kJniCallObjectMethodAStub = 0x00008220;
constexpr std::uint32_t kJniDeleteLocalRefStub = 0x00008230;
constexpr std::uint32_t kJniNewLocalRefStub = 0x00008240;
constexpr std::uint32_t kJniIsSameObjectStub = 0x00008250;
constexpr std::uint32_t kJniExceptionOccurredStub = 0x00008260;
constexpr std::uint32_t kJniExceptionDescribeStub = 0x00008270;
constexpr std::uint32_t kJniExceptionClearStub = 0x00008280;
constexpr std::uint32_t kPthreadOnceReturnStub = 0x00008290;
constexpr std::uint32_t kJniGetStaticIntFieldStub = 0x000082a0;
constexpr std::uint32_t kJniCallIntMethodStub = 0x000082b0;
constexpr std::uint32_t kJniCallIntMethodVStub = 0x000082c0;
constexpr std::uint32_t kJniCallIntMethodAStub = 0x000082d0;
constexpr std::uint32_t kJniCallBooleanMethodStub = 0x000082e0;
constexpr std::uint32_t kJniCallBooleanMethodVStub = 0x000082f0;
constexpr std::uint32_t kJniCallBooleanMethodAStub = 0x00008300;
constexpr std::uint32_t kJniCallVoidMethodStub = 0x00008310;
constexpr std::uint32_t kJniCallVoidMethodVStub = 0x00008320;
constexpr std::uint32_t kJniCallVoidMethodAStub = 0x00008330;
constexpr std::uint32_t kJniIsInstanceOfStub = 0x00008340;
constexpr std::uint32_t kJniGetStringUtfLengthStub = 0x00008370;
constexpr std::uint32_t kJniGetStringUtfCharsStub = 0x00008350;
constexpr std::uint32_t kJniReleaseStringUtfCharsStub = 0x00008360;
constexpr std::uint32_t kJniAllocObjectStub = 0x00008380;
constexpr std::uint32_t kJniNewObjectStub = 0x00008390;
constexpr std::uint32_t kJniNewObjectVStub = 0x000083a0;
constexpr std::uint32_t kJniNewObjectAStub = 0x000083b0;
constexpr std::uint32_t kJniGetFieldIdStub = 0x000083c0;
constexpr std::uint32_t kJniGetObjectFieldStub = 0x000083d0;
constexpr std::uint32_t kJniGetPrimitiveFieldStub = 0x000083e0;
constexpr std::uint32_t kJniSetFieldStub = 0x000083f0;
constexpr std::uint32_t kJniNewBooleanArrayStub = 0x00008400;
constexpr std::uint32_t kJniNewByteArrayStub = 0x00008410;
constexpr std::uint32_t kJniNewCharArrayStub = 0x00008420;
constexpr std::uint32_t kJniNewShortArrayStub = 0x00008430;
constexpr std::uint32_t kJniNewIntArrayStub = 0x00008440;
constexpr std::uint32_t kJniNewLongArrayStub = 0x00008450;
constexpr std::uint32_t kJniNewFloatArrayStub = 0x00008460;
constexpr std::uint32_t kJniNewDoubleArrayStub = 0x00008470;
constexpr std::uint32_t kJniGetArrayLengthStub = 0x00008480;
constexpr std::uint32_t kJniNewObjectArrayStub = 0x00008490;
constexpr std::uint32_t kJniGetObjectArrayElementStub = 0x000084a0;
constexpr std::uint32_t kJniSetObjectArrayElementStub = 0x000084b0;
constexpr std::uint32_t kJniGetPrimitiveArrayElementsStub = 0x000084c0;
constexpr std::uint32_t kJniReleasePrimitiveArrayElementsStub = 0x000084d0;
constexpr std::uint32_t kJniGetPrimitiveArrayRegionStub = 0x000084e0;
constexpr std::uint32_t kJniSetPrimitiveArrayRegionStub = 0x000084f0;
constexpr std::uint32_t kBsearchReturnStub = 0x00008500;
constexpr std::uint32_t kJniGetStaticMethodIdStub = 0x00008510;
constexpr std::uint32_t kJniCallStaticObjectMethodStub = 0x00008520;
constexpr std::uint32_t kJniCallStaticPrimitiveMethodStub = 0x00008530;
constexpr std::uint32_t kJniCallStaticVoidMethodStub = 0x00008540;
constexpr std::uint32_t kQsortReturnStub = 0x00008550;
constexpr std::uint32_t kSignalReturnStub = 0x00008560;
constexpr std::uint32_t kJniUnknownStubBase = 0x00008800;
constexpr std::uint32_t kJniVmObject = 0x00009000;
constexpr std::uint32_t kJniVmTable = 0x00009100;
constexpr std::uint32_t kJniEnvObject = 0x00009200;
constexpr std::uint32_t kJniEnvTable = 0x00009300;
constexpr std::uint32_t kJniFakeClassHandle = 0x0000a000;
constexpr std::uint32_t kJniInputEventClassHandle = 0x0000a010;
constexpr std::uint32_t kJniKeyEventClassHandle = 0x0000a020;
constexpr std::uint32_t kJniMotionEventClassHandle = 0x0000a030;
constexpr std::uint32_t kJniFakeContextHandle = 0x0000a100;
constexpr std::uint32_t kJniFakeFieldId = 0x0000a200;
constexpr std::uint32_t kJniFakeObjectHandle = 0x0000a300;
constexpr std::uint32_t kJniFakeStringHandle = 0x0000a400;
constexpr std::uint32_t kJniFakeMethodId = 0x0000a500;
constexpr std::uint32_t kJniFakeMotionEventHandle = 0x0000a600;
constexpr std::uint32_t kJniSharedPreferencesHandle = 0x0000a700;
constexpr std::uint32_t kJniSharedPreferencesEditorHandle = 0x0000a704;
constexpr std::uint32_t kJniFakeDirectBufferHandle = 0x0000a708;
constexpr std::uint32_t kJniDynamicMethodIdBase = 0x0000a800;
constexpr std::uint32_t kJniDynamicStringHandleBase = 0x0f000000;
constexpr std::uint32_t kGuestNativeWindowHandle = 0x0000b000;
constexpr std::uint32_t kGuestLooperHandle = 0x0000b100;
constexpr std::uint32_t kGuestConfigurationHandle = 0x0000b200;
constexpr std::uint32_t kGuestSensorManagerHandle = 0x0000b300;
constexpr std::uint32_t kGuestSensorQueueHandle = 0x0000b400;
constexpr std::uint32_t kSvcJniAttach = 0x00fff001;
constexpr std::uint32_t kSvcJniFindClass = 0x00fff002;
constexpr std::uint32_t kSvcJniRegisterNatives = 0x00fff003;
constexpr std::uint32_t kSvcJniFatalError = 0x00fff004;
constexpr std::uint32_t kSvcJniGetJavaVm = 0x00fff005;
constexpr std::uint32_t kSvcJniGetEnv = 0x00fff006;
constexpr std::uint32_t kSvcJniNewGlobalRef = 0x00fff007;
constexpr std::uint32_t kSvcJniDeleteGlobalRef = 0x00fff008;
constexpr std::uint32_t kSvcJniGetObjectClass = 0x00fff009;
constexpr std::uint32_t kSvcJniNullCode = 0x00fff00a;
constexpr std::uint32_t kSvcJniExceptionCheck = 0x00fff00b;
constexpr std::uint32_t kSvcJniPushLocalFrame = 0x00fff00c;
constexpr std::uint32_t kSvcJniPopLocalFrame = 0x00fff00d;
constexpr std::uint32_t kSvcJniGetStaticFieldId = 0x00fff00e;
constexpr std::uint32_t kSvcJniGetStaticObjectField = 0x00fff00f;
constexpr std::uint32_t kSvcJniNewStringUtf = 0x00fff010;
constexpr std::uint32_t kSvcJniGetMethodId = 0x00fff011;
constexpr std::uint32_t kSvcJniCallObjectMethod = 0x00fff012;
constexpr std::uint32_t kSvcJniCallObjectMethodV = 0x00fff013;
constexpr std::uint32_t kSvcJniCallObjectMethodA = 0x00fff014;
constexpr std::uint32_t kSvcJniDeleteLocalRef = 0x00fff015;
constexpr std::uint32_t kSvcJniNewLocalRef = 0x00fff016;
constexpr std::uint32_t kSvcJniIsSameObject = 0x00fff017;
constexpr std::uint32_t kSvcJniExceptionOccurred = 0x00fff018;
constexpr std::uint32_t kSvcJniExceptionDescribe = 0x00fff019;
constexpr std::uint32_t kSvcJniExceptionClear = 0x00fff01a;
constexpr std::uint32_t kSvcJniGetStaticIntField = 0x00fff01b;
constexpr std::uint32_t kSvcJniCallIntMethod = 0x00fff01c;
constexpr std::uint32_t kSvcJniCallIntMethodV = 0x00fff01d;
constexpr std::uint32_t kSvcJniCallIntMethodA = 0x00fff01e;
constexpr std::uint32_t kSvcJniCallBooleanMethod = 0x00fff01f;
constexpr std::uint32_t kSvcJniCallBooleanMethodV = 0x00fff020;
constexpr std::uint32_t kSvcJniCallBooleanMethodA = 0x00fff021;
constexpr std::uint32_t kSvcJniCallVoidMethod = 16773154u;
constexpr std::uint32_t kSvcJniCallVoidMethodV = 16773155u;
constexpr std::uint32_t kSvcJniCallVoidMethodA = 16773156u;
constexpr std::uint32_t kSvcJniIsInstanceOf = 0x00fff025;
constexpr std::uint32_t kSvcJniGetStringUtfLength = 0x00fff028;
constexpr std::uint32_t kSvcJniGetStringUtfChars = 0x00fff026;
constexpr std::uint32_t kSvcJniReleaseStringUtfChars = 0x00fff027;
constexpr std::uint32_t kSvcJniAllocObject = 0x00fff029;
constexpr std::uint32_t kSvcJniNewObject = 0x00fff02a;
constexpr std::uint32_t kSvcJniNewObjectV = 0x00fff02b;
constexpr std::uint32_t kSvcJniNewObjectA = 0x00fff02c;
constexpr std::uint32_t kSvcJniGetFieldId = 0x00fff02d;
constexpr std::uint32_t kSvcJniGetObjectField = 0x00fff02e;
constexpr std::uint32_t kSvcJniGetPrimitiveField = 0x00fff02f;
constexpr std::uint32_t kSvcJniSetField = 0x00fff030;
constexpr std::uint32_t kSvcJniNewBooleanArray = 0x00fff031;
constexpr std::uint32_t kSvcJniNewByteArray = 0x00fff032;
constexpr std::uint32_t kSvcJniNewCharArray = 0x00fff033;
constexpr std::uint32_t kSvcJniNewShortArray = 0x00fff034;
constexpr std::uint32_t kSvcJniNewIntArray = 0x00fff035;
constexpr std::uint32_t kSvcJniNewLongArray = 0x00fff036;
constexpr std::uint32_t kSvcJniNewFloatArray = 0x00fff037;
constexpr std::uint32_t kSvcJniNewDoubleArray = 0x00fff038;
constexpr std::uint32_t kSvcJniGetArrayLength = 0x00fff039;
constexpr std::uint32_t kSvcJniNewObjectArray = 0x00fff03a;
constexpr std::uint32_t kSvcJniGetObjectArrayElement = 0x00fff03b;
constexpr std::uint32_t kSvcJniSetObjectArrayElement = 0x00fff03c;
constexpr std::uint32_t kSvcJniGetPrimitiveArrayElements = 0x00fff03d;
constexpr std::uint32_t kSvcJniReleasePrimitiveArrayElements = 0x00fff03e;
constexpr std::uint32_t kSvcJniGetPrimitiveArrayRegion = 0x00fff03f;
constexpr std::uint32_t kSvcJniSetPrimitiveArrayRegion = 0x00fff040;
constexpr std::uint32_t kSvcJniGetStaticMethodId = 0x00fff041;
constexpr std::uint32_t kSvcJniCallStaticObjectMethod = 0x00fff042;
constexpr std::uint32_t kSvcJniCallStaticPrimitiveMethod = 0x00fff043;
constexpr std::uint32_t kSvcJniCallStaticVoidMethod = 0x00fff044;
constexpr std::uint32_t kSvcJniDetach = 0x00fff045;
constexpr std::uint32_t kSvcPthreadOnceReturn = 0x00fff100;
constexpr std::uint32_t kSvcCxaThrowProbe = 0x00fff101;
constexpr std::uint32_t kSvcUnwindRaiseProbe = 0x00fff102;
constexpr std::uint32_t kSvcBsearchReturn = 0x00fff103;
constexpr std::uint32_t kSvcQsortReturn = 0x00fff104;
constexpr std::uint32_t kSvcSignalReturn = 0x00fff105;
constexpr std::uint32_t kSvcMonoMetadataDecodeRow = 0x00fff106;
constexpr std::uint32_t kSvcMonoMetadataDecodeRowCol = 0x00fff107;
 constexpr std::uint32_t kSvcJniUnknownBase = 0x00ffe000;
constexpr std::uint32_t kJniEnvFunctionCount = 236;
constexpr std::uint32_t kThunkStubStart = 0x00010000;
constexpr std::uint32_t kDataSlotStart = 0x00050000;
constexpr std::uint32_t kBootstrapRuntimeBase = 0x000d0000;
constexpr std::uint32_t kBootstrapStringBase = 0x000e0000;
constexpr std::uint32_t kGuestHeapStart = 0x05000000;
// The original 0x06e00000 limit demonstrably exhausts during Mono JIT
// compilation at interactive frame 145. A reversible 1 MiB extension can
// use part of the otherwise unused 2 MiB gap below kStackTop. Keep the
// last 1 MiB entirely reserved for the main guest stack.
#if defined(PROJECTV7_TEST_HEAP_EXTEND_1M) && PROJECTV7_TEST_HEAP_EXTEND_1M
constexpr std::uint32_t kGuestHeapEnd = 0x06f00000;
#else
constexpr std::uint32_t kGuestHeapEnd = 0x06e00000;
#endif
constexpr std::uint32_t kStackTop = 0x07000000;
// Keep cooperative guest-thread stacks entirely above the main stack and heap.
// The old 0x06ff0000-down layout eventually entered kGuestHeap and also left only
// 128 KiB between worker stacks, allowing busy Mono/Unity workers to overwrite
// parked thread frames.  The upper 16 MiB of the 128 MiB guest image is otherwise free.
constexpr std::uint32_t kGuestThreadStackTop = 0x0bfc0000;
constexpr std::uint32_t kGuestThreadStackStride = 0x00100000; // 1 MiB per guest thread
constexpr std::uint32_t kGuestThreadStackFloor = 0x07fc0000;
// Give Mono a completely separate allocator arena after the highest worker
// stack. Unlike moving kGuestHeapEnd upwards, this preserves the main stack's
// original 2 MiB reserve below kStackTop.
#if defined(PROJECTV7_TEST_HEAP_OVERFLOW_32M) && PROJECTV7_TEST_HEAP_OVERFLOW_32M
constexpr std::uint32_t kGuestOverflowHeapStart = 0x0c000000u;
constexpr std::uint32_t kGuestOverflowHeapEnd = 0x0e000000u;
#if !defined(PROJECTV7_CNR_CONTENT_HEAP) || !PROJECTV7_CNR_CONTENT_HEAP
constexpr std::size_t kGuestMemorySize = 224u * 1024u * 1024u;
#endif
#else
#if !defined(PROJECTV7_CNR_CONTENT_HEAP) || !PROJECTV7_CNR_CONTENT_HEAP
constexpr std::size_t kGuestMemorySize = 192u * 1024u * 1024u;
#endif
#endif
#if defined(PROJECTV7_CNR_CONTENT_HEAP) && PROJECTV7_CNR_CONTENT_HEAP
// Preserve all ELF mappings, stacks, existing heap addresses, and the synthetic
// JNI handle range below 0x10000000. DLC parsing needs multi-megabyte contiguous
// allocations after Unity has consumed the small bootstrap arenas.
constexpr std::uint32_t kGuestContentHeapStart = 0x10000000u;
constexpr std::uint32_t kGuestContentHeapEnd = 0x30000000u;
constexpr std::size_t kGuestMemorySize = kGuestContentHeapEnd;
static_assert(kGuestContentHeapStart >= 0x10000000u, "Content heap must not overlap JNI handles");
#endif
// Discard zero-filled anonymous pages before ELF loading. Reserving guest address
// space must not retain physical RAM for unused regions. Boundary pages containing
// the host allocator's metadata are preserved.
bool DiscardGuestPages(GuestMemory& memory, std::size_t offset, std::size_t size) {
    if (offset > memory.size() || size > memory.size() - offset) return false;
#if defined(__ANDROID__)
    const long page_size = ::sysconf(_SC_PAGESIZE);
    if (page_size <= 0) return false;
    const auto page = static_cast<std::uintptr_t>(page_size);
    const auto raw = reinterpret_cast<std::uintptr_t>(memory.data() + offset);
    const auto begin = ((raw + page - 1u) / page) * page;
    const auto end = ((raw + size) / page) * page;
    if (end > begin) return ::madvise(reinterpret_cast<void*>(begin), end - begin, MADV_DONTNEED) == 0;
#endif
    return true;
}

constexpr std::uint32_t kGuestSignalInfoSize = 128u;
constexpr std::uint32_t kGuestUcontextSize = 744u;
constexpr std::uint32_t kGuestSignalFrameReserve = 1024u;

constexpr std::uint16_t kElfTypeSharedObject = 3;
constexpr std::uint16_t kElfMachineArm = 40;
constexpr std::uint32_t kProgramTypeLoad = 1;
constexpr std::uint32_t kSectionTypeRel = 9;
constexpr std::uint32_t kSectionTypeDynsym = 11;
constexpr std::uint16_t kShnUndefined = 0;
constexpr std::uint8_t kSymbolTypeObject = 1;
constexpr std::uint8_t kSymbolTypeFunc = 2;

constexpr std::uint32_t kRelArmNone = 0;
constexpr std::uint32_t kRelArmAbs32 = 2;
constexpr std::uint32_t kRelArmRel32 = 3;
constexpr std::uint32_t kRelArmCopy = 20;
constexpr std::uint32_t kRelArmGlobDat = 21;
constexpr std::uint32_t kRelArmJumpSlot = 22;
constexpr std::uint32_t kRelArmRelative = 23;

#pragma pack(push, 1)
struct Elf32Ehdr {
    unsigned char ident[16];
    std::uint16_t type;
    std::uint16_t machine;
    std::uint32_t version;
    std::uint32_t entry;
    std::uint32_t phoff;
    std::uint32_t shoff;
    std::uint32_t flags;
    std::uint16_t ehsize;
    std::uint16_t phentsize;
    std::uint16_t phnum;
    std::uint16_t shentsize;
    std::uint16_t shnum;
    std::uint16_t shstrndx;
};

struct Elf32Phdr {
    std::uint32_t type;
    std::uint32_t offset;
    std::uint32_t vaddr;
    std::uint32_t paddr;
    std::uint32_t filesz;
    std::uint32_t memsz;
    std::uint32_t flags;
    std::uint32_t align;
};

struct Elf32Shdr {
    std::uint32_t name;
    std::uint32_t type;
    std::uint32_t flags;
    std::uint32_t addr;
    std::uint32_t offset;
    std::uint32_t size;
    std::uint32_t link;
    std::uint32_t info;
    std::uint32_t addralign;
    std::uint32_t entsize;
};

struct Elf32Sym {
    std::uint32_t name;
    std::uint32_t value;
    std::uint32_t size;
    std::uint8_t info;
    std::uint8_t other;
    std::uint16_t shndx;
};

struct Elf32Rel {
    std::uint32_t offset;
    std::uint32_t info;
};
#pragma pack(pop)

static_assert(sizeof(Elf32Ehdr) == 52);
static_assert(sizeof(Elf32Phdr) == 32);
static_assert(sizeof(Elf32Shdr) == 40);
static_assert(sizeof(Elf32Sym) == 16);
static_assert(sizeof(Elf32Rel) == 8);

struct GuestImage {
    std::string name;
    std::string path;
    std::uint32_t base = 0;
    std::uint32_t end = 0;
    std::vector<std::uint8_t> file;
    Elf32Ehdr ehdr{};
    std::size_t load_segments = 0;
};

struct ExportSymbol {
    std::string owner;
    std::uint32_t address = 0;
};

struct UnresolvedRelocation {
    std::string owner;
    std::string symbol;
    std::uint32_t target = 0;
    std::uint32_t type = 0;
    std::uint8_t symbol_type = 0;
};

struct LinkStats {
    std::size_t relative = 0;
    std::size_t internal = 0;
    std::size_t cross = 0;
    std::size_t unresolved = 0;
    std::size_t unsupported = 0;
    std::size_t unity_to_mono = 0;
    std::vector<std::string> cross_examples;
    std::vector<std::string> unresolved_examples;
};

bool RangeFits(std::size_t offset, std::size_t size, std::size_t total) {
    return offset <= total && size <= total - offset;
}

template <typename T>
bool ReadStruct(const std::vector<std::uint8_t>& bytes, std::size_t offset, T& out) {
    if (!RangeFits(offset, sizeof(T), bytes.size())) return false;
    std::memcpy(&out, bytes.data() + offset, sizeof(T));
    return true;
}

bool LoadFile(const std::string& path, std::vector<std::uint8_t>& bytes, std::string& error) {
    std::ifstream file(path, std::ios::binary | std::ios::ate);
    if (!file) {
        error = "Could not open " + path;
        return false;
    }
    const std::streamsize size = file.tellg();
    if (size <= 0) {
        error = "ELF file is empty: " + path;
        return false;
    }
    file.seekg(0, std::ios::beg);
    bytes.resize(static_cast<std::size_t>(size));
    if (!file.read(reinterpret_cast<char*>(bytes.data()), size)) {
        error = "Could not read " + path;
        return false;
    }
    return true;
}

bool GetSection(const GuestImage& image, std::uint32_t index, Elf32Shdr& out) {
    if (index >= image.ehdr.shnum || image.ehdr.shentsize < sizeof(Elf32Shdr)) return false;
    return ReadStruct(image.file,
                      static_cast<std::size_t>(image.ehdr.shoff) +
                          static_cast<std::size_t>(index) * image.ehdr.shentsize,
                      out);
}

std::string ReadString(const GuestImage& image, const Elf32Shdr& strtab, std::uint32_t offset) {
    if (offset >= strtab.size || !RangeFits(strtab.offset, strtab.size, image.file.size())) return {};
    const std::size_t start = static_cast<std::size_t>(strtab.offset) + offset;
    const std::size_t limit = static_cast<std::size_t>(strtab.offset) + strtab.size;
    std::size_t end = start;
    while (end < limit && image.file[end] != 0) ++end;
    return std::string(reinterpret_cast<const char*>(image.file.data() + start), end - start);
}

bool Read32(const GuestMemory& memory, std::uint32_t address, std::uint32_t& value) {
    if (address > memory.size() || sizeof(value) > memory.size() - address) return false;
    std::memcpy(&value, memory.data() + address, sizeof(value));
    return true;
}

bool Write32(GuestMemory& memory, std::uint32_t address, std::uint32_t value) {
    if (address > memory.size() || sizeof(value) > memory.size() - address) return false;
    std::memcpy(memory.data() + address, &value, sizeof(value));
    return true;
}

bool WriteBytes(GuestMemory& memory, std::uint32_t address, const void* data, std::size_t size) {
    if (address > memory.size() || size > memory.size() - address) return false;
    std::memcpy(memory.data() + address, data, size);
    return true;
}

bool LoadAndMapImage(GuestImage& image, GuestMemory& memory, std::string& error) {
    if (!LoadFile(image.path, image.file, error)) return false;
    if (!ReadStruct(image.file, 0, image.ehdr) ||
        image.ehdr.ident[0] != 0x7f || image.ehdr.ident[1] != 'E' ||
        image.ehdr.ident[2] != 'L' || image.ehdr.ident[3] != 'F' ||
        image.ehdr.ident[4] != 1 || image.ehdr.ident[5] != 1 ||
        image.ehdr.type != kElfTypeSharedObject || image.ehdr.machine != kElfMachineArm) {
        error = image.name + " is not ARM32 little-endian ET_DYN";
        return false;
    }

    for (std::uint32_t i = 0; i < image.ehdr.phnum; ++i) {
        Elf32Phdr phdr{};
        if (!ReadStruct(image.file,
                        static_cast<std::size_t>(image.ehdr.phoff) +
                            static_cast<std::size_t>(i) * image.ehdr.phentsize,
                        phdr)) {
            error = image.name + ": bad program header";
            return false;
        }
        if (phdr.type != kProgramTypeLoad) continue;
        const std::uint64_t guest_address = static_cast<std::uint64_t>(image.base) + phdr.vaddr;
        const std::uint64_t guest_end = guest_address + phdr.memsz;
        if (!RangeFits(phdr.offset, phdr.filesz, image.file.size()) || guest_end > memory.size()) {
            error = image.name + ": PT_LOAD outside shared guest memory";
            return false;
        }
        if (phdr.filesz) {
            std::memcpy(memory.data() + guest_address, image.file.data() + phdr.offset, phdr.filesz);
        }
        if (phdr.memsz > phdr.filesz) {
            std::memset(memory.data() + guest_address + phdr.filesz, 0, phdr.memsz - phdr.filesz);
        }
        image.end = std::max(image.end, static_cast<std::uint32_t>(guest_end));
        ++image.load_segments;
    }
    if (!image.load_segments) {
        error = image.name + ": no PT_LOAD segments";
        return false;
    }
    return true;
}

void CollectExports(const GuestImage& image,
                    std::unordered_map<std::string, ExportSymbol>& exports) {
    for (std::uint32_t i = 0; i < image.ehdr.shnum; ++i) {
        Elf32Shdr symtab{};
        if (!GetSection(image, i, symtab) || symtab.type != kSectionTypeDynsym ||
            symtab.entsize < sizeof(Elf32Sym) || !RangeFits(symtab.offset, symtab.size, image.file.size())) {
            continue;
        }
        Elf32Shdr strtab{};
        if (!GetSection(image, symtab.link, strtab)) continue;
        const std::size_t count = symtab.size / symtab.entsize;
        for (std::size_t n = 0; n < count; ++n) {
            Elf32Sym sym{};
            if (!ReadStruct(image.file, static_cast<std::size_t>(symtab.offset) + n * symtab.entsize, sym)) continue;
            if (sym.shndx == kShnUndefined || sym.name == 0) continue;
            const std::string name = ReadString(image, strtab, sym.name);
            if (name.empty()) continue;
            exports.emplace(name, ExportSymbol{image.name, image.base + sym.value});
            if (name == "JNI_OnLoad") {
                exports.emplace(image.name + "!JNI_OnLoad", ExportSymbol{image.name, image.base + sym.value});
            }
        }
    }
}

bool GetRelocationSymbol(const GuestImage& image,
                         const Elf32Shdr& relsec,
                         std::uint32_t symbol_index,
                         Elf32Sym& sym,
                         std::string& name) {
    Elf32Shdr symtab{};
    if (!GetSection(image, relsec.link, symtab) || symtab.entsize < sizeof(Elf32Sym) ||
        symbol_index >= symtab.size / symtab.entsize) {
        return false;
    }
    if (!ReadStruct(image.file,
                    static_cast<std::size_t>(symtab.offset) +
                        static_cast<std::size_t>(symbol_index) * symtab.entsize,
                    sym)) {
        return false;
    }
    Elf32Shdr strtab{};
    if (!GetSection(image, symtab.link, strtab)) return false;
    name = ReadString(image, strtab, sym.name);
    return true;
}

void AddExample(std::vector<std::string>& list, const std::string& value) {
    if (value.empty() || list.size() >= 12) return;
    if (std::find(list.begin(), list.end(), value) == list.end()) list.push_back(value);
}

bool ApplyRelocations(const GuestImage& image,
                      GuestMemory& memory,
                      const std::unordered_map<std::string, ExportSymbol>& exports,
                      LinkStats& stats,
                      std::vector<UnresolvedRelocation>& unresolved_out,
                      std::string& error) {
    for (std::uint32_t i = 0; i < image.ehdr.shnum; ++i) {
        Elf32Shdr relsec{};
        if (!GetSection(image, i, relsec) || relsec.type != kSectionTypeRel ||
            relsec.entsize < sizeof(Elf32Rel)) {
            continue;
        }
        if (!RangeFits(relsec.offset, relsec.size, image.file.size())) {
            error = image.name + ": bad relocation section";
            return false;
        }
        const std::size_t count = relsec.size / relsec.entsize;
        for (std::size_t n = 0; n < count; ++n) {
            Elf32Rel rel{};
            if (!ReadStruct(image.file,
                            static_cast<std::size_t>(relsec.offset) + n * relsec.entsize,
                            rel)) {
                error = image.name + ": unreadable relocation";
                return false;
            }
            const std::uint32_t type = rel.info & 0xffu;
            const std::uint32_t symbol_index = rel.info >> 8;
            const std::uint32_t target = image.base + rel.offset;
            std::uint32_t addend = 0;
            if (!Read32(memory, target, addend)) {
                error = image.name + ": relocation target outside memory";
                return false;
            }

            if (type == kRelArmNone) continue;
            if (type == kRelArmRelative) {
                if (!Write32(memory, target, image.base + addend)) {
                    error = image.name + ": failed R_ARM_RELATIVE write";
                    return false;
                }
                ++stats.relative;
                continue;
            }

            Elf32Sym sym{};
            std::string symbol_name;
            if (!GetRelocationSymbol(image, relsec, symbol_index, sym, symbol_name)) {
                ++stats.unsupported;
                continue;
            }

            std::uint32_t symbol_address = 0;
            std::string owner;
            bool resolved = false;
            if (sym.shndx != kShnUndefined) {
                symbol_address = image.base + sym.value;
                owner = image.name;
                resolved = true;
            } else if (!symbol_name.empty()) {
                const auto found = exports.find(symbol_name);
                if (found != exports.end()) {
                    symbol_address = found->second.address;
                    owner = found->second.owner;
                    resolved = true;
                }
            }

            if (!resolved) {
                ++stats.unresolved;
                AddExample(stats.unresolved_examples, image.name + ":" + symbol_name);
                unresolved_out.push_back({image.name, symbol_name, target, type,
                                          static_cast<std::uint8_t>(sym.info & 0x0fu)});
                continue;
            }

            std::uint32_t relocated = 0;
            switch (type) {
            case kRelArmAbs32:
                relocated = symbol_address + addend;
                break;
            case kRelArmRel32:
                relocated = symbol_address + addend - target;
                break;
            case kRelArmGlobDat:
            case kRelArmJumpSlot:
                relocated = symbol_address;
                break;
            case kRelArmCopy:
                ++stats.unsupported;
                continue;
            default:
                ++stats.unsupported;
                continue;
            }
            if (!Write32(memory, target, relocated)) {
                error = image.name + ": failed relocation write";
                return false;
            }

            if (owner == image.name) {
                ++stats.internal;
            } else {
                ++stats.cross;
                AddExample(stats.cross_examples, image.name + " -> " + owner + ": " + symbol_name);
                if (image.name == "libunity.so" && owner == "libmono.so") ++stats.unity_to_mono;
            }
        }
    }
    return true;
}

std::optional<ExportSymbol> FindExport(const std::unordered_map<std::string, ExportSymbol>& exports,
                                       const std::string& name) {
    const auto found = exports.find(name);
    if (found == exports.end()) return std::nullopt;
    return found->second;
}

class BasicEnvironment : public Dynarmic::A32::UserCallbacks {
public:
    explicit BasicEnvironment(GuestMemory& memory) : memory_(memory) {}
    Dynarmic::A32::Jit* jit = nullptr;
    bool saw_return = false;
    bool failed = false;
    bool executed_heap_code = false;
    std::uint32_t first_heap_code_pc = 0;
    std::uint32_t fault_pc = 0;
    int fault_exception = -1;
    std::size_t fallback_instruction_count = 0;
    std::uint32_t invalid_code_address = 0;
    std::uint32_t code_read_last = 0;
    std::uint32_t code_read_prev1 = 0;
    std::uint32_t code_read_prev2 = 0;
    std::uint32_t code_read_prev3 = 0;
    std::uint32_t code_read_last_image = 0;
    std::uint32_t watched_write_address = 0;
    std::size_t watched_write_count = 0;
#if defined(PROJECTV7_FAULT_CODE_WATCH) && PROJECTV7_FAULT_CODE_WATCH
    std::size_t fault_code_watch_write_count = 0;
#endif
#if defined(PROJECTV7_FAULT_CODE_SNAPSHOT) && PROJECTV7_FAULT_CODE_SNAPSHOT
    std::uint32_t fault_code_snapshot_last = 0u;
    bool fault_code_snapshot_initialized = false;
    std::size_t fault_code_snapshot_changes = 0;
#endif
    std::uint32_t bad_write_address = 0;
    std::size_t bad_write_size = 0;
    std::uint32_t bad_write_pc = 0;
    std::uint32_t bad_write_lr = 0;
    std::uint32_t bad_write_r0 = 0;
    std::uint32_t bad_write_r1 = 0;
    std::uint32_t bad_write_r2 = 0;
    std::uint32_t bad_write_r3 = 0;
    std::uint32_t bad_write_fp = 0;
    std::uint32_t bad_write_sp = 0;
    std::uint32_t bad_write_mono_cfg = 0;
    std::uint32_t bad_write_mono_cfg_field14 = 0;
    std::uint32_t bad_write_mono_field88 = 0;
    std::uint32_t bad_write_mono_local144 = 0;
    std::uint32_t bad_write_mono_cfg_field00 = 0;
    std::uint32_t bad_write_mono_local84c = 0;
    std::uint32_t bad_write_list_bad_link_source = 0;
    std::uint32_t bad_write_list_bad_link_hops = 0;
    std::uint32_t bad_write_list_source_word0 = 0;
    std::uint32_t bad_write_list_source_word8 = 0;
    std::uint32_t bad_write_list_end_node = 0;
    std::uint32_t bad_write_list_end_next = 0;
    std::uint32_t bad_write_list_stop_reason = 0;
    std::uint64_t ticks_left = 1000000;
#if defined(PROJECTV7_STARTUP_HOTPAGE_TRACE) && PROJECTV7_STARTUP_HOTPAGE_TRACE
    std::array<std::uint64_t, 16> guest_clock_id_counts{};
    std::uint64_t guest_clock_getres_calls = 0;
    std::uint64_t guest_clock_unknown_id_calls = 0;
#endif
    std::uint64_t total_cache_invalidation_us = 0;
    std::size_t total_cache_invalidation_calls = 0;
    std::unordered_map<std::uint32_t, std::unordered_set<std::uint32_t>> translated_code_words_by_page_;

    bool HasTranslatedCodeOverlap(std::uint32_t start, std::uint32_t end) const {
        if (end <= start) return false;
        const std::uint32_t first_page = start >> 12;
        const std::uint32_t last_page = (end - 1u) >> 12;
        for (std::uint32_t page = first_page;; ++page) {
            const auto page_it = translated_code_words_by_page_.find(page);
            if (page_it != translated_code_words_by_page_.end()) {
                for (const std::uint32_t code_address : page_it->second) {
                    if (code_address < end && code_address + 4u > start) return true;
                }
            }
            if (page == last_page) break;
        }
        return false;
    }

    void ForgetTranslatedCodeRange(std::uint32_t start, std::uint32_t size) {
        if (size == 0u) return;
        const std::uint64_t end64 = static_cast<std::uint64_t>(start) + size;
        const std::uint32_t end = end64 > 0x100000000ull ? 0xffffffffu : static_cast<std::uint32_t>(end64);
        const std::uint32_t first_page = start >> 12;
        const std::uint32_t last_page = (end > start ? end - 1u : start) >> 12;
        for (std::uint32_t page = first_page;; ++page) {
            auto page_it = translated_code_words_by_page_.find(page);
            if (page_it != translated_code_words_by_page_.end()) {
                for (auto it = page_it->second.begin(); it != page_it->second.end();) {
                    const std::uint32_t code_address = *it;
                    if (code_address < end && code_address + 4u > start)
                        it = page_it->second.erase(it);
                    else
                        ++it;
                }
                if (page_it->second.empty()) translated_code_words_by_page_.erase(page_it);
            }
            if (page == last_page) break;
        }
    }

    void InvalidateTranslatedCodeRange(std::uint32_t start, std::uint32_t size) {
        if (size == 0u || !jit) return;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        const auto invalidate_begin = std::chrono::steady_clock::now();
#endif
        jit->InvalidateCacheRange(start, size);
        ForgetTranslatedCodeRange(start, size);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        total_cache_invalidation_us += static_cast<std::uint64_t>(
            std::chrono::duration_cast<std::chrono::microseconds>(
                std::chrono::steady_clock::now() - invalidate_begin).count());
        ++total_cache_invalidation_calls;
#endif
    }

    std::optional<std::uint32_t> MemoryReadCode(std::uint32_t address) override {
        code_read_prev3 = code_read_prev2;
        code_read_prev2 = code_read_prev1;
        code_read_prev1 = code_read_last;
        code_read_last = address;
        if (address >= kMainBase && address < kGuestHeapStart) code_read_last_image = address;
        if (address == 0) return 0xef000000u | kSvcJniNullCode;
        if (address > memory_.size() || 4 > memory_.size() - address) {
            invalid_code_address = address;
            return std::nullopt;
        }
        if (address >= kGuestHeapStart && address < kGuestHeapEnd) {
            if (!executed_heap_code) first_heap_code_pc = address;
            executed_heap_code = true;
        }
        translated_code_words_by_page_[address >> 12].insert(address);
        return Read<std::uint32_t>(address);
    }
    std::uint8_t MemoryRead8(std::uint32_t a) override { return Read<std::uint8_t>(a); }
    std::uint16_t MemoryRead16(std::uint32_t a) override { return Read<std::uint16_t>(a); }
    std::uint32_t MemoryRead32(std::uint32_t a) override { return Read<std::uint32_t>(a); }
    std::uint64_t MemoryRead64(std::uint32_t a) override { return Read<std::uint64_t>(a); }
    void MemoryWrite8(std::uint32_t a, std::uint8_t v) override { Write(a, v); }
    void MemoryWrite16(std::uint32_t a, std::uint16_t v) override { Write(a, v); }
    void MemoryWrite32(std::uint32_t a, std::uint32_t v) override {
#if defined(__ANDROID__)
        if (watched_write_address != 0 && a == watched_write_address) {
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "WATCH write32 addr=0x%08x old=0x%08x new=0x%08x pc=0x%08x lr=0x%08x sp=0x%08x n=%zu",
                                a, Read<std::uint32_t>(a), v,
                                jit ? jit->Regs()[15] : 0u,
                                jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u,
                                watched_write_count++);
        }
#endif
        Write(a, v);
    }
    void MemoryWrite64(std::uint32_t a, std::uint64_t v) override { Write(a, v); }
    bool MemoryWriteExclusive8(std::uint32_t a, std::uint8_t v, std::uint8_t expected) override {
        if (Read<std::uint8_t>(a) != expected) return false;
        Write(a, v); return true;
    }
    bool MemoryWriteExclusive16(std::uint32_t a, std::uint16_t v, std::uint16_t expected) override {
        if (Read<std::uint16_t>(a) != expected) return false;
        Write(a, v); return true;
    }
    bool MemoryWriteExclusive32(std::uint32_t a, std::uint32_t v, std::uint32_t expected) override {
        if (Read<std::uint32_t>(a) != expected) return false;
        Write(a, v); return true;
    }
    bool MemoryWriteExclusive64(std::uint32_t a, std::uint64_t v, std::uint64_t expected) override {
        if (Read<std::uint64_t>(a) != expected) return false;
        Write(a, v); return true;
    }

    void InterpreterFallback(std::uint32_t pc, std::size_t num_instructions) override {
        failed = true;
        fault_pc = pc;
        fault_exception = -2;
        fallback_instruction_count = num_instructions;
        if (jit) jit->HaltExecution();
    }
    void CallSVC(std::uint32_t swi) override {
        if (swi == 0) saw_return = true;
        else failed = true;
        if (jit) {
            if (swi != 0) fault_pc = jit->Regs()[15];
            jit->HaltExecution();
        }
    }
    void ExceptionRaised(std::uint32_t pc, Dynarmic::A32::Exception exception) override {
        failed = true;
        fault_pc = pc;
        fault_exception = static_cast<int>(exception);
#if defined(__ANDROID__)
        const std::uint32_t aligned_pc = pc & ~1u;
        const std::uint32_t instruction = Read<std::uint32_t>(aligned_pc);
        const std::uint16_t half0 = Read<std::uint16_t>(aligned_pc);
        const std::uint16_t half1 = Read<std::uint16_t>(aligned_pc + 2u);
        __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                            "PV7EXCEPTION pc=0x%08x type=%d cpsr=0x%08x thumb=%d insn32=0x%08x half0=0x%04x half1=0x%04x lr=0x%08x",
                            pc, static_cast<int>(exception), jit ? jit->Cpsr() : 0u,
                            jit && (jit->Cpsr() & 0x20u) ? 1 : 0,
                            instruction, half0, half1, jit ? jit->Regs()[14] : 0u);
        // Collect adjacent guest bytes only on a fatal fault. This distinguishes
        // an unsupported ARM instruction from a corrupted Mono-generated code
        // page without adding any work to the normal render loop.
        __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                            "PV7EXCEPTION codewindow pc=0x%08x words=%08x,%08x,%08x,%08x,[%08x],%08x,%08x,%08x,%08x sp=%08x r0=%08x r1=%08x r2=%08x r3=%08x r12=%08x",
                            aligned_pc,
                            aligned_pc >= 16u ? Read<std::uint32_t>(aligned_pc - 16u) : 0u,
                            aligned_pc >= 12u ? Read<std::uint32_t>(aligned_pc - 12u) : 0u,
                            aligned_pc >= 8u ? Read<std::uint32_t>(aligned_pc - 8u) : 0u,
                            aligned_pc >= 4u ? Read<std::uint32_t>(aligned_pc - 4u) : 0u,
                            instruction,
                            Read<std::uint32_t>(aligned_pc + 4u),
                            Read<std::uint32_t>(aligned_pc + 8u),
                            Read<std::uint32_t>(aligned_pc + 12u),
                            Read<std::uint32_t>(aligned_pc + 16u),
                            jit ? jit->Regs()[13] : 0u,
                            jit ? jit->Regs()[0] : 0u,
                            jit ? jit->Regs()[1] : 0u,
                            jit ? jit->Regs()[2] : 0u,
                            jit ? jit->Regs()[3] : 0u,
                            jit ? jit->Regs()[12] : 0u);
#endif
        if (jit) jit->HaltExecution();
    }
    void AddTicks(std::uint64_t ticks) override { ticks_left = ticks >= ticks_left ? 0 : ticks_left - ticks; }
    std::uint64_t GetTicksRemaining() override { return ticks_left; }

protected:
    template <typename T>
    T Read(std::uint32_t address) const {
        T value{};
        if (address > memory_.size() || sizeof(T) > memory_.size() - address) return value;
        std::memcpy(&value, memory_.data() + address, sizeof(T));
        return value;
    }
    template <typename T>
    void Write(std::uint32_t address, T value) {
#if defined(__ANDROID__) && defined(PROJECTV7_FAULT_CODE_WATCH) && PROJECTV7_FAULT_CODE_WATCH
        // Trap direct CPU writes to the precise invalid Mono-generated opcode.
        // Its page-table fast path is disabled only in the dedicated watch build.
        constexpr std::uint32_t kFaultCodeAddress = 0x06bc3140u;
        if (address <= kFaultCodeAddress + 3u &&
            address + sizeof(T) > kFaultCodeAddress &&
            fault_code_watch_write_count < 200u) {
            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                "PV7CODEWRITE n=%zu at=%08x size=%zu oldword=%08x val=%llx pc=%08x lr=%08x sp=%08x",
                                fault_code_watch_write_count++, address, sizeof(T),
                                Read<std::uint32_t>(kFaultCodeAddress),
                                static_cast<unsigned long long>(value),
                                jit ? jit->Regs()[15] : 0u,
                                jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
        }
#endif
        if (address > memory_.size() || sizeof(T) > memory_.size() - address) {
            failed = true;
            fault_pc = address;
            bad_write_address = address;
            bad_write_size = sizeof(T);
            if (jit) {
                bad_write_pc = jit->Regs()[15];
                bad_write_lr = jit->Regs()[14];
                bad_write_r0 = jit->Regs()[0];
                bad_write_r1 = jit->Regs()[1];
                bad_write_r2 = jit->Regs()[2];
                bad_write_r3 = jit->Regs()[3];
                bad_write_fp = jit->Regs()[11];
                bad_write_sp = jit->Regs()[13];
                // Disassembly at libmono +0x85014 loads the STR base from
                // [fp-0x144]. Earlier +0x84fe4 normally initializes that
                // local from [context+0x14]. Capture both independently to
                // detect stale pointer/sentinel propagation. Read only.
                if (bad_write_pc >= 0x03084ff0u && bad_write_pc < 0x03085020u) {
                    auto safe_word = [&](std::uint32_t p, std::uint32_t& word) {
                        if (p > memory_.size() || 4u > memory_.size() - p) return;
                        std::memcpy(&word, memory_.data() + p, sizeof(word));
                    };
                    if (bad_write_fp >= 0x84cu) {
                        safe_word(bad_write_fp - 0x144u, bad_write_mono_local144);
                        safe_word(bad_write_fp - 0x84cu, bad_write_mono_local84c);
                        safe_word(bad_write_fp - 0x848u, bad_write_mono_cfg);
                        if (bad_write_mono_cfg != 0u && bad_write_mono_cfg < memory_.size()) {
                            safe_word(bad_write_mono_cfg, bad_write_mono_cfg_field00);
                            safe_word(bad_write_mono_cfg + 0x14u, bad_write_mono_cfg_field14);
                            if (bad_write_mono_cfg_field14 != 0u &&
                                bad_write_mono_cfg_field14 < memory_.size()) {
                                safe_word(bad_write_mono_cfg_field14 + 0x88u, bad_write_mono_field88);
                            }
                            // Inspect this list only after a guest write fault.
                            // 8192 bound, cycle protection, no guest mutations.
                            // stop_reason: 1 null, 2 invalid, 3 cycle,
                            // 4 sentinel next, 5 exhausted 8192 budget.
                            std::uint32_t node = bad_write_mono_cfg_field14;
                            std::unordered_set<std::uint32_t> visited;
                            for (std::uint32_t hop = 0u; hop < 8192u; ++hop) {
                                bad_write_list_bad_link_hops = hop;
                                bad_write_list_end_node = node;
                                if (node == 0u) { bad_write_list_stop_reason = 1u; break; }
                                if (node > memory_.size() ||
                                    12u > memory_.size() - node) {
                                    bad_write_list_stop_reason = 2u; break;
                                }
                                if (!visited.insert(node).second) {
                                    bad_write_list_stop_reason = 3u; break;
                                }
                                std::uint32_t next = 0u;
                                safe_word(node + 4u, next);
                                bad_write_list_end_next = next;
                                if (next == 0xff000000u) {
                                    bad_write_list_bad_link_source = node;
                                    bad_write_list_bad_link_hops = hop + 1u;
                                    bad_write_list_stop_reason = 4u;
                                    safe_word(node, bad_write_list_source_word0);
                                    safe_word(node + 8u, bad_write_list_source_word8);
                                    break;
                                }
                                if (hop == 8191u) bad_write_list_stop_reason = 5u;
                                node = next;
                            }
                        }
                    }
                }
                jit->HaltExecution();
            }
            return;
        }
        std::memcpy(memory_.data() + address, &value, sizeof(T));
    }
    GuestMemory& memory_;
};

bool WriteSvcStub(GuestMemory& memory, std::uint32_t address, std::uint32_t svc) {
    return svc <= 0x00ffffffu &&
           Write32(memory, address, 0xef000000u | svc) &&
           Write32(memory, address + 4, 0xe12fff1eu);
}

// Entries explicitly routed to a fallback still need executable code.
// Skipping every nonzero entry left CallFloatMethod[VA] pointing at zero bytes.
bool InitializeJniVmTable(GuestMemory& memory) {
    return WriteSvcStub(memory,kJniAttachStub,kSvcJniAttach) &&
           WriteSvcStub(memory,kJniDetachStub,kSvcJniDetach) &&
           WriteSvcStub(memory,kJniGetEnvStub,kSvcJniGetEnv) &&
           Write32(memory,kJniVmObject,kJniVmTable) &&
           Write32(memory,kJniVmTable+4u*4u,kJniAttachStub) &&
           Write32(memory,kJniVmTable+5u*4u,kJniDetachStub) &&
           Write32(memory,kJniVmTable+6u*4u,kJniGetEnvStub) &&
           Write32(memory,kJniVmTable+7u*4u,kJniAttachStub);
}

bool InitializeJniFallbackStubs(GuestMemory& memory) {
    for (std::uint32_t slot = 4; slot < kJniEnvFunctionCount; ++slot) {
        std::uint32_t entry = 0;
        if (!Read32(memory, kJniEnvTable + slot * 4u, entry)) return false;
        const std::uint32_t stub = kJniUnknownStubBase + slot * 8u;
        if (entry != 0 && entry != stub) continue;
        if (!WriteSvcStub(memory, stub, kSvcJniUnknownBase + slot) ||
            !Write32(memory, kJniEnvTable + slot * 4u, stub)) return false;
    }
    return true;
}

bool WriteReturnStub(GuestMemory& memory) {
    return WriteSvcStub(memory, kReturnStub, 0);
}

bool ExecuteSharedMonoProbe(GuestMemory& memory,
                            const std::unordered_map<std::string, ExportSymbol>& exports,
                            std::uint32_t& root_domain,
                            std::uint32_t& fault_pc) {
    const auto symbol = FindExport(exports, "mono_get_root_domain");
    if (!symbol || symbol->owner != "libmono.so" || !WriteReturnStub(memory)) return false;

    BasicEnvironment env(memory);
    Dynarmic::ExclusiveMonitor global_monitor{1};
    Dynarmic::A32::UserConfig config;
    config.callbacks = &env;
    config.processor_id = 0;
    config.global_monitor = &global_monitor;
    config.arch_version = Dynarmic::A32::ArchVersion::v7;
    config.define_unpredictable_behaviour = true;
    config.always_little_endian = true;
    config.enable_cycle_counting = true;
    config.code_cache_size = 32u * 1024u * 1024u;
    Dynarmic::A32::Jit jit(config);
    env.jit = &jit;
    jit.Regs().fill(0);
    jit.Regs()[13] = kStackTop;
    jit.Regs()[14] = kReturnStub;
    jit.Regs()[15] = symbol->address & ~1u;
    jit.SetCpsr((symbol->address & 1u) ? 0x20u : 0u);
    jit.Run();
    root_domain = jit.Regs()[0];
    fault_pc = env.fault_pc;
    return !env.failed && env.saw_return && root_domain == 0;
}

struct ThunkInstallResult {
    std::unordered_map<std::uint32_t, std::string> id_to_name;
    std::unordered_map<std::string, std::uint32_t> name_to_stub;
    std::unordered_map<std::string, std::uint32_t> name_to_data;
};

bool IsFunctionRelocation(const UnresolvedRelocation& rel) {
    if (rel.type == kRelArmJumpSlot || rel.symbol_type == kSymbolTypeFunc) return true;
    if (rel.symbol_type == kSymbolTypeObject) return false;
    // Bionic/compiler helper functions are sometimes STT_NOTYPE in old binaries.
    static const char* prefixes[] = {
        "pthread_", "__cxa_", "__aeabi_", "__gnu_", "__android_",
        "dl", "sig", "sem_"
    };
    for (const char* prefix : prefixes) {
        if (rel.symbol.rfind(prefix, 0) == 0) return true;
    }
    static const char* known_functions[] = {
        "malloc", "calloc", "realloc", "free", "memcpy", "memmove", "memset", "memcmp",
        "strlen", "strcmp", "strncmp", "strcpy", "strncpy", "strdup", "strchr", "strrchr",
        "strstr", "getenv", "setenv", "unsetenv", "mmap", "mprotect", "munmap", "mremap",
        "clock_gettime", "gettimeofday", "sysconf", "getpid", "gettid", "open", "close",
        "read", "write", "lseek", "fstat", "stat", "lstat", "access", "abort", "exit",
        "_exit", "printf", "fprintf", "snprintf", "vsnprintf", "puts", "putchar", "sleep",
        "usleep", "nanosleep", "sched_yield", "syscall", "prctl", "bsd_signal"
    };
    for (const char* name : known_functions) if (rel.symbol == name) return true;
    return false;
}

bool InstallTrapThunks(GuestMemory& memory,
                       const std::vector<UnresolvedRelocation>& unresolved,
                       ThunkInstallResult& out,
                       std::string& error) {
    std::uint32_t next_stub = kThunkStubStart;
    std::uint32_t next_data = kDataSlotStart;
    std::uint32_t next_id = 1;

    for (const auto& rel : unresolved) {
        if (rel.symbol.empty()) continue;
        std::uint32_t address = 0;
        if (IsFunctionRelocation(rel)) {
            auto found = out.name_to_stub.find(rel.symbol);
            if (found == out.name_to_stub.end()) {
                if (next_stub + 8 >= kDataSlotStart || next_id > 0x00ffffffu) {
                    error = "Thunk stub region exhausted";
                    return false;
                }
                if (!Write32(memory, next_stub, 0xef000000u | next_id) ||
                    !Write32(memory, next_stub + 4, 0xe12fff1eu)) {
                    error = "Could not write thunk stub";
                    return false;
                }
                out.id_to_name.emplace(next_id, rel.symbol);
                out.name_to_stub.emplace(rel.symbol, next_stub);
                address = next_stub;
                next_stub += 8;
                ++next_id;
            } else {
                address = found->second;
            }
        } else {
            auto found = out.name_to_data.find(rel.symbol);
            if (found == out.name_to_data.end()) {
                if (next_data + 64 >= kBootstrapStringBase) {
                    error = "Data slot region exhausted";
                    return false;
                }
                address = next_data;
                out.name_to_data.emplace(rel.symbol, address);
                std::memset(memory.data() + address, 0, 64);
                if (rel.symbol == "__page_size") Write32(memory, address, 4096);
                if (rel.symbol == "__stack_chk_guard") Write32(memory, address, 0x6b8b4567u);
                next_data += 64;
            } else {
                address = found->second;
            }
        }

        std::uint32_t addend = 0;
        Read32(memory, rel.target, addend);
        std::uint32_t relocated = address;
        if (rel.type == kRelArmAbs32) relocated = address + addend;
        else if (rel.type == kRelArmRel32) relocated = address + addend - rel.target;
        if (!Write32(memory, rel.target, relocated)) {
            error = "Could not patch unresolved relocation for " + rel.symbol;
            return false;
        }
    }

    // Unity 4.6 resolves several GLES extensions dynamically through eglGetProcAddress.
    // Preinstall guest-callable thunks for the subset we bridge so the ARM32 caller never
    // receives an unusable arm64 host function pointer.
    static const char* dynamic_gles_symbols[] = {
        "glDiscardFramebufferEXT",
        "glMapBufferOES", "glUnmapBufferOES",
        "glMapBufferRangeEXT", "glFlushMappedBufferRangeEXT",
        "glGenQueriesEXT", "glDeleteQueriesEXT", "glBeginQueryEXT", "glEndQueryEXT",
        "glGetQueryObjectuivEXT", "glQueryCounterEXT", "glGetQueryObjectui64vEXT",
        "glDrawBuffersNV",
        "glRenderbufferStorageMultisampleEXT", "glFramebufferTexture2DMultisampleEXT",
        "glRenderbufferStorageMultisampleAPPLE", "glResolveMultisampleFramebufferAPPLE",
        "glGetProgramBinaryOES", "glProgramBinaryOES",
        "glPushGroupMarkerEXT", "glPopGroupMarkerEXT"
    };
    for (const char* symbol : dynamic_gles_symbols) {
        if (out.name_to_stub.find(symbol) != out.name_to_stub.end()) continue;
        if (next_stub + 8 >= kDataSlotStart || next_id > 0x00ffffffu) {
            error = "Thunk stub region exhausted while installing dynamic GLES bridge";
            return false;
        }
        if (!Write32(memory, next_stub, 0xef000000u | next_id) ||
            !Write32(memory, next_stub + 4, 0xe12fff1eu)) {
            error = "Could not write dynamic GLES thunk stub";
            return false;
        }
        out.id_to_name.emplace(next_id, symbol);
        out.name_to_stub.emplace(symbol, next_stub);
        next_stub += 8;
        ++next_id;
    }
    return true;
}

#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
std::string DeriveHotpatchDir(const std::string& managed_dir) {
    const std::string marker = "/Data/Managed";
    const std::size_t marker_pos = managed_dir.rfind(marker);
    if (marker_pos != std::string::npos) return managed_dir.substr(0, marker_pos) + "/projectv7-dev-hotpatch";
    const std::string windows_marker = "\\Data\\Managed";
    const std::size_t windows_pos = managed_dir.rfind(windows_marker);
    if (windows_pos != std::string::npos) return managed_dir.substr(0, windows_pos) + "\\projectv7-dev-hotpatch";
    return managed_dir + "/projectv7-dev-hotpatch";
}
#endif

class BootstrapEnvironment final : public BasicEnvironment {
#ifdef PV7_CNR_BRIDGE_TEST
    friend int RunCnrJniBridgeTests();
#endif
public:
    BootstrapEnvironment(GuestMemory& memory,
                         const std::unordered_map<std::uint32_t, std::string>& thunks,
                         const std::unordered_map<std::string, std::uint32_t>& name_to_stub,
                         const std::unordered_map<std::string, ExportSymbol>& exports,
                         std::string managed_dir,
                         void* host_native_window,
                         void* host_jni_env = nullptr)
        : BasicEnvironment(memory), thunks_(thunks), name_to_stub_(name_to_stub), exports_(exports),
          managed_dir_(managed_dir), host_native_window_(host_native_window), host_jni_env_(host_jni_env)
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
          , call_trace_recorder_(managed_dir)
#endif
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
          , hotpatch_runtime_(DeriveHotpatchDir(managed_dir))
#endif
    {
#if defined(__ANDROID__)
        // Unity queries Java metrics during initJni, before nativeResize.
        // Supplying 1x1 here lets it cache the minimum 128x128 render size.
        if (host_native_window_) {
            auto* window = static_cast<ANativeWindow*>(host_native_window_);
            guest_display_width = std::max(1, ANativeWindow_getWidth(window));
            guest_display_height = std::max(1, ANativeWindow_getHeight(window));
        }
#endif
        Write32(memory_, kBootstrapRuntimeBase, 0); // errno
        WriteSvcStub(memory_, kBsearchReturnStub, kSvcBsearchReturn);
        WriteSvcStub(memory_, kQsortReturnStub, kSvcQsortReturn);
        WriteSvcStub(memory_, kSignalReturnStub, kSvcSignalReturn);
        guest_thread_launches_.reserve(512);
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
        call_trace_recorder_.WriteNames(thunks_);
#endif
        static constexpr char kLocaleC[] = "C";
        WriteBytes(memory_, kBootstrapRuntimeBase + 0x100, kLocaleC, sizeof(kLocaleC));
    }

    ~BootstrapEnvironment() override {
        CloseUrlStreams();
        ShutdownHostGraphics();
    }
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
    void TraceEvent(std::uint32_t type,std::uint32_t pc,std::uint32_t lr,
                    std::uint32_t a0=0,std::uint32_t a1=0,
                    std::uint32_t a2=0,std::uint32_t a3=0) {
        call_trace_recorder_.Add(type,current_thread_id_,pc,lr,a0,a1,a2,a3);
    }
    void FlushTrace() { call_trace_recorder_.Flush(); }
#endif

    std::string first_thunk;
    std::uint32_t first_thunk_id = 0;
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
    std::size_t hotpatch_trap_count = 0;
    std::size_t hotpatch_handled_count = 0;
    std::string hotpatch_last_detail;
    std::string HotpatchStatusLine() const { return hotpatch_runtime_.StatusLine(); }
#endif
    bool live_ctor_trace = false;
    std::size_t handled_calls = 0;
    std::size_t executable_mprotect_calls = 0;
    std::size_t arm_cacheflush_calls = 0;
    std::size_t total_executable_mprotect_calls = 0;
    std::size_t total_arm_cacheflush_calls = 0;
    std::size_t total_cacheflush_skipped_untranslated = 0;
    std::size_t total_cacheflush_invalidated_translated = 0;
    std::unordered_map<std::uint64_t, std::uint64_t> cacheflush_range_counts_;
    bool pending_cacheflush = false;
    std::uint32_t pending_cacheflush_start = 0;
    std::uint32_t pending_cacheflush_size = 0;
    std::uint32_t first_exec_region = 0;
    std::uint32_t first_exec_region_size = 0;
    std::uint32_t last_mprotect_address = 0;
    std::uint32_t last_mprotect_length = 0;
    std::uint32_t last_mprotect_prot = 0;
    std::uint32_t last_mprotect_caller_lr = 0;
    std::vector<std::string> call_trace;
    struct JniMotionEventRecord {
        HostTouchEvent event;
        std::uint64_t identity = 0;
    };
    std::optional<HostTouchEvent> active_touch_event;
    std::uint32_t guest_display_width = 1;
    std::uint32_t guest_display_height = 1;
    // Unity 4.6 can keep a smaller logical default-framebuffer viewport even
    // when Android gives it a larger Surface. The GLES bridge stretches that
    // logical viewport to the host Surface, so input must be mapped into the
    // same logical coordinate space or NGUI rejects it before raycasting.
    std::uint32_t guest_logical_viewport_width = 0;
    std::uint32_t guest_logical_viewport_height = 0;
    std::unordered_map<std::uint64_t, bool> suspect_mutex_logged_;
    std::uint64_t active_touch_identity = 0;
    std::uint64_t next_touch_identity = 1;
    std::uint32_t next_jni_motion_event_handle = 0x0e000000u;
    std::unordered_map<std::uint32_t, JniMotionEventRecord> jni_motion_events;

    const HostTouchEvent* MotionEventForHandle(std::uint32_t handle) const {
        if (handle == kJniFakeMotionEventHandle && active_touch_event) return &*active_touch_event;
        const auto found = jni_motion_events.find(handle);
        return found == jni_motion_events.end() ? nullptr : &found->second.event;
    }

    std::uint64_t MotionEventIdentityForHandle(std::uint32_t handle) const {
        if (handle == kJniFakeMotionEventHandle && active_touch_event) return active_touch_identity;
        const auto found = jni_motion_events.find(handle);
        return found == jni_motion_events.end() ? 0u : found->second.identity;
    }

    const HostTouchEvent* NativeMotionEventForHandle(std::uint32_t handle) const {
        if (const HostTouchEvent* event = MotionEventForHandle(handle)) return event;
        return active_touch_event ? &*active_touch_event : nullptr;
    }

    std::uint32_t NewMotionGlobalRef(std::uint32_t source_handle) {
        const HostTouchEvent* event = MotionEventForHandle(source_handle);
        if (!event) return source_handle;
        std::uint32_t handle = next_jni_motion_event_handle;
        next_jni_motion_event_handle += 4u;
        if (next_jni_motion_event_handle < 0x0e000000u) next_jni_motion_event_handle = 0x0e000000u;
        jni_motion_events[handle] = JniMotionEventRecord{*event, MotionEventIdentityForHandle(source_handle)};
        return handle;
    }
    struct JniNativeRegistration {
        std::string class_name;
        std::string name;
        std::string signature;
        std::uint32_t function = 0;
    };
    struct JniArrayRecord {
        std::uint32_t length = 0;
        std::uint32_t element_size = 1;
        bool object_array = false;
    };
    struct GuestPrefValue {
        enum class Type : std::uint8_t { Int = 1, Float = 2, String = 3 };
        Type type = Type::Int;
        std::int32_t int_value = 0;
        float float_value = 0.0f;
        std::string string_value;
    };
    struct BsearchFrame {
        std::uint32_t key = 0;
        std::uint32_t base = 0;
        std::uint32_t size = 0;
        std::uint32_t comparator = 0;
        std::uint32_t low = 0;
        std::uint32_t high = 0;
        std::uint32_t mid = 0;
        std::uint32_t caller_lr = 0;
    };
    struct QsortFrame {
        std::uint32_t base = 0;
        std::uint32_t size = 0;
        std::uint32_t comparator = 0;
        std::uint32_t caller_lr = 0;
        std::uint32_t pivot_guest = 0;
        std::vector<std::pair<std::uint32_t, std::uint32_t>> ranges;
        std::uint32_t lo = 0;
        std::uint32_t hi = 0;
        std::uint32_t i = 0;
        std::uint32_t j = 0;
    };
    bool jni_attach_seen = false;
    bool jni_get_java_vm_seen = false;
    bool jni_get_env_seen = false;
    bool jni_new_global_ref_seen = false;
    bool jni_get_object_class_seen = false;
    bool jni_null_code_seen = false;
    std::uint32_t jni_null_code_lr = 0;
    std::string jni_static_field_name;
    std::string jni_static_field_signature;
    bool jni_get_static_object_field_seen = false;
    bool jni_get_static_int_field_seen = false;
    std::uint32_t jni_static_int_field_value = 0;
    std::string jni_new_string_utf;
    std::string jni_fake_object_string;
    std::uint32_t next_jni_string_handle = kJniDynamicStringHandleBase;
    std::unordered_map<std::uint32_t, std::string> jni_strings;
    std::string jni_method_name;
    std::string jni_method_signature;
    std::unordered_map<std::string, std::uint32_t> jni_method_ids;
    std::unordered_map<std::uint32_t, std::string> jni_method_names;
    std::unordered_map<std::uint32_t, std::string> jni_method_signatures;
    std::unordered_map<std::uint32_t, std::string> jni_method_classes;
    std::uint32_t next_jni_method_id = kJniDynamicMethodIdBase;
    std::unordered_map<std::string, std::uint32_t> jni_field_ids;
    std::unordered_map<std::uint32_t, std::string> jni_field_names;
    std::unordered_map<std::uint32_t, std::uint32_t> jni_reflected_methods;
    std::unordered_map<std::uint32_t, std::uint32_t> jni_reflected_fields;
    std::uint32_t next_jni_field_id = 0x0f300000u;
    std::uint32_t next_jni_reflected_handle = 0x0f400000u;
    static constexpr std::uint32_t kPackageManagerHandle = 0x0f500000u;
    static constexpr std::uint32_t kPackageInfoHandle = 0x0f500004u;
    static constexpr std::uint32_t kRestartIntentHandle = 0x0f500008u;
    bool guest_prefs_loaded_ = false;
    std::unordered_map<std::string, GuestPrefValue> guest_prefs_;
    std::uint32_t jni_direct_buffer_address = 0;
    std::uint32_t jni_direct_buffer_capacity = 0;
    std::uint32_t jni_call_object_method_count = 0;
    std::uint32_t jni_call_int_method_count = 0;
    std::uint32_t jni_call_boolean_method_count = 0;
    std::uint32_t jni_unknown_slot = 0xffffffffu;
    std::uint32_t jni_unknown_lr = 0;
    std::uint32_t jni_unknown_caller_lr = 0;
    std::uint32_t jni_unknown_r0 = 0;
    std::uint32_t jni_unknown_r1 = 0;
    std::uint32_t jni_unknown_r2 = 0;
    std::uint32_t jni_unknown_r3 = 0;
    bool jni_fatal_error_seen = false;
    std::string jni_last_class;
    std::uint32_t next_jni_class_handle = 0x0d000000u;
    std::unordered_map<std::string, std::uint32_t> jni_class_handles;
    std::unordered_map<std::uint32_t, std::string> jni_class_names;
    std::vector<std::string> jni_requested_classes;
    std::vector<JniNativeRegistration> jni_native_registrations;
    std::uint32_t jni_www_class_handle = 0;
    std::uint32_t www_done_callback_ = 0;
    std::uint32_t www_error_callback_ = 0;
    std::uint32_t www_header_callback_ = 0;
    std::uint32_t www_progress_callback_ = 0;
    std::uint32_t www_read_callback_ = 0;
    struct HostHttpPending {
        std::uint32_t request_id = 0;
        int status = -1;
        std::string headers;
        std::string error;
        std::vector<std::uint8_t> body;
    };
    std::deque<HostHttpPending> host_http_pending_;
    std::uint32_t next_www_object_handle_ = 0x0d800000u;
    std::vector<std::string> diagnostic_formats;
    std::vector<std::pair<std::uint32_t, std::size_t>> allocation_trace;
    std::vector<std::string> file_trace;
    bool premature_return = false;
    std::uint32_t return_sp = 0;
    std::uint32_t return_r11 = 0;
    std::uint32_t premature_r0 = 0;
    std::vector<std::uint32_t> premature_regs;
    std::uint32_t unexpected_svc0_pc = 0;
    std::uint32_t unexpected_svc0_r7 = 0;
    std::vector<std::pair<std::uint32_t, std::uint32_t>> premature_frames;
    bool pending_guest_callback = false;
    std::size_t pthread_once_callbacks = 0;

    void ResetPhaseDiagnostics() {
        first_thunk.clear();
        first_thunk_id = 0;
        handled_calls = 0;
        executable_mprotect_calls = 0;
        arm_cacheflush_calls = 0;
        pending_cacheflush = false;
        pending_cacheflush_start = 0;
        pending_cacheflush_size = 0;
        first_exec_region = 0;
        first_exec_region_size = 0;
        last_mprotect_address = 0;
        last_mprotect_length = 0;
        last_mprotect_prot = 0;
        last_mprotect_caller_lr = 0;
        fault_exception = -1;
        fallback_instruction_count = 0;
        invalid_code_address = 0;
        code_read_last = 0;
        code_read_prev1 = 0;
        code_read_prev2 = 0;
        code_read_prev3 = 0;
        code_read_last_image = 0;
        bad_write_address = 0;
        bad_write_size = 0;
        bad_write_pc = 0;
        bad_write_lr = 0;
        bad_write_r0 = 0;
        bad_write_r1 = 0;
        bad_write_r2 = 0;
        bad_write_r3 = 0;
        executed_heap_code = false;
        first_heap_code_pc = 0;
        call_trace.clear();
        jni_attach_seen = false;
        jni_get_java_vm_seen = false;
        jni_get_env_seen = false;
        jni_new_global_ref_seen = false;
        jni_get_object_class_seen = false;
        jni_null_code_seen = false;
        jni_null_code_lr = 0;
        jni_static_field_name.clear();
        jni_static_field_signature.clear();
        jni_get_static_object_field_seen = false;
        jni_get_static_int_field_seen = false;
        jni_static_int_field_value = 0;
        jni_new_string_utf.clear();
        jni_fake_object_string.clear();
        jni_method_name.clear();
        jni_method_signature.clear();
        jni_call_object_method_count = 0;
        jni_call_int_method_count = 0;
        jni_call_boolean_method_count = 0;
        jni_unknown_slot = 0xffffffffu;
        jni_unknown_lr = 0;
        jni_unknown_caller_lr = 0;
        jni_unknown_r0 = 0;
        jni_unknown_r1 = 0;
        jni_unknown_r2 = 0;
        jni_unknown_r3 = 0;
        jni_fatal_error_seen = false;
        jni_last_class.clear();
        jni_requested_classes.clear();
        jni_native_registrations.clear();
        diagnostic_formats.clear();
        allocation_trace.clear();
        file_trace.clear();
        premature_return = false;
        return_sp = 0;
        return_r11 = 0;
        premature_r0 = 0;
        premature_regs.clear();
        unexpected_svc0_pc = 0;
        unexpected_svc0_r7 = 0;
        premature_frames.clear();
        pending_guest_callback = false;
        pthread_once_callbacks = 0;
        cooperative_yield_requested_ = false;
        thread_exit_requested_ = false;
    }

    bool FastMonoMetadataDecodeRow(bool single_column) {
        if (!jit) return false;
        const std::uint32_t table = jit->Regs()[0];
        const std::uint32_t row_index = jit->Regs()[1];
        if (!Fits(table, 12u)) return false;

        const std::uint32_t row_info = Read<std::uint32_t>(table + 4u);
        const std::uint32_t encoding = Read<std::uint32_t>(table + 8u);
        const std::uint32_t row_count = row_info & 0x00ffffffu;
        const std::uint32_t row_size = row_info >> 24;
        const std::uint32_t column_count = encoding >> 24;
        if (row_index >= row_count || row_size == 0u || column_count == 0u || column_count > 12u)
            return false;

        const std::uint32_t row_base = Read<std::uint32_t>(table);
        const std::uint64_t row64 = static_cast<std::uint64_t>(row_base) +
                                    static_cast<std::uint64_t>(row_index) * row_size;
        if (row64 > UINT32_MAX || !Fits(static_cast<std::uint32_t>(row64), row_size)) return false;
        std::uint32_t cursor = static_cast<std::uint32_t>(row64);

        auto read_column = [&](std::uint32_t column, std::uint32_t& value) -> bool {
            const std::uint32_t width = ((encoding >> (column * 2u)) & 3u) + 1u;
            if (width == 1u) {
                if (!Fits(cursor, 1u)) return false;
                value = Read<std::uint8_t>(cursor);
            } else if (width == 2u) {
                if (!Fits(cursor, 2u)) return false;
                value = Read<std::uint16_t>(cursor);
            } else if (width == 4u) {
                if (!Fits(cursor, 4u)) return false;
                value = Read<std::uint32_t>(cursor);
            } else {
                return false;
            }
            cursor += width;
            return true;
        };

        if (single_column) {
            const std::uint32_t wanted_column = jit->Regs()[2];
            if (wanted_column >= column_count) return false;
            std::uint32_t value = 0u;
            for (std::uint32_t column = 0; column <= wanted_column; ++column) {
                if (!read_column(column, value)) return false;
            }
            jit->Regs()[0] = value;
            ++mono_metadata_decode_row_col_fast_calls_;
            return true;
        }

        const std::uint32_t output = jit->Regs()[2];
        const std::uint32_t output_columns = jit->Regs()[3];
        if (output_columns != column_count || !Fits(output, column_count * 4u)) return false;
        for (std::uint32_t column = 0; column < column_count; ++column) {
            std::uint32_t value = 0u;
            if (!read_column(column, value) || !Write32(memory_, output + column * 4u, value)) return false;
        }
        ++mono_metadata_decode_row_fast_calls_;
        return true;
    }

     // Mango's standalone loader deliberately treats SVC as an ABI boundary: the
    // CPU engine stops with the guest registers intact, and a compatibility layer
    // decides what the syscall means. Dynarmic already gives us the equivalent
    // callback, so keep all real ARM EABI syscalls here instead of growing ad-hoc
    // SVC cases throughout the translator.
    bool DispatchGuestSyscall(std::uint32_t syscall_nr) {
        if (!jit) return false;
        switch (syscall_nr) {
            case 0x000f0002u: { // __ARM_NR_cacheflush
                const std::uint32_t start = jit->Regs()[0];
                const std::uint32_t end = jit->Regs()[1];
                ++arm_cacheflush_calls;
                ++total_arm_cacheflush_calls;

                const bool valid_range =
                    end > start && static_cast<std::size_t>(end) <= memory_.size();

                if (valid_range) {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                    const std::uint64_t range_key =
                        (static_cast<std::uint64_t>(start) << 32u) | static_cast<std::uint64_t>(end - start);
                    ++cacheflush_range_counts_[range_key];
#endif
                    ++total_cacheflush_invalidated_translated;
                    pending_cacheflush = true;
                    pending_cacheflush_start = start;
                    pending_cacheflush_size = end - start;
                    // Guest cacheflush is authoritative for self-modifying/JIT code.
                    // Our translated-range bookkeeping can lag behind newly generated
                    // Mono code, so always invalidate a valid requested range.
                    // Do this after jit.Run() returns to avoid recursively taking
                    // Dynarmic's cache lock from inside the SVC callback.
                    jit->HaltExecution();
                } else {
                    ++total_cacheflush_skipped_untranslated;
                }
                jit->Regs()[0] = 0u;
                return true;
            }
            case 0x000f0005u: // __ARM_NR_set_tls
                arm_tls_value_ = jit->Regs()[0];
                jit->Regs()[0] = 0u;
                return true;
            case 0x000f0006u: // __ARM_NR_get_tls
                jit->Regs()[0] = arm_tls_value_;
                return true;
            case 175u: { // ARM EABI __NR_rt_sigprocmask
                const std::uint32_t how = jit->Regs()[0];
                const std::uint32_t set_ptr = jit->Regs()[1];
                const std::uint32_t old_ptr = jit->Regs()[2];
                const std::uint32_t sigset_size = jit->Regs()[3];
                if (sigset_size != 4u && sigset_size != 8u) {
                    jit->Regs()[0] = static_cast<std::uint32_t>(-22); // -EINVAL
                    return true;
                }
                std::uint32_t mask = guest_signal_masks_[current_thread_id_];
                if (old_ptr != 0u) {
                    if (!Fits(old_ptr, sigset_size)) {
                        jit->Regs()[0] = static_cast<std::uint32_t>(-14); // -EFAULT
                        return true;
                    }
                    Write32(memory_, old_ptr, mask);
                    if (sigset_size == 8u) Write32(memory_, old_ptr + 4u, 0u);
                }
                if (set_ptr != 0u) {
                    if (!Fits(set_ptr, sigset_size)) {
                        jit->Regs()[0] = static_cast<std::uint32_t>(-14); // -EFAULT
                        return true;
                    }
                    std::uint32_t requested = 0u;
                    Read32(memory_, set_ptr, requested);
                    switch (how) {
                        case 0u: mask |= requested; break;  // SIG_BLOCK
                        case 1u: mask &= ~requested; break; // SIG_UNBLOCK
                        case 2u: mask = requested; break;   // SIG_SETMASK
                        default:
                            jit->Regs()[0] = static_cast<std::uint32_t>(-22); // -EINVAL
                            return true;
                    }
                    mask &= ~GuestSignalBit(9u);
                    mask &= ~GuestSignalBit(19u);
                    guest_signal_masks_[current_thread_id_] = mask;
                    if (HasDeliverablePendingGuestSignal(current_thread_id_))
                        MarkGuestThreadRunnable(current_thread_id_);
                }
                jit->Regs()[0] = 0u;
                return true;
            }
            case 176u: { // ARM EABI __NR_rt_sigpending
                const std::uint32_t set_ptr = jit->Regs()[0];
                const std::uint32_t sigset_size = jit->Regs()[1];
                if ((sigset_size != 4u && sigset_size != 8u) || !set_ptr || !Fits(set_ptr, sigset_size)) {
                    jit->Regs()[0] = static_cast<std::uint32_t>(-22); // -EINVAL/-EFAULT is enough here
                    return true;
                }
                std::uint32_t pending_mask = 0u;
                const auto pending = guest_pending_signals_.find(current_thread_id_);
                if (pending != guest_pending_signals_.end()) {
                    for (const std::uint32_t signal : pending->second)
                        pending_mask |= GuestSignalBit(signal);
                }
                Write32(memory_, set_ptr, pending_mask);
                if (sigset_size == 8u) Write32(memory_, set_ptr + 4u, 0u);
                jit->Regs()[0] = 0u;
                return true;
            }
            default:
                return false;
        }
    }

    void CallSVC(std::uint32_t swi) override {
        if (DispatchUrlStreamJni(swi)) return;
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
        pv7::trace::SvcScope svc_trace(call_trace_recorder_,swi,current_thread_id_,
                                      jit ? jit->Regs().data() : nullptr);
#endif
        if (swi == 0) {
            const std::uint32_t svc_pc = jit ? jit->Regs()[15] : 0;
            if (jit && (svc_pc < kReturnStub || svc_pc > kReturnStub + 8)) {
                const std::uint32_t syscall_nr = jit->Regs()[7];
                if (DispatchGuestSyscall(syscall_nr)) return;
                unexpected_svc0_pc = svc_pc;
                unexpected_svc0_r7 = syscall_nr;
                first_thunk = "__kernel_svc0";
                first_thunk_id = unexpected_svc0_r7;
                jit->HaltExecution();
                return;
            }
            return_sp = jit ? jit->Regs()[13] : 0;
            return_r11 = jit ? jit->Regs()[11] : 0;
            // Worker entry points intentionally use per-thread guest stacks. Reaching
            // the common return sentinel from a worker is a normal thread-function
            // return, not the main-call stack corruption check used below.
            if (current_thread_id_ != 1) {
                saw_return = true;
                if (jit) jit->HaltExecution();
                return;
            }
            if (jit && return_sp != kStackTop) {
                premature_return = true;
                premature_r0 = jit->Regs()[0];
                premature_regs.assign(jit->Regs().begin(), jit->Regs().end());
                std::uint32_t fp = return_r11;
                for (int depth = 0; depth < 12; ++depth) {
                    if (fp < 8 || fp >= kStackTop || fp >= memory_.size()) break;
                    std::uint32_t saved_lr = 0;
                    std::uint32_t previous_fp = 0;
                    if (!Read32(memory_, fp, saved_lr) || !Read32(memory_, fp - 4, previous_fp)) break;
                    premature_frames.emplace_back(fp, saved_lr);
                    if (previous_fp <= fp || previous_fp > kStackTop) break;
                    fp = previous_fp;
                }
                jit->HaltExecution();
                return;
            }
            saw_return = true;
            if (jit) jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcUnwindRaiseProbe) {
            const std::uint32_t exception = jit->Regs()[0];
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "UNWIND RAISE exception=0x%08x r1=0x%08x r2=0x%08x r3=0x%08x sp=0x%08x lr=0x%08x",
                                exception, jit->Regs()[1], jit->Regs()[2], jit->Regs()[3],
                                jit->Regs()[13], jit->Regs()[14]);
#endif
            failed = true;
            fault_pc = kMainBase + 0x00004d74u;
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcCxaThrowProbe) {
            const std::uint32_t object = jit->Regs()[0];
            const std::uint32_t type_info = jit->Regs()[1];
            const std::uint32_t destructor = jit->Regs()[2];
            std::uint32_t name_ptr = 0;
            std::string type_name;
            if (type_info && Fits(type_info + 4u, 4u)) {
                name_ptr = Read<std::uint32_t>(type_info + 4u);
                if (name_ptr) type_name = ReadCString(name_ptr);
            }
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "CXX THROW object=0x%08x type=0x%08x name=%s dtor=0x%08x sp=0x%08x lr=0x%08x",
                                object, type_info, type_name.c_str(), destructor,
                                jit->Regs()[13], jit->Regs()[14]);
#endif
            failed = true;
            fault_pc = kUnityBase + 0x00939fd0u;
            jit->HaltExecution();
            return;
        }
         if (jit && (swi == kSvcMonoMetadataDecodeRow || swi == kSvcMonoMetadataDecodeRowCol)) {
            const bool single_column = swi == kSvcMonoMetadataDecodeRowCol;
            if (FastMonoMetadataDecodeRow(single_column)) return;
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                "PV7 mono metadata fastpath rejected svc=0x%06x r0=0x%08x r1=%u r2=0x%08x r3=%u lr=0x%08x",
                                swi, jit->Regs()[0], jit->Regs()[1], jit->Regs()[2], jit->Regs()[3], jit->Regs()[14]);
#endif
            failed = true;
            fault_pc = single_column ? kMonoMetadataDecodeRowCol : kMonoMetadataDecodeRow;
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcBsearchReturn) {
            if (bsearch_frames_.empty()) {
                failed = true;
                fault_pc = jit->Regs()[15];
                jit->HaltExecution();
                return;
            }
            BsearchFrame& frame = bsearch_frames_.back();
            const int comparison = static_cast<std::int32_t>(jit->Regs()[0]);
            if (comparison == 0) {
                const std::uint32_t result = frame.base + frame.mid * frame.size;
                const std::uint32_t caller_lr = frame.caller_lr;
                bsearch_frames_.pop_back();
                jit->Regs()[0] = result;
                jit->Regs()[14] = caller_lr;
                return;
            }
            if (comparison < 0) frame.high = frame.mid;
            else frame.low = frame.mid + 1u;
            if (frame.low >= frame.high) {
                const std::uint32_t caller_lr = frame.caller_lr;
                bsearch_frames_.pop_back();
                jit->Regs()[0] = 0;
                jit->Regs()[14] = caller_lr;
                return;
            }
            frame.mid = frame.low + (frame.high - frame.low) / 2u;
            const std::uint64_t element64 = static_cast<std::uint64_t>(frame.base) +
                                            static_cast<std::uint64_t>(frame.mid) * frame.size;
            if (element64 > UINT32_MAX || !Fits(static_cast<std::uint32_t>(element64), frame.size)) {
                const std::uint32_t caller_lr = frame.caller_lr;
                bsearch_frames_.pop_back();
                jit->Regs()[0] = 0;
                jit->Regs()[14] = caller_lr;
                return;
            }
            jit->Regs()[0] = frame.key;
            jit->Regs()[1] = static_cast<std::uint32_t>(element64);
            jit->Regs()[2] = 0;
            jit->Regs()[3] = 0;
            jit->Regs()[14] = kBsearchReturnStub;
            jit->Regs()[15] = frame.comparator & ~1u;
            jit->SetCpsr((frame.comparator & 1u) ? 0x20u : 0u);
            pending_guest_callback = true;
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcQsortReturn) {
            if (qsort_frames_.empty()) {
                failed = true;
                fault_pc = jit->Regs()[15];
                jit->HaltExecution();
                return;
            }
            HandleGuestQsortComparison(static_cast<std::int32_t>(jit->Regs()[0]));
            return;
        }
        if (jit && swi == kSvcSignalReturn) {
            auto frames = guest_signal_frames_.find(current_thread_id_);
            if (frames == guest_signal_frames_.end() || frames->second.empty()) {
                failed = true;
                fault_pc = jit->Regs()[15];
                jit->HaltExecution();
                return;
            }
            const GuestSignalFrame frame = frames->second.back();
            frames->second.pop_back();
            if (frames->second.empty()) guest_signal_frames_.erase(frames);
            jit->Regs() = frame.regs;
            jit->ExtRegs() = frame.ext_regs;
            jit->SetCpsr(frame.cpsr);
            jit->SetFpscr(frame.fpscr);
            guest_signal_masks_[current_thread_id_] = frame.signal_mask;
            const auto sigsuspend_stub = name_to_stub_.find("sigsuspend");
            if (sigsuspend_stub != name_to_stub_.end() && frame.regs[15] == sigsuspend_stub->second)
                guest_signal_interrupted_[current_thread_id_] = true;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL return thread=%u signal=%u resume_pc=0x%08x",
                                current_thread_id_, frame.signal, frame.regs[15]);
#endif
            return;
        }
        if (jit && swi == kSvcPthreadOnceReturn) {
            if (pthread_once_frames_.empty()) {
                failed = true;
                fault_pc = jit->Regs()[15];
                jit->HaltExecution();
                return;
            }
            const PthreadOnceFrame frame = pthread_once_frames_.back();
            pthread_once_frames_.pop_back();
            pthread_once_done_[frame.control] = true;
            if (frame.control && Fits(frame.control, 4)) Write32(memory_, frame.control, 2u);
            jit->Regs()[0] = 0;
            jit->Regs()[14] = frame.caller_lr;
            return;
        }
        if (jit && swi == kSvcJniNullCode) {
            jni_null_code_seen = true;
            jni_null_code_lr = jit->Regs()[14];
            failed = true;
            fault_pc = 0;
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcJniDetach) {
            // Guest workers share a cooperative host thread. Detaching a guest
            // must not detach that host thread from the real Android VM.
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniAttach) {
            jni_attach_seen = true;
            if (jit->Regs()[1]) Write32(memory_, jit->Regs()[1], kJniEnvObject);
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetJavaVm) {
            jni_get_java_vm_seen = true;
            if (jit->Regs()[1]) Write32(memory_, jit->Regs()[1], kJniVmObject);
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetEnv) {
            jni_get_env_seen = true;
            if (jit->Regs()[1]) Write32(memory_, jit->Regs()[1], kJniEnvObject);
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniNewGlobalRef) {
            jni_new_global_ref_seen = true;
            const std::uint32_t source_handle = jit->Regs()[1];
            jit->Regs()[0] = NewMotionGlobalRef(source_handle);
#if defined(__ANDROID__)
            if (MotionEventForHandle(jit->Regs()[0])) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7INPUT NewGlobalRef motion src=0x%08x dst=0x%08x identity=%llu refs=%zu",
                                    source_handle, jit->Regs()[0],
                                    static_cast<unsigned long long>(MotionEventIdentityForHandle(jit->Regs()[0])),
                                    jni_motion_events.size());
            }
#endif
            return;
        }
        if (jit && swi == kSvcJniAllocObject) {
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && (swi == kSvcJniNewObject || swi == kSvcJniNewObjectV || swi == kSvcJniNewObjectA)) {
            const std::uint32_t class_handle = jit->Regs()[1];
            const std::uint32_t method_id = jit->Regs()[2];
            std::string class_name = JniClassNameFor(class_handle);
            const auto method_class = jni_method_classes.find(method_id);
            if (class_name.empty() && method_class != jni_method_classes.end()) class_name = method_class->second;
            const std::string method_name = JniMethodNameFor(method_id);
            const std::string method_signature = JniMethodSignatureFor(method_id);
            if (class_name=="java/io/File" && method_name=="<init>" &&
                method_signature=="(Ljava/lang/String;)V") {
                const auto guest_path=JniStringValue(JniConstructorArg32(swi,0));
                std::string mapped;
                if (pv7::compat::NormalizeGuestPath(guest_path,managed_dir_,mapped) &&
                    mapped.find("/GuestSharedStorage/")!=std::string::npos) {
                    jit->Regs()[0]=Pv7FileRef(mapped); return;
                }
            }
            if (class_name == "com/unity3d/player/WWW" && method_name == "<init>" &&
                method_signature == "(ILjava/lang/String;[BLjava/util/Map;)V") {
                const std::uint32_t request_id = JniConstructorArg32(swi, 0);
                const std::string url = JniStringValue(JniConstructorArg32(swi, 1));
                const std::uint32_t post_handle = JniConstructorArg32(swi, 2);
                std::vector<std::uint8_t> post_data;
                const bool has_post = CopyJniByteArray(post_handle, post_data);
#if defined(__ANDROID__)
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7WWW construct id=%u url=%s post=%zu",
                                    request_id, url.c_str(), has_post ? post_data.size() : 0u);
#endif
                if (!FetchHostHttp(request_id, url, has_post ? &post_data : nullptr)) {
                    HostHttpPending pending;
                    pending.request_id = request_id;
                    pending.error = "CNR64 host HTTP bridge unavailable";
                    host_http_pending_.push_back(std::move(pending));
                }
                const std::uint32_t object_handle = next_www_object_handle_;
                next_www_object_handle_ += 4u;
                jit->Regs()[0] = object_handle;
                return;
            }
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && swi == kSvcJniDeleteGlobalRef) {
            const std::uint32_t handle = jit->Regs()[1];
            const bool was_motion = jni_motion_events.erase(handle) != 0;
#if defined(__ANDROID__)
            if (was_motion) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7INPUT DeleteGlobalRef motion handle=0x%08x refs=%zu",
                                    handle, jni_motion_events.size());
            }
#endif
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetObjectClass) {
            jni_get_object_class_seen = true;
            const std::uint32_t object_handle = jit->Regs()[1];
            if (pv7_guest_file_objects_.find(object_handle)!=pv7_guest_file_objects_.end()) {
                jit->Regs()[0]=EnsureJniClassHandle("java/io/File");
                return;
            }
            if (MotionEventForHandle(object_handle)) {
                const auto motion_class = jni_class_handles.find("android/view/MotionEvent");
                jit->Regs()[0] = motion_class != jni_class_handles.end()
                    ? motion_class->second : kJniMotionEventClassHandle;
            } else {
                jit->Regs()[0] = kJniFakeClassHandle;
            }
            return;
        }
        if (jit && swi == kSvcJniGetFieldId) {
            jni_static_field_name = ReadCString(jit->Regs()[2]);
            jni_static_field_signature = ReadCString(jit->Regs()[3]);
            jit->Regs()[0] = InternJniField(jni_static_field_name);
            return;
        }
        if (jit && swi == kSvcJniGetObjectField) {
            const auto field = jni_field_names.find(jit->Regs()[2]);
            if (jit->Regs()[1] == kPackageInfoHandle && field != jni_field_names.end() &&
                field->second == "versionName") {
                if (const char* version = pv7::compat::GuestVersionName()) {
                    jit->Regs()[0] = NewJniString(version);
                    return;
                }
            }
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && swi == kSvcJniGetPrimitiveField) {
            const auto field = jni_field_names.find(jit->Regs()[2]);
            if (field != jni_field_names.end()) {
                if (field->second == "widthPixels") { jit->Regs()[0] = guest_display_width; return; }
                if (field->second == "heightPixels") { jit->Regs()[0] = guest_display_height; return; }
                if (field->second == "densityDpi") { jit->Regs()[0] = 160u; return; }
                if (field->second == "density" || field->second == "scaledDensity" ||
                    field->second == "xdpi" || field->second == "ydpi") {
                    const float value = (field->second == "xdpi" || field->second == "ydpi") ? 160.0f : 1.0f;
                    std::memcpy(&jit->Regs()[0], &value, sizeof(value)); return;
                }
            }
            jit->Regs()[0] = 0;
            jit->Regs()[1] = 0;
            return;
        }
        if (jit && swi == kSvcJniSetField) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniExceptionCheck) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniExceptionOccurred) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniExceptionDescribe) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniExceptionClear) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniPushLocalFrame) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniPopLocalFrame) {
            jit->Regs()[0] = jit->Regs()[1];
            return;
        }
        if (jit && swi == kSvcJniGetStaticMethodId) {
            jni_method_name = ReadCString(jit->Regs()[2]);
            jni_method_signature = ReadCString(jit->Regs()[3]);
            jit->Regs()[0] = InternJniMethod(JniClassNameFor(jit->Regs()[1]), jni_method_name, jni_method_signature);
            return;
        }
        if (jit && (swi == kSvcJniCallStaticObjectMethod ||
                    swi == kSvcJniUnknownBase + 115u || swi == kSvcJniUnknownBase + 116u)) {
            const auto method = jni_method_names.find(jit->Regs()[2]);
            const std::string name = method != jni_method_names.end() ? method->second : jni_method_name;
            const auto owner = jni_method_classes.find(jit->Regs()[2]);
            std::uint32_t pv7_static_return=0;
            if (Pv7GuestStorageStaticMethod(name,JniMethodSignatureFor(jit->Regs()[2]),
                                            pv7_static_return)) {
                jit->Regs()[0]=pv7_static_return; return;
            }
            if (owner != jni_method_classes.end() && owner->second == "java/lang/Class" && name == "forName") {
                std::string class_name = JniStringValue(JniArg32(jit, swi, 0));
                std::replace(class_name.begin(), class_name.end(), '.', '/');
                jit->Regs()[0] = class_name.empty() ? 0u : GetOrCreateJniClassHandle(class_name);
                return;
            }
            if (owner != jni_method_classes.end() &&
                owner->second == "com/unity3d/player/ReflectionHelper" &&
                (name == "getMethodID" || name == "getFieldID" || name == "getConstructorID")) {
                const auto clazz = JniArg32(jit, swi, 0);
                const bool constructor = name == "getConstructorID";
                const std::string member = constructor ? "<init>" : JniStringValue(JniArg32(jit, swi, 1));
                const std::string signature = JniStringValue(JniArg32(jit, swi, constructor ? 1 : 2));
                const auto reflected = next_jni_reflected_handle;
                next_jni_reflected_handle += 4u;
                if (name == "getMethodID" || constructor)
                    jni_reflected_methods[reflected] = InternJniMethod(JniClassNameFor(clazz), member, signature);
                else jni_reflected_fields[reflected] = InternJniField(member);
                jit->Regs()[0] = reflected;
                return;
            }
            jni_fake_object_string.clear();
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && swi == kSvcJniCallStaticPrimitiveMethod) {
            std::string method = jni_method_name;
            const auto found_method = jni_method_names.find(jit->Regs()[2]);
            if (found_method != jni_method_names.end()) method = found_method->second;
#if defined(__ANDROID__)
            if (active_touch_event) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7INPUT JNI primitive method=%s obj=0x%08x mid=0x%08x arg=0x%08x",
                                    method.c_str(), jit->Regs()[1], jit->Regs()[2], jit->Regs()[3]);
            }
#endif
            const HostTouchEvent* motion = MotionEventForHandle(jit->Regs()[1]);
            if (motion) {
#if defined(__ANDROID__)
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7INPUT JNI primitive resolved method=%s obj=0x%08x identity=%llu",
                                    method.c_str(), jit->Regs()[1],
                                    static_cast<unsigned long long>(MotionEventIdentityForHandle(jit->Regs()[1])));
#endif
                std::uint32_t pointer_index = jit->Regs()[3];
                if (pointer_index >= motion->points.size() && Fits(pointer_index, 4u)) {
                    std::uint32_t indirect_index = 0;
                    if (Read32(memory_, pointer_index, indirect_index)) pointer_index = indirect_index;
                }
                const HostTouchPoint* point = pointer_index < motion->points.size()
                    ? &motion->points[pointer_index] : nullptr;
                auto return_float = [&](float value) {
                    std::uint32_t bits = 0;
                    static_assert(sizeof(bits) == sizeof(value));
                    std::memcpy(&bits, &value, sizeof(bits));
                    jit->Regs()[0] = bits;
                    jit->Regs()[1] = 0;
                };
                if (method == "getX") {
                    return_float(point ? point->x : 0.0f);
                    return;
                }
                if (method == "getY") {
                    return_float(point ? point->y : 0.0f);
                    return;
                }
                if (method == "getPressure") { return_float(point ? point->pressure : 0.0f); return; }
                if (method == "getSize") { return_float(point ? point->size : 0.0f); return; }
                if (method == "getTouchMajor" || method == "getTouchMinor" ||
                    method == "getToolMajor" || method == "getToolMinor") {
                    return_float(point ? std::max(point->size, 1.0f) : 0.0f); return;
                }
                if (method == "getXPrecision" || method == "getYPrecision") { return_float(1.0f); return; }
                if (method == "getOrientation" || method.rfind("getHistorical", 0) == 0) {
                    return_float(0.0f); return;
                }
                if (method == "getDownTime" || method == "getEventTime") {
                    const std::int64_t nanos = method == "getDownTime"
                        ? motion->downTimeNanos : motion->eventTimeNanos;
                    const std::uint64_t millis = static_cast<std::uint64_t>(std::max<std::int64_t>(0, nanos / 1000000));
                    jit->Regs()[0] = static_cast<std::uint32_t>(millis);
                    jit->Regs()[1] = static_cast<std::uint32_t>(millis >> 32u);
                    return;
                }
            }
            jit->Regs()[0] = 0;
            jit->Regs()[1] = 0;
            return;
        }
        if (jit && swi == kSvcJniCallStaticVoidMethod) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetStaticFieldId) {
            jni_static_field_name = ReadCString(jit->Regs()[2]);
            jni_static_field_signature = ReadCString(jit->Regs()[3]);
            jit->Regs()[0] = kJniFakeFieldId;
            return;
        }
        if (jit && swi == kSvcJniGetStaticObjectField) {
            jni_get_static_object_field_seen = true;
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && swi == kSvcJniGetStaticIntField) {
            jni_get_static_int_field_seen = true;
            jni_static_int_field_value = (jni_static_field_name == "FULL_WAKE_LOCK") ? 0x1au : 0u;
            jit->Regs()[0] = jni_static_int_field_value;
            return;
        }
        if (jit && swi == kSvcJniNewStringUtf) {
            jni_new_string_utf = ReadCString(jit->Regs()[1]);
            jit->Regs()[0] = NewJniString(jni_new_string_utf);
            return;
        }
        if (jit && swi == kSvcJniGetStringUtfLength) {
            const std::string value = JniStringValue(jit->Regs()[1]);
            jit->Regs()[0] = static_cast<std::uint32_t>(value.size());
            return;
        }
        if (jit && swi == kSvcJniGetStringUtfChars) {
            const std::string value = JniStringValue(jit->Regs()[1]);
            const std::uint32_t ptr = Allocate(value.size() + 1u, 1u);
            if (!ptr || !WriteCString(ptr, value)) {
                jit->Regs()[0] = 0;
                return;
            }
            if (jit->Regs()[2] && Fits(jit->Regs()[2], 1u)) Write(jit->Regs()[2], static_cast<std::uint8_t>(1));
            jit->Regs()[0] = ptr;
            return;
        }
        if (jit && swi == kSvcJniReleaseStringUtfChars) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetMethodId) {
            jni_method_name = ReadCString(jit->Regs()[2]);
            jni_method_signature = ReadCString(jit->Regs()[3]);
            if (jit->Regs()[1] == 0x0dffff00u && jni_method_name == "getKeyCode") {
                jit->Regs()[0] = 0;
                return;
            }
#if defined(__ANDROID__)
            if (active_touch_event) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7INPUT JNI GetMethodID name=%s sig=%s",
                                    jni_method_name.c_str(), jni_method_signature.c_str());
            }
#endif
            const std::string class_name = JniClassNameFor(jit->Regs()[1]);
            const std::string key = class_name + "\n" + jni_method_name + "\n" + jni_method_signature;
            auto found_method = jni_method_ids.find(key);
            if (found_method == jni_method_ids.end()) {
                const std::uint32_t id = next_jni_method_id;
                next_jni_method_id += 4u;
                jni_method_ids.emplace(key, id);
                jni_method_names.emplace(id, jni_method_name);
                jni_method_signatures.emplace(id, jni_method_signature);
                jni_method_classes.emplace(id, class_name);
                jit->Regs()[0] = id;
            } else {
                jit->Regs()[0] = found_method->second;
                if (!class_name.empty()) jni_method_classes[found_method->second] = class_name;
            }
            return;
        }
        if (jit && (swi == kSvcJniCallObjectMethod || swi == kSvcJniCallObjectMethodV || swi == kSvcJniCallObjectMethodA)) {
            ++jni_call_object_method_count;
            const std::uint32_t object = jit->Regs()[1];
            const std::uint32_t method_id = jit->Regs()[2];
            const std::string method = JniMethodNameFor(method_id);

            std::uint32_t pv7_file_return=0;
            if (Pv7GuestStorageObjectMethod(method, JniMethodSignatureFor(method_id),
                object, pv7_file_return)) { jit->Regs()[0]=pv7_file_return; return; }
            if (method == "getPackageManager") { jit->Regs()[0] = kPackageManagerHandle; return; }
            if (method == "getPackageInfo" && object == kPackageManagerHandle) {
                jit->Regs()[0] = kPackageInfoHandle; return;
            }
            if (method == "getLaunchIntentForPackage" && object == kPackageManagerHandle &&
                pv7::compat::SupportsHostRestart()) {
                const char* package = pv7::compat::GuestPackageName();
                const auto requested = JniStringValue(JniArg32(jit, swi, 0));
                jit->Regs()[0] = package && requested == package ? kRestartIntentHandle : 0u; return;
            }
            if (method == "addFlags" && object == kRestartIntentHandle) {
                jit->Regs()[0] = object; return;
            }
            if (method == "getPackageName") {
                if (const char* compat_package = pv7::compat::GuestPackageName()) {
                    jit->Regs()[0] = NewJniString(compat_package);
                    return;
                }
            }
            if (method == "getSharedPreferences") {
                LoadGuestPrefs();
                jit->Regs()[0] = kJniSharedPreferencesHandle;
                return;
            }
            if (object == kJniSharedPreferencesHandle && method == "edit") {
                LoadGuestPrefs();
                jit->Regs()[0] = kJniSharedPreferencesEditorHandle;
                return;
            }
            if (object == kJniSharedPreferencesHandle && method == "getString") {
                LoadGuestPrefs();
                const std::string key = JniStringValue(JniArg32(jit, swi, 0));
                const std::string fallback = JniStringValue(JniArg32(jit, swi, 1));
                const auto found = guest_prefs_.find(key);
                const std::string value = found != guest_prefs_.end() &&
                                          found->second.type == GuestPrefValue::Type::String
                    ? found->second.string_value : fallback;
                jit->Regs()[0] = NewJniString(value);
                return;
            }
            if (object == kJniSharedPreferencesEditorHandle && method == "putString") {
                LoadGuestPrefs();
                GuestPrefValue value;
                value.type = GuestPrefValue::Type::String;
                const std::string key = JniStringValue(JniArg32(jit, swi, 0));
                value.string_value = JniStringValue(JniArg32(jit, swi, 1));
                guest_prefs_[key] = std::move(value);
                jit->Regs()[0] = kJniSharedPreferencesEditorHandle;
                return;
            }
            if (object == kJniSharedPreferencesEditorHandle && method == "putInt") {
                LoadGuestPrefs();
                GuestPrefValue value;
                value.type = GuestPrefValue::Type::Int;
                const std::string key = JniStringValue(JniArg32(jit, swi, 0));
                value.int_value = static_cast<std::int32_t>(JniArg32(jit, swi, 1));
                guest_prefs_[key] = value;
                jit->Regs()[0] = kJniSharedPreferencesEditorHandle;
                return;
            }
            if (object == kJniSharedPreferencesEditorHandle && method == "putFloat") {
                LoadGuestPrefs();
                const auto key=JniStringValue(JniArg32(jit,swi,0));
                GuestPrefValue value;value.type=GuestPrefValue::Type::Float;
                if(swi==kSvcJniCallObjectMethodA) {
                    const auto bits=JniArg32(jit,swi,1);std::memcpy(&value.float_value,&bits,4);
                } else {
                    const auto address=swi==kSvcJniCallObjectMethodV?
                        (jit->Regs()[3]+11u)&~7u:jit->Regs()[13];
                    value.float_value=static_cast<float>(Read<double>(address));
                }
                guest_prefs_[key]=value;jit->Regs()[0]=kJniSharedPreferencesEditorHandle;return;
            }
            if (object == kJniSharedPreferencesEditorHandle && method == "remove") {
                LoadGuestPrefs();
                guest_prefs_.erase(JniStringValue(JniArg32(jit, swi, 0)));
                jit->Regs()[0] = kJniSharedPreferencesEditorHandle;
                return;
            }

            jni_fake_object_string.clear();
            jit->Regs()[0] = kJniFakeObjectHandle;
            return;
        }
        if (jit && (swi == kSvcJniCallIntMethod || swi == kSvcJniCallIntMethodV || swi == kSvcJniCallIntMethodA)) {
            ++jni_call_int_method_count;
            std::string method = JniMethodNameFor(jit->Regs()[2]);
            if (jit->Regs()[1] == kJniSharedPreferencesHandle && method == "getInt") {
                LoadGuestPrefs();
                const std::string key = JniStringValue(JniArg32(jit, swi, 0));
                const std::int32_t fallback = static_cast<std::int32_t>(JniArg32(jit, swi, 1));
                const auto found = guest_prefs_.find(key);
                jit->Regs()[0] = static_cast<std::uint32_t>(
                    found != guest_prefs_.end() && found->second.type == GuestPrefValue::Type::Int
                        ? found->second.int_value : fallback);
                return;
            }
#if defined(__ANDROID__)
            if (active_touch_event) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7INPUT JNI int method=%s obj=0x%08x mid=0x%08x arg=0x%08x",
                                    method.c_str(), jit->Regs()[1], jit->Regs()[2], jit->Regs()[3]);
            }
#endif
            const HostTouchEvent* motion = MotionEventForHandle(jit->Regs()[1]);
            if (motion) {
#if defined(__ANDROID__)
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7INPUT JNI int resolved method=%s obj=0x%08x identity=%llu",
                                    method.c_str(), jit->Regs()[1],
                                    static_cast<unsigned long long>(MotionEventIdentityForHandle(jit->Regs()[1])));
#endif
                if (method == "getAction") jit->Regs()[0] = static_cast<std::uint32_t>(motion->action);
                else if (method == "getMetaState") jit->Regs()[0] = static_cast<std::uint32_t>(motion->metaState);
                else if (method == "getButtonState") jit->Regs()[0] = static_cast<std::uint32_t>(motion->buttonState);
                else if (method == "getSource") jit->Regs()[0] = static_cast<std::uint32_t>(motion->source);
                else if (method == "getDeviceId") jit->Regs()[0] = static_cast<std::uint32_t>(motion->deviceId);
                else if (method == "getPointerCount") jit->Regs()[0] = static_cast<std::uint32_t>(motion->points.size());
                else if (method == "getPointerId") {
                    std::uint32_t index = jit->Regs()[3];
                    if (swi != kSvcJniCallIntMethod && Fits(index, 4u)) {
                        std::uint32_t indirect_index = 0;
                        if (Read32(memory_, index, indirect_index)) index = indirect_index;
                    }
                    jit->Regs()[0] = index < motion->points.size()
                        ? static_cast<std::uint32_t>(motion->points[index].id) : 0xffffffffu;
                } else if (method == "getToolType") jit->Regs()[0] = 1u;
                else if (method == "getHistorySize") jit->Regs()[0] = 0u;
                else jit->Regs()[0] = 0u;
            } else {
                if (method == "getWidth" || method == "getRawWidth")
                    jit->Regs()[0] = guest_display_width;
                else if (method == "getHeight" || method == "getRawHeight")
                    jit->Regs()[0] = guest_display_height;
                else if (method == "getDisplayId" || method == "getRotation")
                    jit->Regs()[0] = 0u;
                else
                    jit->Regs()[0] = 0u;
#if defined(__ANDROID__)
                if (active_touch_event && (method == "getWidth" || method == "getRawWidth" ||
                                           method == "getHeight" || method == "getRawHeight")) {
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                        "PV7INPUT display method=%s value=%u",
                                        method.c_str(), jit->Regs()[0]);
                }
#endif
            }
            return;
        }
        if (jit && (swi == kSvcJniCallBooleanMethod || swi == kSvcJniCallBooleanMethodV || swi == kSvcJniCallBooleanMethodA)) {
            ++jni_call_boolean_method_count;
            const std::string method = JniMethodNameFor(jit->Regs()[2]);
            std::uint32_t pv7_boolean_return=0;
            if (Pv7GuestStorageFileBoolean(method,jit->Regs()[1],pv7_boolean_return)) {
                jit->Regs()[0]=pv7_boolean_return; return;
            }
            if (jit->Regs()[1] == kJniSharedPreferencesHandle && method == "contains") {
                LoadGuestPrefs();
                const std::string key = JniStringValue(JniArg32(jit, swi, 0));
                jit->Regs()[0] = guest_prefs_.find(key) != guest_prefs_.end() ? 1u : 0u;
                return;
            }
            if (jit->Regs()[1] == kJniSharedPreferencesEditorHandle && method == "commit") {
                SaveGuestPrefs();
                jit->Regs()[0] = 1u;
                return;
            }
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && (swi == kSvcJniCallVoidMethod || swi == kSvcJniCallVoidMethodV || swi == kSvcJniCallVoidMethodA)) {
            const std::string method = JniMethodNameFor(jit->Regs()[2]);
            if(method=="showSoftInput" || method=="hideSoftInput") {
#if defined(__ANDROID__)
                auto h=static_cast<JNIEnv*>(host_jni_env_);
                if(h) {
                    auto c=h->FindClass("me/jacqueb/cnr64poc/HostKeyboard");
                    if(!c && h->ExceptionCheck())h->ExceptionClear();
                    if(!c)c=h->FindClass("app/projectv7/wrapper/HostKeyboard");
                    if(c) {
                        if(method=="showSoftInput") {
                            auto m=h->GetStaticMethodID(c,"show","(Ljava/lang/String;Ljava/lang/String;IZZ)V");
                            auto text=h->NewStringUTF(JniStringValue(JniArg32(jit,swi,0)).c_str());
                            auto hint=h->NewStringUTF(JniStringValue(JniArg32(jit,swi,6)).c_str());
                            if(m)h->CallStaticVoidMethod(c,m,text,hint,static_cast<jint>(JniArg32(jit,swi,1)),
                                static_cast<jboolean>(JniArg32(jit,swi,3)!=0),static_cast<jboolean>(JniArg32(jit,swi,4)!=0));
                            h->DeleteLocalRef(text);h->DeleteLocalRef(hint);
                        } else {
                            auto m=h->GetStaticMethodID(c,"hide","()V");if(m)h->CallStaticVoidMethod(c,m);
                        }
                        h->DeleteLocalRef(c);
                    }
                    if(h->ExceptionCheck()){h->ExceptionDescribe();h->ExceptionClear();}
                }
#endif
                jit->Regs()[0]=0;return;
            }
            if (method == "startActivity" && JniArg32(jit, swi, 0) == kRestartIntentHandle &&
                pv7::compat::SupportsHostRestart()) {
                SaveGuestPrefs();
                g_guest_restart_requested.store(true);
                SetGuestGameLoopRunning(false);
#if defined(__ANDROID__)
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC", "PV7CNR restart requested through module");
#endif
            }
            if (jit->Regs()[1] == kJniSharedPreferencesEditorHandle && method == "apply") {
                SaveGuestPrefs();
            }
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniIsInstanceOf) {
            const std::uint32_t object = jit->Regs()[1];
            const std::uint32_t clazz = jit->Regs()[2];
            const bool is_motion = MotionEventForHandle(object) != nullptr;
            bool result = object != 0 && clazz != 0;
            std::string class_name;
            const auto named_class = jni_class_names.find(clazz);
            if (named_class != jni_class_names.end()) class_name = named_class->second;
            if (clazz == kJniMotionEventClassHandle || class_name == "android/view/MotionEvent") result = is_motion;
            else if (clazz == kJniKeyEventClassHandle || class_name == "android/view/KeyEvent") result = false;
            else if (clazz == kJniInputEventClassHandle || class_name == "android/view/InputEvent") result = is_motion;
            jit->Regs()[0] = result ? 1u : 0u;
            return;
        }
        if (jit && swi == kSvcJniDeleteLocalRef) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniNewLocalRef) {
            jit->Regs()[0] = jit->Regs()[1];
            return;
        }
        if (jit && swi == kSvcJniIsSameObject) {
            const std::uint32_t lhs = jit->Regs()[1];
            const std::uint32_t rhs = jit->Regs()[2];
            const std::uint64_t lhs_identity = MotionEventIdentityForHandle(lhs);
            const std::uint64_t rhs_identity = MotionEventIdentityForHandle(rhs);
            jit->Regs()[0] = (lhs == rhs || (lhs_identity != 0 && lhs_identity == rhs_identity)) ? 1u : 0u;
            return;
        }
        if (jit && (swi == kSvcJniNewBooleanArray || swi == kSvcJniNewByteArray ||
                    swi == kSvcJniNewCharArray || swi == kSvcJniNewShortArray ||
                    swi == kSvcJniNewIntArray || swi == kSvcJniNewLongArray ||
                    swi == kSvcJniNewFloatArray || swi == kSvcJniNewDoubleArray)) {
            std::uint32_t element_size = 1;
            if (swi == kSvcJniNewCharArray || swi == kSvcJniNewShortArray) element_size = 2;
            else if (swi == kSvcJniNewIntArray || swi == kSvcJniNewFloatArray) element_size = 4;
            else if (swi == kSvcJniNewLongArray || swi == kSvcJniNewDoubleArray) element_size = 8;
            const std::uint32_t length = jit->Regs()[1];
            const std::uint64_t byte_count = static_cast<std::uint64_t>(length) * element_size;
            if (byte_count > SIZE_MAX - 16u) {
                jit->Regs()[0] = 0;
                return;
            }
            const std::uint32_t array = Allocate(static_cast<std::size_t>(byte_count) + 16u, 8u);
            if (array != 0) jni_arrays_[array] = {length, element_size, false};
            jit->Regs()[0] = array;
            return;
        }
        if (jit && swi == kSvcJniNewObjectArray) {
            const std::uint32_t length = jit->Regs()[1];
            const std::uint64_t byte_count = static_cast<std::uint64_t>(length) * 4u;
            if (byte_count > SIZE_MAX - 16u) {
                jit->Regs()[0] = 0;
                return;
            }
            const std::uint32_t array = Allocate(static_cast<std::size_t>(byte_count) + 16u, 4u);
            if (array != 0) {
                jni_arrays_[array] = {length, 4u, true};
                for (std::uint32_t index = 0; index < length; ++index)
                    Write32(memory_, array + 16u + index * 4u, jit->Regs()[3]);
            }
            jit->Regs()[0] = array;
            return;
        }
        if (jit && swi == kSvcJniGetArrayLength) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            jit->Regs()[0] = found == jni_arrays_.end() ? 0u : found->second.length;
            return;
        }
        if (jit && swi == kSvcJniGetObjectArrayElement) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            std::uint32_t value = 0;
            if (found != jni_arrays_.end() && found->second.object_array &&
                jit->Regs()[2] < found->second.length) {
                Read32(memory_, jit->Regs()[1] + 16u + jit->Regs()[2] * 4u, value);
            }
            jit->Regs()[0] = value;
            return;
        }
        if (jit && swi == kSvcJniSetObjectArrayElement) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            if (found != jni_arrays_.end() && found->second.object_array &&
                jit->Regs()[2] < found->second.length) {
                Write32(memory_, jit->Regs()[1] + 16u + jit->Regs()[2] * 4u, jit->Regs()[3]);
            }
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniGetPrimitiveArrayElements) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            if (found == jni_arrays_.end() || found->second.object_array) {
                jit->Regs()[0] = 0;
                return;
            }
            if (jit->Regs()[2] && Fits(jit->Regs()[2], 1u))
                Write(jit->Regs()[2], static_cast<std::uint8_t>(0));
            jit->Regs()[0] = jit->Regs()[1] + 16u;
            return;
        }
        if (jit && swi == kSvcJniReleasePrimitiveArrayElements) {
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && (swi == kSvcJniGetPrimitiveArrayRegion || swi == kSvcJniSetPrimitiveArrayRegion)) {
            const auto found = jni_arrays_.find(jit->Regs()[1]);
            const std::uint32_t start = jit->Regs()[2];
            const std::uint32_t length = jit->Regs()[3];
            std::uint32_t guest_buffer = 0;
            Read32(memory_, jit->Regs()[13], guest_buffer);
            if (found != jni_arrays_.end() && !found->second.object_array &&
                start <= found->second.length && length <= found->second.length - start) {
                const std::size_t byte_count = static_cast<std::size_t>(length) * found->second.element_size;
                const std::uint32_t array_data = jit->Regs()[1] + 16u + start * found->second.element_size;
                if (guest_buffer != 0 && Fits(guest_buffer, byte_count) && Fits(array_data, byte_count)) {
                    if (swi == kSvcJniGetPrimitiveArrayRegion)
                        std::memcpy(memory_.data() + guest_buffer, memory_.data() + array_data, byte_count);
                    else
                        std::memcpy(memory_.data() + array_data, memory_.data() + guest_buffer, byte_count);
                }
            }
            jit->Regs()[0] = 0;
            return;
        }
        if(jit && swi>=kSvcJniUnknownBase+55u && swi<=kSvcJniUnknownBase+57u) {
            const auto slot=swi-kSvcJniUnknownBase;
            if(jit->Regs()[1]!=kJniSharedPreferencesHandle || JniMethodNameFor(jit->Regs()[2])!="getFloat") {
                CallSVC(kSvcJniCallStaticPrimitiveMethod);return;
            }
            float value=0;
            if(jit->Regs()[1]==kJniSharedPreferencesHandle && JniMethodNameFor(jit->Regs()[2])=="getFloat") {
                LoadGuestPrefs();std::uint32_t keyHandle=0;
                if(slot==57u) {
                    keyHandle=Read<std::uint32_t>(jit->Regs()[3]);
                    const auto bits=Read<std::uint32_t>(jit->Regs()[3]+8u);std::memcpy(&value,&bits,4);
                } else {
                    keyHandle=slot==56u?Read<std::uint32_t>(jit->Regs()[3]):jit->Regs()[3];
                    const auto address=slot==56u?(jit->Regs()[3]+11u)&~7u:jit->Regs()[13];
                    value=static_cast<float>(Read<double>(address));
                }
                const auto found=guest_prefs_.find(JniStringValue(keyHandle));
                if(found!=guest_prefs_.end() && found->second.type==GuestPrefValue::Type::Float)value=found->second.float_value;
            }
            std::memcpy(&jit->Regs()[0],&value,4);return;
        }
        if (jit && swi >= kSvcJniUnknownBase && swi < kSvcJniUnknownBase + kJniEnvFunctionCount) {
            const auto compatibility_slot = swi - kSvcJniUnknownBase;
            jni_unknown_lr = jit->Regs()[14];
            Read32(memory_, jit->Regs()[13] + 20u, jni_unknown_caller_lr);
            jni_unknown_r0 = jit->Regs()[0];
            jni_unknown_r1 = jit->Regs()[1];
            jni_unknown_r2 = jit->Regs()[2];
            jni_unknown_r3 = jit->Regs()[3];

            // JNI's first metadata/reflection entries have stable table indices across
            // Android releases. Handle the whole low-index compatibility cluster here
            // instead of discovering each one through a hotpatch stop. This mirrors the
            // ABI-boundary approach used elsewhere in the guest runtime.
            switch (compatibility_slot) {
                case 4:  // GetVersion
                    jit->Regs()[0] = 0x00010006u; // JNI_VERSION_1_6
                    return;
                case 5:  // DefineClass
                    jit->Regs()[0] = kJniFakeClassHandle;
                    return;
                case 7:  // FromReflectedMethod
                    { const auto found = jni_reflected_methods.find(jit->Regs()[1]);
                      jit->Regs()[0] = found == jni_reflected_methods.end() ? kJniFakeMethodId : found->second; }
                    return;
                case 8:  // FromReflectedField
                    { const auto found = jni_reflected_fields.find(jit->Regs()[1]);
                      jit->Regs()[0] = found == jni_reflected_fields.end() ? kJniFakeFieldId : found->second; }
                    return;
                case 9:  // ToReflectedMethod
                case 12: // ToReflectedField
                    jit->Regs()[0] = kJniFakeObjectHandle;
                    return;
                case 10: // GetSuperclass
                    jit->Regs()[0] = jit->Regs()[1] ? kJniFakeClassHandle : 0u;
                    return;
                case 11: // IsAssignableFrom
                    jit->Regs()[0] = (jit->Regs()[1] != 0u && jit->Regs()[2] != 0u) ? 1u : 0u;
                    return;
                case 26: // EnsureLocalCapacity
                    jit->Regs()[0] = 0u;
                    return;
                case 229: { // NewDirectByteBuffer
                    const std::uint64_t capacity = static_cast<std::uint64_t>(jit->Regs()[2]) |
                                                   (static_cast<std::uint64_t>(jit->Regs()[3]) << 32u);
                    jni_direct_buffer_address = jit->Regs()[1];
                    jni_direct_buffer_capacity = capacity > UINT32_MAX
                        ? UINT32_MAX : static_cast<std::uint32_t>(capacity);
                    jit->Regs()[0] = kJniFakeDirectBufferHandle;
                    return;
                }
                case 230: // GetDirectBufferAddress
                    jit->Regs()[0] = (jit->Regs()[1] == kJniFakeDirectBufferHandle)
                        ? jni_direct_buffer_address : 0u;
                    return;
                case 231: { // GetDirectBufferCapacity (jlong)
                    const std::uint64_t capacity = (jit->Regs()[1] == kJniFakeDirectBufferHandle)
                        ? static_cast<std::uint64_t>(jni_direct_buffer_capacity) : 0u;
                    jit->Regs()[0] = static_cast<std::uint32_t>(capacity);
                    jit->Regs()[1] = static_cast<std::uint32_t>(capacity >> 32u);
                    return;
                }
                case 232: // GetObjectRefType
                    jit->Regs()[0] = jit->Regs()[1] ? 1u : 0u; // JNILocalRefType / JNIInvalidRefType
                    return;
                default:
                    break;
            }
            jni_unknown_slot = compatibility_slot;
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
            const std::string hotpatch_name = "jni.slot." + std::to_string(jni_unknown_slot);
            if (TryApplyHotpatch(hotpatch_name, swi) || WaitForHotpatch(hotpatch_name, swi)) return;
#endif
            failed = true;
            fault_pc = jit->Regs()[15];
            jit->HaltExecution();
            return;
        }
        if (jit && swi == kSvcJniFindClass) {
            jni_last_class = ReadCString(jit->Regs()[1]);
            jni_requested_classes.push_back(jni_last_class);
            auto class_it = jni_class_handles.find(jni_last_class);
            if (class_it == jni_class_handles.end()) {
                const std::uint32_t class_handle = next_jni_class_handle;
                next_jni_class_handle += 4u;
                jni_class_handles.emplace(jni_last_class, class_handle);
                jni_class_names.emplace(class_handle, jni_last_class);
                jit->Regs()[0] = class_handle;
            } else {
                jit->Regs()[0] = class_it->second;
            }
            return;
        }
        if (jit && swi == kSvcJniRegisterNatives) {
            const std::uint32_t methods = jit->Regs()[2];
            const std::uint32_t count = jit->Regs()[3];
            std::string registration_class = JniClassNameFor(jit->Regs()[1]);
            if (registration_class.empty()) registration_class = jni_last_class;
            if (count <= 256 && Fits(methods, static_cast<std::size_t>(count) * 12u)) {
                for (std::uint32_t i = 0; i < count; ++i) {
                    const std::uint32_t entry = methods + i * 12u;
                    JniNativeRegistration reg;
                    reg.class_name = registration_class;
                    reg.name = ReadCString(Read<std::uint32_t>(entry));
                    reg.signature = ReadCString(Read<std::uint32_t>(entry + 4));
                    reg.function = Read<std::uint32_t>(entry + 8);
                    if (registration_class == "com/unity3d/player/WWW") {
                        jni_www_class_handle = jit->Regs()[1];
                        if (reg.name == "doneCallback") www_done_callback_ = reg.function;
                        else if (reg.name == "errorCallback") www_error_callback_ = reg.function;
                        else if (reg.name == "headerCallback") www_header_callback_ = reg.function;
                        else if (reg.name == "progressCallback") www_progress_callback_ = reg.function;
                        else if (reg.name == "readCallback") www_read_callback_ = reg.function;
#if defined(__ANDROID__)
                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                            "PV7WWW RegisterNatives %s%s -> 0x%08x",
                                            reg.name.c_str(), reg.signature.c_str(), reg.function);
#endif
                    }
                    jni_native_registrations.push_back(std::move(reg));
                }
            }
            jit->Regs()[0] = 0;
            return;
        }
        if (jit && swi == kSvcJniFatalError) {
            jni_fatal_error_seen = true;
            failed = true;
            fault_pc = jit->Regs()[15];
            jit->HaltExecution();
            return;
        }
        const auto found = thunks_.find(swi);
        std::string name = found == thunks_.end() ? "<unknown>" : found->second;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        if (live_ctor_trace && handled_calls < 48) {
            const std::uint32_t lr = jit ? jit->Regs()[14] : 0u;
            const std::uint32_t caller_word = (lr >= 4u && Fits(lr - 4u, 4u))
                                                ? Read<std::uint32_t>(lr - 4u) : 0u;
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "CTOR thunk enter id=%u name=%s r0=0x%08x r1=0x%08x sp=0x%08x fp=0x%08x lr=0x%08x caller=0x%08x",
                                swi, name.c_str(), jit ? jit->Regs()[0] : 0u,
                                jit ? jit->Regs()[1] : 0u,
                                jit ? jit->Regs()[13] : 0u,
                                jit ? jit->Regs()[11] : 0u,
                                lr, caller_word);
        }
#endif
        const std::size_t versionSep = name.find('@');
        if (versionSep != std::string::npos) name.resize(versionSep);
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        if (call_trace.size() >= 160) call_trace.erase(call_trace.begin());
        call_trace.push_back(name);
        auto& thread_trace = guest_thread_call_traces_[current_thread_id_];
        if (thread_trace.size() >= 16) thread_trace.erase(thread_trace.begin());
        thread_trace.push_back(name);
#endif
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
        if (TryApplyHotpatch(name, swi) || Dispatch(name)) {
#else
        if (Dispatch(name)) {
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (live_ctor_trace && handled_calls < 48) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "CTOR thunk exit id=%u name=%s r0=0x%08x",
                                    swi, name.c_str(), jit ? jit->Regs()[0] : 0u);
            }
#endif
            ++handled_calls;
            return;
        }
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
        if (WaitForHotpatch(name, swi)) {
            ++handled_calls;
            return;
        }
#endif
#if defined(__ANDROID__)
        if ((name == "abort" || name == "exit" || name == "_exit") && jit) {
            const auto& regs = jit->Regs();
            const std::uint32_t lr = regs[14];
            std::string nearest_name;
            std::string nearest_owner;
            std::uint32_t nearest_address = 0u;
            for (const auto& item : exports_) {
                const std::uint32_t address = item.second.address & ~1u;
                if (address <= (lr & ~1u) && address >= nearest_address) {
                    nearest_address = address;
                    nearest_name = item.first;
                    nearest_owner = item.second.owner;
                }
            }
            std::uint32_t stack_words[8]{};
            for (std::size_t i = 0; i < 8; ++i) {
                const std::uint32_t address = regs[13] + static_cast<std::uint32_t>(i * 4u);
                if (Fits(address, 4u)) stack_words[i] = Read<std::uint32_t>(address);
            }
            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                "PV7TERM name=%s thread=%u pc=0x%08x lr=0x%08x sp=0x%08x fp=0x%08x status_r0=0x%08x r1=0x%08x r2=0x%08x r3=0x%08x nearest=%s:%s+0x%x",
                                name.c_str(), current_thread_id_, regs[15], lr, regs[13], regs[11], regs[0], regs[1], regs[2], regs[3],
                                nearest_owner.empty() ? "?" : nearest_owner.c_str(),
                                nearest_name.empty() ? "?" : nearest_name.c_str(),
                                nearest_address ? ((lr & ~1u) - nearest_address) : 0u);
            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                "PV7TERM stack=%08x,%08x,%08x,%08x,%08x,%08x,%08x,%08x",
                                stack_words[0], stack_words[1], stack_words[2], stack_words[3],
                                stack_words[4], stack_words[5], stack_words[6], stack_words[7]);
            for (const auto& text : diagnostic_formats) {
                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                    "PV7TERM guest-log %s", text.c_str());
            }
        }
#endif
        first_thunk_id = swi;
        first_thunk = name;
        if (jit) jit->HaltExecution();
    }

#if defined(__ANDROID__)
    void ShutdownHostGraphics() {
        // SurfaceView destruction can race the translated Unity render loop. Tear down
        // the EGL objects on the same worker thread that created them before Java
        // releases the ANativeWindow. Do not terminate EGL_DEFAULT_DISPLAY here,
        // because Android's UI renderer in this process may share that display.
        const EGLDisplay display = eglGetCurrentDisplay();
        const EGLSurface draw = eglGetCurrentSurface(EGL_DRAW);
        const EGLSurface read = eglGetCurrentSurface(EGL_READ);
        const EGLContext context = eglGetCurrentContext();
        if (display != EGL_NO_DISPLAY) {
            if (draw != EGL_NO_SURFACE) owned_egl_surfaces_.emplace(draw, display);
            if (read != EGL_NO_SURFACE) owned_egl_surfaces_.emplace(read, display);
            if (context != EGL_NO_CONTEXT) owned_egl_contexts_.emplace(context, display);
            eglMakeCurrent(display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
        }
        // Unity can unbind before nativePause returns. Ownership survives unbinding,
        // so the old window connection must be destroyed even with no current context.
        std::size_t surfaces_destroyed = 0, contexts_destroyed = 0;
        for (const auto& [surface, owner_display] : owned_egl_surfaces_)
            if (eglDestroySurface(owner_display, surface) == EGL_TRUE) ++surfaces_destroyed;
        for (const auto& [owned_context, owner_display] : owned_egl_contexts_)
            if (eglDestroyContext(owner_display, owned_context) == EGL_TRUE) ++contexts_destroyed;
        if (!owned_egl_surfaces_.empty() || !owned_egl_contexts_.empty())
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                "PV7EGL cleanup surfaces=%zu/%zu contexts=%zu/%zu",
                surfaces_destroyed, owned_egl_surfaces_.size(), contexts_destroyed, owned_egl_contexts_.size());
        owned_egl_surfaces_.clear();
        owned_egl_contexts_.clear();
        eglReleaseThread();
        egl_handles_.clear();
        host_native_window_ = nullptr;
    }
#else
    void ShutdownHostGraphics() { host_native_window_ = nullptr; }
#endif

    std::string DescribeStoppedCall() const {
        std::ostringstream out;
        if (!jit) return "No active guest CPU";
        const auto& r=jit->Regs();
        out << "Stopped guest pc=0x" << std::hex << r[15] << " lr=0x" << r[14]
            << " r0=0x" << r[0] << std::dec;
        const auto mutex=guest_mutexes_.find(r[0]);
        if (mutex!=guest_mutexes_.end()) {
            out << " mutex_owner=" << mutex->second.owner << " type=" << mutex->second.type
                << " recursion=" << mutex->second.recursion;
            for (const auto& thread : guest_thread_launches_) {
                if (thread.id!=mutex->second.owner) continue;
                out << " owner_state=" << static_cast<int>(thread.state)
                    << " owner_finished=" << thread.finished << " owner_pc=0x" << std::hex
                    << thread.regs[15] << " owner_lr=0x" << thread.regs[14]
                    << " owner_wait=0x" << thread.wait_object << std::dec;
            }
        }
        const auto sem=guest_semaphores_.find(r[0]);
        if (sem!=guest_semaphores_.end()) out << " semaphore_value=" << sem->second;
        for (const auto& thread : guest_thread_launches_) {
            const auto pending=guest_pending_signals_.find(thread.id);
            const auto mask=guest_signal_masks_.find(thread.id);
            const auto frames=guest_signal_frames_.find(thread.id);
            out << "\n      thread=" << thread.id << " state=" << static_cast<int>(thread.state)
                << " finished=" << thread.finished << " pc=" << std::hex << thread.regs[15]
                << " lr=" << thread.regs[14] << " wait=" << thread.wait_object
                << " mask=" << (mask==guest_signal_masks_.end()?0:mask->second) << std::dec
                << " pending=" << (pending==guest_pending_signals_.end()?0:pending->second.size())
                << " signal_frames=" << (frames==guest_signal_frames_.end()?0:frames->second.size());
        }
        for (const auto& [signal,action] : guest_signal_actions_)
            out << "\n      action signal=" << signal << " handler=" << std::hex << action.handler
                << " mask=" << action.mask << " flags=" << action.flags << std::dec;
        return out.str();
    }

    std::uint32_t AllocateGuestBuffer(std::size_t size, std::uint32_t alignment = 16u) {
        return Allocate(size, alignment);
    }

    void SetJniDirectBuffer(std::uint32_t address, std::uint32_t capacity) {
        jni_direct_buffer_address = address;
        jni_direct_buffer_capacity = capacity;
    }

    const std::uint8_t* GuestBytes(std::uint32_t address, std::size_t size) const {
        return Fits(address, size) ? memory_.data() + address : nullptr;
    }

private:
    std::string JniClassNameFor(std::uint32_t handle) const {
        const auto found = jni_class_names.find(handle);
        return found == jni_class_names.end() ? std::string{} : found->second;
    }

    std::uint32_t EnsureJniClassHandle(const std::string& class_name) {
        const auto found = jni_class_handles.find(class_name);
        if (found != jni_class_handles.end()) return found->second;
        const std::uint32_t handle = next_jni_class_handle;
        next_jni_class_handle += 4u;
        jni_class_handles.emplace(class_name, handle);
        jni_class_names.emplace(handle, class_name);
        return handle;
    }

    std::uint32_t JniConstructorArg32(std::uint32_t swi, std::size_t index) const {
        if (!jit) return 0u;
        if (swi == kSvcJniNewObjectA) {
            const std::uint32_t args = jit->Regs()[3];
            std::uint32_t value = 0;
            return args && Read32(memory_, args + static_cast<std::uint32_t>(index * 8u), value) ? value : 0u;
        }
        if (swi == kSvcJniNewObjectV) {
            const std::uint32_t args = jit->Regs()[3];
            std::uint32_t value = 0;
            return args && Read32(memory_, args + static_cast<std::uint32_t>(index * 4u), value) ? value : 0u;
        }
        if (index == 0u) return jit->Regs()[3];
        std::uint32_t value = 0;
        return Read32(memory_, jit->Regs()[13] + static_cast<std::uint32_t>((index - 1u) * 4u), value)
            ? value : 0u;
    }

    bool CopyJniByteArray(std::uint32_t handle, std::vector<std::uint8_t>& out) const {
        if (handle == 0u) return false;
        const auto found = jni_arrays_.find(handle);
        if (found == jni_arrays_.end() || found->second.object_array || found->second.element_size != 1u)
            return false;
        if (!Fits(handle + 16u, found->second.length)) return false;
        out.assign(memory_.begin() + handle + 16u,
                   memory_.begin() + handle + 16u + found->second.length);
        return true;
    }

#if defined(__ANDROID__)
    bool FetchHostHttp(std::uint32_t request_id, const std::string& url,
                       const std::vector<std::uint8_t>* post_data) {
        JNIEnv* host = static_cast<JNIEnv*>(host_jni_env_);
        if (!host || url.empty()) return false;

        jclass http_class = nullptr;
        jmethodID fetch = nullptr;
        const char* http_classes[] = {
            "app/projectv7/wrapper/HostHttp",
            "me/jacqueb/cnr64poc/HostHttp",
        };
        const char* result_signatures[] = {
            "(Ljava/lang/String;[B)Lapp/projectv7/wrapper/HostHttp$Result;",
            "(Ljava/lang/String;[B)Lme/jacqueb/cnr64poc/HostHttp$Result;",
        };
        for (std::size_t i = 0; i < 2u && !fetch; ++i) {
            http_class = host->FindClass(http_classes[i]);
            if (!http_class) {
                if (host->ExceptionCheck()) host->ExceptionClear();
                continue;
            }
            fetch = host->GetStaticMethodID(http_class, "fetch", result_signatures[i]);
            if (!fetch) {
                if (host->ExceptionCheck()) host->ExceptionClear();
                host->DeleteLocalRef(http_class);
                http_class = nullptr;
            }
        }
        if (!http_class || !fetch) return false;

        jstring host_url = host->NewStringUTF(url.c_str());
        jbyteArray host_post = nullptr;
        if (post_data) {
            host_post = host->NewByteArray(static_cast<jsize>(post_data->size()));
            if (host_post && !post_data->empty()) {
                host->SetByteArrayRegion(host_post, 0, static_cast<jsize>(post_data->size()),
                                         reinterpret_cast<const jbyte*>(post_data->data()));
            }
        }
        jobject result = host->CallStaticObjectMethod(http_class, fetch, host_url, host_post);
        if (host->ExceptionCheck()) {
            host->ExceptionDescribe();
            host->ExceptionClear();
            if (host_post) host->DeleteLocalRef(host_post);
            if (host_url) host->DeleteLocalRef(host_url);
            host->DeleteLocalRef(http_class);
            return false;
        }

        HostHttpPending pending;
        pending.request_id = request_id;
        if (result) {
            jclass result_class = host->GetObjectClass(result);
            jfieldID body_field = host->GetFieldID(result_class, "body", "[B");
            jfieldID headers_field = host->GetFieldID(result_class, "headers", "Ljava/lang/String;");
            jfieldID error_field = host->GetFieldID(result_class, "error", "Ljava/lang/String;");
            jfieldID status_field = host->GetFieldID(result_class, "status", "I");
            pending.status = status_field ? host->GetIntField(result, status_field) : -1;

            auto copy_string = [&](jfieldID field) -> std::string {
                if (!field) return {};
                auto value = static_cast<jstring>(host->GetObjectField(result, field));
                if (!value) return {};
                const char* chars = host->GetStringUTFChars(value, nullptr);
                std::string text = chars ? chars : "";
                if (chars) host->ReleaseStringUTFChars(value, chars);
                host->DeleteLocalRef(value);
                return text;
            };
            pending.headers = copy_string(headers_field);
            pending.error = copy_string(error_field);

            if (body_field) {
                auto body = static_cast<jbyteArray>(host->GetObjectField(result, body_field));
                if (body) {
                    const jsize length = host->GetArrayLength(body);
                    if (length > 0) {
                        pending.body.resize(static_cast<std::size_t>(length));
                        host->GetByteArrayRegion(body, 0, length,
                                                reinterpret_cast<jbyte*>(pending.body.data()));
                    }
                    host->DeleteLocalRef(body);
                }
            }
            host->DeleteLocalRef(result_class);
            host->DeleteLocalRef(result);
        } else {
            pending.error = "Host HTTP bridge returned no result";
        }

        if (host_post) host->DeleteLocalRef(host_post);
        if (host_url) host->DeleteLocalRef(host_url);
        host->DeleteLocalRef(http_class);
        host_http_pending_.push_back(std::move(pending));
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                            "PV7WWW host fetch id=%u url=%s queued=%zu",
                            request_id, url.c_str(), host_http_pending_.size());
        return true;
    }
#else
    bool FetchHostHttp(std::uint32_t, const std::string&, const std::vector<std::uint8_t>*) { return false; }
#endif

    bool InvokeWwwGuestCallback(std::uint32_t function, std::uint32_t arg2,
                                std::uint32_t arg3,
                                const std::vector<std::uint32_t>& stack_args,
                                std::uint32_t& result) {
        if (!jit || function == 0u) return false;
        constexpr std::uint32_t kWwwCallbackStackTop = 0x06f80000u;
        const auto saved_regs = jit->Regs();
        const auto saved_ext_regs = jit->ExtRegs();
        const std::uint32_t saved_cpsr = jit->Cpsr();
        const std::uint32_t saved_fpscr = jit->Fpscr();
        const auto saved_tls = tls_values_;
        const std::uint32_t saved_arm_tls = arm_tls_value_;
        const std::uint32_t saved_thread = current_thread_id_;

        ResetPhaseDiagnostics();
        saw_return = false;
        premature_return = false;
        failed = false;
        current_thread_id_ = 0x7ffffffeu;
        jit->ClearHalt();
        jit->Regs().fill(0);
        jit->Regs()[0] = kJniEnvObject;
        jit->Regs()[1] = jni_www_class_handle ? jni_www_class_handle : kJniFakeClassHandle;
        jit->Regs()[2] = arg2;
        jit->Regs()[3] = arg3;
        for (std::size_t i = 0; i < stack_args.size(); ++i)
            Write32(memory_, kWwwCallbackStackTop + static_cast<std::uint32_t>(i * 4u), stack_args[i]);
        jit->Regs()[13] = kWwwCallbackStackTop;
        jit->Regs()[14] = kReturnStub;
        jit->Regs()[15] = function & ~1u;
        jit->SetCpsr((function & 1u) ? 0x20u : 0u);

        for (int slice = 0; slice < 1024 && !saw_return && !failed && first_thunk.empty(); ++slice) {
            ticks_left = 1000000u;
            const auto reason = jit->Run();
            if (saw_return || failed || !first_thunk.empty()) break;
            if (pending_guest_callback) {
                pending_guest_callback = false;
                jit->ClearHalt();
                continue;
            }
            if (pending_cacheflush) {
                const std::uint32_t start = pending_cacheflush_start;
                const std::uint32_t size = pending_cacheflush_size;
                pending_cacheflush = false;
                pending_cacheflush_start = 0;
                pending_cacheflush_size = 0;
                jit->ClearHalt();
                if (size) InvalidateTranslatedCodeRange(start, size);
                continue;
            }
            if (Dynarmic::Has(reason, Dynarmic::HaltReason::CacheInvalidation)) {
                jit->ClearHalt();
                continue;
            }
            if (cooperative_yield_requested_) {
                cooperative_yield_requested_ = false;
                jit->ClearHalt();
                continue;
            }
            jit->ClearHalt();
        }
        const bool ok = saw_return && !failed && first_thunk.empty();
        result = ok ? jit->Regs()[0] : 0u;
#if defined(__ANDROID__)
        if (!ok) {
            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                "PV7WWW callback failed func=0x%08x thunk=%s pc=0x%08x",
                                function, first_thunk.empty() ? "-" : first_thunk.c_str(), jit->Regs()[15]);
        }
#endif
        jit->ClearHalt();
        jit->Regs() = saved_regs;
        jit->ExtRegs() = saved_ext_regs;
        jit->SetCpsr(saved_cpsr);
        jit->SetFpscr(saved_fpscr);
        tls_values_ = saved_tls;
        arm_tls_value_ = saved_arm_tls;
        current_thread_id_ = saved_thread;
        saw_return = false;
        failed = false;
        premature_return = false;
        first_thunk.clear();
        first_thunk_id = 0;
        thread_exit_requested_ = false;
        cooperative_yield_requested_ = false;
        return ok;
    }

#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
    bool TryApplyHotpatch(const std::string& name, std::uint32_t svc_id) {
        if (!jit) return false;

        const HotpatchRule rule = hotpatch_runtime_.LookupRule(name);
        if (rule.kind == HotpatchRule::Kind::ReturnU32) {
            Ret(rule.value);
            ++hotpatch_handled_count;
            hotpatch_last_detail = "rule:return " + name;
            return true;
        }
        if (rule.kind == HotpatchRule::Kind::Alias) {
            if (Dispatch(rule.target)) {
                ++hotpatch_handled_count;
                hotpatch_last_detail = "rule:alias " + name + " -> " + rule.target;
                return true;
            }
        }

        Cnr64HotpatchContextV1 context{};
        context.abi_version = CNR64_HOTPATCH_ABI_VERSION;
        context.symbol = name.c_str();
        context.svc_id = svc_id;
        context.thread_id = current_thread_id_;
        context.regs = jit->Regs().data();
        context.ext_regs = jit->ExtRegs().data();
        context.ext_reg_count = static_cast<std::uint32_t>(jit->ExtRegs().size());
        context.cpsr = jit->Cpsr();
        context.fpscr = jit->Fpscr();
        context.guest_memory = memory_.data();
        context.guest_memory_size = static_cast<std::uint32_t>(memory_.size());
        context.host_native_window = host_native_window_;
        context.managed_dir = managed_dir_.c_str();
        context.host_context = this;
        context.allocate_guest = [](void* opaque, std::uint32_t size, std::uint32_t alignment) -> std::uint32_t {
            auto* self = static_cast<BootstrapEnvironment*>(opaque);
            return self ? self->Allocate(size, alignment ? alignment : 1u) : 0u;
        };
        context.log_message = [](void* opaque, const char* message) {
            auto* self = static_cast<BootstrapEnvironment*>(opaque);
            if (!self || !message) return;
            if (self->file_trace.size() >= 96) self->file_trace.erase(self->file_trace.begin());
            self->file_trace.push_back(std::string("hotpatch: ") + message);
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_INFO, "CNR64HOTPATCH", "%s", message);
#endif
        };
        context.find_cond_waiter_thread = [](void* opaque, std::uint32_t cond) -> std::uint32_t {
            auto* self = static_cast<BootstrapEnvironment*>(opaque);
            if (!self) return 0u;
            for (const auto& waiter : self->guest_cond_waits_) {
                if (static_cast<std::uint32_t>(waiter.first) == cond && !waiter.second.signaled)
                    return static_cast<std::uint32_t>(waiter.first >> 32);
            }
            return 0u;
        };
        context.prefer_guest_thread = [](void* opaque, std::uint32_t thread_id) {
            auto* self = static_cast<BootstrapEnvironment*>(opaque);
            if (self) self->preferred_guest_thread_id_ = thread_id;
        };
        context.find_guest_thread_by_name = [](void* opaque, const char* name) -> std::uint32_t {
            auto* self = static_cast<BootstrapEnvironment*>(opaque);
            if (!self || !name) return 0u;
            for (const auto& item : self->guest_thread_names_) {
                if (item.second == name) return item.first;
            }
            return 0u;
        };
        context.configure_guest_thread_sampling =
            [](void* opaque, std::uint32_t thread_id, std::uint64_t ticks_per_slice,
               std::uint32_t sample_limit) {
                auto* self = static_cast<BootstrapEnvironment*>(opaque);
                if (!self || thread_id == 0u) return;
                if (ticks_per_slice == 0u || sample_limit == 0u) {
                    self->guest_thread_sampling_.erase(thread_id);
                    return;
                }
                self->guest_thread_sampling_[thread_id] = {
                    std::max<std::uint64_t>(ticks_per_slice, 1000u), sample_limit};
            };

        std::string detail;
        const int result = hotpatch_runtime_.DispatchPlugins(context, detail);
        jit->SetCpsr(context.cpsr);
        jit->SetFpscr(context.fpscr);
        if (result == CNR64_HOTPATCH_PASS) return false;

        hotpatch_last_detail = detail.empty() ? name : detail + ": " + name;
        ++hotpatch_handled_count;
        if (result == CNR64_HOTPATCH_RETRY_GUEST) {
            const auto stub = name_to_stub_.find(name);
            if (stub != name_to_stub_.end()) {
                jit->Regs()[15] = stub->second;
                pending_guest_callback = true;
                jit->HaltExecution();
            }
        } else if (result == CNR64_HOTPATCH_HALT) {
            first_thunk_id = svc_id;
            first_thunk = "hotpatch requested halt: " + name;
            jit->HaltExecution();
        }
        return true;
    }

    bool WaitForHotpatch(const std::string& name, std::uint32_t svc_id) {
        ++hotpatch_trap_count;
        const std::uint32_t wait_ms = hotpatch_runtime_.TrapWaitMilliseconds();
#if defined(__ANDROID__)
        __android_log_print(ANDROID_LOG_WARN, "CNR64HOTPATCH",
                            "TRAP symbol=%s svc=%u pc=0x%08x lr=0x%08x wait_ms=%u dir=%s",
                            name.c_str(), svc_id, jit ? jit->Regs()[15] : 0u,
                            jit ? jit->Regs()[14] : 0u, wait_ms,
                            hotpatch_runtime_.Directory().c_str());
#endif
        if (wait_ms == 0) return false;
        const auto deadline = std::chrono::steady_clock::now() + std::chrono::milliseconds(wait_ms);
        auto next_log = std::chrono::steady_clock::now() + std::chrono::seconds(5);
        while (std::chrono::steady_clock::now() < deadline) {
            if (TryApplyHotpatch(name, svc_id)) {
#if defined(__ANDROID__)
                __android_log_print(ANDROID_LOG_INFO, "CNR64HOTPATCH",
                                    "RESUMED symbol=%s detail=%s", name.c_str(), hotpatch_last_detail.c_str());
#endif
                return true;
            }
            std::this_thread::sleep_for(std::chrono::milliseconds(250));
#if defined(__ANDROID__)
            if (std::chrono::steady_clock::now() >= next_log) {
                __android_log_print(ANDROID_LOG_WARN, "CNR64HOTPATCH",
                                    "WAITING symbol=%s dir=%s", name.c_str(), hotpatch_runtime_.Directory().c_str());
                next_log = std::chrono::steady_clock::now() + std::chrono::seconds(5);
            }
#endif
        }
        return false;
    }
#endif

    static std::uint32_t AlignUp(std::uint32_t value, std::uint32_t alignment) {
        return (value + alignment - 1u) & ~(alignment - 1u);
    }

    bool Fits(std::uint32_t address, std::size_t size) const {
        return address <= memory_.size() && size <= memory_.size() - address;
    }

    std::string NormalizeGuestPath(const std::string& path) const {
        if (managed_dir_.empty()) return path;
        static const std::string mono_profile = "/mono/2.0/";
        static const std::string packaged_data_prefix = "assets/bin/Data";
        const std::string managed_suffix = "/Managed";
        const std::string data_dir = managed_dir_.size() >= managed_suffix.size() &&
                                     managed_dir_.compare(managed_dir_.size() - managed_suffix.size(),
                                                          managed_suffix.size(), managed_suffix) == 0
            ? managed_dir_.substr(0, managed_dir_.size() - managed_suffix.size())
            : managed_dir_;

        std::string compat_path;
        if (pv7::compat::NormalizeGuestPath(path, managed_dir_, compat_path)) {
            return compat_path;
        }

        if (path == packaged_data_prefix) return data_dir;
        if (path.rfind(packaged_data_prefix + "/", 0) == 0) {
            return data_dir + path.substr(packaged_data_prefix.size());
        }

        static const std::string current_managed_marker = "/files/Data/Managed";
        static const std::string legacy_managed_marker = "/files/Managed";
        std::size_t managed_pos = path.find(current_managed_marker);
        std::size_t marker_size = current_managed_marker.size();
        if (managed_pos == std::string::npos) {
            managed_pos = path.find(legacy_managed_marker);
            marker_size = legacy_managed_marker.size();
        }
        if (managed_pos != std::string::npos &&
            (path.rfind("/data/data/", 0) == 0 || path.rfind("/data/user/0/", 0) == 0)) {
            std::string suffix = path.substr(managed_pos + marker_size);
            if (suffix.rfind(mono_profile, 0) == 0) suffix = "/" + suffix.substr(mono_profile.size());
            return managed_dir_ + suffix;
        }
        const std::size_t profile_pos = path.rfind(mono_profile);
        if (profile_pos != std::string::npos) return managed_dir_ + "/" + path.substr(profile_pos + mono_profile.size());
        return path;
    }

    std::string GuestPrefsPath() const {
        const std::size_t files_marker = managed_dir_.find("/files/");
        if (files_marker == std::string::npos) return {};
        return managed_dir_.substr(0, files_marker + 6u) + "/GuestPlayerPrefs.bin";
    }

    void LoadGuestPrefs() {
        if (guest_prefs_loaded_) return;
        guest_prefs_loaded_ = true;
        guest_prefs_.clear();
        const std::string path = GuestPrefsPath();
        if (path.empty()) return;
        std::ifstream in(path, std::ios::binary);
        if (!in) return;
        auto read_u32 = [&in](std::uint32_t& value) -> bool {
            in.read(reinterpret_cast<char*>(&value), sizeof(value));
            return static_cast<bool>(in);
        };
        std::uint32_t magic = 0, version = 0, count = 0;
        if (!read_u32(magic) || !read_u32(version) || !read_u32(count) ||
            magic != 0x50563750u || version != 1u || count > 4096u) return;
        for (std::uint32_t i = 0; i < count; ++i) {
            std::uint32_t key_len = 0;
            if (!read_u32(key_len) || key_len > 4096u) break;
            std::string key(key_len, '\0');
            if (key_len) in.read(key.data(), static_cast<std::streamsize>(key_len));
            std::uint8_t type_byte = 0;
            in.read(reinterpret_cast<char*>(&type_byte), sizeof(type_byte));
            if (!in) break;
            GuestPrefValue value;
            value.type = static_cast<GuestPrefValue::Type>(type_byte);
            if (value.type == GuestPrefValue::Type::Int) {
                in.read(reinterpret_cast<char*>(&value.int_value), sizeof(value.int_value));
            } else if (value.type == GuestPrefValue::Type::Float) {
                in.read(reinterpret_cast<char*>(&value.float_value), sizeof(value.float_value));
            } else if (value.type == GuestPrefValue::Type::String) {
                std::uint32_t len = 0;
                if (!read_u32(len) || len > 1024u * 1024u) break;
                value.string_value.resize(len);
                if (len) in.read(value.string_value.data(), static_cast<std::streamsize>(len));
            } else {
                break;
            }
            if (!in) break;
            guest_prefs_[std::move(key)] = std::move(value);
        }
    }

    void SaveGuestPrefs() {
        LoadGuestPrefs();
        const std::string path = GuestPrefsPath();
        if (path.empty()) return;
        const std::string temp_path = path + ".tmp";
        std::ofstream out(temp_path, std::ios::binary | std::ios::trunc);
        if (!out) return;
        auto write_u32 = [&out](std::uint32_t value) {
            out.write(reinterpret_cast<const char*>(&value), sizeof(value));
        };
        write_u32(0x50563750u); // PV7P
        write_u32(1u);
        write_u32(static_cast<std::uint32_t>(guest_prefs_.size()));
        for (const auto& pair : guest_prefs_) {
            write_u32(static_cast<std::uint32_t>(pair.first.size()));
            if (!pair.first.empty()) out.write(pair.first.data(), static_cast<std::streamsize>(pair.first.size()));
            const std::uint8_t type_byte = static_cast<std::uint8_t>(pair.second.type);
            out.write(reinterpret_cast<const char*>(&type_byte), sizeof(type_byte));
            if (pair.second.type == GuestPrefValue::Type::Int) {
                out.write(reinterpret_cast<const char*>(&pair.second.int_value), sizeof(pair.second.int_value));
            } else if (pair.second.type == GuestPrefValue::Type::Float) {
                out.write(reinterpret_cast<const char*>(&pair.second.float_value), sizeof(pair.second.float_value));
            } else {
                write_u32(static_cast<std::uint32_t>(pair.second.string_value.size()));
                if (!pair.second.string_value.empty())
                    out.write(pair.second.string_value.data(), static_cast<std::streamsize>(pair.second.string_value.size()));
            }
        }
        out.flush();
        out.close();
        if (!out) {
            std::remove(temp_path.c_str());
            return;
        }
        std::remove(path.c_str());
        std::rename(temp_path.c_str(), path.c_str());
    }

    #include "pv7_guest_storage_jni_members.inc"

    std::string JniStringValue(std::uint32_t handle) const {
        if (handle == kJniFakeStringHandle) return jni_new_string_utf;
        if (handle == kJniFakeObjectHandle) return jni_fake_object_string;
        const auto it = jni_strings.find(handle);
        return it == jni_strings.end() ? std::string() : it->second;
    }

    std::uint32_t NewJniString(const std::string& value) {
        const std::uint32_t handle = next_jni_string_handle;
        next_jni_string_handle += 4u;
        std::string mapped;
        const bool translated=pv7::compat::NormalizeGuestPath(value,managed_dir_,mapped);
        jni_strings[handle] = translated ? mapped : value;
        return handle;
    }

#include "jni_url_stream_methods.inc"

    std::uint32_t InternJniField(const std::string& name) {
        const auto found = jni_field_ids.find(name);
        if (found != jni_field_ids.end()) return found->second;
        const auto id = next_jni_field_id; next_jni_field_id += 4u;
        jni_field_ids[name] = id; jni_field_names[id] = name; return id;
    }
    std::uint32_t InternJniMethod(const std::string& clazz, const std::string& name, const std::string& signature) {
        const std::string key = clazz + "\n" + name + "\n" + signature;
        const auto found = jni_method_ids.find(key);
        if (found != jni_method_ids.end()) return found->second;
        const auto id = next_jni_method_id; next_jni_method_id += 4u;
        jni_method_ids[key] = id; jni_method_names[id] = name;
        jni_method_signatures[id] = signature; jni_method_classes[id] = clazz;
        return id;
    }
    std::string JniMethodNameFor(std::uint32_t method_id) const {
        const auto it = jni_method_names.find(method_id);
        return it == jni_method_names.end() ? jni_method_name : it->second;
    }

    std::string JniMethodSignatureFor(std::uint32_t method_id) const {
        const auto it = jni_method_signatures.find(method_id);
        return it == jni_method_signatures.end() ? jni_method_signature : it->second;
    }

    std::uint32_t JniArg32(Dynarmic::A32::Jit* jit, std::uint32_t swi,
                           std::size_t index, bool object_call = true) const {
        if (!jit) return 0u;
        const bool is_a = swi == kSvcJniCallObjectMethodA || swi == kSvcJniCallIntMethodA ||
                          swi == kSvcJniCallBooleanMethodA || swi == kSvcJniCallVoidMethodA || swi == kSvcJniUnknownBase + 116u;
        const bool is_v = swi == kSvcJniCallObjectMethodV || swi == kSvcJniCallIntMethodV ||
                          swi == kSvcJniCallBooleanMethodV || swi == kSvcJniCallVoidMethodV || swi == kSvcJniUnknownBase + 115u;
        (void)object_call;
        if (is_a) {
            const std::uint32_t args = jit->Regs()[3];
            std::uint32_t value = 0;
            if (args && Read32(memory_, args + static_cast<std::uint32_t>(index * 8u), value)) return value;
            return 0u;
        }
        if (is_v) {
            const std::uint32_t args = jit->Regs()[3];
            std::uint32_t value = 0;
            if (args && Read32(memory_, args + static_cast<std::uint32_t>(index * 4u), value)) return value;
            return 0u;
        }
        if (index == 0u) return jit->Regs()[3];
        std::uint32_t value = 0;
        const std::uint32_t stack_address = jit->Regs()[13] + static_cast<std::uint32_t>((index - 1u) * 4u);
        if (Read32(memory_, stack_address, value)) return value;
        return 0u;
    }

    std::string ReadCString(std::uint32_t address, std::size_t max_len = 4096) const {
        if (!Fits(address, 1)) return {};
        std::string out;
        for (std::size_t i = 0; i < max_len && Fits(address + static_cast<std::uint32_t>(i), 1); ++i) {
            const char c = static_cast<char>(memory_[address + i]);
            if (c == '\0') break;
            out.push_back(c);
        }
        return out;
    }

    std::string ReadFullCString(std::uint32_t address) const {
        if (!Fits(address, 1u)) return {};
        const char* start = reinterpret_cast<const char*>(memory_.data() + address);
        const std::size_t available = memory_.size() - address;
        const char* end = static_cast<const char*>(std::memchr(start, 0, available));
        return std::string(start, end ? static_cast<std::size_t>(end - start) : available);
    }

public:
    void MapNormalizedHostTouch(HostTouchEvent& touch) const {
        const float width = guest_logical_viewport_width ? guest_logical_viewport_width : 128u;
        const float height = guest_logical_viewport_height ? guest_logical_viewport_height : 128u;
        for (auto& point : touch.points) { point.x *= width / 128.0f; point.y *= height / 128.0f; }
    }

private:
    bool WriteCString(std::uint32_t address, const std::string& value, std::size_t max_bytes = SIZE_MAX) {
        const std::size_t count = std::min(value.size(), max_bytes == 0 ? 0 : max_bytes - 1);
        if (!Fits(address, count + 1)) return false;
        if (count) std::memcpy(memory_.data() + address, value.data(), count);
        memory_[address + count] = 0;
        return true;
    }

    void AddFreeBlock(std::uint32_t address, std::size_t size) {
        if (size == 0) return;
        std::uint32_t region_end = 0u;
        if (address >= kGuestHeapStart && address < kGuestHeapEnd) {
            region_end = kGuestHeapEnd;
        }
#if defined(PROJECTV7_TEST_HEAP_OVERFLOW_32M) && PROJECTV7_TEST_HEAP_OVERFLOW_32M
        else if (address >= kGuestOverflowHeapStart && address < kGuestOverflowHeapEnd) {
            region_end = kGuestOverflowHeapEnd;
        }
#endif
#if defined(PROJECTV7_CNR_CONTENT_HEAP) && PROJECTV7_CNR_CONTENT_HEAP
        else if (address >= kGuestContentHeapStart && address < kGuestContentHeapEnd) {
            region_end = kGuestContentHeapEnd;
        }
#endif
        if (!region_end) return;
        std::uint64_t end64 = static_cast<std::uint64_t>(address) + size;
        if (end64 > region_end) end64 = region_end;
        std::uint32_t end = static_cast<std::uint32_t>(end64);

        auto next = free_blocks_.lower_bound(address);
        if (next != free_blocks_.begin()) {
            auto previous = next;
            --previous;
            const std::uint64_t previous_end =
                static_cast<std::uint64_t>(previous->first) + previous->second;
            if (previous_end >= address) {
                address = previous->first;
                end = static_cast<std::uint32_t>(std::max<std::uint64_t>(end, previous_end));
                free_blocks_.erase(previous);
            }
        }
        next = free_blocks_.lower_bound(address);
        while (next != free_blocks_.end() && next->first <= end) {
            const std::uint64_t next_end =
                static_cast<std::uint64_t>(next->first) + next->second;
            end = static_cast<std::uint32_t>(std::max<std::uint64_t>(end, next_end));
            next = free_blocks_.erase(next);
        }
        free_blocks_[address] = static_cast<std::size_t>(end - address);

        // If the highest free block reaches the bump pointer, return that whole tail
        // to the bump allocator as well. This keeps short-lived large allocations from
        // permanently ratcheting heap_next_ toward the main guest stack.
        const auto reclaim_tail = [&](std::uint32_t& bump) {
            for (;;) {
                auto tail = free_blocks_.lower_bound(bump);
                if (tail == free_blocks_.begin()) break;
                --tail;
                const std::uint64_t tail_end = static_cast<std::uint64_t>(tail->first) + tail->second;
                if (tail_end != bump) break;
                bump = tail->first;
                free_blocks_.erase(tail);
            }
        };
#if defined(PROJECTV7_CNR_CONTENT_HEAP) && PROJECTV7_CNR_CONTENT_HEAP
        reclaim_tail(heap_content_next_);
#endif
        reclaim_tail(heap_next_);
#if defined(PROJECTV7_TEST_HEAP_OVERFLOW_32M) && PROJECTV7_TEST_HEAP_OVERFLOW_32M
        reclaim_tail(heap_overflow_next_);
#endif
    }

    bool FreeAllocation(std::uint32_t address) {
        if (!address) return true;
        const auto allocation = allocations_.find(address);
        if (allocation == allocations_.end()) return false;
        const std::size_t requested = allocation->second;
        allocations_.erase(allocation);
        const std::size_t reserved = static_cast<std::size_t>(
            AlignUp(static_cast<std::uint32_t>(std::max<std::size_t>(requested, 1u)), 16u));
        AddFreeBlock(address, reserved);
        if (reserved >= 65536u) DiscardGuestPages(memory_, address, reserved);
        return true;
    }

    std::uint32_t Allocate(std::size_t size, std::uint32_t alignment = 16) {
        if (size == 0) size = 1;
        if (alignment == 0) alignment = 1;
#if defined(PROJECTV7_CNR_CONTENT_HEAP) && PROJECTV7_CNR_CONTENT_HEAP
        constexpr std::size_t max_arena_size = kGuestContentHeapEnd - kGuestContentHeapStart;
#elif defined(PROJECTV7_TEST_HEAP_OVERFLOW_32M) && PROJECTV7_TEST_HEAP_OVERFLOW_32M
        constexpr std::size_t max_arena_size =
            static_cast<std::size_t>(kGuestOverflowHeapEnd - kGuestOverflowHeapStart);
#else
        constexpr std::size_t max_arena_size =
            static_cast<std::size_t>(kGuestHeapEnd - kGuestHeapStart);
#endif
        if (size > max_arena_size) {
#if defined(__ANDROID__)
            ++guest_heap_oom_count_;
            if (guest_heap_oom_count_ <= 16u) {
                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                    "PV7HEAPOOM oversized requested=%zu heap_next=%08x heap_end=%08x free_blocks=%zu count=%zu",
                                    size, heap_next_, kGuestHeapEnd, free_blocks_.size(), guest_heap_oom_count_);
            }
#endif
            return 0;
        }
        const std::uint32_t reserved = AlignUp(static_cast<std::uint32_t>(size), 16u);

        // First fit from recycled guest blocks. Free blocks include allocation padding,
        // so an aligned subrange can safely be split into prefix and suffix fragments.
        for (auto block = free_blocks_.begin(); block != free_blocks_.end(); ++block) {
            const std::uint32_t block_start = block->first;
            const std::uint64_t block_end = static_cast<std::uint64_t>(block_start) + block->second;
            const std::uint32_t aligned = AlignUp(block_start, alignment);
            const std::uint64_t allocation_end = static_cast<std::uint64_t>(aligned) + reserved;
            if (allocation_end > block_end) continue;

            const std::size_t prefix = static_cast<std::size_t>(aligned - block_start);
            const std::size_t suffix = static_cast<std::size_t>(block_end - allocation_end);
            free_blocks_.erase(block);
            if (prefix) AddFreeBlock(block_start, prefix);
            if (suffix) AddFreeBlock(static_cast<std::uint32_t>(allocation_end), suffix);
            allocations_[aligned] = size;
            if (allocation_trace.size() < 32) allocation_trace.emplace_back(aligned, size);
            std::memset(memory_.data() + aligned, 0, size);
            return aligned;
        }

        const std::uint32_t aligned = AlignUp(heap_next_, alignment);
        const std::uint64_t end = static_cast<std::uint64_t>(aligned) + reserved;
        if (end > kGuestHeapEnd) {
#if defined(PROJECTV7_TEST_HEAP_OVERFLOW_32M) && PROJECTV7_TEST_HEAP_OVERFLOW_32M
            const std::uint32_t overflow_aligned = AlignUp(heap_overflow_next_, alignment);
            const std::uint64_t overflow_end =
                static_cast<std::uint64_t>(overflow_aligned) + reserved;
            if (overflow_end <= kGuestOverflowHeapEnd) {
                heap_overflow_next_ = static_cast<std::uint32_t>(overflow_end);
                allocations_[overflow_aligned] = size;
                if (allocation_trace.size() < 32)
                    allocation_trace.emplace_back(overflow_aligned, size);
                std::memset(memory_.data() + overflow_aligned, 0, size);
#if defined(PROJECTV7_MONO_FATAL_CONTEXT_TRACE) && PROJECTV7_MONO_FATAL_CONTEXT_TRACE && defined(__ANDROID__)
                ++guest_overflow_alloc_count_;
                if (guest_overflow_alloc_count_ <= 12u) {
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                        "PV7HEAPOVERFLOW alloc=%zu addr=%08x high_next=%08x low_next=%08x count=%zu",
                                        size, overflow_aligned, heap_overflow_next_,
                                        heap_next_, guest_overflow_alloc_count_);
                }
#endif
                return overflow_aligned;
            }
#endif
#if defined(PROJECTV7_CNR_CONTENT_HEAP) && PROJECTV7_CNR_CONTENT_HEAP
            const std::uint32_t content_aligned = AlignUp(heap_content_next_, alignment);
            const std::uint64_t content_end = static_cast<std::uint64_t>(content_aligned) + reserved;
            if (content_end <= kGuestContentHeapEnd) {
                heap_content_next_ = static_cast<std::uint32_t>(content_end);
                allocations_[content_aligned] = size;
                if (allocation_trace.size() < 32) allocation_trace.emplace_back(content_aligned, size);
                std::memset(memory_.data() + content_aligned, 0, size);
                return content_aligned;
            }
#endif
#if defined(__ANDROID__)
            ++guest_heap_oom_count_;
            if (guest_heap_oom_count_ <= 16u) {
                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                    "PV7HEAPOOM exhausted requested=%zu aligned=%08x end=%08llx heap_next=%08x heap_end=%08x free_blocks=%zu count=%zu",
                                    size, aligned, static_cast<unsigned long long>(end), heap_next_,
                                    kGuestHeapEnd, free_blocks_.size(), guest_heap_oom_count_);
            }
#endif
            return 0;
        }
        heap_next_ = static_cast<std::uint32_t>(end);
        allocations_[aligned] = size;
        if (allocation_trace.size() < 32) allocation_trace.emplace_back(aligned, size);
        std::memset(memory_.data() + aligned, 0, size);
        return aligned;
    }

    void SetErrno(std::uint32_t value) {
        Write32(memory_, kBootstrapRuntimeBase, value);
    }

    std::uint32_t Arg(unsigned index) const {
        if (!jit) return 0;
        if (index < 4) return jit->Regs()[index];
        std::uint32_t value = 0;
        const std::uint32_t sp = jit->Regs()[13];
        Read32(memory_, sp + static_cast<std::uint32_t>((index - 4) * 4), value);
        return value;
    }

    std::uint64_t Arg64(unsigned word_index) const {
        const std::uint64_t lo = Arg(word_index);
        const std::uint64_t hi = Arg(word_index + 1);
        return lo | (hi << 32);
    }

    float ArgFloat(unsigned word_index) const {
        const std::uint32_t bits = Arg(word_index);
        float value = 0.0f;
        std::memcpy(&value, &bits, sizeof(value));
        return value;
    }

    double ArgDouble(unsigned word_index) const {
        const std::uint64_t bits = Arg64(word_index);
        double value = 0.0;
        std::memcpy(&value, &bits, sizeof(value));
        return value;
    }

    void Ret(std::uint32_t value) {
        if (jit) jit->Regs()[0] = value;
    }

    void Ret64(std::uint64_t value) {
        if (!jit) return;
        jit->Regs()[0] = static_cast<std::uint32_t>(value);
        jit->Regs()[1] = static_cast<std::uint32_t>(value >> 32);
    }

    void RetFloat(float value) {
        std::uint32_t bits = 0;
        std::memcpy(&bits, &value, sizeof(bits));
        Ret(bits);
    }

    void RetDouble(double value) {
        std::uint64_t bits = 0;
        std::memcpy(&bits, &value, sizeof(bits));
        Ret64(bits);
    }

#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
    void RecordSyncEvent(const char* op, std::uint32_t object, std::uint32_t detail = 0) {
        auto& trace = guest_thread_sync_traces_[current_thread_id_];
        if (trace.size() >= 96) trace.erase(trace.begin());
        std::ostringstream line;
        line << op << "@0x" << std::hex << object;
        if (detail != 0) line << ":" << detail;
        if (jit) line << "/lr=0x" << jit->Regs()[14] << "/sp=0x" << jit->Regs()[13];
        trace.push_back(line.str());
    }
#endif

    struct VirtualFile {
        std::string data;
        std::size_t pos = 0;
    };

    std::uint32_t RegisterVirtualFile(std::string data) {
        const std::uint32_t handle = next_file_handle_++;
        virtual_files_[handle] = {std::move(data), 0};
        return handle;
    }

    std::uint32_t RegisterFileHandle(std::FILE* file, const std::string& path = {}) {
        if (!file) return 0;
        const std::uint32_t handle = Allocate(84, 4);
        if (!handle) return 0;
#if defined(_WIN32)
        const int fd = ::_fileno(file);
#else
        const int fd = ::fileno(file);
#endif
        const std::uint16_t flags = 0x0004u; // old Bionic __SRD
        const std::int16_t guest_fd = static_cast<std::int16_t>(fd);
        Write<std::uint16_t>(handle + 12, flags);
        Write<std::int16_t>(handle + 14, guest_fd);
        file_handles_[handle] = file;
        if (!path.empty()) file_paths_[handle] = path;
        return handle;
    }

    std::FILE* LookupFileHandle(std::uint32_t handle) const {
        const auto it = file_handles_.find(handle);
        return it == file_handles_.end() ? nullptr : it->second;
    }

#if !defined(_WIN32)
    bool WriteGuestStat(std::uint32_t guest, const struct stat& st) {
        if (!Fits(guest, 104)) return false;
        std::memset(memory_.data() + guest, 0, 104);
        Write32(memory_, guest + 0x10, static_cast<std::uint32_t>(st.st_mode));
        Write32(memory_, guest + 0x14, static_cast<std::uint32_t>(st.st_nlink));
        Write32(memory_, guest + 0x18, static_cast<std::uint32_t>(st.st_uid));
        Write32(memory_, guest + 0x1c, static_cast<std::uint32_t>(st.st_gid));
        const std::uint64_t size = static_cast<std::uint64_t>(st.st_size);
        std::memcpy(memory_.data() + guest + 0x30, &size, sizeof(size));
        Write32(memory_, guest + 0x38, static_cast<std::uint32_t>(st.st_blksize));
        const std::uint64_t blocks = static_cast<std::uint64_t>(st.st_blocks);
        std::memcpy(memory_.data() + guest + 0x40, &blocks, sizeof(blocks));
        Write32(memory_, guest + 0x48, static_cast<std::uint32_t>(st.st_atim.tv_sec));
        Write32(memory_, guest + 0x4c, static_cast<std::uint32_t>(st.st_atim.tv_nsec));
        Write32(memory_, guest + 0x50, static_cast<std::uint32_t>(st.st_mtim.tv_sec));
        Write32(memory_, guest + 0x54, static_cast<std::uint32_t>(st.st_mtim.tv_nsec));
        Write32(memory_, guest + 0x58, static_cast<std::uint32_t>(st.st_ctim.tv_sec));
        Write32(memory_, guest + 0x5c, static_cast<std::uint32_t>(st.st_ctim.tv_nsec));
        const std::uint64_t ino = static_cast<std::uint64_t>(st.st_ino);
        std::memcpy(memory_.data() + guest + 0x60, &ino, sizeof(ino));
        return true;
    }
#endif

#if defined(__ANDROID__)
    std::uint32_t RegisterEglHandle(std::uintptr_t host_handle, bool allow_zero = false) {
        if (host_handle == 0 && !allow_zero) return 0;
        for (const auto& entry : egl_handles_) {
            if (entry.second == host_handle) return entry.first;
        }
        const std::uint32_t guest_handle = next_egl_handle_++;
        egl_handles_[guest_handle] = host_handle;
        return guest_handle;
    }

    bool LookupEglHandleValue(std::uint32_t guest_handle, std::uintptr_t& host_handle) const {
        if (guest_handle == 0) return false;
        const auto found = egl_handles_.find(guest_handle);
        if (found == egl_handles_.end()) return false;
        host_handle = found->second;
        return true;
    }

    std::uintptr_t LookupEglHandle(std::uint32_t guest_handle) const {
        std::uintptr_t host_handle = 0;
        return LookupEglHandleValue(guest_handle, host_handle) ? host_handle : 0;
    }

    void UnregisterEglHandle(std::uint32_t guest_handle) {
        if (guest_handle != 0) egl_handles_.erase(guest_handle);
    }

    std::vector<EGLint> ReadEglAttribList(std::uint32_t guest_address) const {
        std::vector<EGLint> attributes;
        if (guest_address == 0) return attributes;
        for (std::size_t index = 0; index < 128; ++index) {
            std::uint32_t raw = 0;
            if (!Read32(memory_, guest_address + static_cast<std::uint32_t>(index * 4u), raw)) break;
            const EGLint value = static_cast<EGLint>(raw);
            attributes.push_back(value);
            if (value == EGL_NONE) break;
        }
        if (!attributes.empty() && attributes.back() != EGL_NONE) attributes.push_back(EGL_NONE);
        return attributes;
    }
#endif

#if defined(__ANDROID__)
    struct GuestZStream32 {
        std::uint32_t next_in = 0;
        std::uint32_t avail_in = 0;
        std::uint32_t total_in = 0;
        std::uint32_t next_out = 0;
        std::uint32_t avail_out = 0;
        std::uint32_t total_out = 0;
        std::uint32_t msg = 0;
        std::uint32_t state = 0;
        std::uint32_t zalloc = 0;
        std::uint32_t zfree = 0;
        std::uint32_t opaque = 0;
        std::int32_t data_type = 0;
        std::uint32_t adler = 0;
        std::uint32_t reserved = 0;
    };
    static_assert(sizeof(GuestZStream32) == 56, "ARM32 z_stream layout must remain 56 bytes");

    enum class HostZlibKind {
        Inflate,
        Deflate,
    };

    struct HostZlibStream {
        z_stream stream{};
        HostZlibKind kind = HostZlibKind::Inflate;
        bool initialized = false;
    };

    bool ReadGuestZStream(std::uint32_t address, GuestZStream32& stream) const {
        if (!Fits(address, sizeof(stream))) return false;
        std::memcpy(&stream, memory_.data() + address, sizeof(stream));
        return true;
    }

    bool WriteGuestZStream(std::uint32_t address, const GuestZStream32& stream) {
        if (!Fits(address, sizeof(stream))) return false;
        std::memcpy(memory_.data() + address, &stream, sizeof(stream));
        return true;
    }

    std::uint32_t GuestPointerFromHost(const Bytef* pointer) const {
        if (!pointer) return 0;
        const auto* begin = memory_.data();
        const auto* end = begin + memory_.size();
        if (pointer < begin || pointer > end) return 0;
        const std::size_t offset = static_cast<std::size_t>(pointer - begin);
        return offset <= UINT32_MAX ? static_cast<std::uint32_t>(offset) : 0u;
    }

    bool SyncGuestZlibBuffersToHost(std::uint32_t guest_address, HostZlibStream& state) {
        GuestZStream32 guest{};
        if (!ReadGuestZStream(guest_address, guest)) return false;

        if (guest.avail_in != 0) {
            if (guest.next_in == 0 || !Fits(guest.next_in, guest.avail_in)) return false;
            state.stream.next_in = reinterpret_cast<Bytef*>(memory_.data() + guest.next_in);
        } else if (guest.next_in != 0 && Fits(guest.next_in, 1)) {
            state.stream.next_in = reinterpret_cast<Bytef*>(memory_.data() + guest.next_in);
        } else {
            state.stream.next_in = Z_NULL;
        }
        state.stream.avail_in = static_cast<uInt>(guest.avail_in);

        if (guest.avail_out != 0) {
            if (guest.next_out == 0 || !Fits(guest.next_out, guest.avail_out)) return false;
            state.stream.next_out = reinterpret_cast<Bytef*>(memory_.data() + guest.next_out);
        } else if (guest.next_out != 0 && Fits(guest.next_out, 1)) {
            state.stream.next_out = reinterpret_cast<Bytef*>(memory_.data() + guest.next_out);
        } else {
            state.stream.next_out = Z_NULL;
        }
        state.stream.avail_out = static_cast<uInt>(guest.avail_out);
        return true;
    }

    bool SyncHostZlibStateToGuest(std::uint32_t guest_address,
                                  const HostZlibStream& state,
                                  bool expose_state) {
        GuestZStream32 guest{};
        if (!ReadGuestZStream(guest_address, guest)) return false;
        guest.next_in = GuestPointerFromHost(state.stream.next_in);
        guest.avail_in = static_cast<std::uint32_t>(state.stream.avail_in);
        guest.total_in = static_cast<std::uint32_t>(state.stream.total_in);
        guest.next_out = GuestPointerFromHost(state.stream.next_out);
        guest.avail_out = static_cast<std::uint32_t>(state.stream.avail_out);
        guest.total_out = static_cast<std::uint32_t>(state.stream.total_out);
        guest.state = expose_state ? guest_address : 0u;
        guest.data_type = static_cast<std::int32_t>(state.stream.data_type);
        guest.adler = static_cast<std::uint32_t>(state.stream.adler);
        guest.reserved = static_cast<std::uint32_t>(state.stream.reserved);
        if (state.stream.msg) {
            const std::size_t length = std::strlen(state.stream.msg);
            const std::uint32_t message = Allocate(length + 1u, 1u);
            if (message && WriteCString(message, state.stream.msg)) guest.msg = message;
        } else {
            guest.msg = 0;
        }
        return WriteGuestZStream(guest_address, guest);
    }

    void DisposeHostZlibStream(HostZlibStream& state) {
        if (!state.initialized) return;
        if (state.kind == HostZlibKind::Inflate) ::inflateEnd(&state.stream);
        else ::deflateEnd(&state.stream);
        state.initialized = false;
    }

    bool DispatchZlib(const std::string& name) {
#if PROJECTV7_FAST_SYMBOL_DISPATCH
        if (name.empty() || (name.front() != 'c' && name.front() != 'd' && name.front() != 'i')) return false;
#endif
        const Pv7DispatchNameKey& fast_name{name};
        if (fast_name == "crc32") {
            const std::uint32_t guest_buffer = Arg(1);
            const std::uint32_t length = Arg(2);
            if (length != 0 && (guest_buffer == 0 || !Fits(guest_buffer, length))) {
                Ret(0);
                return true;
            }
            const Bytef* buffer = length == 0 ? Z_NULL : reinterpret_cast<const Bytef*>(memory_.data() + guest_buffer);
            Ret(static_cast<std::uint32_t>(::crc32(static_cast<uLong>(Arg(0)), buffer, static_cast<uInt>(length))));
            return true;
        }

        if (fast_name == "inflateInit_" || fast_name == "inflateInit2_" || fast_name == "deflateInit2_") {
            const std::uint32_t guest_address = Arg(0);
            const std::uint32_t guest_stream_size = fast_name == "deflateInit2_" ? Arg(7)
                                                   : fast_name == "inflateInit2_" ? Arg(3)
                                                   : Arg(2);
            if (guest_address == 0 || !Fits(guest_address, sizeof(GuestZStream32)) ||
                guest_stream_size != sizeof(GuestZStream32)) {
                Ret(static_cast<std::uint32_t>(Z_VERSION_ERROR));
                return true;
            }

            auto existing = zlib_streams_.find(guest_address);
            if (existing != zlib_streams_.end()) {
                DisposeHostZlibStream(existing->second);
                zlib_streams_.erase(existing);
            }

            auto [inserted_it, inserted] = zlib_streams_.try_emplace(guest_address);
            (void)inserted;
            HostZlibStream& state = inserted_it->second;
            state = HostZlibStream{};
            state.stream.zalloc = Z_NULL;
            state.stream.zfree = Z_NULL;
            state.stream.opaque = Z_NULL;
            int rc = Z_STREAM_ERROR;
            if (fast_name == "inflateInit_") {
                state.kind = HostZlibKind::Inflate;
                rc = ::inflateInit(&state.stream);
            } else if (fast_name == "inflateInit2_") {
                state.kind = HostZlibKind::Inflate;
                rc = ::inflateInit2(&state.stream, static_cast<int>(Arg(1)));
            } else {
                state.kind = HostZlibKind::Deflate;
                rc = ::deflateInit2(&state.stream,
                                    static_cast<int>(Arg(1)),
                                    static_cast<int>(Arg(2)),
                                    static_cast<int>(Arg(3)),
                                    static_cast<int>(Arg(4)),
                                    static_cast<int>(Arg(5)));
            }
            state.initialized = rc == Z_OK;
            SyncHostZlibStateToGuest(guest_address, state, rc == Z_OK);
            if (file_trace.size() < 96) {
                std::ostringstream trace;
                trace << name << " guest=0x" << std::hex << guest_address << std::dec
                      << " stream_size=" << guest_stream_size << " rc=" << rc;
                file_trace.push_back(trace.str());
            }
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }

        if (fast_name == "inflate" || fast_name == "deflate" ||
            fast_name == "inflateReset" || fast_name == "deflateReset" ||
            fast_name == "inflateEnd" || fast_name == "deflateEnd") {
            const std::uint32_t guest_address = Arg(0);
            auto it = zlib_streams_.find(guest_address);
            if (it == zlib_streams_.end() || !it->second.initialized) {
                Ret(static_cast<std::uint32_t>(Z_STREAM_ERROR));
                return true;
            }

            HostZlibStream& state = it->second;
            const bool wants_inflate = name.rfind("inflate", 0) == 0;
            if ((wants_inflate && state.kind != HostZlibKind::Inflate) ||
                (!wants_inflate && state.kind != HostZlibKind::Deflate)) {
                Ret(static_cast<std::uint32_t>(Z_STREAM_ERROR));
                return true;
            }

            int rc = Z_STREAM_ERROR;
            if (fast_name == "inflate" || fast_name == "deflate") {
                if (!SyncGuestZlibBuffersToHost(guest_address, state)) {
                    Ret(static_cast<std::uint32_t>(Z_STREAM_ERROR));
                    return true;
                }
                rc = wants_inflate
                    ? ::inflate(&state.stream, static_cast<int>(Arg(1)))
                    : ::deflate(&state.stream, static_cast<int>(Arg(1)));
                SyncHostZlibStateToGuest(guest_address, state, true);
            } else if (fast_name == "inflateReset" || fast_name == "deflateReset") {
                rc = wants_inflate ? ::inflateReset(&state.stream) : ::deflateReset(&state.stream);
                SyncHostZlibStateToGuest(guest_address, state, rc == Z_OK);
            } else {
                rc = wants_inflate ? ::inflateEnd(&state.stream) : ::deflateEnd(&state.stream);
                state.initialized = false;
                SyncHostZlibStateToGuest(guest_address, state, false);
                zlib_streams_.erase(it);
            }

            if (file_trace.size() < 96) {
                std::ostringstream trace;
                trace << name << " guest=0x" << std::hex << guest_address << std::dec << " rc=" << rc;
                file_trace.push_back(trace.str());
            }
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        return false;
    }
#endif

#include "gles_bridge_methods.inc"
#include "compat_bridge_methods.inc"

public:
    // Retain the initial one-shot guest worker report, but allow the frequent
    // per-frame pumps to skip string formatting/allocations in production.
    bool record_pump_trace = true;
    std::vector<std::string> guest_thread_pump_trace;
    std::string last_trylock_busy_detail;
    std::uint64_t trylock_busy_total = 0;
    bool allow_main_thread_cooperative_yield = false;
    std::uint32_t main_thread_wait_mutex_ = 0u;
    struct WorkerBoundaryRecord {
        std::uint32_t thread_id = 0;
        std::uint32_t thread_start = 0;
        std::string thunk;
        std::uint32_t fault_pc = 0;
        std::uint32_t jni_slot = 0xffffffffu;
        std::array<std::uint32_t, 16> regs{};
    };
    bool worker_boundary_seen = false;
    std::uint32_t worker_boundary_thread_id = 0;
    std::string worker_boundary_thunk;
    std::uint32_t worker_boundary_fault_pc = 0;
    std::uint32_t worker_boundary_jni_slot = 0xffffffffu;
    std::array<std::uint32_t, 16> worker_boundary_regs{};
    std::vector<WorkerBoundaryRecord> worker_boundaries;

    void ResetWorkerBoundaryDiagnostics() {
        worker_boundary_seen = false;
        worker_boundary_thread_id = 0;
        worker_boundary_thunk.clear();
        worker_boundary_fault_pc = 0;
        worker_boundary_jni_slot = 0xffffffffu;
        worker_boundary_regs.fill(0);
        worker_boundaries.clear();
    }

    bool ConsumeCooperativeYieldRequest() {
        if (!cooperative_yield_requested_) return false;
        cooperative_yield_requested_ = false;
        return true;
    }

    bool MainTrylockWaitPending() const { return main_thread_wait_mutex_ != 0u; }

    bool MainTrylockWaitOwnerActive() const {
        if (main_thread_wait_mutex_ == 0u) return false;
        const auto mutex = guest_mutexes_.find(main_thread_wait_mutex_);
        if (mutex == guest_mutexes_.end() || mutex->second.owner == 0u || mutex->second.owner == 1u)
            return false;
        const auto owner = std::find_if(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                                        [&](const GuestThreadLaunch& item) {
                                            return item.id == mutex->second.owner;
                                        });
        return owner != guest_thread_launches_.end() && !owner->finished &&
               (owner->state == GuestThreadState::New ||
                owner->state == GuestThreadState::Runnable);
    }

    void PreferMainTrylockWaitOwner() {
        if (!MainTrylockWaitOwnerActive()) return;
        const auto mutex = guest_mutexes_.find(main_thread_wait_mutex_);
        preferred_guest_thread_id_ = mutex->second.owner;
        trylock_preferred_guest_thread_id_ = mutex->second.owner;
    }

    void ClearMainTrylockWait() { main_thread_wait_mutex_ = 0u; }

    static std::uint32_t GuestSignalBit(std::uint32_t signal) {
        return signal >= 1u && signal <= 32u ? (1u << (signal - 1u)) : 0u;
    }

    bool GuestSignalIsBlocked(std::uint32_t thread_id, std::uint32_t signal) const {
        const std::uint32_t bit = GuestSignalBit(signal);
        if (bit == 0u) return false;
        const auto mask = guest_signal_masks_.find(thread_id);
        return mask != guest_signal_masks_.end() && (mask->second & bit) != 0u;
    }

    bool HasDeliverablePendingGuestSignal(std::uint32_t thread_id) const {
        const auto pending = guest_pending_signals_.find(thread_id);
        if (pending == guest_pending_signals_.end()) return false;
        return std::any_of(pending->second.begin(), pending->second.end(),
                           [&](std::uint32_t signal) { return !GuestSignalIsBlocked(thread_id, signal); });
    }

    void MarkGuestThreadRunnable(std::uint32_t thread_id) {
        if (thread_id == 1u) return;
        const auto thread = std::find_if(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                                         [&](const GuestThreadLaunch& item) {
                                             return item.id == thread_id && !item.finished;
                                         });
        if (thread == guest_thread_launches_.end()) return;
        thread->state = GuestThreadState::Runnable;
        thread->wait_object = 0u;
        thread->wait_aux = 0u;
        thread->wait_owner = 0u;
        preferred_guest_thread_id_ = thread_id;
    }

    bool QueueGuestSignal(std::uint32_t target_thread, std::uint32_t signal) {
        if (signal == 0) return target_thread == 1 ||
            std::any_of(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                        [&](const GuestThreadLaunch& item) { return item.id == target_thread && !item.finished; });
        if (target_thread != 1) {
            const auto target = std::find_if(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                                             [&](const GuestThreadLaunch& item) {
                                                 return item.id == target_thread && !item.finished;
                                             });
            if (target == guest_thread_launches_.end()) return false;
        }
        guest_pending_signals_[target_thread].push_back(signal);
        if (!GuestSignalIsBlocked(target_thread, signal) && target_thread != 1u) {
            const auto target = std::find_if(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                                             [&](const GuestThreadLaunch& item) {
                                                 return item.id == target_thread && !item.finished;
                                             });
            if (target != guest_thread_launches_.end() && target->state == GuestThreadState::BlockedSignal)
                MarkGuestThreadRunnable(target_thread);
        }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                            "PV7SIGNAL queue sender=%u target=%u signal=%u",
                            current_thread_id_, target_thread, signal);
#endif
        return true;
    }

    bool DeliverPendingGuestSignal() {
        if (!jit) return false;
        auto pending = guest_pending_signals_.find(current_thread_id_);
        if (pending == guest_pending_signals_.end() || pending->second.empty()) return false;
        const auto deliverable = std::find_if(
            pending->second.begin(), pending->second.end(),
            [&](std::uint32_t signal) { return !GuestSignalIsBlocked(current_thread_id_, signal); });
        if (deliverable == pending->second.end()) return false;
        const std::uint32_t signal = *deliverable;
        pending->second.erase(deliverable);
        if (pending->second.empty()) guest_pending_signals_.erase(pending);

        const auto action_it = guest_signal_actions_.find(signal);
        if (action_it == guest_signal_actions_.end() || action_it->second.handler == 0u ||
            action_it->second.handler == 1u) {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL ignored/default thread=%u signal=%u handler=0x%08x",
                                current_thread_id_, signal,
                                action_it == guest_signal_actions_.end() ? 0u : action_it->second.handler);
#endif
            return true;
        }

        GuestSignalFrame frame{};
        frame.regs = jit->Regs();
        frame.ext_regs = jit->ExtRegs();
        frame.cpsr = jit->Cpsr();
        frame.fpscr = jit->Fpscr();
        frame.signal = signal;
        frame.signal_mask = guest_signal_masks_[current_thread_id_];

        const GuestSignalAction action = action_it->second;
        std::uint32_t handler_mask = frame.signal_mask | action.mask;
        constexpr std::uint32_t kSaNoDefer = 0x40000000u;
        if ((action.flags & kSaNoDefer) == 0u) handler_mask |= GuestSignalBit(signal);
        guest_signal_masks_[current_thread_id_] = handler_mask;
        const bool uses_siginfo = (action.flags & 0x4u) != 0u; // SA_SIGINFO
        std::uint32_t handler_sp = frame.regs[13];
        if (uses_siginfo) {
            // ARM32 Android delivers SA_SIGINFO handlers a real siginfo_t and
            // ucontext_t. Mono uses that context for exception/backtrace control,
            // so null placeholders are not sufficient.
            std::uint32_t frame_base = 0;
            if (frame.regs[13] > kGuestSignalFrameReserve) {
                frame_base = (frame.regs[13] - kGuestSignalFrameReserve) & ~7u;
            }
            if (frame_base != 0u && Fits(frame_base, kGuestSignalFrameReserve)) {
                std::memset(memory_.data() + frame_base, 0, kGuestSignalFrameReserve);
                handler_sp = frame_base;
                frame.guest_siginfo = frame_base + 64u;
                frame.guest_ucontext = frame.guest_siginfo + kGuestSignalInfoSize;
            } else {
                frame.guest_siginfo = Allocate(kGuestSignalInfoSize, 8u);
                frame.guest_ucontext = Allocate(kGuestUcontextSize, 8u);
                if (frame.guest_siginfo) std::memset(memory_.data() + frame.guest_siginfo, 0, kGuestSignalInfoSize);
                if (frame.guest_ucontext) std::memset(memory_.data() + frame.guest_ucontext, 0, kGuestUcontextSize);
            }
            frame.uses_siginfo = frame.guest_siginfo != 0u && frame.guest_ucontext != 0u;
            if (frame.uses_siginfo) {
                // siginfo_t: signo, errno, code. Raised/thread-directed signals use
                // SI_TKILL on Android/Linux; the remaining payload is zeroed.
                Write32(memory_, frame.guest_siginfo + 0u, signal);
                Write32(memory_, frame.guest_siginfo + 4u, 0u);
                Write32(memory_, frame.guest_siginfo + 8u, static_cast<std::uint32_t>(-6));

                // ARM32 ucontext_t starts with flags/link/stack_t (20 bytes), then
                // struct sigcontext. Populate the architectural state exactly where
                // old Bionic/Mono expects it.
                const std::uint32_t uc = frame.guest_ucontext;
                Write32(memory_, uc + 0u, 0u); // uc_flags
                Write32(memory_, uc + 4u, 0u); // uc_link
                // uc_stack reports the alternate signal stack state, not the
                // interrupted thread's normal pthread stack. No guest sigaltstack
                // has been installed, so report the kernel default: SS_DISABLE.
                Write32(memory_, uc + 8u, 0u); // uc_stack.ss_sp
                Write32(memory_, uc + 12u, 2u); // uc_stack.ss_flags = SS_DISABLE
                Write32(memory_, uc + 16u, 0u); // uc_stack.ss_size
                constexpr std::uint32_t mcontext = 20u;
                Write32(memory_, uc + mcontext + 0u, 0u); // trap_no
                Write32(memory_, uc + mcontext + 4u, 0u); // error_code
                Write32(memory_, uc + mcontext + 8u, frame.signal_mask); // oldmask
                for (std::uint32_t reg = 0; reg <= 10u; ++reg)
                    Write32(memory_, uc + mcontext + 12u + reg * 4u, frame.regs[reg]);
                Write32(memory_, uc + mcontext + 56u, frame.regs[11]);
                Write32(memory_, uc + mcontext + 60u, frame.regs[12]);
                Write32(memory_, uc + mcontext + 64u, frame.regs[13]);
                Write32(memory_, uc + mcontext + 68u, frame.regs[14]);
                Write32(memory_, uc + mcontext + 72u, frame.regs[15]);
                Write32(memory_, uc + mcontext + 76u, frame.cpsr);
                Write32(memory_, uc + mcontext + 80u, 0u); // fault_address
            }
        }
        guest_signal_frames_[current_thread_id_].push_back(frame);

        jit->Regs()[0] = signal;
        jit->Regs()[1] = frame.uses_siginfo ? frame.guest_siginfo : 0u;
        jit->Regs()[2] = frame.uses_siginfo ? frame.guest_ucontext : 0u;
        jit->Regs()[13] = frame.uses_siginfo ? handler_sp : frame.regs[13];
        jit->Regs()[14] = kSignalReturnStub;
        jit->Regs()[15] = action.handler & ~1u;
        jit->SetCpsr((frame.cpsr & ~0x20u) | ((action.handler & 1u) ? 0x20u : 0u));
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                            "PV7SIGNAL deliver thread=%u signal=%u handler=0x%08x interrupted_pc=0x%08x interrupted_sp=0x%08x handler_sp=0x%08x ucontext=0x%08x",
                            current_thread_id_, signal, action.handler, frame.regs[15], frame.regs[13],
                            handler_sp, frame.guest_ucontext);
#endif
        return true;
    }

    std::size_t PumpQueuedGuestThreads(std::size_t max_threads, int max_slices_per_thread) {
        if (!jit || max_threads == 0 || max_slices_per_thread <= 0) return 0;

        const auto saved_regs = jit->Regs();
        const auto saved_ext_regs = jit->ExtRegs();
        const std::uint32_t saved_cpsr = jit->Cpsr();
        const std::uint32_t saved_fpscr = jit->Fpscr();
        const auto saved_tls_values = tls_values_;
        const std::uint32_t saved_arm_tls = arm_tls_value_;
        const std::uint32_t saved_thread_id = current_thread_id_;
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
        TraceEvent(pv7::trace::WorkerEnter, jit->Regs()[15], jit->Regs()[14],
                   static_cast<std::uint32_t>(max_threads),
                   static_cast<std::uint32_t>(max_slices_per_thread));
#endif
        std::size_t pumped = 0;
        guest_thread_pump_trace.clear();
        const std::size_t thread_count_at_entry = guest_thread_launches_.size();
        std::size_t inspected = 0;

        // A live compatibility experiment may nominate the guest thread that should
        // receive the next cooperative quantum. This mirrors the normal OS behavior
        // where a thread explicitly awakened by a synchronization primitive is made
        // runnable immediately instead of waiting for a full round-robin scan.
        const std::uint32_t preferred_thread_at_entry = preferred_guest_thread_id_;
        const bool trylock_preferred_only =
            preferred_thread_at_entry != 0u &&
            preferred_thread_at_entry == trylock_preferred_guest_thread_id_;
        trylock_preferred_guest_thread_id_ = 0u;
        if (preferred_thread_at_entry != 0u && !guest_thread_launches_.empty()) {
            const auto preferred = std::find_if(
                guest_thread_launches_.begin(), guest_thread_launches_.end(),
                [&](const GuestThreadLaunch& item) {
                    return item.id == preferred_thread_at_entry && !item.finished;
                });
            if (preferred != guest_thread_launches_.end())
                guest_thread_pump_cursor_ = static_cast<std::size_t>(
                    std::distance(guest_thread_launches_.begin(), preferred));
            preferred_guest_thread_id_ = 0u;
        }

        // A blocked compatibility thunk is already represented by the saved guest PC
        // plus its argument registers. Do not spend a Dynarmic quantum re-entering a
        // wait that cannot possibly complete yet; the thread becomes runnable again
        // as soon as the object it is waiting on changes state.
        auto blocked_wait_is_ready = [&](const GuestThreadLaunch& launch) -> bool {
            if (!launch.started) return true;


            auto at_stub = [&](const char* symbol) -> bool {
                const auto stub = name_to_stub_.find(symbol);
                return stub != name_to_stub_.end() && launch.regs[15] == stub->second;
            };
            auto wait_key = [&](std::uint32_t object) -> std::uint64_t {
                return (static_cast<std::uint64_t>(launch.id) << 32) | object;
            };

            if (HasDeliverablePendingGuestSignal(launch.id)) {
#if defined(PROJECTV7_TEST_WORKER_NANOSLEEP_DEADLINES) && PROJECTV7_TEST_WORKER_NANOSLEEP_DEADLINES
                guest_nanosleep_deadlines_.erase(launch.id);
#endif
                return true;
            }
#if defined(PROJECTV7_TEST_WORKER_NANOSLEEP_DEADLINES) && PROJECTV7_TEST_WORKER_NANOSLEEP_DEADLINES
            const auto wake = guest_nanosleep_deadlines_.find(launch.id);
            if (wake != guest_nanosleep_deadlines_.end()) {
                if (std::chrono::steady_clock::now() < wake->second) return false;
                guest_nanosleep_deadlines_.erase(wake);
            }
#endif

            if (at_stub("sigsuspend")) return false;

            if (at_stub("pthread_mutex_lock")) {
                const auto mutex = guest_mutexes_.find(launch.regs[0]);
                return mutex == guest_mutexes_.end() || mutex->second.owner == 0u ||
                       mutex->second.owner == launch.id;
            }

            if (at_stub("sem_wait") || at_stub("sem_timedwait")) {
                const std::uint32_t sem = launch.regs[0];
                const auto value = guest_semaphores_.find(sem);
                if (value != guest_semaphores_.end() && value->second > 0) return true;
                if (at_stub("sem_timedwait")) {
                    const auto deadline = guest_sem_deadlines_.find(wait_key(sem));
                    if (deadline != guest_sem_deadlines_.end() &&
                        std::chrono::system_clock::now() >= deadline->second)
                        return true;
                }
                return false;
            }

            if (at_stub("pthread_join")) {
                const std::uint32_t target_id = launch.regs[0];
                const auto target = std::find_if(
                    guest_thread_launches_.begin(), guest_thread_launches_.end(),
                    [&](const GuestThreadLaunch& item) { return item.id == target_id; });
                return target == guest_thread_launches_.end() || target->finished;
            }

            if (at_stub("pthread_cond_wait") || at_stub("pthread_cond_timedwait") ||
                at_stub("pthread_cond_timedwait_monotonic_np")) {
                const std::uint32_t cond = launch.regs[0];
                const std::uint32_t mutex_address = launch.regs[1];
                const auto wait = guest_cond_waits_.find(wait_key(cond));
                if (wait == guest_cond_waits_.end()) return true;
                const auto cond_state = guest_conds_.find(cond);
                const std::uint64_t generation = cond_state == guest_conds_.end()
                    ? 0u : cond_state->second.broadcast_generation;
                const bool broadcasted = generation != wait->second.broadcast_generation;
                const bool timed_out = wait->second.timed &&
                    std::chrono::system_clock::now() >= wait->second.deadline;
                if (!broadcasted && !wait->second.signaled && !timed_out) return false;
                if (timed_out && !broadcasted && !wait->second.signaled) return true;
                const auto mutex = guest_mutexes_.find(mutex_address);
                return mutex == guest_mutexes_.end() || mutex->second.owner == 0u ||
                       mutex->second.owner == launch.id;
            }

            return true;
        };

        // Round-robin across the guest thread table. Starting from element zero on
        // every cooperative yield can permanently starve later-created threads when
        // the runnable set is larger than max_threads (FMOD commonly exposes this).
        while (pumped < max_threads && inspected < thread_count_at_entry &&
               !guest_thread_launches_.empty()) {
            if (guest_thread_pump_cursor_ >= guest_thread_launches_.size())
                guest_thread_pump_cursor_ = 0;
            const std::size_t launch_index = guest_thread_pump_cursor_;
            guest_thread_pump_cursor_ = (guest_thread_pump_cursor_ + 1) % guest_thread_launches_.size();
            ++inspected;
            auto& launch = guest_thread_launches_[launch_index];
            if (launch.finished || launch.start < kMainBase || launch.start >= kGuestHeapStart) continue;
            if (!blocked_wait_is_ready(launch)) continue;
            if (launch.state != GuestThreadState::New) {
                launch.state = GuestThreadState::Runnable;
                launch.wait_object = 0;
                launch.wait_aux = 0;
                launch.wait_owner = 0;
            }

            if (!launch.started) {
                launch.started = true;
                launch.regs.fill(0);
                launch.ext_regs.fill(0);
                launch.regs[0] = launch.arg;
                const std::uint64_t stack_offset =
                    static_cast<std::uint64_t>(launch.id - 2u) * kGuestThreadStackStride;
                if (stack_offset > static_cast<std::uint64_t>(kGuestThreadStackTop - kGuestThreadStackFloor)) {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                    __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                        "PV7STACK exhausted guest thread stack region id=%u offset=0x%llx",
                                        launch.id, static_cast<unsigned long long>(stack_offset));
#endif
                    launch.finished = true;
                    continue;
                }
                launch.regs[13] = kGuestThreadStackTop - static_cast<std::uint32_t>(stack_offset);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                if (launch.id == 3u) {
                    // The Unity async-reader worker keeps its queue mutex pointer at
                    // sp+4 after a 56-byte entry prologue. Watch that slot to find
                    // the writer if the parked worker frame is corrupted.
                    watched_write_address = launch.regs[13] - 52u;
                    watched_write_count = 0;
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                        "PV7STACK watch thread=3 slot=0x%08x stack_top=0x%08x",
                                        watched_write_address, launch.regs[13]);
                }
#endif
                launch.regs[14] = kReturnStub;
                launch.regs[15] = launch.start & ~1u;
                launch.cpsr = (launch.start & 1u) ? 0x20u : 0u;
                launch.fpscr = 0;
                launch.tls_values.clear();
                launch.arm_tls = 0;
            }

            tls_values_ = launch.tls_values;
            arm_tls_value_ = launch.arm_tls;
            current_thread_id_ = launch.id;
            ResetPhaseDiagnostics();
            saw_return = false;
            premature_return = false;
            failed = false;
            fault_pc = 0;
            jit->ClearHalt();
            jit->Regs() = launch.regs;
            jit->ExtRegs() = launch.ext_regs;
            jit->SetCpsr(launch.cpsr);
            jit->SetFpscr(launch.fpscr);

#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            const auto worker_quantum_begin = std::chrono::steady_clock::now();
#endif
            int slices_run = 0;
            std::size_t cooperative_yields = 0;
            for (; slices_run < max_slices_per_thread; ++slices_run) {
                const auto sampling_before = guest_thread_sampling_.find(current_thread_id_);
                const bool pc_sampling =
                    sampling_before != guest_thread_sampling_.end() &&
                    sampling_before->second.samples_remaining != 0u;
                const std::uint64_t slice_tick_budget =
                    pc_sampling ? sampling_before->second.ticks_per_slice : 1000000u;
                ticks_left = slice_tick_budget;
                DeliverPendingGuestSignal();
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                const std::uint32_t watched_before = watched_write_address != 0
                    ? Read<std::uint32_t>(watched_write_address) : 0u;
                const std::uint32_t watched_pc_before = jit->Regs()[15];
                const std::uint32_t watched_lr_before = jit->Regs()[14];
                const auto worker_slice_begin = std::chrono::steady_clock::now();
#endif
                const auto halt_reason = jit->Run();
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                const std::uint64_t worker_slice_us = static_cast<std::uint64_t>(
                    std::chrono::duration_cast<std::chrono::microseconds>(
                        std::chrono::steady_clock::now() - worker_slice_begin).count());
                if (pc_sampling) {
                    const auto sampling_after = guest_thread_sampling_.find(current_thread_id_);
                    if (sampling_after != guest_thread_sampling_.end() &&
                        sampling_after->second.samples_remaining != 0u) {
                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                            "PV7SAMPLE thread=%u us=%llu slice=%d ticks=%llu pc_before=0x%08x lr_before=0x%08x pc_after=0x%08x lr_after=0x%08x last=%s",
                                            current_thread_id_,
                                            static_cast<unsigned long long>(worker_slice_us), slices_run,
                                            static_cast<unsigned long long>(slice_tick_budget),
                                            watched_pc_before, watched_lr_before, jit->Regs()[15], jit->Regs()[14],
                                            call_trace.empty() ? "-" : call_trace.back().c_str());
                        --sampling_after->second.samples_remaining;
                    }
                }
                if (current_thread_id_ == unity_preload_thread_id_ && worker_slice_us >= 20000u) {
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                        "PV7PERF preload-slice us=%llu slice=%d pc_before=0x%08x lr_before=0x%08x pc_after=0x%08x lr_after=0x%08x ticks_left=%lld last=%s",
                                        static_cast<unsigned long long>(worker_slice_us), slices_run,
                                        watched_pc_before, watched_lr_before, jit->Regs()[15], jit->Regs()[14],
                                        static_cast<long long>(ticks_left),
                                        call_trace.empty() ? "-" : call_trace.back().c_str());
                }
                if (watched_write_address != 0) {
                    const std::uint32_t watched_after = Read<std::uint32_t>(watched_write_address);
                    if (watched_before != watched_after && watched_write_count < 64u) {
                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                            "PV7STACK slice-change thread=%u addr=0x%08x old=0x%08x new=0x%08x pc_before=0x%08x lr_before=0x%08x pc_after=0x%08x lr_after=0x%08x",
                                            current_thread_id_, watched_write_address, watched_before, watched_after,
                                            watched_pc_before, watched_lr_before, jit->Regs()[15], jit->Regs()[14]);
                        ++watched_write_count;
                    }
                }
#endif
                if (saw_return || premature_return || failed || !first_thunk.empty() || thread_exit_requested_) break;
                if (cooperative_yield_requested_) {
                    ++cooperative_yields;
                    cooperative_yield_requested_ = false;
                    jit->ClearHalt();
                    // A cooperative yield ends this guest thread's current quantum.
                    // Re-entering the same worker immediately defeats sched_yield and
                    // repeatedly retries blocked pthread/sem waits before another
                    // runnable guest thread gets a chance to make progress.
                    break;
                }
                if (pending_guest_callback) {
                    pending_guest_callback = false;
                    jit->ClearHalt();
                    continue;
                }
                if (pending_cacheflush) {
                    const std::uint32_t start = pending_cacheflush_start;
                    const std::uint32_t size = pending_cacheflush_size;
                    pending_cacheflush = false;
                    pending_cacheflush_start = 0;
                    pending_cacheflush_size = 0;
                    jit->ClearHalt();
                    if (size != 0) InvalidateTranslatedCodeRange(start, size);
                    continue;
                }
                if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                    jit->ClearHalt();
                    continue;
                }
                jit->ClearHalt();
            }

#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            const std::uint64_t worker_quantum_us = static_cast<std::uint64_t>(
                std::chrono::duration_cast<std::chrono::microseconds>(
                    std::chrono::steady_clock::now() - worker_quantum_begin).count());
            guest_thread_perf_us_[current_thread_id_] += worker_quantum_us;
            ++guest_thread_perf_visits_[current_thread_id_];
            if (worker_quantum_us >= 100000u) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7PERF slow-worker thread=%u elapsed_us=%llu slices=%d yields=%zu pc=0x%08x lr=0x%08x last=%s",
                                    current_thread_id_, static_cast<unsigned long long>(worker_quantum_us),
                                    slices_run, cooperative_yields, jit->Regs()[15], jit->Regs()[14],
                                    call_trace.empty() ? "-" : call_trace.back().c_str());
            }
#endif
            launch.regs = jit->Regs();
            launch.ext_regs = jit->ExtRegs();
            launch.cpsr = jit->Cpsr();
            launch.fpscr = jit->Fpscr();
            launch.tls_values = tls_values_;
            launch.arm_tls = arm_tls_value_;
            launch.total_slices += static_cast<std::size_t>(slices_run);
            if (saw_return || thread_exit_requested_) {
                launch.finished = true;
                launch.state = GuestThreadState::Finished;
                for (auto& waiter : guest_thread_launches_) {
                    if (!waiter.finished && waiter.state == GuestThreadState::BlockedJoin &&
                        waiter.wait_object == launch.id) {
                        waiter.state = GuestThreadState::Runnable;
                        preferred_guest_thread_id_ = waiter.id;
                    }
                }
            }

            if (failed || premature_return || !first_thunk.empty() || jni_unknown_slot != 0xffffffffu) {
                WorkerBoundaryRecord boundary;
                boundary.thread_id = launch.id;
                boundary.thread_start = launch.start;
                boundary.thunk = first_thunk;
                boundary.fault_pc = fault_pc;
                boundary.jni_slot = jni_unknown_slot;
                boundary.regs = jit->Regs();
                const bool duplicate = std::any_of(worker_boundaries.begin(), worker_boundaries.end(),
                    [&](const WorkerBoundaryRecord& existing) {
                        return existing.thread_id == boundary.thread_id &&
                               existing.fault_pc == boundary.fault_pc &&
                               existing.jni_slot == boundary.jni_slot &&
                               existing.thunk == boundary.thunk &&
                               existing.regs[15] == boundary.regs[15];
                    });
                if (!duplicate && worker_boundaries.size() < 32) worker_boundaries.push_back(boundary);
                if (!worker_boundary_seen) {
                    worker_boundary_seen = true;
                    worker_boundary_thread_id = launch.id;
                    worker_boundary_thunk = first_thunk;
                    worker_boundary_fault_pc = fault_pc;
                    worker_boundary_jni_slot = jni_unknown_slot;
                    worker_boundary_regs = jit->Regs();
                }
                // A worker that reached an invalid/unhandled boundary cannot make
                // useful forward progress until a shim is supplied. Quarantine it
                // for the remainder of this diagnostic pass so other workers and
                // the Unity main thread can continue and expose more boundaries.
                std::ostringstream detail;
                detail<<" fault="<<std::hex<<fault_pc<<" jni="<<jni_unknown_slot<<std::dec
                      <<" failed="<<failed<<" premature="<<premature_return<<" thunk="<<first_thunk;
                launch.terminal_detail=detail.str();
                launch.finished = true;
            }

            if (record_pump_trace) {
            std::ostringstream thread_line;
            thread_line << "id=" << launch.id << " start=0x" << std::hex << launch.start
                        << " pc=0x" << launch.regs[15] << std::dec
                        << " slices=" << slices_run
                        << " total=" << launch.total_slices
                        << " yields=" << cooperative_yields
                        << " finished=" << (launch.finished ? "YES" : "NO")
                        << " failed=" << (failed ? "YES" : "NO");
            if (!first_thunk.empty()) thread_line << " thunk=" << first_thunk;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            const auto recent_calls = guest_thread_call_traces_.find(launch.id);
            if (recent_calls != guest_thread_call_traces_.end() && !recent_calls->second.empty()) {
                thread_line << " recent=[";
                for (std::size_t i = 0; i < recent_calls->second.size(); ++i) {
                    if (i != 0) thread_line << ',';
                    thread_line << recent_calls->second[i];
                }
                thread_line << ']';
            }
            const auto recent_sync = guest_thread_sync_traces_.find(launch.id);
            if (recent_sync != guest_thread_sync_traces_.end() && !recent_sync->second.empty()) {
                thread_line << " sync=[";
                for (std::size_t i = 0; i < recent_sync->second.size(); ++i) {
                    if (i != 0) thread_line << ',';
                    thread_line << recent_sync->second[i];
                }
                thread_line << ']';
            }
#endif
            guest_thread_pump_trace.push_back(thread_line.str());
            }
            ++pumped;
            if (trylock_preferred_only && launch.id == preferred_thread_at_entry) break;
        }

        tls_values_ = saved_tls_values;
        arm_tls_value_ = saved_arm_tls;
        current_thread_id_ = saved_thread_id;
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
        TraceEvent(pv7::trace::WorkerExit, jit->Regs()[15], jit->Regs()[14],
                   static_cast<std::uint32_t>(pumped),
                   static_cast<std::uint32_t>(max_slices_per_thread));
#endif
        cooperative_yield_requested_ = false;
        thread_exit_requested_ = false;
        jit->ClearHalt();
        jit->Regs() = saved_regs;
        jit->ExtRegs() = saved_ext_regs;
        jit->SetCpsr(saved_cpsr);
        jit->SetFpscr(saved_fpscr);
        return pumped;
    }

    bool DispatchClocks(const std::string& name) {
        const Pv7DispatchNameKey& fast_name{name};
        if (fast_name == "clock") { Ret(0); return true; }
        if (fast_name == "clock_gettime" || fast_name == "clock_getres") {
            const std::uint32_t clock_id = Arg(0);
            const std::uint32_t ts = Arg(1);
#if defined(PROJECTV7_STARTUP_HOTPAGE_TRACE) && PROJECTV7_STARTUP_HOTPAGE_TRACE
            if (clock_id < guest_clock_id_counts.size()) ++guest_clock_id_counts[clock_id];
            else ++guest_clock_unknown_id_calls;
            if (fast_name == "clock_getres") ++guest_clock_getres_calls;
#endif
            if (!Fits(ts, 8)) return false;
#if defined(PROJECTV7_TEST_NATIVE_CLOCKS) && PROJECTV7_TEST_NATIVE_CLOCKS && defined(__ANDROID__)
            // Android and the ARMv7 bionic guest use Linux clock IDs. Keep
            // REALTIME (0) and MONOTONIC (1) distinct. Returning wall time for
            // CLOCK_MONOTONIC breaks Unity/Mono timeout arithmetic.
            ::timespec now{};
            const ::clockid_t host_id = static_cast<::clockid_t>(clock_id);
            const int result = fast_name == "clock_getres"
                ? ::clock_getres(host_id, &now)
                : ::clock_gettime(host_id, &now);
            if (result != 0) {
                Write32(memory_, kBootstrapRuntimeBase, static_cast<std::uint32_t>(errno));
                Ret(0xffffffffu);
                return true;
            }
            Write32(memory_, ts, static_cast<std::uint32_t>(now.tv_sec));
            Write32(memory_, ts + 4u, static_cast<std::uint32_t>(now.tv_nsec));
#else
            const auto now = std::chrono::system_clock::now().time_since_epoch();
            const auto sec = std::chrono::duration_cast<std::chrono::seconds>(now);
            const auto nsec = std::chrono::duration_cast<std::chrono::nanoseconds>(now - sec);
            Write32(memory_, ts, static_cast<std::uint32_t>(sec.count()));
            Write32(memory_, ts + 4, fast_name == "clock_getres" ? 1000000u : static_cast<std::uint32_t>(nsec.count()));
#endif
            Ret(0);
            return true;
        }
        if (fast_name == "gettimeofday") {
            const std::uint32_t tv = Arg(0);
            if (tv) {
                if (!Fits(tv, 8)) return false;
                const auto now = std::chrono::system_clock::now().time_since_epoch();
                const auto sec = std::chrono::duration_cast<std::chrono::seconds>(now);
                const auto usec = std::chrono::duration_cast<std::chrono::microseconds>(now - sec);
                Write32(memory_, tv, static_cast<std::uint32_t>(sec.count()));
                Write32(memory_, tv + 4, static_cast<std::uint32_t>(usec.count()));
            }
            Ret(0);
            return true;
        }
        if (fast_name == "time") {
            const auto seconds = std::chrono::duration_cast<std::chrono::seconds>(
                std::chrono::system_clock::now().time_since_epoch()).count();
            if (Arg(0)) Write32(memory_, Arg(0), static_cast<std::uint32_t>(seconds));
            Ret(static_cast<std::uint32_t>(seconds));
            return true;
        }
        return false;
    }

    bool Dispatch(const std::string& name) {
        const Pv7DispatchNameKey& fast_name{name};
        if (!jit) return false;
#if PROJECTV7_FAST_SYMBOL_DISPATCH
        if (DispatchClocks(name)) return true;
#endif
#if defined(__ANDROID__)
        if ((fast_name == "socket" || fast_name == "connect" || fast_name == "recv" || fast_name == "send" ||
             fast_name == "recvfrom" || fast_name == "sendto" || fast_name == "recvmsg" || fast_name == "sendmsg" ||
             fast_name == "getaddrinfo" || fast_name == "gethostbyname" || fast_name == "gethostbyaddr" ||
             fast_name == "poll" || fast_name == "select" || fast_name == "epoll_wait" || fast_name == "getsockopt" ||
             fast_name == "fcntl" || fast_name == "ioctl") && g_net_trace_count.fetch_add(1) < 160u) {
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7NET call=%s a0=0x%08x a1=0x%08x a2=0x%08x a3=0x%08x tid=%u pc=0x%08x",
                                name.c_str(), Arg(0), Arg(1), Arg(2), Arg(3),
                                current_thread_id_, jit->Regs()[15]);
        }
#endif
        if (DispatchCompatExtras(name)) return true;
#if defined(__ANDROID__)
        if (DispatchGles(name)) return true;
        if (DispatchZlib(name)) return true;
#endif
        if (fast_name == "ALooper_forThread" || fast_name == "ALooper_prepare") {
            Ret(kGuestLooperHandle);
            return true;
        }
        if (fast_name == "ALooper_wake" || fast_name == "AInputQueue_attachLooper" ||
            fast_name == "AInputQueue_detachLooper" || fast_name == "AInputQueue_finishEvent") {
            Ret(0);
            return true;
        }
        if (fast_name == "ALooper_pollAll") {
            if (Arg(1) != 0) Write32(memory_, Arg(1), 0u);
            if (Arg(2) != 0) Write32(memory_, Arg(2), 0u);
            if (Arg(3) != 0) Write32(memory_, Arg(3), 0u);
            Ret(0xffffffffu); // ALOOPER_POLL_TIMEOUT
            return true;
        }
        if (fast_name == "AInputQueue_getEvent") {
            if (Arg(1) != 0) Write32(memory_, Arg(1), 0u);
            Ret(0xffffffffu);
            return true;
        }
        if (fast_name == "AInputQueue_preDispatchEvent") { Ret(0); return true; }
        if (fast_name == "AConfiguration_new") { Ret(kGuestConfigurationHandle); return true; }
        if (fast_name == "AConfiguration_delete" || fast_name == "AConfiguration_fromAssetManager") { Ret(0); return true; }
        if (fast_name == "AInputEvent_getType") {
            Ret(NativeMotionEventForHandle(Arg(0)) ? 2u : 0u); // AINPUT_EVENT_TYPE_MOTION
            return true;
        }
        if (fast_name == "AInputEvent_getSource") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            Ret(touch ? static_cast<std::uint32_t>(touch->source) : 0u);
            return true;
        }
        if (fast_name == "AInputEvent_getDeviceId") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            Ret(touch ? static_cast<std::uint32_t>(touch->deviceId) : 0u);
            return true;
        }
        if (fast_name == "AKeyEvent_getAction" || fast_name == "AKeyEvent_getKeyCode" || fast_name == "AKeyEvent_getMetaState") {
            Ret(0);
            return true;
        }
        if (fast_name == "AMotionEvent_getAction") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            Ret(touch ? static_cast<std::uint32_t>(touch->action) : 0u);
            return true;
        }
        if (fast_name == "AMotionEvent_getMetaState") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            Ret(touch ? static_cast<std::uint32_t>(touch->metaState) : 0u);
            return true;
        }
        if (fast_name == "AMotionEvent_getFlags" || fast_name == "AMotionEvent_getEdgeFlags" ||
            fast_name == "AMotionEvent_getHistorySize") {
            Ret(0);
            return true;
        }
        if (fast_name == "AMotionEvent_getPointerId") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            const std::uint32_t index = Arg(1);
            Ret(touch && index < touch->points.size()
                ? static_cast<std::uint32_t>(touch->points[index].id) : 0xffffffffu);
            return true;
        }
        if (fast_name == "AMotionEvent_getPointerCount") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            Ret(touch ? static_cast<std::uint32_t>(touch->points.size()) : 0u);
            return true;
        }
        if (fast_name == "AMotionEvent_getDownTime") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            Ret64(touch ? static_cast<std::uint64_t>(touch->downTimeNanos) : 0u);
            return true;
        }
        if (fast_name == "AMotionEvent_getEventTime") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            Ret64(touch ? static_cast<std::uint64_t>(touch->eventTimeNanos) : 0u);
            return true;
        }
        if (fast_name == "AMotionEvent_getHistoricalEventTime") {
            Ret64(0);
            return true;
        }
        if (fast_name == "AMotionEvent_getX" || fast_name == "AMotionEvent_getY" ||
            fast_name == "AMotionEvent_getPressure" || fast_name == "AMotionEvent_getSize" ||
            fast_name == "AMotionEvent_getTouchMajor" || fast_name == "AMotionEvent_getTouchMinor" ||
            fast_name == "AMotionEvent_getToolMajor" || fast_name == "AMotionEvent_getToolMinor" ||
            fast_name == "AMotionEvent_getOrientation" || fast_name == "AMotionEvent_getXPrecision" ||
            fast_name == "AMotionEvent_getYPrecision") {
            const HostTouchEvent* touch = NativeMotionEventForHandle(Arg(0));
            const std::uint32_t index = Arg(1);
            const HostTouchPoint* point = touch && index < touch->points.size()
                ? &touch->points[index] : nullptr;
            float value = 0.0f;
            if (fast_name == "AMotionEvent_getX") value = point ? point->x : 0.0f;
            else if (fast_name == "AMotionEvent_getY") value = point ? point->y : 0.0f;
            else if (fast_name == "AMotionEvent_getPressure") value = point ? point->pressure : 0.0f;
            else if (fast_name == "AMotionEvent_getSize") value = point ? point->size : 0.0f;
            else if (fast_name == "AMotionEvent_getXPrecision" || fast_name == "AMotionEvent_getYPrecision") value = 1.0f;
            else if (fast_name == "AMotionEvent_getTouchMajor" || fast_name == "AMotionEvent_getTouchMinor" ||
                     fast_name == "AMotionEvent_getToolMajor" || fast_name == "AMotionEvent_getToolMinor")
                value = point ? std::max(point->size, 1.0f) : 0.0f;
            RetFloat(value);
            return true;
        }
        if (fast_name == "AMotionEvent_getHistoricalX" || fast_name == "AMotionEvent_getHistoricalY" ||
            fast_name == "AMotionEvent_getHistoricalPressure" || fast_name == "AMotionEvent_getHistoricalSize") {
            RetFloat(0.0f);
            return true;
        }
        if (fast_name == "ASensorManager_getInstance") { Ret(kGuestSensorManagerHandle); return true; }
        if (fast_name == "ASensorManager_getDefaultSensor") { Ret(0); return true; }
        if (fast_name == "ASensorManager_getSensorList") {
            if (Arg(1) != 0) Write32(memory_, Arg(1), 0u);
            Ret(0);
            return true;
        }
        if (fast_name == "ASensorManager_createEventQueue") { Ret(kGuestSensorQueueHandle); return true; }
        if (fast_name == "ASensorManager_destroyEventQueue" ||
            fast_name == "ASensorEventQueue_disableSensor" || fast_name == "ASensorEventQueue_enableSensor" ||
            fast_name == "ASensorEventQueue_setEventRate") {
            Ret(0);
            return true;
        }
        if (fast_name == "ASensorEventQueue_getEvents" || fast_name == "ASensorEventQueue_hasEvents") {
            Ret(0);
            return true;
        }
        if (fast_name == "ASensor_getName" || fast_name == "ASensor_getVendor") { Ret(0); return true; }
        if (fast_name == "ASensor_getType" || fast_name == "ASensor_getMinDelay") { Ret(0); return true; }
        if (fast_name == "ASensor_getResolution") { RetFloat(0.0f); return true; }

        // ARM Android uses the soft-float AAPCS at shared-library boundaries, so
        // scalar float/double libm calls arrive in the core-register word stream.
        if (fast_name == "sinf") { RetFloat(std::sin(ArgFloat(0))); return true; }
        if (fast_name == "cosf") { RetFloat(std::cos(ArgFloat(0))); return true; }
        if (fast_name == "tanf") { RetFloat(std::tan(ArgFloat(0))); return true; }
        if (fast_name == "asinf") { RetFloat(std::asin(ArgFloat(0))); return true; }
        if (fast_name == "acosf") { RetFloat(std::acos(ArgFloat(0))); return true; }
        if (fast_name == "atanf") { RetFloat(std::atan(ArgFloat(0))); return true; }
        if (fast_name == "atan2f") { RetFloat(std::atan2(ArgFloat(0), ArgFloat(1))); return true; }
        if (fast_name == "expf") { RetFloat(std::exp(ArgFloat(0))); return true; }
        if (fast_name == "logf") { RetFloat(std::log(ArgFloat(0))); return true; }
        if (fast_name == "sqrtf") { RetFloat(std::sqrt(ArgFloat(0))); return true; }
        if (fast_name == "ceilf") { RetFloat(std::ceil(ArgFloat(0))); return true; }
        if (fast_name == "floorf") { RetFloat(std::floor(ArgFloat(0))); return true; }
        if (fast_name == "fmodf") { RetFloat(std::fmod(ArgFloat(0), ArgFloat(1))); return true; }
        if (fast_name == "powf") { RetFloat(std::pow(ArgFloat(0), ArgFloat(1))); return true; }
        if (fast_name == "__fpclassifyf") { Ret(static_cast<std::uint32_t>(std::fpclassify(ArgFloat(0)))); return true; }
        if (fast_name == "isnan") { Ret(std::isnan(ArgDouble(0)) ? 1u : 0u); return true; }
        if (fast_name == "sin") { RetDouble(std::sin(ArgDouble(0))); return true; }
        if (fast_name == "cos") { RetDouble(std::cos(ArgDouble(0))); return true; }
        if (fast_name == "tan") { RetDouble(std::tan(ArgDouble(0))); return true; }
        if (fast_name == "asin") { RetDouble(std::asin(ArgDouble(0))); return true; }
        if (fast_name == "acos") { RetDouble(std::acos(ArgDouble(0))); return true; }
        if (fast_name == "atan") { RetDouble(std::atan(ArgDouble(0))); return true; }
        if (fast_name == "atan2") { RetDouble(std::atan2(ArgDouble(0), ArgDouble(2))); return true; }
        if (fast_name == "exp") { RetDouble(std::exp(ArgDouble(0))); return true; }
        if (fast_name == "log") { RetDouble(std::log(ArgDouble(0))); return true; }
        if (fast_name == "log10") { RetDouble(std::log10(ArgDouble(0))); return true; }
        if (fast_name == "sqrt") { RetDouble(std::sqrt(ArgDouble(0))); return true; }
        if (fast_name == "ceil") { RetDouble(std::ceil(ArgDouble(0))); return true; }
        if (fast_name == "floor") { RetDouble(std::floor(ArgDouble(0))); return true; }
        if (fast_name == "rint") { RetDouble(std::rint(ArgDouble(0))); return true; }
        if (fast_name == "round") { RetDouble(std::round(ArgDouble(0))); return true; }
        if (fast_name == "trunc") { RetDouble(std::trunc(ArgDouble(0))); return true; }
        if (fast_name == "sinh") { RetDouble(std::sinh(ArgDouble(0))); return true; }
        if (fast_name == "cosh") { RetDouble(std::cosh(ArgDouble(0))); return true; }
        if (fast_name == "tanh") { RetDouble(std::tanh(ArgDouble(0))); return true; }
        if (fast_name == "fmod") { RetDouble(std::fmod(ArgDouble(0), ArgDouble(2))); return true; }
        if (fast_name == "pow") { RetDouble(std::pow(ArgDouble(0), ArgDouble(2))); return true; }
        if (fast_name == "ldexp") { RetDouble(std::ldexp(ArgDouble(0), static_cast<int>(Arg(2)))); return true; }
        if (fast_name == "frexp") {
            int exponent = 0;
            const double value = std::frexp(ArgDouble(0), &exponent);
            if (Arg(2)) Write32(memory_, Arg(2), static_cast<std::uint32_t>(exponent));
            RetDouble(value);
            return true;
        }
        if (fast_name == "modff") {
            float integral = 0.0f;
            const float fraction = std::modf(ArgFloat(0), &integral);
            if (Arg(1) && Fits(Arg(1), 4)) std::memcpy(memory_.data() + Arg(1), &integral, 4);
            RetFloat(fraction);
            return true;
        }
        if (fast_name == "div") {
            const std::int32_t denom = static_cast<std::int32_t>(Arg(1));
            const std::int32_t numer = static_cast<std::int32_t>(Arg(0));
            if (!denom) return false;
            jit->Regs()[0] = static_cast<std::uint32_t>(numer / denom);
            jit->Regs()[1] = static_cast<std::uint32_t>(numer % denom);
            return true;
        }

        if (fast_name == "__aeabi_memmove") {
            const std::uint32_t dst = Arg(0), src = Arg(1), n = Arg(2);
            if (!Fits(dst, n) || !Fits(src, n)) return false;
            std::memmove(memory_.data() + dst, memory_.data() + src, n);
            Ret(dst);
            return true;
        }
        if (fast_name == "__aeabi_memset") {
            const std::uint32_t dst = Arg(0), n = Arg(1);
            if (!Fits(dst, n)) return false;
            std::memset(memory_.data() + dst, static_cast<int>(Arg(2) & 0xffu), n);
            Ret(dst);
            return true;
        }
        if (fast_name == "memchr") {
            const std::uint32_t ptr = Arg(0), n = Arg(2);
            if (!Fits(ptr, n)) return false;
            void* found = std::memchr(memory_.data() + ptr, static_cast<int>(Arg(1)), n);
            Ret(found ? ptr + static_cast<std::uint32_t>(static_cast<std::uint8_t*>(found) - (memory_.data() + ptr)) : 0u);
            return true;
        }
        if (fast_name == "memmem") {
            const std::uint32_t hay = Arg(0), hay_len = Arg(1), needle = Arg(2), needle_len = Arg(3);
            if (!Fits(hay, hay_len) || !Fits(needle, needle_len)) return false;
            if (needle_len == 0) { Ret(hay); return true; }
            std::uint32_t result = 0;
            for (std::uint32_t i = 0; i + needle_len <= hay_len; ++i) {
                if (std::memcmp(memory_.data() + hay + i, memory_.data() + needle, needle_len) == 0) { result = hay + i; break; }
            }
            Ret(result);
            return true;
        }
        if (fast_name == "memalign") { Ret(Allocate(Arg(1), std::max<std::uint32_t>(Arg(0), 1u))); return true; }

        if (fast_name == "strpbrk") {
            const std::string src = ReadCString(Arg(0)), accept = ReadCString(Arg(1));
            const std::size_t pos = src.find_first_of(accept);
            Ret(pos == std::string::npos ? 0u : Arg(0) + static_cast<std::uint32_t>(pos));
            return true;
        }
        if (fast_name == "strcasestr") {
            std::string hay = ReadCString(Arg(0)), needle = ReadCString(Arg(1));
            std::transform(hay.begin(), hay.end(), hay.begin(), [](unsigned char c){ return static_cast<char>(std::tolower(c)); });
            std::transform(needle.begin(), needle.end(), needle.begin(), [](unsigned char c){ return static_cast<char>(std::tolower(c)); });
            const std::size_t pos = hay.find(needle);
            Ret(pos == std::string::npos ? 0u : Arg(0) + static_cast<std::uint32_t>(pos));
            return true;
        }
        if (fast_name == "strncat") {
            const std::uint32_t dst = Arg(0);
            const std::string addition = ReadCString(Arg(1)).substr(0, Arg(2));
            if (!WriteCString(dst, ReadCString(dst) + addition)) return false;
            Ret(dst); return true;
        }
        if (fast_name == "strcoll") { Ret(static_cast<std::uint32_t>(ReadCString(Arg(0)).compare(ReadCString(Arg(1))))); return true; }
        if (fast_name == "strxfrm") {
            const std::string src = ReadCString(Arg(1));
            if (Arg(2) && Arg(0)) WriteCString(Arg(0), src, Arg(2));
            Ret(static_cast<std::uint32_t>(src.size())); return true;
        }
        if (fast_name == "strtoull") {
            const std::string text = ReadCString(Arg(0));
            char* end = nullptr;
            const unsigned long long value = std::strtoull(text.c_str(), &end, static_cast<int>(Arg(2)));
            if (Arg(1)) Write32(memory_, Arg(1), Arg(0) + static_cast<std::uint32_t>(end - text.c_str()));
            Ret64(static_cast<std::uint64_t>(value)); return true;
        }
        if (fast_name == "strtod") {
            const std::string text = ReadCString(Arg(0));
            char* end = nullptr;
            const double value = std::strtod(text.c_str(), &end);
            if (Arg(1)) Write32(memory_, Arg(1), Arg(0) + static_cast<std::uint32_t>(end - text.c_str()));
            RetDouble(value); return true;
        }

        if (fast_name == "access") {
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            if (file_trace.size() < 24) file_trace.push_back("access " + path);
            errno = 0;
#if defined(_WIN32)
            const int result = ::_access(path.c_str(), static_cast<int>(Arg(1)));
#else
            const int result = ::access(path.c_str(), static_cast<int>(Arg(1)));
#endif
            SetErrno(result == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(result));
            return true;
        }
        if (fast_name == "__errno" || fast_name == "__get_h_errno") {
            Ret(kBootstrapRuntimeBase);
            return true;
        }
        if (fast_name == "malloc") {
            Ret(Allocate(Arg(0)));
            return true;
        }
        if (fast_name == "calloc") {
            const std::uint64_t bytes = static_cast<std::uint64_t>(Arg(0)) * Arg(1);
            Ret(bytes > 0x01000000u ? 0 : Allocate(static_cast<std::size_t>(bytes)));
            return true;
        }
        if (fast_name == "free") {
            FreeAllocation(Arg(0));
            Ret(0);
            return true;
        }
        if (fast_name == "realloc") {
            const std::uint32_t old_ptr = Arg(0);
            const std::size_t new_size = Arg(1);
            if (!old_ptr) {
                Ret(Allocate(new_size));
                return true;
            }
            if (new_size == 0) {
                FreeAllocation(old_ptr);
                Ret(0);
                return true;
            }
            const auto old = allocations_.find(old_ptr);
            const std::size_t old_size = old != allocations_.end() ? old->second : 0u;
            const std::uint32_t fresh = Allocate(new_size);
            if (!fresh) {
                Ret(0);
                return true;
            }
            if (old_size && Fits(old_ptr, old_size)) {
                std::memcpy(memory_.data() + fresh, memory_.data() + old_ptr,
                            std::min<std::size_t>(old_size, new_size));
            }
            FreeAllocation(old_ptr);
            Ret(fresh);
            return true;
        }
        if (fast_name == "memcpy" || fast_name == "__aeabi_memcpy") {
            const std::uint32_t dst = Arg(0), src = Arg(1), n = Arg(2);
            if (!Fits(dst, n) || !Fits(src, n)) return false;
            std::memcpy(memory_.data() + dst, memory_.data() + src, n);
            Ret(dst);
            return true;
        }
        if (fast_name == "memmove") {
            const std::uint32_t dst = Arg(0), src = Arg(1), n = Arg(2);
            if (!Fits(dst, n) || !Fits(src, n)) return false;
            std::memmove(memory_.data() + dst, memory_.data() + src, n);
            Ret(dst);
            return true;
        }
        if (fast_name == "memset") {
            const std::uint32_t dst = Arg(0), n = Arg(2);
            if (!Fits(dst, n)) return false;
            std::memset(memory_.data() + dst, static_cast<int>(Arg(1) & 0xffu), n);
            Ret(dst);
            return true;
        }
        if (fast_name == "memcmp") {
            const std::uint32_t a = Arg(0), b = Arg(1), n = Arg(2);
            if (!Fits(a, n) || !Fits(b, n)) return false;
            Ret(static_cast<std::uint32_t>(std::memcmp(memory_.data() + a, memory_.data() + b, n)));
            return true;
        }
        if (fast_name == "strlen") {
            Ret(static_cast<std::uint32_t>(ReadFullCString(Arg(0)).size()));
            return true;
        }
        if (fast_name == "strcmp" || fast_name == "strcasecmp" || fast_name == "strncmp" || fast_name == "strncasecmp") {
            std::string a = ReadFullCString(Arg(0));
            std::string b = ReadFullCString(Arg(1));
            std::size_t limit = (fast_name == "strncmp" || fast_name == "strncasecmp") ? Arg(2) : SIZE_MAX;
            if (fast_name == "strcasecmp" || fast_name == "strncasecmp") {
                std::transform(a.begin(), a.end(), a.begin(), [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
                std::transform(b.begin(), b.end(), b.begin(), [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
            }
            const std::size_t n = std::min({limit, a.size() + 1, b.size() + 1});
            int result = 0;
            for (std::size_t i = 0; i < n; ++i) {
                const unsigned char ca = i < a.size() ? static_cast<unsigned char>(a[i]) : 0;
                const unsigned char cb = i < b.size() ? static_cast<unsigned char>(b[i]) : 0;
                if (ca != cb) { result = ca < cb ? -1 : 1; break; }
                if (ca == 0) break;
            }
            Ret(static_cast<std::uint32_t>(result));
            return true;
        }
        if (fast_name == "strcpy" || fast_name == "strncpy" || fast_name == "strlcpy") {
            const std::uint32_t dst = Arg(0);
            const std::string src = ReadFullCString(Arg(1));
            if (fast_name == "strcpy") {
                if (!WriteCString(dst, src)) return false;
                Ret(dst);
            } else if (fast_name == "strncpy") {
                const std::size_t n = Arg(2);
                if (!Fits(dst, n)) return false;
                const std::size_t copy = std::min(n, src.size());
                if (copy) std::memcpy(memory_.data() + dst, src.data(), copy);
                if (n > copy) std::memset(memory_.data() + dst + copy, 0, n - copy);
                Ret(dst);
            } else {
                const std::size_t n = Arg(2);
                if (n && !WriteCString(dst, src, n)) return false;
                Ret(static_cast<std::uint32_t>(src.size()));
            }
            return true;
        }
        if (fast_name == "strdup") {
            const std::string src = ReadFullCString(Arg(0));
            const std::uint32_t dst = Allocate(src.size() + 1);
            if (dst) WriteCString(dst, src);
            Ret(dst);
            return true;
        }
        if (fast_name == "strchr" || fast_name == "strrchr") {
            const std::string src = ReadFullCString(Arg(0));
            const char needle = static_cast<char>(Arg(1));
            const std::size_t pos = fast_name == "strchr" ? src.find(needle) : src.rfind(needle);
            Ret(pos == std::string::npos ? 0 : Arg(0) + static_cast<std::uint32_t>(pos));
            return true;
        }
        if (fast_name == "strstr") {
            const std::string hay = ReadFullCString(Arg(0));
            const std::string needle = ReadFullCString(Arg(1));
            const std::size_t pos = hay.find(needle);
            Ret(pos == std::string::npos ? 0 : Arg(0) + static_cast<std::uint32_t>(pos));
            return true;
        }
        if (fast_name == "strcat") {
            const std::uint32_t dst = Arg(0);
            const std::string combined = ReadFullCString(dst) + ReadFullCString(Arg(1));
            if (!WriteCString(dst, combined)) return false;
            Ret(dst);
            return true;
        }
        if (fast_name == "getenv") {
            Ret(0);
            return true;
        }
        if (fast_name == "setenv" || fast_name == "unsetenv" || fast_name == "chdir" || fast_name == "chmod") {
            Ret(0);
            return true;
        }
        if (fast_name == "getcwd") {
            const std::uint32_t buffer = Arg(0), size = Arg(1);
            if (!buffer || size < 2 || !WriteCString(buffer, "/", size)) Ret(0);
            else Ret(buffer);
            return true;
        }
        if (fast_name == "setlocale") {
            Ret(kBootstrapRuntimeBase + 0x100);
            return true;
        }
        // Minimal C-locale wide-character support used by libstdc++ locale facets
        // during Unity's global constructor phase. Android ARM32 wchar_t/wint_t are
        // 32-bit here, and the C locale maps bytes 1:1 to Unicode code points 0..255.
        if (fast_name == "wctob") {
            const std::uint32_t wc = Arg(0);
            Ret(wc <= 0xffu ? wc : 0xffffffffu); // EOF if not single-byte representable
            return true;
        }
        if (fast_name == "btowc") {
            const std::uint32_t c = Arg(0);
            Ret(c == 0xffffffffu ? 0xffffffffu : (c & 0xffu)); // WEOF or byte value
            return true;
        }
        if (fast_name == "mbrtowc") {
            const std::uint32_t out = Arg(0);
            const std::uint32_t src = Arg(1);
            const std::uint32_t count = Arg(2);
            if (!src) { Ret(0); return true; }
            if (count == 0) { Ret(0xfffffffeu); return true; } // (size_t)-2, incomplete
            const std::uint8_t c = Read<std::uint8_t>(src);
            if (out && Fits(out, 4)) Write32(memory_, out, static_cast<std::uint32_t>(c));
            Ret(c == 0 ? 0u : 1u);
            return true;
        }
        if (fast_name == "wcrtomb") {
            const std::uint32_t out = Arg(0);
            const std::uint32_t wc = Arg(1);
            if (!out) { Ret(1); return true; } // reset conversion state in C locale
            if (wc > 0xffu || !Fits(out, 1)) { Ret(0xffffffffu); return true; }
            memory_[out] = static_cast<std::uint8_t>(wc);
            Ret(1);
            return true;
        }
        if (fast_name == "wctype") {
            const std::string cls = ReadCString(Arg(0));
            static const std::unordered_map<std::string, std::uint32_t> classes = {
                {"alnum",1},{"alpha",2},{"blank",3},{"cntrl",4},{"digit",5},{"graph",6},
                {"lower",7},{"print",8},{"punct",9},{"space",10},{"upper",11},{"xdigit",12}
            };
            const auto it = classes.find(cls);
            Ret(it == classes.end() ? 0u : it->second);
            return true;
        }
        if (fast_name == "iswctype") {
            const std::uint32_t wc = Arg(0);
            const std::uint32_t cls = Arg(1);
            const bool ascii = wc <= 0x7fu;
            const unsigned char c = static_cast<unsigned char>(wc & 0xffu);
            bool match = false;
            if (ascii) {
                switch (cls) {
                case 1: match = std::isalnum(c) != 0; break;
                case 2: match = std::isalpha(c) != 0; break;
                case 3: match = c == ' ' || c == '\t'; break;
                case 4: match = std::iscntrl(c) != 0; break;
                case 5: match = std::isdigit(c) != 0; break;
                case 6: match = std::isgraph(c) != 0; break;
                case 7: match = std::islower(c) != 0; break;
                case 8: match = std::isprint(c) != 0; break;
                case 9: match = std::ispunct(c) != 0; break;
                case 10: match = std::isspace(c) != 0; break;
                case 11: match = std::isupper(c) != 0; break;
                case 12: match = std::isxdigit(c) != 0; break;
                default: break;
                }
            }
            Ret(match ? 1u : 0u);
            return true;
        }
        if (fast_name == "towlower" || fast_name == "towupper") {
            const std::uint32_t wc = Arg(0);
            if (wc <= 0x7fu) {
                const unsigned char c = static_cast<unsigned char>(wc);
                Ret(static_cast<std::uint32_t>(fast_name == "towlower" ? std::tolower(c) : std::toupper(c)));
            } else {
                Ret(wc);
            }
            return true;
        }
        if (fast_name == "wcslen") {
            const std::uint32_t src = Arg(0);
            std::uint32_t length = 0;
            if (src) {
                while (length < 0x100000u && Fits(src + length * 4u, 4)) {
                    std::uint32_t wc = 0;
                    Read32(memory_, src + length * 4u, wc);
                    if (wc == 0) break;
                    ++length;
                }
            }
            Ret(length);
            return true;
        }
        if (fast_name == "wcscoll") {
            std::uint32_t a = Arg(0), b = Arg(1);
            for (std::size_t i = 0; i < 0x100000u; ++i) {
                std::uint32_t ca = 0, cb = 0;
                if (!Read32(memory_, a + static_cast<std::uint32_t>(i * 4u), ca) ||
                    !Read32(memory_, b + static_cast<std::uint32_t>(i * 4u), cb)) {
                    Ret(0); return true;
                }
                if (ca != cb) { Ret(static_cast<std::uint32_t>(ca < cb ? -1 : 1)); return true; }
                if (ca == 0) { Ret(0); return true; }
            }
            Ret(0);
            return true;
        }
        if (fast_name == "wcsxfrm") {
            const std::uint32_t dst = Arg(0), src = Arg(1), count = Arg(2);
            std::uint32_t length = 0;
            while (length < 0x100000u) {
                std::uint32_t wc = 0;
                if (!Read32(memory_, src + length * 4u, wc) || wc == 0) break;
                ++length;
            }
            if (dst && count) {
                const std::uint32_t copy_count = std::min(length, count - 1u);
                for (std::uint32_t i = 0; i < copy_count; ++i) {
                    std::uint32_t wc = 0;
                    Read32(memory_, src + i * 4u, wc);
                    Write32(memory_, dst + i * 4u, wc);
                }
                Write32(memory_, dst + copy_count * 4u, 0u);
            }
            Ret(length);
            return true;
        }
        if (fast_name == "wcsftime") {
            // Locale facet construction only needs this symbol to be callable.
            // Full date formatting is deferred until real gameplay requests it.
            if (Arg(0) && Arg(1) > 0 && Fits(Arg(0), 4)) Write32(memory_, Arg(0), 0u);
            Ret(0);
            return true;
        }
        if (fast_name == "__system_property_get") {
            const std::string property = ReadCString(Arg(0), 256);
            std::string value;
            if (!pv7::compat::GetSystemProperty(property, value)) {
                static const std::unordered_map<std::string, std::string> properties = {
                    {"ro.build.version.sdk", "35"},
                    {"ro.build.version.release", "15"},
                    {"ro.build.version.codename", "REL"},
                    {"ro.product.cpu.abi", "armeabi-v7a"},
                    {"ro.product.cpu.abi2", "armeabi"},
                    {"ro.product.cpu.abilist", "arm64-v8a,armeabi-v7a,armeabi"},
                    {"ro.product.cpu.abilist32", "armeabi-v7a,armeabi"},
                    {"ro.product.cpu.abilist64", "arm64-v8a"},
                    {"ro.product.manufacturer", "Google"},
                    {"ro.product.brand", "google"},
                    {"ro.product.model", "ProjectV7"},
                    {"ro.product.device", "pv7"},
                    {"ro.hardware", "goldfish"},
                    {"ro.kernel.qemu", "1"},
                    {"ro.debuggable", "0"},
                    {"ro.secure", "1"}
                };
                const auto found = properties.find(property);
                if (found != properties.end()) value = found->second;
            }
            if (Arg(1) != 0 && !WriteCString(Arg(1), value, 92)) return false;
            if (file_trace.size() < 32) file_trace.push_back("property " + property + "=" + value);
            Ret(static_cast<std::uint32_t>(value.size()));
            return true;
        }
        if (fast_name == "sysconf") {
            const std::uint32_t key = Arg(0);
            if (key == 39) Ret(4096);             // _SC_PAGESIZE on bionic/glibc
            else if (key == 84) Ret(4);           // _SC_NPROCESSORS_ONLN
            else if (key == 2) Ret(100);           // _SC_CLK_TCK
            else Ret(4);
            return true;
        }
        if (fast_name == "getdtablesize") {
#if !defined(_WIN32)
            const long limit = ::sysconf(_SC_OPEN_MAX);
            Ret(limit > 0 ? static_cast<std::uint32_t>(std::min<long>(limit, 0x7fffffffL)) : 1024u);
#else
            Ret(1024u);
#endif
            return true;
        }
        if (fast_name == "getpid") { Ret(1234); return true; }
        if (fast_name == "gettid") { Ret(current_thread_id_); return true; }
        if (fast_name == "getuid" || fast_name == "geteuid" || fast_name == "getresuid") { Ret(1000); return true; }
        if (fast_name == "nanosleep") {
            // Old Unity uses nanosleep for main-thread pacing. A pure no-op makes
            // some render paths spin far faster than the original Android runtime.
            // Preserve cooperative waits on guest workers, but give the guest main
            // thread a bounded host-time sleep. The 1 ms cap keeps legacy callers
            // from freezing the wrapper when they pass unexpectedly large values.
            if (current_thread_id_ == 1) {
                const std::uint32_t request = Arg(0);
                if (!request || !Fits(request, 8u)) { Ret(0xffffffffu); return true; }
                std::uint32_t seconds = 0, nanoseconds = 0;
                Read32(memory_, request, seconds);
                Read32(memory_, request + 4u, nanoseconds);
                nanoseconds = std::min<std::uint32_t>(nanoseconds, 999999999u);
                std::uint64_t total_ns = static_cast<std::uint64_t>(seconds) * 1000000000ull + nanoseconds;
                total_ns = std::min<std::uint64_t>(total_ns, 1000000ull);
                if (total_ns != 0) std::this_thread::sleep_for(std::chrono::nanoseconds(total_ns));
                if (Arg(1) && Fits(Arg(1), 8u)) {
                    Write32(memory_, Arg(1), 0u);
                    Write32(memory_, Arg(1) + 4u, 0u);
                }
                Ret(0);
                if (jit && allow_main_thread_cooperative_yield) {
                    // Sleeping inside a translated render call must let queued guest
                    // workers run. Resume after this completed SVC, as usleep does.
                    const auto stub = name_to_stub_.find(name);
                    if (stub != name_to_stub_.end() && jit->Regs()[15] == stub->second)
                        jit->Regs()[15] = stub->second + 4u;
                    cooperative_yield_requested_ = true;
                    jit->HaltExecution();
                }
                return true;
            }
#if defined(PROJECTV7_TEST_WORKER_NANOSLEEP_DEADLINES) && PROJECTV7_TEST_WORKER_NANOSLEEP_DEADLINES
            // Diagnostic A/B: preserve the guest worker sleep duration without
            // blocking the native host thread that schedules all guest workers.
            // The bounded 25 ms deadline preserves the original observed 20 ms
            // guest worker wait, while capping longer requests. OFF is unchanged.
            std::uint64_t requested_ns = 0u;
            const std::uint32_t request = Arg(0);
            if (request != 0u && Fits(request, 8u)) {
                std::uint32_t seconds = 0u, nanoseconds = 0u;
                Read32(memory_, request, seconds);
                Read32(memory_, request + 4u, nanoseconds);
                if (nanoseconds < 1000000000u) {
                    requested_ns = static_cast<std::uint64_t>(seconds) * 1000000000ull + nanoseconds;
                    const std::uint64_t bounded_ns = std::min<std::uint64_t>(requested_ns, 25000000ull);
                    if (bounded_ns != 0u)
                        guest_nanosleep_deadlines_[current_thread_id_] =
                            std::chrono::steady_clock::now() + std::chrono::nanoseconds(bounded_ns);
                }
            }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (guest_nanosleep_trace_count_[current_thread_id_]++ < 8u) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SLEEP worker-nanosleep tid=%u requested_ns=%llu capped_ns=%llu",
                                    current_thread_id_, static_cast<unsigned long long>(requested_ns),
                                    static_cast<unsigned long long>(std::min<std::uint64_t>(requested_ns, 25000000ull)));
            }
#endif
#endif
            Ret(0);
            if (jit) {
                // Dynarmic stops at the SVC instruction itself when the callback
                // requests a cooperative halt. This call has already completed, so
                // resume at the thunk's BX LR instead of re-entering nanosleep forever.
                const auto stub = name_to_stub_.find(name);
                if (stub != name_to_stub_.end() && jit->Regs()[15] == stub->second)
                    jit->Regs()[15] = stub->second + 4u;
                cooperative_yield_requested_ = true;
                jit->HaltExecution();
            }
            return true;
        }
        if (fast_name == "sched_yield" || fast_name == "sleep" || fast_name == "usleep") {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (fast_name == "usleep" && guest_sleep_log_counts_[current_thread_id_]++ < 4u) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SLEEP usleep thread=%u usec=%u sp=0x%08x lr=0x%08x pc=0x%08x",
                                    current_thread_id_, Arg(0), jit ? jit->Regs()[13] : 0u,
                                    jit ? jit->Regs()[14] : 0u, jit ? jit->Regs()[15] : 0u);
            }
#endif
            Ret(0);
            if (jit && (current_thread_id_ != 1 || allow_main_thread_cooperative_yield)) {
                // The SVC callback halts before the thunk's BX LR executes. Advance
                // to that return instruction so the next quantum continues past the
                // completed sleep/yield call instead of invoking it again forever.
                const auto stub = name_to_stub_.find(name);
                if (stub != name_to_stub_.end() && jit->Regs()[15] == stub->second)
                    jit->Regs()[15] = stub->second + 4u;
                cooperative_yield_requested_ = true;
                jit->HaltExecution();
            }
            return true;
        }
#if !PROJECTV7_FAST_SYMBOL_DISPATCH
        if (DispatchClocks(name)) return true;
#endif
        if (fast_name == "mmap") {
            const std::uint32_t requested = Arg(0);
            const std::size_t length = Arg(1);
            const std::uint32_t prot = Arg(2);
            const std::uint32_t flags = Arg(3);
            const int fd = static_cast<int>(Arg(4));
            const std::uint32_t offset = Arg(5);
            if (length == 0 || length > UINT32_MAX) { SetErrno(22); Ret(0xffffffffu); return true; }

            // Linux/Android MAP_FIXED (0x10) requires mmap() to return exactly
            // the requested address, replacing the existing mapping there. Boehm GC
            // relies on this when it remaps portions of its heap as PROT_NONE. The
            // guest address space is already backed by our flat memory vector, so
            // honor the fixed address in-place instead of consuming a new heap block.
            std::uint32_t ptr = 0;
            if ((flags & 0x10u) != 0u && requested != 0u) {
                if ((requested & 0xfffu) != 0u || !Fits(requested, length)) {
                    SetErrno((requested & 0xfffu) != 0u ? 22u : 12u);
                    Ret(0xffffffffu);
                    return true;
                }
                ptr = requested;
                std::memset(memory_.data() + ptr, 0, length);
            } else {
                ptr = Allocate(AlignUp(static_cast<std::uint32_t>(length), 4096), 4096);
                if (!ptr) { SetErrno(12); Ret(0xffffffffu); return true; }
            }
            if (file_trace.size() < 32) {
                std::ostringstream line;
                line << "mmap req=0x" << std::hex << requested << " len=0x" << length
                     << " prot=0x" << prot << " flags=0x" << flags << std::dec
                     << " fd=" << fd << " off=" << offset << " -> guest=0x" << std::hex << ptr << std::dec;
                file_trace.push_back(line.str());
            }
#if !defined(_WIN32)
            if (fd >= 0) {
                std::size_t copied = 0;
                while (copied < length) {
                    const ssize_t rc = ::pread(fd,
                                               memory_.data() + ptr + copied,
                                               length - copied,
                                               static_cast<off_t>(offset) + static_cast<off_t>(copied));
                    if (rc < 0) { SetErrno(static_cast<std::uint32_t>(errno)); Ret(0xffffffffu); return true; }
                    if (rc == 0) break;
                    copied += static_cast<std::size_t>(rc);
                }
                if (file_trace.size() < 32) {
                    std::ostringstream line;
                    line << "mmap fd=" << fd << " offset=" << offset << " length=" << length
                         << " -> guest=0x" << std::hex << ptr << std::dec << " copied=" << copied;
                    file_trace.push_back(line.str());
                }
            }
#endif
            SetErrno(0);
            Ret(ptr);
            return true;
        }
        if (fast_name == "mprotect") {
            const std::uint32_t address = Arg(0);
            const std::uint32_t length = Arg(1);
            const std::uint32_t prot = Arg(2);
            last_mprotect_address = address;
            last_mprotect_length = length;
            last_mprotect_prot = prot;
            if (jit) {
                const std::uint32_t fp = jit->Regs()[11];
                if (fp && Fits(fp, 4)) last_mprotect_caller_lr = Read<std::uint32_t>(fp);
            }
            if (file_trace.size() < 32) {
                std::ostringstream line;
                line << "mprotect addr=0x" << std::hex << address << " len=0x" << length
                     << " prot=0x" << prot << std::dec;
                file_trace.push_back(line.str());
            }
            if ((prot & 0x4u) != 0) { // PROT_EXEC
                ++executable_mprotect_calls;
                ++total_executable_mprotect_calls;
                if (first_exec_region == 0) {
                    first_exec_region = address;
                    first_exec_region_size = length;
                }
                // Only executable mappings need Dynarmic code-cache invalidation, and it
                // must happen after jit.Run() returns rather than recursively in the callback.
                pending_cacheflush = true;
                pending_cacheflush_start = address;
                pending_cacheflush_size = length;
                if (jit) jit->HaltExecution();
            }
            Ret(0);
            return true;
        }
        if (fast_name == "munmap") {
            // mmap() allocations are tracked by their returned base address, so release
            // the whole mapping when the legacy runtime unmaps it. The length is advisory
            // here because this compatibility layer does not support partial unmaps yet.
            FreeAllocation(Arg(0));
            Ret(0);
            return true;
        }
        if (fast_name == "madvise") { Ret(0); return true; }
        if (fast_name == "mremap") {
            const std::uint32_t old_ptr = Arg(0);
            const std::size_t old_size = Arg(1);
            const std::size_t new_size = Arg(2);
            const std::uint32_t fresh = Allocate(new_size, 4096);
            if (fresh && old_ptr && Fits(old_ptr, std::min(old_size, new_size)))
                std::memcpy(memory_.data() + fresh, memory_.data() + old_ptr,
                            std::min(old_size, new_size));
            if (fresh) FreeAllocation(old_ptr);
            Ret(fresh ? fresh : 0xffffffffu);
            return true;
        }
        if (fast_name == "pthread_once") {
            const std::uint32_t control = Arg(0);
            const std::uint32_t initializer = Arg(1);
            if (!control || !initializer) {
                Ret(22u); // EINVAL
                return true;
            }
            if (pthread_once_done_[control]) {
                Ret(0);
                return true;
            }
            pthread_once_frames_.push_back({control, jit->Regs()[14]});
            if (Fits(control, 4)) Write32(memory_, control, 1u);
            ++pthread_once_callbacks;
            jit->Regs()[0] = 0;
            jit->Regs()[1] = 0;
            jit->Regs()[2] = 0;
            jit->Regs()[3] = 0;
            jit->Regs()[14] = kPthreadOnceReturnStub;
            jit->Regs()[15] = initializer & ~1u;
            jit->SetCpsr((initializer & 1u) ? 0x20u : 0u);
            pending_guest_callback = true;
            jit->HaltExecution();
            return true;
        }
        if (fast_name == "pthread_create") {
            const std::uint32_t thread_id = next_thread_id_++;
            if (Arg(0)) Write32(memory_, Arg(0), thread_id);
            guest_thread_launches_.push_back({thread_id, Arg(2), Arg(3), false});
            guest_signal_masks_[thread_id] = guest_signal_masks_[current_thread_id_];
            const std::uint32_t thread_start = Arg(2);
            const std::uint32_t thread_arg = Arg(3);
            std::uint32_t wrapped_arg = 0u;
            std::uint32_t wrapped_start = 0u;
            std::uint32_t wrapped_name_ptr = 0u;
            std::string wrapped_name;
            // Unity 4.x commonly creates native workers through its generic thread
            // trampoline at 0x01312574. The trampoline stores the real callback and
            // callback argument at +20/+16 and the prctl(PR_SET_NAME) string at +32.
            if (thread_start == 0x01312574u && Fits(thread_arg + 36u, 4u)) {
                Read32(memory_, thread_arg + 16u, wrapped_arg);
                Read32(memory_, thread_arg + 20u, wrapped_start);
                Read32(memory_, thread_arg + 32u, wrapped_name_ptr);
                wrapped_name = ReadCString(wrapped_name_ptr, 96u);
                if (!wrapped_name.empty()) guest_thread_names_[thread_id] = wrapped_name;
                if (wrapped_name == "UnityPreload") unity_preload_thread_id_ = thread_id;
            }
#if defined(__ANDROID__)
            if (g_thread_trace_count.fetch_add(1) < 120u) {
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7THREAD create creator=%u id=%u start=0x%08x arg=0x%08x wrapped_start=0x%08x wrapped_arg=0x%08x name='%s'",
                                    current_thread_id_, thread_id, thread_start, thread_arg,
                                    wrapped_start, wrapped_arg, wrapped_name.c_str());
            }
#endif
            if (jit) {
                // Let the newly-created guest thread reach its initial wait point
                // before the creator can signal it. On a real OS pthread_create
                // makes the child runnable immediately; delaying first execution
                // until a later main-thread wait can lose legitimate cond signals.
                // pthread_create itself has completed. Preserve Dynarmic's advanced
                // guest PC while giving the newly-runnable child a scheduling point.
                cooperative_yield_requested_ = true;
                jit->HaltExecution();
            }
            if (file_trace.size() < 32) {
                std::ostringstream line;
                line << "pthread_create id=" << thread_id
                     << " start=0x" << std::hex << Arg(2)
                     << " arg=0x" << Arg(3) << std::dec;
                file_trace.push_back(line.str());
            }
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_getschedparam") {
            if (Arg(1) != 0 && !Write32(memory_, Arg(1), 0u)) return false; // SCHED_OTHER
            if (Arg(2) != 0 && !Write32(memory_, Arg(2), 0u)) return false; // sched_priority
            Ret(0);
            return true;
        }
        if (fast_name == "sched_get_priority_max" || fast_name == "sched_get_priority_min") {
            const int policy = static_cast<int>(Arg(0));
            if (policy == 1 || policy == 2) Ret(fast_name == "sched_get_priority_max" ? 99u : 1u);
            else Ret(0);
            return true;
        }
        auto block_current_sync_call = [&](const std::string& symbol) -> bool {
            if (!jit) return false;
            const auto stub = name_to_stub_.find(symbol);
            if (stub == name_to_stub_.end()) return false;
            const auto thread = std::find_if(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                                             [&](const GuestThreadLaunch& item) { return item.id == current_thread_id_; });
            if (thread != guest_thread_launches_.end()) {
                thread->wait_object = Arg(0);
                thread->wait_aux = 0;
                thread->wait_owner = 0;
                if (symbol == "pthread_mutex_lock") {
                    thread->state = GuestThreadState::BlockedMutex;
                    const auto mutex = guest_mutexes_.find(Arg(0));
                    if (mutex != guest_mutexes_.end()) {
                        thread->wait_owner = mutex->second.owner;
                        if (mutex->second.owner != 0u && mutex->second.owner != current_thread_id_)
                            preferred_guest_thread_id_ = mutex->second.owner;
                    }
                } else if (symbol == "pthread_cond_wait" || symbol == "pthread_cond_timedwait" ||
                           symbol == "pthread_cond_timedwait_monotonic_np") {
                    thread->state = GuestThreadState::BlockedCond;
                    thread->wait_aux = Arg(1);
                } else if (symbol == "sem_wait" || symbol == "sem_timedwait") {
                    thread->state = GuestThreadState::BlockedSemaphore;
                } else if (symbol == "pthread_join") {
                    thread->state = GuestThreadState::BlockedJoin;
                } else if (symbol == "sigsuspend") {
                    thread->state = GuestThreadState::BlockedSignal;
                }
            }
            jit->Regs()[15] = stub->second;
            cooperative_yield_requested_ = true;
            jit->HaltExecution();
            return true;
        };
        auto make_wait_key = [&](std::uint32_t object) -> std::uint64_t {
            return (static_cast<std::uint64_t>(current_thread_id_) << 32) | object;
        };
        auto decode_deadline = [&](std::uint32_t guest_timespec,
                                   std::chrono::system_clock::time_point& out) -> bool {
            if (!guest_timespec || !Fits(guest_timespec, 8u)) return false;
            std::uint32_t seconds = 0;
            std::uint32_t nanoseconds = 0;
            if (!Read32(memory_, guest_timespec, seconds) ||
                !Read32(memory_, guest_timespec + 4u, nanoseconds)) return false;
            if (nanoseconds >= 1000000000u) return false;
            const auto duration = std::chrono::duration_cast<std::chrono::system_clock::duration>(
                std::chrono::seconds(seconds) + std::chrono::nanoseconds(nanoseconds));
            out = std::chrono::system_clock::time_point(duration);
            return true;
        };

        if (fast_name == "pthread_exit") {
            Ret(0);
            if (current_thread_id_ != 1 && jit) {
                thread_exit_requested_ = true;
                jit->HaltExecution();
            }
            return true;
        }
        if (fast_name == "pthread_mutexattr_init") {
            guest_mutex_attr_types_[Arg(0)] = 0u;
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_mutexattr_destroy") {
            guest_mutex_attr_types_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_mutexattr_settype") {
            guest_mutex_attr_types_[Arg(0)] = Arg(1);
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_mutex_init") {
            GuestMutexState state{};
            const auto attr = guest_mutex_attr_types_.find(Arg(1));
            if (Arg(1) != 0 && attr != guest_mutex_attr_types_.end()) state.type = attr->second;
            guest_mutexes_[Arg(0)] = state;
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_mutex_destroy") {
            guest_mutexes_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_mutex_lock" || fast_name == "pthread_mutex_trylock") {
            if (Arg(0) == 0) { Ret(22u); return true; } // EINVAL
            auto& mutex = guest_mutexes_[Arg(0)];
            if (mutex.owner == 0) {
                mutex.owner = current_thread_id_;
                mutex.recursion = 1;
                if (fast_name == "pthread_mutex_trylock") {
                    const std::uint64_t trylock_key =
                        (static_cast<std::uint64_t>(current_thread_id_) << 32) | Arg(0);
                    guest_trylock_spin_counts_.erase(trylock_key);
                }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                mutex.acquire_lr = jit ? jit->Regs()[14] : 0u;
                mutex.acquire_sp = jit ? jit->Regs()[13] : 0u;
                mutex.acquire_pc = jit ? jit->Regs()[15] : 0u;
                mutex.acquire_caller_lr = 0u;
                mutex.acquire_caller2_lr = 0u;
                if (mutex.acquire_sp != 0u && Fits(mutex.acquire_sp + 4u, 4u))
                    Read32(memory_, mutex.acquire_sp + 4u, mutex.acquire_caller_lr);
                if (mutex.acquire_sp != 0u && Fits(mutex.acquire_sp + 12u, 4u))
                    Read32(memory_, mutex.acquire_sp + 12u, mutex.acquire_caller2_lr);
                RecordSyncEvent("lock", Arg(0));
#endif
                Ret(0);
                return true;
            }
            if (mutex.owner == current_thread_id_ && mutex.type == 1u) {
                ++mutex.recursion;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                RecordSyncEvent("relock", Arg(0), mutex.recursion);
#endif
                Ret(0);
                return true;
            }
            if (fast_name == "pthread_mutex_trylock") {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                const std::uint64_t trylock_key =
                    (static_cast<std::uint64_t>(current_thread_id_) << 32) | Arg(0);
                const std::uint32_t trylock_busy_count = guest_trylock_busy_log_counts_[trylock_key]++;
                if (trylock_busy_count < 4u) {
                    const auto owner = std::find_if(
                        guest_thread_launches_.begin(), guest_thread_launches_.end(),
                        [&](const GuestThreadLaunch& item) { return item.id == mutex.owner; });
                    std::uint32_t raw_mutex = 0;
                    Read32(memory_, Arg(0), raw_mutex);
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                        "PV7MUTEX trylock-busy thread=%u mutex=0x%08x owner=%u recursion=%u type=%u raw=0x%08x owner_started=%s owner_finished=%s owner_pc=0x%08x acquire_pc=0x%08x acquire_lr=0x%08x acquire_sp=0x%08x acquire_caller_lr=0x%08x acquire_caller2_lr=0x%08x lr=0x%08x",
                                        current_thread_id_, Arg(0), mutex.owner, mutex.recursion, mutex.type,
                                        raw_mutex,
                                        owner != guest_thread_launches_.end() && owner->started ? "YES" : "NO",
                                        owner != guest_thread_launches_.end() && owner->finished ? "YES" : "NO",
                                        owner != guest_thread_launches_.end() ? owner->regs[15] : 0u,
                                        mutex.acquire_pc, mutex.acquire_lr, mutex.acquire_sp, mutex.acquire_caller_lr,
                                        mutex.acquire_caller2_lr, jit ? jit->Regs()[14] : 0u);
                    if (trylock_busy_count == 0u) {
                        const auto owner_sync = guest_thread_sync_traces_.find(mutex.owner);
                        if (owner_sync != guest_thread_sync_traces_.end()) {
                            const std::size_t start = owner_sync->second.size() > 32u
                                ? owner_sync->second.size() - 32u : 0u;
                            for (std::size_t i = start; i < owner_sync->second.size(); ++i) {
                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                    "PV7MUTEX owner-prior[%zu]=%s",
                                                    i - start, owner_sync->second[i].c_str());
                            }
                        }
                    }
                }
#endif
                Ret(16u); // EBUSY
                if (jit) {
                    // A real pthread_mutex_trylock returns immediately. Forcing a
                    // cooperative scheduler pass on every EBUSY turns Unity spin loops
                    // into thousands of expensive worker quanta. Spin cheaply for a
                    // short bounded interval, then let the lock owner run.
                    const std::uint64_t trylock_key =
                        (static_cast<std::uint64_t>(current_thread_id_) << 32) | Arg(0);
                    std::uint32_t& spin_count = guest_trylock_spin_counts_[trylock_key];
                    ++spin_count;
                    if (mutex.owner != 0u && mutex.owner != current_thread_id_) {
                        preferred_guest_thread_id_ = mutex.owner;
                        trylock_preferred_guest_thread_id_ = mutex.owner;
                    }
                    if ((spin_count & 31u) == 0u) {
                        if (current_thread_id_ == 1u && mutex.owner == unity_preload_thread_id_ &&
                            jit->Regs()[14] == kCnrPreloadTrylockCaller) {
                            main_thread_wait_mutex_ = Arg(0);
                        }
                        cooperative_yield_requested_ = true;
                        jit->HaltExecution();
                    }
                }
                return true;
            }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            const std::uint64_t block_key = (static_cast<std::uint64_t>(current_thread_id_) << 32) | Arg(0);
            if (!guest_mutex_block_logged_[block_key]) {
                guest_mutex_block_logged_[block_key] = true;
                std::uint32_t raw_mutex = 0;
                Read32(memory_, Arg(0), raw_mutex);
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7MUTEX block thread=%u mutex=0x%08x owner=%u recursion=%u type=%u raw=0x%08x lr=0x%08x",
                                    current_thread_id_, Arg(0), mutex.owner, mutex.recursion, mutex.type,
                                    raw_mutex, jit ? jit->Regs()[14] : 0u);
                const auto sync_trace = guest_thread_sync_traces_.find(current_thread_id_);
                if (sync_trace != guest_thread_sync_traces_.end()) {
                    const std::size_t start = sync_trace->second.size() > 24 ? sync_trace->second.size() - 24 : 0;
                    for (std::size_t i = start; i < sync_trace->second.size(); ++i) {
                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                            "PV7MUTEX prior[%zu]=%s",
                                            i - start, sync_trace->second[i].c_str());
                    }
                }
            }
#endif
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            RecordSyncEvent("block", Arg(0), mutex.owner);
#endif
#if defined(__ANDROID__)
            const std::uint64_t suspect_key = (static_cast<std::uint64_t>(current_thread_id_) << 32) | Arg(0);
            if (current_thread_id_ == 1u && suspect_mutex_logged_.size() < 64u && !suspect_mutex_logged_[suspect_key]) {
                suspect_mutex_logged_[suspect_key] = true;
                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                    "PV7MUTEX main-block thread=%u mutex=%08x owner=%u recursion=%u type=%u lr=%08x sp=%08x r4=%08x r5=%08x r6=%08x r7=%08x r8=%08x r9=%08x r10=%08x r11=%08x",
                    current_thread_id_, Arg(0), mutex.owner, mutex.recursion, mutex.type,
                    jit->Regs()[14], jit->Regs()[13], jit->Regs()[4], jit->Regs()[5], jit->Regs()[6],
                    jit->Regs()[7], jit->Regs()[8], jit->Regs()[9], jit->Regs()[10], jit->Regs()[11]);
            }
#endif
            if (!block_current_sync_call(name)) return false;
            return true;
        }
        if (fast_name == "pthread_mutex_unlock") {
            auto found = guest_mutexes_.find(Arg(0));
            if (found == guest_mutexes_.end() || found->second.owner == 0) {
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                RecordSyncEvent("unlock-noop", Arg(0),
                                found == guest_mutexes_.end() ? 0xffffffffu : 0u);
#endif
                Ret(0);
                return true;
            }
            if (found->second.owner != current_thread_id_) {
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                RecordSyncEvent("unlock-EPERM", Arg(0), found->second.owner);
#endif
                Ret(1u); // EPERM
                return true;
            }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            RecordSyncEvent("unlock", Arg(0), found->second.owner);
#endif
            if (found->second.recursion > 1) --found->second.recursion;
            else {
                found->second.owner = 0;
                found->second.recursion = 0;
            }
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_cond_init") {
            guest_conds_[Arg(0)] = {};
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_cond_destroy") {
            guest_conds_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_cond_signal") {
            const std::uint32_t cond = Arg(0);
            bool signaled_waiter = false;
            for (auto& waiter : guest_cond_waits_) {
                if (static_cast<std::uint32_t>(waiter.first) == cond && !waiter.second.signaled) {
                    waiter.second.signaled = true;
                    signaled_waiter = true;
                    preferred_guest_thread_id_ = static_cast<std::uint32_t>(waiter.first >> 32);
                    break;
                }
            }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7COND signal thread=%u cond=0x%08x waiter=%s",
                                current_thread_id_, cond, signaled_waiter ? "YES" : "NO");
#endif
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_cond_broadcast") {
            const std::uint32_t cond = Arg(0);
            ++guest_conds_[cond].broadcast_generation;
            for (const auto& waiter : guest_cond_waits_) {
                if (static_cast<std::uint32_t>(waiter.first) == cond) {
                    preferred_guest_thread_id_ = static_cast<std::uint32_t>(waiter.first >> 32);
                    break;
                }
            }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7COND broadcast thread=%u cond=0x%08x generation=%llu",
                                current_thread_id_, cond,
                                static_cast<unsigned long long>(guest_conds_[cond].broadcast_generation));
#endif
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_cond_wait" || fast_name == "pthread_cond_timedwait" ||
            fast_name == "pthread_cond_timedwait_monotonic_np") {
            const std::uint32_t cond = Arg(0);
            const std::uint32_t mutex_address = Arg(1);
            const std::uint64_t wait_key = make_wait_key(cond);
            auto wait = guest_cond_waits_.find(wait_key);
            if (wait == guest_cond_waits_.end()) {
                GuestCondWaitState state{};
                state.broadcast_generation = guest_conds_[cond].broadcast_generation;
                state.mutex = mutex_address;
                state.timed = fast_name != "pthread_cond_wait";
                if (state.timed && !decode_deadline(Arg(2), state.deadline)) {
                    Ret(22u); // EINVAL
                    return true;
                }
                auto mutex = guest_mutexes_.find(mutex_address);
                if (mutex != guest_mutexes_.end() && mutex->second.owner == current_thread_id_) {
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                    RecordSyncEvent("cond-release", mutex_address, current_thread_id_);
#endif
                    mutex->second.owner = 0;
                    mutex->second.recursion = 0;
                }
                wait = guest_cond_waits_.emplace(wait_key, state).first;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                const auto mutex_now = guest_mutexes_.find(mutex_address);
                const std::uint32_t owner_now = mutex_now == guest_mutexes_.end() ? 0u : mutex_now->second.owner;
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7COND wait-enter thread=%u cond=0x%08x mutex=0x%08x owner_after_release=%u lr=0x%08x",
                                    current_thread_id_, cond, mutex_address, owner_now,
                                    jit ? jit->Regs()[14] : 0u);
#endif
            }

            auto& cond_state = guest_conds_[cond];
            const bool broadcasted = cond_state.broadcast_generation != wait->second.broadcast_generation;
            const bool signaled = wait->second.signaled;
            const bool timed_out = wait->second.timed &&
                                   std::chrono::system_clock::now() >= wait->second.deadline;
            if (!broadcasted && !signaled && !timed_out) {
                if (!block_current_sync_call(name)) return false;
                return true;
            }
            if (timed_out && !broadcasted && !signaled) {
                guest_cond_waits_.erase(wait);
                Ret(110u); // ETIMEDOUT
                return true;
            }

            auto& mutex = guest_mutexes_[mutex_address];
            if (mutex.owner != 0 && mutex.owner != current_thread_id_) {
                if (!block_current_sync_call(name)) return false;
                return true;
            }
            mutex.owner = current_thread_id_;
            mutex.recursion = 1;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            RecordSyncEvent("cond-reacquire", mutex_address, current_thread_id_);
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7COND wait-wake thread=%u cond=0x%08x mutex=0x%08x reason=%s",
                                current_thread_id_, cond, mutex_address,
                                broadcasted ? "broadcast" : (signaled ? "signal" : "timeout"));
#endif
            guest_cond_waits_.erase(wait);
            Ret(0);
            return true;
        }
        if (fast_name == "sem_init") {
            guest_semaphores_[Arg(0)] = static_cast<std::int64_t>(Arg(2));
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SYNC sem_init thread=%u sem=0x%08x value=%u",
                                current_thread_id_, Arg(0), Arg(2));
#endif
            Ret(0);
            return true;
        }
        if (fast_name == "sem_destroy") {
            guest_semaphores_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (fast_name == "sem_getvalue") {
            const auto found = guest_semaphores_.find(Arg(0));
            const std::uint32_t value = found == guest_semaphores_.end()
                                            ? 0u
                                            : static_cast<std::uint32_t>(std::max<std::int64_t>(0, found->second));
            if (Arg(1) != 0 && !Write32(memory_, Arg(1), value)) return false;
            Ret(0);
            return true;
        }
        if (fast_name == "sem_post") {
            auto& value = guest_semaphores_[Arg(0)];
            ++value;
            const auto waiter = std::find_if(
                guest_thread_launches_.begin(), guest_thread_launches_.end(),
                [&](const GuestThreadLaunch& item) {
                    return !item.finished && item.state == GuestThreadState::BlockedSemaphore &&
                           item.wait_object == Arg(0);
                });
            if (waiter != guest_thread_launches_.end()) preferred_guest_thread_id_ = waiter->id;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SYNC sem_post thread=%u sem=0x%08x value=%lld",
                                current_thread_id_, Arg(0), static_cast<long long>(value));
#endif
            Ret(0);
            return true;
        }
        if (fast_name == "sem_trywait") {
            auto& value = guest_semaphores_[Arg(0)];
            if (value > 0) {
                --value;
                Ret(0);
            } else {
                Write32(memory_, kBootstrapRuntimeBase, 11u); // EAGAIN
                Ret(0xffffffffu);
            }
            return true;
        }
        if (fast_name == "sem_wait" || fast_name == "sem_timedwait") {
            auto& value = guest_semaphores_[Arg(0)];
            const std::uint64_t wait_key = make_wait_key(Arg(0));
            if (value > 0) {
                --value;
                guest_sem_deadlines_.erase(wait_key);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                guest_sem_wait_logged_.erase(wait_key);
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SYNC %s acquired thread=%u sem=0x%08x remaining=%lld",
                                    name.c_str(), current_thread_id_, Arg(0), static_cast<long long>(value));
#endif
                Ret(0);
                return true;
            }
            if (fast_name == "sem_timedwait") {
                auto deadline = guest_sem_deadlines_.find(wait_key);
                if (deadline == guest_sem_deadlines_.end()) {
                    std::chrono::system_clock::time_point parsed{};
                    if (!decode_deadline(Arg(1), parsed)) {
                        Write32(memory_, kBootstrapRuntimeBase, 22u); // EINVAL
                        Ret(0xffffffffu);
                        return true;
                    }
                    deadline = guest_sem_deadlines_.emplace(wait_key, parsed).first;
                }
                if (std::chrono::system_clock::now() >= deadline->second) {
                    guest_sem_deadlines_.erase(deadline);
                    Write32(memory_, kBootstrapRuntimeBase, 110u); // ETIMEDOUT
                    Ret(0xffffffffu);
                    return true;
                }
            }
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (!guest_sem_wait_logged_[wait_key]) {
                guest_sem_wait_logged_[wait_key] = true;
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SYNC %s blocked thread=%u sem=0x%08x value=%lld lr=0x%08x",
                                    name.c_str(), current_thread_id_, Arg(0), static_cast<long long>(value),
                                    jit ? jit->Regs()[14] : 0u);
            }
#endif
            if (!block_current_sync_call(name)) return false;
            return true;
        }
        if (fast_name == "pthread_kill") {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL source=pthread_kill sender=%u target=%u signal=%u lr=0x%08x sp=0x%08x",
                                current_thread_id_, Arg(0), Arg(1), jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
#endif
            Ret(QueueGuestSignal(Arg(0), Arg(1)) ? 0u : 3u); // ESRCH
            return true;
        }
        if (fast_name == "pthread_join") {
            const std::uint32_t target = Arg(0);
            const auto thread = std::find_if(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                                             [&](const GuestThreadLaunch& item) { return item.id == target; });
            if (thread == guest_thread_launches_.end()) { Ret(3u); return true; } // ESRCH
            if (!thread->finished) {
                if (!block_current_sync_call(name)) return false;
                return true;
            }
            if (Arg(1) != 0) Write32(memory_, Arg(1), thread->regs[0]);
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_attr_init") {
            guest_pthread_attr_threads_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_attr_destroy") {
            guest_pthread_attr_threads_.erase(Arg(0));
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_attr_setstacksize" || fast_name == "pthread_attr_setdetachstate" ||
            fast_name == "pthread_attr_setschedparam" || fast_name == "pthread_attr_setschedpolicy" ||
            fast_name == "pthread_setschedparam" || fast_name == "pthread_detach") {
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_self") { Ret(current_thread_id_); return true; }
        if (fast_name == "pthread_equal") { Ret(Arg(0) == Arg(1) ? 1u : 0u); return true; }
        if (fast_name == "pthread_key_create") {
            const std::uint32_t key = next_tls_key_++;
            if (Arg(0)) Write32(memory_, Arg(0), key);
            Ret(0);
            return true;
        }
        if (fast_name == "pthread_key_delete") { tls_values_.erase(Arg(0)); Ret(0); return true; }
        if (fast_name == "pthread_setspecific") { tls_values_[Arg(0)] = Arg(1); Ret(0); return true; }
        if (fast_name == "pthread_getspecific") { Ret(tls_values_[Arg(0)]); return true; }
        if (fast_name == "pthread_getattr_np") {
            const std::uint32_t thread_id = Arg(0);
            const std::uint32_t attr = Arg(1);
            const bool known_thread = thread_id == 1u ||
                std::any_of(guest_thread_launches_.begin(), guest_thread_launches_.end(),
                            [&](const GuestThreadLaunch& item) { return item.id == thread_id; });
            if (!known_thread) { Ret(3u); return true; } // ESRCH
            if (attr) guest_pthread_attr_threads_[attr] = thread_id;
            Ret(0);
            return true;
        }
        if (fast_name == "setjmp" || fast_name == "sigsetjmp") {
            const std::uint32_t buffer = Arg(0);
            if (!jit || buffer == 0) {
                Ret(0);
                return true;
            }
            GuestJumpContext context{};
            context.regs = jit->Regs();
            context.cpsr = jit->Cpsr();
            context.resume_pc = jit->Regs()[14];
            context.thread_id = current_thread_id_;
            jump_contexts_[buffer] = context;
            Ret(0);
            return true;
        }
        if (fast_name == "longjmp" || fast_name == "siglongjmp") {
            const std::uint32_t buffer = Arg(0);
            const auto saved = jump_contexts_.find(buffer);
            if (!jit || saved == jump_contexts_.end()) {
                return false;
            }
            const std::uint32_t value = Arg(1) == 0 ? 1u : Arg(1);
            const GuestJumpContext context = saved->second;
            jit->Regs() = context.regs;
            jit->Regs()[0] = value;
            jit->Regs()[15] = context.resume_pc & ~1u;
            jit->SetCpsr((context.cpsr & ~0x20u) | ((context.resume_pc & 1u) ? 0x20u : 0u));
            pending_guest_callback = true;
            jit->HaltExecution();
            return true;
        }
        if (fast_name == "pthread_attr_getdetachstate") {
            if (Arg(1)) Write32(memory_, Arg(1), 0);
            Ret(0); return true;
        }
        if (fast_name == "pthread_attr_getstack") {
            std::uint32_t thread_id = current_thread_id_;
            if (const auto it = guest_pthread_attr_threads_.find(Arg(0)); it != guest_pthread_attr_threads_.end())
                thread_id = it->second;

            std::uint32_t stack_base = 0x06f00000u;
            std::uint32_t stack_size = 0x00100000u;
            if (thread_id >= 2u) {
                const std::uint64_t stack_offset =
                    static_cast<std::uint64_t>(thread_id - 2u) * kGuestThreadStackStride;
                if (stack_offset > static_cast<std::uint64_t>(kGuestThreadStackTop - kGuestThreadStackFloor)) {
                    Ret(22u); // EINVAL
                    return true;
                }
                const std::uint32_t stack_top =
                    kGuestThreadStackTop - static_cast<std::uint32_t>(stack_offset);
                stack_base = stack_top - kGuestThreadStackStride;
                stack_size = kGuestThreadStackStride;
            }
            if (Arg(1)) Write32(memory_, Arg(1), stack_base);
            if (Arg(2)) Write32(memory_, Arg(2), stack_size);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7STACK pthread_attr_getstack caller=%u target=%u attr=0x%08x base=0x%08x size=0x%08x sp=0x%08x",
                                current_thread_id_, thread_id, Arg(0), stack_base, stack_size,
                                jit ? jit->Regs()[13] : 0u);
#endif
            Ret(0);
            return true;
        }
        if (fast_name == "__pthread_cleanup_push" || fast_name == "__pthread_cleanup_pop") { Ret(0); return true; }
        if (fast_name == "dlopen") { Ret(1); return true; }
        if (fast_name == "dlclose") { Ret(0); return true; }
        if (fast_name == "dlerror") { Ret(0); return true; }
        if (fast_name == "dlsym") {
            const std::string symbol = ReadCString(Arg(1));
            const auto guest = exports_.find(symbol);
            if (guest != exports_.end()) Ret(guest->second.address);
            else {
                const auto thunk = name_to_stub_.find(symbol);
                Ret(thunk == name_to_stub_.end() ? 0u : thunk->second);
            }
            return true;
        }
        if (fast_name == "sprintf" || fast_name == "snprintf" || fast_name == "vsprintf" || fast_name == "vsnprintf" || fast_name == "vasprintf") {
            const bool allocate_result = fast_name == "vasprintf";
            const bool bounded = fast_name == "snprintf" || fast_name == "vsnprintf";
            const bool va_list_mode = fast_name == "vsprintf" || fast_name == "vsnprintf" || allocate_result;
            const std::uint32_t dst = Arg(0);
            const std::size_t capacity = bounded ? static_cast<std::size_t>(Arg(1)) : SIZE_MAX;
            const std::uint32_t fmt_addr = allocate_result ? Arg(1) : Arg(bounded ? 2 : 1);
            std::uint32_t va_ptr = va_list_mode ? (allocate_result ? Arg(2) : Arg(bounded ? 3 : 2)) : 0;
            unsigned word_index = bounded ? 3u : 2u;
            const std::string fmt = ReadCString(fmt_addr);
            auto next_word = [&]() -> std::uint32_t {
                if (!va_list_mode) return Arg(word_index++);
                std::uint32_t value = 0;
                if (!Read32(memory_, va_ptr, value)) return 0;
                va_ptr += 4;
                return value;
            };
            auto next_u64 = [&]() -> std::uint64_t {
                if (!va_list_mode && (word_index & 1u)) ++word_index;
                if (va_list_mode && (va_ptr & 7u)) va_ptr = AlignUp(va_ptr, 8);
                const std::uint64_t lo = next_word();
                const std::uint64_t hi = next_word();
                return lo | (hi << 32);
            };
            std::string out;
            for (std::size_t i = 0; i < fmt.size(); ++i) {
                if (fmt[i] != '%') { out.push_back(fmt[i]); continue; }
                if (++i >= fmt.size()) break;
                if (fmt[i] == '%') { out.push_back('%'); continue; }
                bool zero_pad = false;
                while (i < fmt.size() && (fmt[i] == '-' || fmt[i] == '+' || fmt[i] == ' ' || fmt[i] == '#' || fmt[i] == '0')) {
                    if (fmt[i] == '0') zero_pad = true;
                    ++i;
                }
                unsigned width = 0;
                while (i < fmt.size() && std::isdigit(static_cast<unsigned char>(fmt[i]))) width = width * 10u + static_cast<unsigned>(fmt[i++] - '0');
                int precision = -1;
                if (i < fmt.size() && fmt[i] == '.') {
                    ++i; precision = 0;
                    while (i < fmt.size() && std::isdigit(static_cast<unsigned char>(fmt[i]))) precision = precision * 10 + (fmt[i++] - '0');
                }
                bool wide = false;
                if (i < fmt.size() && (fmt[i] == 'l' || fmt[i] == 'z' || fmt[i] == 'j' || fmt[i] == 't')) {
                    const char length = fmt[i++];
                    wide = length == 'j' || (length == 'l' && i < fmt.size() && fmt[i] == 'l');
                    if (wide && i < fmt.size() && fmt[i] == 'l') ++i;
                } else if (i < fmt.size() && fmt[i] == 'h') {
                    ++i; if (i < fmt.size() && fmt[i] == 'h') ++i;
                }
                if (i >= fmt.size()) break;
                const char conv = fmt[i];
                std::string piece;
                if (conv == 's') {
                    piece = ReadCString(next_word());
                    if (precision >= 0 && piece.size() > static_cast<std::size_t>(precision)) piece.resize(static_cast<std::size_t>(precision));
                } else if (conv == 'c') {
                    piece.push_back(static_cast<char>(next_word() & 0xffu));
                } else if (conv == 'p') {
                    char buf[32]{};
                    std::snprintf(buf, sizeof(buf), "0x%08x", next_word());
                    piece = buf;
                } else if (conv == 'd' || conv == 'i' || conv == 'u' || conv == 'x' || conv == 'X' || conv == 'o') {
                    const std::uint64_t raw = wide ? next_u64() : next_word();
                    char buf[96]{};
                    if (conv == 'd' || conv == 'i') std::snprintf(buf, sizeof(buf), "%lld", static_cast<long long>(wide ? static_cast<std::int64_t>(raw) : static_cast<std::int32_t>(raw)));
                    else if (conv == 'u') std::snprintf(buf, sizeof(buf), "%llu", static_cast<unsigned long long>(wide ? raw : static_cast<std::uint32_t>(raw)));
                    else if (conv == 'x') std::snprintf(buf, sizeof(buf), "%llx", static_cast<unsigned long long>(wide ? raw : static_cast<std::uint32_t>(raw)));
                    else if (conv == 'X') std::snprintf(buf, sizeof(buf), "%llX", static_cast<unsigned long long>(wide ? raw : static_cast<std::uint32_t>(raw)));
                    else std::snprintf(buf, sizeof(buf), "%llo", static_cast<unsigned long long>(wide ? raw : static_cast<std::uint32_t>(raw)));
                    piece = buf;
                } else {
                    piece.push_back('%'); piece.push_back(conv);
                }
                if (width > piece.size()) out.append(width - piece.size(), zero_pad ? '0' : ' ');
                out += piece;
            }
            const std::size_t required = out.size();
            if (allocate_result) {
                const std::uint32_t allocated = Allocate(required + 1);
                if (!allocated || !dst || !WriteCString(allocated, out) || !Write32(memory_, dst, allocated)) {
                    Ret(0xffffffffu);
                    return true;
                }
            } else if (dst && (!bounded || capacity != 0)) {
                const std::size_t cap = bounded ? capacity : required + 1;
                WriteCString(dst, out, cap);
            }
            Ret(static_cast<std::uint32_t>(required));
            return true;
        }
#if defined(__ANDROID__)
        if (fast_name == "eglGetDisplay") {
            const EGLDisplay display = eglGetDisplay(EGL_DEFAULT_DISPLAY);
            const std::uint32_t guest_display = RegisterEglHandle(reinterpret_cast<std::uintptr_t>(display));
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7EGL eglGetDisplay -> guest=0x%08x host=%p",
                                guest_display, display);
            Ret(guest_display);
            return true;
        }
        if (fast_name == "eglInitialize") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            EGLint major = 0;
            EGLint minor = 0;
            const EGLBoolean ok = display != EGL_NO_DISPLAY
                ? eglInitialize(display, &major, &minor)
                : EGL_FALSE;
            if (ok == EGL_TRUE) {
                if (Arg(1)) Write32(memory_, Arg(1), static_cast<std::uint32_t>(major));
                if (Arg(2)) Write32(memory_, Arg(2), static_cast<std::uint32_t>(minor));
            }
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7EGL eglInitialize display=0x%08x ok=%d version=%d.%d",
                                Arg(0), ok == EGL_TRUE ? 1 : 0, major, minor);
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (fast_name == "eglChooseConfig") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const auto attributes = ReadEglAttribList(Arg(1));
            EGLint capacity = static_cast<EGLint>(Arg(3));
            capacity = std::max<EGLint>(0, std::min<EGLint>(capacity, 256));
            std::vector<EGLConfig> configs(static_cast<std::size_t>(capacity));
            EGLint count = 0;
            const EGLBoolean ok = display != EGL_NO_DISPLAY
                ? eglChooseConfig(display,
                                  attributes.empty() ? nullptr : attributes.data(),
                                  capacity > 0 ? configs.data() : nullptr,
                                  capacity,
                                  &count)
                : EGL_FALSE;
            if (Arg(4)) Write32(memory_, Arg(4), static_cast<std::uint32_t>(count));
            if (ok == EGL_TRUE && Arg(2) && capacity > 0) {
                const EGLint written = std::min(count, capacity);
                for (EGLint index = 0; index < written; ++index) {
                    Write32(memory_, Arg(2) + static_cast<std::uint32_t>(index * 4),
                            RegisterEglHandle(reinterpret_cast<std::uintptr_t>(configs[static_cast<std::size_t>(index)]), true));
                }
            }
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7EGL eglChooseConfig display=0x%08x ok=%d count=%d cap=%d attr_count=%zu first_guest=0x%08x",
                                Arg(0), ok == EGL_TRUE ? 1 : 0, count, capacity, attributes.size(),
                                (ok == EGL_TRUE && Arg(2) && count > 0 && capacity > 0)
                                    ? Read<std::uint32_t>(Arg(2)) : 0u);
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (fast_name == "eglGetConfigAttrib") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            std::uintptr_t config_host = 0;
            const bool has_config = LookupEglHandleValue(Arg(1), config_host);
            const EGLConfig config = reinterpret_cast<EGLConfig>(config_host);
            EGLint value = 0;
            const EGLBoolean ok = display != EGL_NO_DISPLAY && has_config
                ? eglGetConfigAttrib(display, config, static_cast<EGLint>(Arg(2)), &value)
                : EGL_FALSE;
            if (ok == EGL_TRUE && Arg(3)) Write32(memory_, Arg(3), static_cast<std::uint32_t>(value));
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7EGL eglGetConfigAttrib display=0x%08x config=0x%08x attr=0x%04x ok=%d value=%d",
                                Arg(0), Arg(1), Arg(2), ok == EGL_TRUE ? 1 : 0, value);
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (fast_name == "eglCreateWindowSurface") {
            // Once Java has asked the translated game loop to stop, the SurfaceView's
            // BufferQueue may already be on its way out. Never reconnect Unity to an
            // abandoned ANativeWindow while the current guest call is winding down.
            if (!IsGuestGameLoopRunning()) {
                Ret(0u);
                return true;
            }
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            std::uintptr_t config_host = 0;
            const bool has_config = LookupEglHandleValue(Arg(1), config_host);
            const EGLConfig config = reinterpret_cast<EGLConfig>(config_host);
            const auto attributes = ReadEglAttribList(Arg(3));
            ANativeWindow* window = Arg(2) == kGuestNativeWindowHandle
                ? static_cast<ANativeWindow*>(host_native_window_)
                : nullptr;
            const EGLSurface surface = display != EGL_NO_DISPLAY && has_config && window
                ? eglCreateWindowSurface(display, config, window,
                                         attributes.empty() ? nullptr : attributes.data())
                : EGL_NO_SURFACE;
            if (surface != EGL_NO_SURFACE) owned_egl_surfaces_[surface] = display;
            const std::uint32_t guest_surface = RegisterEglHandle(reinterpret_cast<std::uintptr_t>(surface));
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7EGL eglCreateWindowSurface display=0x%08x config=0x%08x native=0x%08x hostWindow=%p attrs=%zu -> guest=0x%08x host=%p",
                                Arg(0), Arg(1), Arg(2), window, attributes.size(), guest_surface, surface);
            Ret(guest_surface);
            return true;
        }
        if (fast_name == "eglCreateContext") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            std::uintptr_t config_host = 0;
            const bool has_config = LookupEglHandleValue(Arg(1), config_host);
            const EGLConfig config = reinterpret_cast<EGLConfig>(config_host);
            const EGLContext shared = reinterpret_cast<EGLContext>(LookupEglHandle(Arg(2)));
            const auto attributes = ReadEglAttribList(Arg(3));
            const EGLContext context = display != EGL_NO_DISPLAY && has_config
                ? eglCreateContext(display, config, shared,
                                   attributes.empty() ? nullptr : attributes.data())
                : EGL_NO_CONTEXT;
            if (context != EGL_NO_CONTEXT) owned_egl_contexts_[context] = display;
            const std::uint32_t guest_context = RegisterEglHandle(reinterpret_cast<std::uintptr_t>(context));
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7EGL eglCreateContext display=0x%08x config=0x%08x shared=0x%08x attrs=%zu -> guest=0x%08x host=%p",
                                Arg(0), Arg(1), Arg(2), attributes.size(), guest_context, context);
            Ret(guest_context);
            return true;
        }
        if (fast_name == "eglMakeCurrent") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLSurface draw = reinterpret_cast<EGLSurface>(LookupEglHandle(Arg(1)));
            const EGLSurface read = reinterpret_cast<EGLSurface>(LookupEglHandle(Arg(2)));
            const EGLContext context = reinterpret_cast<EGLContext>(LookupEglHandle(Arg(3)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY
                ? eglMakeCurrent(display, draw, read, context)
                : EGL_FALSE;
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7EGL eglMakeCurrent display=0x%08x draw=0x%08x read=0x%08x context=0x%08x ok=%d",
                                Arg(0), Arg(1), Arg(2), Arg(3), ok == EGL_TRUE ? 1 : 0);
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (fast_name == "eglSwapBuffers") {
            if (!IsGuestGameLoopRunning()) {
                Ret(0u);
                return true;
            }
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLSurface surface = reinterpret_cast<EGLSurface>(LookupEglHandle(Arg(1)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY && surface != EGL_NO_SURFACE
                ? eglSwapBuffers(display, surface)
                : EGL_FALSE;
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (fast_name == "eglDestroySurface") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLSurface surface = reinterpret_cast<EGLSurface>(LookupEglHandle(Arg(1)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY && surface != EGL_NO_SURFACE
                ? eglDestroySurface(display, surface)
                : EGL_FALSE;
            if (ok == EGL_TRUE) { owned_egl_surfaces_.erase(surface); UnregisterEglHandle(Arg(1)); }
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (fast_name == "eglDestroyContext") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLContext context = reinterpret_cast<EGLContext>(LookupEglHandle(Arg(1)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY && context != EGL_NO_CONTEXT
                ? eglDestroyContext(display, context)
                : EGL_FALSE;
            if (ok == EGL_TRUE) { owned_egl_contexts_.erase(context); UnregisterEglHandle(Arg(1)); }
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (fast_name == "eglTerminate") {
            const EGLDisplay display = reinterpret_cast<EGLDisplay>(LookupEglHandle(Arg(0)));
            const EGLBoolean ok = display != EGL_NO_DISPLAY ? eglTerminate(display) : EGL_FALSE;
            if (ok == EGL_TRUE) UnregisterEglHandle(Arg(0));
            Ret(ok == EGL_TRUE ? 1u : 0u);
            return true;
        }
        if (fast_name == "eglGetError") {
            Ret(static_cast<std::uint32_t>(eglGetError()));
            return true;
        }
        if (fast_name == "eglGetProcAddress") {
            // Never expose an arm64 host function pointer to the ARM32 guest. Return a
            // guest SVC thunk only for extension entry points implemented by this bridge.
            const std::string symbol = ReadCString(Arg(0), 256);
            const auto found = name_to_stub_.find(symbol);
            Ret(found == name_to_stub_.end() ? 0u : found->second);
            return true;
        }
#endif
        if (fast_name == "ANativeWindow_fromSurface") {
            Ret(host_native_window_ ? kGuestNativeWindowHandle : 0u);
            if (file_trace.size() < 64) {
                file_trace.push_back(host_native_window_
                    ? "ANativeWindow_fromSurface -> real host window"
                    : "ANativeWindow_fromSurface -> null (no host Surface)");
            }
            return true;
        }
        if (fast_name == "ANativeWindow_setBuffersGeometry") {
            int result = -1;
#if defined(__ANDROID__)
            const int32_t requested_width = static_cast<int32_t>(Arg(1));
            const int32_t requested_height = static_cast<int32_t>(Arg(2));
            const int32_t requested_format = static_cast<int32_t>(Arg(3));
            if (host_native_window_ && Arg(0) == kGuestNativeWindowHandle) {
                result = ANativeWindow_setBuffersGeometry(
                    static_cast<ANativeWindow*>(host_native_window_),
                    requested_width, requested_height, requested_format);
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7WINDOW setBuffersGeometry req=%dx%d fmt=%d result=%d hostNow=%dx%d",
                                    requested_width, requested_height, requested_format, result,
                                    ANativeWindow_getWidth(static_cast<ANativeWindow*>(host_native_window_)),
                                    ANativeWindow_getHeight(static_cast<ANativeWindow*>(host_native_window_)));
            }
#endif
            Ret(static_cast<std::uint32_t>(result));
            return true;
        }
        if (fast_name == "__android_log_print" || fast_name == "__android_log_vprint" ||
            fast_name == "printf" || fast_name == "vprintf" || fast_name == "fprintf" || fast_name == "vfprintf" ||
            fast_name == "puts" || fast_name == "putchar" || fast_name == "fputs" || fast_name == "fputc" ||
            fast_name == "fflush" || fast_name == "perror") {
            std::uint32_t text_arg = 0;
            if (fast_name == "printf" || fast_name == "vprintf" || fast_name == "puts" || fast_name == "perror") text_arg = Arg(0);
            else if (fast_name == "fprintf" || fast_name == "vfprintf" || fast_name == "fputs") text_arg = Arg(1);
            else if (fast_name == "__android_log_print" || fast_name == "__android_log_vprint") text_arg = Arg(2);
            if (text_arg) {
                const std::string text = ReadCString(text_arg);
                if (!text.empty()) {
                    std::string display_text = text;
                    if (text == "%s") {
                        std::vector<std::uint32_t> candidates;
                        const std::uint32_t arg3 = Arg(3);
                        if (arg3) candidates.push_back(arg3);
                        if (fast_name == "__android_log_vprint" && arg3) {
                            for (std::uint32_t offset = 0; offset <= 28u; offset += 4u) {
                                if (Fits(arg3 + offset, 4u))
                                    candidates.push_back(Read<std::uint32_t>(arg3 + offset));
                            }
                        }
                        for (const std::uint32_t candidate : candidates) {
                            if (!candidate) continue;
                            const std::string expanded = ReadCString(candidate);
                            if (expanded.empty()) continue;
                            if (expanded.find("initialization") != std::string::npos ||
                                expanded.find("[EGL]") != std::string::npos ||
                                expanded.find("failed") != std::string::npos) {
                                display_text = expanded;
                                break;
                            }
                            if (display_text == text && expanded.size() >= 4u && expanded.size() < 256u)
                                display_text = expanded;
                        }
                    }
                    // Mono's fatal diagnostics often arrive as `%s%s%s` or
                    // `  %s`. Decode *only* these simple string-only variadics
                    // from the original guest ARM EABI argument slots. Do not
                    // reinterpret unknown printf specifiers or dereference host
                    // pointers. This is logging only, not guest execution.
                    if (text != "%s" &&
                        (fast_name == "__android_log_print" || fast_name == "printf" ||
                         fast_name == "fprintf") && text.find("%s") != std::string::npos) {
                        std::uint32_t arg_index = fast_name == "__android_log_print" ? 3u :
                                                  fast_name == "fprintf" ? 2u : 1u;
                        std::string expanded;
                        bool valid_string_format = true;
                        bool did_expand = false;
                        for (std::size_t i = 0; i < text.size() && expanded.size() < 2048u; ++i) {
                            if (text[i] != '%') { expanded.push_back(text[i]); continue; }
                            if (i + 1u >= text.size()) { valid_string_format = false; break; }
                            const char spec = text[++i];
                            if (spec == '%') { expanded.push_back('%'); continue; }
                            if (spec != 's' || arg_index > 9u) {
                                valid_string_format = false;
                                break;
                            }
                            const std::uint32_t guest_ptr = Arg(arg_index++);
                            if (guest_ptr != 0u && Fits(guest_ptr, 1u)) {
                                std::string value = ReadCString(guest_ptr);
                                if (value.size() > 512u) value.resize(512u);
                                expanded += value;
                            } else {
                                expanded += guest_ptr == 0u ? "(null)" : "<invalid guest string>";
                            }
                            did_expand = true;
                        }
                        if (valid_string_format && did_expand) display_text = std::move(expanded);
                    }
                    if (diagnostic_formats.size() < 16) diagnostic_formats.push_back(name + ": " + display_text);
#if defined(__ANDROID__)
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC", "PV7GUESTLOG %s: %s", name.c_str(), display_text.c_str());
#if defined(PROJECTV7_MONO_FATAL_CONTEXT_TRACE) && PROJECTV7_MONO_FATAL_CONTEXT_TRACE
                    if (jit && (display_text.find("unknown opcode") != std::string::npos ||
                                display_text.find("unknown type") != std::string::npos ||
                                display_text.find("Assertion:") != std::string::npos)) {
                        const auto& regs = jit->Regs();
                        const std::uint32_t sp = regs[13];
                        __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                            "PV7MONOFATAL pc=%08x lr=%08x sp=%08x fp=%08x r0=%08x r1=%08x r2=%08x r3=%08x r4=%08x r5=%08x r6=%08x r7=%08x r8=%08x r9=%08x r10=%08x r12=%08x",
                                            regs[15], regs[14], sp, regs[11],
                                            regs[0], regs[1], regs[2], regs[3],
                                            regs[4], regs[5], regs[6], regs[7],
                                            regs[8], regs[9], regs[10], regs[12]);
                        if (Fits(sp, 48u)) {
                            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                "PV7MONOFATAL stack=%08x,%08x,%08x,%08x,%08x,%08x,%08x,%08x,%08x,%08x,%08x,%08x",
                                                Read<std::uint32_t>(sp), Read<std::uint32_t>(sp + 4u),
                                                Read<std::uint32_t>(sp + 8u), Read<std::uint32_t>(sp + 12u),
                                                Read<std::uint32_t>(sp + 16u), Read<std::uint32_t>(sp + 20u),
                                                Read<std::uint32_t>(sp + 24u), Read<std::uint32_t>(sp + 28u),
                                                Read<std::uint32_t>(sp + 32u), Read<std::uint32_t>(sp + 36u),
                                                Read<std::uint32_t>(sp + 40u), Read<std::uint32_t>(sp + 44u));
                        }
                        // Dev-only Mono ARM emitter frame recovery. Ghidra mapped
                        // mono_arch_output_basic_block's unknown-opcode log call
                        // to original ELF offset 0x00123a74 (return 0x00123a78).
                        // Scan the current guest stack once at fatal time. Do not
                        // patch JIT code, interrupt threads, or skip instructions.
                        static bool captured_emitter_frame = false;
                        if (!captured_emitter_frame &&
                            display_text.find("unknown opcode") != std::string::npos) {
                            captured_emitter_frame = true;
                            constexpr std::uint32_t kEmitterLogReturn = kMonoBase + 0x00123a78u;
                            constexpr std::uint32_t kGuestStackTop = 0x07000000u;
                            std::size_t hits = 0u;
                            if (sp < kGuestStackTop) {
                                for (std::uint32_t addr = sp;
                                     addr + 4u <= kGuestStackTop && addr - sp <= 0x3000u;
                                     addr += 4u) {
                                    if (!Fits(addr, 4u) ||
                                        Read<std::uint32_t>(addr) != kEmitterLogReturn)
                                        continue;
                                    ++hits;
                                    __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                        "PV7MONOFRAME return=%08x slot=%08x",
                                                        kEmitterLogReturn, addr);
                                    // At the gcc EABI logging call, the word
                                    // immediately before the saved return PC is
                                    // the caller's frame pointer. The offending
                                    // MonoInst* lives at [r11-0x18] in the
                                    // Ghidra-confirmed ARM emitter default arm.
                                    if (addr >= 4u && Fits(addr - 4u, 4u)) {
                                        const auto compiler_fp = Read<std::uint32_t>(addr - 4u);
                                        if (compiler_fp >= 0x474u && Fits(compiler_fp - 0x474u, 4u)) {
                                            const auto ins_ptr = Read<std::uint32_t>(compiler_fp - 0x18u);
                                            const auto local_src = Read<std::uint32_t>(compiler_fp - 0x474u);
                                            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                                "PV7MONOINST fp=%08x ins=%08x local474=%08x local01c=%08x",
                                                                compiler_fp, ins_ptr, local_src,
                                                                Read<std::uint32_t>(compiler_fp - 0x1cu));
                                            // The Mono ARM emitter's current instruction is
                                            // seeded from basic_block->code (+8), advanced via
                                            // current->next (+0x14), or set by an alternate
                                            // path. Read pointers once, only after the fatal.
                                            const std::uint32_t previous_inst =
                                                Read<std::uint32_t>(compiler_fp - 0x34u);
                                            const std::uint32_t bb =
                                                compiler_fp >= 0x484u
                                                    ? Read<std::uint32_t>(compiler_fp - 0x484u) : 0u;
                                            const std::uint32_t cfg =
                                                compiler_fp >= 0x480u
                                                    ? Read<std::uint32_t>(compiler_fp - 0x480u) : 0u;
                                            const std::uint32_t head =
                                                bb <= 0xfffffff7u && Fits(bb + 8u, 4u)
                                                    ? Read<std::uint32_t>(bb + 8u) : 0u;
                                            const std::uint32_t prev_next =
                                                previous_inst <= 0xffffffebu &&
                                                Fits(previous_inst + 0x14u, 4u)
                                                    ? Read<std::uint32_t>(previous_inst + 0x14u) : 0u;
                                            const std::uint32_t prev_opcode =
                                                Fits(previous_inst, 2u)
                                                    ? Read<std::uint16_t>(previous_inst) : 0u;
                                            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                                "PV7MONOLINK cfg=%08x bb=%08x head=%08x prev=%08x prev_opcode=%04x prev_next=%08x current=%08x",
                                                                cfg, bb, head, previous_inst,
                                                                prev_opcode, prev_next, ins_ptr);
                                            // The caller at Mono ELF +0x1e49c passes
                                            // r1 from its [fp-0x10], seeded from
                                            // cfg->0x14. Compare the live list
                                            // head with the saved callee argument.
                                            if (cfg <= 0xffffffe3u && Fits(cfg, 0x20u)) {
                                                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                                    "PV7MONOCFG cfg=%08x bb_arg=%08x list14=%08x list18=%08x fields00=%08x,%08x,%08x,%08x,%08x,%08x,%08x,%08x",
                                                                    cfg, bb,
                                                                    Read<std::uint32_t>(cfg + 0x14u),
                                                                    Read<std::uint32_t>(cfg + 0x18u),
                                                                    Read<std::uint32_t>(cfg),
                                                                    Read<std::uint32_t>(cfg + 4u),
                                                                    Read<std::uint32_t>(cfg + 8u),
                                                                    Read<std::uint32_t>(cfg + 12u),
                                                                    Read<std::uint32_t>(cfg + 16u),
                                                                    Read<std::uint32_t>(cfg + 20u),
                                                                    Read<std::uint32_t>(cfg + 24u),
                                                                    Read<std::uint32_t>(cfg + 28u));
                                            }
                                            if (bb <= 0xffffffe7u && Fits(bb, 0x18u)) {
                                                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                                    "PV7MONOBLOCK bb=%08x words=%08x,%08x,%08x,%08x,%08x,%08x",
                                                                    bb,
                                                                    Read<std::uint32_t>(bb),
                                                                    Read<std::uint32_t>(bb + 4u),
                                                                    Read<std::uint32_t>(bb + 8u),
                                                                    Read<std::uint32_t>(bb + 12u),
                                                                    Read<std::uint32_t>(bb + 16u),
                                                                    Read<std::uint32_t>(bb + 20u));
                                            }
                                            if (Fits(ins_ptr, 32u)) {
                                                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                                    "PV7MONOINST opcode=%04x words=%08x,%08x,%08x,%08x,%08x,%08x,%08x,%08x",
                                                                    Read<std::uint16_t>(ins_ptr),
                                                                    Read<std::uint32_t>(ins_ptr),
                                                                    Read<std::uint32_t>(ins_ptr + 4u),
                                                                    Read<std::uint32_t>(ins_ptr + 8u),
                                                                    Read<std::uint32_t>(ins_ptr + 12u),
                                                                    Read<std::uint32_t>(ins_ptr + 16u),
                                                                    Read<std::uint32_t>(ins_ptr + 20u),
                                                                    Read<std::uint32_t>(ins_ptr + 24u),
                                                                    Read<std::uint32_t>(ins_ptr + 28u));
                                            }
                                        }
                                    }
                                    for (int delta = -16; delta <= 32; delta += 16) {
                                        const std::uint32_t pos =
                                            static_cast<std::uint32_t>(
                                                static_cast<std::int64_t>(addr) + delta);
                                        if (Fits(pos, 16u)) {
                                            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                                "PV7MONOFRAME words at=%08x %08x,%08x,%08x,%08x",
                                                                pos, Read<std::uint32_t>(pos),
                                                                Read<std::uint32_t>(pos + 4u),
                                                                Read<std::uint32_t>(pos + 8u),
                                                                Read<std::uint32_t>(pos + 12u));
                                        }
                                    }
                                    std::size_t candidates = 0u;
                                    for (int delta = -96; delta <= 128 && candidates < 8u;
                                         delta += 4) {
                                        const std::int64_t cand64 = static_cast<std::int64_t>(addr) + delta;
                                        if (cand64 < static_cast<std::int64_t>(sp) ||
                                            cand64 >= kGuestStackTop)
                                            continue;
                                        const auto candidate_fp = Read<std::uint32_t>(
                                            static_cast<std::uint32_t>(cand64));
                                        if (candidate_fp < sp + 0x18u ||
                                            candidate_fp >= kGuestStackTop ||
                                            !Fits(candidate_fp - 0x18u, 4u))
                                            continue;
                                        const auto ins_ptr = Read<std::uint32_t>(candidate_fp - 0x18u);
                                        if (ins_ptr < 0x04000000u || !Fits(ins_ptr, 16u))
                                            continue;
                                        const auto opcode = Read<std::uint16_t>(ins_ptr);
                                        if (opcode > 0x1000u)
                                            continue;
                                        ++candidates;
                                        __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                            "PV7MONOFRAME candidate fp=%08x ins=%08x opcode=%04x aux=%08x",
                                                            candidate_fp, ins_ptr, opcode,
                                                            Read<std::uint32_t>(ins_ptr + 4u));
                                    }
                                    if (hits >= 4u) break;
                                }
                            }
                            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                "PV7MONOFRAME summary hits=%zu sp=%08x expectedRet=%08x",
                                                hits, sp, kEmitterLogReturn);
                        }
                    }
#endif
#endif
                }
            }
            Ret(0);
            return true;
        }
        if (fast_name == "__cxa_guard_acquire") {
            const std::uint32_t guard = Arg(0);
            if (!guard || !Fits(guard, 1)) { Ret(0); return true; }
            if (Read<std::uint8_t>(guard) != 0) { Ret(0); return true; }
            if (cxa_guards_in_progress_[guard]) { Ret(0); return true; }
            cxa_guards_in_progress_[guard] = true;
            Ret(1);
            return true;
        }
        if (fast_name == "__cxa_guard_release") {
            const std::uint32_t guard = Arg(0);
            if (guard && Fits(guard, 1)) Write(guard, static_cast<std::uint8_t>(1));
            cxa_guards_in_progress_.erase(guard);
            Ret(0);
            return true;
        }
        if (fast_name == "__cxa_guard_abort") {
            const std::uint32_t guard = Arg(0);
            if (guard && Fits(guard, 1)) Write(guard, static_cast<std::uint8_t>(0));
            cxa_guards_in_progress_.erase(guard);
            Ret(0);
            return true;
        }
        if (fast_name == "__cxa_atexit" || fast_name == "__aeabi_atexit" || fast_name == "__cxa_finalize") { Ret(0); return true; }
        if (fast_name == "__gnu_Unwind_Find_exidx") {
            const std::uint32_t pc = Arg(0);
            const std::uint32_t count_out = Arg(1);
            std::uint32_t table = 0;
            std::uint32_t count = 0;
            if (pc >= kMainBase && pc < 0x00107578u) {
                table = kMainBase + 0x00005bd0u;
                count = 0x000001c0u / 8u;
            } else if (pc >= kUnityBase && pc < 0x01bf9688u) {
                table = kUnityBase + 0x0093eb88u;
                count = 0x00026fa0u / 8u;
            } else if (pc >= kMonoBase && pc < 0x033d10a4u) {
                table = kMonoBase + 0x002e6648u;
                count = 0x0000b4f0u / 8u;
            }
            if (count_out >= 0x1000u && Fits(count_out, 4u)) {
                Write32(memory_, count_out, count);
            }
#if defined(__ANDROID__)
            else if (count_out != 0) {
                const std::uint32_t sp = jit ? jit->Regs()[13] : 0u;
                const std::uint32_t stack_return = sp >= 4u ? Read<std::uint32_t>(sp - 4u) : 0u;
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "UNWIND exidx rejected invalid count ptr=0x%08x pc=0x%08x code=[0x%08x 0x%08x 0x%08x 0x%08x] image=0x%08x lr=0x%08x sp[-4]=0x%08x",
                                    count_out, pc, code_read_prev3, code_read_prev2,
                                    code_read_prev1, code_read_last, code_read_last_image,
                                    jit ? jit->Regs()[14] : 0u, stack_return);
            }
#endif
            if (count_out != 0 && (count_out < 0x1000u || !Fits(count_out, 4u))) table = 0;
            Ret(table);
            return true;
        }
        if (fast_name == "sigaction") {
            const std::uint32_t signal = Arg(0);
            const std::uint32_t action_ptr = Arg(1);
            const std::uint32_t old_action_ptr = Arg(2);
            const auto existing = guest_signal_actions_.find(signal);
            const GuestSignalAction old_action = existing == guest_signal_actions_.end()
                                                     ? GuestSignalAction{}
                                                     : existing->second;
            if (old_action_ptr != 0) {
                if (!Fits(old_action_ptr, 16u)) return false;
                Write32(memory_, old_action_ptr, old_action.handler);
                Write32(memory_, old_action_ptr + 4u, old_action.mask);
                Write32(memory_, old_action_ptr + 8u, old_action.flags);
                Write32(memory_, old_action_ptr + 12u, old_action.restorer);
            }
            if (action_ptr != 0) {
                if (!Fits(action_ptr, 16u)) return false;
                GuestSignalAction action{};
                Read32(memory_, action_ptr, action.handler);
                Read32(memory_, action_ptr + 4u, action.mask);
                Read32(memory_, action_ptr + 8u, action.flags);
                Read32(memory_, action_ptr + 12u, action.restorer);
                guest_signal_actions_[signal] = action;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                    "PV7SIGNAL sigaction signal=%u handler=0x%08x mask=0x%08x flags=0x%08x",
                                    signal, action.handler, action.mask, action.flags);
#endif
            }
            Ret(0);
            return true;
        }
        if (fast_name == "bsd_signal" || fast_name == "signal") {
            const std::uint32_t signal = Arg(0);
            const std::uint32_t handler = Arg(1);
            const auto existing = guest_signal_actions_.find(signal);
            const std::uint32_t old_handler = existing == guest_signal_actions_.end() ? 0u : existing->second.handler;
            guest_signal_actions_[signal].handler = handler;
            Ret(old_handler);
            return true;
        }
        if (fast_name == "tkill") {
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL source=tkill sender=%u target=%u signal=%u lr=0x%08x sp=0x%08x",
                                current_thread_id_, Arg(0), Arg(1), jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
#endif
            const bool queued = QueueGuestSignal(Arg(0), Arg(1));
            // Mono reads errno after a failed tkill to exclude vanished threads
            // from its GC suspension acknowledgement count.
            if (!queued) Write32(memory_, kBootstrapRuntimeBase, 3u); // ESRCH
            Ret(queued ? 0u : 0xffffffffu);
            return true;
        }
        if (fast_name == "kill") {
            const std::uint32_t target = (Arg(0) == 0u || Arg(0) == 1234u) ? current_thread_id_ : Arg(0);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL source=kill sender=%u target=%u signal=%u lr=0x%08x sp=0x%08x",
                                current_thread_id_, target, Arg(1), jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
#endif
            Ret(QueueGuestSignal(target, Arg(1)) ? 0u : 0xffffffffu);
            return true;
        }
        if (fast_name == "raise") {
            const std::uint32_t signal = Arg(0);
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7SIGNAL source=raise sender=%u target=%u signal=%u lr=0x%08x sp=0x%08x",
                                current_thread_id_, current_thread_id_, signal, jit ? jit->Regs()[14] : 0u,
                                jit ? jit->Regs()[13] : 0u);
            if (jit && signal == 11u) {
                const std::uint32_t sp = jit->Regs()[13];
                std::uint32_t caller_fp = 0u;
                if (Fits(sp, 4u)) Read32(memory_, sp, caller_fp);
                if (caller_fp >= 36u && Fits(caller_fp - 36u, 40u)) {
                    std::uint32_t log_domain = 0u, log_level = 0u, log_format = 0u, log_args = 0u;
                    Read32(memory_, caller_fp - 24u, log_domain);
                    Read32(memory_, caller_fp - 28u, log_level);
                    Read32(memory_, caller_fp - 32u, log_format);
                    Read32(memory_, caller_fp - 36u, log_args);
                    std::uint32_t arg0 = 0u, arg1 = 0u, arg2 = 0u;
                    if (log_args && Fits(log_args, 12u)) {
                        Read32(memory_, log_args, arg0);
                        Read32(memory_, log_args + 4u, arg1);
                        Read32(memory_, log_args + 8u, arg2);
                    }
                    const std::string domain = log_domain && Fits(log_domain, 1u) ? ReadCString(log_domain) : std::string();
                    const std::string format = log_format && Fits(log_format, 1u) ? ReadCString(log_format) : std::string();
                    const std::string arg0_text = arg0 && Fits(arg0, 1u) ? ReadCString(arg0) : std::string();
                    const std::string arg2_text = arg2 && Fits(arg2, 1u) ? ReadCString(arg2) : std::string();
                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                        "PV7FATAL fp=0x%08x level=0x%08x format=0x%08x args=0x%08x arg0=0x%08x arg1=%u arg2=0x%08x domain='%s' format='%s' arg0str='%s' arg2str='%s'",
                                        caller_fp, log_level, log_format, log_args, arg0, arg1, arg2,
                                        domain.c_str(), format.c_str(), arg0_text.c_str(), arg2_text.c_str());
                }
            }
#endif
            if (!QueueGuestSignal(current_thread_id_, signal)) {
                Ret(0xffffffffu);
                return true;
            }
            // POSIX raise() targets the calling thread synchronously. Save the
            // successful return value before entering the handler so the signal
            // frame resumes inside the compatibility thunk and then returns 0.
            Ret(0);
            if (DeliverPendingGuestSignal()) {
                pending_guest_callback = true;
                if (jit) jit->HaltExecution();
            }
            return true;
        }
        if (fast_name == "sigsuspend") {
            auto interrupted = guest_signal_interrupted_.find(current_thread_id_);
            if (interrupted != guest_signal_interrupted_.end() && interrupted->second) {
                interrupted->second = false;
                const auto saved = guest_sigsuspend_saved_masks_.find(current_thread_id_);
                if (saved != guest_sigsuspend_saved_masks_.end()) {
                    guest_signal_masks_[current_thread_id_] = saved->second;
                    guest_sigsuspend_saved_masks_.erase(saved);
                }
                MarkGuestThreadRunnable(current_thread_id_);
                Write32(memory_, kBootstrapRuntimeBase, 4u); // EINTR
                Ret(0xffffffffu);
                return true;
            }

            const std::uint32_t mask_ptr = Arg(0);
            if (!mask_ptr || !Fits(mask_ptr, 4u)) {
                Write32(memory_, kBootstrapRuntimeBase, 22u); // EINVAL
                Ret(0xffffffffu);
                return true;
            }
            if (guest_sigsuspend_saved_masks_.find(current_thread_id_) ==
                guest_sigsuspend_saved_masks_.end()) {
                std::uint32_t temporary_mask = 0u;
                Read32(memory_, mask_ptr, temporary_mask);
                guest_sigsuspend_saved_masks_[current_thread_id_] = guest_signal_masks_[current_thread_id_];
                // SIGKILL and SIGSTOP may never be blocked.
                temporary_mask &= ~GuestSignalBit(9u);
                temporary_mask &= ~GuestSignalBit(19u);
                guest_signal_masks_[current_thread_id_] = temporary_mask;
            }

            if (HasDeliverablePendingGuestSignal(current_thread_id_)) {
                const std::uint32_t before_pc = jit ? jit->Regs()[15] : 0u;
                if (DeliverPendingGuestSignal() && jit && jit->Regs()[15] != before_pc) {
                    pending_guest_callback = true;
                    jit->HaltExecution();
                    return true;
                }
            }
            if (!block_current_sync_call(name)) return false;
            return true;
        }
        if (fast_name == "sigprocmask" || fast_name == "pthread_sigmask") {
            const std::uint32_t how = Arg(0);
            const std::uint32_t set_ptr = Arg(1);
            const std::uint32_t old_ptr = Arg(2);
            std::uint32_t mask = guest_signal_masks_[current_thread_id_];
            if (old_ptr != 0u) {
                if (!Fits(old_ptr, 4u)) {
                    Ret(fast_name == "pthread_sigmask" ? 22u : 0xffffffffu);
                    if (fast_name == "sigprocmask") Write32(memory_, kBootstrapRuntimeBase, 22u);
                    return true;
                }
                Write32(memory_, old_ptr, mask);
            }
            if (set_ptr != 0u) {
                if (!Fits(set_ptr, 4u)) {
                    Ret(fast_name == "pthread_sigmask" ? 22u : 0xffffffffu);
                    if (fast_name == "sigprocmask") Write32(memory_, kBootstrapRuntimeBase, 22u);
                    return true;
                }
                std::uint32_t requested = 0u;
                Read32(memory_, set_ptr, requested);
                switch (how) {
                    case 0u: mask |= requested; break;  // SIG_BLOCK
                    case 1u: mask &= ~requested; break; // SIG_UNBLOCK
                    case 2u: mask = requested; break;   // SIG_SETMASK
                    default:
                        Ret(fast_name == "pthread_sigmask" ? 22u : 0xffffffffu);
                        if (fast_name == "sigprocmask") Write32(memory_, kBootstrapRuntimeBase, 22u);
                        return true;
                }
                mask &= ~GuestSignalBit(9u);
                mask &= ~GuestSignalBit(19u);
                guest_signal_masks_[current_thread_id_] = mask;
                if (HasDeliverablePendingGuestSignal(current_thread_id_))
                    MarkGuestThreadRunnable(current_thread_id_);
            }
            Ret(0u);
            return true;
        }
        if (fast_name == "setitimer" || fast_name == "prctl") {
            Ret(0); return true;
        }
        if (fast_name == "fopen") {
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            const std::string mode = ReadCString(Arg(1));
            if (file_trace.size() < 24) file_trace.push_back("fopen " + path + " mode=" + mode);
            errno = 0;
            if (path == "/proc/cpuinfo" && mode.find('r') != std::string::npos) {
                static constexpr char kArmV7CpuInfo[] =
                    "Processor\t: ARMv7 Processor rev 0 (v7l)\n"
                    "processor\t: 0\n"
                    "BogoMIPS\t: 38.40\n"
                    "Features\t: swp half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt\n"
                    "CPU implementer\t: 0x51\n"
                    "CPU architecture: 7\n"
                    "CPU variant\t: 0x2\n"
                    "CPU part\t: 0x06f\n"
                    "CPU revision\t: 0\n"
                    "Hardware\t: CNR64 Virtual ARMv7\n";
                const std::uint32_t handle = RegisterVirtualFile(kArmV7CpuInfo);
                SetErrno(0);
                if (file_trace.size() < 24) file_trace.push_back("  -> virtual ARMv7 cpuinfo handle");
                Ret(handle);
                return true;
            }
            std::FILE* file = std::fopen(path.c_str(), mode.c_str());
            SetErrno(file ? 0u : static_cast<std::uint32_t>(errno));
            const std::uint32_t handle = RegisterFileHandle(file, path);
            if (path.find("mscorlib.dll") != std::string::npos && file_trace.size() < 96) {
                std::ostringstream trace;
                trace << "  -> fopen handle=0x" << std::hex << handle << std::dec << " errno=" << errno;
                file_trace.push_back(trace.str());
            }
            Ret(handle);
            return true;
        }
        if (fast_name == "fclose") {
            const std::uint32_t handle = Arg(0);
            const auto virtual_it = virtual_files_.find(handle);
            if (virtual_it != virtual_files_.end()) {
                virtual_files_.erase(virtual_it);
                Ret(0);
                return true;
            }
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { SetErrno(9); Ret(0xffffffffu); return true; }
            const int rc = std::fclose(file);
            file_handles_.erase(handle);
            file_paths_.erase(handle);
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (fast_name == "fread" || fast_name == "fwrite") {
            const std::uint32_t guest = Arg(0);
            const std::size_t size = Arg(1);
            const std::size_t count = Arg(2);
            const std::uint32_t handle = Arg(3);
            const std::uint64_t bytes = static_cast<std::uint64_t>(size) * count;
            if (bytes > memory_.size() || !Fits(guest, static_cast<std::size_t>(bytes))) {
                SetErrno(14); Ret(0); return true;
            }
            auto virtual_it = virtual_files_.find(handle);
            if (virtual_it != virtual_files_.end()) {
                if (fast_name != "fread" || size == 0) { Ret(0); return true; }
                VirtualFile& vf = virtual_it->second;
                const std::size_t available = vf.data.size() - std::min(vf.pos, vf.data.size());
                const std::size_t copied = std::min<std::size_t>(available, static_cast<std::size_t>(bytes));
                if (copied) std::memcpy(memory_.data() + guest, vf.data.data() + vf.pos, copied);
                vf.pos += copied;
                Ret(static_cast<std::uint32_t>(copied / size));
                return true;
            }
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { SetErrno(9); Ret(0); return true; }
            const long before = std::ftell(file);
            const std::size_t done = fast_name == "fread"
                ? std::fread(memory_.data() + guest, size, count, file)
                : std::fwrite(memory_.data() + guest, size, count, file);
            const auto path_it = file_paths_.find(handle);
            if (fast_name == "fread" && path_it != file_paths_.end() &&
                path_it->second.find("mscorlib.dll") != std::string::npos && file_trace.size() < 96) {
                std::ostringstream trace;
                trace << "fread mscorlib pos=" << before << " request=" << bytes << " result=" << done * size << " head=";
                const std::size_t head = std::min<std::size_t>(done * size, 8);
                for (std::size_t i = 0; i < head; ++i) {
                    char hex[4]{};
                    std::snprintf(hex, sizeof(hex), "%02x", static_cast<unsigned>(memory_[guest + i]));
                    trace << hex;
                }
                file_trace.push_back(trace.str());
            }
            Ret(static_cast<std::uint32_t>(done));
            return true;
        }
        if (fast_name == "fseek") {
            const std::uint32_t handle = Arg(0);
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { SetErrno(9); Ret(0xffffffffu); return true; }
            const auto path_it = file_paths_.find(handle);
            const long before = std::ftell(file);
            const int rc = std::fseek(file, static_cast<long>(static_cast<std::int32_t>(Arg(1))), static_cast<int>(Arg(2)));
            if (path_it != file_paths_.end() && path_it->second.find("mscorlib.dll") != std::string::npos && file_trace.size() < 96) {
                std::ostringstream trace;
                trace << "fseek mscorlib from=" << before << " offset=" << static_cast<std::int32_t>(Arg(1))
                      << " whence=" << Arg(2) << " rc=" << rc << " to=" << std::ftell(file);
                file_trace.push_back(trace.str());
            }
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (fast_name == "ftell") {
            std::FILE* file = LookupFileHandle(Arg(0));
            if (!file) { SetErrno(9); Ret(0xffffffffu); return true; }
            Ret(static_cast<std::uint32_t>(std::ftell(file)));
            return true;
        }
        if (fast_name == "fgets") {
            const std::uint32_t dst = Arg(0);
            const int size = static_cast<int>(Arg(1));
            const std::uint32_t handle = Arg(2);
            if (size <= 0 || !Fits(dst, static_cast<std::size_t>(size))) { Ret(0); return true; }
            auto virtual_it = virtual_files_.find(handle);
            if (virtual_it != virtual_files_.end()) {
                VirtualFile& vf = virtual_it->second;
                if (vf.pos >= vf.data.size()) { Ret(0); return true; }
                std::size_t count = 0;
                const std::size_t limit = static_cast<std::size_t>(size - 1);
                while (count < limit && vf.pos < vf.data.size()) {
                    const char c = vf.data[vf.pos++];
                    memory_[dst + count++] = static_cast<std::uint8_t>(c);
                    if (c == '\n') break;
                }
                memory_[dst + count] = 0;
                Ret(dst);
                return true;
            }
            std::FILE* file = LookupFileHandle(handle);
            if (!file) { Ret(0); return true; }
            Ret(std::fgets(reinterpret_cast<char*>(memory_.data() + dst), size, file) ? dst : 0u);
            return true;
        }
        if (fast_name == "fflush") {
            std::FILE* file = LookupFileHandle(Arg(0));
            Ret(static_cast<std::uint32_t>(file ? std::fflush(file) : 0));
            return true;
        }
        if (fast_name == "ferror" || fast_name == "clearerr") {
            std::FILE* file = LookupFileHandle(Arg(0));
            if (!file) { Ret(fast_name == "ferror" ? 1u : 0u); return true; }
            if (fast_name == "clearerr") { std::clearerr(file); Ret(0); }
            else Ret(static_cast<std::uint32_t>(std::ferror(file)));
            return true;
        }
        if (fast_name == "getc" || fast_name == "ungetc") {
            std::FILE* file = LookupFileHandle(fast_name == "getc" ? Arg(0) : Arg(1));
            if (!file) { Ret(0xffffffffu); return true; }
            const int rc = fast_name == "getc" ? std::getc(file) : std::ungetc(static_cast<int>(Arg(0)), file);
            Ret(static_cast<std::uint32_t>(rc));
            return true;
        }
        if (fast_name == "socket") {
#if !defined(_WIN32)
            errno = 0;
            const int fd = ::socket(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)), static_cast<int>(Arg(2)));
            if (fd >= 0) socket_handles_[static_cast<std::uint32_t>(fd)] = true;
            SetErrno(fd >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            if (file_trace.size() < 32) {
                std::ostringstream line;
                line << "socket domain=" << static_cast<int>(Arg(0))
                     << " type=" << static_cast<int>(Arg(1))
                     << " protocol=" << static_cast<int>(Arg(2))
                     << " fd=" << fd;
                file_trace.push_back(line.str());
            }
            Ret(static_cast<std::uint32_t>(fd));
#else
            const std::uint32_t handle = next_socket_handle_++;
            socket_handles_[handle] = true;
            Ret(handle);
#endif
            return true;
        }
        if (fast_name == "bind") {
#if !defined(_WIN32)
            const std::uint32_t address = Arg(1);
            const socklen_t length = static_cast<socklen_t>(Arg(2));
            if (address == 0 || !Fits(address, length)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
            errno = 0;
            const int rc = ::bind(static_cast<int>(Arg(0)),
                                  reinterpret_cast<const sockaddr*>(memory_.data() + address), length);
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "listen") {
#if !defined(_WIN32)
            errno = 0;
            const int rc = ::listen(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "setsockopt") {
#if !defined(_WIN32)
            const std::uint32_t value = Arg(3);
            const socklen_t length = static_cast<socklen_t>(Arg(4));
            if (value != 0 && !Fits(value, length)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
            errno = 0;
            const int rc = ::setsockopt(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)),
                                        static_cast<int>(Arg(2)),
                                        value ? memory_.data() + value : nullptr, length);
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "shutdown") {
#if !defined(_WIN32)
            errno = 0;
            const int rc = ::shutdown(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "connect") {
#if !defined(_WIN32)
            const std::uint32_t address = Arg(1);
            const socklen_t length = static_cast<socklen_t>(Arg(2));
            if (address == 0 || !Fits(address, length)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
            errno = 0;
            const int rc = ::connect(static_cast<int>(Arg(0)),
                                     reinterpret_cast<const sockaddr*>(memory_.data() + address), length);
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(111); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "accept") {
#if !defined(_WIN32)
            const std::uint32_t address = Arg(1);
            const std::uint32_t length_ptr = Arg(2);
            std::uint32_t guest_length = 0;
            if (length_ptr && !Read32(memory_, length_ptr, guest_length)) {
                SetErrno(EFAULT); Ret(0xffffffffu); return true;
            }
            socklen_t length = static_cast<socklen_t>(guest_length);
            sockaddr* host_address = address && length && Fits(address, length)
                ? reinterpret_cast<sockaddr*>(memory_.data() + address) : nullptr;
            errno = 0;
            const int fd = ::accept(static_cast<int>(Arg(0)), host_address, length_ptr ? &length : nullptr);
            if (fd >= 0) socket_handles_[static_cast<std::uint32_t>(fd)] = true;
            if (length_ptr) Write32(memory_, length_ptr, static_cast<std::uint32_t>(length));
            SetErrno(fd >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(fd));
#else
            SetErrno(11); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "recv" || fast_name == "send") {
#if !defined(_WIN32)
            const std::uint32_t buffer = Arg(1);
            const std::size_t length = Arg(2);
            if (!Fits(buffer, length)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
            errno = 0;
            const ssize_t rc = fast_name == "recv"
                ? ::recv(static_cast<int>(Arg(0)), memory_.data() + buffer, length, static_cast<int>(Arg(3)))
                : ::send(static_cast<int>(Arg(0)), memory_.data() + buffer, length, static_cast<int>(Arg(3)));
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(11); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "recvfrom" || fast_name == "sendto") {
#if !defined(_WIN32)
            const std::uint32_t buffer = Arg(1);
            const std::size_t length = Arg(2);
            if (!Fits(buffer, length)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
            errno = 0;
            ssize_t rc = -1;
            if (fast_name == "recvfrom") {
                const std::uint32_t address = Arg(4);
                const std::uint32_t length_ptr = Arg(5);
                std::uint32_t guest_length = 0;
                if (length_ptr) Read32(memory_, length_ptr, guest_length);
                socklen_t addr_length = static_cast<socklen_t>(guest_length);
                sockaddr* host_address = address && addr_length && Fits(address, addr_length)
                    ? reinterpret_cast<sockaddr*>(memory_.data() + address) : nullptr;
                rc = ::recvfrom(static_cast<int>(Arg(0)), memory_.data() + buffer, length,
                                static_cast<int>(Arg(3)), host_address,
                                length_ptr ? &addr_length : nullptr);
                if (length_ptr) Write32(memory_, length_ptr, static_cast<std::uint32_t>(addr_length));
            } else {
                const std::uint32_t address = Arg(4);
                const socklen_t addr_length = static_cast<socklen_t>(Arg(5));
                if (address && !Fits(address, addr_length)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
                rc = ::sendto(static_cast<int>(Arg(0)), memory_.data() + buffer, length,
                              static_cast<int>(Arg(3)),
                              address ? reinterpret_cast<const sockaddr*>(memory_.data() + address) : nullptr,
                              addr_length);
            }
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(11); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "recvmsg" || fast_name == "sendmsg") {
            SetErrno(38); // ENOSYS: translate msghdr only if a guest path actually needs it.
            Ret(0xffffffffu);
            return true;
        }
        if (fast_name == "getsockname" || fast_name == "getpeername") {
#if !defined(_WIN32)
            const std::uint32_t address = Arg(1);
            const std::uint32_t length_ptr = Arg(2);
            std::uint32_t guest_length = 0;
            if (!length_ptr || !Read32(memory_, length_ptr, guest_length) ||
                (address && !Fits(address, guest_length))) {
                SetErrno(EFAULT); Ret(0xffffffffu); return true;
            }
            socklen_t length = static_cast<socklen_t>(guest_length);
            errno = 0;
            const int rc = fast_name == "getsockname"
                ? ::getsockname(static_cast<int>(Arg(0)), reinterpret_cast<sockaddr*>(memory_.data() + address), &length)
                : ::getpeername(static_cast<int>(Arg(0)), reinterpret_cast<sockaddr*>(memory_.data() + address), &length);
            Write32(memory_, length_ptr, static_cast<std::uint32_t>(length));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "getsockopt") {
#if !defined(_WIN32)
            std::uint32_t guest_length = 0;
            if (!Arg(4) || !Read32(memory_, Arg(4), guest_length) ||
                (Arg(3) && !Fits(Arg(3), guest_length))) {
                SetErrno(EFAULT); Ret(0xffffffffu); return true;
            }
            socklen_t length = static_cast<socklen_t>(guest_length);
            errno = 0;
            const int rc = ::getsockopt(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)),
                                        static_cast<int>(Arg(2)),
                                        Arg(3) ? memory_.data() + Arg(3) : nullptr, &length);
            Write32(memory_, Arg(4), static_cast<std::uint32_t>(length));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "gethostname") {
#if !defined(_WIN32)
            if (!Fits(Arg(0), Arg(1))) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
            errno = 0;
            const int rc = ::gethostname(reinterpret_cast<char*>(memory_.data() + Arg(0)), Arg(1));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            const std::string hostname = "localhost";
            Ret(WriteCString(Arg(0), hostname, Arg(1)) ? 0u : 0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "getaddrinfo") {
#if !defined(_WIN32)
            const std::string node = Arg(0) ? ReadCString(Arg(0)) : std::string();
            const std::string service = Arg(1) ? ReadCString(Arg(1)) : std::string();
            addrinfo hints{};
            addrinfo* hints_ptr = nullptr;
            if (Arg(2) && Fits(Arg(2), 20u)) {
                hints.ai_flags = static_cast<int>(Read<std::uint32_t>(Arg(2) + 0u));
                hints.ai_family = static_cast<int>(Read<std::uint32_t>(Arg(2) + 4u));
                hints.ai_socktype = static_cast<int>(Read<std::uint32_t>(Arg(2) + 8u));
                hints.ai_protocol = static_cast<int>(Read<std::uint32_t>(Arg(2) + 12u));
                hints_ptr = &hints;
            }
            addrinfo* results = nullptr;
            const int rc = ::getaddrinfo(node.empty() ? nullptr : node.c_str(),
                                         service.empty() ? nullptr : service.c_str(), hints_ptr, &results);
            if (Arg(3)) Write32(memory_, Arg(3), 0u);
            if (rc != 0 || !results) { if (results) ::freeaddrinfo(results); Ret(static_cast<std::uint32_t>(rc)); return true; }

            std::vector<std::uint32_t> guest_entries;
            for (addrinfo* it = results; it && guest_entries.size() < 32u; it = it->ai_next) {
                if (!it->ai_addr || it->ai_addrlen == 0 || it->ai_addrlen > 256u) continue;
                const std::uint32_t guest_addr = Allocate(it->ai_addrlen, 4u);
                const std::uint32_t guest_info = Allocate(32u, 4u);
                if (!guest_addr || !guest_info) break;
                std::memcpy(memory_.data() + guest_addr, it->ai_addr, it->ai_addrlen);
                std::uint32_t canon = 0;
                if (it->ai_canonname && *it->ai_canonname) {
                    const std::string canon_name(it->ai_canonname);
                    canon = Allocate(canon_name.size() + 1u, 1u);
                    if (canon) WriteCString(canon, canon_name);
                }
                Write32(memory_, guest_info + 0u, static_cast<std::uint32_t>(it->ai_flags));
                Write32(memory_, guest_info + 4u, static_cast<std::uint32_t>(it->ai_family));
                Write32(memory_, guest_info + 8u, static_cast<std::uint32_t>(it->ai_socktype));
                Write32(memory_, guest_info + 12u, static_cast<std::uint32_t>(it->ai_protocol));
                Write32(memory_, guest_info + 16u, static_cast<std::uint32_t>(it->ai_addrlen));
                Write32(memory_, guest_info + 20u, canon);
                Write32(memory_, guest_info + 24u, guest_addr);
                Write32(memory_, guest_info + 28u, 0u);
                if (!guest_entries.empty()) Write32(memory_, guest_entries.back() + 28u, guest_info);
                guest_entries.push_back(guest_info);
            }
            ::freeaddrinfo(results);
            if (guest_entries.empty()) { Ret(static_cast<std::uint32_t>(EAI_MEMORY)); return true; }
            if (Arg(3)) Write32(memory_, Arg(3), guest_entries.front());
            Ret(0u);
#else
            if (Arg(3)) Write32(memory_, Arg(3), 0u);
            Ret(static_cast<std::uint32_t>(-2));
#endif
            return true;
        }
        if (fast_name == "freeaddrinfo") { Ret(0); return true; }
        if (fast_name == "gai_strerror") {
#if !defined(_WIN32)
            const char* text = ::gai_strerror(static_cast<int>(Arg(0)));
            const std::string message = text ? text : "getaddrinfo error";
#else
            const std::string message = "Name or service not known";
#endif
            const std::uint32_t guest = Allocate(message.size() + 1, 1);
            if (guest != 0) WriteCString(guest, message);
            Ret(guest);
            return true;
        }
        if (fast_name == "gethostbyname" || fast_name == "gethostbyaddr") {
#if !defined(_WIN32)
            hostent* result = nullptr;
            std::string query;
            if (fast_name == "gethostbyname") {
                query = ReadCString(Arg(0));
                result = query.empty() ? nullptr : ::gethostbyname(query.c_str());
            } else {
                const std::size_t address_length = static_cast<std::size_t>(Arg(1));
                if (Arg(0) != 0 && address_length != 0 && Fits(Arg(0), address_length)) {
                    result = ::gethostbyaddr(memory_.data() + Arg(0),
                                             static_cast<socklen_t>(address_length),
                                             static_cast<int>(Arg(2)));
                }
            }

            if (!result || !result->h_addr_list || result->h_length <= 0) {
                // Bionic exposes h_errno through __get_h_errno(). The guest bridge maps
                // that TLS-style integer onto our small runtime errno cell as well.
                Write32(memory_, kBootstrapRuntimeBase, static_cast<std::uint32_t>(h_errno));
#if defined(__ANDROID__)
                __android_log_print(ANDROID_LOG_WARN, "CNR64POC",
                                    "PV7NET %s query=%s failed h_errno=%d",
                                    name.c_str(), query.c_str(), h_errno);
#endif
                Ret(0u);
                return true;
            }

            std::size_t address_count = 0;
            while (result->h_addr_list[address_count] && address_count < 64u) ++address_count;
            if (address_count == 0u) { Ret(0u); return true; }

            const std::string canonical = result->h_name ? result->h_name : query;
            const std::uint32_t guest_name = Allocate(canonical.size() + 1u, 1u);
            const std::uint32_t guest_aliases = Allocate(4u, 4u); // null-only char**
            const std::uint32_t guest_addr_list = Allocate((address_count + 1u) * 4u, 4u);
            const std::uint32_t guest_hostent = Allocate(20u, 4u);
            if (!guest_name || !guest_aliases || !guest_addr_list || !guest_hostent) {
                Ret(0u);
                return true;
            }
            WriteCString(guest_name, canonical);
            Write32(memory_, guest_aliases, 0u);
            for (std::size_t i = 0; i < address_count; ++i) {
                const std::uint32_t guest_address = Allocate(static_cast<std::size_t>(result->h_length), 4u);
                if (!guest_address) { Ret(0u); return true; }
                std::memcpy(memory_.data() + guest_address,
                            result->h_addr_list[i], static_cast<std::size_t>(result->h_length));
                Write32(memory_, guest_addr_list + static_cast<std::uint32_t>(i * 4u), guest_address);
            }
            Write32(memory_, guest_addr_list + static_cast<std::uint32_t>(address_count * 4u), 0u);

            // 32-bit Bionic struct hostent layout.
            Write32(memory_, guest_hostent + 0u, guest_name);
            Write32(memory_, guest_hostent + 4u, guest_aliases);
            Write32(memory_, guest_hostent + 8u, static_cast<std::uint32_t>(result->h_addrtype));
            Write32(memory_, guest_hostent + 12u, static_cast<std::uint32_t>(result->h_length));
            Write32(memory_, guest_hostent + 16u, guest_addr_list);
            Write32(memory_, kBootstrapRuntimeBase, 0u);
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7NET %s query=%s ok family=%d len=%d addrs=%zu guest=0x%08x",
                                name.c_str(), query.c_str(), result->h_addrtype,
                                result->h_length, address_count, guest_hostent);
#endif
            Ret(guest_hostent);
#else
            Ret(0u);
#endif
            return true;
        }
#if !defined(_WIN32)
        if (fast_name == "inet_pton") {
            const std::string source = ReadCString(Arg(1));
            const std::size_t destination_size = Arg(0) == AF_INET6 ? 16u : 4u;
            void* destination = Arg(2) != 0 && Fits(Arg(2), destination_size)
                ? static_cast<void*>(memory_.data() + Arg(2)) : nullptr;
            if (!destination) { Ret(0); return true; }
            Ret(static_cast<std::uint32_t>(::inet_pton(static_cast<int>(Arg(0)), source.c_str(), destination)));
            return true;
        }
        if (fast_name == "inet_ntop") {
            const std::size_t source_size = Arg(0) == AF_INET6 ? 16u : 4u;
            const void* source = Arg(1) != 0 && Fits(Arg(1), source_size)
                ? static_cast<const void*>(memory_.data() + Arg(1)) : nullptr;
            char* destination = Arg(2) != 0 && Fits(Arg(2), Arg(3))
                ? reinterpret_cast<char*>(memory_.data() + Arg(2)) : nullptr;
            if (!source || !destination) { Ret(0); return true; }
            Ret(::inet_ntop(static_cast<int>(Arg(0)), source, destination, static_cast<socklen_t>(Arg(3))) ? Arg(2) : 0u);
            return true;
        }
        if (fast_name == "inet_addr") {
            const std::string source = ReadCString(Arg(0));
            Ret(static_cast<std::uint32_t>(::inet_addr(source.c_str())));
            return true;
        }
        if (fast_name == "inet_aton") {
            const std::string source = ReadCString(Arg(0));
            in_addr* destination = Arg(1) != 0 && Fits(Arg(1), sizeof(in_addr))
                ? reinterpret_cast<in_addr*>(memory_.data() + Arg(1)) : nullptr;
            Ret(destination ? static_cast<std::uint32_t>(::inet_aton(source.c_str(), destination)) : 0u);
            return true;
        }
        if (fast_name == "inet_ntoa") {
            in_addr address{};
            const std::uint32_t raw = Arg(0);
            std::memcpy(&address, &raw, sizeof(raw));
            const std::string value = ::inet_ntoa(address);
            const std::uint32_t guest = Allocate(value.size() + 1, 1);
            if (guest != 0) WriteCString(guest, value);
            Ret(guest);
            return true;
        }
#endif
        if (fast_name == "ioctl") {
#if !defined(_WIN32)
            errno = 0;
            void* argp = Arg(2) && Fits(Arg(2), 8u) ? memory_.data() + Arg(2) : nullptr;
            const int rc = ::ioctl(static_cast<int>(Arg(0)), static_cast<unsigned long>(Arg(1)), argp);
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(25); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "fcntl") {
#if !defined(_WIN32)
            errno = 0;
            const int rc = ::fcntl(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)), static_cast<int>(Arg(2)));
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(25); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "poll") {
#if !defined(_WIN32)
            const std::uint32_t descriptors = Arg(0);
            const std::uint32_t count = Arg(1);
            if (count > 4096u || (count && !Fits(descriptors, static_cast<std::size_t>(count) * 8u))) return false;
            std::vector<pollfd> host_poll(count);
            for (std::uint32_t i = 0; i < count; ++i) {
                const std::uint32_t base = descriptors + i * 8u;
                host_poll[i].fd = static_cast<int>(Read<std::uint32_t>(base));
                host_poll[i].events = Read<std::int16_t>(base + 4u);
                host_poll[i].revents = 0;
            }
            errno = 0;
            const int rc = ::poll(host_poll.data(), host_poll.size(), static_cast<int>(Arg(2)));
            for (std::uint32_t i = 0; i < count; ++i)
                std::memcpy(memory_.data() + descriptors + i * 8u + 6u, &host_poll[i].revents, sizeof(host_poll[i].revents));
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "select") {
#if !defined(_WIN32)
            const int nfds = static_cast<int>(Arg(0));
            fd_set readfds{}, writefds{}, exceptfds{};
            fd_set* read_ptr = nullptr; fd_set* write_ptr = nullptr; fd_set* except_ptr = nullptr;
            auto import_set = [&](std::uint32_t guest, fd_set& set, fd_set*& ptr) -> bool {
                if (!guest) return true;
                if (!Fits(guest, 128u)) return false;
                std::memcpy(&set, memory_.data() + guest, std::min<std::size_t>(sizeof(fd_set), 128u));
                ptr = &set;
                return true;
            };
            if (!import_set(Arg(1), readfds, read_ptr) || !import_set(Arg(2), writefds, write_ptr) ||
                !import_set(Arg(3), exceptfds, except_ptr)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
            timeval timeout{}; timeval* timeout_ptr = nullptr;
            if (Arg(4)) {
                if (!Fits(Arg(4), 8u)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
                timeout.tv_sec = static_cast<std::int32_t>(Read<std::uint32_t>(Arg(4)));
                timeout.tv_usec = static_cast<std::int32_t>(Read<std::uint32_t>(Arg(4) + 4u));
                timeout_ptr = &timeout;
            }
            errno = 0;
            const int rc = ::select(nfds, read_ptr, write_ptr, except_ptr, timeout_ptr);
            auto export_set = [&](std::uint32_t guest, fd_set* ptr) {
                if (guest && ptr) std::memcpy(memory_.data() + guest, ptr, std::min<std::size_t>(sizeof(fd_set), 128u));
            };
            export_set(Arg(1), read_ptr); export_set(Arg(2), write_ptr); export_set(Arg(3), except_ptr);
            if (Arg(4) && timeout_ptr) {
                Write32(memory_, Arg(4), static_cast<std::uint32_t>(timeout.tv_sec));
                Write32(memory_, Arg(4) + 4u, static_cast<std::uint32_t>(timeout.tv_usec));
            }
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "epoll_create") {
#if !defined(_WIN32)
            errno = 0;
            const int fd = ::epoll_create(static_cast<int>(Arg(0)));
            if (fd >= 0) socket_handles_[static_cast<std::uint32_t>(fd)] = true;
            SetErrno(fd >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(fd));
#else
            const std::uint32_t handle = next_socket_handle_++;
            socket_handles_[handle] = true;
            Ret(handle);
#endif
            return true;
        }
        if (fast_name == "epoll_ctl") {
#if !defined(_WIN32)
            epoll_event event{};
            epoll_event* event_ptr = nullptr;
            if (Arg(3)) {
                if (!Fits(Arg(3), 12u)) { SetErrno(EFAULT); Ret(0xffffffffu); return true; }
                std::memcpy(&event, memory_.data() + Arg(3), std::min<std::size_t>(sizeof(event), 12u));
                event_ptr = &event;
            }
            errno = 0;
            const int rc = ::epoll_ctl(static_cast<int>(Arg(0)), static_cast<int>(Arg(1)),
                                       static_cast<int>(Arg(2)), event_ptr);
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "epoll_wait") {
#if !defined(_WIN32)
            const int maxevents = static_cast<int>(Arg(2));
            if (maxevents < 0 || maxevents > 4096 || !Fits(Arg(1), static_cast<std::size_t>(maxevents) * 12u)) return false;
            std::vector<epoll_event> events(static_cast<std::size_t>(maxevents));
            errno = 0;
            const int rc = ::epoll_wait(static_cast<int>(Arg(0)), events.data(), maxevents, static_cast<int>(Arg(3)));
            if (rc > 0) {
                for (int i = 0; i < rc; ++i)
                    std::memcpy(memory_.data() + Arg(1) + static_cast<std::uint32_t>(i * 12u),
                                &events[static_cast<std::size_t>(i)], std::min<std::size_t>(sizeof(epoll_event), 12u));
            }
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "pipe") {
            if (Arg(0) == 0 || !Fits(Arg(0), 8u)) return false;
#if !defined(_WIN32)
            int fds[2] = {-1, -1};
            errno = 0;
            const int rc = ::pipe(fds);
            if (rc == 0) {
                socket_handles_[static_cast<std::uint32_t>(fds[0])] = true;
                socket_handles_[static_cast<std::uint32_t>(fds[1])] = true;
                Write32(memory_, Arg(0), static_cast<std::uint32_t>(fds[0]));
                Write32(memory_, Arg(0) + 4u, static_cast<std::uint32_t>(fds[1]));
            }
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            const std::uint32_t read_handle = next_socket_handle_++;
            const std::uint32_t write_handle = next_socket_handle_++;
            socket_handles_[read_handle] = true;
            socket_handles_[write_handle] = true;
            Write32(memory_, Arg(0), read_handle);
            Write32(memory_, Arg(0) + 4u, write_handle);
            Ret(0);
#endif
            return true;
        }
        if (fast_name == "open") {
#if !defined(_WIN32)
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            if (file_trace.size() < 24) file_trace.push_back("open " + path);
            errno = 0;
            const int fd = ::open(path.c_str(), static_cast<int>(Arg(1)), static_cast<mode_t>(Arg(2)));
            SetErrno(fd >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(fd));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "close") {
#if !defined(_WIN32)
            errno = 0;
            const int rc = ::close(static_cast<int>(Arg(0)));
            socket_handles_.erase(Arg(0));
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            const auto fake_socket = socket_handles_.find(Arg(0));
            if (fake_socket != socket_handles_.end()) {
                socket_handles_.erase(fake_socket);
                SetErrno(0);
                Ret(0);
            } else {
                SetErrno(38);
                Ret(0xffffffffu);
            }
#endif
            return true;
        }
        if (fast_name == "read" || fast_name == "write") {
#if !defined(_WIN32)
            const int fd = static_cast<int>(Arg(0));
            const std::uint32_t guest = Arg(1);
            const std::size_t count = Arg(2);
            if (!Fits(guest, count)) { SetErrno(14); Ret(0xffffffffu); return true; }
            const ssize_t rc = fast_name == "read"
                ? ::read(fd, memory_.data() + guest, count)
                : ::write(fd, memory_.data() + guest, count);
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "lseek") {
#if !defined(_WIN32)
            const off_t rc = ::lseek(static_cast<int>(Arg(0)), static_cast<off_t>(static_cast<std::int32_t>(Arg(1))), static_cast<int>(Arg(2)));
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "fstat" || fast_name == "stat" || fast_name == "lstat") {
#if !defined(_WIN32)
            struct stat st{};
            int rc = -1;
            std::uint32_t guest_stat = 0;
            if (fast_name == "fstat") {
                rc = ::fstat(static_cast<int>(Arg(0)), &st);
                guest_stat = Arg(1);
            } else {
                const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
                rc = fast_name == "stat" ? ::stat(path.c_str(), &st) : ::lstat(path.c_str(), &st);
                guest_stat = Arg(1);
            }
            if (rc == 0 && !WriteGuestStat(guest_stat, st)) { SetErrno(14); Ret(0xffffffffu); return true; }
            SetErrno(rc == 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "readlink") {
#if !defined(_WIN32)
            const std::string path = NormalizeGuestPath(ReadCString(Arg(0)));
            const std::uint32_t dst = Arg(1);
            const std::size_t size = Arg(2);
            if (!Fits(dst, size)) { SetErrno(14); Ret(0xffffffffu); return true; }
            const ssize_t rc = ::readlink(path.c_str(), reinterpret_cast<char*>(memory_.data() + dst), size);
            SetErrno(rc >= 0 ? 0u : static_cast<std::uint32_t>(errno));
            Ret(static_cast<std::uint32_t>(rc));
#else
            SetErrno(38); Ret(0xffffffffu);
#endif
            return true;
        }
        if (fast_name == "opendir") {
            SetErrno(2); Ret(0); return true;
        }
        if (fast_name == "closedir") { Ret(0); return true; }
        if (fast_name == "isatty") { Ret(0); return true; }
        if (fast_name == "atoi" || fast_name == "atol") {
            const std::string text = ReadCString(Arg(0));
            Ret(static_cast<std::uint32_t>(std::strtol(text.c_str(), nullptr, 10)));
            return true;
        }
        if (fast_name == "strtol" || fast_name == "strtoul") {
            const std::string text = ReadCString(Arg(0));
            char* end = nullptr;
            const int base = static_cast<int>(Arg(2));
            unsigned long value = fast_name == "strtol"
                ? static_cast<unsigned long>(std::strtol(text.c_str(), &end, base))
                : std::strtoul(text.c_str(), &end, base);
            if (Arg(1)) Write32(memory_, Arg(1), Arg(0) + static_cast<std::uint32_t>(end - text.c_str()));
            Ret(static_cast<std::uint32_t>(value));
            return true;
        }
        if (fast_name == "tolower") { Ret(static_cast<std::uint32_t>(std::tolower(static_cast<unsigned char>(Arg(0))))); return true; }
        if (fast_name == "isalnum") { Ret(std::isalnum(static_cast<unsigned char>(Arg(0))) ? 1u : 0u); return true; }
        if (fast_name == "isalpha") { Ret(std::isalpha(static_cast<unsigned char>(Arg(0))) ? 1u : 0u); return true; }
        if (fast_name == "isspace") { Ret(std::isspace(static_cast<unsigned char>(Arg(0))) ? 1u : 0u); return true; }
        if (fast_name == "isxdigit") { Ret(std::isxdigit(static_cast<unsigned char>(Arg(0))) ? 1u : 0u); return true; }
        if (fast_name == "strerror") {
            static constexpr char kError[] = "ProjectV7 guest errno";
            WriteBytes(memory_, kBootstrapRuntimeBase + 0x200, kError, sizeof(kError));
            Ret(kBootstrapRuntimeBase + 0x200); return true;
        }
        if (fast_name == "uname") {
            const std::uint32_t p = Arg(0);
            if (!Fits(p, 390)) return false;
            std::memset(memory_.data() + p, 0, 390);
            std::string node_name = "pv7";
            pv7::compat::GetSystemProperty("ro.product.device", node_name);
            WriteCString(p, "Linux", 65);
            WriteCString(p + 65, node_name, 65);
            WriteCString(p + 130, "6.0", 65);
            WriteCString(p + 260, "aarch64", 65);
            Ret(0); return true;
        }
        if (fast_name == "getrusage") {
            if (Arg(1) && Fits(Arg(1), 72)) std::memset(memory_.data() + Arg(1), 0, 72);
            Ret(0); return true;
        }
        if (fast_name == "bsearch") {
            const std::uint32_t key = Arg(0);
            const std::uint32_t base = Arg(1);
            const std::uint32_t nmemb = Arg(2);
            const std::uint32_t size = Arg(3);
            const std::uint32_t compar = Arg(4);
            constexpr std::uint32_t kMonoClassFieldComparator = kMonoBase + 0x00146b38u;
            if (compar == kMonoClassFieldComparator && Fits(key + 58, 2) && size >= 4) {
                const std::uint16_t key_value = Read<std::uint16_t>(key + 58);
                std::uint32_t lo = 0, hi = nmemb;
                while (lo < hi) {
                    const std::uint32_t mid = lo + (hi - lo) / 2;
                    const std::uint64_t element64 = static_cast<std::uint64_t>(base) + static_cast<std::uint64_t>(mid) * size;
                    if (element64 > UINT32_MAX) break;
                    const std::uint32_t element = static_cast<std::uint32_t>(element64);
                    if (!Fits(element, 4)) break;
                    const std::uint32_t object = Read<std::uint32_t>(element);
                    if (!Fits(object + 58, 2)) break;
                    const std::uint16_t element_value = Read<std::uint16_t>(object + 58);
                    if (key_value == element_value) { Ret(element); return true; }
                    if (key_value < element_value) hi = mid;
                    else lo = mid + 1;
                }
                Ret(0);
                return true;
            }
            constexpr std::uint32_t kMonoMetadataRowComparator = kMonoBase + 0x001c9a80u;
            constexpr std::uint32_t kMonoMetadataRangeComparator = kMonoBase + 0x001c9944u;
            if ((compar == kMonoMetadataRowComparator || compar == kMonoMetadataRangeComparator) && Fits(key, 16)) {
                const std::uint32_t target = Read<std::uint32_t>(key);
                const std::uint32_t column = Read<std::uint32_t>(key + 4);
                const std::uint32_t table = Read<std::uint32_t>(key + 8);
                if (!Fits(table, 12)) { Ret(0); return true; }
                const std::uint32_t table_base = Read<std::uint32_t>(table);
                const std::uint32_t row_info = Read<std::uint32_t>(table + 4);
                const std::uint32_t column_info = Read<std::uint32_t>(table + 8);
                const std::uint32_t row_count = row_info & 0x00ffffffu;
                const std::uint32_t row_size = Read<std::uint8_t>(table + 7);
                const std::uint32_t column_count = column_info >> 24;
                if (!row_size || column >= column_count || !size || nmemb > row_count) { Ret(0); return true; }

                auto decode_column = [&](std::uint32_t element, std::uint32_t& value, std::uint32_t& row) -> bool {
                    if (element < table_base) return false;
                    const std::uint32_t delta = element - table_base;
                    row = delta / row_size;
                    if (row >= row_count) return false;
                    std::uint32_t offset = 0;
                    for (std::uint32_t i = 0; i < column; ++i) {
                        offset += ((column_info >> (i * 2)) & 3u) + 1u;
                    }
                    const std::uint32_t width = ((column_info >> (column * 2)) & 3u) + 1u;
                    const std::uint32_t address = table_base + row * row_size + offset;
                    if (!Fits(address, width)) return false;
                    if (width == 1) value = Read<std::uint8_t>(address);
                    else if (width == 2) value = Read<std::uint16_t>(address);
                    else if (width == 4) value = Read<std::uint32_t>(address);
                    else {
                        value = static_cast<std::uint32_t>(memory_[address]) |
                                (static_cast<std::uint32_t>(memory_[address + 1]) << 8) |
                                (static_cast<std::uint32_t>(memory_[address + 2]) << 16);
                    }
                    return true;
                };

                std::uint32_t lo = 0, hi = nmemb;
                while (lo < hi) {
                    const std::uint32_t mid = lo + (hi - lo) / 2;
                    const std::uint64_t element64 = static_cast<std::uint64_t>(base) + static_cast<std::uint64_t>(mid) * size;
                    if (element64 > UINT32_MAX) break;
                    const std::uint32_t element = static_cast<std::uint32_t>(element64);
                    std::uint32_t decoded = 0, row = 0;
                    if (!decode_column(element, decoded, row)) break;

                    int cmp = 0;
                    if (compar == kMonoMetadataRowComparator) {
                        cmp = target < decoded ? -1 : (target > decoded ? 1 : 0);
                    } else {
                        if (target < decoded) {
                            cmp = -1;
                        } else if (row + 1 >= row_count) {
                            cmp = 0;
                        } else {
                            const std::uint32_t next_element = table_base + (row + 1) * row_size;
                            std::uint32_t next_decoded = 0, next_row = 0;
                            if (!decode_column(next_element, next_decoded, next_row)) break;
                            if (target >= next_decoded || decoded == next_decoded) cmp = 1;
                            else cmp = 0;
                        }
                    }

                    if (cmp == 0) {
                        Write<std::uint32_t>(key + 12, row);
                        Ret(element);
                        return true;
                    }
                    if (cmp < 0) hi = mid;
                    else lo = mid + 1;
                }
                Ret(0);
                return true;
            }
            if (nmemb == 0 || size == 0) {
                Ret(0);
                return true;
            }
            const std::uint64_t total = static_cast<std::uint64_t>(nmemb) * size;
            if (total > SIZE_MAX || !Fits(base, static_cast<std::size_t>(total)) ||
                !Fits(compar & ~1u, 4u)) {
                return false;
            }
            BsearchFrame frame;
            frame.key = key;
            frame.base = base;
            frame.size = size;
            frame.comparator = compar;
            frame.low = 0;
            frame.high = nmemb;
            frame.mid = nmemb / 2u;
            frame.caller_lr = jit->Regs()[14];
            bsearch_frames_.push_back(frame);
            const std::uint32_t element = base + frame.mid * size;
            jit->Regs()[0] = key;
            jit->Regs()[1] = element;
            jit->Regs()[2] = 0;
            jit->Regs()[3] = 0;
            jit->Regs()[14] = kBsearchReturnStub;
            jit->Regs()[15] = compar & ~1u;
            jit->SetCpsr((compar & 1u) ? 0x20u : 0u);
            pending_guest_callback = true;
            jit->HaltExecution();
            return true;
        }
        return false;
    }

    struct GuestJumpContext {
        std::array<std::uint32_t, 16> regs{};
        std::uint32_t cpsr = 0;
        std::uint32_t resume_pc = 0;
        std::uint32_t thread_id = 0;
    };
    struct GuestSignalAction {
        std::uint32_t handler = 0;
        std::uint32_t mask = 0;
        std::uint32_t flags = 0;
        std::uint32_t restorer = 0;
    };
    struct GuestSignalFrame {
        std::array<std::uint32_t, 16> regs{};
        std::array<std::uint32_t, 64> ext_regs{};
        std::uint32_t cpsr = 0;
        std::uint32_t fpscr = 0;
        std::uint32_t signal = 0;
        std::uint32_t signal_mask = 0;
        std::uint32_t guest_siginfo = 0;
        std::uint32_t guest_ucontext = 0;
        bool uses_siginfo = false;
    };

    const std::unordered_map<std::uint32_t, std::string>& thunks_;
    const std::unordered_map<std::string, std::uint32_t>& name_to_stub_;
    const std::unordered_map<std::string, ExportSymbol>& exports_;
    std::uint32_t heap_next_ = kGuestHeapStart;
#if defined(PROJECTV7_CNR_CONTENT_HEAP) && PROJECTV7_CNR_CONTENT_HEAP
    std::uint32_t heap_content_next_ = kGuestContentHeapStart;
#endif
#if defined(PROJECTV7_TEST_HEAP_OVERFLOW_32M) && PROJECTV7_TEST_HEAP_OVERFLOW_32M
    std::uint32_t heap_overflow_next_ = kGuestOverflowHeapStart;
#if defined(PROJECTV7_MONO_FATAL_CONTEXT_TRACE) && PROJECTV7_MONO_FATAL_CONTEXT_TRACE && defined(__ANDROID__)
    std::size_t guest_overflow_alloc_count_ = 0u;
#endif
#endif
#if defined(__ANDROID__)
    std::size_t guest_heap_oom_count_ = 0u;
#endif
    std::uint32_t next_tls_key_ = 1;
    enum class GuestThreadState : std::uint8_t {
        New,
        Runnable,
        BlockedMutex,
        BlockedCond,
        BlockedSemaphore,
        BlockedJoin,
        BlockedSignal,
        Finished,
        Faulted,
    };
    struct GuestThreadLaunch {
        std::uint32_t id = 0;
        std::uint32_t start = 0;
        std::uint32_t arg = 0;
        bool started = false;
        bool finished = false;
        GuestThreadState state = GuestThreadState::New;
        std::uint32_t wait_object = 0;
        std::uint32_t wait_aux = 0;
        std::uint32_t wait_owner = 0;
        std::string terminal_detail;
        std::array<std::uint32_t, 16> regs{};
        std::array<std::uint32_t, 64> ext_regs{};
        std::uint32_t cpsr = 0;
        std::uint32_t fpscr = 0;
        std::unordered_map<std::uint32_t, std::uint32_t> tls_values;
        std::uint32_t arm_tls = 0;
        std::size_t total_slices = 0;
    };
    std::uint32_t next_thread_id_ = 2;
    std::uint32_t current_thread_id_ = 1;
    std::uint32_t preferred_guest_thread_id_ = 0;
    std::uint32_t trylock_preferred_guest_thread_id_ = 0;
    std::uint32_t unity_preload_thread_id_ = 0;
    struct GuestThreadSamplingConfig {
        std::uint64_t ticks_per_slice = 1000000u;
        std::uint32_t samples_remaining = 0u;
    };
    std::unordered_map<std::uint32_t, std::string> guest_thread_names_;
    std::unordered_map<std::uint32_t, GuestThreadSamplingConfig> guest_thread_sampling_;
#if defined(PROJECTV7_TEST_WORKER_NANOSLEEP_DEADLINES) && PROJECTV7_TEST_WORKER_NANOSLEEP_DEADLINES
    // Test-only monotonic wakeups: completed worker nanosleep calls remain parked
    // until their requested delay elapses instead of consuming every guest pump.
    std::unordered_map<std::uint32_t, std::chrono::steady_clock::time_point> guest_nanosleep_deadlines_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_nanosleep_trace_count_;
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
    std::unordered_map<std::uint32_t, std::uint64_t> guest_thread_perf_us_;
    std::unordered_map<std::uint32_t, std::uint64_t> guest_thread_perf_visits_;
#endif
     std::uint64_t mono_metadata_decode_row_fast_calls_ = 0;
    std::uint64_t mono_metadata_decode_row_col_fast_calls_ = 0;
    bool cooperative_yield_requested_ = false;
    bool thread_exit_requested_ = false;
    std::vector<GuestThreadLaunch> guest_thread_launches_;
    std::unordered_map<std::uint64_t, std::chrono::steady_clock::time_point> guest_receive_deadlines_;
    std::size_t guest_thread_pump_cursor_ = 0;
    struct GuestMutexState {
        std::uint32_t owner = 0;
        std::uint32_t recursion = 0;
        std::uint32_t type = 0;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
        std::uint32_t acquire_lr = 0;
        std::uint32_t acquire_sp = 0;
        std::uint32_t acquire_pc = 0;
        std::uint32_t acquire_caller_lr = 0;
        std::uint32_t acquire_caller2_lr = 0;
#endif
    };
    struct GuestCondState {
        std::uint64_t broadcast_generation = 0;
    };
    struct GuestCondWaitState {
        std::uint64_t broadcast_generation = 0;
        std::uint32_t mutex = 0;
        bool signaled = false;
        bool timed = false;
        std::chrono::system_clock::time_point deadline{};
    };
    std::unordered_map<std::uint32_t, GuestMutexState> guest_mutexes_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_mutex_attr_types_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_pthread_attr_threads_;
    std::unordered_map<std::uint32_t, GuestCondState> guest_conds_;
    std::unordered_map<std::uint64_t, GuestCondWaitState> guest_cond_waits_;
    std::unordered_map<std::uint32_t, std::int64_t> guest_semaphores_;
    std::unordered_map<std::uint64_t, std::uint32_t> guest_trylock_spin_counts_;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
    std::unordered_map<std::uint64_t, bool> guest_sem_wait_logged_;
    std::unordered_map<std::uint64_t, bool> guest_mutex_block_logged_;
    std::unordered_map<std::uint64_t, std::uint32_t> guest_trylock_busy_log_counts_;
    std::unordered_map<std::uint32_t, std::vector<std::string>> guest_thread_call_traces_;
    std::unordered_map<std::uint32_t, std::vector<std::string>> guest_thread_sync_traces_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_sleep_log_counts_;
#endif
    std::unordered_map<std::uint64_t, std::chrono::system_clock::time_point> guest_sem_deadlines_;
    std::unordered_map<std::uint32_t, std::size_t> allocations_;
    std::map<std::uint32_t, std::size_t> free_blocks_;
    std::unordered_map<std::uint32_t, std::uint32_t> tls_values_;
    std::unordered_map<std::uint32_t, GuestJumpContext> jump_contexts_;
    std::unordered_map<std::uint32_t, GuestSignalAction> guest_signal_actions_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_signal_masks_;
    std::unordered_map<std::uint32_t, std::uint32_t> guest_sigsuspend_saved_masks_;
    std::unordered_map<std::uint32_t, std::vector<std::uint32_t>> guest_pending_signals_;
    std::unordered_map<std::uint32_t, std::vector<GuestSignalFrame>> guest_signal_frames_;
    std::unordered_map<std::uint32_t, bool> guest_signal_interrupted_;
    std::unordered_map<std::uint32_t, JniArrayRecord> jni_arrays_;
    std::vector<BsearchFrame> bsearch_frames_;
    std::vector<QsortFrame> qsort_frames_;
    std::uint32_t strtok_state_ = 0;
    std::uint64_t rand48_state_ = 0x1234abcd330eULL;
    std::unordered_map<std::string, std::string> guest_environment_;
    std::unordered_map<std::uint32_t, Sha1State32> sha1_states_;
    std::unordered_map<std::uint32_t, bool> cxa_guards_in_progress_;
    struct PthreadOnceFrame {
        std::uint32_t control = 0;
        std::uint32_t caller_lr = 0;
    };
    std::vector<PthreadOnceFrame> pthread_once_frames_;
    std::unordered_map<std::uint32_t, bool> pthread_once_done_;
    std::uint32_t arm_tls_value_ = 0;
    std::uint32_t next_file_handle_ = 0x70000000u;
    std::uint32_t next_socket_handle_ = 0x72000000u;
    std::unordered_map<std::uint32_t, bool> socket_handles_;
    std::unordered_map<std::uint32_t, std::FILE*> file_handles_;
    std::unordered_map<std::uint32_t, std::string> file_paths_;
    std::unordered_map<std::uint32_t, VirtualFile> virtual_files_;
#if !defined(_WIN32)
    std::uint32_t next_dir_handle_ = 0x73000000u;
    std::unordered_map<std::uint32_t, DIR*> dir_handles_;
    std::unordered_map<std::uint32_t, std::uint32_t> dir_guest_entries_;
#endif
#if defined(__ANDROID__)
    std::uint32_t next_egl_handle_ = 0x71000000u;
    std::unordered_map<std::uint32_t, std::uintptr_t> egl_handles_;
    std::unordered_map<EGLSurface, EGLDisplay> owned_egl_surfaces_;
    std::unordered_map<EGLContext, EGLDisplay> owned_egl_contexts_;
    std::unordered_map<std::uint32_t, std::uint32_t> gl_string_cache_;
    std::unordered_map<std::uint32_t, GuestGlMappedRange> gl_mapped_ranges_;
    std::unordered_map<std::uint32_t, HostZlibStream> zlib_streams_;
#endif
    std::string managed_dir_;
    void* host_native_window_ = nullptr;
    void* host_jni_env_ = nullptr;
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
    pv7::trace::Recorder call_trace_recorder_;
#endif
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
    HotpatchRuntime hotpatch_runtime_;
#endif

public:
    std::uint32_t GetOrCreateJniClassHandle(const std::string& class_name) {
        const std::uint32_t handle = EnsureJniClassHandle(class_name);
        if (class_name == "com/unity3d/player/WWW") jni_www_class_handle = handle;
        return handle;
    }

    std::size_t PumpHostHttpCallbacks(std::size_t max_requests = 2u) {
        if (host_http_pending_.empty() || max_requests == 0u) return 0u;
        if (!www_done_callback_ || !www_error_callback_ || !www_header_callback_ || !www_read_callback_) {
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_WARN, "CNR64POC",
                                "PV7WWW response waiting for callbacks done=0x%08x error=0x%08x header=0x%08x read=0x%08x",
                                www_done_callback_, www_error_callback_, www_header_callback_, www_read_callback_);
#endif
            return 0u;
        }

        std::size_t pumped = 0u;
        while (pumped < max_requests && !host_http_pending_.empty()) {
            HostHttpPending pending = std::move(host_http_pending_.front());
            host_http_pending_.pop_front();
            bool aborted = false;
            std::uint32_t callback_result = 0u;

            if (!pending.error.empty() || pending.status >= 400 || pending.status < 0) {
                std::string message = !pending.error.empty()
                    ? pending.error : ("HTTP " + std::to_string(pending.status));
                const std::uint32_t string_handle = NewJniString(message);
                InvokeWwwGuestCallback(www_error_callback_, pending.request_id, string_handle, {}, callback_result);
                aborted = true;
            } else {
                if (!pending.headers.empty()) {
                    const std::uint32_t header_handle = NewJniString(pending.headers);
                    if (!InvokeWwwGuestCallback(www_header_callback_, pending.request_id, header_handle,
                                                {}, callback_result) || callback_result != 0u) {
                        aborted = true;
                    }
                }

                constexpr std::size_t kChunkSize = 32u * 1024u;
                for (std::size_t offset = 0; !aborted && offset < pending.body.size(); offset += kChunkSize) {
                    const std::size_t count = std::min(kChunkSize, pending.body.size() - offset);
                    const std::uint32_t array = Allocate(count + 16u, 4u);
                    if (!array || !Fits(array + 16u, count)) {
                        const std::uint32_t error_handle = NewJniString("CNR64 WWW guest buffer allocation failed");
                        InvokeWwwGuestCallback(www_error_callback_, pending.request_id, error_handle, {}, callback_result);
                        aborted = true;
                        break;
                    }
                    jni_arrays_[array] = {static_cast<std::uint32_t>(count), 1u, false};
                    std::memcpy(memory_.data() + array + 16u, pending.body.data() + offset, count);
                    if (!InvokeWwwGuestCallback(www_read_callback_, pending.request_id, array,
                                                {static_cast<std::uint32_t>(count)}, callback_result) ||
                        callback_result != 0u) {
                        const std::uint32_t error_handle = NewJniString("CNR64 WWW request aborted");
                        InvokeWwwGuestCallback(www_error_callback_, pending.request_id, error_handle, {}, callback_result);
                        aborted = true;
                    }
                }

                if (!aborted) {
                    InvokeWwwGuestCallback(www_done_callback_, pending.request_id, 0u, {}, callback_result);
                }
            }
#if defined(__ANDROID__)
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7WWW delivered id=%u status=%d bytes=%zu aborted=%d",
                                pending.request_id, pending.status, pending.body.size(), aborted ? 1 : 0);
#endif
            ++pumped;
        }
        return pumped;
    }

    std::uint32_t NewHostInputString(const std::string& text) { return NewJniString(text); }

    void PumpFrameBackgroundWork() {
        PumpQueuedGuestThreads(32, 4);
        PumpHostHttpCallbacks(2);
        if (++background_snapshot_frames_ % 120u == 0u) {
            std::ofstream snapshot(GuestPrefsPath()+".threads",std::ios::trunc);
            for (const auto& state:DebugGuestThreadStates()) snapshot<<state<<"\n";
            snapshot<<"URL proxies="<<url_streams_.size()<<"\n";
        }
    }
    unsigned background_snapshot_frames_=0;


    std::uint32_t DebugAllocateGuest(std::size_t size, std::uint32_t alignment = 16u) {
        return Allocate(size, alignment);
    }

    bool DebugReadGuest32(std::uint32_t address, std::uint32_t& value) const {
        return Read32(memory_, address, value);
    }

    std::string DebugThunkNameForPc(std::uint32_t pc) const {
        for (const auto& item : name_to_stub_) {
            if (pc == item.second || pc == item.second + 4u) return item.first;
        }
        return {};
    }

#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
    void ResetGuestThreadPerf() {
        guest_thread_perf_us_.clear();
        guest_thread_perf_visits_.clear();
    }

    void ResetCacheflushRangeSummary() {
        cacheflush_range_counts_.clear();
    }

    void LogGuestThreadPerf(const char* label) const {
        std::vector<std::pair<std::uint64_t, std::uint32_t>> ranked;
        ranked.reserve(guest_thread_perf_us_.size());
        for (const auto& item : guest_thread_perf_us_)
            ranked.emplace_back(item.second, item.first);
        std::sort(ranked.begin(), ranked.end(), [](const auto& a, const auto& b) {
            return a.first > b.first;
        });
        const std::size_t count = std::min<std::size_t>(ranked.size(), 16u);
        for (std::size_t i = 0; i < count; ++i) {
            const std::uint32_t id = ranked[i].second;
            const auto name = guest_thread_names_.find(id);
            const auto visits = guest_thread_perf_visits_.find(id);
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7PERF worker-total label=%s thread=%u name=%s total_us=%llu visits=%llu",
                                label, id,
                                name == guest_thread_names_.end() || name->second.empty() ? "-" : name->second.c_str(),
                                static_cast<unsigned long long>(ranked[i].first),
                                static_cast<unsigned long long>(visits == guest_thread_perf_visits_.end() ? 0u : visits->second));
        }
        std::vector<std::pair<std::uint64_t, std::uint64_t>> hot_ranges;
        hot_ranges.reserve(cacheflush_range_counts_.size());
        for (const auto& item : cacheflush_range_counts_)
            hot_ranges.emplace_back(item.second, item.first);
        std::sort(hot_ranges.begin(), hot_ranges.end(), [](const auto& a, const auto& b) {
            return a.first > b.first;
        });
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                            "PV7PERF cacheflush-ranges label=%s unique=%zu", label,
                            cacheflush_range_counts_.size());
        const std::size_t range_count = std::min<std::size_t>(hot_ranges.size(), 8u);
        for (std::size_t i = 0; i < range_count; ++i) {
            const std::uint64_t key = hot_ranges[i].second;
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "PV7PERF cacheflush-range label=%s start=0x%08x size=%u calls=%llu",
                                label, static_cast<std::uint32_t>(key >> 32u),
                                static_cast<std::uint32_t>(key),
                                static_cast<unsigned long long>(hot_ranges[i].first));
        }
    }
#endif
    std::vector<std::string> DebugGuestThreadStates() const {
        std::vector<std::string> result;
        result.reserve(guest_thread_launches_.size());
        auto state_name = [](GuestThreadState state) -> const char* {
            switch (state) {
                case GuestThreadState::New: return "new";
                case GuestThreadState::Runnable: return "runnable";
                case GuestThreadState::BlockedMutex: return "mutex";
                case GuestThreadState::BlockedCond: return "cond";
                case GuestThreadState::BlockedSemaphore: return "sem";
                case GuestThreadState::BlockedJoin: return "join";
                case GuestThreadState::BlockedSignal: return "signal";
                case GuestThreadState::Finished: return "finished";
                case GuestThreadState::Faulted: return "faulted";
            }
            return "unknown";
        };
        for (const auto& thread : guest_thread_launches_) {
            std::ostringstream line;
            line << "id=" << thread.id;
            const auto name = guest_thread_names_.find(thread.id);
            if (name != guest_thread_names_.end() && !name->second.empty())
                line << " name='" << name->second << "'";
            line << " state=" << state_name(thread.state)
                 << " started=" << (thread.started ? "yes" : "no")
                 << " finished=" << (thread.finished ? "yes" : "no")
                 << " pc=0x" << std::hex << thread.regs[15]
                 << " lr=0x" << thread.regs[14]
                 << " wait=0x" << thread.wait_object
                 << " aux=0x" << thread.wait_aux
                 << std::dec << " owner=" << thread.wait_owner
                 << " slices=" << thread.total_slices;
            const std::string thunk = DebugThunkNameForPc(thread.regs[15]);
            if (!thunk.empty()) line << " thunk='" << thunk << "'";
            line << thread.terminal_detail;
            result.push_back(line.str());
        }
        return result;
    }
};

std::string RunMonoBootstrapTrapProbe(GuestMemory memory,
                                      const std::unordered_map<std::string, ExportSymbol>& exports,
                                      const std::vector<UnresolvedRelocation>& unresolved,
                                      const std::string& managed_dir,
                                      const std::string& package_code_path,
                                      void* host_native_window,
                                      void* host_jni_env) {
    std::ostringstream report;
    report << "  Mono JIT bootstrap trap probe:\n";

    ThunkInstallResult thunks;
    std::string error;
    if (!InstallTrapThunks(memory, unresolved, thunks, error)) {
        report << "    Thunk installation: FAIL (" << error << ")";
        return report.str();
    }
    report << "    Function trap stubs installed: " << thunks.name_to_stub.size() << "\n";
    report << "    Data import slots installed: " << thunks.name_to_data.size() << "\n";
    std::uint32_t unity_free_got_after_thunks = 0;
    Read32(memory, kUnityBase + 0x00a89000u, unity_free_got_after_thunks);
    const auto free_stub_after_thunks = thunks.name_to_stub.find("free");
    const std::uint32_t free_stub_after_thunks_address =
        free_stub_after_thunks != thunks.name_to_stub.end() ? free_stub_after_thunks->second : 0u;
    report << "    Unity free GOT after thunk install: got=0x" << std::hex
           << unity_free_got_after_thunks << " stub=0x" << free_stub_after_thunks_address
           << std::dec << "\n";
#if defined(__ANDROID__)
    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                        "Unity free GOT after thunk install: got=0x%08x stub=0x%08x",
                        unity_free_got_after_thunks, free_stub_after_thunks_address);
#endif

    const auto init = FindExport(exports, "mono_jit_init_version");
    if (!init || init->owner != "libmono.so") {
        report << "    mono_jit_init_version lookup: FAIL";
        return report.str();
    }
    report << "    mono_jit_init_version lookup: PASS @ 0x" << std::hex << init->address << std::dec << "\n";

    static constexpr char kDomainName[] = "CNR64 Root Domain";
    static constexpr char kRuntimeVersion[] = "v2.0.50727";
    const std::uint32_t domain_string = kBootstrapStringBase;
    const std::uint32_t runtime_string = kBootstrapStringBase + 0x100;
    const std::uint32_t managed_string = kBootstrapStringBase + 0x200;
    if (!WriteBytes(memory, domain_string, kDomainName, sizeof(kDomainName)) ||
        !WriteBytes(memory, runtime_string, kRuntimeVersion, sizeof(kRuntimeVersion)) ||
        !WriteBytes(memory, managed_string, managed_dir.c_str(), managed_dir.size() + 1) ||
        !WriteReturnStub(memory) ||
        !WriteSvcStub(memory, kPthreadOnceReturnStub, kSvcPthreadOnceReturn)) {
        report << "    Guest bootstrap data setup: FAIL";
        return report.str();
    }
    report << "    Managed assemblies path: " << managed_dir << "\n";

    // These two Mono metadata readers are pure, extremely hot during UnityPreload,
    // and this exact legacy libmono build spends millions of guest instructions in
    // them. Replace only the verified ARM prologues with SVC+BX LR stubs so the
    // compatibility layer can perform the same little-endian row decode natively.
     std::uint32_t mono_decode_row_word0 = 0u, mono_decode_row_word1 = 0u;
    std::uint32_t mono_decode_col_word0 = 0u, mono_decode_col_word1 = 0u;
    const bool mono_metadata_prologues_match =
        Read32(memory, kMonoMetadataDecodeRow, mono_decode_row_word0) &&
        Read32(memory, kMonoMetadataDecodeRow + 4u, mono_decode_row_word1) &&
        Read32(memory, kMonoMetadataDecodeRowCol, mono_decode_col_word0) &&
        Read32(memory, kMonoMetadataDecodeRowCol + 4u, mono_decode_col_word1) &&
        mono_decode_row_word0 == 0xe92d4810u && mono_decode_row_word1 == 0xe28db008u &&
        mono_decode_col_word0 == 0xe92d4830u && mono_decode_col_word1 == 0xe28db00cu;
#if defined(PROJECTV7_TEST_ORIGINAL_MONO_METADATA) && PROJECTV7_TEST_ORIGINAL_MONO_METADATA
    constexpr bool kUseMonoMetadataFastpath = false;
#else
    constexpr bool kUseMonoMetadataFastpath = true;
#endif
    if (mono_metadata_prologues_match && kUseMonoMetadataFastpath) {
        if (!WriteSvcStub(memory, kMonoMetadataDecodeRow, kSvcMonoMetadataDecodeRow) ||
            !WriteSvcStub(memory, kMonoMetadataDecodeRowCol, kSvcMonoMetadataDecodeRowCol)) {
            report << "    Mono metadata native fastpaths: PATCH FAILED\n";
            return report.str();
        }
        report << "    Mono metadata native fastpaths: ARMED row=0x" << std::hex
               << kMonoMetadataDecodeRow << " row_col=0x" << kMonoMetadataDecodeRowCol
               << std::dec << "\n";
    } else {
        report << (kUseMonoMetadataFastpath
                       ? "    Mono metadata native fastpaths: SKIPPED (unexpected libmono prologues)\n"
                       : "    Mono metadata native fastpaths: DISABLED for original guest ARM comparison\n");
    }

     BootstrapEnvironment env(memory, thunks.id_to_name, thunks.name_to_stub, exports, managed_dir, host_native_window, host_jni_env);
#if defined(PROJECTV7_DEV_HOTPATCH) && PROJECTV7_DEV_HOTPATCH
    report << "    Developer hotpatch runtime: " << env.HotpatchStatusLine() << "\n";
#endif
    Dynarmic::ExclusiveMonitor global_monitor{1};
    auto guest_page_table = std::make_unique<std::array<std::uint8_t*, Dynarmic::A32::UserConfig::NUM_PAGE_TABLE_ENTRIES>>();
    guest_page_table->fill(nullptr);
    constexpr std::size_t kGuestPageSize = 1u << Dynarmic::A32::UserConfig::PAGE_BITS;
    for (std::size_t offset = 0; offset + kGuestPageSize <= memory.size(); offset += kGuestPageSize)
        (*guest_page_table)[offset >> Dynarmic::A32::UserConfig::PAGE_BITS] = memory.data() + offset;
#if defined(PROJECTV7_FAULT_CODE_WATCH) && PROJECTV7_FAULT_CODE_WATCH
    // One guest JIT-code page is routed through MemoryWrite callbacks for a
    // causal trace. Production retains the original direct page-table mapping.
    constexpr std::uint32_t kWatchedFaultAddress = 0x06bc3140u;
    (*guest_page_table)[kWatchedFaultAddress >> Dynarmic::A32::UserConfig::PAGE_BITS] = nullptr;
    report << "    Developer JIT-code write watch: " << std::hex
           << kWatchedFaultAddress << std::dec << " (single-page slow memory)\n";
#endif

    Dynarmic::A32::UserConfig config;
    config.callbacks = &env;
    config.processor_id = 0;
    config.global_monitor = &global_monitor;
    config.arch_version = Dynarmic::A32::ArchVersion::v7;
    config.define_unpredictable_behaviour = true;
    config.always_little_endian = true;
    config.enable_cycle_counting = true;
    config.code_cache_size = 32u * 1024u * 1024u;
#if defined(PROJECTV7_TEST_ARM64_NO_BLOCK_LINKING) && PROJECTV7_TEST_ARM64_NO_BLOCK_LINKING && defined(__aarch64__)
    // Dev-only A/B: prevent ARM64 Dynarmic from chaining cached translated
    // blocks. This retains guest instruction semantics and all other
    // optimizations, isolating possible stale ARM64 block links during Mono JIT.
    config.optimizations &= ~Dynarmic::OptimizationFlag::BlockLinking;
#endif
    config.page_table = guest_page_table.get();
#if defined(__ANDROID__) && defined(__aarch64__) && PROJECTV7_TEST_ARM64_FASTMEM
    config.fastmem_pointer = reinterpret_cast<std::uintptr_t>(memory.data());
    config.fastmem_exclusive_access = false;
    report << "    ARM64 guarded fastmem: enabled\n";
#endif

    config.detect_misaligned_access_via_page_table = 8u | 16u | 32u | 64u;
    config.only_detect_misalignment_via_page_table_on_page_boundary = true;
    report << "    Dynarmic guest page table: ENABLED (" << (memory.size() / kGuestPageSize)
           << " mapped pages)\n";
    Dynarmic::A32::Jit jit(config);
    env.jit = &jit;

    constexpr std::uint32_t kUnityCxaThrow = kUnityBase + 0x00939fd0u;
    constexpr std::uint32_t kMainUnwindRaise = kMainBase + 0x00004d74u;
    if (!WriteSvcStub(memory, kUnityCxaThrow, kSvcCxaThrowProbe) ||
        !WriteSvcStub(memory, kMainUnwindRaise, kSvcUnwindRaiseProbe)) {
        report << "    C++ exception probe setup: FAIL\n";
        return report.str();
    }
    report << "    C++ exception probes armed: throw=0x" << std::hex << kUnityCxaThrow
           << " unwind=0x" << kMainUnwindRaise << std::dec << "\n";

    // Android's dynamic linker runs dependency init arrays before JNI_OnLoad or
    // any exported entrypoints. The original Unity binary has hundreds of C++
    // static constructors (std::string globals, registries, locks, etc.); leaving
    // these zeroed causes later code to treat null std::string data as a live rep.
    constexpr std::uint64_t kCtorTicksPerSlice = 100000;
    constexpr int kCtorMaxSlices = 100;
    std::size_t init_array_executed = 0;
    std::size_t init_array_null_entries = 0;

    auto run_guest_initializer = [&](const char* owner, std::size_t index,
                                     std::uint32_t function) -> bool {
        if (function == 0) {
            ++init_array_null_entries;
            return true;
        }
        if (std::string_view(owner) == "libunity.so" && function == kUnityBase + 0x00058ba4u) {
            report << "    ELF init-array diagnostic skip: libunity locale bootstrap fn=0x"
                   << std::hex << function << std::dec << "\n";
            return true;
        }

#if defined(__ANDROID__)
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                            "CTOR enter %s[%zu] fn=0x%08x", owner, index, function);
#endif

        env.ResetPhaseDiagnostics();
        env.saw_return = false;
        env.failed = false;
        env.fault_pc = 0;
        jit.ClearHalt();
        jit.Regs().fill(0);
        jit.Regs()[13] = kStackTop;
        jit.Regs()[14] = kReturnStub;
        jit.Regs()[15] = function & ~1u;
        jit.SetCpsr((function & 1u) ? 0x20u : 0u);

        std::uint32_t previous_pc = 0xffffffffu;
        int same_pc_slices = 0;
        std::size_t cache_restarts = 0;
        int slices = 0;
        for (; slices < kCtorMaxSlices; ++slices) {
            env.ticks_left = kCtorTicksPerSlice;
            const auto halt_reason = jit.Run();
#if defined(__ANDROID__)
            std::uint32_t helper_word = 0;
            std::uint32_t throw_word = 0;
            Read32(memory, kUnityBase + 0x0090d604u, helper_word);
            Read32(memory, kUnityCxaThrow, throw_word);
            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                "CTOR slice %s[%zu] n=%d pc=0x%08x halt=0x%llx helper=0x%08x throw=0x%08x",
                                owner, index, slices, jit.Regs()[15],
                                static_cast<unsigned long long>(halt_reason), helper_word, throw_word);
#endif
            if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
            if (env.pending_guest_callback) {
                env.pending_guest_callback = false;
                jit.ClearHalt();
                continue;
            }
            if (env.pending_cacheflush) {
                const std::uint32_t start = env.pending_cacheflush_start;
                const std::uint32_t size = env.pending_cacheflush_size;
                env.pending_cacheflush = false;
                env.pending_cacheflush_start = 0;
                env.pending_cacheflush_size = 0;
                jit.ClearHalt();
                if (size != 0) env.InvalidateTranslatedCodeRange(start, size);
                ++cache_restarts;
                previous_pc = 0xffffffffu;
                same_pc_slices = 0;
                continue;
            }
            if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                jit.ClearHalt();
                ++cache_restarts;
                previous_pc = 0xffffffffu;
                same_pc_slices = 0;
                continue;
            }
            const std::uint32_t pc = jit.Regs()[15];
            if (pc == previous_pc) ++same_pc_slices;
            else same_pc_slices = 0;
            previous_pc = pc;
            if (same_pc_slices >= 2) break;
        }

        const bool ok = env.saw_return && !env.failed && !env.premature_return && env.first_thunk.empty();
        if (!ok) {
            report << "    ELF init-array failure: " << owner << "[" << index << "] fn=0x"
                   << std::hex << function << std::dec << " slices=" << slices
                   << " cache_restarts=" << cache_restarts << "\n";
            if (!env.first_thunk.empty())
                report << "      blocked by thunk: " << env.first_thunk << " (SVC " << env.first_thunk_id << ")\n";
            if (env.unexpected_svc0_pc != 0)
                report << "      unexpected svc0: pc=0x" << std::hex << env.unexpected_svc0_pc
                       << " r7=0x" << env.unexpected_svc0_r7 << std::dec << "\n";
            if (env.bad_write_address != 0)
                report << "      bad write: addr=0x" << std::hex << env.bad_write_address
                       << " pc=0x" << env.bad_write_pc << " lr=0x" << env.bad_write_lr << std::dec << "\n";
            if (env.failed) {
                report << "      fault_pc=0x" << std::hex << env.fault_pc
                       << " exception=" << std::dec << env.fault_exception
                       << " fallback_count=" << env.fallback_instruction_count << "\n";
                report << "      registers:";
                for (std::size_t r = 0; r < 16; ++r)
                    report << " r" << r << "=0x" << std::hex << jit.Regs()[r] << std::dec;
                report << "\n";
                report << "      code-read trail: 0x" << std::hex << env.code_read_prev3
                       << " 0x" << env.code_read_prev2 << " 0x" << env.code_read_prev1
                       << " 0x" << env.code_read_last << std::dec << "\n";
            }
            if (!env.saw_return && !env.failed && !env.premature_return && env.first_thunk.empty()) {
                report << "      constructor budget exhausted: pc=0x" << std::hex << jit.Regs()[15]
                       << " lr=0x" << jit.Regs()[14] << std::dec << "\n";
            }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
            if (!env.call_trace.empty()) {
                report << "      thunk trace:";
                const std::size_t trace_start = env.call_trace.size() > 24 ? env.call_trace.size() - 24 : 0;
                for (std::size_t i = trace_start; i < env.call_trace.size(); ++i)
                    report << " " << env.call_trace[i];
                report << "\n";
            }
#endif
            return false;
        }
        ++init_array_executed;
        return true;
    };

    auto run_init_array = [&](const char* owner, std::uint32_t array_address,
                              std::size_t count) -> bool {
        for (std::size_t i = 0; i < count; ++i) {
            std::uint32_t function = 0;
            if (!Read32(memory, array_address + static_cast<std::uint32_t>(i * 4u), function)) {
                report << "    ELF init-array read failure: " << owner << "[" << i << "]\n";
                return false;
            }
            if (!run_guest_initializer(owner, i, function)) return false;
        }
        return true;
    };

    // DT_NEEDED ordering for libunity: dependencies first, then libunity itself.
    env.live_ctor_trace = true;
    const bool elf_init_ok =
        run_init_array("libmono.so", kMonoBase + 0x003ac680u, 1) &&
        run_init_array("libmain.so", kMainBase + 0x000073a4u, 1) &&
        run_init_array("libunity.so", kUnityBase + 0x00a87f3cu, 940u / 4u);
    env.live_ctor_trace = false;
    report << "    ELF init arrays: " << (elf_init_ok ? "PASS" : "FAIL")
           << " executed=" << init_array_executed
           << " null_entries=" << init_array_null_entries << "\n";
    if (!elf_init_ok) return report.str();

    const auto set_assemblies = FindExport(exports, "mono_set_assemblies_path");
    bool assemblies_path_ok = false;
    if (set_assemblies && set_assemblies->owner == "libmono.so") {
        env.ticks_left = 5000000;
        jit.Regs().fill(0);
        jit.Regs()[0] = managed_string;
        jit.Regs()[13] = kStackTop;
        jit.Regs()[14] = kReturnStub;
        jit.Regs()[15] = set_assemblies->address & ~1u;
        jit.SetCpsr((set_assemblies->address & 1u) ? 0x20u : 0u);
        jit.Run();
        assemblies_path_ok = env.saw_return && !env.failed && env.first_thunk.empty();
    }
    report << "    mono_set_assemblies_path: " << (assemblies_path_ok ? "PASS" : "FAIL") << "\n";
    if (!assemblies_path_ok) {
        if (!env.first_thunk.empty()) report << "      stopped at thunk: " << env.first_thunk << "\n";
        if (env.failed) report << "      fault_pc=0x" << std::hex << env.fault_pc << std::dec << "\n";
        return report.str();
    }

    env.ResetPhaseDiagnostics();
    env.saw_return = false;
    env.failed = false;
    env.fault_pc = 0;
    env.first_thunk.clear();
    env.first_thunk_id = 0;
    jit.ClearHalt();
    jit.Regs().fill(0);
    jit.Regs()[0] = domain_string;
    jit.Regs()[1] = runtime_string;
    jit.Regs()[13] = kStackTop;
    jit.Regs()[14] = kReturnStub;
    jit.Regs()[15] = init->address & ~1u;
    jit.SetCpsr((init->address & 1u) ? 0x20u : 0u);

    // Real Unity launches must let Unity own Mono initialization. Running
    // mono_jit_init_version here first was useful as an early standalone JIT
    // validation probe, but it makes Unity's later normal initialization a
    // second mono_init_internal() call and trips Mono's domain.c assertion.
    constexpr bool kUnityOwnsMonoInitialization = true;
    constexpr std::uint64_t kTicksPerSlice = 1000000;
    constexpr int kMaxSlices = 100;
    int slices = 0;
    std::size_t cache_invalidation_restarts = 0;
    std::uint32_t previous_pc = 0xffffffffu;
    int same_pc_slices = 0;
    for (; !kUnityOwnsMonoInitialization && slices < kMaxSlices; ++slices) {
        env.ticks_left = kTicksPerSlice;
        const auto halt_reason = jit.Run();
        if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
        if (env.pending_guest_callback) {
            env.pending_guest_callback = false;
            jit.ClearHalt();
            continue;
        }
        if (env.pending_cacheflush) {
            const std::uint32_t start = env.pending_cacheflush_start;
            const std::uint32_t size = env.pending_cacheflush_size;
            env.pending_cacheflush = false;
            env.pending_cacheflush_start = 0;
            env.pending_cacheflush_size = 0;
            jit.ClearHalt();
            if (size != 0) env.InvalidateTranslatedCodeRange(start, size);
            ++cache_invalidation_restarts;
            previous_pc = 0xffffffffu;
            same_pc_slices = 0;
            continue;
        }
        if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
            ++cache_invalidation_restarts;
            previous_pc = 0xffffffffu;
            same_pc_slices = 0;
            continue;
        }
        if (env.ConsumeCooperativeYieldRequest()) {
            env.PumpQueuedGuestThreads(32, 4);
            env.ResetPhaseDiagnostics();
            env.saw_return = false;
            env.premature_return = false;
            env.failed = false;
            env.fault_pc = 0;
            jit.ClearHalt();
            previous_pc = 0xffffffffu;
            same_pc_slices = 0;
            continue;
        }
        const std::uint32_t pc = jit.Regs()[15];
        if (pc == previous_pc) ++same_pc_slices;
        else same_pc_slices = 0;
        previous_pc = pc;
        if (same_pc_slices >= 2) break;
    }

    const bool init_returned = env.saw_return;
    const bool init_premature_return = env.premature_return;
    const std::uint32_t init_return_sp = env.return_sp;
    const std::uint32_t init_return_r11 = env.return_r11;
    const bool init_failed = env.failed;
    const std::uint32_t init_fault_pc = env.fault_pc;
    const std::string init_first_thunk = env.first_thunk;
    const std::uint32_t init_first_thunk_id = env.first_thunk_id;
    const std::uint32_t init_domain = init_returned ? jit.Regs()[0] : 0u;

    // The original Mono startup first stores the created root domain at 0x3b3cb0,
    // then copies it to the global read by mono_get_root_domain() at 0x3b3c78.
    constexpr std::uint32_t kMonoCreatedRootDomainGlobal = kMonoBase + 0x003b3cb0u;
    constexpr std::uint32_t kMonoRootDomainGlobal = kMonoBase + 0x003b3c78u;
    std::uint32_t created_root_global_after_init = 0;
    std::uint32_t root_global_after_init = 0;
    Read32(memory, kMonoCreatedRootDomainGlobal, created_root_global_after_init);
    Read32(memory, kMonoRootDomainGlobal, root_global_after_init);
    bool root_domain_match = false;
    std::uint32_t root_domain_after_init = 0;
    std::uint32_t current_domain_after_init = 0;
    if (init_returned && init_domain != 0) {
        const auto root = FindExport(exports, "mono_get_root_domain");
        if (root && root->owner == "libmono.so") {
            env.saw_return = false;
            env.failed = false;
            env.fault_pc = 0;
            env.first_thunk.clear();
            env.first_thunk_id = 0;
            env.ticks_left = 1000000;
            jit.ClearHalt();
            jit.Regs().fill(0);
            jit.Regs()[13] = kStackTop;
            jit.Regs()[14] = kReturnStub;
            jit.Regs()[15] = root->address & ~1u;
            jit.SetCpsr((root->address & 1u) ? 0x20u : 0u);
            jit.Run();
            root_domain_after_init = jit.Regs()[0];
            root_domain_match = env.saw_return && !env.failed && root_domain_after_init == init_domain;
        }

        const auto current = FindExport(exports, "mono_domain_get");
        if (current && current->owner == "libmono.so") {
            env.saw_return = false;
            env.failed = false;
            env.fault_pc = 0;
            env.first_thunk.clear();
            env.first_thunk_id = 0;
            env.ticks_left = 1000000;
            jit.ClearHalt();
            jit.Regs().fill(0);
            jit.Regs()[13] = kStackTop;
            jit.Regs()[14] = kReturnStub;
            jit.Regs()[15] = current->address & ~1u;
            jit.SetCpsr((current->address & 1u) ? 0x20u : 0u);
            jit.Run();
            if (env.saw_return && !env.failed && env.first_thunk.empty())
                current_domain_after_init = jit.Regs()[0];
        }
    }

    report << "    Execution slices: " << (slices + 1) << " x up to " << kTicksPerSlice << " ticks\n";
    report << "    Cache invalidation restarts: " << cache_invalidation_restarts << "\n";
    report << "    Implemented thunk calls handled: " << env.handled_calls << "\n";
    report << "    Executable mprotect calls: " << env.executable_mprotect_calls;
    if (env.first_exec_region) report << " first=0x" << std::hex << env.first_exec_region << "+0x" << env.first_exec_region_size << std::dec;
    report << "\n";
    report << "    ARM cacheflush syscalls: " << env.arm_cacheflush_calls << "\n";
    report << "    Dynarmic fetched code from Mono guest heap: " << (env.executed_heap_code ? "YES" : "no");
    if (env.executed_heap_code) report << " first_pc=0x" << std::hex << env.first_heap_code_pc << std::dec;
    report << "\n";
    report << "    Saved mono_jit_init_version return: 0x" << std::hex << init_domain << std::dec << "\n";
    report << "    Root globals after init: created=0x" << std::hex << created_root_global_after_init
           << " published=0x" << root_global_after_init << std::dec << "\n";
    if (init_domain != 0) {
        report << "    Root domain validation: " << (root_domain_match ? "PASS" : "FAIL")
               << " init=0x" << std::hex << init_domain
               << " get_root=0x" << root_domain_after_init
               << " domain_get=0x" << current_domain_after_init << std::dec << "\n";
    }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
    if (!env.call_trace.empty()) {
        report << "    Early thunk trace:";
        for (const auto& name : env.call_trace) report << " " << name;
        report << "\n";
    }
    if (!env.allocation_trace.empty()) {
        report << "    Allocation trace:";
        for (const auto& item : env.allocation_trace)
            report << " 0x" << std::hex << item.first << ":0x" << item.second << std::dec;
        report << "\n";
    }
    if (!env.file_trace.empty()) {
        report << "    File trace:\n";
        for (const auto& item : env.file_trace) report << "      " << item << "\n";
    }
    if (!env.diagnostic_formats.empty()) {
        report << "    Guest diagnostics:\n";
        for (const auto& text : env.diagnostic_formats) report << "      " << text << "\n";
    }
#endif
    if (kUnityOwnsMonoInitialization) {
        report << "    Mono pre-initialization: SKIPPED (Unity owns runtime initialization)\n";
    } else if (init_premature_return) {
        report << "    PREMATURE sentinel return: sp=0x" << std::hex << init_return_sp
               << " expected=0x" << kStackTop << " r11=0x" << init_return_r11
               << " r0=0x" << env.premature_r0 << std::dec << "\n";
        if (!env.premature_frames.empty()) {
            report << "    Guest frame unwind (fp -> saved lr):";
            for (const auto& frame : env.premature_frames)
                report << " 0x" << std::hex << frame.first << "->0x" << frame.second << std::dec;
        }
    } else if (!init_first_thunk.empty()) {
        report << "    Next unsupported external thunk: " << init_first_thunk
               << " (SVC " << init_first_thunk_id << ")\n";
        report << "    Bootstrap dispatch: PASS (advanced through implemented services, stopped safely)";
    } else if (init_returned) {
        report << "    mono_jit_init_version RETURNED, saved r0=0x"
               << std::hex << init_domain << std::dec;
    } else if (init_failed) {
        report << "    Bootstrap execution: FAIL before unsupported thunk, fault_pc=0x"
               << std::hex << init_fault_pc << std::dec;
    } else {
        report << "    Bootstrap execution stopped without return/thunk, pc=0x"
               << std::hex << jit.Regs()[15] << " sp=0x" << jit.Regs()[13]
               << " lr=0x" << jit.Regs()[14] << " r0=0x" << jit.Regs()[0]
               << " r1=0x" << jit.Regs()[1] << " r2=0x" << jit.Regs()[2]
               << " r3=0x" << jit.Regs()[3] << std::dec;
    }
    if (kUnityOwnsMonoInitialization || (init_returned && init_domain != 0 && root_domain_match)) {
        const auto unity_jni = FindExport(exports, "libunity.so!JNI_OnLoad");
        report << "    Shared-state Unity JNI lookup: ";
        if (!unity_jni) {
            report << "FAIL (libunity.so JNI_OnLoad not indexed)\n";
        } else {
            report << "PASS owner=" << unity_jni->owner << " @ 0x"
                   << std::hex << unity_jni->address << std::dec << "\n";

            bool jni_stubs_ok =
                WriteSvcStub(memory, kJniAttachStub, kSvcJniAttach) &&
                WriteSvcStub(memory, kJniFindClassStub, kSvcJniFindClass) &&
                WriteSvcStub(memory, kJniRegisterNativesStub, kSvcJniRegisterNatives) &&
                WriteSvcStub(memory, kJniFatalErrorStub, kSvcJniFatalError) &&
                WriteSvcStub(memory, kJniGetJavaVmStub, kSvcJniGetJavaVm) &&
                WriteSvcStub(memory, kJniGetEnvStub, kSvcJniGetEnv) &&
                WriteSvcStub(memory, kJniNewGlobalRefStub, kSvcJniNewGlobalRef) &&
                WriteSvcStub(memory, kJniDeleteGlobalRefStub, kSvcJniDeleteGlobalRef) &&
                WriteSvcStub(memory, kJniGetObjectClassStub, kSvcJniGetObjectClass) &&
                WriteSvcStub(memory, kJniExceptionCheckStub, kSvcJniExceptionCheck) &&
                WriteSvcStub(memory, kJniPushLocalFrameStub, kSvcJniPushLocalFrame) &&
                WriteSvcStub(memory, kJniPopLocalFrameStub, kSvcJniPopLocalFrame) &&
                WriteSvcStub(memory, kJniGetStaticFieldIdStub, kSvcJniGetStaticFieldId) &&
                WriteSvcStub(memory, kJniGetStaticObjectFieldStub, kSvcJniGetStaticObjectField) &&
                WriteSvcStub(memory, kJniGetStaticIntFieldStub, kSvcJniGetStaticIntField) &&
                WriteSvcStub(memory, kJniCallIntMethodStub, kSvcJniCallIntMethod) &&
                WriteSvcStub(memory, kJniCallIntMethodVStub, kSvcJniCallIntMethodV) &&
                WriteSvcStub(memory, kJniCallIntMethodAStub, kSvcJniCallIntMethodA) &&
                WriteSvcStub(memory, kJniCallBooleanMethodStub, kSvcJniCallBooleanMethod) &&
                WriteSvcStub(memory, kJniCallBooleanMethodVStub, kSvcJniCallBooleanMethodV) &&
                WriteSvcStub(memory, kJniCallBooleanMethodAStub, kSvcJniCallBooleanMethodA) &&
                WriteSvcStub(memory, kJniCallVoidMethodStub, kSvcJniCallVoidMethod) &&
                WriteSvcStub(memory, kJniCallVoidMethodVStub, kSvcJniCallVoidMethodV) &&
                WriteSvcStub(memory, kJniCallVoidMethodAStub, kSvcJniCallVoidMethodA) &&
                WriteSvcStub(memory, kJniIsInstanceOfStub, kSvcJniIsInstanceOf) &&
                WriteSvcStub(memory, kJniGetStringUtfLengthStub, kSvcJniGetStringUtfLength) &&
                WriteSvcStub(memory, kJniGetStringUtfCharsStub, kSvcJniGetStringUtfChars) &&
                WriteSvcStub(memory, kJniReleaseStringUtfCharsStub, kSvcJniReleaseStringUtfChars) &&
                WriteSvcStub(memory, kJniAllocObjectStub, kSvcJniAllocObject) &&
                WriteSvcStub(memory, kJniNewObjectStub, kSvcJniNewObject) &&
                WriteSvcStub(memory, kJniNewObjectVStub, kSvcJniNewObjectV) &&
                WriteSvcStub(memory, kJniNewObjectAStub, kSvcJniNewObjectA) &&
                WriteSvcStub(memory, kJniGetFieldIdStub, kSvcJniGetFieldId) &&
                WriteSvcStub(memory, kJniGetObjectFieldStub, kSvcJniGetObjectField) &&
                WriteSvcStub(memory, kJniGetPrimitiveFieldStub, kSvcJniGetPrimitiveField) &&
                WriteSvcStub(memory, kJniSetFieldStub, kSvcJniSetField) &&
                WriteSvcStub(memory, kJniNewBooleanArrayStub, kSvcJniNewBooleanArray) &&
                WriteSvcStub(memory, kJniNewByteArrayStub, kSvcJniNewByteArray) &&
                WriteSvcStub(memory, kJniNewCharArrayStub, kSvcJniNewCharArray) &&
                WriteSvcStub(memory, kJniNewShortArrayStub, kSvcJniNewShortArray) &&
                WriteSvcStub(memory, kJniNewIntArrayStub, kSvcJniNewIntArray) &&
                WriteSvcStub(memory, kJniNewLongArrayStub, kSvcJniNewLongArray) &&
                WriteSvcStub(memory, kJniNewFloatArrayStub, kSvcJniNewFloatArray) &&
                WriteSvcStub(memory, kJniNewDoubleArrayStub, kSvcJniNewDoubleArray) &&
                WriteSvcStub(memory, kJniGetArrayLengthStub, kSvcJniGetArrayLength) &&
                WriteSvcStub(memory, kJniNewObjectArrayStub, kSvcJniNewObjectArray) &&
                WriteSvcStub(memory, kJniGetObjectArrayElementStub, kSvcJniGetObjectArrayElement) &&
                WriteSvcStub(memory, kJniSetObjectArrayElementStub, kSvcJniSetObjectArrayElement) &&
                WriteSvcStub(memory, kJniGetPrimitiveArrayElementsStub, kSvcJniGetPrimitiveArrayElements) &&
                WriteSvcStub(memory, kJniReleasePrimitiveArrayElementsStub, kSvcJniReleasePrimitiveArrayElements) &&
                WriteSvcStub(memory, kJniGetPrimitiveArrayRegionStub, kSvcJniGetPrimitiveArrayRegion) &&
                WriteSvcStub(memory, kJniSetPrimitiveArrayRegionStub, kSvcJniSetPrimitiveArrayRegion) &&
                WriteSvcStub(memory, kJniGetStaticMethodIdStub, kSvcJniGetStaticMethodId) &&
                WriteSvcStub(memory, kJniCallStaticObjectMethodStub, kSvcJniCallStaticObjectMethod) &&
                WriteSvcStub(memory, kJniCallStaticPrimitiveMethodStub, kSvcJniCallStaticPrimitiveMethod) &&
                WriteSvcStub(memory, kJniCallStaticVoidMethodStub, kSvcJniCallStaticVoidMethod) &&
                WriteSvcStub(memory, kJniNewStringUtfStub, kSvcJniNewStringUtf) &&
                WriteSvcStub(memory, kJniGetMethodIdStub, kSvcJniGetMethodId) &&
                WriteSvcStub(memory, kJniCallObjectMethodStub, kSvcJniCallObjectMethod) &&
                WriteSvcStub(memory, kJniCallObjectMethodVStub, kSvcJniCallObjectMethodV) &&
                WriteSvcStub(memory, kJniCallObjectMethodAStub, kSvcJniCallObjectMethodA) &&
                WriteSvcStub(memory, kJniDeleteLocalRefStub, kSvcJniDeleteLocalRef) &&
                WriteSvcStub(memory, kJniNewLocalRefStub, kSvcJniNewLocalRef) &&
                WriteSvcStub(memory, kJniIsSameObjectStub, kSvcJniIsSameObject) &&
                WriteSvcStub(memory, kJniExceptionOccurredStub, kSvcJniExceptionOccurred) &&
                WriteSvcStub(memory, kJniExceptionDescribeStub, kSvcJniExceptionDescribe) &&
                WriteSvcStub(memory, kJniExceptionClearStub, kSvcJniExceptionClear) &&
                WriteSvcStub(memory, kPthreadOnceReturnStub, kSvcPthreadOnceReturn) &&
                InitializeJniVmTable(memory) &&
                Write32(memory, kJniEnvObject, kJniEnvTable) &&
                Write32(memory, kJniEnvTable + 0x18u, kJniFindClassStub) &&
                Write32(memory, kJniEnvTable + 0x3cu, kJniExceptionOccurredStub) &&
                Write32(memory, kJniEnvTable + 0x40u, kJniExceptionDescribeStub) &&
                Write32(memory, kJniEnvTable + 0x44u, kJniExceptionClearStub) &&
                Write32(memory, kJniEnvTable + 0x48u, kJniFatalErrorStub) &&
                Write32(memory, kJniEnvTable + 0x4cu, kJniPushLocalFrameStub) &&
                Write32(memory, kJniEnvTable + 0x50u, kJniPopLocalFrameStub) &&
                Write32(memory, kJniEnvTable + 0x54u, kJniNewGlobalRefStub) &&
                Write32(memory, kJniEnvTable + 0x58u, kJniDeleteGlobalRefStub) &&
                Write32(memory, kJniEnvTable + 0x5cu, kJniDeleteLocalRefStub) &&
                Write32(memory, kJniEnvTable + 0x60u, kJniIsSameObjectStub) &&
                Write32(memory, kJniEnvTable + 0x64u, kJniNewLocalRefStub) &&
                Write32(memory, kJniEnvTable + 0x6cu, kJniAllocObjectStub) &&
                Write32(memory, kJniEnvTable + 0x70u, kJniNewObjectStub) &&
                Write32(memory, kJniEnvTable + 0x74u, kJniNewObjectVStub) &&
                Write32(memory, kJniEnvTable + 0x78u, kJniNewObjectAStub) &&
                Write32(memory, kJniEnvTable + 0x7cu, kJniGetObjectClassStub) &&
                Write32(memory, kJniEnvTable + 0x80u, kJniIsInstanceOfStub) &&
                Write32(memory, kJniEnvTable + 0x84u, kJniGetMethodIdStub) &&
                // JNINativeInterface instance method-call families. Keep the table
                // indices exactly aligned with jni.h, including the primitive groups
                // that this old Unity build reaches through ReflectionHelper.
                Write32(memory, kJniEnvTable + 34u * 4u, kJniCallObjectMethodStub) &&
                Write32(memory, kJniEnvTable + 35u * 4u, kJniCallObjectMethodVStub) &&
                Write32(memory, kJniEnvTable + 36u * 4u, kJniCallObjectMethodAStub) &&
                Write32(memory, kJniEnvTable + 37u * 4u, kJniCallBooleanMethodStub) &&
                Write32(memory, kJniEnvTable + 38u * 4u, kJniCallBooleanMethodVStub) &&
                Write32(memory, kJniEnvTable + 39u * 4u, kJniCallBooleanMethodAStub) &&
                Write32(memory, kJniEnvTable + 40u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 41u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 42u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 43u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 44u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 45u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 46u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 47u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 48u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 49u * 4u, kJniCallIntMethodStub) &&
                Write32(memory, kJniEnvTable + 50u * 4u, kJniCallIntMethodVStub) &&
                Write32(memory, kJniEnvTable + 51u * 4u, kJniCallIntMethodAStub) &&
                Write32(memory, kJniEnvTable + 52u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 53u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 54u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 55u * 4u, kJniUnknownStubBase + 55u * 8u) &&
                Write32(memory, kJniEnvTable + 56u * 4u, kJniUnknownStubBase + 56u * 8u) &&
                Write32(memory, kJniEnvTable + 57u * 4u, kJniUnknownStubBase + 57u * 8u) &&
                Write32(memory, kJniEnvTable + 58u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 59u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 60u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 61u * 4u, kJniCallVoidMethodStub) &&
                Write32(memory, kJniEnvTable + 62u * 4u, kJniCallVoidMethodVStub) &&
                Write32(memory, kJniEnvTable + 63u * 4u, kJniCallVoidMethodAStub) &&
                // Nonvirtual calls have the same return ABI. The extra jclass argument
                // does not need interpretation for the compatibility object model.
                Write32(memory, kJniEnvTable + 64u * 4u, kJniCallObjectMethodStub) &&
                Write32(memory, kJniEnvTable + 65u * 4u, kJniCallObjectMethodVStub) &&
                Write32(memory, kJniEnvTable + 66u * 4u, kJniCallObjectMethodAStub) &&
                Write32(memory, kJniEnvTable + 67u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 68u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 69u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 70u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 71u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 72u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 73u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 74u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 75u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 76u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 77u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 78u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 79u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 80u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 81u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 82u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 83u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 84u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 85u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 86u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 87u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 88u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 89u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 90u * 4u, kJniCallStaticPrimitiveMethodStub) &&
                Write32(memory, kJniEnvTable + 91u * 4u, kJniCallVoidMethodStub) &&
                Write32(memory, kJniEnvTable + 92u * 4u, kJniCallVoidMethodVStub) &&
                Write32(memory, kJniEnvTable + 93u * 4u, kJniCallVoidMethodAStub) &&
                Write32(memory, kJniEnvTable + 0x178u, kJniGetFieldIdStub) &&
                Write32(memory, kJniEnvTable + 0x17cu, kJniGetObjectFieldStub) &&
                Write32(memory, kJniEnvTable + 0x180u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x184u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x188u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x18cu, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x190u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x194u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x198u, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x19cu, kJniGetPrimitiveFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1a0u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1a4u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1a8u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1acu, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1b0u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1b4u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1b8u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1bcu, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x1c0u, kJniSetFieldStub) &&
                Write32(memory, kJniEnvTable + 0x2acu, kJniGetArrayLengthStub) &&
                Write32(memory, kJniEnvTable + 0x2b0u, kJniNewObjectArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2b4u, kJniGetObjectArrayElementStub) &&
                Write32(memory, kJniEnvTable + 0x2b8u, kJniSetObjectArrayElementStub) &&
                Write32(memory, kJniEnvTable + 0x2bcu, kJniNewBooleanArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2c0u, kJniNewByteArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2c4u, kJniNewCharArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2c8u, kJniNewShortArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2ccu, kJniNewIntArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2d0u, kJniNewLongArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2d4u, kJniNewFloatArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2d8u, kJniNewDoubleArrayStub) &&
                Write32(memory, kJniEnvTable + 0x2dcu, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2e0u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2e4u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2e8u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2ecu, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2f0u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2f4u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2f8u, kJniGetPrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x2fcu, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x300u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x304u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x308u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x30cu, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x310u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x314u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x318u, kJniReleasePrimitiveArrayElementsStub) &&
                Write32(memory, kJniEnvTable + 0x31cu, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x320u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x324u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x328u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x32cu, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x330u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x334u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x338u, kJniGetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x33cu, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x340u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x344u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x348u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x34cu, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x350u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x354u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x358u, kJniSetPrimitiveArrayRegionStub) &&
                Write32(memory, kJniEnvTable + 0x35cu, kJniRegisterNativesStub) &&
                Write32(memory, kJniEnvTable + 0x240u, kJniGetStaticFieldIdStub) &&
                Write32(memory, kJniEnvTable + 0x244u, kJniGetStaticObjectFieldStub) &&
                Write32(memory, kJniEnvTable + 0x258u, kJniGetStaticIntFieldStub) &&
                Write32(memory, kJniEnvTable + 0x29cu, kJniNewStringUtfStub) &&
                Write32(memory, kJniEnvTable + 0x2a0u, kJniGetStringUtfLengthStub) &&
                Write32(memory, kJniEnvTable + 0x2a4u, kJniGetStringUtfCharsStub) &&
                Write32(memory, kJniEnvTable + 0x2a8u, kJniReleaseStringUtfCharsStub) &&
                Write32(memory, kJniEnvTable + 0x36cu, kJniGetJavaVmStub) &&
                Write32(memory, kJniEnvTable + 0x390u, kJniExceptionCheckStub);

            if (jni_stubs_ok) {
                jni_stubs_ok =
                    Write32(memory, kJniEnvTable + 113u * 4u, kJniGetStaticMethodIdStub) &&
                    Write32(memory, kJniEnvTable + 114u * 4u, kJniCallStaticObjectMethodStub);
                for (std::uint32_t slot = 117u; jni_stubs_ok && slot <= 140u; ++slot)
                    jni_stubs_ok = Write32(memory, kJniEnvTable + slot * 4u, kJniCallStaticPrimitiveMethodStub);
                for (std::uint32_t slot = 141u; jni_stubs_ok && slot <= 143u; ++slot)
                    jni_stubs_ok = Write32(memory, kJniEnvTable + slot * 4u, kJniCallStaticVoidMethodStub);
            }

            if (jni_stubs_ok) jni_stubs_ok = InitializeJniFallbackStubs(memory);

            if (!jni_stubs_ok) {
                report << "    Shared-state Unity JNI setup: FAIL writing VM/env tables\n";
            } else {
                env.ResetPhaseDiagnostics();
                env.saw_return = false;
                env.premature_return = false;
                env.failed = false;
                env.fault_pc = 0;
                env.ticks_left = kTicksPerSlice;
                jit.ClearHalt();
                jit.Regs().fill(0);
                jit.Regs()[0] = kJniVmObject;
                jit.Regs()[1] = 0;
                jit.Regs()[13] = kStackTop;
                jit.Regs()[14] = kReturnStub;
                jit.Regs()[15] = unity_jni->address & ~1u;
                jit.SetCpsr((unity_jni->address & 1u) ? 0x20u : 0u);

                constexpr int kUnityJniMaxSlices = 20;
                int unity_jni_slices = 0;
                std::size_t unity_jni_cache_restarts = 0;
                std::uint32_t unity_previous_pc = 0xffffffffu;
                int unity_same_pc_slices = 0;
                for (; unity_jni_slices < kUnityJniMaxSlices; ++unity_jni_slices) {
                    env.ticks_left = kTicksPerSlice;
                    const auto halt_reason = jit.Run();
                    if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
                    if (env.pending_guest_callback) {
                        env.pending_guest_callback = false;
                        jit.ClearHalt();
                        continue;
                    }
                    if (env.pending_cacheflush) {
                        const std::uint32_t start = env.pending_cacheflush_start;
                        const std::uint32_t size = env.pending_cacheflush_size;
                        env.pending_cacheflush = false;
                        env.pending_cacheflush_start = 0;
                        env.pending_cacheflush_size = 0;
                        jit.ClearHalt();
                        if (size != 0) env.InvalidateTranslatedCodeRange(start, size);
                        ++unity_jni_cache_restarts;
                        unity_previous_pc = 0xffffffffu;
                        unity_same_pc_slices = 0;
                        continue;
                    }
                    if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                        ++unity_jni_cache_restarts;
                        unity_previous_pc = 0xffffffffu;
                        unity_same_pc_slices = 0;
                        continue;
                    }
                    const std::uint32_t pc = jit.Regs()[15];
                    if (pc == unity_previous_pc) ++unity_same_pc_slices;
                    else unity_same_pc_slices = 0;
                    unity_previous_pc = pc;
                    if (unity_same_pc_slices >= 2) break;
                }

                const std::uint32_t unity_jni_result = env.saw_return ? jit.Regs()[0] : 0u;
                report << "    Shared-state Unity JNI execution: slices=" << unity_jni_slices
                       << " cache_restarts=" << unity_jni_cache_restarts
                       << " returned=" << (env.saw_return ? "YES" : "NO")
                       << " result=0x" << std::hex << unity_jni_result << std::dec << "\n";
                report << "    Shared-state Unity JNI AttachCurrentThread: "
                       << (env.jni_attach_seen ? "YES" : "NO") << "\n";
                report << "    Shared-state Unity JNI classes (" << env.jni_requested_classes.size() << "):\n";
                for (const auto& class_name : env.jni_requested_classes) {
                    report << "      " << class_name << "\n";
                }
                report << "    Shared-state Unity JNI native registrations ("
                       << env.jni_native_registrations.size() << "):\n";
                std::uint32_t unity_init_jni = 0;
                std::uint32_t unity_native_file = 0;
                std::uint32_t unity_native_init_www = 0;
                std::uint32_t unity_native_set_default_display = 0;
                std::uint32_t unity_native_recreate_gfx_state = 0;
                std::uint32_t unity_native_resize = 0;
                std::uint32_t unity_native_resume = 0;
                std::uint32_t unity_native_pause = 0;
                std::uint32_t unity_native_focus_changed = 0;
                std::uint32_t unity_native_render = 0;
                std::uint32_t unity_native_inject_event = 0;
                std::uint32_t unity_input_string=0,unity_input_closed=0,unity_input_canceled=0;
                std::uint32_t unity_fmod_init_jni = 0;
                std::uint32_t unity_fmod_get_info = 0;
                std::uint32_t unity_fmod_process = 0;
                std::uint32_t unity_fmod_block_streaming = 0;
                std::uint32_t unity_fmod_unblock_streaming = 0;
                for (const auto& reg : env.jni_native_registrations) {
                    if(reg.class_name=="com/unity3d/player/UnityPlayer") {
                        if(reg.name=="nativeSetInputString")unity_input_string=reg.function;
                        else if(reg.name=="nativeSoftInputClosed")unity_input_closed=reg.function;
                        else if(reg.name=="nativeSetInputCanceled")unity_input_canceled=reg.function;
                    }
                    report << "      " << reg.class_name << " :: " << reg.name << reg.signature
                           << " -> 0x" << std::hex << reg.function << std::dec << "\n";
                    if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                        reg.name == "initJni" && reg.signature == "(Landroid/content/Context;)V") {
                        unity_init_jni = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeFile" && reg.signature == "(Ljava/lang/String;)V") {
                        unity_native_file = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeInitWWW" && reg.signature == "(Ljava/lang/Class;)V") {
                        unity_native_init_www = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeSetDefaultDisplay" && reg.signature == "(I)V") {
                        unity_native_set_default_display = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeRecreateGfxState" && reg.signature == "(Landroid/view/Surface;)V") {
                        unity_native_recreate_gfx_state = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeResize" && reg.signature == "(IIII)V") {
                        unity_native_resize = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeResume" && reg.signature == "()V") {
                        unity_native_resume = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativePause" && reg.signature == "()Z") {
                        unity_native_pause = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeFocusChanged" && reg.signature == "(Z)V") {
                        unity_native_focus_changed = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeRender" && reg.signature == "()Z") {
                        unity_native_render = reg.function;
                    } else if (reg.class_name == "com/unity3d/player/UnityPlayer" &&
                               reg.name == "nativeInjectEvent" &&
                               reg.signature == "(Landroid/view/InputEvent;)Z") {
                        unity_native_inject_event = reg.function;
                    } else if (reg.class_name == "org/fmod/FMODAudioDevice" &&
                               reg.name == "fmodInitJni" && reg.signature == "()I") {
                        unity_fmod_init_jni = reg.function;
                    } else if (reg.class_name == "org/fmod/FMODAudioDevice" &&
                               reg.name == "fmodGetInfo" && reg.signature == "(I)I") {
                        unity_fmod_get_info = reg.function;
                    } else if (reg.class_name == "org/fmod/FMODAudioDevice" &&
                               reg.name == "fmodProcess" &&
                               reg.signature == "(Ljava/nio/ByteBuffer;)I") {
                        unity_fmod_process = reg.function;
                    } else if (reg.class_name == "org/fmod/FMODAudioDevice" &&
                               reg.name == "fmodBlockStreaming" && reg.signature == "()I") {
                        unity_fmod_block_streaming = reg.function;
                    } else if (reg.class_name == "org/fmod/FMODAudioDevice" &&
                               reg.name == "fmodUnblockStreaming" && reg.signature == "()I") {
                        unity_fmod_unblock_streaming = reg.function;
                    }
                }
                const bool unity_jni_ready = env.saw_return && !env.failed && env.first_thunk.empty() &&
                                             !env.jni_fatal_error_seen && unity_jni_result == 0x00010006u;
                if (!env.first_thunk.empty()) {
                    report << "    Shared-state Unity JNI blocked by thunk: " << env.first_thunk
                           << " (SVC " << env.first_thunk_id << ")\n";
                }
                if (env.failed) {
                    report << "    Shared-state Unity JNI fault PC: 0x" << std::hex
                           << env.fault_pc << std::dec << "\n";
                }
                if (env.jni_fatal_error_seen) {
                    report << "    Shared-state Unity JNI FatalError: YES\n";
                }

                report << "    Shared-state Unity initJni lookup: ";
                if (!unity_jni_ready) {
                    report << "SKIPPED because JNI_OnLoad did not complete cleanly\n";
                } else if (unity_init_jni == 0) {
                    report << "FAIL (registered initJni not found)\n";
                } else {
                    report << "PASS @ 0x" << std::hex << unity_init_jni << std::dec << "\n";

                    for (const auto& patch : pv7::compat::StartupInstructionPatches()) {
                        std::uint32_t actual_instruction = 0;
                        const bool applied =
                            Read32(memory, patch.address, actual_instruction) &&
                            actual_instruction == patch.expected_instruction &&
                            Write32(memory, patch.address, patch.replacement_instruction);
                        if (applied) env.InvalidateTranslatedCodeRange(patch.address, 4);
                        report << "    Compat patch "
                               << (patch.name ? patch.name : "(unnamed)") << ": "
                               << (applied ? "APPLIED" : "SKIPPED")
                               << " instruction=0x" << std::hex << actual_instruction
                               << " address=0x" << patch.address << std::dec << "\n";
                    }

                    env.ResetPhaseDiagnostics();
                    env.saw_return = false;
                    env.premature_return = false;
                    env.failed = false;
                    env.fault_pc = 0;
                    env.ticks_left = kTicksPerSlice;
                    jit.ClearHalt();
                    jit.Regs().fill(0);
                    jit.Regs()[0] = kJniEnvObject;
                    jit.Regs()[1] = kJniFakeClassHandle;
                    jit.Regs()[2] = kJniFakeContextHandle;
                    jit.Regs()[13] = kStackTop;
                    jit.Regs()[14] = kReturnStub;
                    jit.Regs()[15] = unity_init_jni & ~1u;
                    jit.SetCpsr((unity_init_jni & 1u) ? 0x20u : 0u);

                    constexpr int kUnityInitJniMaxSlices = 30;
                    int unity_init_jni_slices = 0;
                    std::size_t unity_init_jni_cache_restarts = 0;
                    std::uint32_t unity_init_previous_pc = 0xffffffffu;
                    int unity_init_same_pc_slices = 0;
                    for (; unity_init_jni_slices < kUnityInitJniMaxSlices; ++unity_init_jni_slices) {
                        env.ticks_left = kTicksPerSlice;
                        const auto halt_reason = jit.Run();
                        if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
                        if (env.pending_guest_callback) {
                            env.pending_guest_callback = false;
                            jit.ClearHalt();
                            continue;
                        }
                        if (env.pending_cacheflush) {
                            const std::uint32_t start = env.pending_cacheflush_start;
                            const std::uint32_t size = env.pending_cacheflush_size;
                            env.pending_cacheflush = false;
                            env.pending_cacheflush_start = 0;
                            env.pending_cacheflush_size = 0;
                            jit.ClearHalt();
                            if (size != 0) env.InvalidateTranslatedCodeRange(start, size);
                            ++unity_init_jni_cache_restarts;
                            unity_init_previous_pc = 0xffffffffu;
                            unity_init_same_pc_slices = 0;
                            continue;
                        }
                        if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                            ++unity_init_jni_cache_restarts;
                            unity_init_previous_pc = 0xffffffffu;
                            unity_init_same_pc_slices = 0;
                            continue;
                        }
                        const std::uint32_t pc = jit.Regs()[15];
                        if (pc == unity_init_previous_pc) ++unity_init_same_pc_slices;
                        else unity_init_same_pc_slices = 0;
                        unity_init_previous_pc = pc;
                        if (unity_init_same_pc_slices >= 2) break;
                    }

                    report << "    Shared-state Unity initJni execution: slices=" << unity_init_jni_slices
                           << " cache_restarts=" << unity_init_jni_cache_restarts
                           << " returned=" << (env.saw_return ? "YES" : "NO") << "\n";
                    report << "    Shared-state Unity initJni GetJavaVM: "
                           << (env.jni_get_java_vm_seen ? "YES" : "NO") << "\n";
                    report << "    Shared-state Unity initJni GetEnv: "
                           << (env.jni_get_env_seen ? "YES" : "NO") << "\n";
                    report << "    Shared-state Unity initJni NewGlobalRef: "
                           << (env.jni_new_global_ref_seen ? "YES" : "NO") << "\n";
                    report << "    Shared-state Unity initJni GetObjectClass: "
                           << (env.jni_get_object_class_seen ? "YES" : "NO") << "\n";
                    if (!env.jni_static_field_name.empty()) {
                        report << "    Shared-state Unity initJni static field: "
                               << env.jni_static_field_name << " " << env.jni_static_field_signature << "\n";
                    }
                    report << "    Shared-state Unity initJni GetStaticObjectField: "
                           << (env.jni_get_static_object_field_seen ? "YES" : "NO") << "\n";
                    if (env.jni_get_static_int_field_seen) {
                        report << "    Shared-state Unity initJni GetStaticIntField: YES value=0x"
                               << std::hex << env.jni_static_int_field_value << std::dec << "\n";
                    }
                    if (!env.jni_new_string_utf.empty()) {
                        report << "    Shared-state Unity initJni NewStringUTF: " << env.jni_new_string_utf << "\n";
                    }
                    if (!env.jni_method_name.empty()) {
                        report << "    Shared-state Unity initJni GetMethodID: " << env.jni_method_name
                               << " " << env.jni_method_signature << "\n";
                    }
                    if (env.jni_call_object_method_count != 0) {
                        report << "    Shared-state Unity initJni CallObjectMethod count: "
                               << env.jni_call_object_method_count << "\n";
                    }
                    if (env.jni_call_int_method_count != 0) {
                        report << "    Shared-state Unity initJni CallIntMethod count: "
                               << env.jni_call_int_method_count << "\n";
                    }
                    if (env.jni_call_boolean_method_count != 0) {
                        report << "    Shared-state Unity initJni CallBooleanMethod count: "
                               << env.jni_call_boolean_method_count << "\n";
                    }
                    if (env.jni_unknown_slot != 0xffffffffu) {
                        report << "    Shared-state Unity initJni unknown JNI slot: index=" << env.jni_unknown_slot
                               << " offset=0x" << std::hex << (env.jni_unknown_slot * 4u)
                               << " lr=0x" << env.jni_unknown_lr
                               << " caller_lr=0x" << env.jni_unknown_caller_lr
                               << " args=[0x" << env.jni_unknown_r0 << ",0x" << env.jni_unknown_r1
                               << ",0x" << env.jni_unknown_r2 << ",0x" << env.jni_unknown_r3 << "]"
                               << std::dec << "\n";
                    }
                    if (env.jni_null_code_seen) {
                        report << "    Shared-state Unity initJni null-call trap: lr=0x"
                               << std::hex << env.jni_null_code_lr << std::dec << "\n";
                    }
                    if (!env.first_thunk.empty()) {
                        report << "    Shared-state Unity initJni blocked by thunk: " << env.first_thunk
                               << " (SVC " << env.first_thunk_id << ")\n";
                    }
                    if (env.premature_return) {
                        report << "    Shared-state Unity initJni premature return: sp=0x" << std::hex
                               << env.return_sp << " expected=0x" << kStackTop
                               << " r11=0x" << env.return_r11 << " r0=0x" << env.premature_r0
                               << std::dec << "\n";
                        if (env.premature_regs.size() >= 16) {
                            report << "    Shared-state Unity initJni registers:";
                            for (std::size_t i = 0; i < 16; ++i)
                                report << " r" << i << "=0x" << std::hex << env.premature_regs[i] << std::dec;
                            report << "\n";
                        }
                        if (!env.premature_frames.empty()) {
                            report << "    Shared-state Unity initJni frame unwind (fp -> saved lr):";
                            for (const auto& frame : env.premature_frames)
                                report << " 0x" << std::hex << frame.first << "->0x" << frame.second << std::dec;
                            report << "\n";
                        }
                    }
                    if (env.failed) {
                        report << "    Shared-state Unity initJni fault PC: 0x" << std::hex
                               << env.fault_pc << std::dec << "\n";
                    }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                    if (!env.call_trace.empty()) {
                        report << "    Shared-state Unity initJni thunk trace:";
                        for (const auto& name : env.call_trace) report << " " << name;
                        report << "\n";
                    }
#endif

                    const bool unity_init_jni_clean = env.saw_return && !env.failed &&
                                                      !env.premature_return && env.first_thunk.empty();
                    auto run_unity_registered_native = [&](const char* label,
                                                           std::uint32_t function,
                                                           std::uint32_t arg2,
                                                           std::uint32_t arg3,
                                                           std::uint32_t arg4,
                                                           std::uint32_t arg5,
                                                           const std::string* jni_string_arg = nullptr,
                                                           bool quiet = false) -> bool {
                        if (!quiet) report << "    Shared-state Unity " << label << " lookup: ";
                        if (function == 0) {
                            if (!quiet) report << "FAIL (registered native not found)\n";
                            return false;
                        }
                        if (!quiet) report << "PASS @ 0x" << std::hex << function << std::dec << "\n";

                        env.ResetPhaseDiagnostics();
                        env.ResetWorkerBoundaryDiagnostics();
                        const bool is_render_call = std::strncmp(label, "nativeRender", 12) == 0;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        if (is_render_call) env.ResetCacheflushRangeSummary();
#endif
                        const bool is_interactive_render = std::strcmp(label, "nativeRenderLoop") == 0;
                        const bool is_input_call = std::strcmp(label, "nativeInjectEvent") == 0;
                        const bool is_audio_call = std::strncmp(label, "fmod", 4) == 0;
                        const bool is_pause_call = std::strcmp(label, "nativePause") == 0;
                        const bool needs_worker_pump = is_render_call || is_input_call || is_audio_call || is_pause_call;
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        if (is_render_call) env.ResetGuestThreadPerf();
#endif
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
                        std::uint32_t pv7_phase_hash=2166136261u;
                        for(const char* c=label;*c;++c)
                            pv7_phase_hash=(pv7_phase_hash ^ static_cast<std::uint8_t>(*c))*16777619u;
                        env.TraceEvent(pv7::trace::Phase, function, 0u, pv7_phase_hash, 1u);
#endif
                        env.allow_main_thread_cooperative_yield = needs_worker_pump;
                        if (jni_string_arg) env.jni_new_string_utf = *jni_string_arg;
                        env.saw_return = false;
                        env.premature_return = false;
                        env.failed = false;
                        env.fault_pc = 0;
                        env.ticks_left = kTicksPerSlice;
                        jit.ClearHalt();
                        jit.Regs().fill(0);
                        jit.Regs()[0] = kJniEnvObject;
                        jit.Regs()[1] = kJniFakeObjectHandle;
                        jit.Regs()[2] = arg2;
                        jit.Regs()[3] = arg3;
                        Write32(memory, kStackTop, arg4);
                        Write32(memory, kStackTop + 4u, arg5);
                        jit.Regs()[13] = kStackTop;
                        jit.Regs()[14] = kReturnStub;
                        jit.Regs()[15] = function & ~1u;
                        jit.SetCpsr((function & 1u) ? 0x20u : 0u);
                        env.ClearMainTrylockWait();

                        // Render/input calls can legitimately need thousands of cooperative
                        // slices while Unity's preload and worker threads make progress. A
                        // lower release-only cap can make clean builds appear frozen during
                        // early Unity startup even when developer builds continue normally.
                        const int kUnityFollowupMaxSlices = is_interactive_render ? 120000 : (is_render_call ? 16000 : (needs_worker_pump ? 8000 : 1000));
                        constexpr int kUnityPeriodicWorkerPumpSlices = 16;
                        int followup_slices = 0;
                        std::size_t followup_cache_restarts = 0;
                        std::size_t followup_pending_cache_restarts = 0;
                        std::size_t followup_dynarmic_cache_restarts = 0;
                        std::size_t followup_worker_pumps = 0;
                        std::size_t followup_preload_wait_pumps = 0;
                        std::size_t followup_guest_callbacks = 0;
                        std::size_t followup_cooperative_yields = 0;
                        std::size_t followup_periodic_pumps = 0;
                        std::size_t followup_tick_exhaustions = 0;
#if defined(PROJECTV7_STARTUP_HOTPAGE_TRACE) && PROJECTV7_STARTUP_HOTPAGE_TRACE && defined(__ANDROID__)
                        std::unordered_map<std::uint32_t, std::size_t> exhausted_guest_pc_pages;
                        const auto clock_ids_at_start = env.guest_clock_id_counts;
                        const auto clock_getres_at_start = env.guest_clock_getres_calls;
                        const auto clock_unknown_at_start = env.guest_clock_unknown_id_calls;
#endif
                        const std::size_t total_cacheflush_at_start = env.total_arm_cacheflush_calls;
                        const std::uint64_t cache_invalidation_us_at_start = env.total_cache_invalidation_us;
                        const std::size_t cache_invalidation_calls_at_start = env.total_cache_invalidation_calls;
                        const std::size_t total_exec_mprotect_at_start = env.total_executable_mprotect_calls;
                        const std::size_t skipped_cacheflush_at_start = env.total_cacheflush_skipped_untranslated;
                        const std::size_t invalidated_cacheflush_at_start = env.total_cacheflush_invalidated_translated;
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        std::uint64_t followup_main_run_us = 0;
                        std::uint64_t followup_worker_pump_us = 0;
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        std::size_t queue_probe_empty = 0;
                        std::size_t queue_probe_nonempty = 0;
                        std::size_t queue_probe_state_changes = 0;
                        std::size_t queue_probe_invalid = 0;
                        std::uint32_t queue_probe_last_object = 0;
                        std::uint32_t queue_probe_last_head = 0;
                        std::uint32_t queue_probe_last_tail = 0;
                        bool queue_probe_had_state = false;
                        bool queue_probe_previous_nonempty = false;
                        std::size_t queue_probe_head_changes = 0;
                        std::size_t queue_probe_tail_changes = 0;
                        std::size_t queue_probe_empty_streak = 0;
                        std::size_t queue_probe_max_empty_streak = 0;
                        int queue_probe_last_transition_slice = -1;
                        const auto queue_probe_started = std::chrono::steady_clock::now();
#endif
                        std::array<std::uint32_t, 8> followup_previous_state{};
                        bool followup_have_previous_state = false;
                        int followup_same_state_slices = 0;
                        auto pump_render_workers = [&]() -> bool {
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                            const auto worker_pump_begin = std::chrono::steady_clock::now();
#endif
                            // Default 4 matches historical release scheduling exactly.
                            // Override only in isolated A/B builds; do not alter guest sync semantics.
                            followup_worker_pumps += env.PumpQueuedGuestThreads(32, PROJECTV7_WORKER_QUANTUM_SLICES);
                            followup_worker_pumps += env.PumpHostHttpCallbacks(2);
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                            followup_worker_pump_us += static_cast<std::uint64_t>(
                                std::chrono::duration_cast<std::chrono::microseconds>(
                                    std::chrono::steady_clock::now() - worker_pump_begin).count());
#endif
                            env.ResetPhaseDiagnostics();
                            env.saw_return = false;
                            env.premature_return = false;
                            env.failed = false;
                            env.fault_pc = 0;
                            env.allow_main_thread_cooperative_yield = needs_worker_pump;
                            jit.ClearHalt();
                            followup_have_previous_state = false;
                            followup_same_state_slices = 0;
                            return true;
                        };
                        for (; followup_slices < kUnityFollowupMaxSlices; ++followup_slices) {
                            if (needs_worker_pump && !is_interactive_render && !IsGuestGameLoopRunning()) break;
                            if (env.MainTrylockWaitPending()) {
                                const bool main_signal_ready = env.HasDeliverablePendingGuestSignal(1u) ||
                                                               env.pending_guest_callback;
                                if (!main_signal_ready && env.MainTrylockWaitOwnerActive() &&
                                    followup_preload_wait_pumps < 2048u) {
                                    env.PreferMainTrylockWaitOwner();
                                    ++followup_preload_wait_pumps;
                                    if (!pump_render_workers()) break;
                                    continue;
                                }
                                env.ClearMainTrylockWait();
                            }
#if defined(__ANDROID__)
                            if (is_render_call && followup_slices != 0 && (followup_slices % 256) == 0) {
                                const std::string startup_thunk = env.DebugThunkNameForPc(jit.Regs()[15]);
                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                    "PV7STARTUP %s slice=%d pc=0x%08x lr=0x%08x sp=0x%08x r0=0x%08x workers=%zu thunk=%s",
                                                    label, followup_slices, jit.Regs()[15], jit.Regs()[14],
                                                    jit.Regs()[13], jit.Regs()[0], followup_worker_pumps,
                                                    startup_thunk.empty() ? "-" : startup_thunk.c_str());
                                if (followup_slices == 256 && startup_thunk == "pthread_mutex_trylock") {
                                    for (const auto& thread_state : env.DebugGuestThreadStates())
                                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC", "PV7STARTUP thread %s", thread_state.c_str());
                                }
                            }
#endif
                            env.ticks_left = kTicksPerSlice;
                            // Worker quanta can queue POSIX signals for the Unity main thread.
                            // Deliver them before resuming main guest code, just as the host
                            // scheduler would on return to user mode. Without this, a main-thread
                            // signal handler can be starved indefinitely while the main guest spins
                            // in a trylock, which in turn can strand a worker waiting for that
                            // handler's semaphore acknowledgement.
                            env.DeliverPendingGuestSignal();
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                            const auto main_run_begin = std::chrono::steady_clock::now();
                            const std::uint32_t main_run_pc_before = jit.Regs()[15];
#endif
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
                            env.TraceEvent(pv7::trace::JitEnter, jit.Regs()[15], jit.Regs()[14],
                                           static_cast<std::uint32_t>(followup_slices), jit.Regs()[0], jit.Regs()[13]);
#endif
                            const auto halt_reason = jit.Run();
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
                            env.TraceEvent(pv7::trace::JitExit, jit.Regs()[15], jit.Regs()[14],
                                           static_cast<std::uint32_t>(followup_slices),
                                           static_cast<std::uint32_t>(halt_reason),
                                           static_cast<std::uint32_t>(env.ticks_left));
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_FAULT_CODE_SNAPSHOT) && PROJECTV7_FAULT_CODE_SNAPSHOT
                            // Passive snapshot after each JIT slice. Does not replace any
                            // fast-memory page or alter guest code, unlike a write trap.
                            constexpr std::uint32_t kFaultWordAddress = 0x06bc3140u;
                            std::uint32_t fault_word = 0u;
                            if (Read32(memory, kFaultWordAddress, fault_word)) {
                                if ((!env.fault_code_snapshot_initialized && fault_word != 0u) ||
                                    (env.fault_code_snapshot_initialized &&
                                     fault_word != env.fault_code_snapshot_last)) {
                                    if (env.fault_code_snapshot_changes < 64u) {
                                        __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                            "PV7CODESNAPSHOT change=%zu phase=%s slice=%d old=%08x new=%08x pc=%08x lr=%08x sp=%08x",
                                                            env.fault_code_snapshot_changes, label, followup_slices,
                                                            env.fault_code_snapshot_last, fault_word,
                                                            jit.Regs()[15], jit.Regs()[14], jit.Regs()[13]);
                                    }
                                    ++env.fault_code_snapshot_changes;
                                }
#if defined(PROJECTV7_FAULT_CODE_NOP_TEST) && PROJECTV7_FAULT_CODE_NOP_TEST
                                // One-off *diagnostic* bypass. The generated opcode is
                                // invalid on real ARMv7 as well. Only touch the precise
                                // observed code window; never enable in production.
                                if (fault_word == 0xf5a00000u &&
                                    std::strcmp(label, "nativeRenderLoop") == 0) {
                                    std::uint32_t before0 = 0u, before1 = 0u, before2 = 0u;
                                    std::uint32_t before3 = 0u, after0 = 0u;
                                    const bool matches =
                                        Read32(memory, kFaultWordAddress - 16u, before0) &&
                                        Read32(memory, kFaultWordAddress - 12u, before1) &&
                                        Read32(memory, kFaultWordAddress - 8u, before2) &&
                                        Read32(memory, kFaultWordAddress - 4u, before3) &&
                                        Read32(memory, kFaultWordAddress + 4u, after0) &&
                                        before0 == 0xe1a0c00du && before1 == 0xe92d5ff0u &&
                                        before2 == 0xe24dd070u && before3 == 0xe1a0b00du &&
                                        after0 == 0xebffe3b9u;
                                    if (matches && Write32(memory, kFaultWordAddress, 0xe1a00000u)) {
                                        jit.InvalidateCacheRange(kFaultWordAddress, 4u);
                                        fault_word = 0xe1a00000u;
                                        __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                            "PV7DIAGNOP applied addr=%08x phase=%s slice=%d",
                                                            kFaultWordAddress, label, followup_slices);
                                    } else {
                                        __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                            "PV7DIAGNOP skipped matches=%d before=%08x,%08x,%08x,%08x after=%08x",
                                                            matches ? 1 : 0, before0, before1, before2,
                                                            before3, after0);
                                    }
                                }
#endif
                                env.fault_code_snapshot_last = fault_word;
                                env.fault_code_snapshot_initialized = true;
                            }
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                            if (is_render_call && std::strcmp(label, "nativeRenderFrame2") == 0) {
                                // libunity.so at 0x002f34e8 obtains its queue object from
                                // the relocated global pointer slot at ELF VA 0x00b8c95c.
                                // Sample only after JIT slices. Do not intercept guest code.
                                constexpr std::uint32_t kQueuePointerSlot = kUnityBase + 0x00b8c95cu;
                                std::uint32_t object = 0, head = 0, tail = 0;
                                if (!Read32(memory, kQueuePointerSlot, object) || object == 0 ||
                                    !Read32(memory, object + 0xf4u, head) ||
                                    !Read32(memory, object + 0xf8u, tail)) {
                                    ++queue_probe_invalid;
                                } else {
                                    const bool nonempty = head != object + 0xf4u;
                                    if (nonempty) ++queue_probe_nonempty;
                                    else ++queue_probe_empty;
                                    if (queue_probe_had_state && object == queue_probe_last_object) {
                                        if (head != queue_probe_last_head) ++queue_probe_head_changes;
                                        if (tail != queue_probe_last_tail) ++queue_probe_tail_changes;
                                    }
                                    if (nonempty) {
                                        queue_probe_empty_streak = 0;
                                    } else {
                                        ++queue_probe_empty_streak;
                                        queue_probe_max_empty_streak = std::max(
                                            queue_probe_max_empty_streak, queue_probe_empty_streak);
                                    }
                                    if (!queue_probe_had_state ||
                                        nonempty != queue_probe_previous_nonempty) {
                                        const auto elapsed_ms = std::chrono::duration_cast<std::chrono::milliseconds>(
                                            std::chrono::steady_clock::now() - queue_probe_started).count();
                                        if (queue_probe_had_state) ++queue_probe_state_changes;
                                        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                            "PV7QUEUE transition slice=%d state=%s elapsed_ms=%lld gap_slices=%d pc=0x%08x lr=0x%08x head=0x%08x tail=0x%08x workers=%zu callbacks=%zu cache=%zu coop=%zu pending_callback=%d",
                                                            followup_slices + 1, nonempty ? "nonempty" : "empty",
                                                            static_cast<long long>(elapsed_ms),
                                                            queue_probe_last_transition_slice < 0 ? -1 :
                                                                followup_slices + 1 - queue_probe_last_transition_slice,
                                                            jit.Regs()[15], jit.Regs()[14], head, tail,
                                                            followup_worker_pumps, followup_guest_callbacks,
                                                            followup_cache_restarts, followup_cooperative_yields,
                                                            env.pending_guest_callback ? 1 : 0);
                                        queue_probe_last_transition_slice = followup_slices + 1;
                                    }
                                    queue_probe_had_state = true;
                                    queue_probe_previous_nonempty = nonempty;
                                    queue_probe_last_object = object;
                                    queue_probe_last_head = head;
                                    queue_probe_last_tail = tail;
                                }
                            }
#endif
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                            const std::uint64_t main_run_elapsed_us = static_cast<std::uint64_t>(
                                std::chrono::duration_cast<std::chrono::microseconds>(
                                    std::chrono::steady_clock::now() - main_run_begin).count());
                            followup_main_run_us += main_run_elapsed_us;
#if defined(__ANDROID__)
                            if (main_run_elapsed_us >= 100000u) {
                                const char* slow_last_call = env.call_trace.empty() ? "-" : env.call_trace.back().c_str();
                                // Confirm slow-path guest code bytes at runtime. Ghidra's
                                // ELF image base is 0x10000, so the correct Ghidra address
                                // for guest 0x013deb88 is 0x003eeb88, which does decode to
                                // the original word-scan LDR. Log the actual guest memory
                                // once to detect unexpected runtime modifications before
                                // optimizing the hot function or thread scheduling.
                                static bool logged_unity_slow_code_fingerprint = false;
                                if (!logged_unity_slow_code_fingerprint) {
                                    logged_unity_slow_code_fingerprint = true;
                                    std::uint32_t word_0dd0 = 0u;
                                    std::uint32_t word_eb58 = 0u;
                                    std::uint32_t word_eb88 = 0u;
                                    const bool code_ok =
                                        Read32(memory, kUnityBase + 0x003d0dd0u, word_0dd0) &&
                                        Read32(memory, kUnityBase + 0x003deb58u, word_eb58) &&
                                        Read32(memory, kUnityBase + 0x003deb88u, word_eb88);
                                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                        "PV7PERF unity-code-fingerprint ok=%u base=0x%08x "
                                                        "w_3d0dd0=0x%08x w_3deb58=0x%08x w_3deb88=0x%08x "
                                                        "pc=0x%08x lr=0x%08x",
                                                        code_ok ? 1u : 0u, kUnityBase,
                                                        word_0dd0, word_eb58, word_eb88,
                                                        jit.Regs()[15], jit.Regs()[14]);
                                }
                                std::uint32_t outer_caller_lr = 0u;
                                const std::uint32_t current_pc = jit.Regs()[15];
                                if (jit.Regs()[14] == 0x013d0dd4u &&
                                    current_pc >= 0x013deb88u && current_pc < 0x013dec08u &&
                                    jit.Regs()[13] <= memory.size() - 32u) {
                                    Read32(memory, jit.Regs()[13] + 28u, outer_caller_lr);
                                }
                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                    "PV7PERF slow-main slice=%d elapsed_us=%llu ticks_left=%llu pc_before=0x%08x pc_after=0x%08x lr_after=0x%08x outer_lr=0x%08x r1=0x%08x r2=0x%08x r3=0x%08x r6=0x%08x last=%s",
                                                    followup_slices + 1,
                                                    static_cast<unsigned long long>(main_run_elapsed_us),
                                                    static_cast<unsigned long long>(env.ticks_left),
                                                    main_run_pc_before, current_pc, jit.Regs()[14], outer_caller_lr,
                                                    jit.Regs()[1], jit.Regs()[2], jit.Regs()[3],
                                                    jit.Regs()[6], slow_last_call);
                            }
#endif
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                            if ((followup_slices & 63) == 63) {
                                const char* last_call = env.call_trace.empty() ? "-" : env.call_trace.back().c_str();
                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                    "Unity %s progress slice=%d pc=0x%08x lr=0x%08x sp=0x%08x last=%s main_us=%llu worker_us=%llu worker_pumps=%zu",
                                                    label, followup_slices + 1, jit.Regs()[15], jit.Regs()[14],
                                                    jit.Regs()[13], last_call,
                                                    static_cast<unsigned long long>(followup_main_run_us),
                                                    static_cast<unsigned long long>(followup_worker_pump_us),
                                                    followup_worker_pumps);
                            }
#endif
                            if (env.saw_return || env.premature_return || env.failed || !env.first_thunk.empty()) break;
                            if (env.pending_guest_callback) {
                                ++followup_guest_callbacks;
                                env.pending_guest_callback = false;
                                jit.ClearHalt();
                                continue;
                            }
                            if (env.pending_cacheflush) {
                                const std::uint32_t start = env.pending_cacheflush_start;
                                const std::uint32_t size = env.pending_cacheflush_size;
                                env.pending_cacheflush = false;
                                env.pending_cacheflush_start = 0;
                                env.pending_cacheflush_size = 0;
                                jit.ClearHalt();
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
                                env.TraceEvent(pv7::trace::CacheInvalidate,
                                               jit.Regs()[15],jit.Regs()[14],start,size);
#endif
                                if (size != 0) env.InvalidateTranslatedCodeRange(start, size);
                                ++followup_cache_restarts;
                                ++followup_pending_cache_restarts;
                                followup_have_previous_state = false;
                                followup_same_state_slices = 0;
                                continue;
                            }
                            if (Dynarmic::Has(halt_reason, Dynarmic::HaltReason::CacheInvalidation)) {
                                ++followup_cache_restarts;
                                ++followup_dynarmic_cache_restarts;
                                followup_have_previous_state = false;
                                followup_same_state_slices = 0;
                                continue;
                            }
                            if (env.ConsumeCooperativeYieldRequest()) {
                                ++followup_cooperative_yields;
                                if (!pump_render_workers()) break;
                                continue;
                            }
                            if (needs_worker_pump && ((followup_slices + 1) % kUnityPeriodicWorkerPumpSlices) == 0) {
                                ++followup_periodic_pumps;
                                if (!pump_render_workers()) break;
                                continue;
                            }
                            if (env.ticks_left == 0) {
                                ++followup_tick_exhaustions;
#if defined(PROJECTV7_STARTUP_HOTPAGE_TRACE) && PROJECTV7_STARTUP_HOTPAGE_TRACE && defined(__ANDROID__)
                                if (is_render_call && std::strcmp(label, "nativeRenderFrame2") == 0) {
                                    ++exhausted_guest_pc_pages[jit.Regs()[15] & ~0xfffu];
                                }
#endif
                            }
                            const std::array<std::uint32_t, 8> followup_state = {
                                jit.Regs()[15], jit.Regs()[0], jit.Regs()[1], jit.Regs()[2],
                                jit.Regs()[3], jit.Regs()[13], jit.Regs()[14], jit.Cpsr()
                            };
                            if (followup_have_previous_state && followup_state == followup_previous_state)
                                ++followup_same_state_slices;
                            else
                                followup_same_state_slices = 0;
                            followup_previous_state = followup_state;
                            followup_have_previous_state = true;
                            if (followup_same_state_slices >= 8) break;
                        }

#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        if (is_render_call && std::strcmp(label, "nativeRenderFrame2") == 0) {
                            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                "PV7QUEUE frame2 empty=%zu nonempty=%zu transitions=%zu invalid=%zu head_changes=%zu tail_changes=%zu max_empty_streak=%zu object=0x%08x head=0x%08x tail=0x%08x",
                                                queue_probe_empty, queue_probe_nonempty,
                                                queue_probe_state_changes, queue_probe_invalid,
                                                queue_probe_head_changes, queue_probe_tail_changes,
                                                queue_probe_max_empty_streak, queue_probe_last_object,
                                                queue_probe_last_head, queue_probe_last_tail);
                        }
#endif
                        const std::uint32_t followup_result = env.saw_return ? jit.Regs()[0] : 0u;
                        const bool followup_ok = env.saw_return && !env.failed && !env.premature_return &&
                                                 env.first_thunk.empty();
#if defined(__ANDROID__)
                        if (is_render_call) {
                            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                "PV7STARTUP %s end slices=%d returned=%d ok=%d failed=%d premature=%d pc=0x%08x lr=0x%08x workers=%zu owner_wait_pumps=%zu cache=%zu pending_cache=%zu dyn_cache=%zu callbacks=%zu coop=%zu periodic=%zu tick_exhaust=%zu guest_cacheflush=%zu skipped_untranslated=%zu invalidated_translated=%zu exec_mprotect=%zu invalidate_us=%llu invalidate_calls=%zu thunk=%s return_sp=0x%08x return_r11=0x%08x premature_r0=0x%08x",
                                                label, followup_slices, env.saw_return ? 1 : 0, followup_ok ? 1 : 0,
                                                env.failed ? 1 : 0, env.premature_return ? 1 : 0,
                                                jit.Regs()[15], jit.Regs()[14], followup_worker_pumps,
                                                followup_preload_wait_pumps,
                                                followup_cache_restarts, followup_pending_cache_restarts,
                                                followup_dynarmic_cache_restarts,
                                                followup_guest_callbacks, followup_cooperative_yields,
                                                followup_periodic_pumps, followup_tick_exhaustions,
                                                env.total_arm_cacheflush_calls - total_cacheflush_at_start,
                                                env.total_cacheflush_skipped_untranslated - skipped_cacheflush_at_start,
                                                env.total_cacheflush_invalidated_translated - invalidated_cacheflush_at_start,
                                                env.total_executable_mprotect_calls - total_exec_mprotect_at_start,
                                                static_cast<unsigned long long>(env.total_cache_invalidation_us - cache_invalidation_us_at_start),
                                                env.total_cache_invalidation_calls - cache_invalidation_calls_at_start,
                                                env.first_thunk.empty() ? "-" : env.first_thunk.c_str(),
                                                env.return_sp, env.return_r11, env.premature_r0);
#if defined(PROJECTV7_STARTUP_HOTPAGE_TRACE) && PROJECTV7_STARTUP_HOTPAGE_TRACE && defined(__ANDROID__)
                            if (std::strcmp(label, "nativeRenderFrame2") == 0) {
                                std::vector<std::pair<std::uint32_t, std::size_t>> hottest(
                                    exhausted_guest_pc_pages.begin(), exhausted_guest_pc_pages.end());
                                std::sort(hottest.begin(), hottest.end(),
                                          [](const auto& a, const auto& b) { return a.second > b.second; });
                                for (std::size_t i = 0; i < std::min<std::size_t>(hottest.size(), 18u); ++i) {
                                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                        "PV7HOTPAGE page=%08x tick_exhaust=%zu rank=%zu",
                                                        hottest[i].first, hottest[i].second, i + 1u);
                                }
                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                    "PV7CLOCK frame2 realtime=%llu monotonic=%llu process=%llu thread=%llu monotonic_raw=%llu realtime_coarse=%llu monotonic_coarse=%llu boottime=%llu getres=%llu unknown=%llu",
                                                    static_cast<unsigned long long>(env.guest_clock_id_counts[0] - clock_ids_at_start[0]),
                                                    static_cast<unsigned long long>(env.guest_clock_id_counts[1] - clock_ids_at_start[1]),
                                                    static_cast<unsigned long long>(env.guest_clock_id_counts[2] - clock_ids_at_start[2]),
                                                    static_cast<unsigned long long>(env.guest_clock_id_counts[3] - clock_ids_at_start[3]),
                                                    static_cast<unsigned long long>(env.guest_clock_id_counts[4] - clock_ids_at_start[4]),
                                                    static_cast<unsigned long long>(env.guest_clock_id_counts[5] - clock_ids_at_start[5]),
                                                    static_cast<unsigned long long>(env.guest_clock_id_counts[6] - clock_ids_at_start[6]),
                                                    static_cast<unsigned long long>(env.guest_clock_id_counts[7] - clock_ids_at_start[7]),
                                                    static_cast<unsigned long long>(env.guest_clock_getres_calls - clock_getres_at_start),
                                                    static_cast<unsigned long long>(env.guest_clock_unknown_id_calls - clock_unknown_at_start));
                            }
#endif
                            if (is_interactive_render && !followup_ok && env.premature_return) {
                                const std::size_t frame_count = std::min<std::size_t>(env.premature_frames.size(), 6u);
                                for (std::size_t i = 0; i < frame_count; ++i) {
                                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                        "PV7PREMATURE frame[%zu] fp=0x%08x lr=0x%08x",
                                                        i, env.premature_frames[i].first, env.premature_frames[i].second);
                                }
                            }
                            if (is_interactive_render && !followup_ok && env.failed) {
                                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                    "PV7FAULT fault_pc=0x%08x exception=%d invalid_code=0x%08x fallback=%zu bad_write=0x%08x bad_write_size=%zu bad_write_pc=0x%08x bad_write_lr=0x%08x jni_slot=%u jni_null=%d code_reads=%08x,%08x,%08x,%08x",
                                                    env.fault_pc, env.fault_exception, env.invalid_code_address,
                                                    env.fallback_instruction_count, env.bad_write_address,
                                                    env.bad_write_size, env.bad_write_pc, env.bad_write_lr,
                                                    env.jni_unknown_slot, env.jni_null_code_seen ? 1 : 0,
                                                    env.code_read_last, env.code_read_prev1,
                                                    env.code_read_prev2, env.code_read_prev3);
                                if (env.bad_write_address != 0u) {
                                    // Fault-only: preserve source registers for bogus guest
                                    // object pointers (e.g. Mono str [r3,#0x94]).
                                    __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                        "PV7BADWRITE context addr=%08x pc=%08x lr=%08x r0=%08x r1=%08x r2=%08x r3=%08x fp=%08x sp=%08x cfg=%08x cfg_field14=%08x field88=%08x local144=%08x local84c=%08x cfg_field00=%08x bad_next_from=%08x hops=%u prev_word0=%08x prev_word8=%08x end_node=%08x end_next=%08x stop_reason=%u",
                                                        env.bad_write_address, env.bad_write_pc,
                                                        env.bad_write_lr, env.bad_write_r0,
                                                        env.bad_write_r1, env.bad_write_r2,
                                                        env.bad_write_r3, env.bad_write_fp,
                                                        env.bad_write_sp, env.bad_write_mono_cfg,
                                                        env.bad_write_mono_cfg_field14,
                                                        env.bad_write_mono_field88,
                                                        env.bad_write_mono_local144,
                                                        env.bad_write_mono_local84c,
                                                        env.bad_write_mono_cfg_field00,
                                                        env.bad_write_list_bad_link_source,
                                                        env.bad_write_list_bad_link_hops,
                                                        env.bad_write_list_source_word0,
                                                        env.bad_write_list_source_word8,
                                                        env.bad_write_list_end_node,
                                                        env.bad_write_list_end_next,
                                                        env.bad_write_list_stop_reason);
                                }
                            }
                        }
#endif
#if defined(__ANDROID__) && defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        if (is_render_call) env.LogGuestThreadPerf(label);
#endif
#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
                        env.TraceEvent(pv7::trace::Phase,jit.Regs()[15],jit.Regs()[14],
                                       pv7_phase_hash, 0u,
                                       static_cast<std::uint32_t>(followup_slices),
                                       followup_ok ? 1u : 0u);
                        env.FlushTrace();
#endif
                        if (!followup_ok)
                            report << "    Native call failed label=" << label << " slices=" << followup_slices
                                   << " returned=" << env.saw_return << " failed=" << env.failed
                                   << " premature=" << env.premature_return
                                   << " thunk=" << env.first_thunk << " workers=" << followup_worker_pumps << "\n";
                        if (quiet) {
#if defined(__ANDROID__)
                            if (is_input_call) {
                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                    "PV7INPUT native returned=%d result=0x%08x failed=%d premature=%d thunk=%s slices=%d worker_pumps=%zu",
                                                    env.saw_return ? 1 : 0, followup_result,
                                                    env.failed ? 1 : 0, env.premature_return ? 1 : 0,
                                                    env.first_thunk.empty() ? "-" : env.first_thunk.c_str(),
                                                    followup_slices, followup_worker_pumps);
                            }
#endif
                            return followup_ok;
                        }
                        report << "    Shared-state Unity " << label << " execution: slices=" << followup_slices
                               << " cache_restarts=" << followup_cache_restarts
                               << " worker_pumps=" << followup_worker_pumps
                               << " returned=" << (env.saw_return ? "YES" : "NO")
                               << " result=0x" << std::hex << followup_result << std::dec << "\n";
                        if (!env.saw_return && !env.failed && !env.premature_return && env.first_thunk.empty()) {
                            report << "    Shared-state Unity " << label << " stopped without return: pc=0x"
                                   << std::hex << jit.Regs()[15] << " lr=0x" << jit.Regs()[14]
                                   << " sp=0x" << jit.Regs()[13] << " r0=0x" << jit.Regs()[0]
                                   << " r1=0x" << jit.Regs()[1] << " r2=0x" << jit.Regs()[2]
                                   << " r3=0x" << jit.Regs()[3] << std::dec
                                   << " same_state_slices=" << followup_same_state_slices;
                            const std::string stopped_thunk = env.DebugThunkNameForPc(jit.Regs()[15]);
                            if (!stopped_thunk.empty()) report << " thunk=" << stopped_thunk;
                            report << "\n";
                            const auto thread_states = env.DebugGuestThreadStates();
                            if (!thread_states.empty()) {
                                report << "    Shared-state Unity " << label << " complete guest thread table:\n";
                                for (const auto& thread_line : thread_states)
                                    report << "      " << thread_line << "\n";
                            }
                        }
                        if (env.pthread_once_callbacks != 0) {
                            report << "    Shared-state Unity " << label << " pthread_once initializers executed: "
                                   << env.pthread_once_callbacks << "\n";
                        }
                        if (env.jni_unknown_slot != 0xffffffffu) {
                            report << "    Shared-state Unity " << label << " unknown JNI slot: index="
                                   << env.jni_unknown_slot << " offset=0x" << std::hex
                                   << (env.jni_unknown_slot * 4u) << " lr=0x" << env.jni_unknown_lr
                                   << " caller_lr=0x" << env.jni_unknown_caller_lr << std::dec << "\n";
                        }
                        if (!env.first_thunk.empty()) {
                            report << "    Shared-state Unity " << label << " blocked by thunk: "
                                   << env.first_thunk << " (SVC " << env.first_thunk_id << ")\n";
                        }
                        if (env.premature_return) {
                            report << "    Shared-state Unity " << label << " premature return: sp=0x"
                                   << std::hex << env.return_sp << " r0=0x" << env.premature_r0
                                   << " r11=0x" << env.return_r11 << std::dec << "\n";
                        }
                        if (env.executable_mprotect_calls != 0) {
                            report << "    Shared-state Unity " << label << " executable mprotect calls: "
                                   << env.executable_mprotect_calls << " first=0x" << std::hex
                                   << env.first_exec_region << " size=0x" << env.first_exec_region_size
                                   << std::dec << "\n";
                        }
                        if (env.last_mprotect_length != 0) {
                            report << "    Shared-state Unity " << label << " last mprotect: addr=0x" << std::hex
                                   << env.last_mprotect_address << " len=0x" << env.last_mprotect_length
                                   << " prot=0x" << env.last_mprotect_prot << " caller_lr=0x"
                                   << env.last_mprotect_caller_lr << std::dec << "\n";
                        }
                        if (env.failed) {
                            report << "    Shared-state Unity " << label << " fault PC: 0x"
                                   << std::hex << env.fault_pc << std::dec
                                   << " exception=" << env.fault_exception << "\n";
                            if (env.fallback_instruction_count != 0) {
                                report << "    Shared-state Unity " << label << " interpreter fallback instructions: "
                                       << env.fallback_instruction_count << "\n";
                            }
                            if (env.invalid_code_address != 0) {
                                report << "    Shared-state Unity " << label << " invalid code fetch: 0x" << std::hex
                                       << env.invalid_code_address << " previous=[0x" << env.code_read_prev1
                                       << ",0x" << env.code_read_prev2 << ",0x" << env.code_read_prev3
                                       << "]" << std::dec << "\n";
                            }
                            if (env.bad_write_address != 0) {
                                report << "    Shared-state Unity " << label << " bad guest write: addr=0x" << std::hex
                                       << env.bad_write_address << " size=0x" << env.bad_write_size
                                       << " pc=0x" << env.bad_write_pc << " lr=0x" << env.bad_write_lr
                                       << " args=[0x" << env.bad_write_r0 << ",0x" << env.bad_write_r1
                                       << ",0x" << env.bad_write_r2 << ",0x" << env.bad_write_r3 << "]"
                                       << std::dec << "\n";
                            }
                            report << "    Shared-state Unity " << label << " fault registers:";
                            for (std::size_t i = 0; i < 16; ++i)
                                report << " r" << i << "=0x" << std::hex << jit.Regs()[i] << std::dec;
                            report << "\n";
                        }
                        if (!env.worker_boundaries.empty()) {
                            report << "    Shared-state Unity " << label << " worker compatibility boundaries: "
                                   << env.worker_boundaries.size() << "\n";
                            for (const auto& boundary : env.worker_boundaries) {
                                report << "      thread=" << boundary.thread_id
                                       << " start=0x" << std::hex << boundary.thread_start << std::dec;
                                if (!boundary.thunk.empty()) report << " thunk=" << boundary.thunk;
                                if (boundary.jni_slot != 0xffffffffu) report << " jni_slot=" << boundary.jni_slot;
                                if (boundary.fault_pc != 0)
                                    report << " fault_pc=0x" << std::hex << boundary.fault_pc << std::dec;
                                report << " pc=0x" << std::hex << boundary.regs[15]
                                       << " lr=0x" << boundary.regs[14]
                                       << " sp=0x" << boundary.regs[13] << std::dec << "\n";
                            }
                        }
#if defined(PROJECTV7_DEV_DIAGNOSTICS) && PROJECTV7_DEV_DIAGNOSTICS
                        if (!env.guest_thread_pump_trace.empty()) {
                            report << "    Shared-state Unity " << label << " final guest thread states:\n";
                            for (const auto& thread_line : env.guest_thread_pump_trace)
                                report << "      " << thread_line << "\n";
                        }
                        if (!env.file_trace.empty()) {
                            report << "    Shared-state Unity " << label << " file/memory trace:\n";
                            const std::size_t start = env.file_trace.size() > 16 ? env.file_trace.size() - 16 : 0;
                            for (std::size_t i = start; i < env.file_trace.size(); ++i)
                                report << "      " << env.file_trace[i] << "\n";
                        }
                        if (!env.call_trace.empty()) {
                            report << "    Shared-state Unity " << label << " thunk trace:";
                            for (const auto& name : env.call_trace) report << " " << name;
                            report << "\n";
                        }
#endif
                        return env.saw_return && !env.failed && !env.premature_return &&
                               env.first_thunk.empty();
                    };

                    if (unity_init_jni_clean) {
                        const bool native_file_ok = run_unity_registered_native(
                            "nativeFile", unity_native_file, kJniFakeStringHandle, 0, 0, 0,
                            &package_code_path);
                        if (native_file_ok) {
                            const std::uint32_t www_class_handle =
                                env.GetOrCreateJniClassHandle("com/unity3d/player/WWW");
                            const bool init_www_ok = run_unity_registered_native(
                                "nativeInitWWW", unity_native_init_www, www_class_handle, 0, 0, 0);
                            if (init_www_ok) {
                            const bool default_display_ok = run_unity_registered_native(
                                "nativeSetDefaultDisplay", unity_native_set_default_display, 0, 0, 0, 0);
                            if (default_display_ok) {
                                const bool recreate_gfx_ok = run_unity_registered_native(
                                    "nativeRecreateGfxState", unity_native_recreate_gfx_state,
                                    kJniFakeObjectHandle, 0, 0, 0);
                                if (recreate_gfx_ok) {
                                    std::uint32_t surface_width = 1;
                                    std::uint32_t surface_height = 1;
#if defined(__ANDROID__)
                                    if (host_native_window) {
                                        const int width = ANativeWindow_getWidth(static_cast<ANativeWindow*>(host_native_window));
                                        const int height = ANativeWindow_getHeight(static_cast<ANativeWindow*>(host_native_window));
                                        if (width > 0) surface_width = static_cast<std::uint32_t>(width);
                                        if (height > 0) surface_height = static_cast<std::uint32_t>(height);
                                    }
#endif
                                    env.guest_display_width = surface_width;
                                    env.guest_display_height = surface_height;
                                    report << "    Shared-state Unity host surface size: "
                                           << surface_width << "x" << surface_height << "\n";
                                    const bool resize_ok = run_unity_registered_native(
                                        "nativeResize", unity_native_resize,
                                        surface_width, surface_height, surface_width, surface_height);
                                    if (resize_ok) {
                                        const bool resume_ok = run_unity_registered_native(
                                            "nativeResume", unity_native_resume, 0, 0, 0, 0);
                                        if (resume_ok) {
                                            const bool focus_ok = run_unity_registered_native(
                                                "nativeFocusChanged", unity_native_focus_changed, 1u, 0, 0, 0);
                                            if (!focus_ok)
                                                report << "    Shared-state Unity focus acquisition failed\n";
                                            std::uint32_t unity_free_got_before_render = 0;
                                            Read32(memory, kUnityBase + 0x00a89000u, unity_free_got_before_render);
                                            report << "    Shared-state Unity free GOT before nativeRender: got=0x"
                                                   << std::hex << unity_free_got_before_render << std::dec << "\n";
#if defined(__ANDROID__)
                                            __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                                "Unity free GOT before nativeRender: got=0x%08x",
                                                                unity_free_got_before_render);
#endif
                                            const bool first_render_ok = run_unity_registered_native(
                                                "nativeRender", unity_native_render, 0, 0, 0, 0);
                                            if (first_render_ok) {
                                                // nativeResize ignores dimensions before Unity initializes its renderer.
                                                // Replay the resize after the first nativeRender has initialized it.
                                                const bool late_resize_ok = run_unity_registered_native(
                                                    "nativeResizeAfterInit", unity_native_resize,
                                                    surface_width, surface_height, surface_width, surface_height);
                                                report << "    Shared-state Unity post-init resize: "
                                                       << (late_resize_ok ? "PASS" : "FAIL") << "\n";
                                                const std::size_t pumped_threads =
                                                    env.PumpQueuedGuestThreads(32, 4);
                                                report << "    Shared-state Unity queued worker threads pumped: "
                                                       << pumped_threads << "\n";
                                                for (const auto& thread_line : env.guest_thread_pump_trace)
                                                    report << "      " << thread_line << "\n";
#if !defined(PROJECTV7_DEV_DIAGNOSTICS) || !PROJECTV7_DEV_DIAGNOSTICS
                                                // Frame-two cooperative pumps can execute thousands
                                                // of times; production does not consume their traces.
                                                env.record_pump_trace = false;
#endif
                                                const bool second_render_ok = run_unity_registered_native(
                                                    "nativeRenderFrame2", unity_native_render, 0, 0, 0, 0);
                                                if (second_render_ok) {
                                                    const bool third_render_ok = run_unity_registered_native(
                                                        "nativeRenderFrame3", unity_native_render, 0, 0, 0, 0);
                                                    if (third_render_ok) {
#if defined(__ANDROID__)
                                                        AndroidAudioOutput audio_output;
                                                        std::uint32_t fmod_audio_buffer = 0;
                                                        std::uint32_t fmod_audio_bytes = 0;
                                                        std::uint32_t fmod_sample_rate = 0;
                                                        std::uint32_t fmod_block_frames = 0;
                                                        bool fmod_audio_ready = false;
                                                        bool fmod_logged_pcm = false;

                                                        auto fmod_get_info = [&](std::uint32_t index,
                                                                                 std::uint32_t& value) -> bool {
                                                            if (unity_fmod_get_info == 0) return false;
                                                            if (!run_unity_registered_native(
                                                                    "fmodGetInfo", unity_fmod_get_info,
                                                                    index, 0, 0, 0, nullptr, true)) {
                                                                return false;
                                                            }
                                                            value = jit.Regs()[0];
                                                            return true;
                                                        };

                                                        __android_log_print(
                                                            ANDROID_LOG_INFO, "CNR64POC",
                                                            "PV7AUDIO natives init=0x%08x info=0x%08x process=0x%08x block=0x%08x unblock=0x%08x",
                                                            unity_fmod_init_jni, unity_fmod_get_info,
                                                            unity_fmod_process, unity_fmod_block_streaming,
                                                            unity_fmod_unblock_streaming);

                                                        if (unity_fmod_init_jni != 0 && unity_fmod_get_info != 0 &&
                                                            unity_fmod_process != 0) {
                                                            const bool fmod_init_ok = run_unity_registered_native(
                                                                "fmodInitJni", unity_fmod_init_jni,
                                                                0, 0, 0, 0, nullptr, true);
                                                            const bool fmod_unblock_ok =
                                                                unity_fmod_unblock_streaming == 0 ||
                                                                run_unity_registered_native(
                                                                    "fmodUnblockStreaming",
                                                                    unity_fmod_unblock_streaming,
                                                                    0, 0, 0, 0, nullptr, true);
                                                            std::uint32_t fmod_output_ready = 0;
                                                            const bool info_ok =
                                                                fmod_get_info(0, fmod_sample_rate) &&
                                                                fmod_get_info(1, fmod_block_frames) &&
                                                                fmod_get_info(3, fmod_output_ready);
                                                            const std::uint64_t bytes64 =
                                                                static_cast<std::uint64_t>(fmod_block_frames) * 4u;
                                                            if (fmod_init_ok && fmod_unblock_ok && info_ok &&
                                                                fmod_sample_rate >= 8000u &&
                                                                fmod_sample_rate <= 192000u &&
                                                                fmod_block_frames != 0u &&
                                                                bytes64 <= 1024u * 1024u) {
                                                                fmod_audio_bytes = static_cast<std::uint32_t>(bytes64);
                                                                fmod_audio_buffer = env.AllocateGuestBuffer(
                                                                    fmod_audio_bytes, 16u);
                                                                if (fmod_audio_buffer != 0u) {
                                                                    env.SetJniDirectBuffer(fmod_audio_buffer,
                                                                                           fmod_audio_bytes);
                                                                    fmod_audio_ready =
                                                                        audio_output.Configure(fmod_sample_rate);
                                                                }
                                                            }
                                                            __android_log_print(
                                                                fmod_audio_ready ? ANDROID_LOG_INFO : ANDROID_LOG_WARN,
                                                                "CNR64POC",
                                                                "PV7AUDIO configure init=%d unblock=%d info=%d rate=%u frames=%u bytes=%u outputReady=%u guest=0x%08x opensl=%d",
                                                                fmod_init_ok ? 1 : 0,
                                                                fmod_unblock_ok ? 1 : 0,
                                                                info_ok ? 1 : 0,
                                                                fmod_sample_rate, fmod_block_frames,
                                                                fmod_audio_bytes, fmod_output_ready,
                                                                fmod_audio_buffer,
                                                                fmod_audio_ready ? 1 : 0);
                                                        }

                                                        auto pump_fmod_audio = [&]() {
                                                            if (!fmod_audio_ready || fmod_audio_buffer == 0u ||
                                                                fmod_audio_bytes == 0u) return;
                                                            for (int block = 0;
                                                                 block < 4 && audio_output.CanAccept(); ++block) {
                                                                std::uint32_t output_ready = 0;
                                                                if (!fmod_get_info(3, output_ready) || output_ready != 1u)
                                                                    break;
                                                                if (!run_unity_registered_native(
                                                                        "fmodProcess", unity_fmod_process,
                                                                        kJniFakeDirectBufferHandle,
                                                                        0, 0, 0, nullptr, true)) {
                                                                    __android_log_print(
                                                                        ANDROID_LOG_WARN, "CNR64POC",
                                                                        "PV7AUDIO fmodProcess failed");
                                                                    fmod_audio_ready = false;
                                                                    break;
                                                                }
                                                                const std::uint8_t* pcm = env.GuestBytes(
                                                                    fmod_audio_buffer, fmod_audio_bytes);
                                                                if (!pcm) {
                                                                    fmod_audio_ready = false;
                                                                    break;
                                                                }
                                                                if (!fmod_logged_pcm) {
                                                                    std::uint32_t peak = 0;
                                                                    for (std::uint32_t offset = 0;
                                                                         offset + 1u < fmod_audio_bytes;
                                                                         offset += 2u) {
                                                                        std::int16_t sample = 0;
                                                                        std::memcpy(&sample, pcm + offset,
                                                                                    sizeof(sample));
                                                                        const std::uint32_t magnitude =
                                                                            sample == INT16_MIN
                                                                                ? 32768u
                                                                                : static_cast<std::uint32_t>(
                                                                                      std::abs(static_cast<int>(sample)));
                                                                        peak = std::max(peak, magnitude);
                                                                    }
                                                                    __android_log_print(
                                                                        ANDROID_LOG_INFO, "CNR64POC",
                                                                        "PV7AUDIO first PCM block peak=%u bytes=%u",
                                                                        peak, fmod_audio_bytes);
                                                                    fmod_logged_pcm = true;
                                                                }
                                                                if (!audio_output.Enqueue(pcm, fmod_audio_bytes))
                                                                    break;
                                                            }
                                                        };
                                                        pump_fmod_audio();
#endif
                                                        report << "    Shared-state Unity interactive game loop: STARTED"
                                                               << " nativeInjectEvent=0x" << std::hex
                                                               << unity_native_inject_event << std::dec << "\n";
                                                        std::uint32_t unity_input_get_touch_count = 0u;
                                                        const bool have_touch_count_address =
                                                            pv7::compat::LookupGuestAddress(
                                                                "UnityEngine.Input::get_touchCount",
                                                                unity_input_get_touch_count);
                                                        std::uint64_t frame_index = 3;
                                                        unsigned slow_frame_reports = 0;
                                                        while (IsGuestGameLoopRunning()) {
                                                            bool had_touch_this_frame = false;
                                                            int touch_action_this_frame = -1;
                                                            HostTouchEvent touch;
                                                            if (PopHostTouchEvent(touch)) {
                                                                had_touch_this_frame = true;
                                                                touch_action_this_frame = touch.action;
                                                                env.MapNormalizedHostTouch(touch);
                                                                env.active_touch_event = std::move(touch);
                                                                env.active_touch_identity = env.next_touch_identity++;
                                                                bool input_ok = unity_native_inject_event != 0;
                                                                if (input_ok) {
                                                                    input_ok = run_unity_registered_native(
                                                                        "nativeInjectEvent", unity_native_inject_event,
                                                                        kJniFakeMotionEventHandle, 0, 0, 0,
                                                                        nullptr, true);
                                                                }
#if defined(__ANDROID__)
                                                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                                                    "PV7INPUT inject action=%d pointers=%zu x=%.1f y=%.1f ok=%d",
                                                                                    env.active_touch_event->action,
                                                                                    env.active_touch_event->points.size(),
                                                                                    env.active_touch_event->points.empty() ? 0.0f : env.active_touch_event->points[0].x,
                                                                                    env.active_touch_event->points.empty() ? 0.0f : env.active_touch_event->points[0].y,
                                                                                    input_ok ? 1 : 0);
#endif
                                                                std::uint32_t touch_count_after_inject = 0u;
                                                                if (input_ok && have_touch_count_address &&
                                                                    run_unity_registered_native(
                                                                        "debugTouchCountAfterInject", unity_input_get_touch_count,
                                                                        0, 0, 0, 0, nullptr, true)) {
                                                                    touch_count_after_inject = jit.Regs()[0];
#if defined(__ANDROID__)
                                                                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                                                        "PV7INPUT touchCount after inject action=%d count=%u",
                                                                                        touch_action_this_frame, touch_count_after_inject);
#endif
                                                                }

                                                                env.active_touch_event.reset();
                                                                env.active_touch_identity = 0;
                                                            }
                                                            if(auto text=PopHostTextEvent()) {
                                                                if(unity_input_string)run_unity_registered_native("nativeSetInputString",unity_input_string,env.NewHostInputString(text->text),0,0,0,nullptr,true);
                                                                if(text->closed) {
                                                                    if(unity_input_canceled)run_unity_registered_native("nativeSetInputCanceled",unity_input_canceled,text->canceled?1:0,0,0,0,nullptr,true);
                                                                    if(unity_input_closed)run_unity_registered_native("nativeSoftInputClosed",unity_input_closed,0,0,0,0,nullptr,true);
                                                                }
                                                            }
                                                            const auto render_begin=std::chrono::steady_clock::now();
                                                            const bool render_ok=run_unity_registered_native(
                                                                    "nativeRenderLoop", unity_native_render,
                                                                    0, 0, 0, 0, nullptr, true);
                                                            const auto render_ms=std::chrono::duration_cast<std::chrono::milliseconds>(
                                                                std::chrono::steady_clock::now()-render_begin).count();
                                                            if (render_ms>250 && slow_frame_reports++<32)
                                                                report << "    Slow frame=" << frame_index << " duration_ms=" << render_ms << "\n";
                                                            if (!render_ok) {
                                                                report << "    " << env.DescribeStoppedCall() << "\n";
#if defined(__ANDROID__)
                                                                __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                                                                                    "PV7GAME render loop stopped at frame=%llu",
                                                                                    static_cast<unsigned long long>(frame_index));
#endif
                                                                break;
                                                            }
#if defined(__ANDROID__)
                                                            // Keep translated background workers alive even when nativeRender
                                                            // returns in its first slice. Without this per-frame quantum,
                                                            // Unity 4.6's WWW worker can remain permanently in New state.
                                                            env.PumpFrameBackgroundWork();
                                                            pump_fmod_audio();
#endif
                                                            if (had_touch_this_frame && have_touch_count_address &&
                                                                run_unity_registered_native(
                                                                    "debugTouchCountAfterRender", unity_input_get_touch_count,
                                                                    0, 0, 0, 0, nullptr, true)) {
#if defined(__ANDROID__)
                                                                __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                                                    "PV7INPUT touchCount after render action=%d count=%u frame=%llu",
                                                                                    touch_action_this_frame, jit.Regs()[0],
                                                                                    static_cast<unsigned long long>(frame_index));
#endif
                                                            }
#if defined(__ANDROID__)
                                                            if ((frame_index % 60u) == 0u &&
                                                                g_thread_snapshot_count.fetch_add(1) < 8u) {
                                                                for (const auto& state : env.DebugGuestThreadStates()) {
                                                                    __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
                                                                                        "PV7THREAD state frame=%llu %s",
                                                                                        static_cast<unsigned long long>(frame_index),
                                                                                        state.c_str());
                                                                }
                                                            }
#endif
                                                            ++frame_index;
                                                        }
#if defined(__ANDROID__)
                                                        if (unity_fmod_block_streaming != 0) {
                                                            run_unity_registered_native(
                                                                "audioBlockStreaming",
                                                                unity_fmod_block_streaming,
                                                                0, 0, 0, 0, nullptr, true);
                                                        }
                                                        audio_output.Shutdown();
#endif
                                                        env.active_touch_event.reset();
                                                        if (unity_native_focus_changed != 0) {
                                                            run_unity_registered_native(
                                                                "nativeFocusChanged(false)", unity_native_focus_changed,
                                                                0u, 0, 0, 0, nullptr, true);
                                                        }
                                                        if (unity_native_pause != 0) {
                                                            run_unity_registered_native(
                                                                "nativePause", unity_native_pause,
                                                                0, 0, 0, 0, nullptr, true);
                                                        }
                                                        env.ShutdownHostGraphics();
                                                        report << "    Shared-state Unity interactive game loop: STOPPED after frame="
                                                               << frame_index << "\n";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        }
    } else {
        report << "    Shared-state Unity JNI: SKIPPED because Mono root-domain validation did not pass\n";
    }

    return report.str();
}
} // namespace

void QueueHostTextEvent(std::string text,bool closed,bool canceled) {
    std::lock_guard<std::mutex> lock(g_host_text_mutex);
    g_host_text_event=HostTextEvent{std::move(text),closed,canceled};
}

bool ConsumeGuestRestartRequest() { return g_guest_restart_requested.exchange(false); }

void QueueHostTouchEvent(HostTouchEvent event) {
    std::lock_guard<std::mutex> lock(g_host_touch_mutex);
    const std::int32_t masked_action = event.action & 0xff;
    if (masked_action == 2 && !g_host_touch_queue.empty() &&
        ((g_host_touch_queue.back().action & 0xff) == 2)) {
        g_host_touch_queue.back() = std::move(event);
    } else {
        if (g_host_touch_queue.size() >= 128) g_host_touch_queue.pop_front();
        g_host_touch_queue.push_back(std::move(event));
    }
}

void ClearHostTouchEvents() {
    std::lock_guard<std::mutex> lock(g_host_touch_mutex);
    g_host_touch_queue.clear();
}

void SetGuestGameLoopRunning(bool running) {
    if (running) ClearHostTouchEvents();
    g_guest_game_loop_running.store(running, std::memory_order_release);
    if (!running) ClearHostTouchEvents();
}

bool IsGuestGameLoopRunning() {
    return g_guest_game_loop_running.load(std::memory_order_acquire);
}

SharedGuestLinkResult RunSharedGuestLinkProbe(const std::string& libMainPath,
                                              const std::string& libUnityPath,
                                              const std::string& libMonoPath,
                                              const std::string& managedDirPath,
                                              const std::string& packageCodePath,
                                              void* hostNativeWindow,
                                              void* hostJniEnv) {
    std::ostringstream report;
    report << "\nShared ARM32 guest linker probe:\n";
    GuestMemory memory(kGuestMemorySize, 0);
    DiscardGuestPages(memory, 0, memory.size());

    std::vector<GuestImage> images = {
        {"libmain.so", libMainPath, kMainBase},
        {"libunity.so", libUnityPath, kUnityBase},
        {"libmono.so", libMonoPath, kMonoBase},
    };

    std::string error;
    for (auto& image : images) {
        if (!LoadAndMapImage(image, memory, error)) {
            report << "  Map " << image.name << ": FAIL (" << error << ")";
            return {false, report.str()};
        }
        report << "  Map " << image.name << ": PASS base=0x" << std::hex << image.base
               << " end=0x" << image.end << std::dec << " (" << image.load_segments << " PT_LOAD)\n";
    }

    std::unordered_map<std::string, ExportSymbol> exports;
    for (const auto& image : images) CollectExports(image, exports);
    report << "  Global guest exports indexed: " << exports.size() << "\n";

    LinkStats total{};
    std::vector<UnresolvedRelocation> unresolved;
    for (const auto& image : images) {
        LinkStats local{};
        if (!ApplyRelocations(image, memory, exports, local, unresolved, error)) {
            report << "  Link " << image.name << ": FAIL (" << error << ")";
            return {false, report.str()};
        }
        total.relative += local.relative;
        total.internal += local.internal;
        total.cross += local.cross;
        total.unresolved += local.unresolved;
        total.unsupported += local.unsupported;
        total.unity_to_mono += local.unity_to_mono;
        for (const auto& s : local.cross_examples) AddExample(total.cross_examples, s);
        for (const auto& s : local.unresolved_examples) AddExample(total.unresolved_examples, s);
        report << "  Link " << image.name << ": PASS"
               << " relative=" << local.relative
               << " internal=" << local.internal
               << " cross=" << local.cross
               << " unresolved-system=" << local.unresolved
               << " unsupported=" << local.unsupported << "\n";
    }

    report << "  Cross-library relocations resolved: " << total.cross << "\n";
    report << "  Unity -> Mono relocations resolved: " << total.unity_to_mono << "\n";
    if (!total.cross_examples.empty()) {
        report << "  Cross-link examples:\n";
        for (const auto& item : total.cross_examples) report << "    " << item << "\n";
    }
    report << "  Remaining Android/system relocations: " << total.unresolved << "\n";
    if (!total.unresolved_examples.empty()) {
        report << "  System-thunk examples:\n";
        for (const auto& item : total.unresolved_examples) report << "    " << item << "\n";
    }

    std::uint32_t root_domain = 0xffffffffu;
    std::uint32_t fault_pc = 0;
    const bool shared_exec_ok = ExecuteSharedMonoProbe(memory, exports, root_domain, fault_pc);
    report << "  Execute Mono from shared image: " << (shared_exec_ok ? "PASS" : "FAIL")
           << " (root domain=0x" << std::hex << root_domain << std::dec << ")";
    if (!shared_exec_ok && fault_pc) report << " fault_pc=0x" << std::hex << fault_pc << std::dec;
    report << "\n";

    report << RunMonoBootstrapTrapProbe(memory, exports, unresolved, managedDirPath, packageCodePath,
                                         hostNativeWindow, hostJniEnv) << "\n";

    const bool cross_ok = total.cross > 0 && total.unity_to_mono > 0;
    const bool ok = cross_ok && shared_exec_ok;
    report << "  Shared three-library address space: " << (ok ? "PASS" : "FAIL") << "\n";
    if (ok) {
        report << "  Result: libmain + libunity + libmono are mapped together and real guest imports resolve across libraries.";
    }
    return {ok, report.str()};
}
