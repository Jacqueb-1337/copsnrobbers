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

## October 31: first successful ARM32 nativeRender invocation

The isolated Android x86_64 diagnostic has now completed the original Best Park
1.2.1 ARM32 `nativeInit` and one original `nativeRender` invocation through
Dynarmic, with the original guest APK resources and version-16 OBB.

The original TinyXML2 code progressed without replacing the XML parser or
modifying the game's packaged resources. The developer-only probe exercises
the original guest JNI path and Android GLES2 pbuffer. Its bounded EGL
bridge supports ARM32 `glUniformMatrix4fv`, client-memory
`glVertexAttribPointer`, and `glDrawArrays`, including validation of guest
pointers, parameters, and host GL errors.

Verified Android emulator log:

```text
COCOS_NATIVE_INIT_RETURNED
COCOS_NATIVE_RENDER_BEGIN pc=0x121f0d9
HOST_GLES_DRAW_ARRAYS_OK mode=0x5 first=0 count=4 draw_calls=7
COCOS_NATIVE_RENDER_RETURNED
ANDROID_JAVA_PROBE_EXIT=0
```

The diagnostic performed seven successful original guest `GL_TRIANGLE_STRIP`
draw calls, then the original guest render entrypoint returned normally.
Both Android x86_64 and ARM64 probe library builds passed and the signed
debug diagnostic APK was installed and tested on x86_64.

This is a successful **first render invocation**, not a verified visible
frame or playable wrapper. The EGL surface is offscreen. No `glReadPixels`
visual validation, multiple consecutive frame loop, touch input, audio,
or physical ARM64 hardware execution has been verified. Production PV7
Patcher, Cocos compatibility gating, and shipped Unity/CNR wrapper remain
unchanged. Next: capture/validate the offscreen framebuffer, run consecutive
frames with proper lifecycle and timing, and route the guest render path to
the actual Android surface before attempting input/audio integration.

## October 31: real framebuffer capture and multi-frame intro execution

The x86_64 Android diagnostic now uses `glReadPixels` on the same
480x320 offscreen EGL/GLES2 context used by the original Best Park
1.2.1 ARM32 guest. The readback verifies GL completion and error state,
counts non-black and varying-color pixels, and hashes real RGBA bytes.
Optional PPM snapshots are row-flipped into standard top-down display
order and saved **only** in the diagnostic app's private data directory.
The local captured frame 1 visibly depicts the original Cartoon Network
splash screen. Captured frame 20 visibly depicts the Heavy Boat logo.
These are original game's images rendered by original ARM32 guest code,
not substituted host graphics.

Three consecutive original `nativeRender` invocations returned normally
after successful `nativeInit`, with seven successful quad draw calls per
invocation, stable RGBA fingerprints, and Android JNI diagnostic exit 0.
The extended original guest loop then progressed through 23 completed
render calls, with framebuffer variation and newly loaded OBB resources.
Initial frame 5 exceeded a conservative 20-million-tick limit while
decoding a legitimate PNG; increasing the bounded per-frame cap to
300 million permitted additional original texture loads and frames.

On frame 24 the guest requested two additional Cocos Java APIs:
`getDeviceModel()Ljava/lang/String;` and
`setBackgroundMusicVolume(F)V`. Their original APK implementations were
decompiled to verify behavior. The developer-only Java Activity returns
the real Android `Build.MODEL` and tracks the volume with [0, 1]
clamping. The emulated JNI `CallStaticVoidMethodV` reads the correctly
aligned/promoted float argument; an emulator run confirmed guest
background volume 0.5. **This is state tracking only, not audio playback.**
The current known next stop in frame 24 is the separate method
`playBackgroundMusic(Ljava/lang/String;Z)V`. That request still fails
closed; no fake playback or auto-success has been added.

Added `devtools/cocos_probe/tools/check_render_log.py` to validate
complete guest-render sequences, real nonuniform GLES pixels, and
successful Android JNI probe exit. The incomplete frame-24 run is
correctly rejected by that checker. Both Android x86_64 and ARM64
native diagnostic probe builds pass; the signed diagnostic APK was
rebuilt and exercised on the x86_64 emulator. Private guest APK, OBB,
images, and logs are not checked into Git.

