# Cops n Robbers - Complete LAN Server Integration Guide

## Overview
This guide walks you through setting up a complete LAN server for Cops n Robbers multiplayer gaming.

**What you'll accomplish:**
1. ✅ Start a local game server on your PC
2. ✅ Modify the game APK to connect to your server
3. ✅ Test multiplayer on your local network
4. ✅ Play with other phones on your LAN

---

## Choose Your Server Type

### Option A: Photon Community Server (Recommended - Easier)
- **Pros**: Official Photon software, battle-tested, minimal setup
- **Cons**: External dependency
- **Setup time**: 10 minutes
- **Difficulty**: ⭐⭐ (Easy)

### Option B: Custom C# Server (Experimental)
- **Pros**: Full control, no external dependencies
- **Cons**: Custom protocol implementation
- **Setup time**: 30 minutes
- **Difficulty**: ⭐⭐⭐ (Moderate)

---

## OPTION A: Using Photon Community Server

### Step 1: Download Photon Server
1. Visit: https://www.photonengine.com/en-US/OnPremise
2. Click "Download" for **PhotonServer Community Edition**
3. Extract to: `C:\PhotonServer\` (or your preferred location)

### Step 2: Start Photon Server

**Windows:**
```powershell
# Navigate to Photon installation
cd C:\PhotonServer\bin_Win64

# Run PhotonControl.exe (GUI) or:
# Start service from command line
.\PhotonControl.exe
```

**Verify it's running:**
```powershell
# Check if Photon is listening on port 5055
netstat -an | findstr 5055

# Should show: UDP 0.0.0.0:5055 LISTENING
```

### Step 3: Open Firewall (Important!)

**Windows Firewall:**
```powershell
# Allow incoming UDP on port 5055
New-NetFirewallRule -DisplayName "Photon Server LAN" `
  -Direction Inbound `
  -Action Allow `
  -Protocol UDP `
  -LocalPort 5055

# Verify
Get-NetFirewallRule -DisplayName "Photon Server LAN"
```

### Step 4: Modify Game APK

**Using the Python patcher script:**
```powershell
cd d:\Projects\copsnrobbers

# Find your PC's local IP
ipconfig | findstr IPv4

# Run patcher with your PC's IP (use 127.0.0.1 for testing on same PC)
python apk_patch_server.py `
  target_compiled_final.apk `
  game_localhost.apk `
  127.0.0.1 `
  5055
```

**Output:**
```
============================================================
Cops n Robbers - APK Server Patcher
============================================================

[1/4] Extracting APK...
[2/4] Searching for server configuration...
  Found: ServerSettings.smali
[3/4] Patching configuration...
  Patched: ServerSettings.smali
  Total patches applied: 3
[4/4] Rebuilding APK...
  Signing APK...

============================================================
✓ Patching complete!
============================================================

Modified APK: game_localhost.apk
Server configured: 127.0.0.1:5055
```

### Step 5: Install on Phone

```powershell
# Uninstall old version
adb uninstall com.joydo.minestrikenew

# Install modified version
adb install game_localhost.apk
```

### Step 6: Test Connection

1. **Start Photon Server** (if not already running)
2. **Launch the game** on your phone
3. **Check server logs** for connection messages:
   ```
   [→] Player connected: Player123 (192.168.1.100:xxxxx)
   [→] Room created: Room_ABC123 (Master: Player123)
   [→] Player456 joined Room_ABC123 (2/20)
   ```

**Success indicators:**
- ✅ Game loads without timeout
- ✅ Can create a multiplayer room
- ✅ Other players can see and join the room
- ✅ See kill messages/player events

---

## OPTION B: Using Custom C# Server

### Step 1: Compile the Server

**Requirements:**
- .NET Framework 4.7+ or .NET Core 3.1+

**Compile:**
```powershell
cd d:\Projects\copsnrobbers

# If .NET is available
csc CopsNRobbersServer.cs -out:CopsNRobbersServer.exe

# Or using .NET 5+
dotnet new console -n cnr_server
# Copy CopsNRobbersServer.cs into project
dotnet build -c Release
```

**Alternative - Run as console app:**
```powershell
# Copy CopsNRobbersServer.cs to Visual Studio
# Create new Console App (.NET Framework)
# Add CopsNRobbersServer.cs
# Build and run
```

### Step 2: Start the Server

```powershell
cd d:\Projects\copsnrobbers
.\CopsNRobbersServer.exe
```

**Expected output:**
```
╔════════════════════════════════════════════╗
║  Cops n Robbers - LAN Server v1.0          ║
║  Local Multiplayer Gaming Server           ║
╚════════════════════════════════════════════╝

