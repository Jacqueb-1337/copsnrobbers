# Why Photon Ports Aren't Opening

## The Problem

The game is configured to use **Photon Cloud** (external servers), not local hosting. When you create a room, it connects OUT to `app.exitgamescloud.com`, so ports 5055/5056 never open on your phone.

From `ServerSettings.cs`:
```csharp
public enum HostingOption
{
    NotSet,
    PhotonCloud,      // ← Currently set to this
    SelfHosted,       // ← What we need
    OfflineMode
}

public static string DefaultCloudServerUrl = "app.exitgamescloud.com";
public static string DefaultServerAddress = "127.0.0.1";
public static int DefaultMasterPort = 5055;
```

## Solutions

### Solution 1: Run Photon Server on Your PC (RECOMMENDED)

This is simpler and doesn't require APK modifications:

1. **Download Photon Server Community Edition**
   - https://www.photonengine.com/en-US/OnPremise
   - Download and extract

2. **Start the Photon Server**
   ```
   PhotonControl.exe
   ```
   - It should show "Running" status
   - Default settings work fine

3. **Point game to your PC's server**
   - Phone creates room → automatically tries to connect to cloud
   - BUT if Photon Server is running on your PC at 127.0.0.1:5055...
   
   Actually, this won't work either because the game doesn't know to connect to PC's IP, it only tries cloud.

### Solution 2: Modify APK to Use SelfHosted Mode (COMPLEX)

This requires:
1. Decompile APK to get game assets
2. Find the PhotonServerSettings configuration
3. Change from `PhotonCloud` to `SelfHosted`
4. Point to phone's IP or your PC's IP
5. Recompile and reinstall

The issue: PhotonServerSettings is a serialized Unity asset file (binary), not easily editable in text form.

### Solution 3: Patch the Game Code at Runtime (IF POSSIBLE)

If the game loads settings from PlayerPrefs or config file on first run, you could potentially inject the values via:
```csharp
// Pseudocode - this might work if game checks PlayerPrefs for these
PlayerPrefs.SetString("PhotonServerAddress", "192.168.18.X");
PlayerPrefs.SetInt("PhotonServerPort", 5055);
PlayerPrefs.SetString("PhotonHostType", "SelfHosted");
```

But this requires:
1. Making the APK debuggable
2. Running ADB shell commands to set PlayerPrefs before first launch
3. Restarting the app

## What's Happening Now

When you create a room on the phone:
1. Game calls `PhotonNetwork.ConnectUsingSettings()`
2. Settings say "Use PhotonCloud"
3. Game tries to reach `app.exitgamescloud.com`
4. If that works, room appears in cloud for other devices
5. Ports 5055/5056 NEVER open because game isn't listening, it's connecting OUT

This is why your scanner found no open ports on the phone.

## Recommended Next Step

**Install Photon Server on your PC**, then modify BOTH game APKs to:
- Connect to `<your_pc_ip>:5055` instead of cloud
- Set to `SelfHosted` mode

The modification requires either:
- Finding where settings are loaded and patching the binary
- Or extracting/modifying the PhotonServerSettings asset file

Would you like me to help with one of these approaches?

