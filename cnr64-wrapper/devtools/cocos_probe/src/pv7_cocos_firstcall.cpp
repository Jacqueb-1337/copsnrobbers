#include "pv7_arm32_format.h"
#include "../../../compat_modules/cocos2dx/cocos2dx_guest_jni_arrays.h"
#include "../../../compat_modules/cocos2dx/cocos2dx_jni_refs.h"
#include "../../../compat_modules/cocos2dx/cocos2dx_guest_math.h"
#include "../../../compat_modules/cocos2dx/cocos2dx_jni_marshaller.h"
#if defined(_WIN32)
#include "pv7_cocos_probe_bitmap_win32.h"
#endif
#include "arm32_guest_image.h"
#include "dynarmic/interface/A32/a32.h"

#include "pv7_gles_probe.h"
#include <cstdlib>
#include <cstdio>
#include <algorithm>
#include <array>
#include <chrono>
#include <cmath>
#include <zlib.h>
#include <cstdint>
#include <cstring>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <iterator>
#include <map>
#include <optional>
#include <string>
#include <string_view>
#include <vector>
#include <thread>

// The probe uses one EGL context throughout the original guest's lifecycle.
static Pv7ProbeGles2& DiagnosticGles() {
    static Pv7ProbeGles2 gles;
    return gles;
}
#if defined(PV7_COCOS_JNI_RUNNER)
#include <jni.h>
#include <unistd.h>
#include <cstdio>

static thread_local JNIEnv* g_pv7_cocos_env = nullptr;
static thread_local std::string g_pv7_cocos_capture_path;

static bool RenderAndroidBitmap(
        const pv7::compat::cocos2dx::BitmapTextRequest& request,
        pv7::compat::cocos2dx::BitmapPixels& pixels, std::string& error) {
    JNIEnv* env = g_pv7_cocos_env;
    if (!env) { error = "Android Java JNI context unavailable"; return false; }
    jclass cls = env->FindClass("me/jacqueb/pv7/cocos/Pv7CocosBitmapRenderer");
    if (env->ExceptionCheck() || !cls) {
        env->ExceptionDescribe(); env->ExceptionClear();
        error = "Android bitmap renderer Java class missing"; return false;
    }
    const auto cleanup = [&]() { env->DeleteLocalRef(cls); };
    jmethodID method = env->GetStaticMethodID(cls, "renderForGuest",
        "(Ljava/lang/String;Ljava/lang/String;IIII)[Ljava/lang/Object;");
    if (env->ExceptionCheck() || !method) {
        env->ExceptionDescribe(); env->ExceptionClear(); cleanup();
        error = "Android bitmap renderer JNI signature missing"; return false;
    }
    jstring text = env->NewStringUTF(request.text.c_str());
    jstring font = env->NewStringUTF(request.font.c_str());
    if (env->ExceptionCheck() || !text || !font) {
        env->ExceptionDescribe(); env->ExceptionClear();
        if (text) env->DeleteLocalRef(text);
        if (font) env->DeleteLocalRef(font);
        cleanup();
        error = "Cannot allocate Java bitmap strings"; return false;
    }
    auto result = static_cast<jobjectArray>(
        env->CallStaticObjectMethod(cls, method, text, font,
            static_cast<jint>(request.font_size),
            static_cast<jint>(request.alignment),
            static_cast<jint>(request.width),
            static_cast<jint>(request.height)));
    env->DeleteLocalRef(text);
    env->DeleteLocalRef(font);
    if (env->ExceptionCheck() || !result) {
        env->ExceptionDescribe(); env->ExceptionClear();
        if (result) env->DeleteLocalRef(result);
        cleanup();
        error = "Android Paint/Canvas renderer failed"; return false;
    }
    if (env->GetArrayLength(result) != 2) {
        env->DeleteLocalRef(result); cleanup();
        error = "Unexpected bitmap result packet"; return false;
    }
    auto dimensions = static_cast<jintArray>(env->GetObjectArrayElement(result, 0));
    auto bytes = static_cast<jbyteArray>(env->GetObjectArrayElement(result, 1));
    if (env->ExceptionCheck() || !dimensions || !bytes ||
        env->GetArrayLength(dimensions) != 2) {
        env->ExceptionDescribe(); env->ExceptionClear();
        if (dimensions) env->DeleteLocalRef(dimensions);
        if (bytes) env->DeleteLocalRef(bytes);
        env->DeleteLocalRef(result); cleanup();
        error = "Invalid Java bitmap dimension packet"; return false;
    }
    jint size[2]{};
    env->GetIntArrayRegion(dimensions, 0, 2, size);
    jsize length = env->GetArrayLength(bytes);
    if (env->ExceptionCheck() || length <= 0 || length > 64 * 1024 * 1024) {
        env->ExceptionDescribe(); env->ExceptionClear();
        env->DeleteLocalRef(bytes); env->DeleteLocalRef(dimensions);
        env->DeleteLocalRef(result); cleanup();
        error = "Invalid bitmap pixel buffer length"; return false;
    }
    std::vector<std::uint8_t> payload(static_cast<std::size_t>(length));
    env->GetByteArrayRegion(bytes, 0, length,
                            reinterpret_cast<jbyte*>(payload.data()));
    bool ok = !env->ExceptionCheck();
    if (!ok) {
        env->ExceptionDescribe(); env->ExceptionClear();
        error = "Could not read Java bitmap pixels";
    } else {
        ok = pv7::compat::cocos2dx::ReadBitmapPixels(
            size[0], size[1], std::span<const std::uint8_t>(payload),
            pixels, error);
    }
    env->DeleteLocalRef(bytes);
    env->DeleteLocalRef(dimensions);
    env->DeleteLocalRef(result);
    cleanup();
    return ok;
}
#endif


#if defined(PV7_COCOS_JNI_RUNNER)
static bool ResolveAndroidStaticString(const char* methodName, std::string& path, std::string& error) {
    JNIEnv* env = g_pv7_cocos_env;
    if (!env) { error = "missing Android JNI VM"; return false; }
    jclass cls = env->FindClass("me/jacqueb/pv7/cocos/CocosProbeActivity");
    if (env->ExceptionCheck() || !cls) {
        env->ExceptionDescribe(); env->ExceptionClear();
        error = "Cocos probe Java activity class missing"; return false;
    }
    jmethodID method = env->GetStaticMethodID(
        cls, methodName, "()Ljava/lang/String;");
    if (env->ExceptionCheck() || !method) {
        env->ExceptionDescribe(); env->ExceptionClear();
        env->DeleteLocalRef(cls);
        error = "Android OBB resolver method unavailable"; return false;
    }
    jstring value = static_cast<jstring>(env->CallStaticObjectMethod(cls, method));
    if (env->ExceptionCheck() || !value) {
        env->ExceptionDescribe(); env->ExceptionClear();
        if (value) env->DeleteLocalRef(value);
        env->DeleteLocalRef(cls);
        error = "Android OBB resolver threw"; return false;
    }
    const char* utf = env->GetStringUTFChars(value, nullptr);
    if (env->ExceptionCheck() || !utf) {
        env->ExceptionDescribe(); env->ExceptionClear();
        env->DeleteLocalRef(value); env->DeleteLocalRef(cls);
        error = "Android OBB resolver invalid string"; return false;
    }
    path.assign(utf);
    env->ReleaseStringUTFChars(value, utf);
    env->DeleteLocalRef(value);
    env->DeleteLocalRef(cls);
    return true;
}
static bool ResolveAndroidExpansionPath(std::string& path, std::string& error) {
    return ResolveAndroidStaticString("resolveExpansionPath",path,error);
}
#endif
#if defined(PV7_COCOS_JNI_RUNNER)
static bool ResolveAndroidObbZipEntry(
        const std::string& virtual_path,std::string& path,std::string& error) {
    JNIEnv* env=g_pv7_cocos_env;
    if(!env || virtual_path.size()>512) {
        error="invalid Android guest resource request";return false;
    }
    jclass cls=env->FindClass("me/jacqueb/pv7/cocos/CocosProbeActivity");
    jmethodID method=cls ? env->GetStaticMethodID(cls,
        "resolveGuestObbFile","(Ljava/lang/String;)Ljava/lang/String;") : nullptr;
    if(env->ExceptionCheck()||!method) {
        env->ExceptionDescribe();env->ExceptionClear();
        if(cls)env->DeleteLocalRef(cls);
        error="Android resource resolver missing";return false;
    }
    jstring key=env->NewStringUTF(virtual_path.c_str());
    jstring result=static_cast<jstring>(env->CallStaticObjectMethod(cls,method,key));
    if(key)env->DeleteLocalRef(key);
    env->DeleteLocalRef(cls);
    if(env->ExceptionCheck()) {
        env->ExceptionDescribe();env->ExceptionClear();
        if(result)env->DeleteLocalRef(result);
        error="Android resource resolver threw";return false;
    }
    if(!result) {path.clear();return true;}
    const char* chars=env->GetStringUTFChars(result,nullptr);
    if(env->ExceptionCheck()||!chars) {
        env->ExceptionDescribe();env->ExceptionClear();
        env->DeleteLocalRef(result);
        error="Android resource resolver returned invalid string";return false;
    }
    path.assign(chars);
    env->ReleaseStringUTFChars(result,chars);
    env->DeleteLocalRef(result);
    return true;
}
#endif
// PV7 development-only execution probe: the first actual ARM instructions
// of the owner's original Cocos ELF run under Dynarmic. All unknown imports
// halt at a named SVC, not at fabricated successful return values.
// This does NOT implement Android, rendering, or a complete JNI environment.
namespace {
constexpr std::uint32_t kImageBase = 0x01000000;
constexpr std::uint32_t kImportsBase = 0x01800000;
constexpr std::uint32_t kObjectsBase = 0x01900000;
constexpr std::uint32_t kPthreadOnce = 0x01a00000;
constexpr std::uint32_t kReturnTrap = 0x00008000;
constexpr std::uint32_t kGetEnvStub = 0x00008100;
constexpr std::uint32_t kAttachStub = 0x00008110;
constexpr std::uint32_t kFindClassStub = 0x00008120;
constexpr std::uint32_t kRegisterStub = 0x00008130;
constexpr std::uint32_t kGetVersionStub = 0x00008140;
constexpr std::uint32_t kVM = 0x00009000;
constexpr std::uint32_t kVMTable = 0x00009100;
constexpr std::uint32_t kEnv = 0x00009200;
constexpr std::uint32_t kEnvTable = 0x00009300;
constexpr std::uint32_t kStack = 0x07f00000;
constexpr std::uint32_t kHeapStart = 0x02000000;
constexpr std::uint32_t kHeapEnd = 0x07d00000;
constexpr std::size_t kMemorySize = 128 * 1024 * 1024;
constexpr std::uint32_t kImportSvcBase = 0x00001000;
constexpr std::uint32_t kJniGetEnv = 20;
constexpr std::uint32_t kJniAttach = 21;
constexpr std::uint32_t kJniFindClass = 22;
constexpr std::uint32_t kJniRegister = 23;
constexpr std::uint32_t kJniGetVersion = 24;
constexpr std::uint32_t kKuserCmpxchg = 30;
constexpr std::uint32_t kKuserMemoryBarrier = 31;
constexpr std::uint32_t kKuserGetTls = 32;
constexpr std::uint32_t kGuestTls = 0x0000b000;
constexpr std::uint32_t kMissingJniBase = 0x00010000;
constexpr std::uint32_t kMissingJniSvc = 0x00020000;

bool Fits(std::size_t address, std::size_t size, std::size_t capacity) {
    return address <= capacity && size <= capacity - address;
}
bool Put32(std::vector<std::uint8_t>& mem, std::uint32_t address, std::uint32_t val) {
    if (!Fits(address, sizeof(val), mem.size())) return false;
    std::memcpy(mem.data() + address, &val, sizeof(val));
    return true;
}
bool Trap(std::vector<std::uint8_t>& mem, std::uint32_t address, std::uint32_t id) {
    return Put32(mem, address, 0xef000000u | id) &&
           Put32(mem, address + 4, 0xe12fff1eu); // BX LR
}
std::string CString(const std::vector<std::uint8_t>& mem, std::uint32_t addr) {
    std::string out;
    if (addr >= mem.size()) return out;
    for (std::size_t i = 0; i < 128 && addr + i < mem.size(); ++i) {
        if (!mem[addr + i]) break;
        out.push_back(static_cast<char>(mem[addr + i]));
    }
    return out;
}
struct Import {
    std::string name;
    std::uint32_t address;
    bool data = false;
};
class Bridge final : public Dynarmic::A32::UserCallbacks {
public:
    explicit Bridge(std::vector<std::uint8_t>& memory,
                    const std::vector<Import>& imports)
        : mem_(memory), imports_(imports) {}
    Dynarmic::A32::Jit* jit = nullptr;
    bool returned = false;
    bool faulted = false;
    std::uint32_t fault_address = 0;
    std::string reason;
    std::uint64_t ticks_remaining = 1000000;
    std::uint64_t next_parser_sample_tick_ = 0;
    std::uint64_t draw_calls_ = 0;
    std::uint32_t last_tinyxml_input_ = 0;
    unsigned same_tinyxml_input_count_ = 0;
    std::optional<pv7::compat::cocos2dx::BitmapTextRequest> pending_bitmap;
#if defined(PV7_COCOS_JNI_RUNNER)
    ~Bridge() override { for (auto& entry:open_files_) std::fclose(entry.second); for (auto& entry:z_streams_) ::inflateEnd(&entry.second); }
#endif
    bool StageGuestString(const std::string& value,std::uint32_t& handle) {
        if (value.size()>=4096u || next_jstring_>0xfffcu) return false;
        handle=next_jstring_;next_jstring_+=4;
        guest_strings_.emplace(handle,value);
        if (!refs_.RegisterLocal(handle,pv7::compat::cocos2dx::JniRefKind::String)) {
            guest_strings_.erase(handle);return false;
        }
        return true;
    }    bool StageBitmap(const pv7::compat::cocos2dx::BitmapPixels& image,
                     std::uint32_t& handle, std::string& error) {
        if (!byte_arrays_.Create(image,handle,error)) return false;
        if (!refs_.RegisterLocal(handle,pv7::compat::cocos2dx::JniRefKind::ByteArray)) {
            byte_arrays_.Release(handle);
            error="JNI byte array reference collision";
            return false;
        }
        return true;
    }
    void ReleaseBitmap(std::uint32_t handle) {
        refs_.Delete(handle,pv7::compat::cocos2dx::JniRefScope::Local);
        byte_arrays_.Release(handle);
    }

