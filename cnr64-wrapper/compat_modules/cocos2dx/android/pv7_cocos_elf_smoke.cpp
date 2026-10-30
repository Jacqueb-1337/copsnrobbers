#include "../elf/arm32_guest_image.h"
#include <cstdint>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

// Maps the real ARM32 ELF in data memory on an ARM64 host. Does not run it.
int main(int argc, char** argv) {
    if (argc != 2) {
        std::cerr << "usage: pv7-cocos-elf-smoke libgame.so" << std::endl;
        return 2;
    }
    std::ifstream file(argv[1], std::ios::binary | std::ios::ate);
    if (!file) return 3;
    const auto size = file.tellg();
    if (size <= 0 || size > 192 * 1024 * 1024) return 4;
    file.seekg(0);
    std::vector<std::uint8_t> bytes(static_cast<std::size_t>(size));
    if (!file.read(reinterpret_cast<char*>(bytes.data()), size)) return 5;
    pv7::Arm32GuestImage image;
    std::string error;
    if (!image.Load(bytes, 0x02000000u, error)) {
        std::cerr << "PV7_COCOS_ELF_ERROR " << error << std::endl;
        return 6;
    }
    std::cout << "PV7_COCOS_ARM32_IMAGE_OK" << std::endl;
    std::cout << "mapped_span=" << image.span() << std::endl;
    for (const auto& dep : image.dependencies())
        std::cout << "needed=" << dep << std::endl;
    for (const auto* symbol : {"JNI_OnLoad",
          "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeInit",
          "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender"}) {
        auto address = image.FindExport(symbol);
        std::cout << symbol << "=";
        if (address) std::cout << "0x" << std::hex << *address << std::dec;
        else std::cout << "not-exported";
        std::cout << std::endl;
    }
    std::cout << "constructors=" << image.constructors().size() << std::endl;
    return 0;
}