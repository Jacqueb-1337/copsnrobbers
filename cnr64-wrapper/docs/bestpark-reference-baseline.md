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

## Previously observed reference blocker

App startup logs report a nonexistent OBB expansion directory:
 /storage/emulated/0/Android/obb/com.turner.bestparkintheuniverse

The legacy Google Play expansion downloader and license checker ran,
but the licensing service returned contact errors. The screen remained
black. No matching standalone .obb file was located in initial local
search. Do not call this a PV7 rendering defect.

A strace attachment observed only one blocked syscall and therefore
does not establish comprehensive native-call coverage.


## Expansion data located; game menus and story verified

The owner identified an additional archive in Downloads:
08093-Best-Park-v1-2-1-cache.zip (373,361,872 bytes).
The archive has three entries, including:

- com.turner.bestparkintheuniverse/main.16.com.turner.bestparkintheuniverse.obb
- OBB size: 388,494,611 bytes
- Extracted OBB SHA-256: C59062A368C06731D29B5CE03D8B4D725C146008A9C168606F85569286C3AB44

The extracted entry was copied unchanged to the isolated reference AVD:
 /sdcard/Android/obb/com.turner.bestparkintheuniverse/main.16.com.turner.bestparkintheuniverse.obb

The game's real logs now say "expansion file is ZipResourceFile",
"isDownloading false", "exists? true", then display the complete OBB
path. Despite subsequent legacy Play licensing errors, the original
ARMv7 game renders its animated title screen, responds to Play,
advances through the introductory story, reaches Select Destination,
and opens Select 2 Characters for level 1. Gameplay itself has not
yet been conclusively reached or validated.

This test uses the previously described locally DEX-normalized APK,
not the untouched original package. The original ZIP, OBB entry, APK
and game library were preserved; no licensed game assets are committed.

After restarting the AVD, ADB needed an explicit "adb root" again to
read /proc/<game-pid>/maps. A subsequent collector run succeeded
and wrote local evidence to:
 C:/Users/Jacqueb/AppData/Local/Temp/pv7-bestpark-obb-working-baseline-root/

This confirms functional ARM32 reference execution and asset loading,
but is not proof that the PV7 Cocos host can execute this game or that
all native function calls have been instrumented.


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

## Real native API interception (Frida)

A matching Frida 17.24.3 Python client and Android x86 server were installed
for the isolated Android 9 reference emulator. An actual attach to the
running Best Park process succeeded. An exploratory set of 19 exposed
libndk_translation thunk functions yielded zero calls while attached; those
thunks do not cover all guest-to-host interactions.

Tracing **35 Android host native API entry points** in libc, libdl, liblog,
libEGL, and libGLESv2 produced real live event callbacks:

- 12-second run: 210,703 host API calls observed; first 15,000 calls and
  corresponding returns saved as 30,000 JSONL events; 195,703 later calls
  were counted but not individually serialized due to the size cap.
- Hooked examples observed: read, write, fstat, close, clock_gettime,
  pthread_mutex_lock/unlock, eglSwapBuffers, glClear, glBindTexture,
  and glDrawArrays.
- Return addresses for glClear, glBindTexture, and glDrawArrays mapped
  to the **libndk_translation_proxy_libGLESv2.so** module, supporting
  guest ARM graphics calls passing through the x86 bridge.
- Other return sites mapped to ordinary Android system/vendor libraries;
  these are process-level observations, not necessarily calls originating
  directly in the game library.
- Reproducible version was added as
  tools/pv7_reference_frida_agent.js and
  tools/pv7_reference_frida_trace.py.
- The checked-in scripts were tested by fetching them back to the PC
  and running a 3-second live capture: 32,595 API calls observed,
  15,000 entry and return pairs saved, 17,595 additional calls counted.

The logs, counts, and summaries remain **local-only** under the host's
temporary pv7-frida folder. No raw traces, game assets, or APKs
were committed. These 35 host hooks are a confirmed first real native
boundary trace, NOT an exhaustive recording of all 323 ARM imports,
internal libgame functions, JNI, or every native instruction.

Next: expand to an import-driven set of proxy/host API interceptors,
correlate return addresses and threads to bridge/module origin, add
game-initiated JNI hooks, then instrument guest ARM function-level
control flow through the actual translator or controlled PV7 Dynarmic
execution. Limit per-event recording to avoid unbounded log growth
and avoid interpreting untyped pointer registers as known signatures.

