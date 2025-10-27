# Implementation Roadmap: LiteNetLib + Photon Protocol Wrapper

## Quick Decision Summary

**You asked**: LiteNetLib or custom?
**Answer**: **LiteNetLib (use it!)**

**Why**: 
- Saves 30+ hours of UDP complexity
- You control the protocol layer (custom)
- LAN requirements don't need exotic UDP features
- Battle-tested in production games
- MIT licensed (no strings attached)

---

## Architecture Overview

```
┌─────────────────────────────────────────────┐
│ Cops n Robbers Game (APK)                   │
│  - Unchanged game code                      │
│  - Calls PhotonNetwork methods              │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│ LAN Network Compatibility Layer             │
│ (Your custom code - ~2000 lines)            │
│  - PhotonNetworkFacade.cs                   │
│  - PhotonProtocolCompat.cs                  │
│  - StateSerializationHandler.cs             │
│  - RpcDispatcher.cs                         │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│ LiteNetLib Transport Layer                  │
│ (External library - battle-tested)          │
│  - UDP packet handling                      │
│  - Fragmentation/reassembly                 │
│  - Reliability & ordering                   │
│  - Connection management                    │
└────────────────────┬────────────────────────┘
                     │
            OS UDP Network Stack
```

---

## What Each Layer Does

### Game Layer (Unchanged)
```csharp
// Game code doesn't know it's using a custom server
PhotonNetwork.CreateRoom("Game");
PhotonNetwork.JoinRoom("Game");

photonView.RPC("FireWeapon", RpcTarget.AllBuffered, position, direction, damage);

void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
{
    if (stream.IsWriting)
    {
        stream.SendNext(transform.position);
        stream.SendNext(transform.rotation);
    }
}
```

### Compatibility Layer (Your Code)
```csharp
// Intercepts PhotonNetwork calls
public class PhotonNetworkFacade : MonoBehaviour
{
    // When game calls PhotonNetwork.CreateRoom("Game")
    public static void CreateRoom(string roomName, ...)
    {
        // Your code sends protocol message to LiteNetLib
        LanNetworkManager.SendCreateRoom(roomName);
    }
    
    // When game calls photonView.RPC(...)
    public static void RPC(int viewId, string method, params object[] args)
    {
        // Your code serializes using YOUR protocol
        byte[] data = ProtocolSerializer.SerializeRpc(viewId, method, args);
        // Sends via LiteNetLib
        LiteNetLibClient.Send(data);
    }
}

// Deserializes Photon protocol format
public class PhotonProtocolCompat
{
    public static object[] DeserializeRpcParameters(byte[] data)
    {
        // Uses YOUR extracted OpCodes, TypeCodes, RPC list
        // Returns parameters in correct order
    }
}
```

### Transport Layer (LiteNetLib)
```csharp
// LiteNetLib handles the actual network I/O
public class LanServerCore
{
    private NetManager netManager;
    
    public void Start()
    {
        // LiteNetLib setup
        netManager = new NetManager(new ServerListener());
        netManager.Start(5055); // UDP port
    }
    
    public void SendToRoom(int roomId, byte[] data)
    {
        // LiteNetLib sends reliable/unreliable based on your choice
        netManager.SendToAll(data, DeliveryMethod.ReliableOrdered);
    }
}
```

---

## Implementation Phases

### Phase 1: Protocol Extraction (8-12 hours)

**Goal**: Document everything needed to parse messages

**Deliverable**: `PROTOCOL_SPECIFICATION.md`

**Checklist**:
- [ ] OpCodes extracted (all ~15)
- [ ] EventCodes extracted (all ~10)
- [ ] TypeCodes extracted (all ~20)
- [ ] RPC methods listed (all ~30)
- [ ] OnPhotonSerializeView implementations documented (all ~5)
- [ ] Handshake flow documented
- [ ] Room/Player properties documented

**Key Tool**: dnSpy (free decompiler)

**Output**: Specification document you'll reference constantly

---

### Phase 2: Server Implementation (20-24 hours)

**Goal**: Create functional LAN server using LiteNetLib

**Structure**:
```
LanServer/
├─ LanServer.cs (main entry point, LiteNetLib startup)
├─ ServerListener.cs (event callbacks from LiteNetLib)
├─ GameManager.cs (room/player management)
├─ ProtocolParser.cs (your OpCodes and deserialization)
├─ RpcRouter.cs (route RPCs to correct clients)
├─ StateSync.cs (broadcast position/state updates)
├─ BroadcastServer.cs (UDP discovery on port 5056)
└─ Program.cs (console app entry point)
```

**Phases**:

**2a: LiteNetLib Integration (4-6 hours)**
- Create NetManager on port 5055
- Implement ServerListener callbacks
- Handle peer connected/disconnected
- Test: Connect with test client

**2b: Protocol Parsing (6-8 hours)**
- Implement OpCode recognition
- Deserialize each message type
- Handle fragmented messages (LiteNetLib does this)
- Test: Parse sample protocol messages

