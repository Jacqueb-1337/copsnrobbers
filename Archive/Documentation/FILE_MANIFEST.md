# Cops n Robbers - LAN Server Complete File Manifest

## 📋 Quick Navigation

| File | Purpose | Status |
|------|---------|--------|
| `README_LAN_SERVER.md` | START HERE - Overview & quick start | ✅ Ready |
| `SETUP_LAN_SERVER_GUIDE.md` | Complete setup guide (option A & B) | ✅ Ready |
| `FINDINGS_SERVER_EXTRACTION.md` | Technical feasibility analysis | ✅ Ready |
| `LAN_SERVER_SETUP.md` | Quick reference guide | ✅ Ready |

---

## 🖥️ Server Implementation Files

### Custom C# Server
**File:** `CopsNRobbersServer.cs` (447 lines)

**What it does:**
- Listens on UDP port 5055
- Manages game rooms (create, join, leave)
- Synchronizes player state in real-time
- Broadcasts RPC calls to all players
- Handles disconnections and cleanups
- Supports up to 20 concurrent players per server

**Classes:**
- `CopsNRobbersServer` - Main server class
- `GameRoom` - Room management
- `PlayerSession` - Player tracking

**Compilation:**
```powershell
csc CopsNRobbersServer.cs -out:CopsNRobbersServer.exe
```

**Usage:**
```powershell
.\CopsNRobbersServer.exe
```

---

## 🔧 Automation & Configuration Files

### APK Patcher Script
**File:** `apk_patch_server.py` (150 lines)

**What it does:**
- Extracts game APK with apktool
- Finds server configuration in smali bytecode
- Patches server IP and port
- Rebuilds and re-signs APK
- Fully automated, error-handled

**Usage:**
```powershell
python apk_patch_server.py <input_apk> <output_apk> [ip] [port]

Examples:
  python apk_patch_server.py target_compiled_final.apk game_local.apk 127.0.0.1 5055
  python apk_patch_server.py target_compiled_final.apk game_network.apk 192.168.1.100 5055
```

### Setup Automation Script
**File:** `setup_lan_server.ps1` (180 lines)

**What it does:**
- Checks all prerequisites (apktool, adb, jarsigner, etc.)
- Verifies required files exist
- Compiles C# server if needed
- Patches APK automatically
- Installs on connected phone
- Starts server

**Usage:**
```powershell
.\setup_lan_server.ps1 -ServerType custom -ServerIP 127.0.0.1 -ServerPort 5055
.\setup_lan_server.ps1 -ServerType photon -ServerIP 192.168.1.100
```

### Verification Tool
**File:** `verify_setup.py` (300 lines)

**What it does:**
- Checks all tools are installed (apktool, adb, Java, .NET, Python)
- Verifies all required files exist
- Tests network connectivity
- Tests port availability
- Provides detailed error messages

**Usage:**
```powershell
python verify_setup.py
```

---

## 📖 Documentation Files

### Main Getting Started Guide
**File:** `README_LAN_SERVER.md` (250 lines)

**Sections:**
- Overview of what you have
- Quick start (5 minutes)
- Architecture diagram
- Server protocol specification
- File inventory
- Testing procedures
- Troubleshooting
- Next steps

**Read this:** First, for overview and quick start

---

### Complete Setup Guide
**File:** `SETUP_LAN_SERVER_GUIDE.md` (400 lines)

**Sections:**
- Option A: Photon Community Server (step-by-step)
- Option B: Custom C# Server (detailed)
- Multi-device networking setup
- Firewall configuration
- Troubleshooting (7 common issues)
- Performance tuning
- Monitoring & logging

**Read this:** For detailed, step-by-step instructions

---

### Quick Setup (Short Version)
**File:** `LAN_SERVER_SETUP.md` (200 lines)

**Sections:**
- Network architecture
- Server configuration classes
- Room/multiplayer implementation
- Offline mode capability
- Extraction feasibility (3 options)
- Quick reference commands

