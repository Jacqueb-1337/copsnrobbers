# Cops n Robbers - Complete Photon Server Setup Guide

## Overview

This guide walks you through setting up a **real Photon Server** (the actual infrastructure used by the game) to enable local multiplayer gaming without internet.

**What you'll get:**
- ✅ Working local multiplayer on your WiFi network
- ✅ Same netcode as official game
- ✅ Free license for indie projects
- ✅ No internet required
- ✅ Phone + PC on same network = instant connection

---

## What is Photon?

**Photon** is the multiplayer framework used by Cops n Robbers. There are two versions:

| Component | Status | What It Was |
|-----------|--------|------------|
| **Photon Cloud** | ❌ Shut down (2023) | Online servers for matchmaking |
| **Photon Server (LTS)** | ✅ Active | Self-hosted on your PC |

The game connects to Cloud servers by default. We'll patch it to connect to your local Photon Server instead.

---

## Prerequisites

- **Windows PC** (10 or 11)
- **Python 3.7+** installed
- **apktool** installed
- **Java** (jarsigner - included with Java JDK)
- **USB cable** for phone
- **Android phone** with USB debugging enabled
- **Both on same WiFi network**

### Quick Check (PowerShell)

```powershell
python --version          # Should show 3.7+
apktool --version         # Should show version
jarsigner -version        # Should show version
```

---

## Installation Steps

### Step 1: Quick Setup (Automated)

Run the automated setup script:

```powershell
cd d:\Projects\copsnrobbers
.\SETUP.bat
```

This will guide you through the entire process interactively.

**Or proceed manually below:**

---

### Step 2: Download Photon Server (Manual)

1. Go to: https://www.photonengine.com/en-US/OnPremise
2. Click **"Download SDK"** button
3. Download `Photon-OnPremise-Server-SDK.zip`
4. Extract to: `d:\Projects\copsnrobbers\photon_setup\PhotonServer\`

Your directory structure should look like:
```
photon_setup/
└── PhotonServer/
    ├── bin_Win64/
    ├── bin_Win32/
    ├── deploy/
    └── ...
```

---

### Step 3: Get Free Photon License

1. Go to: https://dashboard.photonengine.com/
2. Sign up (completely free for indie projects)
3. Log in to dashboard
4. Go to **"Self-Hosted"** section
5. Click **"Create License"**
6. Select **"Indie License"** (free)
7. Accept terms
8. Download the `.license` file

**Install the license:**

1. Find the downloaded `.license` file (e.g., `my_photon.license`)
2. Copy it to: `photon_setup\PhotonServer\bin_Win64\`

That's it! Photon will read it automatically.

---

### Step 4: Determine Your PC's Local IP

Open PowerShell and run:

```powershell
ipconfig
```

Look for your **WiFi adapter's IPv4 Address**, e.g.:
```
IPv4 Address. . . . . . . . . . . : 192.168.1.100
```

**Remember this IP!** We'll use it in the next step.

---

### Step 5: Patch the APK

Patch the game APK to connect to your local Photon server:

```powershell
python patch_apk_for_local_server.py target_compiled_final.apk `
  --server-ip 192.168.1.100 `
  --output cnr_patched
```

Replace `192.168.1.100` with **your actual PC IP** from Step 4.

You'll see:
```
[✓] APK found: target_compiled_final.apk
[✓] Patched 12 files with new server address
[✓] Recompiled: cnr_patched_unsigned.apk
[✓] Signed: cnr_patched.apk
[✓] Patching complete!
```

---

### Step 6: Install Patched APK on Phone

Connect your Android phone via USB and run:

```powershell
adb uninstall com.joydo.minestrikenew
Start-Sleep -Seconds 2
adb install d:\Projects\copsnrobbers\cnr_patched.apk
```

Or with auto-install flag:
```powershell
python patch_apk_for_local_server.py target_compiled_final.apk `
  --server-ip 192.168.1.100 `
  --install
```

You should see: `Success`

---

### Step 7: Start Photon Server

**Option A: Double-click batch file**
```
d:\Projects\copsnrobbers\run_photon_server.bat
```

**Option B: Manual start**
```powershell
d:\Projects\copsnrobbers\photon_setup\PhotonServer\bin_Win64\PhotonControl.exe
```

PhotonControl window will appear. You should see:
- Server status: **"Running"** (green indicator)
- Port: **5055**
- Listen IP: Your PC's IP

---

### Step 8: Test Connection

1. **On your phone:**
   - Make sure it's on the **SAME WiFi** as your PC
   - Launch **Cops n Robbers**
   - Go to **Multiplayer**
   - Tap **Worldwide** (or Local WiFi if available)

2. **What to expect:**
   - Game should connect instantly (green "Connected" message)
   - Room list should appear
   - You can create or join rooms

3. **In PhotonControl window, you'll see:**
   ```
   [INFO] Client connected: 192.168.1.50:12345
   [INFO] Player joined room: MyRoom
   ```

---

## Firewall Configuration

If your phone **cannot connect** to the server, open firewall ports:

### PowerShell (Run as Administrator)

```powershell
New-NetFirewallRule -DisplayName "Photon Game Server" `
  -Direction Inbound -Action Allow -Protocol UDP -LocalPort 5055

New-NetFirewallRule -DisplayName "Photon Game Server (TCP)" `
  -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5055
