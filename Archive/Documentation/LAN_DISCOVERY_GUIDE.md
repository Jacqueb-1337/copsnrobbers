# Cops n Robbers - LAN Discovery (No APK Patching)

## How It Works

**Photon has built-in LAN discovery.** Here's what happens:

```
1. Photon Server starts on PC
   └─ Listens on UDP 5055 (game traffic)
   └─ Broadcasts on UDP 5056 (discovery beacon)

2. Phone connects to same WiFi
   └─ Game automatically scans for Photon servers
   └─ Phone detects your PC's broadcast beacon
   └─ Server appears in "Available Rooms" list

3. Phone connects to your PC server
   └─ NO configuration needed
   └─ NO APK patching needed
   └─ Works automatically!
```

---

## Setup Steps (No Patching Required!)

### Step 1: Download & Install Photon Server

1. Visit: https://www.photonengine.com/en-US/OnPremise
2. Download: **PhotonServer Community Edition** (free)
3. Extract to: `C:\PhotonServer\`

### Step 2: Open Firewall Ports

**Run as Administrator** in PowerShell:

```powershell
# Port 5055 - Game communication
New-NetFirewallRule -DisplayName "Photon Game" `
  -Direction Inbound -Action Allow -Protocol UDP -LocalPort 5055

# Port 5056 - Discovery broadcast
New-NetFirewallRule -DisplayName "Photon Discovery" `
  -Direction Inbound -Action Allow -Protocol UDP -LocalPort 5056

# Verify they were created
Get-NetFirewallRule | findstr "Photon"
```

### Step 3: Start Photon Server

```
C:\PhotonServer\bin_Win64\PhotonControl.exe
```

Click "Start" to begin the server. You should see:
```
Server started
[2025-10-26 12:00:00] Loading applications...
[2025-10-26 12:00:00] Master: photon instance started
```

### Step 4: Verify It's Broadcasting

```powershell
# Check that ports are open and listening
netstat -an | findstr 5055
netstat -an | findstr 5056

# Should show:
# UDP    0.0.0.0:5055    0.0.0.0:0    LISTENING
# UDP    0.0.0.0:5056    0.0.0.0:0    LISTENING
```

### Step 5: Install Game on Phone (UNMODIFIED)

```powershell
# Use the ORIGINAL game APK - no patching!
adb install target_compiled_final.apk
```

### Step 6: Test Discovery

1. **Make sure phone is on same WiFi as PC**
2. **Launch the game** on phone
3. **Go to Multiplayer section**
4. **Look for available rooms** - your PC's server should appear!

---

## Why This Works (Technical)

### Photon's Built-in Discovery Mechanism

Photon Community Server includes **UDP broadcast discovery**:

- **Discovery Port**: UDP 5056
- **Broadcast Interval**: Every few seconds
- **Discovery Message**: Server announces itself to local network
- **Client Behavior**: Game automatically listens for broadcasts

### How the Game Finds Your Server

When you select "Multiplayer" → "Join Room", the game:

1. **Sends UDP broadcast query** on port 5056
2. **Photon server responds** with "I'm here!"
3. **Game receives response** and displays available rooms
4. **You select a room** and connect on port 5055

### Why No APK Patching Is Needed

- ✅ Photon discovery is automatic
- ✅ Phone doesn't need to know server IP beforehand
- ✅ Phone scans local network for servers
- ✅ Server broadcasts its presence
- ✅ They find each other automatically!

---

## Network Setup

### Single WiFi Network (Easiest)

```
WiFi Router (192.168.1.1)
├─ PC Server (192.168.1.100:5055)
├─ Phone 1 (192.168.1.50)
├─ Phone 2 (192.168.1.51)
└─ Phone N (192.168.1.x)

✓ All on same network = automatic discovery
```

### Multiple Networks (Won't Work)

```
WiFi Network 1 (192.168.1.x)
└─ PC Server (192.168.1.100:5055)

WiFi Network 2 (192.168.2.x)
└─ Phone (192.168.2.50)

