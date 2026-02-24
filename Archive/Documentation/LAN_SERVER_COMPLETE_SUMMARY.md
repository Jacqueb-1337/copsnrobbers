# LAN Multiplayer Server Project - Complete Summary & Action Plan

## Executive Summary

You've asked for a complete blueprint to create a local multiplayer server that replaces Photon Cloud and Apple GameKit without requiring internet connectivity or cloud authentication. This document summarizes the challenge and provides a step-by-step action plan.

---

## The Challenge

### Current State
- **Working Game**: Cops n Robbers FPS v3.0.2 (3.0.9 is corrupted)
- **Networking**: Photon PUN v1.17 hardcoded to connect to `app.exitgamescloud.com:5055`
- **Problem**: Cloud is offline, so multiplayer doesn't work
- **iOS Backup**: GameKit P2P exists but is iOS-only and hardcoded

### Goal
- **Create**: A LAN-based multiplayer server running on a Windows PC
- **Support**: 4-8 concurrent players on same WiFi network
- **Eliminate**: Any dependency on cloud services or internet connectivity
- **Implement**: Auto-discovery so players don't need to manually enter IP addresses

### Complexity
- **Photon Protocol**: Proprietary binary format that must be reverse-engineered
- **Netcode**: Complex serialization/deserialization of game state and RPC calls
- **APK Patching**: Game code must be modified to talk to LAN server instead of Photon
- **Timing**: Network latency and synchronization challenges

---

## What Must Be Reverse-Engineered

### 1. Photon Protocol Format (Binary)
**What**: How Photon encodes messages for transmission

**Deliverable**: 
```
Message Format:
[OpCode: 1 byte] - Operation type (Connect, CreateRoom, RPC, etc.)
[Length: 2 bytes] - Payload size
[Payload: N bytes] - Operation-specific data
```

**Example**:
```
RPC Call Message:
[0x03] [0x00 0x50] [ViewID=0x00000001][Target=0x00][MethodID=0x1234][Param1...]
OpCode: 0x03 (RPC)
Length: 0x0050 (80 bytes payload)
```

**Where to Find**: Photon3Unity3D.dll in target/assets/bin/Data/Managed/

### 2. RPC Method Definitions
**What**: Every method the game calls remotely

**Deliverable**: Complete list with signatures
```
[PunRPC] void FireWeapon(Vector3 position, Vector3 direction, int damage)
[PunRPC] void PlayAnimation(string animName)
[PunRPC] void TakeDamage(int amount, int shooterID)
[PunRPC] void SyncPosition(Vector3 pos, Quaternion rot)
...
```

**Where to Find**: Assembly-CSharp.dll search for `[PunRPC]` attributes using dnSpy

### 3. State Synchronization Format
**What**: How player state (position, rotation, health, ammo) is serialized

**Deliverable**: Exact order and types for each networked object
```csharp
void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
{
    if (stream.IsWriting)
    {
        stream.SendNext(transform.position);      // Vector3 = 12 bytes
        stream.SendNext(transform.rotation);      // Quaternion = 16 bytes
        stream.SendNext(currentHealth);           // int = 4 bytes
        stream.SendNext(ammunition);              // int = 4 bytes
        // TOTAL: 36 bytes per update
    }
}
```

**Where to Find**: Assembly-CSharp.dll search for `OnPhotonSerializeView`

### 4. Room & Player Properties
**What**: Metadata about rooms and players

**Deliverable**: Structure definitions
```
Room Properties:
  - Name: string
  - MaxPlayers: int
  - IsOpen: bool (can join)
  - IsVisible: bool (appears in search)
  - CustomProperties: Dictionary

Player Properties:
  - PlayerID: int (assigned by server)
  - NickName: string
  - Score: int
  - TeamID: int (if applicable)
  - CustomProperties: Dictionary
```

**Where to Find**: PhotonNetwork.CreateRoom() and Player class in Photon3Unity3D.dll

---

## Implementation Roadmap

### Phase 1: Analysis & Reverse Engineering (8-12 hours)

**Tools Needed**:
- dnSpy (decompiler for .NET DLLs)
- Wireshark (packet sniffer - optional but helpful)
- Text editor (document findings)

**Deliverables**:
1. Protocol specification document (OpCodes, message formats)
2. RPC method catalog (all [PunRPC] methods listed)
3. State sync specifications (OnPhotonSerializeView implementations)
4. Property definitions (Room and Player properties)

**Step-by-Step**:
1. Open dnSpy, load `Photon3Unity3D.dll`
2. Search for `ExitGames.Client.Photon.PhotonNetwork` class
3. Examine `CreateRoom()`, `JoinRoom()`, `LeaveRoom()` implementations
4. Search for `ExitGames.Client.Photon.PhotonStream` class
5. Document `SendNext()` and `ReceiveNext()` methods and type codes
6. Open `Assembly-CSharp.dll`
7. Search for `[PunRPC]` attributes - list every method
8. Search for `OnPhotonSerializeView` - document each implementation
9. Create protocol specification document