## Direct ARM32 guest and internal function tracing (Oct 29)

A separate genuine ARM32 Android 4.4.2 emulator was installed on C:
in the owner's isolated PV7 ARM32Classic folder. It uses Google's
archived Android Emulator 28.0.25 emulator-arm.exe (32-bit host), a
compact private data image and SD card with Best Park's matching OBB,
and software OpenGL ES 2. ADB serial is emulator-5580; existing
Android 16 emulator was not changed.

Legacy Frida 12.4.8 ARM Android server and an isolated portable
Python 3.7 client successfully attached to the actual ARM32 game process.
Cocos JNI initialization and nativeRender were observed. Frida Stalker
produced zero call events and coincided with an unresponsive guest, so it
remains disabled; exact causation was not established.

The game library exports 15,115 function symbols. The new tools
pv7_arm32_function_agent.js and pv7_arm32_function_trace.py use bounded
Interceptor hooks on selected exported ARM functions. They record
calls, return values, raw register-like arguments, thread ids, caller
addresses, caller module and library-relative offsets. Pointers are
NOT decoded, and no internal functions are invoked artificially.

Verified on the running ARM guest:

- 80 hooks installed with zero setup failures.
- 12-second baseline: 668 calls each to Cocos2dxRenderer.nativeRender,
  CCDirector.sharedDirector and CCDirector.drawScene; 2004 total counted
  across those three symbols.
- 39 sampled call events and 39 corresponding returns retained with
  no recording errors.
- drawScene's recorded caller is inside libgame.so while nativeRender's
  caller maps to Android libdvm.so.
- A second test using the committed collector against the same running
  ARM guest returned 80 hooks, 30 sampled calls, 30 sampled returns
  and zero errors.
- All raw game and tracing evidence remains on the owner's PC, not
  uploaded to GitHub. Source scripts, tests and docs are committed.

The CLI writes a JSONL trace plus a per-symbol count and caller summary:

  python pv7_arm32_function_trace.py --seconds 12 --output new-trace.jsonl

Before invocation, run the legacy Frida server in the guest and
forward the local collector port 27044 to guest port 27042. The local
ARM32Classic README-STATUS.md explains the exact boot setup.

These are true observed guest ARM32 function calls, NOT exhaustive
coverage of all 15,115 symbols, all indirect calls or every code path.
The next pass is to add selector groups and exercise more gameplay
scenes while comparing the captured return-site offsets to static ELF
disassembly.


## Multi-profile tracing verified

The ARM32 Frida Interceptor tracer now supports render, scene, assets, input and gameplay profiles, bounded hooks and samples, and a timed ADB tap with phase markers. It does not invoke native functions or use Stalker.

On a real emulator input test, 31 hooks succeeded, zero failed, five native functions fired (11 sampled calls with 11 returns): Cocos2dxRenderer.nativeTouchesBegin/End, CCMenu.ccTouchBegan/Ended and CCLayer.setTouchMode. The verified tap at 249,253 advanced Best Park from title to opening cutscene.

While advancing the cutscene, the assets profile hooked 65 exported ARM32 functions and recorded SoundPlayer.playSound, CCTexture2D.initWithData and CCTexture2D.initWithString, three call/return pairs without errors.

The earlier scene profile hooked 70 functions but captured no scene callbacks, and the gameplay profile hooked 72 of 75 functions, with three explicit Frida interception errors (BossRoom.addXpToPlayer, HordeRoom.addXpToPlayer, PlayerComboSystem.update). No gameplay-profile events fired while in the cutscene. Neither establishes missing game functionality. The selection code now prioritizes game-specific functions for scene and gameplay; this last prioritization passed CI but awaits another live profile test.

Original APK/OBB and all raw trace data remain on user's PC. This is selected native export-call tracing, not exhaustive instrumentation of all game function paths.

## Remaining milestones

1. DONE: Find expansion data and verify native menus, cutscene, and level selection.
   NEXT: Validate actual level gameplay, input, audio, and rendering.
2. DONE: Trace selected internal ARM32 exported functions without x86 translation.
   NEXT: expand function selections and reproduce more gameplay scenes.
3. Connect Arm32TraceRecorder to the separate Cocos Dynarmic backend.
4. Compare synchronized startup, graphics, files, JNI, and input traces.
5. Validate PV7 separately without changing the existing Unity/CNR path.

Production PV7 Cocos support remains disabled.
