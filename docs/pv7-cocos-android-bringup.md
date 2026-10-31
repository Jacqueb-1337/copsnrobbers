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
- After successful OBB open and original ARM32 reads, guest initialization reached a bounded diagnostic tick limit. With sparse read tracing and a larger bounded budget it next revealed a missing memchr import. A bounded guest-address-space memchr implementation has since compiled for ARM64 and x86_64, and subsequently passed the emulator checkpoint.

## Subsequent parser verification
The follow-up Android Activity test of the real main expansion file first exposed an unsupported memchr import. Added a bounded guest-address-space memchr implementation for ARM32 Dynarmic import dispatch and built both ABIs. Retesting on the x86_64 Android emulator confirmed guest initialization continued past memchr and next stopped at vsnprintf (UNIMPLEMENTED_IMPORT:vsnprintf). This remains fail-closed rather than guessing the ARM varargs semantics. No playable wrapper or frame is claimed.
## Android resource and JNI continuation (October 30, 2026)

On the original Best Park ARM32 guest, `vsnprintf` first formatted the literal
"Get data from file(globals.xml) failed!" with no varargs. The emulator passed
that message to Android logging after a bounded guest-memory implementation.
The next actual `sprintf` format was verified against the original `libgame.so`
bytes as `%d-%d`; it returned three bytes for zero and zero. A shared bounded
guest 32-bit formatter now supports checked integer/string conversions for
`sprintf`, `snprintf` and `vsnprintf`; unknown conversions, 64-bit alignment,
and arbitrary host varargs remain deliberately unsupported.

Original `Cocos2dxHelper` methods were read from the supplied original DEX:
`getStringForKey` and `getIntegerForKey` access Activity SharedPreferences
`Cocos2dxPrefsFile`. Guest JNI slots for the observed static object and int
methods now forward to the diagnostic Activity's real preference store.
Emulator traces confirmed guest reads of character skills, dungeon settings,
and other keys, followed by verified `atoi` conversions.

The original APK contains `assets/globals.xml`, whereas the separate
version-16 OBB contains no globals.xml. The diagnostic APK builder now stages
the original base APK's 93 asset files from the caller-provided APK at build
time. These assets are NOT checked into Git. Staging assets does not by itself
establish that Cocos can access them: a native asset-manager/resource lookup
bridge remains necessary. The guest's own `getCurrentLanguage` method was
checked in DEX and returns `Locale.getDefault().getLanguage()`, reproduced by
the diagnostic host. No game frame, sound, input, or production-wrapper
playability is claimed. Stop reasons remain explicit in `cocos-probe.log`.
### Further observed Android checkpoints
The x86_64 Android diagnostic successfully performed `getIntegerForKey`,
returned Android locale language `en` from the game's own
`getCurrentLanguage()` JNI entry, formatted the missing English locale
resource warning with `vsnprintf`, and then executed an actual guest
`gettimeofday` bridge. The ARM32 timeval fields are explicitly checked
32-bit seconds/microseconds, and unsupported timezone pointers fail closed.
`org/cocos2dx/lib/Cocos2dxRenderer` class identification also passed;
the next observed (not yet handled) JNI method is
`setAnimationInterval(D)V`. Both Android native ABIs compiled and the
signed diagnostic APK was reinstalled and exercised. Original asset files
are now packaged in the local debug APK, but the native asset lookup is
still missing, so `globals.xml` and `locale/english.txt` loading are not
claimed working. This does not establish a playable game or nativeRender.
## Android Cocos APK, ZIP and writable-path checkpoint

The original DEX of Best Park was inspected: Cocos2dxHelper.init()
calls nativeSetApkPath(applicationInfo.sourceDir) before nativeInit,
assigns sAssetManager from Context.getAssets(), and sets
sFileDirectory to Context.getFilesDir().getAbsolutePath().
The ARM32 nativeSetApkPath export is now executed via Dynarmic
before nativeInit, using a staged guest JNI string. Real emulator logs
confirmed COCOS_GUEST_SET_APK_PATH_OK and COCOS_VFS_APK_OPEN_OK.

The diagnostic's confined read-only virtual filesystem now admits
only the resolved diagnostic APK, original version-16 expansion OBB,
actual app-private read paths, or virtual /sd/, /hd/, /sounds/, /ipad3/
resource paths. The latter are resolved through Android ZipFile against
the supplied OBB and, if absent, through original APK assets, with
relative-path checks, per-entry 16 MiB limits, and bounded cache
materialization. Emulator verified successful original guest fopen
calls for /sd/gui.ini, /sd/styles.ini and /sd/sounds.txt. These three
specific resources came from the original APK assets. The app-private
read-only check makes missing saves return fopen failure instead of
terminating at VFS_OPEN_NOT_AUTHORIZED. General write mode is not
supported by the diagnostic.