### Phase 2: LAN Server Implementation (16-20 hours)

**Architecture**:
```
LAN Server (C# or Python on Windows PC)
├─ TCP Server (port 5055) - Game traffic
├─ UDP Broadcaster (port 5056) - Server discovery
├─ Room Manager - Create/join/leave rooms
├─ Message Router - Parse and dispatch messages
├─ RPC Executor - Execute remote methods
├─ State Synchronizer - Broadcast player updates
└─ Client Connection Handler (per player)
```

**Implementation**:
1. TCP server that accepts connections from up to 8 clients
2. Handshake: Client connects → Server assigns Player ID → Send back
3. Room management: Create room, join room, track players, notify on changes
4. Message routing: Parse OpCode, dispatch to handler
5. RPC routing: Receive RPC → deserialize params → broadcast to room
6. State sync: Receive state updates → forward to other players
7. UDP broadcaster: Advertise server availability every 1 second

**Deliverables**:
- `LanServer.exe` (compiled executable)
- Configuration file (server name, max players, ports)
- Logging output (debug troubleshooting)

### Phase 3: APK Client Modification (12-16 hours)

**Changes Needed**:
1. **Disable Cloud**: Prevent PhotonNetwork.Connect() to cloud servers
2. **LAN Discovery**: Add UDP listener for server broadcasts
3. **Server Browser UI**: Show discovered servers, allow manual IP entry
4. **Network Manager**: Replace PhotonNetwork with custom LAN version
5. **RPC Interception**: Capture game RPCs, send to LAN server
6. **State Sync**: Capture OnPhotonSerializeView data, send to server
7. **Message Queue**: Thread-safe queue for outgoing messages

**Implementation**:
1. Create new C# scripts:
   - `LanNetworkManager.cs` (main network facade)
   - `BroadcastListener.cs` (UDP discovery)
   - `RpcInterceptor.cs` (RPC capture)
   - `StateSerializer.cs` (state sync)
   - `ServerBrowserUI.cs` (UI for server selection)
2. Modify existing code:
   - Find PhotonNetwork initialization code
   - Replace with LAN initialization
   - Hook game RPC calls to custom interceptor
3. Recompile APK with modifications
4. Sign APK with debug keystore

**Deliverables**:
- Modified APK file ready for installation
- UI showing discovered servers
- Connection to LAN server on device

### Phase 4: Testing & Validation (8-12 hours)

**Test Scenarios**:

**1-Player Test**:
- [ ] Start server on PC
- [ ] Install APK on phone
- [ ] Phone discovers server in UI
- [ ] Phone connects to server
- [ ] Phone receives Player ID
- [ ] Phone can create/join room

**2-Player Test**:
- [ ] Player 1 creates room
- [ ] Player 2 discovers room in server browser
- [ ] Player 2 joins room
- [ ] Both players see each other in-game
- [ ] Player list updates on both

**4-Player Test**:
- [ ] 4 players on different devices
- [ ] All connected to same room
- [ ] Movement syncs across all devices
- [ ] Shooting/RPCs work for all
- [ ] Scoring/health updates correctly

**Stress Tests**:
- [ ] Server handles rapid join/leave
- [ ] Server doesn't crash with 100 messages/second
- [ ] Memory usage stays reasonable (< 500MB)
- [ ] Runs 24+ hours without memory leaks

**Latency Tests**:
- [ ] Ping on LAN: < 50ms
- [ ] State update latency: < 100ms
- [ ] RPC delivery: 100% reliable

---

## What's Already Done

✅ **Completed in Previous Sessions**:
1. Identified game uses Photon PUN v1.17
2. Confirmed GameKit is iOS-only
3. Diagnosed v3.0.9 corruption (unusable)
4. Found working v3.0.2 (playable)
5. Located 677 C# files (decompiled)
6. Created documentation for Photon setup
7. Organized project into clean directory structure
8. Initialized Git repository with code tracked

---

## What You Need to Do Now

### Immediate (1-2 hours)
1. **Review this checklist** - Understand the scope
2. **Decide: C# or Python for server?**
   - C# = faster, same language as game
   - Python = simpler syntax, easier debugging
3. **Decide: When to start?**
   - Can be parallelized (server ≠ client can be developed independently)

### Short-term (Next Session, ~4 hours)
1. **Open dnSpy and start reverse-engineering**
   - Load `target/assets/bin/Data/Managed/Photon3Unity3D.dll`
   - Document message format and op codes
   - Load `target/assets/bin/Data/Managed/Assembly-CSharp.dll`
   - List all RPC methods
   - List all OnPhotonSerializeView implementations
