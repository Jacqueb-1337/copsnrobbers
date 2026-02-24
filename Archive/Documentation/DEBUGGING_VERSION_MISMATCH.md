# Debugging Version Mismatch Between Two Game Packages

## What We Know

Both installed packages show manifest version `3.0.2`:
- `com.joydo.minestrikenew`: versionName=3.0.2
- `com.joydo.minestrikenex`: versionName=3.0.2

**But this doesn't tell us what's in PlayerPrefs.**

## The Real Problem

The game stores a **separate "CurrentVersion"** string in PlayerPrefs when it first runs:

```csharp
public static string GetStrVersion()
{
    return PlayerPrefs.GetString("CurrentVersion", "v3.0.2");
}
```

Each app package has its **own isolated PlayerPrefs storage**, so:
- `com.joydo.minestrikenew` might have `CurrentVersion = "v3.0.2"`
- `com.joydo.minestrikenex` might have `CurrentVersion = "v3.0.9"` (different!)

And rooms created by one app won't be visible to the other because the version filter explicitly rejects mismatches.

## How to Diagnose

Since the package isn't debuggable, you need root access or to use logcat to see what version each app is using:

### Option 1: Check logcat (No root needed)

```powershell
# Clear logs
adb logcat -c

# Launch first game
adb shell am start com.joydo.minestrikenew/com.unity3d.player.UnityPlayerProxyActivity
Start-Sleep -Seconds 3

# Grep for version strings in logs
adb logcat | Select-String -Pattern "v3\.|version|CurrentVersion" | Head -20
```

Look for any log output showing version strings.

### Option 2: Root access (if available)

```bash
adb shell "su -c 'cat /data/data/com.joydo.minestrikenew/shared_prefs/unity.xml | grep -i version'"
adb shell "su -c 'cat /data/data/com.joydo.minestrikenex/shared_prefs/unity.xml | grep -i version'"
```

### Option 3: Make apps debuggable (APK modification)

You'd need to decompile both APKs, add `android:debuggable="true"` to AndroidManifest.xml, recompile, and reinstall. Then you could access PlayerPrefs.

## Practical Solutions

### Solution 1: Reset One App's Data

If you suspect version mismatch, clear one app's data:

```powershell
# Clear data for minestrikenex (resets PlayerPrefs to defaults)
adb shell pm clear com.joydo.minestrikenex

# Reinstall from the same APK to ensure same initial state
adb install -r 'd:\Projects\copsnrobbers\target_compiled_final.apk'
```

This resets its PlayerPrefs to `v3.0.2` (the default).

### Solution 2: Install Both From Same APK

If one is from a different source/version, install both from the same reliable APK:

```powershell
# Uninstall minestrikenex
adb uninstall com.joydo.minestrikenex

# Reinstall from same source as minestrikenew
adb install 'd:\Projects\copsnrobbers\target_compiled_final.apk'
```

But you won't be able to have both installed simultaneously if they have the same package name.

### Solution 3: Check Room Creation

In the app that **created the room**, what's the exact room name and map? 

Then in the other app, go to Worldwide and look at:
- Is any room appearing at all?
- If rooms appear, are they from the same creator?
- Do the rooms show a "<Tips: ...>" message? (This indicates version mismatch)

## Code that Causes the Filtering

Here's exactly what each app does:

```csharp
// When browsing rooms
RoomInfo[] roomList = PhotonNetwork.GetRoomList();
for (int i = 0; i < roomList.Length; i++)
{
    string roomVersion = (string)roomList[i].customProperties["version"];
    string myVersion = UserDataController.GetStrVersion(); // Reads from PlayerPrefs
    
    if (roomList[i].maxPlayers != roomList[i].playerCount && 
        roomVersion == myVersion)  // <-- MUST MATCH EXACTLY
    {
        // Show this room
    }
}
```

**If `roomVersion != myVersion`, the room is hidden.**

## Evidence of Version Mismatch

Look for this message in-game:
- `"<Tips: No room created...>"` - No rooms at all visible (could mean version mismatch if other app created one)
- No rooms appearing in the list - Likely version mismatch

vs.

- `"<Tips: All rooms are full...>"` - Rooms exist but none have space (version match confirmed)
- Seeing rooms from other app - Version match confirmed

