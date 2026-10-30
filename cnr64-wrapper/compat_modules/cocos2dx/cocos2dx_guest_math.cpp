#include "cocos2dx_guest_math.h"
#include <bit>
#include <cmath>
#include <cstddef>
namespace pv7::compat::cocos2dx {
namespace {
template<typename Float> Float Decode(std::uint32_t low,std::uint32_t high=0) {
    if constexpr(sizeof(Float)==4) return std::bit_cast<Float>(low);
    else return std::bit_cast<Float>((static_cast<std::uint64_t>(high)<<32)|low);
}
template<typename Float> void Encode(Float value,std::array<std::uint32_t,16>& regs) {
    if constexpr(sizeof(Float)==4) regs[0]=std::bit_cast<std::uint32_t>(value);
    else {
        const auto bits=std::bit_cast<std::uint64_t>(value);
        regs[0]=static_cast<std::uint32_t>(bits);
        regs[1]=static_cast<std::uint32_t>(bits>>32);
    }
}
template<typename Float>
bool CallUnary(std::string_view name, Float x,Float& result) {
    using Fn=Float (*)(Float);
    struct Func {std::string_view name; Fn call;};
    static const Func funcs[]={
        {"sin",static_cast<Fn>(std::sin)},
        {"cos",static_cast<Fn>(std::cos)},
        {"tan",static_cast<Fn>(std::tan)},
        {"asin",static_cast<Fn>(std::asin)},
        {"acos",static_cast<Fn>(std::acos)},
        {"atan",static_cast<Fn>(std::atan)},
        {"sqrt",static_cast<Fn>(std::sqrt)},
        {"exp",static_cast<Fn>(std::exp)},
        {"exp2",static_cast<Fn>(std::exp2)},
        {"log",static_cast<Fn>(std::log)},
        {"log2",static_cast<Fn>(std::log2)},
        {"log10",static_cast<Fn>(std::log10)},
        {"ceil",static_cast<Fn>(std::ceil)},
        {"floor",static_cast<Fn>(std::floor)},
        {"round",static_cast<Fn>(std::round)},
        {"trunc",static_cast<Fn>(std::trunc)},
        {"fabs",static_cast<Fn>(std::fabs)},
        {"sinh",static_cast<Fn>(std::sinh)},
        {"cosh",static_cast<Fn>(std::cosh)},
        {"tanh",static_cast<Fn>(std::tanh)},
        {"expm1",static_cast<Fn>(std::expm1)},
        {"log1p",static_cast<Fn>(std::log1p)}
    };
    for(const auto& f:funcs) {
        if(f.name==name) {result=f.call(x);return true;}
    }
    return false;
}
template<typename Float>
bool CallBinary(std::string_view name,Float x,Float y,Float& result) {
    using Fn=Float (*)(Float,Float);
    struct Func {std::string_view name; Fn call;};
    static const Func funcs[]={
        {"pow",static_cast<Fn>(std::pow)},
        {"atan2",static_cast<Fn>(std::atan2)},
        {"fmod",static_cast<Fn>(std::fmod)},
        {"hypot",static_cast<Fn>(std::hypot)},
        {"fmin",static_cast<Fn>(std::fmin)},
        {"fmax",static_cast<Fn>(std::fmax)},
        {"remainder",static_cast<Fn>(std::remainder)},
        {"copysign",static_cast<Fn>(std::copysign)}
    };
    for(const auto& f:funcs) {
        if(f.name==name) {result=f.call(x,y);return true;}
    }
    return false;
}
}
bool DispatchGuestMath(std::string_view name,std::array<std::uint32_t,16>& regs) {
    if(name.empty()) return false;
    const bool single=name.ends_with('f') && name!="f";
    if(single) {
        const auto base=name.substr(0,name.size()-1);
        const auto a=Decode<float>(regs[0]);
        float value=0;
        if(CallUnary<float>(base,a,value) ||
           CallBinary<float>(base,a,Decode<float>(regs[1]),value)) {
            Encode(value,regs);
            return true;
        }
        return false;
    }
    const auto a=Decode<double>(regs[0],regs[1]);
    double value=0;
    if(CallUnary<double>(name,a,value) ||
       CallBinary<double>(name,a,Decode<double>(regs[2],regs[3]),value)) {
        Encode(value,regs);
        return true;
    }
    return false;
}
}