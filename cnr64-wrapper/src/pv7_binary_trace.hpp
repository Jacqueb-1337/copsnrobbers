// PV7 developer-only, low-overhead host/guest execution trace.
//
// Scope: every emulated SVC/JNI/import boundary, paired return, every JIT
// timeslice boundary, guest worker pump, and translated code invalidation.
// This does NOT claim to trace every guest ARM BL/BX or instruction: Dynarmic
// JIT runs those internally. Enable instruction stepping only in a separately
// constrained targeted debugger, not during startup performance measurement.
//
// File: app-private files/pv7_call_trace_v1.bin (96 MiB max).
// Layout: packed 40-byte little-endian records, see decoder in devtools/ai-re.
#pragma once
#include <array>
#include <chrono>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <string>
#include <unordered_map>

#if defined(PROJECTV7_TEST_BINARY_CALL_TRACE) && PROJECTV7_TEST_BINARY_CALL_TRACE && defined(__ANDROID__)
#include <android/log.h>
namespace pv7::trace {
enum : std::uint32_t {
    SvcEnter=1, SvcExit=2, Phase=3, JitEnter=4, JitExit=5,
    WorkerEnter=6, WorkerExit=7, CacheInvalidate=8, CodeTranslation=9
};
struct Record {
    std::uint64_t monotonic_ns;
    std::uint32_t type;
    std::uint32_t tid;
    std::uint32_t pc;
    std::uint32_t lr;
    std::uint32_t a0;
    std::uint32_t a1;
    std::uint32_t a2;
    std::uint32_t a3;
};
static_assert(sizeof(Record)==40, "Trace record layout must remain stable");
class Recorder {
public:
    static constexpr std::size_t kBatch = 4096;
    static constexpr std::uint64_t kMaxBytes = 32u*1024u*1024u;
    explicit Recorder(const std::string& managed_dir) {
        std::size_t p = managed_dir.find("/files/");
        if (p == std::string::npos && managed_dir.size() >= 6 &&
            managed_dir.compare(managed_dir.size()-6, 6, "/files") == 0)
            p = managed_dir.size()-6;
        if (p == std::string::npos) {
            __android_log_print(ANDROID_LOG_WARN, "CNR64POC",
                "PV7TRACE disabled: managed_dir not inside app files");
            return;
        }
        const std::string root = managed_dir.substr(0,p+6);
        path_ = root + "/pv7_call_trace_v1.bin";
        const auto names = root + "/pv7_call_trace_names.tsv";
        file_ = std::fopen(path_.c_str(), "wb");
        if (!file_) {
            __android_log_print(ANDROID_LOG_ERROR, "CNR64POC",
                "PV7TRACE cannot open app-private trace: %s",path_.c_str());
            return;
        }
        // Four ASCII magic/version bytes, followed by little-endian record size.
        // The decoder refuses incompatible versions.
        static const std::array<unsigned char,8> header{'P','V','7','T',1,0,40,0};
        if (std::fwrite(header.data(),1,header.size(),file_) != header.size()) {
            std::fclose(file_);
            file_ = nullptr;
            return;
        }
        std::setvbuf(file_,nullptr,_IONBF,0); // batches are already buffered in RAM
        names_path_ = names;
        // Pair CLOCK_BOOTTIME/steady_clock timestamps with epoch UTC once.
        const auto wall = std::chrono::system_clock::now().time_since_epoch();
        const auto epoch_ns = std::chrono::duration_cast<std::chrono::nanoseconds>(wall).count();
        const auto sec = static_cast<std::uint64_t>(epoch_ns / 1000000000LL);
        const auto ns = static_cast<std::uint64_t>(epoch_ns % 1000000000LL);
        Add(0,0,0,0,static_cast<std::uint32_t>(sec),
            static_cast<std::uint32_t>(ns),0,0);
        __android_log_print(ANDROID_LOG_INFO, "CNR64POC",
            "PV7TRACE enabled path=%s record=40 max_bytes=%llu",
            path_.c_str(),static_cast<unsigned long long>(kMaxBytes));
    }
    Recorder(const Recorder&)=delete;
    Recorder& operator=(const Recorder&)=delete;
    ~Recorder() {
        Flush();
        if(file_) std::fclose(file_);
        __android_log_print(ANDROID_LOG_INFO,"CNR64POC",
            "PV7TRACE closed events=%llu dropped=%llu bytes=%llu path=%s",
            static_cast<unsigned long long>(total_),
            static_cast<unsigned long long>(dropped_),
            static_cast<unsigned long long>(bytes_),path_.c_str());
    }
    static std::uint64_t NowNs() {
        return static_cast<std::uint64_t>(
            std::chrono::duration_cast<std::chrono::nanoseconds>(
                std::chrono::steady_clock::now().time_since_epoch()).count());
    }
    bool Enabled() const {return file_ != nullptr;}
    static constexpr std::uint32_t NameHash(const char* str) {
        std::uint32_t h=2166136261u;
        while(*str) h=(h^static_cast<unsigned char>(*str++))*16777619u;
        return h;
    }
    // Record each complete SVC enter/exit pair at a predictable sampling
    // cadence. Never flood startup with millions of libc import events.
    // JIT, scheduler and invalidation events are retained continuously.
    bool ShouldCaptureSvc() {
        if(!file_ || full_ || !frame2_active_) return false;
        ++seen_svc_;
        // An even 1-in-512 sample gives a better timeline than capturing the
        // first several thousand calls of each burst.
        if((seen_svc_ % 512u) != 0u) return false;
        const auto bucket=(NowNs()-frame2_started_ns_)/10000000000ULL;
        if(bucket != svc_bucket_) {
            svc_bucket_=bucket;
            svc_budget_used_=0;
        }
        if(svc_budget_used_ >= 400u) return false;
        ++svc_budget_used_;
        ++captured_svc_;
        return true;
    }
    void Add(std::uint32_t type,std::uint32_t tid,std::uint32_t pc,
             std::uint32_t lr,std::uint32_t a0=0,std::uint32_t a1=0,
             std::uint32_t a2=0,std::uint32_t a3=0) {
        if(!file_ || full_) return;
        const std::uint64_t timestamp=NowNs();
        const bool frame2_phase=type==Phase && a0==NameHash("nativeRenderFrame2");
        if(frame2_phase && a1==1u && !frame2_active_) {
            frame2_active_=true;
            frame2_started_ns_=timestamp;
            __android_log_print(ANDROID_LOG_INFO,"CNR64POC",
                "PV7TRACE frame2 active; complete SVC pairs sampled 1/512, max 400 per 10s");
        }
        if(type!=0 && type!=Phase && !frame2_active_) return;
        records_[used_++]={timestamp,type,tid,pc,lr,a0,a1,a2,a3};
        ++total_;
        if(used_==records_.size()) Flush();
        if(frame2_phase && a1==0u) {
            Flush();
            frame2_active_=false;
            __android_log_print(ANDROID_LOG_INFO,"CNR64POC",
                "PV7TRACE frame2 closed events=%llu seen_svc=%llu captured_svc=%llu bytes=%llu",
                static_cast<unsigned long long>(total_),
                static_cast<unsigned long long>(seen_svc_),
                static_cast<unsigned long long>(captured_svc_),
                static_cast<unsigned long long>(bytes_));
        }
    }
    void Flush() {
        if (!file_ || used_ == 0) return;
        const std::uint64_t size = used_ * sizeof(Record);
        if (bytes_ + size > kMaxBytes) {
            full_ = true;
            dropped_ += used_;
            used_ = 0;
            __android_log_print(ANDROID_LOG_WARN,"CNR64POC","PV7TRACE capped at 32 MiB");
            return;
        }
        const auto written = std::fwrite(records_.data(),sizeof(Record),used_,file_);
        bytes_ += written * sizeof(Record);
        if (written != used_) {
            dropped_ += used_ - written;
            full_ = true;
            __android_log_print(ANDROID_LOG_ERROR,"CNR64POC","PV7TRACE disk write failed");
        }
        used_ = 0;
    }
    void WriteNames(const std::unordered_map<std::uint32_t,std::string>& names) {
        if(!file_ || names_path_.empty()) return;
        std::FILE* f = std::fopen(names_path_.c_str(),"w");
        if(!f) return;
        std::fputs("svc_id\tsymbol\n",f);
        for (const auto& [id,name]:names) {
            std::fprintf(f,"%u\t%s\n",id,name.c_str());
        }
        std::fclose(f);
    }
private:
    std::FILE* file_ = nullptr;
    std::string path_;
    std::string names_path_;
    std::array<Record,kBatch> records_{};
    std::size_t used_=0;
    std::uint64_t total_=0;
    std::uint64_t dropped_=0;
    std::uint64_t bytes_=8;
    std::uint64_t frame2_started_ns_=0;
    std::uint64_t seen_svc_=0;
    std::uint64_t captured_svc_=0;
    std::uint64_t svc_bucket_=~std::uint64_t{0};
    std::uint32_t svc_budget_used_=0;
    bool frame2_active_=false;
    bool full_=false;
};
struct SvcScope {
    Recorder& trace;
    const std::uint32_t* regs;
    std::uint32_t svc,tid;
    std::uint64_t started=0;
    bool captured=false;
    SvcScope(Recorder& recorder,std::uint32_t id,std::uint32_t thread_id,
             const std::uint32_t* guest_regs):
        trace(recorder),regs(guest_regs),svc(id),tid(thread_id),
        captured(trace.ShouldCaptureSvc()) {
        if(!captured) return;
        started=Recorder::NowNs();
        trace.Add(SvcEnter,tid,regs?regs[15]:0,regs?regs[14]:0,svc,
                  regs?regs[0]:0,regs?regs[1]:0,regs?regs[2]:0);
    }
    ~SvcScope() {
        if(!captured) return;
        const auto elapsed=Recorder::NowNs()-started;
        trace.Add(SvcExit,tid,regs?regs[15]:0,regs?regs[14]:0,svc,
                  regs?regs[0]:0,static_cast<std::uint32_t>(elapsed / 1000u),
                  regs?regs[3]:0);
    }
    SvcScope(const SvcScope&)=delete;
    SvcScope& operator=(const SvcScope&)=delete;
};
} // namespace pv7::trace
#endif