2. **Create protocol specification document** (will guide all subsequent work)

### Medium-term (2-3 sessions, ~12 hours)
1. **Build LAN server** (can work in parallel with client)
2. **Build APK client modifications** (can work in parallel with server)
3. **Basic testing** (1 player connection)

### Long-term (2-3 sessions, ~8 hours)
1. **Full integration testing** (2, 4, 8 player scenarios)
2. **Bug fixes and optimization**
3. **Final documentation**

---

## Critical Dependencies

These things must be completed in order:

```
1. Reverse-Engineer Protocol
   ↓
2. Create Protocol Spec Document
   ↓
   ├─→ Build Server (uses protocol spec)
   │   ↓
   │   Server Testing
   │
   └─→ Build Client Patches (uses protocol spec)
       ↓
       APK Recompilation & Installation
       ↓
       Client Testing
       ↓
3. Integration Testing (server + clients together)
   ↓
4. Full QA & Stability
```

---

## Estimated Total Effort

| Phase | Task | Duration | Priority |
|-------|------|----------|----------|
| 1 | Reverse-engineer protocol | 8-12 hrs | **CRITICAL** |
| 2 | Build LAN server | 16-20 hrs | High |
| 3 | Modify APK client | 12-16 hrs | High |
| 4 | Testing & debugging | 8-12 hrs | High |
| **TOTAL** | | **44-60 hrs** | |

**Timeline**: If working 4 hours/session, this is ~11-15 sessions of focused work

---

## Success Criteria

When can you call this DONE?

✅ **Server is DONE when**:
- Accepts 4 clients on same WiFi
- Broadcasts discovery beacon
- Routes messages between clients
- Handles disconnections gracefully
- Doesn't crash under stress
- Logs all activity

✅ **Client is DONE when**:
- Discovers LAN servers via broadcast
- Connects to server and gets Player ID
- Shows server browser UI
- Allows room creation and joining
- Synchronizes player movement
- Executes RPCs from remote players

✅ **Integration is DONE when**:
- 2 players can play together on LAN
- 4 players can play together on LAN
- Gameplay is smooth (< 150ms latency)
- No crashes or data corruption
- Disconnection handled gracefully

---

## Documentation Provided

Three comprehensive guides have been created in `Documentation/`:

1. **LAN_SERVER_IMPLEMENTATION_CHECKLIST.md**
   - 7-phase implementation roadmap
   - Detailed checklists for each component
   - Message format specifications
   - Testing procedures

2. **NETCODE_REVERSE_ENGINEERING_GUIDE.md**
   - How to analyze Photon protocol
   - What to look for in decompiled code
   - Binary protocol design
   - Implementation examples

3. **LAN_SERVER_TECHNICAL_ARCHITECTURE.md**
   - Complete technical specifications
   - Server code examples (C#)
   - Client code examples (C#)
   - Testing strategy and timeline

---

## Next Steps

1. **Read** this document fully
2. **Review** the three guide documents
3. **Decide** on C# vs Python for server
4. **Plan** when you can dedicate focused time
5. **Start** with Phase 1 (reverse-engineering with dnSpy)
6. **Document** protocol findings
7. **Build** server and client incrementally
8. **Test** thoroughly before considering complete

---

## Questions to Ask Yourself

1. **Do I have time?** (44-60 hours is significant)
2. **Do I have the skills?** (Network programming, C#/Python, APK patching)
3. **Is it worth it?** (What's the value vs playing on cloud/offline?)
4. **Can I find help?** (Would you work on this alone?)
5. **What's the deadline?** (When do you want multiplayer working?)

---

## Resources

**Tools**:
- dnSpy (https://github.com/dnSpy/dnSpy) - Analyze DLLs
- APKTool (already have) - Decompile/recompile APK
- Visual Studio or VS Code - Code editing
- Git (already have) - Version control

**References**:
- Photon PUN Documentation: https://doc.photonengine.com/en-us/pun/v1/
- Binary serialization: https://docs.microsoft.com/en-us/dotnet/framework/serialization/binary-serialization
- C# networking: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets

---

## Final Notes

**This is ambitious but achievable.** You have:
- ✅ Working game (v3.0.2)
- ✅ Full source code (677 files decompiled)
- ✅ Networking library (Photon3Unity3D.dll)
- ✅ Development environment (VS Code, tools)
- ✅ Project organization (clean directories, Git)

**What you need**:
- Reverse-engineering skills (learn Photon protocol)
- Network programming skills (TCP/UDP servers)
- Debugging skills (trace message flow)
- Persistence (lots of testing and bug fixing)

**The reward**: Full local multiplayer that works without internet, owned completely by you with no vendor dependencies.

Good luck! 🚀