The original game is **not yet playable through the PV7 Patcher**:
the experimental diagnostic is offscreen, stops on audio JNI, and has
no verified visible Android surface, input handling, audio playback,
physical ARM64 run, or full game loop. Production patcher and existing
Unity/CNR functionality remain unchanged. Next implement real Cocos
audio lifecycle and visible-surface synchronization under separate,
verified compatibility milestones, then complete more frames.

## October 31: original Best Park OBB audio and save data bridge

The experimental x86_64 Android diagnostic now implements the original
Cocos2dxHelper `playBackgroundMusic(String, boolean)` through JNI argument
validation and real Android `MediaPlayer`. Its original Java implementation
was verified against the shipped Best Park 1.2.1 APK. The developer-only
Activity resolves relative audio assets from the real version-16 guest OBB
(or original APK assets), extracts only validated bounded entries to
app-private cache, and passes the resulting canonical file to MediaPlayer.
The Activity retains actual playback state, supports guest looping and
volume, and releases the player at diagnostic termination and Activity
destruction. This is **actual host playback**, not a simulated JNI return.

Observed Android emulator log:

```text
Guest background music volume=0.5
Guest music staged from original OBB: sounds/menu.ogg
Guest background music PLAYING from original asset: sounds/menu.ogg looping=true
COCOS_BACKGROUND_MUSIC_PLAYBACK_STARTED path=sounds/menu.ogg looping=1
```

That call moved the original game past the prior audio JNI blocker within
frame 24. The diagnostic also now supports `setStringForKey(String,String)`
using the exact source-verified Cocos SharedPreferences synchronous
`commit()` mechanism. An emulator run confirmed saving four original
guest keys (`characterSkillsSavedKeys`, `characterSkillPointsKey`,
`completedDungeonsKeys`, `lastAreaDungeonKey`). The next identified
JNI access is `setIntegerForKey(String,int)`, which is modeled using the
same source-verified Android preference API. A subsequent x86_64
Android emulator run confirmed the original guest persisted the integer
`restoreAtArea=0`, followed by additional original string preference keys.
The same run completed all 30 original `nativeRender` calls and returned
Android JNI diagnostic exit 0. The bundled log checker passed
`rendered=30 captured=4 distinct_frame_hashes=3`. The captured frame
30 is visibly the original **Regular Show: Best Park in the Universe**
title screen, after the Cartoon Network and Heavy Boat splash screens.

The code still runs only in the private experimental diagnostic,
not the production PV7 Patcher. There is still no verified visible
Android game surface, interactive controls, complete audio subsystem,
or playable game. Audio in this proof is emitted by the Android emulator,
not by a physical ARM64 device.

## October 31: extended original boot and legacy PlayHaven boundary

With a bounded 90-frame diagnostic, the original game returned from 31
consecutive ARM32 render calls before frame 32 requested
`regularshow.playHavenDisplayContent(String)`. Decompilation of the
original APK confirms this method constructs a legacy
`PHPublisherContentRequest` with the content identifier and asynchronously
calls `send()`. It is an advertising-network request, not a rendering or
storage API.

The isolated offline diagnostic now explicitly records this request as
`COCOS_LEGACY_PLAYHAVEN_AD_REQUEST_DISABLED` after validating the
guest JNI argument and method identity. It deliberately does not contact
the obsolete network, fabricate ad content, or claim PlayHaven service
support. It is limited to the experimental debug runner only. Continued
rendering after this explicitly disabled optional integration must be
tested before claiming that the entire extended render loop succeeds.

### Render-time bitmap fonts and platform queries

A later original guest frame requests dynamic bitmap text for menu
buttons such as `PLAY` and `MORE`, using the original bundled
`fonts/sharkformalfunnyness.ttf`. The existing Android Canvas/Paint
bitmap renderer and original ARM32 `nativeInitBitmapDC` callback
already worked during `nativeInit` but were initially not serviced
when `nativeRender` suspended in a bitmap JNI callback. Reused the
same guarded guest/host callback drain across both entrypoints,
preserving translated ARM32 integer/FP register context and resuming
the interrupted original guest. On the emulator, Android bitmap
renders, texture uploads, and twelve render-time bitmap callbacks
completed before reaching further platform calls.

Source verification against the original APK also established:
`regularshow.isUserSignedIn()` delegates to Play Games authentication;
the offline diagnostic has no authenticated Play Games service and
explicitly returns signed-out rather than inventing success.
`regularshow.isKindle()` checks
`Build.MANUFACTURER.equals("Amazon")`; the diagnostic obtains
that property from the real Android host. Neither platform
compatibility path should be represented as full Play Games support.