**Read this:** For quick lookup or decision-making

---

### Technical Analysis
**File:** `FINDINGS_SERVER_EXTRACTION.md` (500+ lines)

**Sections:**
- Executive summary (YES - extraction is possible)
- Key findings (Photon PUN v1.17)
- Server configuration details
- Room/multiplayer implementation
- Game state synchronization
- Code structure (28 networking files found)
- Three extraction approaches with difficulty ratings
- Complete code examples

**Read this:** To understand technical details and architecture

---

## 🎮 Game Code Reference

### Decompiled C# Source
**Directory:** `game_code/decompiled_full_csharp/` (677 files)

**Key files for multiplayer:**
- `PhotonNetwork.cs` (1,061 lines) - Main networking API
- `NetworkingPeer.cs` (2,236 lines) - Low-level protocol
- `LoadbalancingPeer.cs` (208 lines) - Room operations
- `CRMultiplayerManager.cs` (896 lines) - Game multiplayer logic
- `MultiplayerSelectDirector.cs` (733 lines) - Lobby/room UI
- `ConnectMenu.cs` (199 lines) - Connection UI
- `ServerSettings.cs` (55 lines) - Server configuration

**Other directories:**
- `Photon/` - Photon networking library files
- `Properties/` - Main game logic namespace
- `AnimationOrTween/` - Game mechanics
- `Pathfinding/` - AI systems

---

## 📦 Project Files (Original Game)

### Original Game Files
- `target_compiled_final.apk` (47.3 MB) - Working v3.0.2
- `target/` directory - Extracted and decompiled
  - `smali/` - Java/Android bytecode
  - `assets/bin/Data/Managed/` - C# DLLs
  - `AndroidManifest.xml` - App configuration
  - `res/` - Resources and UI layouts

### Signing Key
- `debug.keystore` - Used to sign modified APKs
  - Password: android
  - Key alias: debugkey

---

## 🚀 Server Protocol Specification

### Message Format
```
Command|Param1|Param2|...
```

### Client → Server Commands

```
CONNECT|PlayerName|Version
  → Registers player with server
  → Response: CONNECTED|ClientId

CREATE_ROOM|RoomName|Map|MaxPlayers
  → Creates new game room
  → Response: ROOM_CREATED|RoomName

JOIN_ROOM|RoomName
  → Joins existing room
  → Response: ROOM_JOINED|RoomName or ROOM_JOIN_FAILED|Reason

STATE_UPDATE|PlayerId|X|Y|Z|Angle|Health|Animation
  → Sends player state (position, rotation, health)
  → Broadcast to room: STATE_UPDATE|PlayerId|...

RPC|MethodName|Param1|Param2|...
  → Calls method on all players in room
  → Used for: shoot, respawn, kill notifications, etc.
  → Broadcast to room: RPC|MethodName|...

DISCONNECT
  → Leave room and disconnect

PING
  → Keep-alive heartbeat
  → Response: PONG
```

### Server → Client Events

```
CONNECTED|ClientId
ROOM_CREATED|RoomName
ROOM_JOINED|RoomName
ROOM_JOIN_FAILED|Reason
PLAYER_JOINED|ClientId|PlayerName
PLAYER_DISCONNECTED|ClientId
STATE_UPDATE|...  (broadcast from other players)
RPC|...           (broadcast from other players)
PONG
```

---

## 📊 Architecture & Networking

### Network Flow
```
Phone 1 (127.0.0.1:xxxxx) ─┐
                            ├─ UDP Protocol ─ Server (0.0.0.0:5055)
Phone 2 (127.0.0.1:yyyyy) ─┤     Port 5055
Phone N (192.168.x.x:z) ───┘
```

### Game State Sync
```
Player 1 Update (100ms)  ──┐
Player 2 Update (100ms)  ──┤─ Server ─┬─→ All Other Players
Player 3 Update (100ms)  ──┤           └─→ Room Broadcast
Event (Kill, Respawn)    ──┘
```

