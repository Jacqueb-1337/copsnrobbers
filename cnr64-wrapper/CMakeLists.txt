cmake_minimum_required(VERSION 3.22)
project(projectv7_wrapper LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# ProjectV7 production builds must not contain the live native hotpatch loader.
# Enable this only in a dedicated developer build used to confirm compatibility
# fixes before baking them into the normal bridge/runtime implementation.
option(PROJECTV7_DEV_HOTPATCH "Enable developer-only live compatibility hotpatching" OFF)
option(PROJECTV7_DEV_DIAGNOSTICS "Enable verbose developer compatibility tracing" OFF)
set(PROJECTV7_COMPAT_MODULES "" CACHE STRING "Semicolon-separated embedded compatibility modules")

set(DYNARMIC_FRONTENDS "A32" CACHE STRING "" FORCE)
set(DYNARMIC_TESTS OFF CACHE BOOL "" FORCE)
set(DYNARMIC_USE_LLVM OFF CACHE BOOL "" FORCE)
set(DYNARMIC_USE_PRECOMPILED_HEADERS OFF CACHE BOOL "" FORCE)
set(DYNARMIC_USE_BUNDLED_EXTERNALS ON CACHE BOOL "" FORCE)
set(DYNARMIC_WARNINGS_AS_ERRORS OFF CACHE BOOL "" FORCE)

if(ANDROID)
    set(DYNARMIC_ENABLE_NO_EXECUTE_SUPPORT ON CACHE BOOL "" FORCE)
endif()

set(BOOST_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/third_party/boost_1_85_0" CACHE PATH "" FORCE)
set(Boost_INCLUDE_DIR "${BOOST_ROOT}" CACHE PATH "" FORCE)
set(Boost_NO_SYSTEM_PATHS ON CACHE BOOL "" FORCE)
set(Boost_NO_BOOST_CMAKE ON CACHE BOOL "" FORCE)

add_subdirectory(third_party/dynarmic dynarmic-build EXCLUDE_FROM_ALL)

set(PROJECTV7_CORE_SOURCES
    src/a32_test_core.cpp
    src/elf_guest_probe.cpp
    src/unity_guest_probe.cpp
    src/shared_guest_linker.cpp
    src/pv7_compat_module_registry.cpp
)

set(PV7_COMPAT_MODULE_CNR 0)
foreach(PV7_COMPAT_MODULE IN LISTS PROJECTV7_COMPAT_MODULES)
    if(PV7_COMPAT_MODULE STREQUAL "cnr")
        set(PV7_COMPAT_MODULE_CNR 1)
        list(APPEND PROJECTV7_CORE_SOURCES
            compat_modules/cnr/cnr_compat_module.cpp
        )
    elseif(NOT PV7_COMPAT_MODULE STREQUAL "")
        message(FATAL_ERROR "Unknown ProjectV7 compatibility module: ${PV7_COMPAT_MODULE}")
    endif()
endforeach()

if(PROJECTV7_DEV_HOTPATCH)
    list(APPEND PROJECTV7_CORE_SOURCES
        devtools/hotpatch/hotpatch_runtime.cpp
    )
endif()

add_library(projectv7-core STATIC ${PROJECTV7_CORE_SOURCES})
add_library(cnr64-test-core ALIAS projectv7-core)
target_link_libraries(projectv7-core PUBLIC dynarmic)
target_include_directories(projectv7-core PUBLIC src)
target_compile_definitions(projectv7-core PRIVATE
    PROJECTV7_DEV_HOTPATCH=$<BOOL:${PROJECTV7_DEV_HOTPATCH}>
    PROJECTV7_DEV_DIAGNOSTICS=$<BOOL:${PROJECTV7_DEV_DIAGNOSTICS}>
    PV7_COMPAT_MODULE_CNR=${PV7_COMPAT_MODULE_CNR}
)
if(PROJECTV7_DEV_HOTPATCH)
    target_include_directories(projectv7-core PRIVATE devtools/hotpatch)
endif()
set_target_properties(projectv7-core PROPERTIES POSITION_INDEPENDENT_CODE ON)

add_executable(cnr64-a32-poc
    src/a32_poc.cpp
)
target_link_libraries(cnr64-a32-poc PRIVATE projectv7-core)

if(ANDROID)
    # Generic Project V7 host library used by reusable wrapper templates.
    # Compatibility modules are selected at configure time through
    # PROJECTV7_COMPAT_MODULES; an empty list produces a game-agnostic host.
    add_library(pv7host SHARED
        src/jni_bridge.cpp
    )
    target_link_libraries(pv7host PRIVATE projectv7-core log android EGL GLESv2 OpenSLES z dl)
    set_target_properties(pv7host PROPERTIES OUTPUT_NAME "pv7host")

    # Transitional legacy target for the existing CNR wrapper while it is
    # migrated onto the generic app.projectv7.wrapper host template.
    add_library(cnr64poc SHARED
        src/jni_bridge.cpp
    )
    target_link_libraries(cnr64poc PRIVATE projectv7-core log android EGL GLESv2 OpenSLES z dl)
    set_target_properties(cnr64poc PROPERTIES OUTPUT_NAME "cnr64poc")
endif()
