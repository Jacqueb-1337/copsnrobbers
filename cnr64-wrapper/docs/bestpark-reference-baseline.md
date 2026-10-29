# PV7 legacy Android reference baseline (October 29, 2026)

## Verified test environment

The Windows SDK has Android 7.1 ARMv7, Android 9 x86/x86_64,
and Android 16 ARM64/x86_64 system images.

A fresh PV7BestParkARMv7 AVD was created but the installed emulator
refused to start it: "CPU Architecture 'arm' is not supported by
the QEMU2 emulator."

A separate PV7BestParkX86Reference AVD was created and booted.
Its Android 9 API 28 system reports CPU ABIs x86,armeabi-v7a,armeabi
and native bridge libndk_translation.so. It supports root ADB and has
system strace. Existing Android9Rooted and Android9x86 AVDs were preserved.

The separately running Android 16 x86_64 emulator supports
x86_64,arm64-v8a and has a native bridge. No Best Park PV7 Cocos
wrapper run was attempted there.

## Owner-provided APK

Input is 08093-Best-Park-v1-2-1.apk. Original SHA-256:
EAA3030C98B72A59CA96BACAF258F96E28F215AF83512CA8FE07B8426DC16301.

Manifest metadata: com.turner.bestparkintheuniverse v1.2.1,
versionCode 16, minSdkVersion 14, native ABI armeabi.

The APK has classes.dex header DEX 036. Android 9 initially refused
to load its classes with "Unrecognized version number ... 0 3 6".
A private diagnostic COPY changed only that DEX header to 035 and
recalculated DEX SHA-1/Adler-32, then was debug-signed and installed
in the isolated emulator. The original APK and game library on disk
remained intact. This modified copy is not an unmodified reference and
its semantics are not guaranteed identical.

## Real ARMv7 native execution observed

The diagnostic APK started an Android process and /proc/maps verified
its lib/arm/libgame.so mapped along with ARM Android libraries and the
x86 native-bridge proxy libraries. Real app logs included:

- RegularShow: native init
- RegularShow: running shared application
- RegularShow: end new AppDelegate
- RegularShow: sharedapp->run
- RegularShow: native init END.

The emulator reported GLES 2 EGL context initialization. This proves
the ARMv7 game library ran far enough to reach Cocos JNI/native startup.
It does NOT prove playable rendering or that every imported API works.

## Reference blocker

App startup logs report a nonexistent OBB expansion directory:
 /storage/emulated/0/Android/obb/com.turner.bestparkintheuniverse

The legacy Google Play expansion downloader and license checker ran,
but the licensing service returned contact errors. The screen remained
black. No matching standalone .obb file was located in initial local
search. Do not call this a PV7 rendering defect.

A strace attachment observed only one blocked syscall and therefore
does not establish comprehensive native-call coverage.

## Repeatable local capture

The reusable cnr64-wrapper/tools/pv7_reference_collect.py collector
was tested on emulator-5560 using:

    python cnr64-wrapper/tools/pv7_reference_collect.py
      --serial emulator-5560
      --package com.turner.bestparkintheuniverse
      --output <new-local-evidence-folder>
      --duration 0

It successfully saved a versioned JSON report, logcat, maps, status,
and screenshot. Report counts: 199 distinct mapped .so module paths
and 11 matching native-related emitted log messages.

All raw evidence remains local to the owner's PC. Device logs can
contain private information and should not be automatically uploaded.
This collector does not claim to instrument every native function call.

## Remaining milestones

1. Find owner-provided original game expansion data and test real menus.
2. Add opt-in native API/import tracing to the reference runtime;
   x86 native translation complicates direct ARM instruction hooks.
3. Connect Arm32TraceRecorder to the separate Cocos Dynarmic backend.
4. Compare synchronized startup, graphics, files, JNI, and input traces.
5. Validate PV7 separately without changing the existing Unity/CNR path.

Production PV7 Cocos support remains disabled.
