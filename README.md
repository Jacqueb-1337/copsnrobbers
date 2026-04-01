# Cops n Robbers Revival Project

Community effort to restore online / LAN multiplayer for the original *Cops n Robbers* Android game by reverse-engineering the protocol and injecting a custom mod loader.

---

## Repository layout

```
cnr-revived-web/          Node.js master + game server (TCP, Photon-like protocol)
APK_Build_Active/     Patched APK ready to install + mod source files
  target_modloader.apk   Install this on the device
  IPRedirectMod.cs       Baked-in mod: redirects Photon connections, loads other mods
  CNRSettingsMod.cs      Drop-in mod: in-game HUD editor + settings overlay
hooking/              IL patching scripts and reference material
Archive/              Old backups, earlier attempts, docs
```

---

## How the mod system works

### 1  APK patch (Assembly-CSharp.dll)

`target_modloader.apk` contains two patched game classes that act as the mod loader. No patching of `Extensions` or any other class is needed  only these two:

**`MainMenuDirector.Awake()`** calls `MainMenuDirector.LoadMods()` (static, also callable from elsewhere):

```csharp
public static void LoadMods()
{
    string dir = "/storage/emulated/0/CNRMods";
    Directory.CreateDirectory(dir);
    foreach (string path in Directory.GetFiles(dir, "*.dll"))
    {
        Type t = Assembly.Load(File.ReadAllBytes(path)).GetType("CNRMods.ModEntry");
        t?.GetMethod("Load", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
    }
}
```

This loads any DLL that exposes `CNRMods.ModEntry.Load()`  that's `CNRMod.dll`.

**`CNRMod.LoadExternalMods()`** is called from `CNRMods.ModEntry.Load()` and does a second pass over the folder, finding **the first `public static void Load()` in any type** in each remaining DLL:

```csharp
private static void LoadExternalMods()
{
    foreach (string path in Directory.GetFiles("/storage/emulated/0/CNRMods", "*.dll"))
    {
        if (Path.GetFileName(path).Equals("CNRMod.dll", StringComparison.OrdinalIgnoreCase)) continue;
        Assembly asm = Assembly.Load(File.ReadAllBytes(path));
        foreach (Type t in asm.GetTypes())
        {
            MethodInfo m = t.GetMethod("Load", BindingFlags.Public | BindingFlags.Static,
                                       null, Type.EmptyTypes, null);
            if (m != null) { m.Invoke(null, null); break; }
        }
    }
}
```

This means extra mods (`CNRSettingsMod`, `CNRRecordingMod`, etc.) don't need to follow the `CNRMods.ModEntry` naming convention  **any public static `Load()` method in any class will be found and called**.

This means the APK itself never needs to be replaced again  you add or update mods by pushing DLLs to the device.

### 2  /sdcard/CNRMods/ folder

Drop compiled mod DLLs here. The loader finds them automatically on the next game launch.

| File | Purpose |
|------|---------|
| `CNRMod.dll` | **Required.** Redirects all Photon traffic to your custom server, reads `server.cfg`, and calls `LoadExternalMods()` to chain-load other mods. |
| `CNRSettingsMod.dll` | Optional. Adds an in-game settings overlay and HUD position/scale editor. |
| `CNRRecordingMod.dll` | Optional. Restores the in-game record button with real H.264 MP4 output via Android MediaCodec. |
| `server.cfg` | Plain-text config read by `CNRMod`. |

**server.cfg format:**

```
SERVER_IP=172.28.48.1
```

Set this to the IP address of the machine running the Node.js server.

### 3  Mod loading chain (summary)

```
Game starts  →  MainMenuDirector.Awake()
  └─ MainMenuDirector.LoadMods()
       └─ finds CNRMod.dll  →  CNRMods.ModEntry.Load()
            ├─ reads server.cfg, patches Photon connection target
            └─ LoadExternalMods()
                 ├─ finds CNRSettingsMod.dll  →  SettingsModEntry.Load()
                 ├─ finds CNRRecordingMod.dll →  RecordingModEntry.Load()
                 └─ finds any other *.dll     →  first public static Load()
```

