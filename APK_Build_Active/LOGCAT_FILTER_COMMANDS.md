# Logcat Filter Commands

## Unity-Focused Filter (Excludes System Noise)

Use this command exactly:

```powershell
adb logcat | Select-String -Pattern "Failed to write input event: Bad file descriptor|DevicePolicyManager|SystemServiceManager|virtualinputservice|Connect\(\)|Filename:|SystemServerTiming|KernelCpuProcStringReader|DisplayPowerController|DisplayDeviceRepository|AppIdleHistory|HistoricalRegistry|TestHarnessModeService|AppOps|PackageManager|StorageManagerService|CompatibilityChangeReporter|WindowManager|DisplayModeDirector|ActivityManager|permission|wificond|starting|kernel|task|settings|service|launch|register|battery|network|watch|system|helper|power|display|toast|notification|kill|intent|window|storage|authentication|^\s+at\s|^$" -NotMatch | Select-String -Pattern "Unity" | Select-String -Pattern ":\s*$" -NotMatch
```

### What It Does

1. Excludes common system noise patterns (DevicePolicyManager, SystemServiceManager, virtualinputservice, etc.)
2. Filters to only lines containing "Unity"
3. Removes empty lines and stack trace lines
4. Shows only meaningful Unity debug output

### For Mod Loader Debugging

Use this to see mod loader initialization and DLL loading:

```powershell
adb logcat | Select-String -Pattern "CNRModLoader|CNRMods"
```
