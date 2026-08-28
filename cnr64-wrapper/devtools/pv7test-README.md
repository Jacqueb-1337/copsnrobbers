# ProjectV7 rapid hotpatch test

From `cnr64-wrapper`:

```powershell
.\pv7test -Plugin plugins/job_signal_prefer.cpp -PluginName job-signal-prefer -CleanDevicePlugins
```

The runner:

- auto-detects the connected ADB device;
- compiles only the requested developer hotpatch plugin;
- replaces older copies of the same plugin on-device;
- optionally clears all device hotpatch plugins for a clean A/B run;
- restarts only the app process unless `-NoRestart` is supplied;
- watches ProjectV7 render progress for a timeout, target pattern, fatal, or stall;
- records frame/slice progress, worker time/pumps, condition events, preload timing, and fatals;
- stores each run under `devtools/pv7test-runs/`;
- compares against `devtools/pv7test-baseline.json` when present.

Useful examples:

```powershell
# Create a clean no-plugin baseline.
.\pv7test -CleanDevicePlugins -SaveBaseline

# Test one live hotpatch against that baseline.
.\pv7test -Plugin plugins/job_signal_prefer.cpp -PluginName job-signal-prefer -CleanDevicePlugins

# Stop as soon as frame 3 appears.
.\pv7test -Plugin plugins/job_signal_prefer.cpp -TargetPattern 'Unity nativeRenderFrame3 progress'

# Hot-load into the currently running app without restarting it.
.\pv7test -Plugin plugins/job_signal_prefer.cpp -NoRestart -TimeoutSec 10
```

This is developer-only tooling. Confirmed compatibility behavior should still be baked into the normal ProjectV7 runtime before production builds.
