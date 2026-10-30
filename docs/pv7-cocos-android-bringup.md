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
- ARM64 `PROJECTV7_COMPAT_MODULES=cocos2dx` full `pv7host` shared-library
  target compiles and links, verified with Android NDK 28. The original build
  failure came from incompatible JNI bridge/core snapshots in the isolated
  checkout. The aligned native bridge, input queue, Unity probe, binary
  tracing, storage JNI helpers, URL handler and thread-pump configuration
  are now compiled together without placeholder JNI functions.
- A successful host link proves native integration only. Cocos JNI_OnLoad,
  nativeInit, nativeRender, actual first frame, and OBB loading still require
  device-side guest execution tests before a playable claim is appropriate.
- `scripts/local_cocos_host_smoke.py` repacks host libraries into a test-only
  APK template. The output must be zipaligned and signed with a development
  keystore before emulator installation. Do not publish such builds as a
  production Cocos compatibility release.
## Additional Android host verification

- With `PROJECTV7_COMPAT_MODULES=cocos2dx`, both complete Android ARM64 and
  x86_64 native shared libraries link successfully using NDK 28.
- A local, debug-signed diagnostic wrapper was assembled by replacing both
  library entries in the v1.36 host template. `zipalign` and `apksigner`
  completed, and APK signing verified using schemes v2 and v3.
- The diagnostic wrapper installed on the existing x86_64 emulator and
  launched `app.projectv7.wrapper.MainActivity`. Android's native loader
  confirmed `libpv7host.so ... ok`; the process stayed alive, created a
  rendering surface and produced no AndroidRuntime fatal exceptions.
- This particular smoke-test APK intentionally contains **zero guest payload
  bytes**, so no Best Park ARM32 instructions or original Cocos JNI methods
  ran. The result verifies native loading/packaging and UI lifecycle only.
- Build artifacts are local and test-only. No production APK released and no
  Project V7 Cocos compatibility gate changed.
## Real Best Park ARM32 execution on Android (2026-10-30)

An optional, developer-only `pv7-cocos-firstcall-android` executable is
compiled from the preexisting desktop Dynarmic Cocos first-call diagnostic,
linked against the Cocos-enabled core and a genuine Android EGL pbuffer and
GLES2 context. The original game binary is supplied separately at runtime.
Unknown JNI/native imports fail closed instead of returning fictitious success.

On an Android x86_64 emulator, the program executed Best Park 1.2.1's
unchanged ARM32 `libgame.so` through Dynarmic. The first direct `JNI_OnLoad`
test returned JNI version `0x00010004` with exit code zero.

The subsequent `boot` diagnostic successfully ran 324 ELF constructors,
executed `JNI_OnLoad`, entered original `nativeInit`, and compiled/linked
multiple real GLES2 shader programs in Android's GL context. It stopped
intentionally at Best Park's first
`org/cocos2dx/lib/Cocos2dxBitmap.createTextBitmap` request, which needs the
actual Android Paint/Canvas renderer to forward pixels into the original
ARM32 guest `nativeInitBitmapDC` callback:

```text
ALL_CONSTRUCTORS_RETURNED count=324
GUEST_RETURN r0=0x10004
EXECUTING_COCOS_NATIVE_INIT
HOST_GLES_LINK program=22 success=1
COCOS_BITMAP_TEXT_REQUEST text=00.0 font=Arial size=24 align=19 dimensions=0x0
COCOS_BITMAP_ANDROID_RENDER_BRIDGE_NOT_CONNECTED
```

The `boot` diagnostic exits 3 at this *known unimplemented boundary*.
Do not interpret the return code as a crash or game success. The separately
written Android Java bitmap renderer compiles but is not connected to this
native executable, which has no Java VM. The next integration needs a JNI
call into the Android host Activity/Java bitmap renderer, followed by the
real guest bitmap callback. Once connected, proceed to asset/OBB file
mapping and a real `nativeRender` frame. Do not use placeholder pixels.
No production guest APK support or patcher change is claimed.
## Android bitmap and expansion-file checkpoint

The isolated Android Activity diagnostic now uses the platform Paint/Canvas
renderer and JNI to transfer native-order ARGB pixels into the original ARM32
`nativeInitBitmapDC`. On an Android x86_64 emulator, all six actual text
bitmap guest callbacks completed, and GLES2 uploaded the resulting textures.

The original Best Park 1.2.1 classes.dex was inspected directly: its
`regularshow.getAPKExpansionFileName()` checks external Android/obb storage
for `main.16.com.turner.bestparkintheuniverse.obb` and returns the existing
absolute filename, or an empty string. This is not an arbitrary OBB guess.
The developer-only Activity resolves the matching filename in its own
`getObbDir()` and forwards that actual path through the guest JNI static
object-call and string UTF accessors.

The corresponding original 388,494,611-byte OBB was extracted from the
user-supplied cache ZIP and copied to the emulator's diagnostic-app OBB
directory, outside version control. The diagnostic successfully opened it
with the restricted read-only guest VFS, and executed original guest read
calls; the initial 2-million-tick budget expired while parsing it.
A bounded larger budget and sparse read logging have been staged for testing.

The file bridge only permits read-only access to the exact current
Activity-resolved OBB filename; it does not expose other filesystem paths.
No game assets or APKs are committed. A packaged diagnostic is NOT a PV7
Patcher-generated playable game and does not yet support complete Cocos
lifecycle, input, audio, or nativeRender.
- After successful OBB open and original ARM32 reads, guest initialization reached a bounded diagnostic tick limit. With sparse read tracing and a larger bounded budget it next revealed a missing memchr import. A bounded guest-address-space memchr implementation has since compiled for ARM64 and x86_64, with follow-up runtime verification pending.
