#include "arm32_guest_image.h"
#include <fstream>
#include <iomanip>
#include <iostream>
#include <iterator>
#include <map>
#include <optional>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc < 2 || argc > 3) {
        std::cerr << "Usage: pv7-arm32-elf-probe libgame.so [--audit-relocations]\n";
        return 2;
    }
    const bool audit = argc == 3 && std::string(argv[2]) == "--audit-relocations";
    if (argc == 3 && !audit) {
        std::cerr << "Unknown argument\n";
        return 2;
    }
    std::ifstream file(argv[1], std::ios::binary);
    if (!file) {
        std::cerr << "Cannot read guest library\n";
        return 1;
    }
    std::vector<std::uint8_t> data(std::istreambuf_iterator<char>{file}, {});
    pv7::Arm32GuestImage guest;
    std::string error;
    if (!guest.Load(data, 0x10000000, error)) {
        std::cerr << "ELF mapping failed: " << error << "\n";
        return 1;
    }
    std::cout << "Mapped ELF32 guest, span=" << guest.span() << " bytes\n";
    for (const auto& dep : guest.dependencies()) {
        std::cout << "NEEDED " << dep << "\n";
    }
    for (const auto* name : {
        "JNI_OnLoad",
        "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeInit",
        "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender"
    }) {
        auto symbol = guest.FindExport(name);
        if (symbol) {
            std::cout << "EXPORT " << name << " @0x" << std::hex
                      << *symbol << std::dec << "\n";
        }
    }
    if (!audit) return 0;

    // Diagnostic audit ONLY: placeholder values model resolved guest imports,
    // NOT executable host thunks. No ARM instructions are executed here.
    // A real backend must individually resolve each import to guest-callable
    // JNI, libc, EGL, OpenGL ES, audio, filesystem, etc. implementations.
    std::map<std::string, std::uint32_t> imports;
    auto mock = [&](std::string_view name) -> std::optional<std::uint32_t> {
        const std::string key(name);
        auto it = imports.find(key);
        if (it != imports.end()) return it->second;
        if (imports.size() >= 100000) return std::nullopt;
        std::uint32_t address = 0x70000000u
            + static_cast<std::uint32_t>(imports.size()) * 16;
        imports.emplace(key, address);
        return address;
    };
    if (!guest.Relocate(mock, error)) {
        std::cerr << "RELOCATION_AUDIT_FAILED: " << error << "\n";
        return 1;
    }
    std::cout << "RELOCATION_AUDIT_PASS (simulated import addresses only)\n";
    std::cout << "EXTERNAL_IMPORT_COUNT " << imports.size() << "\n";
    std::map<std::string, unsigned> typed;
    for (const auto& entry : guest.imported_symbols()) {
        typed.emplace(entry.name, entry.symbol_type);
    }
    unsigned functions=0, objects=0, other=0;
    for (const auto& entry : typed) {
        if (entry.second == 2) ++functions;
        else if (entry.second == 1) ++objects;
        else ++other;
    }
    std::cout << "IMPORTED_FUNCTIONS " << functions << "\n";
    std::cout << "IMPORTED_OBJECTS " << objects << "\n";
    std::cout << "IMPORTED_OTHER " << other << "\n";
    for (const auto& entry : imports) {
        std::cout << "IMPORT " << entry.first << "\n";
    }
    std::cout << "CONSTRUCTOR_COUNT " << guest.constructors().size() << "\n";
    return 0;
}
