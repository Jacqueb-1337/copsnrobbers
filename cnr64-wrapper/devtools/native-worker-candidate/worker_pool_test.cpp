#include "pv7_host_worker_pool.hpp"
#include <atomic>
#include <chrono>
#include <iostream>
#include <set>
using namespace std::chrono_literals;
using Pool = pv7::host::WorkerPool;
void Check(bool b, const char* label) {
    if (!b) throw std::runtime_error(label);
    std::cout << "PASS " << label << std::endl;
}
int main() {
    Pool pool(2, 2);
    std::atomic<int> entered{0};
    std::atomic<bool> release{false};
    std::mutex ids_mutex;
    std::set<std::thread::id> ids;
    auto main_id = std::this_thread::get_id();
    auto work = [&](Pool::Bytes b) {
        { std::lock_guard<std::mutex> lock(ids_mutex); ids.insert(std::this_thread::get_id()); }
        ++entered;
        while (!release.load()) std::this_thread::sleep_for(1ms);
        for (auto& v : b) ++v;
        return b;
    };
    Pool::Bytes input{1,2,3};
    std::future<Pool::Result> a,b,c;
    bool first = pool.TrySubmit(input, work, a);
    bool second = pool.TrySubmit(input, work, b);
    input[0] = 99;
    auto deadline = std::chrono::steady_clock::now()+5s;
    while (entered.load()!=2 && std::chrono::steady_clock::now()<deadline) std::this_thread::sleep_for(1ms);
    bool concurrent = entered.load()==2;
    bool bounded = !pool.TrySubmit(input,work,c);
    release.store(true);
    Check(first && second,"two submissions accepted");
    Check(concurrent,"two native workers run concurrently");
    Check(bounded,"outstanding limit rejects additional work");
    Check(a.wait_for(5s)==std::future_status::ready && b.wait_for(5s)==std::future_status::ready,"nonblocking completion polling");
    auto ar=a.get(), br=b.get();
    Check(!ar.error && !br.error && ar.bytes==Pool::Bytes({2,3,4}) && br.bytes==ar.bytes,"copied input and results independent of submitter");
    Check(ids.size()==2 && !ids.count(main_id),"distinct native thread identities");
    Check(pool.TrySubmit(input,[](Pool::Bytes)->Pool::Bytes{throw std::runtime_error("test");},c),"capacity restored");
    Check(c.wait_for(5s)==std::future_status::ready && bool(c.get().error),"exception returned to owner");
    std::future<Pool::Result> d;
    { Pool drain(1,2); Check(drain.TrySubmit({9},[](Pool::Bytes x){return x;},d),"shutdown test accepted"); }
    Check(d.wait_for(0s)==std::future_status::ready && d.get().bytes==Pool::Bytes({9}),"shutdown drains accepted work");
    std::cout<<"10 tests passed"<<std::endl;
}