```

### Verify

```powershell
netstat -an | findstr 5055
```

Should show:
```
UDP    0.0.0.0:5055    *.*    LISTENING
```

---

## Troubleshooting

### "Photon won't start"

**Problem:** PhotonControl closes immediately

**Solution:**
1. Check license is in: `photon_setup\PhotonServer\bin_Win64\*.license`
2. Verify license is not expired (check dashboard)
3. Check Windows Firewall isn't blocking
4. Try running as Administrator

### "Phone can't find server"

**Problem:** Game shows "Connection Failed" on phone

**Checks:**
1. Both on same WiFi? (not different networks)
2. Server running? (check PhotonControl window is open)
3. Correct IP patched? (check APK patching step)
4. Firewall blocking? (see Firewall section above)

**Test connection:**
```powershell
adb shell ping 192.168.1.100
```

Should show responses (not "Name or service not known")

### "APK installation fails"

**Problem:** `adb install` returns error

**Solution:**
1. Connect phone via USB
2. Enable USB Debugging: Settings → Developer Options → USB Debugging
3. Trust computer on phone prompt
4. Try again

### "Server shows 0 players"

This is normal! Players only appear when they:
- Join a room
- Participate in active game
- Appear in the player list during gameplay

---

## Advanced: Multiple Phones

**To connect multiple phones:**

1. Patch APK for each phone (same IP)
2. Install patched APK on each
3. Connect all to same WiFi
4. Launch game on each
5. One player creates room, others join

All players will see each other in multiplayer lobbies!

---

## Architecture Diagram

```
┌─────────────────────────────────────────────┐
│  Windows PC - Running Photon Server         │
│  IP: 192.168.1.100                          │
│  ┌────────────────────────────────────────┐ │
│  │  PhotonControl.exe                     │ │
│  │  ├─ Master Server (Room management)    │ │
│  │  ├─ Game Server (Player sync)          │ │
│  │  └─ TCP/UDP on port 5055               │ │
│  └────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
              ▲
              │ WiFi (UDP 5055)
              │
       ┌──────┴──────┐
       ▼             ▼
    Phone 1       Phone 2
    (Patched      (Patched
     APK)          APK)
```

---

## What's Happening Under the Hood

1. **Original game:** Connects to `app.exitgamescloud.com` (offline)
2. **Patched game:** Connects to `192.168.1.100` (your PC)
3. **Your PC:** Runs Photon Server that the game expects
4. **Result:** Game works perfectly with local multiplayer

---

## File Structure After Setup

```
d:\Projects\copsnrobbers\
├── SETUP.bat                          ← Run this first!
├── run_photon_server.bat              ← Start server here
├── patch_apk_for_local_server.py      ← Used by SETUP.bat
├── cnr_patched.apk                    ← Install this on phone
├── target_compiled_final.apk          ← Original (unchanged)
└── photon_setup\
    └── PhotonServer\                  ← Photon installation
        ├── bin_Win64\
        │   ├── PhotonControl.exe      ← Server GUI
        │   ├── my_photon.license      ← License file
        │   └── PhotonSocketServer.exe ← Server executable
        └── deploy\
            └── LoadBalancing\         ← Server config
```

---

## Common Questions

**Q: Do I need internet while playing?**  
A: No! Everything runs locally. Internet completely optional.

**Q: How many players can connect?**  
A: Default is 10-16 per room, unlimited rooms. Your PC will be the bottleneck (typically 50-100+ concurrent players on modern PC).

**Q: Can I play from outside my house?**  
A: Not with this setup (broadcast doesn't cross router). Would need port forwarding or VPN.

**Q: What if the server crashes?**  
A: Just restart PhotonControl. Current games disconnect but can reconnect immediately.

**Q: Is this the same as the online servers?**  
A: Yes! Same Photon infrastructure, just running on your PC instead of Exit Games' cloud.

---

## Next Steps

1. ✅ Run `SETUP.bat`
2. ✅ Get free Photon license
3. ✅ Start `run_photon_server.bat`
4. ✅ Launch game on phone
5. ✅ Play local multiplayer! 🎮

---

## Support & Resources

- **Photon Docs:** https://doc.photonengine.com/server/current/
- **Get License:** https://dashboard.photonengine.com/
- **Download SDK:** https://www.photonengine.com/en-US/OnPremise
- **Exit Games Support:** https://www.photonengine.com/contact

---

## Summary

You now have:
- ✅ Photon Server running on your PC
- ✅ Patched APK pointing to your local server
- ✅ Phone connected to same WiFi
- ✅ Complete local multiplayer setup

**Go play Cops n Robbers locally!** 🚔👮‍♂️
