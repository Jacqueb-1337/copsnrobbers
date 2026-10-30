#pragma once

#include <cstdint>
#include <string>
#include <vector>

#include "pv7_compat_module_api.h"

namespace pv7::compat {

// Replaces the active runtime compatibility module with a small declarative
// properties payload supplied by the wrapper APK. Passing an empty string clears it.
void ConfigureRuntimeModule(const std::string& config_text);

// Returns nullptr when no runtime/embedded compatibility module supplies a guest
// package identity. The core remains usable without any game-specific module.
const char* GuestPackageName();
const char* GuestVersionName();
bool SupportsHostRestart();

bool NormalizeGuestPath(const std::string& path,
                        const std::string& managed_dir,
                        std::string& result);

bool GetSystemProperty(const std::string& name, std::string& value);

std::vector<Pv7GuestInstructionPatchV1> StartupInstructionPatches();


bool LookupGuestAddress(const std::string& name, std::uint32_t& address);

std::string EmbeddedModuleSummary();

} // namespace pv7::compat
