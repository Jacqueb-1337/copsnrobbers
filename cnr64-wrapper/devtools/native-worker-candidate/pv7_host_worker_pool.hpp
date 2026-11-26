#pragma once
#include <condition_variable>
#include <cstdint>
#include <deque>
#include <functional>
#include <future>
#include <mutex>
#include <stdexcept>
#include <thread>
#include <utility>
#include <vector>

namespace pv7::host {
// Native threads only: jobs own their input and never call the guest JIT, JNI
// guest handles, or Unity APIs. The submitting thread polls futures without waiting.
class WorkerPool final {
public:
    using Bytes = std::vector<std::uint8_t>;
    using Work = std::function<Bytes(Bytes)>;
    struct Result {
        Bytes bytes;
        std::exception_ptr error;
    };
    explicit WorkerPool(unsigned count = 2, std::size_t max_pending = 8)
        : limit_(max_pending) {
        if (count == 0 || count > 4 || max_pending == 0 || max_pending > 64)
            throw std::invalid_argument("invalid worker pool limits");
        try {
            for (unsigned i = 0; i < count; ++i)
                threads_.emplace_back([this] { Run(); });
        } catch (...) {
            Stop();
            throw;
        }
    }
    ~WorkerPool() { Stop(); }
    WorkerPool(const WorkerPool&) = delete;
    WorkerPool& operator=(const WorkerPool&) = delete;

    // False means caller retains ownership of the work and may retry later.
    bool TrySubmit(const Bytes& input, const Work& work, std::future<Result>& result) {
        if (!work || input.size() > kMaxInputBytes) return false;
        std::lock_guard<std::mutex> lock(mutex_);
        if (stopping_ || outstanding_ >= limit_) return false;
        Job job{input, work, {}};
        auto future = job.promise.get_future();
        jobs_.push_back(std::move(job));
        ++outstanding_;
        result = std::move(future);
        ready_.notify_one();
        return true;
    }

private:
    static constexpr std::size_t kMaxInputBytes = 4u * 1024u * 1024u;
    struct Job {
        Bytes input;
        Work work;
        std::promise<Result> promise;
    };
    void Run() noexcept {
        for (;;) {
            Job job;
            {
                std::unique_lock<std::mutex> lock(mutex_);
                ready_.wait(lock, [this] { return stopping_ || !jobs_.empty(); });
                if (jobs_.empty()) return;
                job = std::move(jobs_.front());
                jobs_.pop_front();
            }
            Result r;
            try { r.bytes = job.work(std::move(job.input)); }
            catch (...) { r.error = std::current_exception(); }
            {
                std::lock_guard<std::mutex> lock(mutex_);
                --outstanding_;
            }
            job.promise.set_value(std::move(r));
        }
    }
    void Stop() noexcept {
        {
            std::lock_guard<std::mutex> lock(mutex_);
            stopping_ = true;
        }
        ready_.notify_all();
        for (auto& thread : threads_) if (thread.joinable()) thread.join();
    }
    std::mutex mutex_;
    std::condition_variable ready_;
    std::deque<Job> jobs_;
    std::vector<std::thread> threads_;
    const std::size_t limit_;
    std::size_t outstanding_ = 0;
    bool stopping_ = false;
};
}
