#include "arm32_guest_image.h"
#include <cassert>
#include <fstream>
#include <iostream>
#include <iterator>

int main(int argc, char**argv) {
    assert(argc == 2);
    std::ifstream file(argv[1], std::ios::binary);
    std::vector<std::uint8_t> bytes(std::istreambuf_iterator<char>{file}, {});
    pv7::Arm32GuestImage image;
    std::string error;
    assert(image.Load(bytes, 0x10000000u, error));
    assert(image.dependencies().size() == 1);
    assert(image.dependencies()[0] == "liblog.so");
    assert(image.FindExport("JNI_OnLoad") == 0x10000180u);
    assert(image.FindExport("Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeInit") == 0x10000190u);
    assert(!image.Relocate({}, error)); // Unknown symbols must NOT be silently faked.
    assert(error.find("eglMakeCurrent") != std::string::npos);
    assert(image.Load(bytes, 0x10000000u, error));
    assert(image.Relocate([](std::string_view name) -> std::optional<std::uint32_t> {
        if (name == "eglMakeCurrent") return 0x78000120u;
        return std::nullopt;
    }, error));
    assert(image.imported_symbols().size() == 1);
    assert(image.imported_symbols()[0].name == "eglMakeCurrent");
    assert(image.imported_symbols()[0].symbol_type == 2); // STT_FUNC
    assert(image.imported_symbols()[0].relocation_type == 21); // R_ARM_GLOB_DAT
    assert(image.ReadWord(0x10000500u) == 0x10000040u);
    assert(image.ReadWord(0x10000504u) == 0x78000120u);
    assert(image.ReadWord(0x10000360u) == 0x10000120u);
    assert(image.constructors().size() == 1);
    assert(image.constructors()[0] == 0x10000120u);
    std::cout << "PASS ELF32/ARM image mapping, exports, relocation, constructors\n";
}