**2c: Game Logic (6-8 hours)**
- Room creation/joining
- RPC routing between clients
- State sync broadcasting
- Player list management
- Test: 2-player connection and gameplay

**2d: UDP Discovery (2-4 hours)**
- Broadcast server announcement every 1 second
- Include: server name, player count, port
- Test: Mobile phone detects server

**Deliverable**: `LanServer.exe` (working server)

---

### Phase 3: Client Implementation (16-20 hours)

**Goal**: Modify APK to use LAN server instead of Photon

**Structure**:
```
Assets/Scripts/Network/
├─ LanNetworkManager.cs (replaces PhotonNetwork)
├─ BroadcastListener.cs (UDP server discovery)
├─ ServerBrowserUI.cs (UI to select server)
├─ LanConnection.cs (TCP client to server)
├─ ProtocolSerializer.cs (serialize game data)
├─ RpcInterceptor.cs (hook game RPC calls)
├─ StateCapture.cs (capture OnPhotonSerializeView)
└─ PhotonNetworkFacade.cs (compatibility layer)
```

**Phases**:

**3a: Disable Cloud (2-3 hours)**
- Find PhotonNetwork initialization
- Comment out Photon connection
- Initialize LanNetworkManager instead
- Test: App starts without Photon errors

**3b: Server Discovery (3-4 hours)**
- UDP listener on port 5056
- Parse server announcements
- Populate server list UI
- Test: Phone shows discovered servers

**3c: Connection (4-6 hours)**
- Connect to selected server
- Send handshake
- Receive Player ID from server
- Store for later use
- Test: Connected state shows in UI

**3d: RPC Interception (4-6 hours)**
- Hook photonView.RPC() calls
- Serialize parameters using YOUR protocol
- Send to server
- Test: Single RPC call works

**3e: State Sync (2-4 hours)**
- Capture OnPhotonSerializeView data
- Send at 10-20 Hz
- Test: Position updates work

**Deliverable**: Modified APK (LAN-enabled)

---

### Phase 4: Integration Testing (8-12 hours)

**Goal**: End-to-end testing with real devices

**Test Scenarios**:

**4a: Connection (2 hours)**
- [ ] Server starts
- [ ] Phone discovers server
- [ ] Phone connects
- [ ] Phone receives Player ID
- [ ] Phone can create room

**4b: 2-Player Gameplay (3 hours)**
- [ ] Player 1 creates room
- [ ] Player 2 joins room
- [ ] Both see each other
- [ ] Movement syncs
- [ ] Can shoot each other
- [ ] Health/score syncs

**4c: 4-Player Gameplay (2 hours)**
- [ ] 4 devices connected
- [ ] All see all players
- [ ] No lag/desync
- [ ] RPCs work for all

**4d: Stress Testing (2 hours)**
- [ ] Server survives 100 messages/second
- [ ] Memory stays stable
- [ ] No crashes after 30 minutes

**4e: Edge Cases (2 hours)**
- [ ] Player disconnects (others notified)
- [ ] Server stops (clients handle gracefully)
- [ ] WiFi interrupt (reconnection works)
- [ ] High latency (still playable)

**Deliverable**: Verified working multiplayer

---

## Technology Stack

### Server
- **Framework**: C# / .NET Framework 4.6+
- **Networking**: LiteNetLib (NuGet package)
- **IDE**: Visual Studio Community (free) or VS Code
- **Output**: Console app or Windows Service

### Client (APK)
- **Framework**: C# / Unity 3D (existing)
- **Networking**: LiteNetLib (NuGet) or C# port
- **UI**: UGUI (existing)
- **Output**: Modified APK

### Tools
- **dnSpy** (protocol extraction)
- **APKTool** (APK modification) - already have
- **Android Studio** (optional, debugging)
- **Wireshark** (optional, packet analysis)

---

## Critical Files You'll Create

### Server Files

**1. LanServer.cs** (~200 lines)
- NetManager initialization
- Main game loop
- Shutdown handling

**2. ProtocolParser.cs** (~300 lines)
- OpCode recognition
- Message deserialization
- Uses YOUR OpCode table

**3. GameManager.cs** (~400 lines)
- Room/player management
- RPC routing
- State broadcasting

**4. StateSync.cs** (~200 lines)
- Position update aggregation
- Broadcast to room

**5. BroadcastServer.cs** (~150 lines)
- UDP server on port 5056
- Advertisement beacon

### Client Files

**1. LanNetworkManager.cs** (~300 lines)
- Replaces PhotonNetwork
- Connection management
- Stores Player ID

**2. BroadcastListener.cs** (~200 lines)
- UDP listener
- Server discovery
- Cache management

**3. ProtocolSerializer.cs** (~400 lines)
- RPC serialization
- State sync serialization
- Uses YOUR TypeCode table

**4. RpcInterceptor.cs** (~200 lines)
- Hooks photonView.RPC()
- Redirects to LAN server

**5. ServerBrowserUI.cs** (~150 lines)
- UI for server selection
- Ping display
- Manual IP entry option

---

## Timeline Estimate

