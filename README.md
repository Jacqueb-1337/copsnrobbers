# Cops n Robbers — Revival Project

Community effort to restore online / LAN multiplayer for the original *Cops n Robbers* Android game by reverse-engineering the protocol and injecting a custom mod loader.

---

## Repository layout

```
node_server/          Node.js master + game server (TCP, Photon-like protocol)
APK_Build_Active/     Patched APK ready to install + mod source files
  target_modloader.apk   Install this on the device
  IPRedirectMod.cs       Baked-in mod: redirects Photon connections, loads other mods
  CNRSettingsMod.cs      Drop-in mod: in-game HUD editor + settings overlay
hooking/              IL patching scripts and reference material
Archive/              Old backups, earlier attempts, docs
```

---

## How the mod system works

### 1 — APK patch (Assembly-CSharp.dll)

`target_modloader.apk` contains a patched `Extensions` class whose **static constructor** was modified to act as a generic mod loader:

```csharp
static Extensions()
{
    // creates /storage/emulated/0/CNRMods/ if missing
    // scans every *.dll in that folder
    // for each DLL: loads it and calls CNRMods.ModEntry.Load()
}
```

This means the APK itself never needs to be replaced again — you add or update mods by pushing DLLs to the device.

### 2 — /sdcard/CNRMods/ folder

Drop compiled mod DLLs here. The loader finds them automatically on the next game launch.

| File | Purpose |
|------|---------|
| `IPRedirectMod.dll` | **Required.** Redirects all Photon traffic to your custom server. Also calls `LoadExternalMods()` which scans the folder a second time and loads any DLL exposing a `public static void Load()` method (broader than the static-ctor convention, catches mods that don't use the `CNRMods.ModEntry` class name). |
| `CNRSettingsMod.dll` | Optional. Adds an in-game settings overlay and HUD position/scale editor. |
| `server.cfg` | Plain-text config read by `IPRedirectMod`. |

**server.cfg format:**

```
SERVER_IP=172.28.48.1
```

Set this to the IP address of the machine running the Node.js server.

### 3 — Mod loading chain (summary)

```
Game starts
  └─ Extensions static ctor
       └─ loads IPRedirectMod.dll  →  CNRMods.ModEntry.Load()
            ├─ patches Photon connection target (reads server.cfg)
            └─ LoadExternalMods()
                 └─ loads CNRSettingsMod.dll  →  SettingsModEntry.Load()
```

---

## Server — Node.js

The server lives in `node_server/`. No extra dependencies are required.

### Requirements

- Node.js ≥ 18
- Ports **5055** (master) and **5056** (game) open in your firewall

### Start

```powershell
cd node_server
npm start
```

Or pass explicit IPs for multi-network setups (e.g. WSA + ZeroTier):

```powershell
node index.js 172.28.48.1 172.29.99.63
```

If no IPs are given, the server auto-detects all non-loopback IPv4 addresses on the machine.

The console will print the `SERVER_IP` value to use in `server.cfg`.

### Ports

| Port | Purpose |
|------|---------|
| 5055 | Master server — room listing, authentication |
| 5056 | Game server — in-room traffic |
| 8080 | Web console (browser status dashboard) |

---

## Installing the patched APK

### Requirements

- ADB connected to the device (or WSA via `adb connect localhost:58526`)

### Steps

```powershell
# 1. Install the patched APK
adb install -r APK_Build_Active\target_modloader.apk

# 2. Push IPRedirectMod and point it at your server
adb shell mkdir -p /sdcard/CNRMods
adb push APK_Build_Active\bin\csc_build\IPRedirectMod.dll  /sdcard/CNRMods/IPRedirectMod.dll
adb push APK_Build_Active\server.cfg                       /sdcard/CNRMods/server.cfg

# 3. (Optional) Push the settings/HUD mod
adb push APK_Build_Active\bin\csc_build\CNRSettingsMod.dll /sdcard/CNRMods/CNRSettingsMod.dll
```

After launch, mod logs are written to:

```
/sdcard/CNRMods/redir.log      # IPRedirectMod
/sdcard/CNRMods/settings.log   # CNRSettingsMod
```

Pull them with `adb pull /sdcard/CNRMods/redir.log`.

---

## Building mods

Mods are plain C# class libraries targeting **.NET 3.5 / Unity 4 Mono**. The compiler used is the one shipped with .NET Framework 4 (`csc.exe /nostdlib`).

```powershell
cd APK_Build_Active
.\compile_mod_loader.ps1    # compiles IPRedirectMod.dll + pushes to device
```

Reference DLLs are taken from the extracted APK at `assets/bin/Data/Managed/`.

### Mod entry-point convention

```csharp
namespace CNRMods          // must match for static-ctor loader
{
    public static class ModEntry
    {
        public static void Load() { /* ... */ }
    }
}
```

`IPRedirectMod`'s secondary loader (`LoadExternalMods`) is more permissive — it finds the first `public static void Load()` in any type, so additional mods (like `CNRSettingsMod`) don't need to follow the `CNRMods.ModEntry` naming convention.

---

## Progress

| Milestone | Status |
|-----------|--------|
| Class name deobfuscation | ✅ Done |
| Custom C# DLL injection | ✅ Done |
| Mod loader (generic, APK-level) | ✅ Done |
| IP redirect (Photon → custom server) | ✅ Done |
| Discovery / room listing (custom server) | ✅ Done |
| HUD editor + settings overlay | ✅ Done |
| Photon ↔ custom UDP packet translation | 🔄 In progress |
| Room loading / joining | ⬜ Todo |
| Full handshake + live matches | ⬜ Todo |
| Automated tests, CI | ⬜ Todo |
| Packaging, signing, public release | ⬜ Todo |
