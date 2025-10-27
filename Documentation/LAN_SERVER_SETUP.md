# Cops n Robbers LAN Server Setup Guide

## Overview
This guide sets up a standalone LAN server that your phone can connect to for multiplayer Cops n Robbers gameplay.

## Two Implementation Options

### OPTION 1: Photon On-Premise Server (RECOMMENDED - Easier)
The game already uses Photon PUN v1.17, so using Photon's own server software is the simplest approach.

**Requirements:**
- Windows PC or Linux server
- Java Runtime (for Photon Server)
- Network connectivity between PC and phone

**Setup Steps:**

1. Download Photon Server Community Edition (free)
   - Visit: https://www.photonengine.com/en-US/OnPremise
   - Download: PhotonServer (Community Edition)
   - Extract to: `C:\PhotonServer\` (recommended)

2. Configure Photon Server
   - Edit: `C:\PhotonServer\deploy\LoadBalancing\Master.xml`
   - Look for the Application configuration
   - Ensure UDP port 5055 is open

3. Start the Photon Server
   - On Windows: Run `C:\PhotonServer\bin_Win64\PhotonControl.exe`
   - Or use command line: `java -jar photon.jar`
   - Server listens on: `0.0.0.0:5055`

4. Modify Game APK
   - Point to local server: `127.0.0.1:5055`
   - See: **MODIFYING THE GAME APK** section below

---

### OPTION 2: Custom .NET Server (Advanced - More Control)
Build your own server in C# for custom game logic.

**Pros:**
- Full control over game logic
- No external dependencies
- Can implement custom features

**Cons:**
- More development required
- Must implement Photon protocol or custom UDP protocol

See: **CUSTOM SERVER IMPLEMENTATION** section below

---

## MODIFYING THE GAME APK

### Step 1: Extract the APK
```powershell
apktool d target_compiled_final.apk -o game_apk_modified -f
cd game_apk_modified
```

### Step 2: Locate ServerSettings
The game loads Photon settings from:
`assets/bin/Data/Managed/Assembly-CSharp.dll`

Unfortunately, compiled DLLs can't be directly edited. Instead, we'll modify at the manifest/configuration level.

### Step 3: Alternative - Patch via Smali
Look for smali files that initialize PhotonNetwork connection. Search for:
```
invoke-static {v0, v1, v2}, LPhotonNetwork;->Connect(...)
```

We can patch these to hardcode `127.0.0.1:5055` instead of reading settings.

### Step 4: Find and Patch the Connection Code
In the extracted APK, search for Photon initialization:
```bash
grep -r "ConnectUsingSettings\|Connect(" --include="*.smali"
```

### Step 5: Rebuild and Test
```powershell
apktool b game_apk_modified -o game_modified.apk
jarsigner -keystore debug.keystore -storepass android game_modified.apk debugkey
adb install -r game_modified.apk
```

---

## QUICK START: Photon Community Server

### Windows Installation (5 minutes):

1. **Download**
   ```powershell
   # Visit: https://downloads.agora.io/Photon/OnPremise
   # Or: https://exit-games.com/en/Download
   # Download PhotonServer_v4.0.29.11126 (or latest)
   ```

2. **Extract and Run**
   ```powershell
   # Extract to C:\PhotonServer\
   cd C:\PhotonServer\bin_Win64
   # Double-click: PhotonControl.exe
   # Or run from command:
   .\PhotonControl.exe
   ```

3. **Verify Running**
   - Photon should be listening on `localhost:5055`
   - Check: `netstat -an | findstr 5055`

4. **Open Firewall Port**
   ```powershell
   # Allow UDP 5055 for LAN connections
   New-NetFirewallRule -DisplayName "Photon Server" `
     -Direction Inbound -Action Allow `
     -Protocol UDP -LocalPort 5055
   ```

5. **Modify Game APK to Connect to Local Server**
   - See instructions in section below

---

## IMPORTANT: APK Modification Strategy

Since we can't directly edit compiled C# DLLs, here are practical options:

### Option A: Man-in-the-Middle Proxy (EASIEST)
1. Run a local proxy on your PC that intercepts UDP traffic
2. When game connects to Photon Cloud, redirect to local server
3. Tools: Charles, mitmproxy, or custom Python script

### Option B: Network Routing
1. Configure your router's DNS to point `app.exitgamescloud.com` to `127.0.0.1`
2. Game thinks it's connecting to cloud, actually connects locally

### Option C: Smali Bytecode Patching (MOST RELIABLE)
1. Find the `PhotonNetwork.Connect()` calls in smali
2. Replace hardcoded IP addresses in smali files
3. Rebuild APK

---

## Game State Synchronization

When multiplayer matches start, the game synchronizes:

**Per-Player Data** (synced every 100ms):
- Position (X, Y, Z)
- Rotation
- Animation state (running, jumping, shooting)
- Weapon equipped
- Health/status

**Per-Match Data** (synced on events):
- Player deaths (sent to all via RPC)
- Score updates
- Match start/end
- Game mode settings

**Photon RPC Methods** used:
- `networkAddMessage()` - Chat/kill messages
- `FireOnline()` - Shooting events
- `OnSerialize()` - Continuous state sync
- `RemoveRPCs()` - Cleanup on disconnect

---

## Testing Your Server Setup

### Test 1: Verify Server Running
```powershell
Test-NetConnection -ComputerName 127.0.0.1 -Port 5055
```

### Test 2: Check Firewall
```powershell
Get-NetFirewallRule -DisplayName "Photon Server"
```

### Test 3: Monitor Connections
```powershell
netstat -an | findstr 5055
```

### Test 4: Phone Connection
1. Find your PC's IP: `ipconfig | findstr IPv4`
2. Modify APK to connect to `<YOUR_PC_IP>:5055`
3. Install on phone
4. Try creating a room - should connect to local server

---

## Files You'll Need

**From your project:**
- `target_compiled_final.apk` - The working game version
- `debug.keystore` - For signing modified APKs

**To Download:**
- PhotonServer Community Edition
- Java Runtime Environment (JRE)

---

## Next Steps

1. **Choose your approach** (Photon Server vs Custom Server)
2. **Download Photon Server** if using Option 1
3. **Configure APK modification** strategy
4. **Test connection** from phone to PC
5. **Enjoy LAN multiplayer!**

---

## Troubleshooting

**Q: Game connects to Photon Cloud instead of local?**
A: Verify APK was modified correctly. Check smali files for hardcoded IP addresses.

**Q: Can't connect from phone to PC?**
A: 
- Verify PC firewall allows UDP 5055
- Check phone and PC are on same network
- Use phone's IP address, not localhost
- Disable VPN/proxy if active

**Q: Server crashes on startup?**
A: 
- Check Java is installed
- Verify port 5055 isn't already in use
- Check Photon configuration files

**Q: Game connects but no players visible?**
A: 
- Verify RPC handlers are working
- Check game version/app ID matches
- Look at server logs for errors