| Phase | Task | Hours | Reality |
|-------|------|-------|---------|
| 1 | Protocol Extraction | 10 | 8-12 |
| 2a | LiteNetLib Setup | 5 | 4-6 |
| 2b | Protocol Parsing | 7 | 6-8 |
| 2c | Game Logic | 7 | 6-8 |
| 2d | UDP Discovery | 3 | 2-4 |
| 3a | Disable Cloud | 2.5 | 2-3 |
| 3b | Server Discovery | 3.5 | 3-4 |
| 3c | Connection | 5 | 4-6 |
| 3d | RPC Interception | 5 | 4-6 |
| 3e | State Sync | 3 | 2-4 |
| 4 | Testing & QA | 10 | 8-12 |
| **TOTAL** | | **60** | **52-73** |

**Realistic Timeline**: 60-75 hours across 15-20 focused sessions

**Calendar**: If 4 hours/session, 2 sessions/week = 8-10 weeks

---

## Success Criteria

### Server Works When:
- ✅ Accepts connection from LiteNetLib client
- ✅ Assigns Player ID
- ✅ Routes RPC messages
- ✅ Broadcasts state updates
- ✅ Handles disconnect
- ✅ Broadcasts discovery beacon
- ✅ Runs 24+ hours without crash

### Client Works When:
- ✅ Discovers server via broadcast
- ✅ Connects and gets Player ID
- ✅ Creates and joins rooms
- ✅ Sends RPC calls
- ✅ Receives RPC calls
- ✅ Sends state updates
- ✅ Receives state updates

### Integration Works When:
- ✅ 2 players see each other
- ✅ Movement smooth and synced
- ✅ Shooting works for both
- ✅ 4 players work together
- ✅ Latency < 150ms
- ✅ No crashes or freezes

---

## What Makes This Possible (Your Advantages)

✅ **Working game** - v3.0.2 is functional
✅ **Full source decompiled** - 677 C# files
✅ **APK tooling experience** - done this before
✅ **Photon library available** - can reverse-engineer
✅ **LiteNetLib proven** - battle-tested
✅ **Clear roadmap** - documented here
✅ **Time available** - can do 4+ hours/session

---

## Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| Protocol extraction incomplete | Start with OpCodes, test incrementally |
| RPC parameters mismatch | Log every RPC, compare with packet capture |
| Latency too high | Test on real LAN, measure RTT |
| Server crashes | Add try/catch, logging, monitoring |
| APK won't recompile | Test modification early, iterate |
| Client doesn't connect | Start with echo server test |

---

## Next Immediate Actions

### Today (Right Now)
1. ✅ Read this document
2. ✅ Read `LITELIB_ANALYSIS_NETCODE_DEEP_DIVE.md`
3. ⏳ Decide: Ready to start?

### Session 1 (Next: 8 hours)
1. Download dnSpy (if needed)
2. Open Photon3Unity3D.dll
3. Extract OpCodes (full list)
4. Extract TypeCodes (full list)
5. Extract RPC methods (all of them)
6. Create PROTOCOL_SPECIFICATION.md

### Session 2 (Following: 8 hours)
1. Open Assembly-CSharp.dll
2. Find all OnPhotonSerializeView implementations
3. Document exact order and types
4. Document handshake sequence
5. Finalize protocol spec

### Session 3 (Following: 6 hours)
1. Create new C# console project
2. Install LiteNetLib via NuGet
3. Create basic NetManager server
4. Test with LiteNetLib test client
5. Verify UDP on port 5055 works

### Session 4+ (Implementation)
- Follow Phase 2 in detail
- Implement piece by piece
- Test after each piece

---

## Resources

**LiteNetLib**:
- GitHub: https://github.com/RevenantX/LiteNetLib
- NuGet: `dotnet add package LiteNetLib`
- Docs: README has examples

**dnSpy**:
- GitHub: https://github.com/dnSpy/dnSpy
- Direct download: Latest release .zip

**C# Networking Basics**:
- Microsoft docs: System.Net.Sockets
- LiteNetLib examples: GitHub repo has samples

**Tools Already Have**:
- Visual Studio Community (free)
- APKTool (have it)
- Git (have it)

---

## Final Thought

This is **achievable and well-planned**. You have:

1. ✅ The game (working version)
2. ✅ The code (decompiled)
3. ✅ The library (Photon3Unity3D.dll)
4. ✅ The tools (dnSpy, APKTool, etc.)
5. ✅ The roadmap (this document)
6. ✅ The networking library (LiteNetLib)
7. ✅ The time (if you commit)

The hardest part is reverse-engineering the protocol. Everything else is straightforward C# coding.

**Estimated chance of success**: 85%+ if you follow this roadmap exactly.

**Biggest risk**: Giving up during protocol extraction when it seems tedious.

**Best advice**: Start with OpCodes, get that working completely. One small piece at a time.

---

**Ready?**

→ Read `LITELIB_ANALYSIS_NETCODE_DEEP_DIVE.md` next
→ Then `PROTOCOL_EXTRACTION_CHECKLIST.md` 
→ Then start Session 1

Good luck! 🚀

