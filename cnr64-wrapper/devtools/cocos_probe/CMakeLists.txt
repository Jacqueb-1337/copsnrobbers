cmake_minimum_required(VERSION 3.22)
project(pv7_cocos_firstcall_smoke LANGUAGES C CXX)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Test-only host executable. Does NOT package an Android wrapper.
set(DYNARMIC_FRONTENDS "A32" CACHE STRING "" FORCE)
set(DYNARMIC_TESTS OFF CACHE BOOL "" FORCE)
set(DYNARMIC_USE_LLVM OFF CACHE BOOL "" FORCE)
set(DYNARMIC_USE_PRECOMPILED_HEADERS OFF CACHE BOOL "" FORCE)
set(DYNARMIC_WARNINGS_AS_ERRORS OFF CACHE BOOL "" FORCE)
set(DYNARMIC_USE_BUNDLED_EXTERNALS ON CACHE BOOL "" FORCE)

set(PV7_THIRD_PARTY "D:/Projects/copsnrobbers/cnr64-wrapper/third_party" CACHE PATH
    "Path to a local third_party directory containing dynarmic and boost_1_85_0")
if(NOT EXISTS "${PV7_THIRD_PARTY}/dynarmic/CMakeLists.txt")
    message(FATAL_ERROR "Supply -DPV7_THIRD_PARTY=<path> with dynarmic source and Boost 1.85.0")
endif()
set(BOOST_ROOT "${PV7_THIRD_PARTY}/boost_1_85_0" CACHE PATH "" FORCE)
set(Boost_INCLUDE_DIR "${BOOST_ROOT}" CACHE PATH "" FORCE)
set(Boost_NO_SYSTEM_PATHS ON CACHE BOOL "" FORCE)
add_subdirectory("${PV7_THIRD_PARTY}/dynarmic" dynarmic)

add_library(pv7elf STATIC src/arm32_guest_image.cpp src/arm32_guest_relocations.cpp)
target_include_directories(pv7elf PUBLIC src)
add_executable(pv7-cocos-firstcall src/pv7_cocos_firstcall.cpp
  ../../compat_modules/cocos2dx/cocos2dx_jni_marshaller.cpp
  ../../compat_modules/cocos2dx/cocos2dx_bitmap_pixels.cpp
  ../../compat_modules/cocos2dx/cocos2dx_guest_jni_arrays.cpp
  ../../compat_modules/cocos2dx/cocos2dx_jni_refs.cpp
  ../../compat_modules/cocos2dx/cocos2dx_guest_math.cpp
  src/pv7_cocos_probe_bitmap_win32.cpp)
target_link_libraries(pv7-cocos-firstcall PRIVATE pv7elf dynarmic)

target_include_directories(pv7-cocos-firstcall PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})

if(WIN32)
  target_link_libraries(pv7-cocos-firstcall PRIVATE gdi32)
endif()
