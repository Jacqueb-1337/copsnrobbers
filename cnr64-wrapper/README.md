# ProjectV7

ProjectV7 is a compatibility runtime for running legacy 32-bit Android native games inside a modern 64-bit Android process. The current Cops N Robbers work is the first development profile and test case, not the intended boundary of the project.

The core goal is to preserve original guest binaries and runtime behavior where practical, including old Unity/Mono JIT code generation, while translating ARMv7 A32 execution and bridging Android/JNI/graphics/runtime ABI boundaries on ARM64-only devices.

## Architecture

```text
64-bit Android / ART
    -> ProjectV7 host shell / game profile
    -> ProjectV7 ARM32 runtime + compatibility bridges
    -> Dynarmic A32 translator
    -> original ARM32 game/native runtime payloads
    -> original managed/native game content
```

Compatibility fixes that are useful across titles belong in the normal ProjectV7 bridge/runtime code. Game-specific behavior should be isolated behind a game profile or ordinary compatibility plugin rather than embedded throughout the core.

## Developer hotpatch environment

Live native hotpatching exists only to confirm a suspected compatibility fix quickly. It is not a production extension mechanism.

`PROJECTV7_DEV_HOTPATCH` defaults to `OFF`. With it disabled, the live rule loader, native plugin loader, trap-wait loop, and developer arbitrary-code extension path are omitted from the core build. `PROJECTV7_DEV_DIAGNOSTICS` also defaults to `OFF`; verbose thunk traces and periodic execution tracing are compiled only into diagnostic builds.

Developer workflow:

1. Reproduce a compatibility failure in a dedicated developer build.
2. Confirm the proposed behavior with a temporary live rule/plugin if that saves a rebuild cycle.
3. Bake the confirmed behavior into the normal compatibility bridge/runtime, or into a standard game/profile compatibility module when it is truly title-specific.
4. Verify again with `PROJECTV7_DEV_HOTPATCH=OFF`.

The developer-only implementation and temporary plugins live under `devtools/hotpatch/`.

## Cops N Robbers development profile

CNR currently exercises ProjectV7 against Unity 4.6.1-era ARM32 `libmain`, `libunity`, and `libmono`, including Mono JIT execution. The Android harness remains named `cnr64poc` for now so existing build/install/test automation continues to work while the reusable core is generalized.

Current milestones include ARMv7 execution, JIT code/cache invalidation, ELF loading/relocation, JNI bridging, cooperative guest threading, zlib ABI bridging, EGL/GLES presentation, and visible rendering from the original game inside the ARM64 host process.

## Android build modes

Production-style configuration, with developer hotpatching excluded:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\devtools\configure_android_production.ps1
```

Validation configuration, with verbose diagnostics but no live native plugin loader:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\devtools\configure_android_validation.ps1
```

Dedicated developer configuration, with both verbose diagnostics and live compatibility hotpatching enabled:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\devtools\configure_android_dev.ps1
```

The existing CNR harness APK can still be built with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build_poc_apk.ps1
```

The harness output remains:

```text
dist/CNR64-Arm64-Dynarmic-PoC.apk
```
