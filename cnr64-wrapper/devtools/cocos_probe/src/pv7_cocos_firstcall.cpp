#include "../../../compat_modules/cocos2dx/cocos2dx_guest_jni_arrays.h"
#include "../../../compat_modules/cocos2dx/cocos2dx_jni_marshaller.h"
#include "arm32_guest_image.h"
#include "dynarmic/interface/A32/a32.h"

#include "pv7_gles_probe.h"
#include <cstdlib>
#include <algorithm>
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
            if (name != "org/cocos2dx/lib/Cocos2dxBitmap") {
                Fault(regs[15], "JNI_FindClass_unknown:" + name);
                return;
            }
            // Diagnostic-only identity for a class known to the original game.
            // No methods are simulated. The next JNI access remains fail-closed.
            constexpr std::uint32_t kDiagnosticBitmapClass = 0x0000c010;
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
            if (classRef == 0x0000c010u &&
                name == "createTextBitmap" &&
                sig == "(Ljava/lang/String;Ljava/lang/String;IIII)V") {
                constexpr std::uint32_t kTextMethod = 0x0000c020;
                regs[0] = kTextMethod;
                std::cout << "JNI_METHOD_IDENTIFIED createTextBitmap token=0x"
                          << std::hex << kTextMethod << std::dec << "\n";
                return;
            }
            Fault(regs[15], "JNI_GetStaticMethodID_unknown:" + name + sig);
            return;
        }
        if (svc == kMissingJniSvc + 171u) {
            std::int32_t count=0;
            if (!byte_arrays_.Length(regs[1],count)) {
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
                !byte_arrays_.CopyRegion(regs[1],start,count,mem_.data()+destination)) {
                Fault(regs[15],"JNI_GetByteArrayRegion_invalid"); return;
            }
            return;
        }        if (svc == kMissingJniSvc + 142u) {
            if (regs[1] != 0x0000c010u || regs[2] != 0x0000c020u) {
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
            // The real Android renderer and its nativeInitBitmapDC callback
            // must be attached before allowing guest execution to continue.
            Fault(regs[15], "JNI_BitmapText_host_renderer_not_connected");
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
            regs[0] = handle;
            std::cout << "JNI_NEW_STRING_UTF handle=0x" << std::hex << handle
                      << std::dec << " bytes=" << guest_strings_.at(handle).size()
                      << "\n";
            return;
        }
        if (svc >= kMissingJniSvc && svc < kMissingJniSvc + 232u) {
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
            // Bounded guest-memory equivalents of common Bionic routines.
            // These operate solely within the ARM guest address space.
            if (item.name == "memcpy" || item.name == "memmove" ||
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
            if (item.name == "strlen") {
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
            if (item.name == "glCreateProgram" || item.name == "glCreateShader" || item.name == "glShaderSource" || item.name == "glCompileShader" || item.name == "glGetShaderiv" || item.name == "glAttachShader" || item.name == "glLinkProgram" || item.name == "glGetProgramiv" || item.name == "glBindAttribLocation" || item.name == "glGetUniformLocation" || item.name == "glGetAttribLocation" || item.name == "glUseProgram" || item.name == "glDeleteShader" || item.name == "glUniform1i") {
                static Pv7ProbeGles2 hostGl;
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
                if (item.name == "glShaderSource") {
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
                }                if (item.name == "glUniform1i") {
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
    std::uint32_t next_jstring_ = 0x0000c100;
    std::map<std::uint32_t, std::string> guest_strings_;
    std::uint32_t next_key_ = 1;
    std::map<std::uint32_t, std::pair<std::uint32_t, std::uint32_t>> keys_;
    std::vector<std::uint8_t>& mem_;
    const std::vector<Import>& imports_;
    struct Destructor { std::uint32_t function, arg, dso; };
    std::vector<Destructor> destructors_;
    unsigned log_count_ = 0;
};
} // namespace

int main(int argc, char** argv) {
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
            : is_data ? kObjectsBase + data_count++ * 512u
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
            for (std::uint32_t i=0;i<256;++i) {
                auto value = static_cast<std::uint8_t>(i);
                if (entry.name == "_tolower_tab_" && i >= 'A' && i <= 'Z')
                    value = static_cast<std::uint8_t>(i + ('a'-'A'));
                else if (entry.name == "_toupper_tab_" && i >= 'a' && i <= 'z')
                    value = static_cast<std::uint8_t>(i - ('a'-'A'));
                memory[table+i] = value;
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
    for (std::uint32_t i=0;i<232;++i) {
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
        if (bridge.faulted) {
            std::cout << "COCOS_NATIVE_INIT_STOP " << bridge.reason
                      << " fault_address=0x" << std::hex << bridge.fault_address
                      << std::dec << "\n";
            return 3;
        }
        if (bridge.returned) {
            std::cout << "COCOS_NATIVE_INIT_RETURNED (render loop not started)\n";
            return 0;
        }
        std::cout << "COCOS_NATIVE_INIT_TIMED_OUT pc=0x" << std::hex
                  << jit.Regs()[15] << std::dec << "\n";
        return 4;
    }
    std::cout << "GUEST_STOP instruction_budget_exhausted_or_stalled pc=0x"
              << std::hex << jit.Regs()[15] << std::dec << "\n";
    return 4;
}