    std::optional<std::uint32_t> MemoryReadCode(std::uint32_t a) override {
        // ARM32 kernels reserve the high kuser vector page. Dynarmic's
        // code-fetch callback must supply *both* the helper SVC and the
        // following BX LR; the translator can fetch beyond a helper entry.
        switch (a) {
        case 0xffff0fc0: return 0xef000000u | kKuserCmpxchg;
        case 0xffff0fa0: return 0xef000000u | kKuserMemoryBarrier;
        case 0xffff0fe0: return 0xef000000u | kKuserGetTls;
        case 0xffff0fc4:
        case 0xffff0fa4:
        case 0xffff0fe4: return 0xe12fff1eu; // BX LR
        default: break;
        }
        if (!Fits(a, 4, mem_.size())) { Fault(a, "code_fetch_out_of_range"); return std::nullopt; }
        return Read<std::uint32_t>(a);
    }
    std::uint8_t MemoryRead8(std::uint32_t a) override { return Read<std::uint8_t>(a); }
    std::uint16_t MemoryRead16(std::uint32_t a) override { return Read<std::uint16_t>(a); }
    std::uint32_t MemoryRead32(std::uint32_t a) override {
        if (a == 0xffff0ffc) return 3; // kuser helper version: TLS, CAS32, barrier
        return Read<std::uint32_t>(a);
    }
    std::uint64_t MemoryRead64(std::uint32_t a) override { return Read<std::uint64_t>(a); }
    void MemoryWrite8(std::uint32_t a, std::uint8_t v) override { Write(a,v); }
    void MemoryWrite16(std::uint32_t a, std::uint16_t v) override { Write(a,v); }
    void MemoryWrite32(std::uint32_t a, std::uint32_t v) override { Write(a,v); }
    void MemoryWrite64(std::uint32_t a, std::uint64_t v) override { Write(a,v); }
    void InterpreterFallback(std::uint32_t pc, std::size_t) override { Fault(pc, "interpreter_fallback"); }
    void ExceptionRaised(std::uint32_t pc, Dynarmic::A32::Exception) override { Fault(pc, "guest_exception"); }
    void AddTicks(std::uint64_t ticks) override {
        ticks_remaining = ticks >= ticks_remaining ? 0 : ticks_remaining - ticks;
        if (next_parser_sample_tick_ && jit &&
            ticks_remaining <= next_parser_sample_tick_) {
            const auto& r=jit->Regs();
            std::cout<<"COCOS_GUEST_PROGRESS remaining_ticks="<<ticks_remaining
                     <<" pc=0x"<<std::hex<<r[15]
                     <<" lr=0x"<<r[14]
                     <<" r0=0x"<<r[0]<<" r1=0x"<<r[1]
                     <<" r2=0x"<<r[2]<<" r3=0x"<<r[3]
                     <<" r4=0x"<<r[4]<<" r5=0x"<<r[5]
                     <<" r6=0x"<<r[6]<<" r7=0x"<<r[7]
                     <<" sp=0x"<<r[13] <<" cpsr=0x"<<jit->Cpsr()
                     <<std::dec;
            if (r[15] >= kImportsBase && r[15] < kImportsBase + imports_.size()*16u) {
                const auto idx=(r[15]-kImportsBase)/16u;
                std::cout<<" import="<<imports_[idx].name;
            }
            if (r[1] < mem_.size()) {
                std::cout<<" r1_bytes=";
                for(std::size_t k=0;k<16 && Fits(r[1],k+1,mem_.size());++k)
                    std::cout<<std::hex<<std::setfill('0')<<std::setw(2)
                             <<static_cast<unsigned>(mem_[r[1]+k]);
                std::cout<<std::dec;
            }
            if(Fits(r[13],64,mem_.size())) {
                std::cout<<" stack_return_candidates=";
                for(unsigned i=0;i<16;++i){
                    std::uint32_t candidate=0;
                    std::memcpy(&candidate,mem_.data()+r[13]+i*4u,4u);
                    if(candidate >= kImageBase && candidate < kImportsBase)
                        std::cout<<std::hex<<candidate<<"@"<<(i*4u)<<",";
                }
                std::cout<<std::dec;
            }
            std::cout<<"\n";
            next_parser_sample_tick_ =
                next_parser_sample_tick_ > 25000000u ?
                next_parser_sample_tick_-25000000u : 0;
        }
    }
    std::uint64_t GetTicksRemaining() override { return ticks_remaining; }
    void CallSVC(std::uint32_t svc) override {
        if (!jit) return;
        auto& regs = jit->Regs();
        if (svc == 0) {
            returned = true;
            jit->HaltExecution();
            return;
        }
        if (svc == kJniGetEnv || svc == kJniAttach) {
            if (!Put32(mem_, regs[1], kEnv)) { Fault(regs[1], "jni_out_pointer_invalid"); return; }
            regs[0] = 0; // JNI_OK only for these two specifically modeled operations
            return;
        }
        if (svc == kJniGetVersion) { regs[0] = 0x00010006; return; }
        if (svc == kKuserGetTls) { regs[0] = kGuestTls; return; }
        if (svc == kKuserMemoryBarrier) { return; } // single-threaded probe only
        if (svc == kKuserCmpxchg) {
            const std::uint32_t ptr = regs[2];
            if ((ptr & 3) || !Fits(ptr, 4, mem_.size())) {
                Fault(ptr, "kuser_cmpxchg_invalid_pointer"); return;
            }
            const std::uint32_t actual = Read<std::uint32_t>(ptr);
            const bool swapped = actual == regs[0];
            if (swapped) Write<std::uint32_t>(ptr, regs[1]);
            regs[0] = swapped ? 0u : 1u;
            const std::uint32_t cpsr = jit->Cpsr();
            jit->SetCpsr(swapped ? (cpsr | (1u << 29)) : (cpsr & ~(1u << 29)));
            return;
        }
        if (svc == kJniFindClass) {
            const auto name = CString(mem_, regs[1]);
            // Both class names are confirmed in the original Best Park APK.
            // Additional calls are resolved by exact method signature below.
            if (name == "com/turner/bestparkintheuniverse/regularshow") {
                constexpr std::uint32_t kGameClass=0x0000c030u;
                refs_.RegisterLocal(kGameClass,pv7::compat::cocos2dx::JniRefKind::Class);
                regs[0]=kGameClass;
                std::cout<<"JNI_GAME_CLASS_FOUND "<<name<<"\n";
                return;
            }
            if (name == "org/cocos2dx/lib/Cocos2dxHelper") { constexpr std::uint32_t kHelper=0xc050u; refs_.RegisterLocal(kHelper,pv7::compat::cocos2dx::JniRefKind::Class); regs[0]=kHelper; std::cout << "JNI_HELPER_CLASS_FOUND " << name << "\n"; return; }
            if (name == "org/cocos2dx/lib/Cocos2dxRenderer") { constexpr std::uint32_t kRenderer=0xc090u; refs_.RegisterLocal(kRenderer,pv7::compat::cocos2dx::JniRefKind::Class); regs[0]=kRenderer; std::cout << "JNI_RENDERER_CLASS_FOUND " << name << "\n"; return; }
            if (name != "org/cocos2dx/lib/Cocos2dxBitmap") {
                Fault(regs[15], "JNI_FindClass_unknown:" + name);
                return;
            }
            // Diagnostic-only identity for a class known to the original game.
            // No methods are simulated. The next JNI access remains fail-closed.
            constexpr std::uint32_t kDiagnosticBitmapClass = 0x0000c010;
            refs_.RegisterLocal(kDiagnosticBitmapClass,pv7::compat::cocos2dx::JniRefKind::Class);
            regs[0] = kDiagnosticBitmapClass;
            std::cout << "JNI_DIAGNOSTIC_CLASS " << name
                      << " token=0x" << std::hex << kDiagnosticBitmapClass
                      << std::dec << "\n";
            return;
        }
        if (svc == kJniRegister) {
            reason = "JNI_RegisterNatives_count:" + std::to_string(regs[3]);
            Fault(regs[15], reason);
            return;
        }
        if (svc == kMissingJniSvc + 113u) {
            const auto classRef=regs[1], methodName=regs[2], signature=regs[3];
            const auto name=CString(mem_,methodName), sig=CString(mem_,signature);
            std::cout << "JNI_STATIC_METHOD_REQUEST class=0x"
                      << std::hex << classRef << std::dec
                      << " method=" << name << " signature=" << sig << "\n";
            if (refs_.Resolve(classRef) == 0x0000c010u &&
                name == "createTextBitmap" &&
                sig == "(Ljava/lang/String;Ljava/lang/String;IIII)V") {
                constexpr std::uint32_t kTextMethod = 0x0000c020;
                regs[0] = kTextMethod;
                std::cout << "JNI_METHOD_IDENTIFIED createTextBitmap token=0x"
                          << std::hex << kTextMethod << std::dec << "\n";
                return;
            }

            if (refs_.Resolve(classRef) == 0x0000c030u &&
                name == "getAPKExpansionFileName" &&
                sig == "()Ljava/lang/String;") {
                constexpr std::uint32_t kExpansionMethod = 0x0000c040u;
                regs[0] = kExpansionMethod;
                std::cout << "JNI_METHOD_IDENTIFIED getAPKExpansionFileName\n";
                return;
            }            if (refs_.Resolve(classRef)==0xc050u && name=="getStringForKey" &&
                sig=="(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;") {
                regs[0]=0xc060u;
                std::cout<<"JNI_METHOD_IDENTIFIED getStringForKey\n";
                return;
            }            if (refs_.Resolve(classRef)==0xc050u && name=="getIntegerForKey" &&
                sig=="(Ljava/lang/String;I)I") {
                regs[0]=0xc070u;
                std::cout<<"JNI_METHOD_IDENTIFIED getIntegerForKey\n";
                return;
            }            if (refs_.Resolve(classRef)==0xc030u && name=="getCurrentLanguage" &&
                sig=="()Ljava/lang/String;") {
                regs[0]=0xc080u;
                std::cout<<"JNI_METHOD_IDENTIFIED getCurrentLanguage\n";
                return;
            }            if (refs_.Resolve(classRef)==0xc090u &&
                name=="setAnimationInterval" && sig=="(D)V") {
                regs[0]=0xc0a0u;
                std::cout<<"JNI_METHOD_IDENTIFIED setAnimationInterval\n";
                return;
            }            if (refs_.Resolve(classRef)==0xc050u && name=="getCocos2dxWritablePath" && sig=="()Ljava/lang/String;") { regs[0]=0xc0b0u; std::cout<<"JNI_METHOD_IDENTIFIED getCocos2dxWritablePath\n"; return; }
            if (refs_.Resolve(classRef)==0xc050u && name=="getBoolForKey" &&
                sig=="(Ljava/lang/String;Z)Z") {
                regs[0]=0xc0c0u;
                std::cout<<"JNI_METHOD_IDENTIFIED getBoolForKey\n";
                return;
            }
            if (refs_.Resolve(classRef)==0xc050u && name=="setBoolForKey" &&
                sig=="(Ljava/lang/String;Z)V") {
                regs[0]=0xc0d0u;
                std::cout<<"JNI_METHOD_IDENTIFIED setBoolForKey\n";
                return;
            }
            if (refs_.Resolve(classRef)==0xc050u && name=="getDeviceModel" &&
                sig=="()Ljava/lang/String;") {
                regs[0]=0xc0e0u;
                std::cout<<"JNI_METHOD_IDENTIFIED getDeviceModel\n";
                return;
            }
            if (refs_.Resolve(classRef)==0xc050u && name=="setBackgroundMusicVolume" &&
                sig=="(F)V") {
                regs[0]=0xc0f0u;
                std::cout<<"JNI_METHOD_IDENTIFIED setBackgroundMusicVolume\n";
                return;
            }
            Fault(regs[15], "JNI_GetStaticMethodID_unknown:" + name + sig);
            return;
        }

        if (svc>=kMissingJniSvc+114u && svc<=kMissingJniSvc+116u &&
            refs_.Resolve(regs[1])==0xc050u && regs[2]==0xc060u) {
#if defined(PV7_COCOS_JNI_RUNNER)
            const auto method=svc-kMissingJniSvc;
            const auto va=regs[3];
            std::uint32_t keyRef=0, defRef=0;
            if (method==114u) {
                if (!Fits(regs[13],4,mem_.size())) {
                    Fault(regs[15],"JNI_Pref_stack_out_of_range");return;
                }
                keyRef=regs[3]; defRef=Read<std::uint32_t>(regs[13]);
            } else {
                const auto stride=method==116u ? 8u : 4u;
                if (!Fits(va,stride+4u,mem_.size())) {
                    Fault(regs[15],"JNI_Pref_va_list_out_of_range");return;
                }
                keyRef=Read<std::uint32_t>(va);
                defRef=Read<std::uint32_t>(va+stride);
            }
            auto key=keyRef ? refs_.Resolve(keyRef) : std::optional<std::uint32_t>{};
            auto def=defRef ? refs_.Resolve(defRef) : std::optional<std::uint32_t>{};
            if ((keyRef&&(!key||!guest_strings_.count(*key))) ||
                (defRef&&(!def||!guest_strings_.count(*def)))) {
                Fault(regs[15],"JNI_Pref_invalid_jstring_refs");return;
            }
            JNIEnv* env=g_pv7_cocos_env;
            jclass cls=env->FindClass("me/jacqueb/pv7/cocos/CocosProbeActivity");
            jmethodID getter=cls ? env->GetStaticMethodID(cls,"readCocosPreference",
                "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;") : nullptr;
            if (env->ExceptionCheck()||!getter) {
                env->ExceptionDescribe();env->ExceptionClear();
                if (cls)env->DeleteLocalRef(cls);
                Fault(regs[15],"JNI_Pref_host_method_missing");return;
            }
            jstring jkey=key ? env->NewStringUTF(guest_strings_.at(*key).c_str()) : nullptr;
            jstring jdef=def ? env->NewStringUTF(guest_strings_.at(*def).c_str()) : nullptr;
            jstring result=static_cast<jstring>(env->CallStaticObjectMethod(
                cls,getter,jkey,jdef));
            if (jkey)env->DeleteLocalRef(jkey);
            if (jdef)env->DeleteLocalRef(jdef);
            env->DeleteLocalRef(cls);
            if (env->ExceptionCheck()) {
                env->ExceptionDescribe();env->ExceptionClear();
                if (result)env->DeleteLocalRef(result);
                Fault(regs[15],"JNI_Pref_java_exception");return;
            }
            if (!result) { regs[0]=0;return; }
            const char* chars=env->GetStringUTFChars(result,nullptr);
            if (env->ExceptionCheck()||!chars) {
                env->ExceptionDescribe();env->ExceptionClear();env->DeleteLocalRef(result);
                Fault(regs[15],"JNI_Pref_string_extract_failed");return;
            }
            std::string value(chars);
            env->ReleaseStringUTFChars(result,chars);
            env->DeleteLocalRef(result);
            if (value.size()>65536u || next_jstring_>0xfffcu) {
                Fault(regs[15],"JNI_Pref_result_too_large");return;
            }
            const auto handle=next_jstring_;next_jstring_+=4;
            guest_strings_.emplace(handle,value);
            if (!refs_.RegisterLocal(handle,pv7::compat::cocos2dx::JniRefKind::String)) {
                Fault(regs[15],"JNI_Pref_string_register_failed");return;
            }
            regs[0]=handle;
            std::cout<<"JNI_PREF_STRING_OK method="<<method<<" key="
                     <<(key?guest_strings_.at(*key):"<null>")
                     <<" bytes="<<value.size()<<"\n";
            return;
#else
            Fault(regs[15],"JNI_Pref_requires_Android_Activity");return;
#endif
        }        // JNI CallStaticBooleanMethod / V / A: exact original Cocos helper ABI.
        if (svc>=kMissingJniSvc+117u && svc<=kMissingJniSvc+119u &&
            refs_.Resolve(regs[1])==0xc050u && regs[2]==0xc0c0u) {
#if defined(PV7_COCOS_JNI_RUNNER)
            const auto slot=svc-kMissingJniSvc;
            std::uint32_t keyRef=0, fallback=0;
            if (slot==117u) {
                if (!Fits(regs[13],4,mem_.size())) {
                    Fault(regs[15],"JNI_PrefBool_stack_invalid");return;
                }
                keyRef=regs[3];
                fallback=Read<std::uint32_t>(regs[13]);
            } else {
                const auto stride=slot==119u ? 8u : 4u;
                if (!Fits(regs[3],stride+4u,mem_.size())) {
                    Fault(regs[15],"JNI_PrefBool_args_invalid");return;
                }
                keyRef=Read<std::uint32_t>(regs[3]);
                fallback=Read<std::uint32_t>(regs[3]+stride);
            }
            auto key=keyRef ? refs_.Resolve(keyRef) : std::optional<std::uint32_t>{};
            if (keyRef && (!key || !guest_strings_.count(*key))) {
                Fault(regs[15],"JNI_PrefBool_key_invalid");return;
            }
            JNIEnv* env=g_pv7_cocos_env;
            jclass cls=env->FindClass("me/jacqueb/pv7/cocos/CocosProbeActivity");
            jmethodID method=cls ? env->GetStaticMethodID(cls,"readCocosBoolean",
                "(Ljava/lang/String;Z)Z") : nullptr;
            if(env->ExceptionCheck() || !method) {
                env->ExceptionDescribe();env->ExceptionClear();
                if(cls)env->DeleteLocalRef(cls);
                Fault(regs[15],"JNI_PrefBool_host_method_missing");return;
            }
            jstring jkey=key ? env->NewStringUTF(guest_strings_.at(*key).c_str()) : nullptr;
            jboolean value=env->CallStaticBooleanMethod(cls,method,jkey,
                fallback ? JNI_TRUE : JNI_FALSE);
            if(jkey)env->DeleteLocalRef(jkey);
            env->DeleteLocalRef(cls);
            if(env->ExceptionCheck()) {
                env->ExceptionDescribe();env->ExceptionClear();
                Fault(regs[15],"JNI_PrefBool_java_exception");return;
            }
            regs[0]=value==JNI_TRUE ? 1u : 0u;
            std::cout<<"JNI_PREF_BOOLEAN_OK key="
                     <<(key?guest_strings_.at(*key):"<null>")
                     <<" fallback="<<(fallback!=0u)
                     <<" value="<<regs[0]<<"\n";
            return;
#else
            Fault(regs[15],"JNI_PrefBool_requires_Android_Activity");return;
#endif
        }
        if (svc>=kMissingJniSvc+129u && svc<=kMissingJniSvc+131u &&
            refs_.Resolve(regs[1])==0xc050u && regs[2]==0xc070u) {
#if defined(PV7_COCOS_JNI_RUNNER)
            const auto slot=svc-kMissingJniSvc;
            const auto va=regs[3];
            std::uint32_t keyRef=0, fallback=0;
            if (slot==129u) {
                if (!Fits(regs[13],4,mem_.size())) {
                    Fault(regs[15],"JNI_PrefInt_stack_invalid");return;
                }
                keyRef=regs[3];fallback=Read<std::uint32_t>(regs[13]);
            } else {
                const auto stride=slot==131u ? 8u : 4u;
                if (!Fits(va,stride+4,mem_.size())) {
                    Fault(regs[15],"JNI_PrefInt_args_invalid");return;
                }
                keyRef=Read<std::uint32_t>(va);
                fallback=Read<std::uint32_t>(va+stride);
            }
            auto key=keyRef ? refs_.Resolve(keyRef) : std::optional<std::uint32_t>{};
            if (keyRef && (!key || !guest_strings_.count(*key))) {
                Fault(regs[15],"JNI_PrefInt_key_invalid");return;
            }
            JNIEnv* env=g_pv7_cocos_env;
            jclass cls=env->FindClass("me/jacqueb/pv7/cocos/CocosProbeActivity");
            jmethodID method=cls ? env->GetStaticMethodID(cls,"readCocosInteger",
                "(Ljava/lang/String;I)I") : nullptr;
            if (env->ExceptionCheck()||!method) {
                env->ExceptionDescribe();env->ExceptionClear();
                if(cls)env->DeleteLocalRef(cls);
                Fault(regs[15],"JNI_PrefInt_java_method_missing");return;
            }
            jstring jkey=key ? env->NewStringUTF(guest_strings_.at(*key).c_str()) : nullptr;
            const jint value=env->CallStaticIntMethod(cls,method,jkey,
                static_cast<jint>(fallback));
            if(jkey)env->DeleteLocalRef(jkey);
            env->DeleteLocalRef(cls);
            if(env->ExceptionCheck()) {
                env->ExceptionDescribe();env->ExceptionClear();
                Fault(regs[15],"JNI_PrefInt_java_exception");return;
            }
            regs[0]=static_cast<std::uint32_t>(value);
            std::cout<<"JNI_PREF_INTEGER_OK key="
                     <<(key?guest_strings_.at(*key):"<null>")
                     <<" value="<<value<<"\n";
            return;
#else
            Fault(regs[15],"JNI_PrefInt_requires_Android_Activity");return;
#endif
        }        // The original version-16 Best Park Java method returns the absolute
        // path to its main expansion file iff it exists, or "" otherwise.
        // Resolve it from the diagnostic Activity's own OBB directory so the
        // guest and host do not need identical Android package identifiers.
        if (svc >= kMissingJniSvc + 114u && svc <= kMissingJniSvc + 116u) {
            const bool gameMethod = refs_.Resolve(regs[1]) == 0xc030u &&
                (regs[2] == 0xc040u || regs[2] == 0xc080u);
            const bool writableMethod = refs_.Resolve(regs[1]) == 0xc050u && regs[2] == 0xc0b0u;
            const bool modelMethod = refs_.Resolve(regs[1]) == 0xc050u && regs[2] == 0xc0e0u;
            if (!gameMethod && !writableMethod && !modelMethod) {
                Fault(regs[15], "JNI_CallStaticObjectMethod_unknown"); return;
            }
#if defined(PV7_COCOS_JNI_RUNNER)
            std::string path, resolver_error;
            const char* accessor = modelMethod ? "getGuestDeviceModel" :
                writableMethod ? "getGuestWritablePath" :
                regs[2] == 0xc080u ? "getGuestLanguage" : "resolveExpansionPath";
            if (!ResolveAndroidStaticString(accessor, path, resolver_error)) {
                Fault(regs[15], "JNI_ExpansionResolver:" + resolver_error); return;
            }
            if (path.size() >= 4096 || next_jstring_ > 0x0000fffcu) {
                Fault(regs[15], "JNI_ExpansionString_invalid"); return;
            }
            const auto handle = next_jstring_;
            next_jstring_ += 4;
            guest_strings_[handle] = path;
            if (!refs_.RegisterLocal(handle, pv7::compat::cocos2dx::JniRefKind::String)) {
                Fault(regs[15], "JNI_ExpansionString_register_failed"); return;
            }
            regs[0] = handle;
            std::cout << (modelMethod ? "COCOS_DEVICE_MODEL_RETURNED bytes=" :
                          writableMethod ? "COCOS_WRITABLE_PATH_RETURNED bytes=" :
                          regs[2]==0xc080u ? "COCOS_LANGUAGE_RETURNED bytes=" :
                          "COCOS_EXPANSION_NAME_RETURNED bytes=")
                      << path.size() << " value=" << path << "\n";
            return;
#else
            Fault(regs[15], "JNI_ExpansionName_requires_Android_Activity");
            return;
#endif
        }
        // Original JNI code reads expansion path through jstring UTF accessor.
        if (svc == kMissingJniSvc + 168u ||
            svc == kMissingJniSvc + 169u ||
            svc == kMissingJniSvc + 170u) {
            const auto ref=refs_.Resolve(regs[1]);
            if (!ref || !guest_strings_.count(*ref)) {
                Fault(regs[15], "JNI_ExpansionString_invalid_ref"); return;
            }
            const auto& value=guest_strings_.at(*ref);
            if (svc == kMissingJniSvc + 168u) {
                regs[0]=static_cast<std::uint32_t>(value.size()); return;
            }
            if (svc == kMissingJniSvc + 169u) {
                if (value.size() >= 65536) {
                    Fault(regs[15], "JNI_GetStringUTFChars_oversized"); return;
                }
                const auto address=Allocate(
                    static_cast<std::uint32_t>(value.size()+1),4u);
                if (!address || !Fits(address,value.size()+1,mem_.size())) {
                    Fault(regs[15], "JNI_GetStringUTFChars_allocation_failed"); return;
                }
                std::memcpy(mem_.data()+address,value.c_str(),value.size()+1);
                guest_string_borrows_[address]=*ref;
                if (regs[2]) {
                    if (!Fits(regs[2],1,mem_.size())) {
                        Fault(regs[15], "JNI_GetStringUTFChars_isCopy_out_of_range"); return;
                    }
                    mem_[regs[2]]=1;
                }
                regs[0]=address;
                std::cout<<"JNI_GET_STRING_UTF chars=0x"<<std::hex<<address
                         <<std::dec<<" bytes="<<value.size()<<"\n";
                return;
            }
            const auto iter=guest_string_borrows_.find(regs[2]);
            if (iter==guest_string_borrows_.end() || iter->second!=*ref) {
                Fault(regs[15], "JNI_ReleaseStringUTFChars_invalid_borrow"); return;
            }
            guest_string_borrows_.erase(iter);
            return;
        }
        // JNI 1.6 object-reference lifetime and identity calls.
        // Class, string, and byte-array objects keep canonical guest handles;
        // local/global JNI references are independently tracked aliases.
        if (svc == kMissingJniSvc + 21u || svc == kMissingJniSvc + 25u) {
            const auto scope = svc == kMissingJniSvc + 21u ?
                pv7::compat::cocos2dx::JniRefScope::Global :
                pv7::compat::cocos2dx::JniRefScope::Local;
            std::uint32_t alias = 0;
            std::string reference_error;
            if (!refs_.Duplicate(regs[1],scope,alias,reference_error)) {
                Fault(regs[15],"JNI_NewReference_failed:"+reference_error);
                return;
            }
            regs[0]=alias;
            return;
        }
        if (svc == kMissingJniSvc + 22u || svc == kMissingJniSvc + 23u) {
            const auto scope = svc == kMissingJniSvc + 22u ?
                pv7::compat::cocos2dx::JniRefScope::Global :
                pv7::compat::cocos2dx::JniRefScope::Local;
            const auto handle=regs[1];
            if (!refs_.Delete(handle,scope)) {
                Fault(regs[15],"JNI_DeleteReference_invalid");
                return;
            }
            // Backing string data stays available to outstanding aliases.
            // Its canonical identity is not valid for JNI calls after deletion.
            return;
        }
        if (svc == kMissingJniSvc + 24u) {
            regs[0]=refs_.IsSame(regs[1],regs[2])?1u:0u;
            return;
        }
        if (svc == kMissingJniSvc + 26u) {
            // This single-threaded probe can accommodate up to 4096 local
            // handles; no Java OOM exception has been synthesized.
            regs[0]=regs[1]<=4096u?0u:static_cast<std::uint32_t>(-4);
            return;
        }
        if (svc == kMissingJniSvc + 232u) {
            regs[0]=static_cast<std::uint32_t>(refs_.RefType(regs[1]));
            return;
        }
        if (svc == kMissingJniSvc + 15u) {
            regs[0]=0; // ExceptionOccurred: no exception pending
            return;
        }
        if (svc == kMissingJniSvc + 17u || svc == kMissingJniSvc + 228u) {
            // ExceptionClear (17) is void; ExceptionCheck (228) is false.
            if (svc == kMissingJniSvc + 228u) regs[0]=0;
            return;
        }        if (svc == kMissingJniSvc + 171u) {
            std::int32_t count=0;
            const auto ref=refs_.Resolve(regs[1]);
            if (!ref || !byte_arrays_.Length(*ref,count)) {
                Fault(regs[15],"JNI_GetArrayLength_unknown_reference");
                return;
            }
            regs[0]=static_cast<std::uint32_t>(count);
            return;
        }
        if (svc == kMissingJniSvc + 200u) {
            // JNI GetByteArrayRegion(JNIEnv*, jbyteArray, start, len, buf).
            const auto count=static_cast<std::int32_t>(regs[3]);
            const auto start=static_cast<std::int32_t>(regs[2]);
            if (!Fits(regs[13],4,mem_.size())) {
                Fault(regs[15],"JNI_GetByteArrayRegion_stack_invalid"); return;
            }
            const auto destination=Read<std::uint32_t>(regs[13]);
            if (count<0 || !Fits(destination,static_cast<std::size_t>(count),mem_.size()) ||
                !refs_.Resolve(regs[1]) ||
                !byte_arrays_.CopyRegion(*refs_.Resolve(regs[1]),start,count,mem_.data()+destination)) {
                Fault(regs[15],"JNI_GetByteArrayRegion_invalid"); return;
            }
            return;
        }        // Original Cocos preference writes: CallStaticVoidMethod / V / A.
        if (svc>=kMissingJniSvc+141u && svc<=kMissingJniSvc+143u &&
            refs_.Resolve(regs[1])==0xc050u && regs[2]==0xc0d0u) {
#if defined(PV7_COCOS_JNI_RUNNER)
            const auto slot=svc-kMissingJniSvc;
            std::uint32_t keyRef=0, booleanValue=0;
            if (slot==141u) {
                if (!Fits(regs[13],4,mem_.size())) {
                    Fault(regs[15],"JNI_SetPrefBool_stack_invalid");return;
                }
                keyRef=regs[3];
                booleanValue=Read<std::uint32_t>(regs[13]);
            } else {
                const auto stride=slot==143u ? 8u : 4u;
                if (!Fits(regs[3],stride+4u,mem_.size())) {
                    Fault(regs[15],"JNI_SetPrefBool_args_invalid");return;
                }
                keyRef=Read<std::uint32_t>(regs[3]);
                booleanValue=Read<std::uint32_t>(regs[3]+stride);
            }
            const auto key=keyRef ? refs_.Resolve(keyRef) : std::optional<std::uint32_t>{};
            if (keyRef && (!key || !guest_strings_.count(*key))) {
                Fault(regs[15],"JNI_SetPrefBool_key_invalid");return;
            }
            JNIEnv* env=g_pv7_cocos_env;
            jclass cls=env->FindClass("me/jacqueb/pv7/cocos/CocosProbeActivity");
            jmethodID method=cls ? env->GetStaticMethodID(cls,"writeCocosBoolean",
                "(Ljava/lang/String;Z)V") : nullptr;
            if(env->ExceptionCheck() || !method) {
                env->ExceptionDescribe();env->ExceptionClear();
                if(cls)env->DeleteLocalRef(cls);
                Fault(regs[15],"JNI_SetPrefBool_host_method_missing");return;
            }
            jstring jkey=key ? env->NewStringUTF(guest_strings_.at(*key).c_str()) : nullptr;
            env->CallStaticVoidMethod(cls,method,jkey,
                booleanValue ? JNI_TRUE : JNI_FALSE);
            if(jkey)env->DeleteLocalRef(jkey);
            env->DeleteLocalRef(cls);
            if(env->ExceptionCheck()) {
                env->ExceptionDescribe();env->ExceptionClear();
                Fault(regs[15],"JNI_SetPrefBool_java_exception");return;
            }
            std::cout<<"JNI_PREF_BOOLEAN_WRITE_OK key="
                     <<(key?guest_strings_.at(*key):"<null>")
                     <<" value="<<(booleanValue!=0u)<<"\n";
            return;
#else
            Fault(regs[15],"JNI_SetPrefBool_requires_Android_Activity");return;
#endif
        }
        // Exact original Cocos2dxHelper.setBackgroundMusicVolume(F)V.
        // ARM32 C varargs promote jfloat to double in CallStaticVoidMethodV.
        // The graphics probe tracks the volume state but does not fake audio.
        if (svc==kMissingJniSvc+142u && refs_.Resolve(regs[1])==0xc050u &&
            regs[2]==0xc0f0u) {
#if defined(PV7_COCOS_JNI_RUNNER)
            const auto aligned=(regs[3]+7u)&~7u;
            if (!Fits(aligned,8,mem_.size())) {
                Fault(regs[15],"JNI_MusicVolume_va_invalid");return;
            }
            double promoted=0;
            std::memcpy(&promoted,mem_.data()+aligned,sizeof(promoted));
            if (!std::isfinite(promoted) || std::abs(promoted)>1000000.0) {
                Fault(regs[15],"JNI_MusicVolume_value_invalid");return;
            }
            JNIEnv* env=g_pv7_cocos_env;
            jclass cls=env->FindClass("me/jacqueb/pv7/cocos/CocosProbeActivity");
            jmethodID setter=cls ? env->GetStaticMethodID(cls,
                "setGuestBackgroundMusicVolume","(F)V") : nullptr;
            if (env->ExceptionCheck()||!setter) {
                env->ExceptionDescribe();env->ExceptionClear();
                if(cls)env->DeleteLocalRef(cls);
                Fault(regs[15],"JNI_MusicVolume_host_method_missing");return;
            }
            env->CallStaticVoidMethod(cls,setter,static_cast<jfloat>(promoted));
            env->DeleteLocalRef(cls);
            if (env->ExceptionCheck()) {
                env->ExceptionDescribe();env->ExceptionClear();
                Fault(regs[15],"JNI_MusicVolume_java_exception");return;
            }
            std::cout<<"COCOS_BACKGROUND_MUSIC_VOLUME_TRACKED_NO_PLAYBACK value="
                     <<promoted<<"\n";
            return;
#else
            Fault(regs[15],"JNI_MusicVolume_requires_Android");return;
#endif
        }
        if (svc==kMissingJniSvc+142u && refs_.Resolve(regs[1])==0xc090u && regs[2]==0xc0a0u) {
#if defined(PV7_COCOS_JNI_RUNNER)
            if(!Fits(regs[3],8,mem_.size())){Fault(regs[15],"JNI_Interval_invalid_va");return;}
            double seconds=0;
            const auto aligned=(regs[3]+7u)&~7u; if(!Fits(aligned,8,mem_.size())){Fault(regs[15],"JNI_Interval_aligned_va_invalid");return;} std::cout<<"COCOS_ANIMATION_VA_TRACE ptr=0x"<<std::hex<<regs[3]<<" w0=0x"<<Read<std::uint32_t>(regs[3])<<" w1=0x"<<Read<std::uint32_t>(regs[3]+4u)<<" aligned0=0x"<<Read<std::uint32_t>(aligned)<<" aligned1=0x"<<Read<std::uint32_t>(aligned+4u)<<std::dec<<"\n"; std::memcpy(&seconds,mem_.data()+aligned,8);
            if (!std::isfinite(seconds) || seconds<=0 || seconds>10) {
                Fault(regs[15],"JNI_Interval_invalid_seconds");return;
            }
            JNIEnv* env=g_pv7_cocos_env;
            auto cls=env->FindClass("me/jacqueb/pv7/cocos/CocosProbeActivity");
            auto method=cls ? env->GetStaticMethodID(cls,"setGuestAnimationInterval","(D)V") : nullptr;
            if(env->ExceptionCheck()||!method) {
                env->ExceptionDescribe();env->ExceptionClear();
                if(cls)env->DeleteLocalRef(cls);
                Fault(regs[15],"JNI_Interval_host_missing");return;
            }
            env->CallStaticVoidMethod(cls,method,seconds);
            env->DeleteLocalRef(cls);
            if(env->ExceptionCheck()) {
                env->ExceptionDescribe();env->ExceptionClear();
                Fault(regs[15],"JNI_Interval_host_exception");return;
            }
            std::cout<<"COCOS_ANIMATION_INTERVAL_SET seconds="<<seconds<<"\n";
            return;
#else
            Fault(regs[15],"JNI_Interval_requires_Android");return;
#endif
        }        if (svc == kMissingJniSvc + 142u) {
            if (refs_.Resolve(regs[1]) != 0x0000c010u || regs[2] != 0x0000c020u) {
                Fault(regs[15], "JNI_CallStaticVoidMethodV_unknown_method");
                return;
            }
            pv7::compat::cocos2dx::BitmapJniMarshaller decoder;
            pv7::compat::cocos2dx::BitmapTextRequest request;
            std::string decodeError;
            std::unordered_map<std::uint32_t,std::string> handles(
                guest_strings_.begin(),guest_strings_.end());
            if (!decoder.Decode(mem_, regs[3],handles,request,decodeError)) {
                Fault(regs[15], "JNI_BitmapText_decode_failed:" + decodeError);
                return;
            }
            std::cout << "COCOS_BITMAP_TEXT_REQUEST text=" << request.text
                      << " font=" << request.font
                      << " size=" << request.font_size
                      << " align=" << request.alignment
                      << " dimensions=" << request.width << "x" << request.height
                      << "\n";
            if (pending_bitmap) {
                Fault(regs[15], "JNI_BitmapText_reentrant_request");
                return;
            }
            // Suspend this real guest JNI call. The driver runs a host text
            // backend, invokes the original nativeInitBitmapDC in Dynarmic,
            // then restores the interrupted guest registers and resumes.
            pending_bitmap = std::move(request);
            jit->HaltExecution();
            return;
        }        if (svc == kMissingJniSvc + 167u) {
            const auto addr = regs[1];
            constexpr std::size_t kMaxStringBytes = 65536;
            if (!addr || addr >= mem_.size()) {
                Fault(regs[15], "JNI_NewStringUTF_invalid_pointer"); return;
            }
            const auto lengthLimit = std::min(kMaxStringBytes, mem_.size()-addr);
            const auto* begin = mem_.data()+addr;
            const auto* end = static_cast<const std::uint8_t*>(
                std::memchr(begin, 0, lengthLimit));
            if (!end) {
                Fault(regs[15], "JNI_NewStringUTF_unterminated"); return;
            }
            if (next_jstring_ > 0x0000fffcu) {
                Fault(regs[15], "JNI_NewStringUTF_token_exhausted"); return;
            }
            const auto handle = next_jstring_;
            next_jstring_ += 4;
            guest_strings_.emplace(handle,
                std::string(reinterpret_cast<const char*>(begin),
                            static_cast<std::size_t>(end-begin)));
            refs_.RegisterLocal(handle,pv7::compat::cocos2dx::JniRefKind::String);
            regs[0] = handle;
            std::cout << "JNI_NEW_STRING_UTF handle=0x" << std::hex << handle
                      << std::dec << " bytes=" << guest_strings_.at(handle).size()
                      << "\n";
            return;
        }
        if (svc >= kMissingJniSvc && svc < kMissingJniSvc + 233u) {
            Fault(regs[15], "UNIMPLEMENTED_JNI_SLOT:" +
                  std::to_string(svc - kMissingJniSvc));
            return;
        }
        if (svc >= kImportSvcBase && svc < kImportSvcBase + imports_.size()) {
            const auto& item = imports_[svc - kImportSvcBase];
            if (item.name == "__android_log_print" ||
                item.name == "__android_log_write") {
                // Both signatures start with priority and tag. log_print has
                // a varargs format, log_write a literal message. This is a
                // bounded diagnostic sink; do NOT interpolate unknown args.
                if (log_count_++ < 16) {
                    std::cout << "ANDROID_LOG priority=" << regs[0]
                              << " tag=" << CString(mem_, regs[1])
                              << " format_or_message=" << CString(mem_, regs[2])
                              << "\n";
                }
                regs[0] = 0; // diagnostic sink accepted (not full liblog)
                return;
            }
            if(pv7::compat::cocos2dx::DispatchGuestMath(item.name,regs)) return;
            if (item.name=="sprintf" || item.name=="snprintf" ||
                item.name=="vsnprintf") {
                const bool unbounded=item.name=="sprintf";
                const auto dest=regs[0];
                const auto cap=unbounded ? 0u : regs[1];
                const auto fmt=unbounded ? regs[1] : regs[2];
                if (cap>65536u || (cap && !Fits(dest,cap,mem_.size()))) {
                    Fault(regs[15],"FORMAT32_INVALID_DESTINATION");return;
                }
                std::size_t argumentIndex=0;
                auto nextArgument=[&]() -> std::optional<std::uint32_t> {
                    if (argumentIndex>=64) return std::nullopt;
                    const auto index=argumentIndex++;
                    if (item.name=="sprintf" && index<2) return regs[2+index];
                    if (item.name=="snprintf" && index==0) return regs[3];
                    std::uint64_t address;
                    if (item.name=="vsnprintf") {
                        address=static_cast<std::uint64_t>(regs[3])+index*4u;
                    } else {
                        const auto stackIndex=item.name=="sprintf" ? index-2 : index-1;
                        address=static_cast<std::uint64_t>(regs[13])+stackIndex*4u;
                    }
                    if (address>mem_.size() || !Fits(static_cast<std::size_t>(address),4,mem_.size()))
                        return std::nullopt;
                    return Read<std::uint32_t>(static_cast<std::uint32_t>(address));
                };
                std::string rendered, formatError;
                if (!pv7::cocos_probe::FormatGuest32(mem_,fmt,nextArgument,rendered,formatError)) {
                    Fault(regs[15],"FORMAT32_UNSUPPORTED:"+formatError);return;
                }
                const auto bytes=unbounded ? rendered.size()+1 : cap;
                if (bytes && !Fits(dest,bytes,mem_.size())) {
                    Fault(regs[15],"FORMAT32_OUTPUT_INVALID");return;
                }
                if (bytes) {
                    const auto count=std::min<std::size_t>(rendered.size(),bytes-1);
                    std::memmove(mem_.data()+dest,rendered.data(),count);
                    mem_[dest+count]=0;
                }
                regs[0]=static_cast<std::uint32_t>(rendered.size());
                std::cout<<"COCOS_FORMAT32_OK name="<<item.name
                         <<" result_bytes="<<rendered.size()<<"\n";
                return;
            }            if (item.name=="atoi") {
                std::string input,error;
                if (!pv7::cocos_probe::ReadGuestText(mem_,regs[0],input,error)) {
                    Fault(regs[15],"ATOI_GUEST_STRING_INVALID:"+error);return;
                }
                std::size_t i=0;
                while(i<input.size() && (input[i]==' ' || input[i]=='\t' ||
                    input[i]=='\n' || input[i]=='\r' ||
                    input[i]=='\f' || input[i]=='\v')) ++i;
                bool negative=false;
                if(i<input.size() && (input[i]=='+' || input[i]=='-'))
                    negative=input[i++]=='-';
                std::uint64_t value=0;
                const auto first=i;
                const auto limit=negative ? 2147483648ull : 2147483647ull;
                while(i<input.size() && input[i]>='0' && input[i]<='9') {
                    const auto digit=static_cast<unsigned>(input[i++]-'0');
                    if(value>(limit-digit)/10u) {
                        Fault(regs[15],"ATOI_GUEST_INT32_OVERFLOW");return;
                    }
                    value=value*10u+digit;
                }
                regs[0]=first==i ? 0u : static_cast<std::uint32_t>(
                    negative ? -static_cast<std::int64_t>(value) :
                               static_cast<std::int64_t>(value));
                std::cout<<"COCOS_ATOI_OK value="<<static_cast<std::int32_t>(regs[0])<<"\n";
                return;
            }            if (item.name=="gettimeofday") {
                // Android ARM32 EABI timeval: two signed 32-bit fields.
                const auto destination=regs[0], timezone=regs[1];
                if (!destination || timezone || !Fits(destination,8,mem_.size())) {
                    Fault(regs[15],"GETTIMEOFDAY_GUEST_LAYOUT_INVALID");return;
                }
                const auto micros=std::chrono::duration_cast<std::chrono::microseconds>(
                    std::chrono::system_clock::now().time_since_epoch()).count();
                const auto seconds=micros/1000000;
                const auto fraction=micros%1000000;
                if (seconds>2147483647LL || seconds<0 || fraction<0) {
                    Fault(regs[15],"GETTIMEOFDAY_TIME32_OVERFLOW");return;
                }
                if (!Put32(mem_,destination,static_cast<std::uint32_t>(seconds)) ||
                    !Put32(mem_,destination+4,static_cast<std::uint32_t>(fraction))) {
                    Fault(regs[15],"GETTIMEOFDAY_WRITE_FAILED");return;
                }
                regs[0]=0;
                static unsigned timeCalls=0;
                if (++timeCalls<=4 || timeCalls%1000u==0)
                    std::cout<<"COCOS_GUEST_GETTIMEOFDAY_OK count="<<timeCalls<<"\n";
                return;
            }
#if defined(PV7_COCOS_JNI_RUNNER)
            if (item.name=="inflateInit2_" || item.name=="inflateInit_") {
                constexpr std::uint32_t guest_size=56u;
                const auto guest=regs[0];
                const auto requested_size=item.name=="inflateInit2_" ? regs[3] : regs[2];
                if (!Fits(guest,guest_size,mem_.size()) || requested_size!=guest_size) {
                    Fault(regs[15],"ZLIB_GUEST_STREAM_LAYOUT_UNSUPPORTED");return;
                }
                if(z_streams_.size()>=32u || z_streams_.count(guest)) {
                    Fault(regs[15],"ZLIB_STREAM_ALREADY_INITIALIZED_OR_LIMIT");return;
                }
                const auto guest_version=CString(mem_,item.name=="inflateInit2_" ? regs[2] : regs[1]);
                if(guest_version.empty() || guest_version[0]!='1') {
                    Fault(regs[15],"ZLIB_GUEST_VERSION_INCOMPATIBLE");return;
                }
                const int window_bits=item.name=="inflateInit2_" ?
                    static_cast<std::int32_t>(regs[1]) : MAX_WBITS;
                if (window_bits < -15 || window_bits > 47) {
                    Fault(regs[15],"ZLIB_WINDOW_BITS_INVALID");return;
                }
                auto [stream_it, inserted] = z_streams_.try_emplace(guest);
                z_stream& stream=stream_it->second;
                const int result=::inflateInit2_(&stream,window_bits,
                    ZLIB_VERSION,static_cast<int>(sizeof(stream)));
                regs[0]=static_cast<std::uint32_t>(result);
                if(result==Z_OK) {
                    Put32(mem_,guest+28u,0x0000d400u);
                    std::cout<<"COCOS_GUEST_ZLIB_INIT_OK window_bits="
                             <<window_bits<<"\n";
                } else {
                    z_streams_.erase(guest);
                    std::cout<<"COCOS_GUEST_ZLIB_INIT_FAILED status="<<result<<"\n";
                }
                return;
            }
            if (item.name=="crc32" || item.name=="adler32") {
                const auto address=regs[1], count=regs[2];
                if(count>16u*1024u*1024u ||
                   (count && !Fits(address,count,mem_.size()))) {
                    Fault(regs[15],"ZLIB_CHECKSUM_INVALID_GUEST_RANGE");return;
                }
                const Bytef* bytes=count ? mem_.data()+address : Z_NULL;
                const uLong result=item.name=="crc32" ?
                    ::crc32(regs[0],bytes,count) : ::adler32(regs[0],bytes,count);
                regs[0]=static_cast<std::uint32_t>(result);
                static unsigned long long checksum_calls=0;
                ++checksum_calls;
                if(checksum_calls<=8 || checksum_calls%10000u==0)
                    std::cout<<"COCOS_GUEST_ZLIB_CHECKSUM_OK name="<<item.name
                             <<" input_bytes="<<count
                             <<" calls="<<checksum_calls<<"\n";
                return;
            }            if (item.name=="inflate" || item.name=="inflateEnd" ||
                item.name=="inflateReset") {
                const auto guest=regs[0];
                auto it=z_streams_.find(guest);
                if (it==z_streams_.end() || !Fits(guest,56u,mem_.size())) {
                    Fault(regs[15],"ZLIB_STREAM_UNKNOWN_OR_INVALID");return;
                }
                z_stream& stream=it->second;
                if(item.name=="inflateEnd") {
                    const int result=::inflateEnd(&stream);
                    z_streams_.erase(it);
                    Put32(mem_,guest+28u,0);
                    regs[0]=static_cast<std::uint32_t>(result);
                    return;
                }
                if(item.name=="inflateReset") {
                    const int result=::inflateReset(&stream);
                    regs[0]=static_cast<std::uint32_t>(result);
                    return;
                }
                const auto in=Read<std::uint32_t>(guest);
                const auto in_count=Read<std::uint32_t>(guest+4u);
                const auto out=Read<std::uint32_t>(guest+12u);
                const auto out_count=Read<std::uint32_t>(guest+16u);
                if(in_count>16u*1024u*1024u || out_count>16u*1024u*1024u ||
                   (in_count&&!Fits(in,in_count,mem_.size())) ||
                   (out_count&&!Fits(out,out_count,mem_.size()))) {
                    Fault(regs[15],"ZLIB_GUEST_BUFFER_RANGE_INVALID");return;
                }
                stream.next_in=in_count ? mem_.data()+in : Z_NULL;
                stream.avail_in=in_count;
                stream.next_out=out_count ? mem_.data()+out : Z_NULL;
                stream.avail_out=out_count;
                const int flush=static_cast<std::int32_t>(regs[1]);
                if(flush<Z_NO_FLUSH || flush>Z_TREES) {
                    Fault(regs[15],"ZLIB_INVALID_FLUSH_MODE");return;
                }
                const int result=::inflate(&stream,flush);
                const auto consumed=in_count-stream.avail_in;
                const auto written=out_count-stream.avail_out;
                if(stream.total_in>UINT32_MAX || stream.total_out>UINT32_MAX ||
                   stream.adler>UINT32_MAX) {
                    Fault(regs[15],"ZLIB_GUEST_COUNTER_OVERFLOW");return;
                }
                Put32(mem_,guest,in+consumed);
                Put32(mem_,guest+4u,stream.avail_in);
                Put32(mem_,guest+8u,static_cast<std::uint32_t>(stream.total_in));
                Put32(mem_,guest+12u,out+written);
                Put32(mem_,guest+16u,stream.avail_out);
                Put32(mem_,guest+20u,static_cast<std::uint32_t>(stream.total_out));
                Put32(mem_,guest+44u,static_cast<std::uint32_t>(stream.data_type));
                Put32(mem_,guest+48u,static_cast<std::uint32_t>(stream.adler));
                regs[0]=static_cast<std::uint32_t>(result);
                static unsigned inflate_calls=0;
                if(++inflate_calls<=6 || inflate_calls%5000u==0) {
                    std::cout<<"COCOS_GUEST_ZLIB_INFLATE status="<<result
                             <<" consumed="<<consumed<<" produced="<<written
                             <<" calls="<<inflate_calls<<"\n";
                }
                return;
            }
#endif
            if (item.name=="setjmp" || item.name=="_setjmp") {
                const auto buffer=regs[0];
                if ((buffer&3u) || !Fits(buffer,32u,mem_.size()) ||
                    (guest_jump_buffers_.size()>=64 && !guest_jump_buffers_.count(buffer))) {
                    Fault(regs[15],"GUEST_SETJMP_INVALID_BUFFER");return;
                }
                GuestJumpCheckpoint checkpoint{};
                checkpoint.regs=regs;
                checkpoint.cpsr=jit->Cpsr();
                guest_jump_buffers_[buffer]=checkpoint;
                regs[0]=0;
                std::cout<<"COCOS_GUEST_SETJMP_REGISTERED buffer=0x"<<std::hex
                         <<buffer<<std::dec<<"\n";
                return;
            }
            if (item.name=="longjmp" || item.name=="_longjmp" ||
                item.name=="siglongjmp") {
                Fault(regs[15],"GUEST_LONGJMP_NEEDS_TESTED_ARM32_CONTINUATION");
                return;
            }
            if (item.name=="getenv") {
                std::string name,error;
                if (!pv7::cocos_probe::ReadGuestText(mem_,regs[0],name,error)) {
                    Fault(regs[15],"GETENV_INVALID_GUEST_NAME:"+error);return;
                }
                if(name=="XML_MEM_BREAKPOINT" || name=="XML_MEM_TRACE") {
                    regs[0]=0; // unset optional libxml2 instrumentation variables
                    std::cout<<"COCOS_GUEST_GETENV_UNSET name="<<name<<"\n";
                    return;
                }
                Fault(regs[15],"GETENV_UNMODELED_NAME:"+name);
                return;
            }
            if (item.name=="strtod" || item.name=="strtof" || item.name=="atof") {
                const auto original=regs[0];
                std::string input,error;
                if (!pv7::cocos_probe::ReadGuestText(mem_,original,input,error)) {
                    Fault(regs[15],"STRTO_FLOAT_GUEST_STRING_INVALID:"+error);return;
                }
                char* end=nullptr;
                double result=std::strtod(input.c_str(),&end);
                const auto consumed=static_cast<std::uint32_t>(end-input.c_str());
                if(item.name!="atof" && regs[1]) {
                    if (!Put32(mem_,regs[1],original+consumed)) {
                        Fault(regs[15],"STRTO_FLOAT_END_POINTER_INVALID");return;
                    }
                }
                if(item.name=="strtof") {
                    const float value=static_cast<float>(result);
                    std::memcpy(&regs[0],&value,4u);
                } else {
                    std::uint64_t bits=0;
                    std::memcpy(&bits,&result,sizeof(bits));
                    regs[0]=static_cast<std::uint32_t>(bits);
                    regs[1]=static_cast<std::uint32_t>(bits>>32u);
                }
                static unsigned conversions=0;
                if(++conversions<=6 || conversions%10000u==0)
                    std::cout<<"COCOS_GUEST_STRTO_FLOAT_OK fn="<<item.name
                             <<" parsed_bytes="<<consumed<<" value="<<result<<"\n";
                return;
            }
            if (item.name=="fopen" || item.name=="fopen64") {
                const auto filename=CString(mem_,regs[0]);
                const auto mode=CString(mem_,regs[1]);
                std::cout<<"COCOS_FILE_REQUEST name="<<filename
                         <<" mode="<<mode<<"\n";
                if(filename.empty() && (mode=="rb" || mode=="r" || mode=="rt")) {
                    regs[0]=0; // ENOENT for empty path, no fake stream
                    return;
                }
#if defined(PV7_COCOS_JNI_RUNNER)
                std::string authorized, error;
                if (!ResolveAndroidExpansionPath(authorized, error)) {
                    Fault(regs[15], "VFS_OBB_RESOLVER_ERROR:" + error); return;
                }
                std::string apk_path, apk_error;
                if (!ResolveAndroidStaticString("getGuestApkPath",apk_path,apk_error)) {
                    Fault(regs[15],"VFS_APK_RESOLVER_ERROR:"+apk_error);return;
                }
                const bool isObb = !authorized.empty() && filename==authorized;
                const bool isApk = !apk_path.empty() && filename==apk_path;
                std::string files_root, files_error;
                if (!ResolveAndroidStaticString("getGuestWritablePath",
                                                files_root,files_error)) {
                    Fault(regs[15],"VFS_GUEST_FILES_RESOLVER_ERROR:"+files_error);return;
                }
                const std::string files_prefix=files_root+"/";
                const bool underFiles=filename.rfind(files_prefix,0)==0;
                bool validPrivateFile=underFiles;
                if(underFiles) {
                    const auto relative=filename.substr(files_prefix.size());
                    validPrivateFile=!relative.empty() && relative.size()<=512u &&
                        relative.find('\\')==std::string::npos &&
                        relative.find(':')==std::string::npos;
                    std::size_t pos=0;
                    while(validPrivateFile && pos<relative.size()) {
                        const auto end=relative.find('/',pos);
                        const auto len=(end==std::string::npos?relative.size():end)-pos;
                        const auto part=relative.substr(pos,len);
                        if(part.empty() || part=="." || part=="..") validPrivateFile=false;
                        pos=end==std::string::npos?relative.size():end+1u;
                    }
                }
                const bool virtualEntry =
                    filename.rfind("/sd/",0)==0 || filename.rfind("/hd/",0)==0 ||
                    filename.rfind("/ipad3/",0)==0 || filename.rfind("/sounds/",0)==0;
                if ((!isObb && !isApk && !virtualEntry && !validPrivateFile) ||
                    (mode!="rb" && mode!="r" && mode!="rt")) {
                    Fault(regs[15],"VFS_OPEN_NOT_AUTHORIZED");return;
                }
                std::string host_path=filename;
                if (virtualEntry) {
                    std::string zip_error;
                    if (!ResolveAndroidObbZipEntry(filename,host_path,zip_error)) {
                        Fault(regs[15],"VFS_OBB_ZIP_ENTRY_ERROR:"+zip_error);return;
                    }
                    if (host_path.empty()) {
                        regs[0]=0;
                        std::cout<<"COCOS_VFS_OBB_ZIP_ENTRY_NOT_FOUND "<<filename<<"\n";
                        return;
                    }
                }
                if (open_files_.size() >= 32u) {
                    Fault(regs[15], "VFS_OPEN_FILE_LIMIT"); return;
                }
                std::FILE* fp = std::fopen(host_path.c_str(), "rb");
                if (!fp) {
                    regs[0] = 0; // underlying OS failed opening real OBB
                    std::cout<<"COCOS_VFS_OBB_OPEN_OS_FAILED\n";
                    return;
                }
                const auto handle = next_file_handle_;
                next_file_handle_ += 4u;
                open_files_.emplace(handle,fp);
                regs[0]=handle;
                std::cout<<(isObb?"COCOS_VFS_OBB_OPEN_OK handle=0x":(isApk?"COCOS_VFS_APK_OPEN_OK handle=0x":(validPrivateFile?"COCOS_VFS_PRIVATE_FILE_OPEN_OK handle=0x":"COCOS_VFS_ZIP_ENTRY_OPEN_OK handle=0x")))<<std::hex<<handle
                         <<std::dec<<"\n";
                return;
#else
                Fault(regs[15],"VFS_OPEN_NOT_CONNECTED");return;
#endif
            }
#if defined(PV7_COCOS_JNI_RUNNER)
            if (item.name=="fread") {
                const auto dest=regs[0], unit=regs[1], count=regs[2],
                           handle=regs[3];
                auto it=open_files_.find(handle);
                const auto total=static_cast<std::uint64_t>(unit)*count;
                if(it==open_files_.end() || total>16u*1024u*1024u ||
                   !Fits(dest,static_cast<std::size_t>(total),mem_.size())) {
                    Fault(regs[15],"VFS_READ_INVALID_RANGE_OR_FILE");return;
                }
                const auto read=std::fread(mem_.data()+dest,unit,count,it->second);
                regs[0]=static_cast<std::uint32_t>(read);
                static unsigned long long read_calls=0; if (++read_calls<=4 || read_calls%65536==0) std::cout<<"COCOS_VFS_READ units="<<read
                         <<" unit_bytes="<<unit<<"\n";
                return;
            }
            if (item.name=="fseek" || item.name=="fseeko" ||
                item.name=="fseeko64") {
                auto it=open_files_.find(regs[0]);
                if(it==open_files_.end() || regs[2]>2) {
                    Fault(regs[15],"VFS_SEEK_BAD_FILE_OR_WHENCE");return;
                }
                const auto offset=static_cast<std::int32_t>(regs[1]);
                regs[0]=std::fseek(it->second,offset,static_cast<int>(regs[2]));
                return;
            }
            if (item.name=="ftell" || item.name=="ftello" || item.name=="ftello64") {
                auto it=open_files_.find(regs[0]);
                if(it==open_files_.end()) {
                    Fault(regs[15],"VFS_TELL_UNKNOWN_FILE");return;
                }
                const auto location=std::ftell(it->second);
                if(location<0 || static_cast<std::uint64_t>(location)>UINT32_MAX) {
                    Fault(regs[15],"VFS_TELL_OFFSET_INVALID");return;
                }
                regs[0]=static_cast<std::uint32_t>(location);
                return;
            }
            if (item.name=="fclose") {
                auto it=open_files_.find(regs[0]);
                if(it==open_files_.end()) {
                    Fault(regs[15],"VFS_CLOSE_UNKNOWN_FILE");return;
                }
                const int status=std::fclose(it->second);
                open_files_.erase(it);
                regs[0]=static_cast<std::uint32_t>(status);
                return;
            }
            if (item.name=="feof" || item.name=="ferror" ||
                item.name=="clearerr" || item.name=="rewind") {
                auto it=open_files_.find(regs[0]);
                if(it==open_files_.end()) {
                    Fault(regs[15],"VFS_STATUS_UNKNOWN_FILE");return;
                }
                if(item.name=="feof") regs[0]=std::feof(it->second);
                else if(item.name=="ferror") regs[0]=std::ferror(it->second);
                else if(item.name=="clearerr") std::clearerr(it->second);
                else std::rewind(it->second);
                return;
            }
#endif            // Bounded guest-memory equivalents of common Bionic routines.
            // These operate solely within the ARM guest address space.
            if (item.name == "memchr") {
                const auto address=regs[0], value=regs[1]&255u, count=regs[2];
                if (count>16u*1024u*1024u || !Fits(address,count,mem_.size())) {
                    Fault(regs[15],"memchr_invalid_guest_range"); return;
                }
                const auto* first=mem_.data()+address;
                const auto* found=static_cast<const std::uint8_t*>(
                    std::memchr(first,static_cast<int>(value),count));
                regs[0]=found ? address+static_cast<std::uint32_t>(found-first) : 0u;
                return;
            }            if (item.name == "memcpy" || item.name == "memmove" ||
                item.name == "__aeabi_memcpy" || item.name == "__aeabi_memcpy4" ||
                item.name == "__aeabi_memcpy8") {
                const auto d = regs[0], src = regs[1], count = regs[2];
                if (count > 1024 * 1024 || !Fits(d, count, mem_.size()) ||
                    !Fits(src, count, mem_.size())) {
                    Fault(regs[15], "memcpy_invalid_range"); return;
                }
                std::memmove(mem_.data() + d, mem_.data() + src, count);
                regs[0] = d;
                return;
            }
            if (item.name == "memset" || item.name == "__aeabi_memset" ||
                item.name == "__aeabi_memset4" || item.name == "__aeabi_memset8" ||
                item.name == "bzero") {
                const auto d = regs[0];
                const bool aeabi = item.name.rfind("__aeabi_", 0) == 0;
                const auto count = aeabi || item.name == "bzero" ? regs[1] : regs[2];
                const auto byte = item.name == "bzero" ? 0u : (aeabi ? regs[2] : regs[1]);
                if (count > 1024 * 1024 || !Fits(d, count, mem_.size())) {
                    Fault(regs[15], "memset_invalid_range"); return;
                }
                std::memset(mem_.data() + d, static_cast<int>(byte & 255u), count);
                regs[0] = d;
                return;
            }
            if (item.name == "memcmp") {
                const auto a = regs[0], b = regs[1], len = regs[2];
                if (len > 1024 * 1024 || !Fits(a,len,mem_.size()) ||
                    !Fits(b,len,mem_.size())) {
                    Fault(regs[15], "memcmp_invalid_range"); return;
                }
                int diff = std::memcmp(mem_.data()+a, mem_.data()+b, len);
                regs[0] = static_cast<std::uint32_t>(diff);
                return;
            }
            // Bionic string helpers on guest bytes. Return guest pointers,
            // never addresses into the host process. Bound both inputs and
            // validate termination to prevent probing beyond guest RAM.
            if (item.name == "strstr" || item.name == "strdup") {
                const auto first=regs[0];
                const auto second=regs[1];
                const auto scan=[&](std::uint32_t address)->std::optional<std::string_view> {
                    if(address>=mem_.size()) return std::nullopt;
                    const auto limit=std::min<std::size_t>(65536,mem_.size()-address);
                    const auto* start=mem_.data()+address;
                    const auto* end=static_cast<const std::uint8_t*>(
                        std::memchr(start,0,limit));
                    if(!end) return std::nullopt;
                    return std::string_view(reinterpret_cast<const char*>(start),
                                            static_cast<std::size_t>(end-start));
                };
                const auto haystack=scan(first);
                if(!haystack) {Fault(regs[15],"string_helper_invalid_source");return;}
                if(item.name=="strdup") {
                    const auto result=Allocate(static_cast<std::uint32_t>(haystack->size()+1),4);
                    if(!result) {regs[0]=0;return;}
                    std::memcpy(mem_.data()+result,mem_.data()+first,haystack->size()+1);
                    regs[0]=result;
                    return;
                }
                const auto needle=scan(second);
                if(!needle) {Fault(regs[15],"strstr_invalid_needle");return;}
                const auto where=haystack->find(*needle);
                regs[0]=where==std::string_view::npos?
                    0u : first+static_cast<std::uint32_t>(where);
                return;
            }
            if(item.name=="strnlen") {
                const auto start=regs[0],max=regs[1];
                if(max>65536u || !Fits(start,max,mem_.size())) {
                    Fault(regs[15],"strnlen_invalid_range");return;
                }
                std::uint32_t count=0;
                while(count<max && mem_[start+count]) ++count;
                regs[0]=count;
                return;
            }            if (item.name == "strlen") {
                // Original Best Park TinyXML2 StrPair::ParseText calls
                // strlen("<") at this Thumb return address. A repeating
                // input pointer means parser cursor is not advancing.
                // Fail closed in the diagnostic before consuming all guest RAM.
                if (regs[14] == kImageBase + 0x2279e9u &&
                    regs[1] < mem_.size() && mem_[regs[1]]=='<' &&
                    CString(mem_,regs[0])=="<") {
                    if (regs[1] == last_tinyxml_input_)
                        ++same_tinyxml_input_count_;
                    else {
                        last_tinyxml_input_=regs[1];
                        same_tinyxml_input_count_=1;
                    }
                    if (same_tinyxml_input_count_ > 8192u) {
                        std::cout<<"TINYXML2_GUEST_INPUT_NOT_ADVANCING ptr=0x"
                                 <<std::hex<<last_tinyxml_input_
                                 <<std::dec<<" repeats="<<same_tinyxml_input_count_<<"\n";
                        Fault(regs[15],"TINYXML2_REPEATED_PARSE_INPUT");
                        return;
                    }
                }
                const auto a = regs[0];
                if (a >= mem_.size()) { Fault(a, "strlen_invalid_pointer"); return; }
                std::size_t n = 0;
                while (n < 1024 * 1024 && Fits(a,n+1,mem_.size()) && mem_[a+n]) ++n;
                if (!Fits(a,n+1,mem_.size()) || n == 1024 * 1024) {
                    Fault(a, "strlen_unterminated"); return;
                }
                regs[0] = static_cast<std::uint32_t>(n);
                return;
            }
            if (item.name == "strcmp" || item.name == "strncmp") {
                const auto a = regs[0], b = regs[1];
                const auto maxcount = item.name == "strncmp"
                    ? std::min<std::uint32_t>(regs[2], 65536u) : 65536u;
                if (item.name == "strncmp" && regs[2] > 65536u) {
                    Fault(regs[15], "strncmp_bound_exceeded"); return;
                }
                int diff = 0;
                bool found_end = false;
                for (std::uint32_t i=0;i<maxcount;++i) {
                    if (!Fits(a,i+1,mem_.size()) || !Fits(b,i+1,mem_.size())) {
                        Fault(regs[15], "strcmp_invalid_pointer"); return;
                    }
                    const auto chA = mem_[a+i], chB = mem_[b+i];
                    if (chA != chB) { diff = static_cast<int>(chA) - static_cast<int>(chB); found_end=true; break; }
                    if (!chA) { found_end=true; break; }
                }
                if (item.name == "strcmp" && !found_end) {
                    Fault(regs[15], "strcmp_unterminated"); return;
                }
                regs[0] = static_cast<std::uint32_t>(diff);
                return;
            }
            if (item.name == "strcpy" || item.name == "strncpy") {
                const auto d=regs[0], src=regs[1];
                const auto limit=item.name == "strncpy"
                    ? std::min<std::uint32_t>(regs[2],65536u) : 65536u;
                if (item.name == "strncpy" && regs[2]>65536u) {
                    Fault(regs[15],"strncpy_bound_exceeded");return;
                }
                bool ended=false;
                for(std::uint32_t i=0;i<limit;++i){
                    if(!Fits(d,i+1,mem_.size())||!Fits(src,i+1,mem_.size())){
                        Fault(regs[15],"strcpy_invalid_pointer");return;
                    }
                    std::uint8_t b=ended ? 0 : mem_[src+i];
                    mem_[d+i]=b;
                    if(!b)ended=true;
                    if(ended&&item.name=="strcpy")break;
                }
                if(item.name=="strcpy"&&!ended){Fault(regs[15],"strcpy_unterminated");return;}
                regs[0]=d;
                return;
            }
            if (item.name == "strchr" || item.name == "strrchr") {
                const auto a=regs[0]; const auto ch=static_cast<std::uint8_t>(regs[1]);
                std::uint32_t found=0;
                bool ended=false;
                for(std::uint32_t i=0;i<65536u;++i){
                    if(!Fits(a,i+1,mem_.size())){Fault(regs[15],"strchr_invalid_pointer");return;}
                    if(mem_[a+i]==ch) { found=a+i; if(item.name=="strchr")break; }
                    if(!mem_[a+i]){ended=true;break;}
                }
                if(!ended && item.name=="strrchr"){Fault(regs[15],"strrchr_unterminated");return;}
                regs[0]=found;
                return;
            }
            if (item.name == "wctob") {
                // Single-byte C locale. WEOF/not-ASCII => EOF (-1).
                regs[0] = regs[0] <= 0x7fu ? regs[0] : 0xffffffffu;
                return;
            }
            if (item.name == "btowc") {
                // C locale byte to wide. EOF stays WEOF.
                regs[0] = regs[0] <= 0x7fu ? regs[0] : 0xffffffffu;
                return;
            }
            if (item.name == "wctype") {
                const auto category = CString(mem_, regs[0]);
                constexpr const char* names[] = {
                    "alnum", "alpha", "blank", "cntrl", "digit", "graph",
                    "lower", "print", "punct", "space", "upper", "xdigit"
                };
                regs[0] = 0;
                for (unsigned i=0;i<std::size(names);++i) {
                    if (category == names[i]) { regs[0] = i+1; break; }
                }
                return;
            }
            if (item.name == "iswctype") {
                const auto code = regs[0], category = regs[1];
                const bool ascii = code < 128u;
                const auto isAlpha = ascii && ((code >= 'A' && code <= 'Z') ||
                                                (code >= 'a' && code <= 'z'));
                const auto isDigit = code >= '0' && code <= '9';
                const auto isSpace = code == ' ' || (code >= 9 && code <= 13);
                const auto isCntrl = code < 32 || code == 127;
                const auto isGraph = code > 32 && code < 127;
                const auto isPrint = code >= 32 && code < 127;
                const auto isUpper = code >= 'A' && code <= 'Z';
                const auto isLower = code >= 'a' && code <= 'z';
                const auto isXdigit = isDigit || (code >= 'A' && code <= 'F') ||
                    (code >= 'a' && code <= 'f');
                bool yes = false;
                switch (category) {
                    case 1: yes = isAlpha || isDigit; break;
                    case 2: yes = isAlpha; break;
                    case 3: yes = code == ' ' || code == 9; break;
                    case 4: yes = isCntrl; break;
                    case 5: yes = isDigit; break;
                    case 6: yes = isGraph; break;
                    case 7: yes = isLower; break;
                    case 8: yes = isPrint; break;
                    case 9: yes = isGraph && !isAlpha && !isDigit; break;
                    case 10: yes = isSpace; break;
                    case 11: yes = isUpper; break;
                    case 12: yes = isXdigit; break;
                    default: break;
                }
                regs[0] = yes ? 1u : 0u;
                return;
            }
            if (item.name == "__cxa_atexit") {
                // Register guest destructor/argument/DSO for later native
                // shutdown. This smoke probe does not unload the library.
                if (destructors_.size() >= 4096) {
                    Fault(regs[15], "cxa_atexit_overflow"); return;
                }
                destructors_.push_back({regs[0], regs[1], regs[2]});
                regs[0] = 0; // registration succeeded
                return;
            }
            if (item.name == "malloc" || item.name == "__libc_malloc" ||
                item.name == "_Znwj" || item.name == "_Znaj") {
                regs[0] = Allocate(regs[0], 16u);
                return;
            }
            if (item.name == "calloc") {
                const auto a = regs[0], b = regs[1];
                if (b && a > 0xffffffffu / b) {
                    regs[0] = 0; return;
                }
                regs[0] = Allocate(a * b, 16u);
                return;
            }
            if (item.name == "free" || item.name == "__libc_free" ||
                item.name == "_ZdlPv" || item.name == "_ZdaPv") {
                if (regs[0] && !allocations_.count(regs[0])) {
                    Fault(regs[15], "free_of_unowned_guest_pointer"); return;
                }
                // Deliberately retains released blocks until probe end.
                // This is a bounded smoke heap, not a production allocator.
                regs[0] = 0;
                return;
            }
            if (item.name == "realloc") {
                const auto pointer = regs[0], requested = regs[1];
                if (pointer && !allocations_.count(pointer)) {
                    Fault(regs[15], "realloc_of_unowned_guest_pointer"); return;
                }
                if (!requested) { regs[0] = 0; return; }
                const auto destination = Allocate(requested, 16u);
                if (destination && pointer) {
                    const auto n = std::min(allocations_[pointer], requested);
                    std::memcpy(mem_.data()+destination,mem_.data()+pointer,n);
                }
                regs[0] = destination;
                return;
            }
            if (item.name == "posix_memalign") {
                const auto out=regs[0], align=regs[1], n=regs[2];
                if (!Fits(out,4,mem_.size()) || align < 4 ||
                    (align & (align - 1)) != 0 || align > 65536u) {
                    regs[0] = 22; return; // EINVAL
                }
                const auto value = Allocate(n, align);
                if (!value) { regs[0] = 12; return; } // ENOMEM
                Put32(mem_,out,value);
                regs[0] = 0;
                return;
            }
            if (item.name == "pthread_key_create") {
                const auto out = regs[0];
                if (!Fits(out, 4, mem_.size()) || next_key_ >= 256) {
                    regs[0] = 11; return; // EAGAIN
                }
                const auto key = next_key_++;
                keys_[key] = {regs[1],0}; // guest destructor, current value
                Put32(mem_,out,key);
                regs[0] = 0;
                return;
            }
            if (item.name == "pthread_getspecific") {
                const auto it = keys_.find(regs[0]);
                regs[0] = it != keys_.end() ? it->second.second : 0u;
                return;
            }
            if (item.name == "pthread_setspecific") {
                const auto it = keys_.find(regs[0]);
                if (it == keys_.end()) { regs[0] = 22; return; }
                it->second.second = regs[1];
                regs[0] = 0;
                return;
            }
            if (item.name == "pthread_key_delete") {
                regs[0] = keys_.erase(regs[0]) ? 0u : 22u;
                return;
            }
            if (item.name == "pthread_self") {
                regs[0] = kGuestTls;
                return;
            }
            if (item.name == "glTexImage2D" || item.name == "glTexSubImage2D" ||
                item.name == "glCompressedTexImage2D" || item.name == "glCompressedTexSubImage2D" ||
                item.name == "glGenerateMipmap" || item.name == "glPixelStorei" || item.name == "glBlendFunc" ||
                item.name == "glBindTexture" || item.name == "glBindBuffer" ||
                item.name == "glEnable" || item.name == "glDisable" ||
                item.name == "glActiveTexture" || item.name == "glClear" ||
                item.name == "glCullFace" || item.name == "glFrontFace" ||
                item.name == "glDepthFunc" || item.name == "glBlendEquation" ||
                item.name == "glDepthMask" || item.name == "glClearColor" ||
                item.name == "glViewport" || item.name == "glScissor" ||
                item.name == "glColorMask" || item.name == "glTexParameteri" ||
                item.name == "glTexParameterf" || item.name == "glLineWidth" ||
                item.name == "glGenTextures" || item.name == "glDeleteTextures" ||
                item.name == "glGenBuffers" || item.name == "glDeleteBuffers" ||
                item.name == "glBufferData" || item.name == "glBufferSubData" ||
                item.name == "glBlendFuncSeparate" || item.name == "glBlendEquationSeparate" ||
                item.name == "glEnableVertexAttribArray" || item.name == "glDisableVertexAttribArray" ||
                item.name == "glGetString" || item.name == "glGetIntegerv" ||
                item.name == "glGetFloatv" || item.name == "glGetBooleanv" ||
                item.name == "glGetError" || item.name == "glCreateProgram" || item.name == "glCreateShader" || item.name == "glShaderSource" || item.name == "glCompileShader" || item.name == "glGetShaderiv" || item.name == "glAttachShader" || item.name == "glLinkProgram" || item.name == "glGetProgramiv" || item.name == "glBindAttribLocation" || item.name == "glGetUniformLocation" || item.name == "glGetAttribLocation" || item.name == "glUseProgram" || item.name == "glDeleteShader" || item.name == "glUniform1i" ||
                item.name == "glUniformMatrix4fv" || item.name == "glVertexAttribPointer" ||
                item.name == "glDrawArrays") {
                auto& hostGl = DiagnosticGles();
                static bool initialized = false;
                if (!initialized) {
                    const char* explicitDir = std::getenv("PV7_GLES_DIR");
                    const char* localApp = std::getenv("LOCALAPPDATA");
                    const std::string directory = explicitDir ? explicitDir :
                        (localApp ? std::string(localApp) + "\\Android\\Sdk\\emulator\\lib64\\gles_swiftshader" : "");
                    std::string error;
                    if (!hostGl.Initialize(directory, error)) {
                        Fault(regs[15], "GLES_CONTEXT_INIT_FAILED:" + error); return;
                    }
                    initialized = true;
                    std::cout << "HOST_GLES2_CONTEXT_READY" << "\n";
                }
                // GLES2 texture uploads. Decode all ARM32 AAPCS stack
                // arguments, including compressed formats, and validate the
                // entire pixel footprint before accessing the guest address.
                if (item.name=="glTexImage2D" || item.name=="glTexSubImage2D" ||
                    item.name=="glCompressedTexImage2D" ||
                    item.name=="glCompressedTexSubImage2D") {
                    const bool sub=item.name=="glTexSubImage2D" ||
                                   item.name=="glCompressedTexSubImage2D";
                    const bool compressed=item.name=="glCompressedTexImage2D" ||
                                          item.name=="glCompressedTexSubImage2D";
                    const auto sp=regs[13];
                    const std::size_t argc=compressed?(sub?5u:4u):5u;
                    if(!Fits(sp,argc*4,mem_.size())) {
                        Fault(regs[15],"GLES_TEXTURE_STACK_INVALID");return;
                    }
                    const auto arg=[&](std::uint32_t index)->std::uint32_t {
                        return Read<std::uint32_t>(sp+4u*index);
                    };
                    const auto target=regs[0];
                    const auto level=static_cast<std::int32_t>(regs[1]);
                    const auto width=static_cast<std::int32_t>(sub?arg(0):regs[3]);
                    const auto height=static_cast<std::int32_t>(sub?arg(1):arg(0));
                    const auto format=sub?arg(2):(compressed?regs[2]:arg(2));
                    const auto type=compressed?0u:arg(3);
                    const auto pixelPtr=compressed?(sub?arg(4):arg(3)):arg(4);
                    const auto imageSize=compressed?(sub?arg(3):arg(2)):0u;
                    const auto border=sub?0:static_cast<std::int32_t>(arg(1));
                    if(width<0 || height<0 || width>16384 || height>16384 ||
                       level<0 || level>16 || border!=0) {
                        Fault(regs[15],"GLES_TEXTURE_ARGUMENTS_INVALID");return;
                    }
                    std::uint64_t byteCount=0;
                    if(compressed) {
                        byteCount=imageSize;
                    } else {
                        unsigned bpp=0;
                        switch(type) {
                        case 0x1401u: // GL_UNSIGNED_BYTE
                            switch(format) {
                            case 0x1906u: case 0x1909u: bpp=1;break;
                            case 0x190au: bpp=2;break;
                            case 0x1907u: bpp=3;break;
                            case 0x1908u: bpp=4;break;
                            default:break;
                            }
                            break;
                        case 0x8363u: if(format==0x1907u)bpp=2;break;
                        case 0x8033u: case 0x8034u:
                            if(format==0x1908u)bpp=2;break;
                        default:break;
                        }
                        if(!bpp) {
                            Fault(regs[15],"GLES_TEXTURE_PIXEL_FORMAT_UNSUPPORTED");return;
                        }
                        const std::uint64_t row=static_cast<std::uint64_t>(width)*bpp;
                        const std::uint64_t alignment=unpack_alignment_;
                        const auto stride=(row+alignment-1)/alignment*alignment;
                        byteCount=height?stride*static_cast<std::uint64_t>(height-1)+row:0;
                    }
                    if(byteCount>64ull*1024*1024 ||
                       (pixelPtr && !Fits(pixelPtr,static_cast<std::size_t>(byteCount),mem_.size())) ||
                       (sub && byteCount && !pixelPtr) ||
                       (compressed && byteCount && !pixelPtr)) {
                        Fault(regs[15],"GLES_TEXTURE_PIXELS_INVALID");return;
                    }
                    auto fn=hostGl.GetProcedure(item.name.c_str());
                    if(!fn) {Fault(regs[15],"GLES_TEXTURE_PROCEDURE_MISSING:"+item.name);return;}
                    const void* pixels=pixelPtr?mem_.data()+pixelPtr:nullptr;
                    if(item.name=="glTexImage2D") {
                        reinterpret_cast<void (*)(unsigned,int,int,int,int,int,
                                                           unsigned,unsigned,const void*)>(fn)(
                            target,level,static_cast<int>(regs[2]),width,height,border,
                            format,type,pixels);
                    } else if(item.name=="glTexSubImage2D") {
                        reinterpret_cast<void (*)(unsigned,int,int,int,int,int,
                                                           unsigned,unsigned,const void*)>(fn)(
                            target,level,static_cast<int>(regs[2]),
                            static_cast<int>(regs[3]),width,height,format,type,pixels);
                    } else if(item.name=="glCompressedTexImage2D") {
                        reinterpret_cast<void (*)(unsigned,int,unsigned,int,int,
                                                           int,int,const void*)>(fn)(
                            target,level,regs[2],width,height,border,
                            static_cast<int>(imageSize),pixels);
                    } else {
                        reinterpret_cast<void (*)(unsigned,int,int,int,int,int,
                                                           unsigned,int,const void*)>(fn)(
                            target,level,static_cast<int>(regs[2]),
                            static_cast<int>(regs[3]),width,height,format,
                            static_cast<int>(imageSize),pixels);
                    }
                    std::cout<<"HOST_GLES_TEXTURE_UPLOAD "<<item.name<<" "
                             <<width<<"x"<<height<<" bytes="<<byteCount<<"\n";
                    return;
                }
                if(item.name=="glGenerateMipmap") {
                    auto fn=hostGl.GetProcedure("glGenerateMipmap");
                    if(!fn) {Fault(regs[15],"GLES_MIPMAP_UNAVAILABLE");return;}
                    reinterpret_cast<void (*)(unsigned)>(fn)(regs[0]);
                    return;
                }                // Core GLES2 state, texture-object and vertex/buffer calls.
                // Every operation invokes the actual SwiftShader entrypoint.
                if (item.name == "glPixelStorei" || item.name == "glBlendFunc" ||
                    item.name == "glBindTexture" || item.name == "glBindBuffer" ||
                    item.name == "glBlendEquationSeparate") {
                    using Fn = void (*)(unsigned,unsigned);
                    auto fn=reinterpret_cast<Fn>(hostGl.GetProcedure(item.name.c_str()));
                    if(!fn) {Fault(regs[15],"GLES_ENTRY_MISSING:"+item.name);return;}
                    fn(regs[0],regs[1]);
                    if(item.name=="glPixelStorei" && regs[0]==0x0cf5u) {
                        if(regs[1]!=1u && regs[1]!=2u &&
                           regs[1]!=4u && regs[1]!=8u) {
                            Fault(regs[15],"GLES_UNPACK_ALIGNMENT_INVALID");return;
                        }
                        unpack_alignment_=regs[1];
                    }
                    if(item.name=="glBindBuffer") {
                        if(regs[0]==0x8892u) array_buffer_binding_=regs[1];
                        if(regs[0]==0x8893u) element_buffer_binding_=regs[1];
                    }
                    return;
                }
                if (item.name == "glEnable" || item.name == "glDisable" ||
                    item.name == "glActiveTexture" || item.name == "glClear" ||
                    item.name == "glCullFace" || item.name == "glFrontFace" ||
                    item.name == "glDepthFunc" || item.name == "glBlendEquation" ||
                    item.name == "glEnableVertexAttribArray" ||
                    item.name == "glDisableVertexAttribArray") {
                    using Fn=void (*)(unsigned);
                    auto fn=reinterpret_cast<Fn>(hostGl.GetProcedure(item.name.c_str()));
                    if(!fn) {Fault(regs[15],"GLES_ENTRY_MISSING:"+item.name);return;}
                    fn(regs[0]);
                    return;
                }
                if (item.name == "glDepthMask") {
                    using Fn=void (*)(unsigned char);
                    auto fn=reinterpret_cast<Fn>(hostGl.GetProcedure("glDepthMask"));
                    if(!fn) {Fault(regs[15],"GLES_ENTRY_MISSING:glDepthMask");return;}
                    fn(static_cast<unsigned char>(regs[0]));
                    return;
                }
                if (item.name == "glLineWidth") {
                    using Fn=void (*)(float);
                    auto fn=reinterpret_cast<Fn>(hostGl.GetProcedure("glLineWidth"));
                    if(!fn) {Fault(regs[15],"GLES_ENTRY_MISSING:glLineWidth");return;}
                    float width;
                    std::memcpy(&width,&regs[0],4);
                    fn(width);
                    return;
                }
                if (item.name == "glClearColor") {
                    using Fn=void (*)(float,float,float,float);
                    auto fn=reinterpret_cast<Fn>(hostGl.GetProcedure("glClearColor"));
                    if(!fn) {Fault(regs[15],"GLES_ENTRY_MISSING:glClearColor");return;}
                    float v[4];
                    std::memcpy(v,regs.data(),16);
                    fn(v[0],v[1],v[2],v[3]);
                    return;
                }
                if (item.name == "glViewport" || item.name == "glScissor" ||
                    item.name == "glColorMask" || item.name == "glBlendFuncSeparate") {
                    using Fn=void (*)(unsigned,unsigned,unsigned,unsigned);
                    auto fn=reinterpret_cast<Fn>(hostGl.GetProcedure(item.name.c_str()));
                    if(!fn) {Fault(regs[15],"GLES_ENTRY_MISSING:"+item.name);return;}
                    fn(regs[0],regs[1],regs[2],regs[3]);
                    return;
                }
                if (item.name == "glTexParameteri" || item.name == "glTexParameterf") {
                    auto addr=hostGl.GetProcedure(item.name.c_str());
                    if(!addr) {Fault(regs[15],"GLES_ENTRY_MISSING:"+item.name);return;}
                    if(item.name=="glTexParameteri") {
                        reinterpret_cast<void (*)(unsigned,unsigned,int)>(addr)(
                            regs[0],regs[1],static_cast<int>(regs[2]));
                    } else {
                        float param;
                        std::memcpy(&param,&regs[2],4);
                        reinterpret_cast<void (*)(unsigned,unsigned,float)>(addr)(
                            regs[0],regs[1],param);
                    }
                    return;
                }
                if (item.name == "glGenTextures" || item.name == "glDeleteTextures" ||
                    item.name == "glGenBuffers" || item.name == "glDeleteBuffers") {
                    const auto count=regs[0],data=regs[1];
                    if(count>4096u || !Fits(data,static_cast<std::size_t>(count)*4u,mem_.size())) {
                        Fault(regs[15],"GLES_OBJECT_ARRAY_INVALID");return;
                    }
                    auto addr=hostGl.GetProcedure(item.name.c_str());
                    if(!addr) {Fault(regs[15],"GLES_ENTRY_MISSING:"+item.name);return;}
                    std::vector<unsigned> objects(count);
                    const bool generating=item.name=="glGenTextures"||item.name=="glGenBuffers";
                    if(!generating && count) std::memcpy(objects.data(),mem_.data()+data,count*4u);
                    reinterpret_cast<void (*)(int,unsigned*)>(addr)(
                        static_cast<int>(count),objects.data());
                    if(generating && count) std::memcpy(mem_.data()+data,objects.data(),count*4u);
                    std::cout << "HOST_GLES_OBJECT_ARRAY " << item.name
                              << " count=" << count << "\n";
                    return;
                }
                if (item.name == "glBufferData" || item.name == "glBufferSubData") {
                    const auto n=regs[1+(item.name=="glBufferSubData")];
                    const auto pointer=regs[2+(item.name=="glBufferSubData")];
                    if(n>64u*1024u*1024u || (pointer && !Fits(pointer,n,mem_.size()))) {
                        Fault(regs[15],"GLES_BUFFER_UPLOAD_INVALID");return;
                    }
                    const void* bytes=pointer?mem_.data()+pointer:nullptr;
                    auto addr=hostGl.GetProcedure(item.name.c_str());
                    if(!addr) {Fault(regs[15],"GLES_ENTRY_MISSING:"+item.name);return;}
                    if(item.name=="glBufferData") {
                        reinterpret_cast<void (*)(unsigned,std::intptr_t,const void*,unsigned)>(addr)(
                            regs[0],static_cast<std::intptr_t>(n),bytes,regs[3]);
                    } else {
                        reinterpret_cast<void (*)(unsigned,std::intptr_t,std::intptr_t,const void*)>(addr)(
                            regs[0],static_cast<std::intptr_t>(regs[1]),
                            static_cast<std::intptr_t>(n),bytes);
                    }
                    return;
                }                if (item.name == "glGetString") {
                    auto cache = gl_strings_.find(regs[0]);
                    if(cache!=gl_strings_.end()) {
                        regs[0]=cache->second;
                        return;
                    }
                    const auto name=regs[0];
                    const auto value=hostGl.GetString(name);
                    if(!value) {
                        // GLES returns nullptr for unsupported enums and sets
                        // its own error flag, which glGetError can query.
                        regs[0]=0;
                        return;
                    }
                    if(value->size()>65535) {
                        Fault(regs[15],"GLES_GET_STRING_TOO_LONG");return;
                    }
                    const auto guest=Allocate(static_cast<std::uint32_t>(value->size()+1),4);
                    if(!guest) {Fault(regs[15],"GLES_STRING_ALLOCATION_FAILED");return;}
                    std::memcpy(mem_.data()+guest,value->c_str(),value->size()+1);
                    gl_strings_.emplace(name,guest);
                    regs[0]=guest;
                    std::cout << "HOST_GLES_STRING enum=0x" << std::hex << name
                              << std::dec << " bytes=" << value->size() << "\n";
                    return;
                }
                if (item.name == "glGetError") {
                    const auto error=hostGl.GetError();
                    if(!error) {Fault(regs[15],"GLES_GET_ERROR_UNAVAILABLE");return;}
                    regs[0]=*error;
                    return;
                }
                if (item.name == "glGetIntegerv" ||
                    item.name == "glGetFloatv" ||
                    item.name == "glGetBooleanv") {
                    const auto pname=regs[0], destination=regs[1];
                    // GLES2 array-valued state is bounded by its enum.
                    // Reject variable-length results until their actual
                    // count is known; do not overwrite guest stack memory.
                    if (pname==0x86a3u) {
                        Fault(regs[15],"GLES_COMPRESSED_FORMAT_ARRAY_NEEDS_LENGTH");
                        return;
                    }
                    const std::size_t count =
                        pname==0x0ba2u || pname==0x0c10u ? 4u :
                        pname==0x0d3au || pname==0x846du ||
                        pname==0x846eu || pname==0x0b70u ||
                        pname==0x0b22u ? 2u : 1u;
                    const std::size_t byteWidth =
                        item.name=="glGetBooleanv" ? 1u : 4u;
                    if(!Fits(destination,count*byteWidth,mem_.size())) {
                        Fault(regs[15],"GLES_QUERY_DESTINATION_INVALID");return;
                    }
                    if(item.name=="glGetIntegerv") {
                        int values[16]{};
                        if(!hostGl.GetIntegerv(pname,values)) {
                            Fault(regs[15],"GLES_GET_INTEGER_UNAVAILABLE");return;
                        }
                        std::memcpy(mem_.data()+destination,values,count*4u);
                    } else if(item.name=="glGetFloatv") {
                        float values[16]{};
                        if(!hostGl.GetFloatv(pname,values)) {
                            Fault(regs[15],"GLES_GET_FLOAT_UNAVAILABLE");return;
                        }
                        std::memcpy(mem_.data()+destination,values,count*4u);
                    } else {
                        unsigned char values[16]{};
                        if(!hostGl.GetBooleanv(pname,values)) {
                            Fault(regs[15],"GLES_GET_BOOLEAN_UNAVAILABLE");return;
                        }
                        std::memcpy(mem_.data()+destination,values,count);
                    }
                    std::cout << "HOST_GLES_QUERY method=" << item.name
                              << " enum=0x" << std::hex << pname << std::dec
                              << " components=" << count << "\n";
                    return;
                }                if (item.name == "glShaderSource") {
                    const auto shader = regs[0], count = regs[1], array = regs[2], lengthArray = regs[3];
                    if (count == 0 || count > 16 || !Fits(array, count * 4u, mem_.size()) ||
                        (lengthArray && !Fits(lengthArray, count * 4u, mem_.size()))) {
                        Fault(regs[15], "GLES_SHADER_SOURCE_BAD_ARGUMENTS"); return;
                    }
                    std::vector<std::string> chunks;
                    std::size_t total = 0;
                    for (std::uint32_t i=0; i<count; ++i) {
                        const auto guest = Read<std::uint32_t>(array + i*4u);
                        if (!guest || guest >= mem_.size()) {
                            Fault(regs[15], "GLES_SHADER_SOURCE_BAD_POINTER"); return;
                        }
                        std::size_t size = 0;
                        if (lengthArray) {
                            const auto specified = Read<std::int32_t>(lengthArray + i*4u);
                            if (specified >= 0) {
                                size = static_cast<std::size_t>(specified);
                                if (size > 1048576u || !Fits(guest,size,mem_.size())) {
                                    Fault(regs[15], "GLES_SHADER_SOURCE_BAD_LENGTH"); return;
                                }
                            } else {
                                const auto* start = mem_.data()+guest;
                                const auto* end = static_cast<const std::uint8_t*>(
                                    std::memchr(start,0,std::min<std::size_t>(1048576u,mem_.size()-guest)));
                                if (!end) { Fault(regs[15], "GLES_SHADER_SOURCE_UNTERMINATED"); return; }
                                size = static_cast<std::size_t>(end-start);
                            }
                        } else {
                            const auto* start = mem_.data()+guest;
                            const auto* end = static_cast<const std::uint8_t*>(
                                std::memchr(start,0,std::min<std::size_t>(1048576u,mem_.size()-guest)));
                            if (!end) { Fault(regs[15], "GLES_SHADER_SOURCE_UNTERMINATED"); return; }
                            size = static_cast<std::size_t>(end-start);
                        }
                        total += size;
                        if (total > 1048576u) { Fault(regs[15], "GLES_SHADER_SOURCE_TOO_LARGE"); return; }
                        chunks.emplace_back(reinterpret_cast<const char*>(mem_.data()+guest),size);
                    }
                    hostGl.ShaderSource(shader,chunks);
                    std::cout << "HOST_GLES_SHADER_SOURCE shader=" << shader << " chunks=" << count
                              << " bytes=" << total << "\n";
                    return;
                }
                if (item.name == "glCompileShader") {
                    hostGl.CompileShader(regs[0]);
                    std::cout << "HOST_GLES_COMPILE shader=" << regs[0] << "\n";
                    return;
                }
                if (item.name == "glGetShaderiv") {
                    if (!Fits(regs[2],4,mem_.size())) {
                        Fault(regs[15],"GLES_GET_SHADER_IV_BAD_POINTER"); return;
                    }
                    const auto value=hostGl.GetShaderParameter(regs[0],regs[1]);
                    Put32(mem_,regs[2],static_cast<std::uint32_t>(value));
                    std::cout << "HOST_GLES_SHADER_PARAM shader=" << regs[0]
                              << " pname=0x" << std::hex << regs[1] << std::dec
                              << " value=" << value << "\n";
                    return;
                }                if (item.name == "glAttachShader") {
                    hostGl.AttachShader(regs[0],regs[1]);
                    std::cout << "HOST_GLES_ATTACH program=" << regs[0]
                              << " shader=" << regs[1] << "\n";
                    return;
                }
                if (item.name == "glLinkProgram") {
                    hostGl.LinkProgram(regs[0]);
                    std::cout << "HOST_GLES_LINK program=" << regs[0] << " success=" << hostGl.GetProgramParameter(regs[0],0x8b82u) << "\n";
                    return;
                }
                if (item.name == "glGetProgramiv") {
                    if (!Fits(regs[2],4,mem_.size())) {
                        Fault(regs[15],"GLES_GET_PROGRAM_IV_BAD_POINTER"); return;
                    }
                    const auto value=hostGl.GetProgramParameter(regs[0],regs[1]);
                    Put32(mem_,regs[2],static_cast<std::uint32_t>(value));
                    std::cout << "HOST_GLES_PROGRAM_PARAM program=" << regs[0]
                              << " pname=0x" << std::hex << regs[1] << std::dec
                              << " value=" << value << "\n";
                    return;
                }                if (item.name == "glBindAttribLocation" || item.name == "glGetUniformLocation" ||
                    item.name == "glGetAttribLocation") {
                    const auto guestName = item.name == "glBindAttribLocation" ? regs[2] : regs[1];
                    if (!Fits(guestName,1,mem_.size())) {
                        Fault(regs[15],"GLES_ATTRIBUTE_NAME_BAD_POINTER"); return;
                    }
                    const auto name = CString(mem_,guestName);
                    if (name.empty() || name.size() == 128u) {
                        Fault(regs[15],"GLES_ATTRIBUTE_NAME_INVALID"); return;
                    }
                    if (item.name == "glBindAttribLocation")
                        hostGl.BindAttrib(regs[0],regs[1],name);
                    else if (item.name == "glGetUniformLocation")
                        regs[0] = static_cast<std::uint32_t>(hostGl.GetUniform(regs[0],name));
                    else
                        regs[0] = static_cast<std::uint32_t>(hostGl.GetAttrib(regs[0],name));
                    std::cout << "HOST_GLES_NAMED " << item.name << " name=" << name << "\n";
                    return;
                }
                if (item.name == "glUseProgram") {
                    hostGl.UseProgram(regs[0]);
                    std::cout << "HOST_GLES_USE_PROGRAM id=" << regs[0] << "\n";
                    return;
                }                if (item.name == "glDeleteShader") {
                    hostGl.DeleteShader(regs[0]);
                    std::cout << "HOST_GLES_DELETE_SHADER id=" << regs[0] << "\n";
                    return;
                }
                if (item.name=="glDrawArrays") {
                    const auto mode=regs[0];
                    const auto first=static_cast<std::int32_t>(regs[1]);
                    const auto count=static_cast<std::int32_t>(regs[2]);
                    if (first<0 || count<0 || count>1000000 ||
                        (mode!=GL_TRIANGLES && mode!=GL_TRIANGLE_STRIP &&
                         mode!=GL_TRIANGLE_FAN && mode!=GL_LINES &&
                         mode!=GL_LINE_STRIP && mode!=GL_POINTS)) {
                        Fault(regs[15],"GLES_DRAW_ARRAYS_INVALID_ARGUMENTS");return;
                    }
                    hostGl.DrawArrays(mode,first,count);
                    const auto glError=hostGl.GetError();
                    if(!glError || *glError!=GL_NO_ERROR) {
                        Fault(regs[15],"GLES_DRAW_ARRAYS_GL_ERROR");return;
                    }
                    ++draw_calls_;
                    std::cout<<"HOST_GLES_DRAW_ARRAYS_OK mode=0x"<<std::hex<<mode
                             <<std::dec<<" first="<<first<<" count="<<count
                             <<" draw_calls="<<draw_calls_<<"\n";
                    return;
                }
                if (item.name=="glVertexAttribPointer") {
                    const auto stack=regs[13];
                    if (!Fits(stack,8u,mem_.size())) {
                        Fault(regs[15],"GLES_VERTEX_ATTRIBUTE_STACK_INVALID");return;
                    }
                    const auto index=regs[0];
                    const auto components=static_cast<std::int32_t>(regs[1]);
                    const auto type=regs[2];
                    const auto normalized=regs[3];
                    const auto stride=static_cast<std::int32_t>(Read<std::uint32_t>(stack));
                    const auto guestPointer=Read<std::uint32_t>(stack+4u);
                    unsigned componentBytes=0;
                    if (type==GL_FLOAT || type==GL_FIXED) componentBytes=4;
                    else if (type==GL_SHORT || type==GL_UNSIGNED_SHORT) componentBytes=2;
                    else if (type==GL_BYTE || type==GL_UNSIGNED_BYTE) componentBytes=1;
                    if (index>=16 || components<1 || components>4 || !componentBytes ||
                        normalized>1u || stride<0 || stride>65536) {
                        Fault(regs[15],"GLES_VERTEX_ATTRIBUTE_INVALID_PARAMETERS");return;
                    }
                    int boundBuffer=0;
                    if(!hostGl.GetIntegerv(GL_ARRAY_BUFFER_BINDING,&boundBuffer)) {
                        Fault(regs[15],"GLES_VERTEX_ATTRIBUTE_BINDING_UNAVAILABLE");return;
                    }
                    const void* pointer=nullptr;
                    if (boundBuffer) {
                        pointer=reinterpret_cast<const void*>(static_cast<std::uintptr_t>(guestPointer));
                    } else {
                        const auto required=static_cast<std::size_t>(components)*componentBytes;
                        if (!Fits(guestPointer,required,mem_.size())) {
                            Fault(regs[15],"GLES_VERTEX_ATTRIBUTE_GUEST_MEMORY_INVALID");return;
                        }
                        pointer=mem_.data()+guestPointer;
                    }
                    hostGl.VertexAttribPointer(index,components,type,normalized!=0u,
                                               stride,pointer);
                    std::cout<<"HOST_GLES_VERTEX_ATTRIB_POINTER index="<<index
                             <<" size="<<components<<" type=0x"<<std::hex<<type
                             <<std::dec<<" buffer="<<boundBuffer
                             <<" guest_ptr=0x"<<std::hex<<guestPointer<<std::dec<<"\n";
                    return;
                }
                if(item.name=="glUniformMatrix4fv") {
                    const auto location=static_cast<std::int32_t>(regs[0]);
                    const auto count=static_cast<std::int32_t>(regs[1]);
                    const auto ptr=regs[3];
                    if(count<0 || count>32 || regs[2]!=0 || (ptr&3u) ||
                       (count && !Fits(ptr,static_cast<std::size_t>(count)*64u,mem_.size()))) {
                        Fault(regs[15],"GLES_UNIFORM_MATRIX_INVALID_GUEST_RANGE");return;
                    }
                    hostGl.UniformMatrix4fv(location,count,false,
                        count ? reinterpret_cast<const float*>(mem_.data()+ptr) : nullptr);
                    std::cout<<"HOST_GLES_UNIFORM_MATRIX4FV location="<<location
                             <<" count="<<count<<"\n";
                    return;
                }
                if (item.name == "glUniform1i") {
                    hostGl.Uniform1i(static_cast<int>(regs[0]),static_cast<int>(regs[1]));
                    std::cout << "HOST_GLES_UNIFORM1I location=" << static_cast<int>(regs[0])
                              << " value=" << static_cast<int>(regs[1]) << "\n";
                    return;
                }                regs[0] = item.name == "glCreateProgram" ? hostGl.CreateProgram() : hostGl.CreateShader(regs[0]);
                if (!regs[0]) { Fault(regs[15], "GLES_OBJECT_CREATION_FAILED:" + item.name); return; }
                std::cout << "HOST_GLES_OBJECT " << item.name << " id=" << regs[0] << "\n";
                return;
            }            reason = "UNIMPLEMENTED_IMPORT:" + item.name;
            std::cout << "IMPORT_TRAP " << item.name
                      << " pc=0x" << std::hex << regs[15]
                      << " lr=0x" << regs[14]
                      << " r0=0x" << regs[0] << " r1=0x" << regs[1]
                      << " r2=0x" << regs[2] << " r3=0x" << regs[3]
                      << std::dec << "\n";
            Fault(regs[15], reason);
            return;
        }
        Fault(regs[15], "unhandled_svc:" + std::to_string(svc));
    }
private:
    template<typename T> T Read(std::uint32_t a) {
        T v{};
        if (!Fits(a, sizeof(T), mem_.size())) { Fault(a, "read_out_of_range"); return v; }
        std::memcpy(&v, mem_.data() + a, sizeof(T));
        return v;
    }
    template<typename T> void Write(std::uint32_t a, T v) {
        if (!Fits(a, sizeof(T), mem_.size())) { Fault(a, "write_out_of_range"); return; }
        std::memcpy(mem_.data() + a, &v, sizeof(T));
    }
    void Fault(std::uint32_t address, std::string why) {
        if (faulted) return;
        faulted = true;
        fault_address = address;
        reason = std::move(why);
        if (jit) jit->HaltExecution();
    }
    std::uint32_t Allocate(std::uint32_t size, std::uint32_t alignment) {
        if (!size) size=1;
        if (size > 32u * 1024 * 1024) return 0;
        const std::uint64_t start =
            (static_cast<std::uint64_t>(next_heap_) + alignment - 1) &
            ~static_cast<std::uint64_t>(alignment - 1);
        const std::uint64_t end = start + size;
        if (end > kHeapEnd || end > mem_.size()) return 0;
        next_heap_ = static_cast<std::uint32_t>(end);
        const auto address = static_cast<std::uint32_t>(start);
        allocations_[address] = size;
        return address;
    }
    std::uint32_t next_heap_ = kHeapStart;
    std::map<std::uint32_t, std::uint32_t> allocations_;
    pv7::compat::cocos2dx::GuestJniByteArrays byte_arrays_;
    pv7::compat::cocos2dx::GuestJniRefs refs_;
    std::uint32_t next_jstring_ = 0x0000c100;
    std::map<std::uint32_t, std::string> guest_strings_;
    std::map<std::uint32_t,std::uint32_t> guest_string_borrows_;
#if defined(PV7_COCOS_JNI_RUNNER)
    std::map<std::uint32_t,std::FILE*> open_files_;
    std::map<std::uint32_t,z_stream> z_streams_;
    std::uint32_t next_file_handle_=0x0000d100u;
#endif
    struct GuestJumpCheckpoint {
        std::array<std::uint32_t,16> regs{};
        std::uint32_t cpsr=0;
    };
    std::map<std::uint32_t,GuestJumpCheckpoint> guest_jump_buffers_;
    std::map<std::uint32_t, std::uint32_t> gl_strings_;
    std::uint32_t unpack_alignment_ = 4;
    std::uint32_t array_buffer_binding_ = 0, element_buffer_binding_ = 0;
    std::uint32_t next_key_ = 1;
    std::map<std::uint32_t, std::pair<std::uint32_t, std::uint32_t>> keys_;
    std::vector<std::uint8_t>& mem_;
    const std::vector<Import>& imports_;
    struct Destructor { std::uint32_t function, arg, dso; };
    std::vector<Destructor> destructors_;
    unsigned log_count_ = 0;
};
} // namespace

