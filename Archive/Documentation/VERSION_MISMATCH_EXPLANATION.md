# Why You Can't See Rooms Between Different Game Versions

## The Problem

The game has a **strict version check** that prevents different APK versions from seeing each other's rooms:

```csharp
// From MultiplayerSelectDirector.cs (line 569)
if (roomList[i].maxPlayers != roomList[i].playerCount && 
    (string)roomList[i].customProperties["version"] == UserDataController.GetStrVersion())
{
    // Only show this room if version matches EXACTLY
    list.Add(roomList[i]);
}
```

## Why This Happens

Each time a room is created, it stores the **exact version string** in room properties:

```csharp
// From MultiplayerSelectDirector.cs (line 622)
hashtable2.Add("version", UserDataController.GetStrVersion());
```

When you browse rooms, the game **filters out** any room that doesn't match your current version:

```csharp
// Version check happens here (CNRConnectMenu.cs, line 203)
if ((string)roomInfo.customProperties["version"] != UserDataController.GetStrVersion() 
    || roomInfo.maxPlayers < playerLimitArray[0] 
    || roomInfo.maxPlayers > playerLimitArray[1] 
    || roomInfo.maxPlayers == roomInfo.playerCount)
{
    // Room is hidden if versions don't match
}
```

## What Version Are You Running?

The version is stored in PlayerPrefs:

```csharp
public static string GetStrVersion()
{
    return PlayerPrefs.GetString("CurrentVersion", "v3.0.2");
}
```

**Default version: `v3.0.2`**

## Your Situation

| App | Package Name | Version Stored |
|-----|--------------|-----------------|
| App 1 | `com.joydo.minestrikenew` | `v3.0.2` (probably) |
| App 2 | `com.joydo.minestrikenex` | `v3.0.2` (probably) |

**But they might be different versions!** If one was installed from an older APK, or if the version was manually changed, they won't see each other's rooms.

## How to Check

On the phone, you'd need to check the PlayerPrefs database for each app:

```bash
# For com.joydo.minestrikenew
adb shell "run-as com.joydo.minestrikenew cat /data/data/com.joydo.minestrikenew/shared_prefs/unity.xml" | grep -i "CurrentVersion"

# For com.joydo.minestrikenex  
adb shell "run-as com.joydo.minestrikenex cat /data/data/com.joydo.minestrikenex/shared_prefs/unity.xml" | grep -i "CurrentVersion"
```

## How to Fix

Make sure **both apps have the same version string**. The version is set in PlayerPrefs and persists:

1. **Check what versions are set** (see commands above)
2. **If different**, uninstall one and reinstall it to reset PlayerPrefs
3. **Or**, if you have APK access, you can decompile, find where version is set on first launch, and ensure both use the same value

## Code Reference

| File | Purpose |
|------|---------|
| `UserDataController.cs` line 23 | `CNR_CUR_VERSION = "v3.0.2"` |
| `UserDataController.cs` line 3403-3405 | `GetStrVersion()` returns stored version |
| `MultiplayerSelectDirector.cs` line 569 | Version filter when browsing rooms |
| `MultiplayerSelectDirector.cs` line 622 | Version stored when creating room |
| `CNRConnectMenu.cs` line 203 | Version check when listing rooms |

## Why This Design?

This prevents:
- Old clients connecting to new servers (breaking changes)
- New clients connecting to old servers (incompatible protocol)
- Version fragmentation in multiplayer lobbies

Unfortunately, it also means different APK versions can't play together, even if they're compatible.