### Room Management
```
Master Client (Room Creator)
  ├─ Is host
  ├─ Runs server loop
  └─ Has authority on game state

Other Players
  ├─ Connect to same room
  ├─ Sync state to master
  └─ Receive state from others
```

---

## ✅ Deployment Checklist

**Before Running Server:**
- [ ] Have Java or .NET installed
- [ ] Have apktool and Android SDK tools
- [ ] Have Python for patching
- [ ] Have debug keystore
- [ ] Port 5055 is available
- [ ] Firewall allows UDP 5055

**Before Installing on Phone:**
- [ ] Server is running
- [ ] APK is properly patched
- [ ] APK is properly signed
- [ ] Phone has USB debugging enabled
- [ ] Phone is connected via adb

**Before Testing Multiplayer:**
- [ ] Both phones on same network
- [ ] Both have patched APK installed
- [ ] Server is running
- [ ] Firewall rules applied
- [ ] Port 5055 verified open

---

## 🔍 Troubleshooting File Reference

| Problem | Solution File | Section |
|---------|---------------|---------|
| APK won't install | SETUP_LAN_SERVER_GUIDE.md | Troubleshooting |
| Connection timeout | SETUP_LAN_SERVER_GUIDE.md | Troubleshooting |
| Players can't see each other | SETUP_LAN_SERVER_GUIDE.md | Troubleshooting |
| Server crashes | SETUP_LAN_SERVER_GUIDE.md | Troubleshooting |
| Port already in use | verify_setup.py | Network check |
| Missing tools | verify_setup.py | Tool check |
| Protocol understanding | FINDINGS_SERVER_EXTRACTION.md | Server Configuration |
| Game logic | game_code/decompiled_full_csharp/ | Source reference |

---

## 📞 Quick Command Reference

**Compile server:**
```powershell
csc CopsNRobbersServer.cs -out:CopsNRobbersServer.exe
```

**Start server:**
```powershell
.\CopsNRobbersServer.exe
```

**Patch APK:**
```powershell
python apk_patch_server.py target_compiled_final.apk game_local.apk 127.0.0.1 5055
```

**Install on phone:**
```powershell
adb install game_local.apk
```

**Check port:**
```powershell
netstat -an | findstr 5055
```

**Verify setup:**
```powershell
python verify_setup.py
```

---

## 🎯 Recommended Reading Order

1. **Quick Overview** (5 min)
   - `README_LAN_SERVER.md`

2. **Decide Approach** (10 min)
   - `SETUP_LAN_SERVER_GUIDE.md` - Option A vs B

3. **Implementation** (30 min)
   - Follow chosen option step-by-step
   - Use verification tool: `python verify_setup.py`

4. **Deployment** (15 min)
   - Compile/start server
   - Patch APK
   - Install on phone
   - Test

5. **Reference** (as needed)
   - `FINDINGS_SERVER_EXTRACTION.md` - Technical questions
   - `game_code/decompiled_full_csharp/` - Game logic questions
   - Troubleshooting sections

---

## 📊 File Statistics

```
Documentation:      ~1,500 lines
Server Code:          ~450 lines
Tools/Scripts:        ~600 lines
Game Code (ref):   ~23,000 lines (677 C# files)
────────────────────────────
Total:             ~25,550 lines of documentation & code
```

---

**Status:** ✅ Complete and Ready for Deployment

All files are configured and tested. You have everything needed to:
1. Extract the embedded LAN server
2. Compile a working server implementation
3. Patch the game APK
4. Deploy locally or on a network
5. Play multiplayer Cops n Robbers!

---

**Last Updated:** October 2025
**Prepared for:** Cops n Robbers v3.0.2
**Difficulty:** Beginner-Friendly ⭐⭐
**Estimated Time to Deploy:** 30-60 minutes

