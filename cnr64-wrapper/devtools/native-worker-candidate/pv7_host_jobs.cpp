#include "pv7_host_jobs.hpp"
#include <algorithm>
#include <cstring>
#include <string_view>
#include <boost/json/src.hpp> // single implementation TU
#if defined(__ANDROID__)
#include <zlib.h>
#endif

namespace pv7::host {
namespace {
using Bytes = WorkerPool::Bytes;
struct Writer {
    Bytes out;
    void Need(std::size_t n) {
        if (n > kMaxJobBytes - out.size()) throw std::length_error("worker result too large");
    }
    void Byte(std::uint8_t v) { Need(1); out.push_back(v); }
    void U32(std::uint32_t v) { for (int i=0;i<4;++i) Byte(static_cast<std::uint8_t>(v >> (i*8))); }
    void U64(std::uint64_t v) { for (int i=0;i<8;++i) Byte(static_cast<std::uint8_t>(v >> (i*8))); }
    void Str(boost::json::string_view s) {
        U32(static_cast<std::uint32_t>(s.size()));
        Need(s.size()); out.insert(out.end(), s.begin(), s.end());
    }
    void Value(const boost::json::value& v) {
        switch (v.kind()) {
        case boost::json::kind::null: Byte(0); break;
        case boost::json::kind::bool_: Byte(v.as_bool()?2:1); break;
        case boost::json::kind::string: Byte(3); Str(v.as_string()); break;
        case boost::json::kind::array:
            Byte(4); U32(static_cast<std::uint32_t>(v.as_array().size()));
            for (const auto& x : v.as_array()) Value(x);
            break;
        case boost::json::kind::object:
            Byte(5); U32(static_cast<std::uint32_t>(v.as_object().size()));
            for (const auto& x : v.as_object()) { Str(x.key()); Value(x.value()); }
            break;
        case boost::json::kind::int64: Byte(6); U64(static_cast<std::uint64_t>(v.as_int64())); break;
        case boost::json::kind::uint64: Byte(7); U64(v.as_uint64()); break;
        case boost::json::kind::double_:
            Byte(8); { double x=v.as_double(); std::uint64_t bits=0; std::memcpy(&bits,&x,sizeof(bits)); U64(bits); } break;
        default: throw std::runtime_error("unknown JSON kind");
        }
    }
};
}
WorkerPool::Bytes RunHostJob(JobOp op, WorkerPool::Bytes input, std::uint32_t aux) {
    if (input.size() > kMaxJobBytes) throw std::length_error("input too large");
    switch (op) {
    case JobOp::JsonTree: {
        if (input.empty() || input.size() > 1024u*1024u) throw std::length_error("JSON input size");
        std::size_t skip = input.size() >= 3 && input[0]==0xef && input[1]==0xbb && input[2]==0xbf ? 3 : 0;
        boost::json::parse_options options; options.max_depth = 64;
        auto value = boost::json::parse(boost::json::string_view(
            reinterpret_cast<const char*>(input.data()+skip),input.size()-skip), {}, options);
        Writer w; w.Byte('P'); w.Byte('V'); w.Byte('J'); w.Byte(1); w.Value(value);
        return std::move(w.out);
    }
    case JobOp::Crc32: {
        std::uint32_t crc = 0xffffffffu;
        for (std::uint8_t b : input) {
            crc ^= b;
            for (int i=0;i<8;++i) crc = (crc>>1) ^ (0xedb88320u & (0u-(crc&1u)));
        }
        Writer w; w.U32(~crc); return std::move(w.out);
    }
    case JobOp::InflateZlib:
#if defined(__ANDROID__)
        if (!aux || aux > kMaxJobBytes) throw std::length_error("inflate cap");
        {
            Bytes output(aux);
            uLongf size=static_cast<uLongf>(output.size());
            if (uncompress(output.data(), &size, input.data(), static_cast<uLong>(input.size())) != Z_OK)
                throw std::runtime_error("invalid or oversized compressed data");
            output.resize(size); return output;
        }
#else
        throw std::runtime_error("inflate unavailable");
#endif
    }
    throw std::invalid_argument("unknown host job");
}
}
