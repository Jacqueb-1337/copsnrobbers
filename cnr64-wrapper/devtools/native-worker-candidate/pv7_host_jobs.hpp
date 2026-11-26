#include <algorithm>
#pragma once
#include "pv7_host_worker_pool.hpp"
#include <chrono>
#include <memory>
#include <optional>
#include <unordered_map>

namespace pv7::host {
constexpr std::uint32_t kHostJobsVersion = 1;
constexpr std::size_t kMaxJobBytes = 4u * 1024u * 1024u;
enum class JobOp : std::uint32_t { JsonTree = 1, Crc32 = 2, InflateZlib = 3 };

// Worker functions accept owned bytes; no guest memory, JIT or JNI state is accessible.
WorkerPool::Bytes RunHostJob(JobOp op, WorkerPool::Bytes input, std::uint32_t aux);

// The harness thread exclusively owns the ticket table. Only copied byte payloads
// cross to native workers. There are at most eight live results/tickets.
class HostJobs final {
public:
    std::uint32_t Submit(std::uint32_t op, const std::uint8_t* input,
                         std::size_t size, std::uint32_t aux) noexcept {
        if (jobs_.size() >= 8 || size > kMaxJobBytes || (size && !input)) return 0;
        if (op < 1 || op > 3) return 0;
        if (op == 1 && size > 1024u * 1024u) return 0;
        if (op == 3 && (aux == 0 || aux > kMaxJobBytes)) return 0;
#if !defined(__ANDROID__)
        if (op == 3) return 0; // zlib backend is linked only in the Android host
#endif
        try {
            if (!pool_) pool_ = std::make_unique<WorkerPool>(2, 8);
            WorkerPool::Bytes bytes;
            if (size) bytes.assign(input, input + size);
            Ticket t;
            auto work = [op, aux](WorkerPool::Bytes owned) {
                return RunHostJob(static_cast<JobOp>(op), std::move(owned), aux);
            };
            if (!pool_->TrySubmit(bytes, work, t.future)) return 0;
            do { ++next_id_; } while (!next_id_ || jobs_.count(next_id_));
            const auto id = next_id_;
            jobs_.emplace(id, std::move(t));
            return id;
        } catch (...) { return 0; }
    }

    // 0 pending; >0 ready length + 1; -1 unknown ID; -2 failed.
    int Poll(std::uint32_t id) noexcept {
        const auto it = jobs_.find(id);
        if (it == jobs_.end()) return -1;
        auto& t = it->second;
        try {
            if (!t.result) {
                if (t.future.wait_for(std::chrono::seconds(0)) != std::future_status::ready) return 0;
                t.result.emplace(t.future.get());
            }
            if (t.result->error || t.result->bytes.size() > kMaxJobBytes) return -2;
            return static_cast<int>(t.result->bytes.size()) + 1;
        } catch (...) { return -2; }
    }

    // -3 means destination capacity insufficient. Retain the ticket for retry.
    int Copy(std::uint32_t id, std::uint8_t* output, std::size_t capacity) noexcept {
        const int state = Poll(id);
        if (state <= 0) return state == 0 ? -4 : state;
        const auto n = static_cast<std::size_t>(state - 1);
        if (capacity < n || (n && !output)) return -3;
        auto it = jobs_.find(id);
        const auto& bytes = it->second.result->bytes;
        if (n) std::copy(bytes.begin(), bytes.end(), output);
        jobs_.erase(it);
        return static_cast<int>(n);
    }

    bool Release(std::uint32_t id) noexcept { return jobs_.erase(id) != 0; }

private:
    struct Ticket {
        std::future<WorkerPool::Result> future;
        std::optional<WorkerPool::Result> result;
    };
    std::unique_ptr<WorkerPool> pool_;
    std::unordered_map<std::uint32_t, Ticket> jobs_;
    std::uint32_t next_id_ = 0;
};
}