// A bounded readback proves pixels reached the Android EGL framebuffer, not
// merely that the guest issued glDrawArrays. PPMs stay in app-private storage.
static bool CaptureCocosFrame(unsigned frame) {
    constexpr int width = 480, height = 320;
    std::vector<std::uint8_t> rgba;
    std::string error;
    if (!DiagnosticGles().ReadFramebuffer(rgba, error)) {
        std::cout << "COCOS_FRAME_READBACK_FAILED frame=" << frame
                  << " reason=" << error << "\n";
        return false;
    }
    std::uint64_t hash = 14695981039346656037ull;
    std::size_t nonblack = 0, differs_from_first = 0, translucent = 0;
    for (std::size_t i = 0; i < rgba.size(); i += 4u) {
        if (rgba[i] || rgba[i+1] || rgba[i+2]) ++nonblack;
        if (rgba[i] != rgba[0] || rgba[i+1] != rgba[1] ||
            rgba[i+2] != rgba[2]) ++differs_from_first;
        if (rgba[i+3] != 255u) ++translucent;
        for (unsigned c = 0; c < 4; ++c) {
            hash ^= rgba[i+c];
            hash *= 1099511628211ull;
        }
    }
    std::cout << "COCOS_FRAME_READBACK_OK frame=" << frame
              << " size=" << width << "x" << height
              << " nonblack=" << nonblack
              << " color_variation=" << differs_from_first
              << " nonopaque=" << translucent
              << " fnv64=0x" << std::hex << hash << std::dec << "\n";
#if defined(PV7_COCOS_JNI_RUNNER)
    if (!g_pv7_cocos_capture_path.empty()) {
        const std::string path = g_pv7_cocos_capture_path +
                                 ".frame-" + std::to_string(frame) + ".ppm";
        std::ofstream out(path, std::ios::binary | std::ios::trunc);
        if (!out) {
            std::cout << "COCOS_FRAME_SAVE_FAILED frame=" << frame << "\n";
            return false;
        }
        out << "P6\n" << width << " " << height << "\n255\n";
        // GLES readback has the bottom row first; PPM stores the top row first.
        for (int y = height - 1; y >= 0; --y) {
            for (int x = 0; x < width; ++x) {
                const auto pixel = (static_cast<std::size_t>(y) * width + x) * 4u;
                out.write(reinterpret_cast<const char*>(rgba.data() + pixel), 3);
            }
        }
        out.close();
        if (!out) {
            std::cout << "COCOS_FRAME_SAVE_FAILED frame=" << frame << "\n";
            return false;
        }
        std::cout << "COCOS_FRAME_SAVED frame=" << frame
                  << " path=" << path << "\n";
    }
#endif
    return true;
}

