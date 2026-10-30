#pragma once
#include <cstddef>
#include <cstdint>
#include <limits>
#include <new>
#include <vector>

#if defined(__ANDROID__) && defined(__aarch64__) && PROJECTV7_TEST_ARM64_FASTMEM
#include <sys/mman.h>
#include <unistd.h>

// A vector-compatible backing store with a page-aligned, guarded 32-bit address
// space. Only the requested guest backing is writable; the rest stays PROT_NONE.
template<class T> struct GuestMemoryAllocator {
    using value_type=T;
    using is_always_equal=std::true_type;
    GuestMemoryAllocator()=default;
    template<class U> GuestMemoryAllocator(const GuestMemoryAllocator<U>&) {}
    static size_t reservation() { return (size_t{1}<<32)+static_cast<size_t>(sysconf(_SC_PAGESIZE)); }
    T* allocate(size_t count) {
        const size_t page=static_cast<size_t>(sysconf(_SC_PAGESIZE));
        if(count> (size_t{1}<<32)/sizeof(T)) throw std::bad_alloc();
        const size_t bytes=count*sizeof(T);
        // The runtime backing is page-sized. Reject partial pages rather than
        // exposing extra bytes to fastmem beyond the vector's logical end.
        if(!bytes || bytes%page) throw std::bad_alloc();
        void* p=mmap(nullptr,reservation(),PROT_NONE,MAP_PRIVATE|MAP_ANONYMOUS,-1,0);
        if(p==MAP_FAILED) throw std::bad_alloc();
        if(mprotect(p,bytes,PROT_READ|PROT_WRITE)) {
            munmap(p,reservation()); throw std::bad_alloc();
        }
        return static_cast<T*>(p);
    }
    void deallocate(T* p,size_t) noexcept { munmap(p,reservation()); }
};
template<class T,class U> bool operator==(const GuestMemoryAllocator<T>&,const GuestMemoryAllocator<U>&){return true;}
template<class T,class U> bool operator!=(const GuestMemoryAllocator<T>&,const GuestMemoryAllocator<U>&){return false;}
using GuestMemory=std::vector<std::uint8_t,GuestMemoryAllocator<std::uint8_t>>;
#else
using GuestMemory=std::vector<std::uint8_t>;
#endif