The 90-frame Android x86_64 diagnostic was subsequently verified with
both JNI and pixel readback, after adding the source-verified Kindle
manufacturer query. Real Android Paint rendered the original game's
button fonts and the original ARM32 bitmap callback completed; multiple
`PLAY` and `MORE` text requests were handled during render-time JNI.
The offline Google Play Games query explicitly returned signed-out.
The emulator's real `Build.MANUFACTURER` query returned non-Amazon.

The signed APK diagnostic reported:
```text
COCOS_RENDER_SEQUENCE_COMPLETED frames=90
ANDROID_JAVA_PROBE_EXIT=0
PASS: rendered=90 captured=10 distinct_frame_hashes=6
```

The original game progressed to a complete visual menu, showing
`PLAY`, `NEW GAME`, `MORE`, and `MENU`, with original title art
and soundtrack. A frame-90 PNG was inspected locally; no guest resources
or captured game art are committed. Both x86_64 and ARM64 probe native
libraries built successfully; the signed APK was tested on an x86_64
emulator only.

## October 31: visible Android EGL window and early touch bridge

The developer-only `CocosProbeActivity` now hosts a real Android
`SurfaceView`. The guest translation worker acquires a reference-counted
`ANativeWindow` from that Surface and configures the same GLES2 renderer
used by the offscreen diagnostic with `EGL_WINDOW_BIT` and
`eglCreateWindowSurface`. It preserves the original game's 480x320 render
buffer and presents each completed original `nativeRender` call to the
Activity with `eglSwapBuffers`. Existing `glReadPixels` checkpoints
still capture actual GLES results. The worker releases EGL context,
surfaces and native window references in order. Android SurfaceHolder
destruction sets a cancellation flag so the bounded guest loop stops
at a safe between-frame boundary; the Activity is landscape.

A signed experimental APK was installed and run on the x86_64 Android
emulator. Android screenshots, not just guest framebuffer dumps, showed
the game progressing from the original loading illustration to the
full title/menu with `PLAY`, `NEW GAME`, and `MENU` in a real
Android window. The actual 90-frame visible diagnostic returned exit 0,
with successful presentation at each 10-frame checkpoint. A subsequent
240-frame visible run also returned exit 0 with no EGL presentation failure.
Both native Android architectures built successfully; this is **not yet
an ARM64 hardware validation**.

Single-finger Android ACTION_DOWN/ACTION_UP events are queued from
the main UI thread with surface-local coordinates mapped to the game's
480x320 guest coordinates. Only the single Dynarmic guest worker calls
the original exported JNI `nativeTouchesBegin(int,float,float)` and
`nativeTouchesEnd(int,float,float)` entrypoints, using ARM32 argument
registers and stack slots. The queue is bounded and rejects invalid
inputs; movement and multitouch array calls are explicitly unimplemented.
The status overlay now hides after the first displayed game frame.
Touch behavior must be confirmed with a live emulator menu interaction
before the experimental build can be called interactive.

All experimental game data, offscreen captures and device screenshots
stay local; no copyrighted game assets are committed. Production
patcher and existing Unity/CNR functionality are unchanged.

### On-screen ARM32 touch dispatch and audio feedback

Verified exported original Cocos touch entry points
`nativeTouchesBegin(int,float,float)` and
`nativeTouchesEnd(int,float,float)` with the ELF dynamic symbol
table and the original APK's `Cocos2dxRenderer.handleActionDown/Up`
methods. Android UI queues event coordinates mapped to the guest
framebuffer. The Dynarmic worker invokes both original ARM32 JNI exports,
and an emulator tap returned successfully from each. A separate
live-frame test synchronized to the guest's 60th presented frame
confirmed PLAY button touches reached the game's own sound handler;
the original guest requested `playEffect(String,boolean,float)`.

The original APK's `Cocos2dxHelper.playEffect` delegates to
`Cocos2dxSound.playEffect`, backed by Android SoundPool. Added a
source-backed Android SoundPool bridge that stages actual original
OBB/APK effect files in app-private cache, loads sound IDs, waits for
bounded asynchronous load completion, and returns the real playback
stream ID or -1 on failure. The emulator verified original
`sounds/click.wav` loaded and played (stream 1), the original
touch-end callback returned, and the game proceeded to load
`/sd/cutscenes.ini` and begin looping original `sounds/ani_loop.ogg`.