int RunCocosProbe(int argc, char** argv) {
    if (argc != 2 && argc != 3) {
        std::cerr << "Usage: pv7-cocos-firstcall libgame.so [constructors|JNI_OnLoad|boot|nativeRender|version]\n";
        return 2;
    }
    const std::string requested = argc == 3 ? argv[2] : "JNI_OnLoad";
    const char* entry_name = nullptr;
    if (requested == "JNI_OnLoad") entry_name = "JNI_OnLoad";
    else if (requested == "nativeRender") entry_name = "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender";
    else if (requested == "version") entry_name = "_ZN7cocos2d14cocos2dVersionEv";
    else if (requested == "constructors" || requested == "boot") entry_name = "JNI_OnLoad";
    else { std::cerr << "Unknown entrypoint\n"; return 2; }

    std::ifstream source(argv[1], std::ios::binary);
    if (!source) { std::cerr << "Cannot read ELF file\n"; return 2; }
    std::vector<std::uint8_t> elf(std::istreambuf_iterator<char>{source}, {});
    pv7::Arm32GuestImage image;
    std::string err;
    if (!image.Load(elf, kImageBase, err)) {
        std::cerr << "LOAD_FAILED " << err << "\n"; return 2;
    }
    const auto entry = image.FindExport(entry_name);
    if (!entry) { std::cerr << "ENTRY_MISSING " << entry_name << "\n"; return 2; }
    if (kImageBase + image.span() >= kImportsBase) {
        std::cerr << "IMAGE_OVERLAPS_THUNKS\n"; return 2;
    }

    // Import records are discovered during Relocate, not Load.
    // The relocation walker appends the current typed import before
    // invoking our resolver. Use that exact record for STT_OBJECT.
    std::map<std::string, std::uint32_t> addresses;
    std::vector<Import> imports;
    unsigned data_count=0;
    auto resolver = [&](std::string_view value) -> std::optional<std::uint32_t> {
        const std::string name(value);
        if (auto it = addresses.find(name); it != addresses.end()) return it->second;
        const auto& pending = image.imported_symbols();
        const bool is_data = !pending.empty() &&
            pending.back().name == name && pending.back().symbol_type == 1;
        if (imports.size() >= 4096) return std::nullopt;
        const std::uint32_t addr = name == "pthread_once" ? kPthreadOnce
            : is_data ? kObjectsBase + data_count++ * 1024u
            : kImportsBase + static_cast<std::uint32_t>(imports.size()) * 16u;
        addresses[name] = addr;
        imports.push_back({name, addr, is_data});
        return addr;
    };
    if (!image.Relocate(resolver, err)) {
        std::cerr << "RELOCATE_FAILED " << err << "\n"; return 2;
    }

    std::vector<std::uint8_t> memory(kMemorySize, 0);
    if (!image.CopyToGuestMemory(memory, err)) {
        std::cerr << "MAP_FAILED " << err << "\n"; return 2;
    }
    for (std::size_t i = 0; i < imports.size(); ++i) {
        if (!imports[i].data && imports[i].name != "pthread_once" &&
            !Trap(memory, imports[i].address, kImportSvcBase + static_cast<std::uint32_t>(i))) {
            std::cerr << "THUNK_FAILED " << imports[i].name << "\n"; return 2;
        }
    }
    unsigned object_count = 0;
    for (const auto& entry : imports) {
        if (!entry.data) continue;
        ++object_count;
        if (entry.name == "__stack_chk_guard")
            Put32(memory, entry.address, 0x5a17ccf0u);
        else if (entry.name == "optind")
            Put32(memory, entry.address, 1u);
        else if (entry.name == "_ctype_" ||
                 entry.name == "_tolower_tab_" ||
                 entry.name == "_toupper_tab_") {
            const auto table = entry.address + 16u;
            Put32(memory, entry.address, table);
            if (entry.name == "_ctype_") {
                // Android Bionic legacy ctype.h: (_ctype_ + 1)[ch]
                // _U=1, _L=2, _N=4, _S=8, _P=16, _C=32,
                // _X=64, _B=128. Raw ASCII values are NOT flags.
                memory[table] = 0; // EOF slot
                for (std::uint32_t c=0;c<256;++c) {
                    std::uint8_t flags=0;
                    const bool upper=c>='A'&&c<='Z';
                    const bool lower=c>='a'&&c<='z';
                    const bool digit=c>='0'&&c<='9';
                    const bool whitespace=c==' '||(c>=9&&c<=13);
                    if(upper)flags|=0x01;
                    if(lower)flags|=0x02;
                    if(digit)flags|=0x04;
                    if(whitespace)flags|=0x08;
                    if(c>=33&&c<=126&&!upper&&!lower&&!digit)flags|=0x10;
                    if(c<32||c==127)flags|=0x20;
                    if((c>='A'&&c<='F')||(c>='a'&&c<='f'))flags|=0x40;
                    if(c==' '||c==9)flags|=0x80;
                    memory[table+c+1u]=flags;
                }
            } else {
                // Bionic conversion tables are SHORT arrays, not byte maps.
                const std::int16_t eof=-1;
                std::memcpy(memory.data()+table,&eof,sizeof(eof));
                for (std::uint32_t c=0;c<256;++c) {
                    std::int16_t value=static_cast<std::int16_t>(c);
                    if (entry.name=="_tolower_tab_"&&c>='A'&&c<='Z')
                        value=static_cast<std::int16_t>(c+('a'-'A'));
                    else if(entry.name=="_toupper_tab_"&&c>='a'&&c<='z')
                        value=static_cast<std::int16_t>(c-('a'-'A'));
                    std::memcpy(memory.data()+table+2u*(c+1u),
                                &value,sizeof(value));
                }
            }
        }
    }
    std::cout << "TYPED_DATA_IMPORTS count=" << object_count << "\n";
    // One-time initialization is vital: actually call the guest initializer
    // via BLX instead of lying to Cocos that initialization succeeded.
    // This simple ARM-state stub is *single-threaded diagnostic only*;
    // a production pthread_once bridge needs synchronized contention.
    constexpr std::uint32_t onceCode[] = {
        0xe92d4010u, // push {r4,lr}
        0xe1a04000u, // mov r4,r0 (pthread_once control pointer)
        0xe5940000u, // ldr r0,[r4]
        0xe3500000u, // cmp r0,#0
        0x1a000004u, // bne done
        0xe3a00001u, // mov r0,#1 (in progress)
        0xe5840000u, // str r0,[r4]
        0xe12fff31u, // blx r1 (run guest initializer)
        0xe3a00002u, // mov r0,#2 (done)
        0xe5840000u, // str r0,[r4]
        0xe3a00000u, // mov r0,#0
        0xe8bd8010u  // pop {r4,pc}
    };
    for (std::size_t i=0;i<std::size(onceCode);++i) {
        if (!Put32(memory, kPthreadOnce + static_cast<std::uint32_t>(4*i), onceCode[i])) {
            std::cerr << "PTHREAD_ONCE_THUNK_FAILED\n"; return 2;
        }
    }
    // Populate every JNI function table slot with a distinct missing-method
    // trap. An unimplemented method is reported by index, not an invalid PC.
    for (std::uint32_t i=0;i<233;++i) {
        const std::uint32_t address = kMissingJniBase + i * 8;
        if (!Trap(memory, address, kMissingJniSvc + i) ||
            !Put32(memory, kEnvTable + i * 4, address)) {
            std::cerr << "JNI_TABLE_SETUP_FAILED\n";return 2;
        }
    }
    // Minimal invocation interface scaffolding. Unknown JNI operations halt.
    if (!Trap(memory, kReturnTrap, 0) ||
        !Trap(memory, kGetEnvStub, kJniGetEnv) ||
        !Trap(memory, kAttachStub, kJniAttach) ||
        !Trap(memory, kFindClassStub, kJniFindClass) ||
        !Trap(memory, kRegisterStub, kJniRegister) ||
        !Trap(memory, kGetVersionStub, kJniGetVersion) ||
        !Put32(memory, kVM, kVMTable) ||
        !Put32(memory, kVMTable + 4 * 4, kAttachStub) ||
        !Put32(memory, kVMTable + 6 * 4, kGetEnvStub) ||
        !Put32(memory, kEnv, kEnvTable) ||
        !Put32(memory, kEnvTable + 6 * 4, kFindClassStub) ||
        !Put32(memory, kEnvTable + 215 * 4, kRegisterStub) ||
        !Put32(memory, kEnvTable + 4 * 4, kGetVersionStub)) {
        std::cerr << "JNI_SCAFFOLD_FAILED\n"; return 2;
    }
    Bridge bridge(memory, imports);
    Dynarmic::A32::UserConfig conf;
    conf.callbacks = &bridge;
    conf.arch_version = Dynarmic::A32::ArchVersion::v7;
    conf.always_little_endian = true;
    conf.enable_cycle_counting = true;
    conf.code_cache_size = 16 * 1024 * 1024;
    Dynarmic::A32::Jit jit(conf);
    bridge.jit = &jit;
    if (requested == "constructors" || requested == "boot") {
        std::size_t n = 0;
        for (const auto address : image.constructors()) {
            bridge.returned = false;
            bridge.faulted = false;
            bridge.reason.clear();
            bridge.ticks_remaining = 200000;
            jit.ClearHalt();
            jit.Regs().fill(0);
            jit.Regs()[13] = kStack;
            jit.Regs()[14] = kReturnTrap;
            jit.Regs()[15] = address & ~1u;
            jit.SetCpsr((address & 1u) ? 0x20u : 0u);
            jit.Run();
            if (!bridge.returned || bridge.faulted) {
                std::cout << "CONSTRUCTOR_STOP index=" << n
                          << " address=0x" << std::hex << address
                          << " pc=0x" << jit.Regs()[15] << std::dec
                          << " reason=" << (bridge.faulted ? bridge.reason : "instruction_budget")
                          << "\n";
                return 3;
            }
            ++n;
            if (n <= 4 || n % 25 == 0)
                std::cout << "CONSTRUCTOR_OK count=" << n << "\n";
        }
        std::cout << "ALL_CONSTRUCTORS_RETURNED count=" << n << "\n";
        if (requested == "constructors") return 0;
    }
    bridge.returned = false;
    bridge.faulted = false;
    bridge.reason.clear();
    bridge.ticks_remaining = 1000000;
    jit.ClearHalt();
    jit.Regs().fill(0);
    jit.Regs()[0] = kVM; // first argument of JNI_OnLoad
    jit.Regs()[13] = kStack;
    jit.Regs()[14] = kReturnTrap;
    jit.Regs()[15] = *entry & ~1u;
    jit.SetCpsr((*entry & 1u) ? 0x20u : 0u);
    std::cout << "EXECUTING_REAL_ARM32 entry=" << entry_name
              << " pc=0x" << std::hex << *entry << std::dec
              << " imports=" << imports.size()
              << " image=" << image.span()
              << " constructors_not_run="
              << (requested == "boot" ? 0u : image.constructors().size())
              << "\n";
    jit.Run();
    if (bridge.faulted) {
        std::cout << "GUEST_STOP " << bridge.reason
                  << " fault_address=0x" << std::hex << bridge.fault_address << std::dec
                  << "\n";
        return 3; // expected actionable missing service, not success
    }
    if (bridge.returned) {
        const auto version = jit.Regs()[0];
        std::cout << "GUEST_RETURN r0=0x" << std::hex << version
                  << std::dec << " (JNI entry returned)\n";
        if (requested == "version") {
            const auto text = CString(memory, version);
            if (text.rfind("cocos2d", 0) != 0) {
                std::cout << "COCOS_VERSION_UNAVAILABLE pointer=0x" << std::hex << version << std::dec << " raw=" << text << "\n";
                return 3;
            }
            std::cout << "COCOS_VERSION_DETECTED " << text << "\n";
            return 0;
        }
        if (requested != "boot") return 0;
#if defined(PV7_COCOS_JNI_RUNNER)
        // Run the unmodified guest TinyXML2 prefix comparator on real A32.
        // This identifies CPU/condition-code errors separately from parser state.
        const auto equal=image.FindExport("_ZN8tinyxml27XMLUtil11StringEqualEPKcS2_i");
        if (!equal) {
            std::cout<<"TINYXML2_STRINGEQUAL_EXPORT_MISSING\n";return 3;
        }
        const auto compare=[&](const char* left,const char* right,
                               std::uint32_t count,bool expected)->bool {
            constexpr std::uint32_t leftAddr=0x0000a000u, rightAddr=0x0000a100u;
            const auto leftSize=std::strlen(left)+1u, rightSize=std::strlen(right)+1u;
            if(!Fits(leftAddr,leftSize,memory.size())||
               !Fits(rightAddr,rightSize,memory.size())||
               leftSize>256u || rightSize>256u)return false;
            std::memcpy(memory.data()+leftAddr,left,leftSize);
            std::memcpy(memory.data()+rightAddr,right,rightSize);
            bridge.returned=false;
            bridge.faulted=false;
            bridge.reason.clear();
            bridge.ticks_remaining=100000;
            jit.ClearHalt();
            jit.Regs().fill(0);
            jit.Regs()[0]=leftAddr;
            jit.Regs()[1]=rightAddr;
            jit.Regs()[2]=count;
            jit.Regs()[13]=kStack;
            jit.Regs()[14]=kReturnTrap;
            jit.Regs()[15]=*equal&~1u;
            jit.SetCpsr((*equal&1u)?0x20u:0u);
            jit.Run();
            const auto result=jit.Regs()[0]!=0;
            std::cout<<"TINYXML2_PREFIX_COMPARISON lhs="<<left<<" rhs="<<right
                     <<" n="<<count<<" value="<<result
                     <<" expected="<<expected
                     <<" returned="<<bridge.returned
                     <<" fault="<<bridge.reason<<"\n";
            return bridge.returned&&!bridge.faulted&&result==expected;
        };
        if(!compare("<?xml","<?",2,true)||
           !compare("<?xml","<!",2,false)||
           !compare("<plist","<",1,true)||
           !compare("abc","abd",3,false)) {
            std::cout<<"TINYXML2_PREFIX_SELFTEST_FAILED\n";return 3;
        }
        std::cout<<"TINYXML2_PREFIX_SELFTEST_PASSED\n";
        std::cout<<"TINYXML2_ROM_LT_LITERAL="<<CString(memory,kImageBase+0x46c961u)<<" byte="<<static_cast<unsigned>(memory[kImageBase+0x46c961u])<<"\n";
        const auto doc_ctor=image.FindExport("_ZN8tinyxml211XMLDocumentC1EbNS_10WhitespaceE");
        const auto doc_identify=image.FindExport("_ZN8tinyxml211XMLDocument8IdentifyEPcPPNS_7XMLNodeE");
        constexpr std::uint32_t xmlDocAddr=0x0000d000u;
        constexpr std::uint32_t xmlTextAddr=0x0000d900u;
        constexpr std::uint32_t xmlNodeOutAddr=0x0000dc00u;
        if (doc_ctor && doc_identify) {
            std::memset(memory.data()+xmlDocAddr,0,2048u);
            bridge.returned=false;
            bridge.faulted=false;
            bridge.reason.clear();
            bridge.ticks_remaining=1000000;
            jit.ClearHalt();
            jit.Regs().fill(0);
            jit.Regs()[0]=xmlDocAddr;
            jit.Regs()[1]=1u;
            jit.Regs()[2]=0u;
            jit.Regs()[13]=kStack;
            jit.Regs()[14]=kReturnTrap;
            jit.Regs()[15]=*doc_ctor&~1u;
            jit.SetCpsr((*doc_ctor&1u)?0x20u:0u);
            jit.Run();
            std::cout<<"TINYXML2_DOCUMENT_CTOR returned="<<bridge.returned
                     <<" fault="<<bridge.reason<<"\n";
            if (bridge.returned&&!bridge.faulted) {
                constexpr const char* probeText="<plist>";
                std::memcpy(memory.data()+xmlTextAddr,probeText,8u);
                Put32(memory,xmlNodeOutAddr,0);
                bridge.returned=false;
                bridge.faulted=false;
                bridge.reason.clear();
                bridge.ticks_remaining=1000000;
                jit.ClearHalt();
                jit.Regs().fill(0);
                jit.Regs()[0]=xmlDocAddr;
                jit.Regs()[1]=xmlTextAddr;
                jit.Regs()[2]=xmlNodeOutAddr;
                jit.Regs()[13]=kStack;
                jit.Regs()[14]=kReturnTrap;
                jit.Regs()[15]=*doc_identify&~1u;
                jit.SetCpsr((*doc_identify&1u)?0x20u:0u);
                jit.Run();
                std::uint32_t node=0;
                std::memcpy(&node,memory.data()+xmlNodeOutAddr,4u);
                std::cout<<"TINYXML2_DIRECT_IDENTIFY cursor_delta="
                         <<static_cast<std::int64_t>(jit.Regs()[0])-xmlTextAddr
                         <<" node=0x"<<std::hex<<node<<std::dec
                         <<" returned="<<bridge.returned
                         <<" fault="<<bridge.reason<<"\n";
                bridge.returned=false;
                bridge.faulted=false;
                bridge.reason.clear();
                bridge.ticks_remaining=100000u;
                jit.ClearHalt();
                jit.Regs().fill(0);
                jit.Regs()[0]=xmlTextAddr;
                jit.Regs()[1]=kImageBase+0x46c961u;
                jit.Regs()[2]=1u;
                jit.Regs()[13]=kStack;
                jit.Regs()[14]=kReturnTrap;
                jit.Regs()[15]=*equal&~1u;
                jit.SetCpsr((*equal&1u)?0x20u:0u);
                jit.Run();
                std::cout<<"TINYXML2_ROM_COMPARE_DIRECT result="<<jit.Regs()[0]
                         <<" returned="<<bridge.returned
                         <<" fault="<<bridge.reason<<"\n";
                const auto skip=image.FindExport("_ZN8tinyxml27XMLUtil14SkipWhiteSpaceEPc");
                if (skip) {
                    bridge.returned=false;
                    bridge.faulted=false;
                    bridge.reason.clear();
                    bridge.ticks_remaining=100000u;
                    jit.ClearHalt();
                    jit.Regs().fill(0);
                    jit.Regs()[0]=xmlTextAddr;
                    jit.Regs()[13]=kStack;
                    jit.Regs()[14]=kReturnTrap;
                    jit.Regs()[15]=*skip&~1u;
                    jit.SetCpsr((*skip&1u)?0x20u:0u);
                    jit.Run();
                    std::cout<<"TINYXML2_DIRECT_SKIP_WHITESPACE delta="
                             <<static_cast<std::int64_t>(jit.Regs()[0])-xmlTextAddr
                             <<" returned="<<bridge.returned
                             <<" fault="<<bridge.reason<<"\n";
                }


            }

        }


#endif
#if defined(PV7_COCOS_JNI_RUNNER)
        // Original Cocos2dxHelper.init() invokes nativeSetApkPath(sourceDir)
        // before the renderer's nativeInit. Reproduce that exact order.
        const auto set_apk=image.FindExport(
            "Java_org_cocos2dx_lib_Cocos2dxHelper_nativeSetApkPath");
        std::string apk_path,apk_error;
        std::uint32_t guest_apk_string=0;
        if (!set_apk || !ResolveAndroidStaticString("getGuestApkPath",apk_path,apk_error) ||
            !bridge.StageGuestString(apk_path,guest_apk_string)) {
            std::cout<<"COCOS_GUEST_APK_PATH_SETUP_FAILED "<<apk_error<<"\n";
            return 3;
        }
        bridge.returned=false;
        bridge.faulted=false;
        bridge.reason.clear();
        bridge.ticks_remaining=2000000;
        jit.ClearHalt();
        jit.Regs().fill(0);
        jit.Regs()[0]=kEnv;
        jit.Regs()[1]=0;
        jit.Regs()[2]=guest_apk_string;
        jit.Regs()[13]=kStack;
        jit.Regs()[14]=kReturnTrap;
        jit.Regs()[15]=*set_apk & ~1u;
        jit.SetCpsr((*set_apk & 1u) ? 0x20u : 0u);
        std::cout<<"COCOS_GUEST_SET_APK_PATH_BEGIN "<<apk_path<<"\n";
        jit.Run();
        if (bridge.faulted || !bridge.returned) {
            std::cout<<"COCOS_GUEST_SET_APK_PATH_STOP "
                     <<(bridge.faulted?bridge.reason:"instruction_budget")<<"\n";
            return 3;
        }
        std::cout<<"COCOS_GUEST_SET_APK_PATH_OK\n";
#endif
        const auto init = image.FindExport("Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeInit");
        if (!init) { std::cout << "COCOS_NATIVE_INIT_MISSING\n";return 3; }
        bridge.returned = false;
        bridge.faulted = false;
        bridge.reason.clear();
        bridge.ticks_remaining = 1000000;
        jit.ClearHalt();
        jit.Regs().fill(0);
        jit.Regs()[0] = kEnv; // JNIEnv* mock
        jit.Regs()[1] = 0; // Java renderer object placeholder
        jit.Regs()[2] = 480; // reference landscape width
        jit.Regs()[3] = 320; // reference landscape height
        jit.Regs()[13] = kStack;
        jit.Regs()[14] = kReturnTrap;
        jit.Regs()[15] = *init & ~1u;
        jit.SetCpsr((*init & 1u) ? 0x20u : 0u);
        std::cout << "EXECUTING_COCOS_NATIVE_INIT pc=0x" << std::hex << *init
                  << std::dec << "\n";
        jit.Run();
        // Drive every suspended Cocos bitmap JNI request as a complete
        // guest->host->guest call. The Win32 renderer is diagnostic only.
        // Android must supply pixels using its own Paint/Canvas implementation.
        unsigned bitmap_callbacks = 0;
        while (!bridge.faulted && bridge.pending_bitmap && bitmap_callbacks < 64) {
            const auto native_bitmap = image.FindExport(
                "Java_org_cocos2dx_lib_Cocos2dxBitmap_nativeInitBitmapDC");
            if (!native_bitmap) {
                std::cout << "COCOS_BITMAP_NATIVE_CALLBACK_EXPORT_MISSING\n";
                return 3;
            }
            auto request = std::move(*bridge.pending_bitmap);
            bridge.pending_bitmap.reset();
            pv7::compat::cocos2dx::BitmapPixels rendered;
            std::string render_error;
            #if defined(PV7_COCOS_JNI_RUNNER)
            if (!RenderAndroidBitmap(request, rendered, render_error)) {
                std::cout << "COCOS_BITMAP_ANDROID_RENDER_FAILED " << render_error << "\n";
                return 3;
            }
            std::cout << "COCOS_BITMAP_ANDROID_RENDER_OK dimensions="
                      << rendered.width << "x" << rendered.height
                      << " bytes=" << rendered.bytes.size() << "\n";
#elif defined(__ANDROID__)
            std::cout << "COCOS_BITMAP_ANDROID_RENDER_BRIDGE_NOT_CONNECTED\n";
            return 3;
#else
if (!pv7::compat::cocos2dx::RenderProbeBitmap(
                    request, rendered, render_error)) {
                std::cout << "COCOS_BITMAP_HOST_RENDER_FAILED " << render_error << "\n";
                return 3;
            }
            #endif
            std::uint32_t array_ref = 0;
            if (!bridge.StageBitmap(rendered, array_ref, render_error)) {
                std::cout << "COCOS_BITMAP_GUEST_ARRAY_FAILED " << render_error << "\n";
                return 3;
            }
            constexpr std::uint32_t callback_stack = kStack - 0x80;
            if (!Put32(memory, callback_stack, array_ref)) {
                bridge.ReleaseBitmap(array_ref);
                std::cout << "COCOS_BITMAP_CALLBACK_STACK_INVALID\n";
                return 3;
            }
            // Interruptible A32 guest call: preserve all integer, VFP, status
            // and return state, run the original function, then resume the
            // suspended CallStaticVoidMethodV at the exact point it stopped.
            const auto interrupted_regs = jit.Regs();
            const auto interrupted_extregs = jit.ExtRegs();
            const auto interrupted_cpsr = jit.Cpsr();
            const auto interrupted_fpscr = jit.Fpscr();
            bridge.returned = false;
            bridge.ticks_remaining = 100000000;
            jit.ClearHalt();
            jit.Regs().fill(0);
            jit.Regs()[0] = kEnv;
            jit.Regs()[1] = 0;
            jit.Regs()[2] = static_cast<std::uint32_t>(rendered.width);
            jit.Regs()[3] = static_cast<std::uint32_t>(rendered.height);
            jit.Regs()[13] = callback_stack;
            jit.Regs()[14] = kReturnTrap;
            jit.Regs()[15] = *native_bitmap & ~1u;
            jit.SetCpsr((*native_bitmap & 1u) ? 0x20u : 0u);
            std::cout << "COCOS_BITMAP_GUEST_CALLBACK_BEGIN pc=0x"
                      << std::hex << *native_bitmap
                      << " array=0x" << array_ref << std::dec
                      << " dimensions=" << rendered.width << "x"
                      << rendered.height << " bytes=" << rendered.bytes.size()
                      << "\n";
            jit.Run();
            const bool completed_callback = bridge.returned && !bridge.faulted;
            bridge.ReleaseBitmap(array_ref);
            if (!completed_callback) {
                std::cout << "COCOS_BITMAP_GUEST_CALLBACK_STOP "
                          << (bridge.faulted ? bridge.reason : "instruction_budget")
                          << " pc=0x" << std::hex << jit.Regs()[15] << std::dec
                          << "\n";
                return 3;
            }
            ++bitmap_callbacks;
            std::cout << "COCOS_BITMAP_GUEST_CALLBACK_COMPLETED count="
                      << bitmap_callbacks << "\n";
            jit.Regs() = interrupted_regs;
            jit.ExtRegs() = interrupted_extregs;
            jit.SetCpsr(interrupted_cpsr);
            jit.SetFpscr(interrupted_fpscr);
            bridge.returned = false;
            bridge.ticks_remaining = 400000000;
            bridge.next_parser_sample_tick_ = 375000000;
            jit.ClearHalt();
            jit.Run();
        }
        if (bridge.pending_bitmap) {
            std::cout << "COCOS_BITMAP_REQUEST_LIMIT count="
                      << bitmap_callbacks << "\n";
            return 3;
        }
        if (bridge.faulted) {
            std::cout << "COCOS_NATIVE_INIT_STOP " << bridge.reason
                      << " fault_address=0x" << std::hex << bridge.fault_address
                      << std::dec << "\n";
            return 3;
        }
        if (bridge.returned) {
            std::cout<<"COCOS_NATIVE_INIT_RETURNED\n";
            const auto render=image.FindExport("Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender");
            if(!render) {
                std::cout<<"COCOS_NATIVE_RENDER_EXPORT_MISSING\n";
                return 3;
            }
            // Exercise the actual Cocos 30 FPS boot sequence without resetting
            // game state; capture only bounded checkpoints for comparison.
            constexpr unsigned kDiagnosticFrames = 30;
            for (unsigned frame = 1; frame <= kDiagnosticFrames; ++frame) {
                bridge.returned=false;
                bridge.faulted=false;
                bridge.reason.clear();
                bridge.ticks_remaining=300000000u;
                jit.ClearHalt();
                jit.Regs().fill(0);
                jit.Regs()[0]=kEnv;
                jit.Regs()[1]=0;
                jit.Regs()[13]=kStack;
                jit.Regs()[14]=kReturnTrap;
                jit.Regs()[15]=*render&~1u;
                jit.SetCpsr((*render&1u)?0x20u:0u);
                std::cout<<"COCOS_NATIVE_RENDER_BEGIN frame="<<frame
                         <<" pc=0x"<<std::hex<<*render<<std::dec<<"\n";
                jit.Run();
                if(bridge.faulted||!bridge.returned||bridge.pending_bitmap) {
                    std::cout<<"COCOS_NATIVE_RENDER_STOP frame="<<frame<<" reason="
                             <<(bridge.faulted?bridge.reason:
                                bridge.pending_bitmap?"pending_bitmap":"instruction_budget")
                             <<" pc=0x"<<std::hex<<jit.Regs()[15]<<std::dec<<"\n";
                    return 3;
                }
                std::cout<<"COCOS_NATIVE_RENDER_RETURNED frame="<<frame<<"\n";
                if ((frame == 1u || (frame % 10u) == 0u) && !CaptureCocosFrame(frame))
                    return 3;
                if (frame < kDiagnosticFrames)
                    std::this_thread::sleep_for(std::chrono::milliseconds(34));
            }
            std::cout<<"COCOS_RENDER_SEQUENCE_COMPLETED frames="
                     <<kDiagnosticFrames<<"\n";
            return 0;
        }
        const auto& halted=jit.Regs();
        std::cout << "COCOS_NATIVE_INIT_TIMED_OUT pc=0x" << std::hex
                  << halted[15] << " r0=0x" << halted[0]
                  << " r1=0x" << halted[1] << " r2=0x" << halted[2]
                  << " r3=0x" << halted[3] << " r4=0x" << halted[4]
                  << " r5=0x" << halted[5] << " sp=0x" << halted[13]
                  << std::dec << "\n";
        return 4;
    }
    std::cout << "GUEST_STOP instruction_budget_exhausted_or_stalled pc=0x"
              << std::hex << jit.Regs()[15] << std::dec << "\n";
    return 4;
}

