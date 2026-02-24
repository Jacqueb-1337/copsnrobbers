# Cops n Robbers - Local LAN Server Setup Guide

## Overview

This guide walks you through setting up a local multiplayer server for Cops n Robbers on your PC, allowing your Android phone to play on a local network without internet.

## Architecture

```
┌─────────────────────────────────────────┐
│   Your Windows PC                       │
│   ┌─────────────────────────────────┐   │
│   │ Photon-Compatible Server        │   │
│   │ (photon_compat_server.py)        │   │
│   │ Listens on UDP 5055              │   │
│   └─────────────────────────────────┘   │
│                                         │
│   192.168.18.100                        │  ← Configure this IP
└─────────────────────────────────────────┘
         ▲
         │ WiFi Network (5055 UDP)
         │
         ▼
┌─────────────────────────────────────────┐
│   Android Phone                         │
│   ┌─────────────────────────────────┐   │
│   │ Cops n Robbers (Patched)        │   │
│   │ Points to 192.168.18.100:5055   │   │
│   └─────────────────────────────────┘   │
└─────────────────────────────────────────┘
```

## Requirements

- Python 3.7+ on Windows PC
- apktool installed (`choco install apktool` or download manually)
- jarsigner (included with Java)
- adb (Android Debug Bridge)
- Same WiFi network for PC and phone
- USB debugging enabled on Android phone

## Step-by-Step Setup

### Step 1: Find Your PC's Local IP

Open PowerShell and run:
```powershell
ipconfig
```

Look for your WiFi adapter's IPv4 Address (usually starts with 192.168.x.x)

Example: `192.168.18.100`

### Step 2: Start the Server

```powershell
cd d:\Projects\copsnrobbers
python photon_compat_server.py
```

You should see:
```
╔════════════════════════════════════════════╗
║  Photon-Compatible LAN Server v1.0        ║
║  Minimal Photon Protocol Implementation    ║
╚════════════════════════════════════════════╝

[✓] Server started on 0.0.0.0:5055
[✓] Listening for UDP connections...
```

**Keep this running in the background.**

### Step 3: Patch the APK

In a **new PowerShell window**:

```powershell
cd d:\Projects\copsnrobbers
python patch_apk_for_local_server.py target_compiled_final.apk --server-ip 192.168.18.100 --output cnr_local_patched
```

Replace `192.168.18.100` with your actual PC IP from Step 1.

You'll see:
```
[✓] APK found: target_compiled_final.apk
[✓] Decompiled to: patch_work_xxx/decompiled
[✓] Patched 12 files with new server address
[✓] Recompiled: patch_work_xxx/cnr_local_patched_unsigned.apk
[✓] Signed: cnr_local_patched.apk
[✓] Patching complete!
[✓] Output: cnr_local_patched.apk
```

### Step 4: Install Patched APK on Phone

```powershell
adb uninstall com.joydo.minestrikenew
Start-Sleep -Seconds 2
adb install d:\Projects\copsnrobbers\cnr_local_patched.apk
```

Or use the patcher with `--install` flag:
```powershell
python patch_apk_for_local_server.py target_compiled_final.apk --server-ip 192.168.18.100 --install
```

### Step 5: Test Connection

On your Android phone:
1. Make sure it's connected to the **same WiFi** as your PC
2. Launch Cops n Robbers
3. Go to Multiplayer → Worldwide (or Local WiFi if available)
4. The game should connect to your local server

In the server window, you'll see:
```
[12:34:56] → Connection from 192.168.18.24:12345
[12:34:57] → Room created: MyRoom | Map: Map1 | Master: Player_12345
```

## Firewall Configuration

If the phone can't connect, you may need to open firewall ports:

```powershell
# Run PowerShell as Administrator
New-NetFirewallRule -DisplayName "Cops n Robbers" `
  -Direction Inbound -Action Allow -Protocol UDP -LocalPort 5055
```

## Server Commands

Once running, you can type commands in the server window:

- `s` - Show server status (connected players, rooms)
- `q` - Quit server

Example output:
```
============================================================
CLIENTS: 1
  192.168.18.24:12345: Player_12345 [✓]

ROOMS: 1
  MyRoom: 1/10 players (Map: Map1)
    [M] Player_12345
============================================================
```

## Protocol Details

The server implements a minimal version of Photon's UDP protocol:

| Message Type | Code | Description |
|---|---|---|
| Ping/Handshake | 0x00 | Keep-alive |
| InitRequest | 0xF3 | Initial connection |
| AuthRequest | 0xF4 | Authentication |
| JoinRoom | 0xF5 | Join existing room |
| CreateRoom | 0xF6 | Create new room |
| SendRPC | 0xF7 | Remote procedure call |
| StateSync | 0xF8 | Player state update |

All messages use simple JSON encoding for payload.

## Troubleshooting

### Phone can't find server
- Verify both PC and phone are on **same WiFi network**
- Check PC IP: `ipconfig` (WiFi adapter)
- Check server is running: Look for `[✓] Server started` message
- Test connection: `adb shell ping 192.168.18.100` (replace IP)

### Server crashes
- Make sure port 5055 is not in use: `netstat -an | findstr 5055`
- Check firewall isn't blocking UDP 5055
- Try running as Administrator

### Game still connects to internet
- Verify APK patching succeeded (check patch output)
- Try patching again with correct server IP
- Uninstall and reinstall the patched APK

### Multiple devices
- Each phone needs the patched APK pointing to your PC's IP
- Server supports up to 20 concurrent players

## Advanced Usage

### Custom Server IP on Phone

If you have multiple PCs and want to switch servers without repatching:

Server can be started on any IP:
```powershell
python photon_compat_server.py --bind 0.0.0.0 --port 5055
```

Then repatch APK with different IP.

### Debugging

Enable verbose output:
```powershell
python photon_compat_server.py --debug
```

Monitor network traffic:
```powershell
netstat -an | findstr 5055
```

## Limitations

- Single local network only (not over internet)
- Broadcasts don't cross router boundaries
- No authentication/player profiles
- Simple state synchronization (not optimized for large worlds)
- UDP only (reliable delivery not guaranteed)

## Next Steps

If you want to expand this:

1. **Add authentication** - Implement login system
2. **Add matchmaking** - Create room browser
3. **Add team balancing** - Auto-fill teams
4. **Add chat** - In-game messaging
5. **Add statistics** - Track wins/losses
6. **Add voice** - VoIP integration

## References

- Photon Pun v1.17 (Game uses this)
- UDP Protocol (5055 default port)
- apktool (APK decompilation/recompilation)

## Support

If you encounter issues:

1. Check server console output (detailed error messages)
2. Verify network connectivity: `ping 192.168.18.100`
3. Check firewall rules
4. Reinstall patched APK fresh
5. Restart server and try again