✗ Broadcast doesn't cross networks/routers
✗ Phone won't see server
```

### Solution for Multiple Networks
- Connect all devices to same WiFi network
- OR use your PC's IP address (APK patching then becomes needed)
- OR use a dedicated server (always accessible)

---

## Troubleshooting

### Issue: Rooms don't appear on phone

**Check 1: Is Photon running?**
```powershell
netstat -an | findstr 5055
```
If nothing shows, Photon isn't running. Start it.

**Check 2: Is firewall allowing broadcasts?**
```powershell
Get-NetFirewallRule | findstr "Photon"
```
If rules don't exist, add them (see Step 2 above).

**Check 3: Are phone and PC on same WiFi?**
- Check WiFi name on phone
- Check WiFi name on PC
- Must be the same network

**Check 4: Wait a few seconds**
- Discovery isn't instant
- Phone might take 5-10 seconds to scan
- Click "Refresh" button if available

### Issue: Phone can't connect after discovering server

**Likely cause**: Firewall blocking port 5055

```powershell
# Verify inbound UDP 5055 is allowed
Get-NetFirewallRule -DisplayName "Photon Game" | fl

# Should show:
# Direction       : Inbound
# Action          : Allow
# Enabled         : True
# Protocol        : UDP
```

### Issue: Only PC can see rooms, but other phones can't

**Possible causes**:
1. Other phones on different WiFi
2. Firewall has MAC address filtering
3. Port forwarding interfering with broadcast
4. Broadcast domain issue (different subnets)

**Solution**: 
- Verify all phones same WiFi SSID
- Check router firewall settings
- Try with all devices closer to router

---

## How to Monitor Server

### View Connected Players

**In Photon Server GUI:**
- Click "Master Server"
- Look for connected players list
- Shows: Player name, IP, connection time

### View Server Logs

**Log file location:**
```
C:\PhotonServer\log\PhotonServer.log
```

**Monitor in real-time:**
```powershell
Get-Content "C:\PhotonServer\log\PhotonServer.log" -Tail 20 -Wait
```

**Look for:**
```
[2025-10-26 12:05:15] App: Player1 connected from 192.168.1.50
[2025-10-26 12:05:20] Game: Player2 joined Room1
[2025-10-26 12:05:45] Game: Kill event - Player1 eliminated Player2
```

---

## Performance Tips

### Server Configuration

Edit `C:\PhotonServer\deploy\LoadBalancing\Master.xml`:

```xml
<!-- Increase network buffer for larger players -->
<NetFrameworkConnectionFactory>
  <FrameSize>16000</FrameSize>
  <RecvBufferSize>131072</RecvBufferSize>
  <SendBufferSize>131072</SendBufferSize>
</NetFrameworkConnectionFactory>

<!-- Increase max concurrent connections -->
<MaxConcurrentClient>100</MaxConcurrentClient>
```

### Network Optimization

```powershell
# Reduce network jitter (Windows only)
# Prioritize gaming traffic in QoS settings

# Router configuration:
# - Enable QoS
# - Prioritize port 5055 traffic
# - Reduce WiFi congestion (use 5GHz if available)
```

---

## Comparison: With vs Without APK Patching

### Using LAN Discovery (This Guide)
```
✓ No APK modifications needed
✓ Use original, unmodified game
✓ Phone auto-discovers server
✓ Simple setup
✓ Works on same WiFi
✗ Doesn't work across networks/internet
```

### Using APK Patching (Original Guide)
```
✓ Works across networks/internet
✓ Can specify exact server IP
✓ More control over connection
✗ Requires APK modification
✗ Requires re-signing APK
✗ More complex setup
```

**Recommendation**: Use **LAN Discovery** for local play with friends on same WiFi. Much simpler!

---

## Quick Reference

```powershell
# 1. Create firewall rules (run once)
New-NetFirewallRule -DisplayName "Photon Game" `
  -Direction Inbound -Action Allow -Protocol UDP -LocalPort 5055
New-NetFirewallRule -DisplayName "Photon Discovery" `
  -Direction Inbound -Action Allow -Protocol UDP -LocalPort 5056

# 2. Verify firewall rules
Get-NetFirewallRule | findstr "Photon"

# 3. Start Photon Server
C:\PhotonServer\bin_Win64\PhotonControl.exe

# 4. Verify server is running
netstat -an | findstr "5055\|5056"

# 5. Install game (unmodified)
adb install target_compiled_final.apk

# 6. Test
# Launch game on phone → Multiplayer → Look for rooms
```

---

## Summary

**With Photon's built-in LAN discovery:**
- Phone automatically finds your PC's server
- No APK patching required
- No configuration needed on phone
- Just install the original game and it works!

**All you need:**
1. ✅ Photon Server running on PC
2. ✅ Firewall ports open (5055, 5056)
3. ✅ Phone on same WiFi network
4. ✅ Original, unmodified game APK

**That's it!** 🎮

