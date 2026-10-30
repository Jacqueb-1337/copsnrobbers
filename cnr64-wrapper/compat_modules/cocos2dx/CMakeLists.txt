cmake_minimum_required(VERSION 3.20)
project(pv7_cocos2dx_contract LANGUAGES CXX)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
add_library(pv7_cocos2dx_contract STATIC cocos2dx_compat_module.cpp cocos2dx_jni_marshaller.cpp cocos2dx_bitmap_pixels.cpp cocos2dx_guest_jni_arrays.cpp cocos2dx_jni_refs.cpp cocos2dx_guest_math.cpp)
add_executable(pv7_cocos2dx_contract_test cocos2dx_compat_module_test.cpp)
target_link_libraries(pv7_cocos2dx_contract_test PRIVATE pv7_cocos2dx_contract)
enable_testing()
add_test(NAME cocos2dx_method_contract COMMAND pv7_cocos2dx_contract_test)

# Engine-neutral ELF mapper runs inside an ARM64 Android process: it maps
# ARM32 guest bytes into data memory and never dlopens the guest image.
add_library(pv7_cocos2dx_elf STATIC
    elf/arm32_guest_image.cpp
    elf/arm32_guest_relocations.cpp
)
target_include_directories(pv7_cocos2dx_elf PUBLIC elf)
set_target_properties(pv7_cocos2dx_elf PROPERTIES POSITION_INDEPENDENT_CODE ON)

add_executable(pv7-cocos-elf-smoke android/pv7_cocos_elf_smoke.cpp)
target_link_libraries(pv7-cocos-elf-smoke PRIVATE pv7_cocos2dx_elf)