[✓] Server started on 0.0.0.0:5055
[✓] Listening for connections...
```

### Step 3: Modify APK

Same as Option A, Step 4:
```powershell
python apk_patch_server.py target_compiled_final.apk game_localhost.apk 127.0.0.1 5055
```

### Step 4: Install and Test

Same as Option A, Steps 5-6

---

## Networking Setup for Multiple Phones

### Scenario: Multiple devices on same WiFi

1. **Find your PC's IP:**
   ```powershell
   ipconfig | findstr IPv4
   # Look for: IPv4 Address . . . : 192.168.x.x
   ```

2. **Modify APK with your PC's IP:**
   ```powershell
   python apk_patch_server.py `
     target_compiled_final.apk `
     game_network.apk `
     192.168.1.100 `
     5055
   ```

3. **Install on each phone:**
   ```powershell
   adb install game_network.apk
   ```

4. **Ensure all phones are on same WiFi network**

5. **Server will show connections:**
   ```
   [→] Player connected: Player1 (192.168.1.50:12345)
   [→] Player connected: Player2 (192.168.1.51:12346)
   ```

---

## Troubleshooting

### Issue: "Connection timeout" on game

**Causes & Solutions:**
1. **Server not running**
   - Verify: `netstat -an | findstr 5055`
   - Solution: Start server

2. **Firewall blocking**
   - Check Windows Firewall rules
   - Create new rule: UDP inbound, port 5055

3. **Wrong IP in APK**
   - Verify IP matches your PC
   - Use `127.0.0.1` for phone on same device
   - Use `192.168.x.x` for other phones

4. **APK patching failed**
   - Re-run patcher script
   - Check apktool is installed: `apktool --version`

### Issue: Players can't see each other

**Causes:**
1. **Different rooms**
   - Both players must create/join same room

2. **Server crashing**
   - Check server console for errors
   - Restart server

3. **Network isolation**
   - Ensure all devices on same WiFi
   - Disable VPN if active

### Issue: APK won't install

**Causes:**
1. **Not signed correctly**
   - Verify: `jarsigner -verify game_localhost.apk`
   - Ensure `debug.keystore` exists

2. **APK corrupted during patching**
   - Re-run patcher
   - Verify original APK: `adb install target_compiled_final.apk`

3. **Installation blocked by OS**
   ```powershell
   # Force install with newer APK
   adb install -r -g game_localhost.apk
   ```

---

## Performance Tuning

### Server Performance (Photon)

Edit `C:\PhotonServer\deploy\LoadBalancing\Master.xml`:

```xml
<!-- Increase max players per room -->
<GameState>
  <MaxPlayers>20</MaxPlayers>
  <GameList>
    <MaxGameList>100</MaxGameList>
  </GameList>
</GameState>

<!-- Increase network buffer -->
<NetFrameworkConnectionFactory>
  <FrameSize>16000</FrameSize>
  <RecvBufferSize>131072</RecvBufferSize>
  <SendBufferSize>131072</SendBufferSize>
</NetFrameworkConnectionFactory>
```

### Client Performance (Game APK)

The custom C# server has tunable parameters in code:
```csharp
private const int SERVER_PORT = 5055;
private const int MAX_PLAYERS = 20;
private const int MAX_ROOMS = 10;
```

Increase `MAX_PLAYERS` and `MAX_ROOMS` as needed.

---

## Monitoring Server Activity

### Photon Server

**Log location:** `C:\PhotonServer\log\`

**Real-time monitoring:**
```powershell
# Watch log file in PowerShell
Get-Content "C:\PhotonServer\log\PhotonServer.log" -Tail 20 -Wait
```

### Custom C# Server

**Monitor connections:**
```powershell
# Watch server console output
# Server prints all connections/disconnections in real-time
```

**Network monitoring:**
```powershell
# See UDP traffic on port 5055
netstat -ano | findstr :5055

# Detailed network stats
Get-NetUDPEndpoint | Where-Object {$_.LocalPort -eq 5055}
```

---

## Next Steps

1. ✅ Choose Photon (Option A) or Custom (Option B) server
2. ✅ Start the server
3. ✅ Patch the APK with server IP
4. ✅ Install on phone
5. ✅ Test: Create room → Have friend join
6. ✅ Play! 🎮

---

## Advanced: Custom Game Protocol

If you want to implement custom game logic beyond basic room management, modify:

**File:** `CopsNRobbersServer.cs`

**Add new command handlers:**
```csharp
case "CUSTOM_COMMAND":
    HandleCustomCommand(clientId, parts);
    break;

private void HandleCustomCommand(string clientId, string[] parts)
{
    // Your custom logic here
    BroadcastToRoom(roomName, "CUSTOM_RESPONSE|data");
}
```

**Modify APK to send commands:**
Use smali bytecode patching or ILSpy C# modification to send custom messages.

---

## Support Files

Located in `d:\Projects\copsnrobbers\`:

- `CopsNRobbersServer.cs` - Custom server source code
- `apk_patch_server.py` - APK patcher script
- `LAN_SERVER_SETUP.md` - Detailed setup guide
- `target_compiled_final.apk` - Original game APK (unmodified)
- `debug.keystore` - Signing key for APKs

---

## Quick Reference

### Start Server (Photon)
```powershell
C:\PhotonServer\bin_Win64\PhotonControl.exe
```

### Start Server (Custom C#)
```powershell
.\CopsNRobbersServer.exe
```

### Patch APK
```powershell
python apk_patch_server.py target_compiled_final.apk game_localhost.apk 127.0.0.1 5055
```

### Install on Phone
```powershell
adb install game_localhost.apk
```

### Check Server Running
```powershell
netstat -an | findstr 5055
```

---

## Questions?

Check these files:
1. `FINDINGS_SERVER_EXTRACTION.md` - Technical architecture details
2. `LAN_SERVER_SETUP.md` - Detailed setup steps
3. `game_code/decompiled_full_csharp/` - Game source code for reference

