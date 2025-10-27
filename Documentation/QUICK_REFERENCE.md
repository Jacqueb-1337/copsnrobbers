# LAN Multiplayer Server - Quick Reference Card

## The Mission
Replace Photon Cloud with a local WiFi server for 4-8 player multiplayer without internet.

## The Problem
1. Photon Cloud is offline
2. GameKit is iOS-only
3. Game hardcoded to `app.exitgamescloud.com:5055`

## The Solution
Create LAN server on Windows PC that:
- Broadcasts "I'm here" via UDP every second
- Accepts TCP connections from 4-8 players
- Routes RPC calls between players
- Syncs game state (position, health, ammo, etc.)
- Feels native to the game (same protocol)

## Three Components to Build

### 1. Server (Windows PC)
```
Listens on port 5055 (TCP) + 5056 (UDP)
├─ Accept connections from game clients
├─ Manage rooms and players
├─ Route RPC messages
├─ Broadcast UDP "I exist" beacon
└─ Sync state between clients
```

**Language**: C# (matches game) or Python (simpler)
**Complexity**: Medium (socket programming + threading)
**Time**: 16-20 hours

### 2. Client (In APK)
```
Installed on Android phones
├─ Listen for UDP broadcast (find server)
├─ Show server browser UI
├─ Connect to server via TCP
├─ Send my RPC calls to server
├─ Receive other players' RPC calls
├─ Send my position/health/ammo every 100ms
└─ Receive other players' state
```

**Language**: C# (modify game code)
**Complexity**: High (hooking into game code + threading)
**Time**: 12-16 hours

### 3. Protocol (Binary Format)
```
Message structure both sides must understand:
[OpCode: 1 byte] [Length: 2 bytes] [Data: N bytes]

OpCodes:
  0x01 = Connect
  0x02 = CreateRoom
  0x03 = RPC Call
  0x04 = State Update
  0x05 = JoinRoom
  0x06 = LeaveRoom
  0x07 = PlayerList
```

**Time to reverse-engineer**: 8-12 hours

## What Must Be Reverse-Engineered

### From Photon3Unity3D.dll:
- [ ] How are messages serialized? (order of bits/bytes)
- [ ] What are the operation codes?
- [ ] How is Vector3/Quaternion encoded?
- [ ] What are type codes? (0x03=int, 0x05=float, 0x07=Vector3, etc.)

### From Assembly-CSharp.dll:
- [ ] List all `[PunRPC]` methods
- [ ] For each method: name, parameter types, order
- [ ] Find all `OnPhotonSerializeView` implementations
- [ ] For each: what data is sent, in what order, what types?

## Critical Success Factors

✅ **Must Do**:
1. Exact Photon protocol reverse-engineering (no guessing)
2. Exact RPC parameter order (must match 100%)
3. Exact state sync serialization order (must match 100%)
4. Thread-safe message queues on both server and client
5. Proper error handling for disconnects

❌ **Don't**:
1. Guess at message format (reverse-engineer with dnSpy)
2. Assume RPC parameter order (verify every one)
3. Ship without testing with 2+ actual devices
4. Ignore error handling (network is unreliable)

## Checklist for Session 1

- [ ] Read all 4 documentation files
- [ ] Download dnSpy if not already installed
- [ ] Open target/assets/bin/Data/Managed/Photon3Unity3D.dll in dnSpy
- [ ] Search for `ExitGames.Client.Photon.PhotonNetwork` class
- [ ] Examine CreateRoom, JoinRoom, SendRPC methods
- [ ] Find PhotonStream class
- [ ] Document SendNext() type codes (0x00-0x0E)
- [ ] Open Assembly-CSharp.dll
- [ ] Search for `[PunRPC]` - count how many methods
- [ ] Search for `OnPhotonSerializeView` - count implementations
- [ ] Create quick protocol specification (start)

## Estimated Timelines

| Milestone | Hours | Weeks |
|-----------|-------|-------|
| Reverse-engineering complete | 8-12 | 2 weeks |
| Server working (2 player test) | 24-32 | 3 weeks |
| Client working (connection) | 36-48 | 5 weeks |
| 4-player integration test | 44-56 | 7 weeks |
| Full QA & bug fixes | 50-60+ | 8 weeks |

**Parallel Work**: Server and client can be developed simultaneously.

## Tools Required

- dnSpy (DLL decompilation) ✅ Download free
- APKTool (APK modification) ✅ Already have
- Visual Studio or VS Code (C# coding) ✅ Already have
- C# compiler (dotnet) ✅ Likely have
- Python (if using Python for server) ✅ Optional
- Android ADB (install APK) ✅ Should have
- Text editor for docs ✅ Have already

## The Workflow

```
Week 1: Reverse-Engineer
  Day 1-2: dnSpy analysis (Protocol)
  Day 3: Document findings
  
Week 2-3: Build Server
  Parallel track: Start coding server
  Test with mock client
  
Week 3-4: Build Client
  Parallel track: Start APK modifications
  Create server browser UI
  
Week 5: Integration
  Connect phone to server
  Test 2-player gameplay
  Fix protocol issues
  
Week 6-7: Stress Testing
  4+ players simultaneously
  Sustained play sessions
  
Week 8: Polish
  Bug fixes
  Performance optimization
  Documentation
```

## Key Files to Monitor

**Server Protocol Spec**: `Documentation/NETCODE_REVERSE_ENGINEERING_GUIDE.md`
**Server Implementation**: `Documentation/LAN_SERVER_TECHNICAL_ARCHITECTURE.md`
**Client Patches**: Will be in `target/` after modifications
**Test Results**: Create `Documentation/TEST_RESULTS.md` during testing

## Warning Signs ⚠️

If you see these, STOP and re-verify:
- Message doesn't parse correctly on server
- RPC methods get called but with wrong parameters
- Player positions constantly "teleport" instead of smooth motion
- Server crashes on 3rd player join
- Clients freeze when another player connects

These usually mean: protocol reverse-engineering is wrong, not code bug.

## Success Signs ✅

- Server accepts connection, sends Player ID
- Client shows server in browser
- Player can create room
- Multiple players see each other join
- Movement smooth and synchronized
- Shooting/health updates in real-time
- No crashes after 30 minutes of gameplay

## Quick Command Reference

**Start reverse-engineering**:
1. Open dnSpy
2. File → Open → `target/assets/bin/Data/Managed/Photon3Unity3D.dll`
3. Search for `PhotonNetwork` (Ctrl+K)
4. Start documenting

**Build server (once code ready)**:
```cmd
cd LanServer
dotnet build -c Release
LanServer.exe
```

**Patch APK (once client code ready)**:
```cmd
apktool b target/ -o target_compiled_lan.apk --use-aapt1
jarsigner -keystore debug.keystore -storepass android target_compiled_lan.apk debugkey
adb install target_compiled_lan.apk
```

**Test on device**:
```cmd
adb shell am start -n com.joydo.minestrikenew/com.unity3d.player.UnityPlayerProxyActivity
adb logcat | grep "Network\|RPC\|Connection"
```

## Remember

> **"Perfect protocol understanding" > "Perfect code"**

The server/client code can be written in a weekend. The protocol reverse-engineering can't be rushed. If the protocol is wrong, the code will never work no matter how perfect it is.

Start there. 🔍

---

**Status**: Checklist created ✅ | Documentation complete ✅ | Ready for execution ⏳
**Next Action**: Review docs → Open dnSpy → Start reverse-engineering

