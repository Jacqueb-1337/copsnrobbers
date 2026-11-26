# PV7 native async work candidate

This is a staged experiment, not an installed or released wrapper. The repository's
tracked wrapper is older than the current local runtime; the patch targets the
local source snapshot identified by source_base.sha256. Do not blindly apply to
an unrelated tree or overwrite existing wrapper changes.

## Contents
- Native worker pool with two real host threads and bounded outstanding work.
- Explicit job table: native JSON to PVJ v1 tree, CRC32, Android bounded zlib decode.
- Guest ARM32 SVC thunk bridge for version/submit/poll/copy/release. Guest addresses
  are validated and copied on the harness thread; workers operate only on owned bytes.
- Audio queue-drain and stage timing telemetry (PV7AUDIOPROFILE). Queue drains are
  a starvation proxy, not AudioFlinger hardware underrun measurements.
- Optional CNRModManager 1.6.9 adapter using the host JSON path with JsonFx fallback.
- Desktop tests for native concurrency, limits, CRC, native JSON, and full managed
  catalog parity.

To integrate: copy pv7_host_worker_pool.hpp, pv7_host_jobs.hpp and pv7_host_jobs.cpp
into the runtime src directory; verify the source hash and apply wrapper_candidate.patch
from the wrapper root. Build a new isolated PV7 host template/module through the
normal PV7 packaging workflow; do not replace a production APK before testing.
The Android host zlib path is compiled for Android only. No raw guest pointers,
JNI handles, or guest JIT state may be captured by worker jobs.

Test results: native pool 10 checks, host jobs 24 checks per catalog (two catalogs),
CNR adapter compile against original Unity 4.6 references, 5176 scalar fields equal
between managed and native JSON routes, 828 versions per catalog, fallback without
native ABI works. Android integrated wrapper syntax checked. Actual ARM64 linking,
device P/Invoke, timing and audio playback not established by desktop tests.
