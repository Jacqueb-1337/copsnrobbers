# ProjectV7 rapid hotpatch test

The rapid-test workflow is deliberately split into narrow stages so the metrics/analyzer path contains no ADB, process-control, compiler, or live hotpatch logic.

## Assistant-side workflow

Run each stage separately from `cnr64-wrapper`:

```powershell
.\pv7compile.ps1 -Plugin plugins/job_signal_prefer.cpp -PluginName job-signal-prefer
.\pv7deploy.ps1 -Clean
.\pv7capture.ps1
.\pv7test.ps1
```

The stages communicate through small ignored JSON handoff files under `devtools/`, so paths do not have to be copied manually.

### 1. Compile

`pv7compile.ps1` only compiles one developer hotpatch `.so` and writes `devtools/pv7test-next-plugin.json`.

```powershell
.\pv7compile.ps1 -Plugin plugins/job_signal_prefer.cpp -PluginName job-signal-prefer
```

### 2. Deploy

`pv7deploy.ps1` only handles ADB deployment. With no `-PluginPath`, it consumes the compile handoff automatically.

```powershell
.\pv7deploy.ps1 -Clean
```

Omit `-Clean` to replace only older copies of the same plugin.

### 3. Capture

`pv7capture.ps1` only controls the app/logcat profiling window. It restarts the app process unless `-NoRestart` is supplied, watches for stalls/fatals/targets, writes the raw log into `devtools/pv7test-runs/<timestamp>/`, and updates `devtools/pv7test-last-run.json`.

```powershell
.\pv7capture.ps1 -TimeoutSec 30 -StallSec 10
```

Stop as soon as frame 3 appears:

```powershell
.\pv7capture.ps1 -TargetPattern 'Unity nativeRenderFrame3 progress'
```

### 4. Analyze

`pv7test.ps1` is intentionally safe/offline. It does not invoke ADB, compile code, control processes, or deploy hotpatches. With no `-LogPath`, it analyzes the most recent capture handoff.

```powershell
.\pv7test.ps1
```

It reports frame/slice progress, worker time/pumps, condition events, preload timing, fatals, and a baseline verdict.

Create a baseline from the latest captured run:

```powershell
.\pv7test.ps1 -PluginName none -SaveBaseline
```

Offline analyzer self-test:

```powershell
.\pv7test.ps1 -SelfTest
```

## One-click local workflow

For manual use, `pv7run.cmd` chains the four narrow scripts without changing PowerShell execution policy:

```cmd
pv7run plugins\job_signal_prefer.cpp job-signal-prefer
```

The assistant should prefer invoking the four stages separately. The one-click wrapper is mainly for local/manual use.

## Output and handoff files

Ignored/generated files:

- `devtools/pv7test-runs/`
- `devtools/pv7test-baseline.json`
- `devtools/pv7test-next-plugin.json`
- `devtools/pv7test-last-deploy.json`
- `devtools/pv7test-last-run.json`

Confirmed compatibility behavior should still be baked into the normal ProjectV7 runtime before production builds.
