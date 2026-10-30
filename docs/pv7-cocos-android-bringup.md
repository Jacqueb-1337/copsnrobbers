# PV7 Cocos2d-x Android bring-up

## Scope

This branch integrates the portable Cocos2d-x JNI method contracts, guest
JNI references and arrays, bitmap buffer marshalling, ARM32 softfp math
helpers and an ELF32 mapper into the **optional** `cocos2dx` native host
configuration.

It does not enable Best Park in the patcher's guest-backend policy. The
existing released Unity host and CNR module are unchanged. Neither ELF
mapping nor successful library compilation proves execution of
`JNI_OnLoad`, `nativeInit`, `nativeRender`, rendering or OBB loading.

## Device-side diagnostic

Configure the standalone module with Android NDK 28 and a desired
`ANDROID_ABI` (tested `arm64-v8a` and `x86_64`):

```sh
cmake -S cnr64-wrapper/compat_modules/cocos2dx -B build-cocos \
  -DCMAKE_TOOLCHAIN_FILE="$ANDROID_NDK_HOME/build/cmake/android.toolchain.cmake" \
  -DANDROID_ABI=x86_64 -DANDROID_PLATFORM=android-26
cmake --build build-cocos
adb push build-cocos/pv7-cocos-elf-smoke /data/local/tmp/
adb push /path/to/extracted/libgame.so /data/local/tmp/
adb shell chmod 755 /data/local/tmp/pv7-cocos-elf-smoke
adb shell /data/local/tmp/pv7-cocos-elf-smoke /data/local/tmp/libgame.so
```

This executable **reads** the original ARM32 ELF, maps PT_LOAD into
process-owned data and reports ELF dependencies and JNI symbol addresses.
It does not call any ARM32 guest instructions, and must not use host
`dlopen` on ARM32 libraries.

The method-contract executable `pv7_cocos2dx_contract_test` can also
run directly on an Android device or emulator.

## Integration into host native build

CMake's `PROJECTV7_COMPAT_MODULES=cocos2dx` option now compiles the
portable contracts and image mapper into the core native host for ARM64
builds. The Android bitmap renderer is kept as a separate Java source;
it is not yet bundled into the host template and cannot be dispatched
from the unmodified Android activity.

## Required before enabling the patcher build button

1. Port the desktop Dynarmic Cocos diagnostic execution loop to the
   production Android host, with bounded guest memory and ARM32 thunks.
2. Implement the Java-side Cocos activity/lifecycle bridge, JNI reference
   and method dispatch and guest-native callbacks, including the original
   bitmap callback.
3. Run `JNI_OnLoad`, then `nativeInit` and a real render loop on an
   Android EGL/GLES2 surface. Capture the first frame and full traces.
4. Resolve the game's OBB expansion-name callback, confined storage, input,
   audio, texture loads, and system imports. Preserve guest package identity.
5. Smoke-test Best Park on a physical ARM64 phone and regress the
   existing Unity/CNR wrapper before enabling Cocos in the patcher.

The standalone Windows GLES2/GDI six-bitmap diagnostic remains useful
evidence for the guest callback ABI, but is not yet the Android runtime.
No original game APK or native guest binaries belong in this repository.
## Validation record (2026-10-30)

- ARM64 NDK 28 and x86_64 NDK 28: standalone contract, ELF libraries and
  diagnostic executable compile successfully.
- Android x86_64 emulator: Best Park's untouched `libgame.so` maps, reports
  seven dependencies and its real JNI_OnLoad/nativeInit/nativeRender symbols.
  `pv7-cocos-elf-smoke` exited 0; portable contract test exited 0.
- Java Android bitmap renderer compiled against android-35.
- ARM64 `PROJECTV7_COMPAT_MODULES=cocos2dx` build: `projectv7-core`
  compiled successfully. **The final `pv7host` shared-library target did
  not compile** because this isolated checkout's legacy `jni_bridge.cpp`
  references a newer `RunA32SelfTest` signature, `SetGuestGameLoopRunning`
  and `HostTouchEvent` absent from its older `a32_test_core.h`.
  Do not ship a wrapper or claim full host compatibility until that mismatch
  is resolved against the correct baseline and the guest lifecycle is wired.