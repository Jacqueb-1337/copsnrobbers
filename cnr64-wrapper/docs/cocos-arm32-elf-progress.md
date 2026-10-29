# Project V7 — Cocos2d-x ARM32 ELF loader progress

This document records what is actually verified for
`Regular Show: Best Park in the Universe` v1.2.1.

## Completed native image stages

The reusable `Arm32GuestImage` class maps an ELF32 ARM shared object
into a caller-selected guest address range, zero-fills BSS, enumerates
DT_NEEDED libraries and exported JNI symbols, applies supported ARM32
REL relocations, and identifies constructor entry points.

The CLI `pv7-arm32-elf-probe` accepts the local game library:

```text
pv7-arm32-elf-probe libgame.so
pv7-arm32-elf-probe libgame.so --audit-relocations
```

The optional audit supplies **deliberately non-executable diagnostic
addresses** for external imports. It verifies relocation records and
prints the imported symbol inventory; it does **not** load or execute
Android guest code or pretend to provide libc, EGL, JNI, sound, or
filesystem services.

Locally, the mapper was compiled with MSVC on Windows and tested against
the actual 5,359,068-byte Best Park `libgame.so` obtained by the owner.
Results:

- Mapped image span: **5,514,632 bytes**.
- Libraries: `liblog.so`, `libz.so`, `libGLESv2.so`,
  `libstdc++.so`, `libm.so`, `libc.so`, `libdl.so`.
- Resolved guest JNI exports at image base `0x10000000`:
  - `JNI_OnLoad`: `0x1014e6e5` (Thumb entry)
  - `Cocos2dxRenderer_nativeInit`: `0x1014e70d`
  - `Cocos2dxRenderer_nativeRender`: `0x1021f0d9`
- **All present relocation records accepted** by the current parser
  in non-executing audit mode, with **330 distinct external symbol names**
  and **324 initializer entries** identified. Of the 330 distinct imports,
**323 are functions** and **7 are data objects** (no untyped imports).
The exact seven data symbols are `__sF`, `__stack_chk_guard`, `_ctype_`,
`_tolower_tab_`, `_toupper_tab_`, `optarg` and `optind`. These need
writable guest-owned data slots or compatible C runtime tables, **not**
function trampolines. The audit CLI now prints `DATA_IMPORT` lines for them.

The `CopyToGuestMemory` API now copies a relocated image into a
caller-owned guest memory array at the selected base with preflight bounds
checks; an undersized destination fails before copying. A synthetic unit test
validates the final relocated words in the destination. This is the memory
bridge groundwork, **not** a running Dynarmic guest/JNI environment.

No copyrighted game bytes or import addresses from the game are checked
into Git. The engine-neutral synthetic ELF tests run in GitHub Actions
and additionally exercise `--audit-relocations`.

## Compatibility Analyzer: instrumentation foundation

The new engine-independent offline static analyzer is
cnr64-wrapper/tools/arm32_compat_analyzer.py. Run it against an
owner-supplied ARM32 .so (no game assets are committed):

~~~sh
python3 cnr64-wrapper/tools/arm32_compat_analyzer.py \
  /path/to/libgame.so --output /path/to/report.json --max-branches 1500
~~~

The versioned JSON report includes dependency libraries; dynamic imports,
symbol types and relocation sites; exported functions; JNI entrypoints;
ELF constructor counts; and bounded *candidate* ARM/Thumb direct branches.
An optional analyst-provided --aosp-tag android-X.Y.Z_rN is recorded only as
**unverified reference metadata**. It does not guess the original Android
version, fetch historical source, or claim the reference implements this ABI.

Evidence levels must stay separate:

- Static facts: ELF headers, DT_NEEDED, dynsym, ELF relocations and constructor
  references are auditable without executing guest instructions.
- Static hypotheses: branch patterns can be false positives when ARM/Thumb
  mode or code-versus-data is ambiguous. Indirect calls, dispatch tables and
  runtime dlopen/dlsym are not exhaustively visible.
- Observed events: only recorded from actual Dynarmic execution callbacks,
  never synthesized from the static import inventory.

The reusable Arm32TraceRecorder in projectv7-elf32 provides thread-safe,
bounded events (call, return, import, fallback, fault) recording guest PC,
target, thread ID, raw r0-r3 samples, symbol and detail. It aggregates
identical call sites and serializes escaped JSON Lines with drop counts.
Raw guest registers are **not decoded argument types**, and diagnostic reports
may contain sensitive values; uploads should remain opt-in.

The recorder is a **sink**, not a live engine tracer yet: the new Cocos guest
Dynarmic dispatcher still needs to be wired to emit events. It cannot yet
trace Best Park gameplay or provide the 323 external function implementations.

The GitHub ELF workflow now runs synthetic ELF fixture and runtime-recorder
regression tests alongside the static analyzer tests.

## Remaining implementation

1. Build a real typed import resolver, distinguishing ELF function calls
   from data symbols; map the 330 imported names to *actual guest
   addresses* and deliberate host services, not placeholder addresses.
2. Copy the validated mapped image to the shared Dynarmic guest memory,
   respecting the chosen guest address, alignment, page permissions and
   avoiding existing Unity/CNR memory ranges.
3. Set up the ARM32 calling convention, stack, JNI `JavaVM` and
   `JNIEnv` tables, execute constructors and `JNI_OnLoad` in controlled
   Dynarmic calls, with bounded fault/trace reporting.
4. Implement Cocos renderer init/render, Android lifecycle, EGL/GL,
   touch, audio and asset access.
5. Map guest OBB/data/cache/files requests to
   `GuestSharedStorage/{obb,data,cache,files}` in the wrapper.
6. Test Best Park on hardware, then enable the independent Cocos2d-x
   module; preserve regression coverage for Unity/CNR.

**The game is not playable yet.** The confirmed result here is genuine
ELF image loading and relocation audit, not guest code execution.