#if defined(PV7_COCOS_JNI_RUNNER)
extern "C" JNIEXPORT jint JNICALL
Java_me_jacqueb_pv7_cocos_CocosProbeActivity_nativeRun(
        JNIEnv* env, jclass, jstring guestPath, jstring logPath) {
    if (!guestPath || !logPath || !env) return 2;
    const char* guest = env->GetStringUTFChars(guestPath, nullptr);
    const char* logfile = env->GetStringUTFChars(logPath, nullptr);
    if (!guest || !logfile) {
        if (guest) env->ReleaseStringUTFChars(guestPath, guest);
        if (logfile) env->ReleaseStringUTFChars(logPath, logfile);
        return 2;
    }
    std::string original(guest);
    std::string path(logfile);
    env->ReleaseStringUTFChars(guestPath, guest);
    env->ReleaseStringUTFChars(logPath, logfile);

    // This debug-only activity is dedicated to a single worker/probe run.
    // Redirect its verbose native output to a private file for adb run-as.
    fflush(stdout); fflush(stderr);
    int savedOut = dup(STDOUT_FILENO);
    int savedErr = dup(STDERR_FILENO);
    FILE* file = fopen(path.c_str(), "w");
    if (!file) return 2;
    dup2(fileno(file), STDOUT_FILENO);
    dup2(fileno(file), STDERR_FILENO);

    char first[] = "pv7-cocos-probe";
    char mode[] = "boot";
    char* args[] = {first, original.data(), mode};
    g_pv7_cocos_env = env;
    g_pv7_cocos_capture_path = path;
    int result = RunCocosProbe(3, args);
    g_pv7_cocos_capture_path.clear();
    g_pv7_cocos_env = nullptr;
    std::cout << "ANDROID_JAVA_PROBE_EXIT=" << result << std::endl;
    fflush(stdout); fflush(stderr);
    if (savedOut >= 0) { dup2(savedOut, STDOUT_FILENO); close(savedOut); }
    if (savedErr >= 0) { dup2(savedErr, STDERR_FILENO); close(savedErr); }
    fclose(file);
    return result;
}
#else
int main(int argc, char** argv) {
    return RunCocosProbe(argc, argv);
}
#endif