---

## Server  Node.js

The server lives in `cnr-revived-web/`. No extra dependencies are required.

### Requirements

- Node.js ≥ 18
- Ports **5055** (master) and **5056** (game) open in your firewall

### Start

```powershell
cd cnr-revived-web
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
| 5055 | Master server  room listing, authentication |
| 5056 | Game server  in-room traffic |
| 8080 | Web console (browser status dashboard) |

---

## Installing the patched APK

### Requirements

- ADB connected to the device (or WSA via `adb connect localhost:58526`)

### Steps

```powershell
# 1. Install the patched APK
adb install -r APK_Build_Active\target_modloader.apk

# 2. Push CNRMod and point it at your server
adb shell mkdir -p /sdcard/CNRMods
adb push cnr-revived-web/mods/CNRMod/CNRMod.dll  /sdcard/CNRMods/CNRMod.dll
adb push APK_Build_Active\server.cfg              /sdcard/CNRMods/server.cfg

# 3. (Optional) Push extra mods
adb push cnr-revived-web/mods/CNRSettingsMod/CNRSettingsMod.dll   /sdcard/CNRMods/CNRSettingsMod.dll
adb push cnr-revived-web/mods/CNRRecordingMod/CNRRecordingMod.dll /sdcard/CNRMods/CNRRecordingMod.dll
```

After launch, mod logs are written to:

```
/sdcard/CNRMods/cnrmod.log        # CNRMod
/sdcard/CNRMods/settings.log      # CNRSettingsMod
/sdcard/CNRMods/recording.log     # CNRRecordingMod
```

Pull them with `adb pull /sdcard/CNRMods/redir.log`.

---

## Building mods

Mods are plain C# class libraries targeting **.NET 3.5 / Unity 4 Mono**. The compiler used is the one shipped with .NET Framework 4 (`csc.exe /nostdlib`).

```powershell
# From the repo root:
.\build.ps1 mod           # CNRMod.dll
.\build.ps1 settings      # CNRSettingsMod.dll
.\build.ps1 recording     # CNRRecordingMod.dll
.\build.ps1 all           # all of the above
.\build.ps1 recording -NoDeploy   # build only, don't push to device
```

Source files live in `cnr-revived-web/mods/<ModName>/<ModName>.cs`. Reference DLLs are taken from the extracted APK at `APK_Build_Active/apk_source/assets/bin/Data/Managed/`.

### Writing a new mod

The only requirement is **a public static `Load()` method somewhere in your DLL**. Class name and namespace don't matter for the secondary loader. Convention used by existing mods:

```csharp
// MyMod.cs
using UnityEngine;

namespace MyModNamespace
{
    public static class MyModEntry
    {
        public static void Load()
        {
            var go = new GameObject("MyMod_Root");
            go.AddComponent<MyModBehaviour>();
            GameObject.DontDestroyOnLoad(go);
        }
    }

    public class MyModBehaviour : MonoBehaviour
    {
        private void Awake() { /* runs once */ }
        private void Update() { /* runs every frame */ }
        private void OnGUI() { /* IMGUI overlay */ }
    }
}
```

Drop the compiled DLL into `/sdcard/CNRMods/` and it will be loaded automatically on next launch by `CNRMod.LoadExternalMods()`.

**Optionally register with CNRMod** so your version appears in the in-game mod manager:

```csharp
// In Load(), after the GameObject is created:
try
{
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        if (!asm.GetName().Name.Equals("CNRMod", StringComparison.OrdinalIgnoreCase)) continue;
        var t = asm.GetType("CNRMods.ModEntry");
        var m = t?.GetMethod("RegisterMod", BindingFlags.Public | BindingFlags.Static,
                              null, new[] { typeof(string), typeof(string) }, null);
        m?.Invoke(null, new object[] { "MyMod", "1.0.0" });
        break;
    }
}
catch { }
```

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
| Protocol (Photon forced into TCP mode  no UDP translation needed) | ✅ Done |
| Room loading / joining | ✅ Done |
| Full handshake + live matches | ✅ Done |
| Packaging, signing, public release | ✅ Done |