Added a bounded Android 64-bit zlib adapter for the original 56-byte
ARM32 z_stream layout. Guest inflateInit2_(-15), inflate(), CRC32,
and additional ZIP entries were verified on the Android emulator:
one 1,724-byte compressed stream yielded 7,311 bytes; a later
10,085-byte stream yielded 27,849 bytes, followed by CRC validation.
Host zlib state remains in stable host storage with validated guest
buffer pointers and is torn down on inflateEnd or bridge destruction.

Other tested progress includes strict optional libxml2 getenv values,
bounded ARM32 strtod/strtof/atof conversions with correct guest soft-float
register returns, getCocos2dxWritablePath through genuine Activity
files directory, and the Cocos renderer's setAnimationInterval JNI V-call
with AAPCS 8-byte-aligned double decoding. The original method stores
nanoseconds = (long)(1e9 * seconds); Android emulator confirmed a
0.0333333-second frame interval, not the original incorrect tiny value.

Increasing the post-bitmap guest instruction budget from 20 million to
100 million permitted deeper game configuration/mission parsing.
The latest Android x86_64 emulator stop after these bridges is
JNI_GetStaticMethodID_unknown:getBoolForKey(Ljava/lang/String;Z)Z on
org/cocos2dx/lib/Cocos2dxHelper.

All source and packaging scripts are developer-only. Original game APK,
OBB and extracted private resource files are not tracked in Git.
Both Android ABI probes compile and a local signed debug diagnostic APK
was rebuilt and installed on the emulator. There is still no verified
nativeInit return, nativeRender loop, first game frame, audio or
controller input, and production PV7 Patcher is unchanged.

## Android boolean preferences, PNG and TinyXML2 checkpoint

Verified against original DEX: Cocos2dxHelper getBoolForKey reads
SharedPreferences.getBoolean and setBoolForKey synchronously commits
an Editor.putBoolean, both in Cocos2dxPrefsFile. Android emulator
confirmed guest muted=false read and write via exact JNI method
signatures, covering ARM32 direct, V and A call forms.

The original guest then opened textures.ini and gui/common.png, and
used setjmp for normal libpng operation. A diagnostic-only setjmp
checkpoint allows the normal zero-return path with validated guest
buffer identity; untested longjmp recovery remains explicitly
unsupported. Original game code decoded a real 512x512 RGBA texture
(1048576 bytes) and uploaded it through Android GLES2.

Read-only VFS now permits fopen mode rt. The original guest opened
/sd/gui/common.plist in that mode, accessing its 13911-byte resource
without granting write access. Repetitive CRC32 trace output is capped.

A post-bitmap guest budget increase from 100M to 400M ticks did not
complete nativeInit. A diagnostic register snapshot places the halt
in the original TinyXML2 XMLUtil::StringEqual, Thumb code at
guest PC 0x0122790c (ELF relative 0x22790c), with r0=0x3c, r2=2,
r4=0 and r5=0x3f. This indicates repeated short comparisons during
XML processing, but does not yet prove why parser progress stalls.
Next inspect caller, input pointer and file advancement. Do not
merely keep raising the instruction budget.

ARM64 and x86_64 diagnostic native builds pass. Android x86_64 debug
APK was signed, installed and exercised. No nativeInit return,
nativeRender, first game frame or production PV7 support is claimed.

## October 31: TinyXML2 parser no-progress diagnostic

Instrumented the original ARM32 guest's 400-million-tick initialization with
bounded 25-million-tick samples of register state, the active import thunk,
guest input bytes, and stack return-address candidates. The repeated import
is **strlen("<")** from TinyXML2 StrPair::ParseText. The guest XML input
pointer stays at the start of `<?xml version="1.0"...` while guest working
heap addresses increase; the stack pointer is stable, indicating an iterative
loop rather than unbounded recursion.

Ran four direct test calls to the *original* exported ARM32
XMLUtil::StringEqual under Dynarmic. The comparisons for matching and
mismatching prefixes all returned expected values. This confirms the basic
comparison helper on those test vectors, but not full parser control-flow
correctness. The 13,911-byte original common.plist parses successfully via
the independent host plist parser. A local-only, equal-size resource copy
without its XML prolog and DOCTYPE made the guest advance to `<plist` but
the same repeated parsing still occurred. The original resource was restored
after the experiment; no modified original-game content was committed.

Added a fail-closed developer-only guard at the original
StrPair::ParseText -> strlen("<") import boundary, identifying repeated
identical guest input pointers and aborting after 8,192 re-entries.
**Verified on the Android x86_64 emulator**: after opening common.plist,
the probe logged `TINYXML2_GUEST_INPUT_NOT_ADVANCING` with 8,193 repeats
and returned `COCOS_NATIVE_INIT_STOP TINYXML2_REPEATED_PARSE_INPUT`
instead of exhausting 400 million translated ticks or consuming the
guest heap. Both Android ARM64 and x86_64 probe libraries compiled.
The exact bug in guest TinyXML2 node/cursor progression remains unproven;
next inspect XMLDocument::Identify / XMLNode::ParseDeep return pointers,
rather than bypassing the parser or claiming a playable game.