The guest later calls `playEffect` with an **empty path**. This is
handled as source-compatible missing audio with stream ID -1, not
as a fatal JNI error or fabricated playback. The final x86_64 Android
emulator run injected a tap on PLAY after the original 60th
presented frame and verified successful original ARM32 touch-begin and
touch-end returns, real click sound stream ID, original cutscene
loading and looping audio, two missing-audio -1 returns,
all 240 successive on-screen original render frames, and JNI exit 0.
An Android screen capture showed Mordecai, Rigby and Benson in the
opening cinematic with subtitle dialogue. This is original guest
state progression, not an artificially rendered picture. The diagnostic
now emits on-screen frame checkpoints to Android logcat for repeatable,
synchronized input testing.

PLAY is now verified to launch an original cutscene, but **interactive
gameplay is not yet verified**. Touch move, multi-pointer arrays, true
hardware ARM64 behavior and a continuous non-diagnostic render loop
remain unsupported or unverified.

## October 31: continuous EGL render loop and native touch movement

The Android-only diagnostic `nativeRun` now takes a configurable frame limit:
`adb shell am start -n me.jacqueb.pv7.cocos/.CocosProbeActivity --ei pv7.maxFrames 1200`
selects a bounded test; a normal launch uses zero, meaning render until the
Android `SurfaceView` is destroyed. The loop uses a monotonic clock target
of 33,333 microseconds per frame, and resets behind-schedule frame deadlines
rather than trying to catch up by spinning. Native EGL cleanup, original audio
cleanup, and between-frame cancellation are preserved. The host-only probe
still has a 240-frame safety limit.

The original APK's `Cocos2dxRenderer.handleActionMove` and
`handleActionCancel` call `nativeTouchesMove(int[],float[],float[])`
and `nativeTouchesCancel(int[],float[],float[])`. The experimental
Dynarmic worker now stages JNI int/float arrays in mapped guest memory,
including real array lengths, element pointers, region reads and releases,
and calls those exact exported original ARM32 functions. Android gesture
events are delivered to the same bounded queue as touch-begin/end, including
the pointer IDs and per-event coordinates. For this first implementation,
movement batches are translated as individual pointer samples rather than
a combined multi-pointer array; simultaneous multitouch gameplay remains
unverified.

A signed emulator build completed 1,200 consecutive original guest render
calls after a scripted Android swipe, with original `nativeTouchesBegin`,
`nativeTouchesMove` and `nativeTouchesEnd` all returning. The extended
frame-log validator passed with `rendered=1200 captured=26
distinct_frame_hashes=11`, requiring real Android EGL frames and touch
movement. Original menu-to-cutscene resources/audio continued to execute.

The 1,200-frame diagnostic originally produced a 31 MB trace due to
successful GLES draw-operation logging. A filtered 600-frame rerun passed
the EGL/render regression validator and reduced this to 1.2 MB. Inspection
showed `COCOS_ATOI_OK` alone represented 48,472 lines and 1.1 MB of the
remaining trace, so those successful parser calls and repetitive progress
samples are now suppressed as well. In a later continuous run, the log
measured only about 81 KB after 377 frames, and the native runner no
longer writes unbounded PPM images unless frame-limited test mode is used.
All original assets, screen captures and OBB data remain local and out
of Git.

A continuous-run exit test reached frame 300 but exposed an Android
surface-destruction race: pressing Back invalidated the window before
`eglSwapBuffers`, producing `EGL_BAD_SURFACE` (12301). The Activity now
signals native cancellation *before* its Back handler destroys the
surface, and the JNI loop recognizes an explicitly canceled presentation
as a normal stop rather than an emulation failure. The repeated emulator
test reached the original frame-300 presentation checkpoint, then pressed
Android Back and finished with:
```text
COCOS_RENDER_MODE continuous max_frames=0
COCOS_RUNTIME_STOPPED_BY_SURFACE at_frame=357
ANDROID_JAVA_PROBE_EXIT=0
```
No `COCOS_ONSCREEN_PRESENT_FAILED` line occurred. The final continuous
run's log was only around 81 KB, with no recurring framebuffer PPM files.
This verifies activity-initiated shutdown in the x86 emulator, but not
long-duration stability, gameplay input, or real physical ARM64 behavior.
