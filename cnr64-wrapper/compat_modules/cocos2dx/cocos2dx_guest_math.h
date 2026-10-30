#pragma once
#include <array>
#include <cstdint>
#include <string_view>
namespace pv7::compat::cocos2dx {
// Base AAPCS (soft-float / softfp ABI) Bionic libm bridge.
// Double inputs occupy aligned r0:r1 (and r2:r3), and return in r0:r1.
// Single-precision inputs and outputs occupy 32-bit integer registers.
// This is NOT the VFP-register hard-float calling convention.
bool DispatchGuestMath(std::string_view name,
                       std::array<std::uint32_t,16>& registers);
}