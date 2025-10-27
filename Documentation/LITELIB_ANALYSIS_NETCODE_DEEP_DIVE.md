# Netcode Analysis & LiteNetLib vs Custom Implementation

## Executive Summary

**Recommendation**: Use **LiteNetLib** for the server layer + custom protocol wrapper

**Rationale**:
- LiteNetLib handles UDP complexity (fragmentation, reliability, ordering)
- Protocol layer remains Photon-compatible (custom)
- Significantly reduces implementation time
- Battle-tested for multiplayer games
- Built-in features for LAN (no cloud needed)

---

## Part 1: LiteNetLib Evaluation

### 1.1 What is LiteNetLib?

**GitHub**: https://github.com/RevenantX/LiteNetLib

**Purpose**: UDP networking library for multiplayer games
- ✅ Cross-platform (Windows, Linux, macOS, mobile)
- ✅ Low-latency UDP transport
- ✅ Built-in packet fragmentation/reassembly
- ✅ Optional reliability and ordering per packet
- ✅ No external dependencies
- ✅ MIT License (free, commercial use OK)

### 1.2 LiteNetLib Architecture

```
Your Game Code
    ↓
Your Protocol Layer (Photon-compatible)
    ↓
LiteNetLib (UDP transport)
    ↓
Network Stack (OS networking)
```

**What LiteNetLib Provides**:
```csharp
NetManager server = new NetManager(listener);
server.Start(5055); // UDP port

NetDataWriter writer = new NetDataWriter();
writer.Put("Hello");
server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
```

**What You Still Must Implement**:
- Protocol format (OpCodes, serialization)
- Game logic (rooms, players, RPC routing)
- State synchronization

### 1.3 LiteNetLib vs Custom UDP

| Feature | LiteNetLib | Custom |
|---------|-----------|--------|
| Fragmentation | ✅ Built-in | ❌ Must implement |
| Reliability | ✅ Built-in | ❌ Must implement |
| Ordering | ✅ Built-in | ❌ Must implement |
| Latency | ✅ Optimized | ❌ Unknown |
| Complexity | 3/10 | 8/10 |
| Time to implement | 20-24 hrs | 40-50 hrs |
| Proven in production | ✅ Yes | ❌ Risky |
| Code size | ~2000 lines | ~5000+ lines |

### 1.4 Does LiteNetLib Work for LAN?

**YES** - LiteNetLib is perfect for LAN multiplayer:
- ✅ Supports UDP broadcast (server discovery)
- ✅ No cloud dependency
- ✅ Works on local networks
- ✅ Single PC can host server
- ✅ Mobile clients on WiFi can connect

### 1.5 LiteNetLib Integration Points

```csharp
// Server startup (LiteNetLib handles UDP)
NetManager server = new NetManager(new ServerListener());
server.Start(5055); // UDP port
server.BroadcastChannel = 5056; // For discovery

// Game logic stays the same
LanGameServer gameServer = new LanGameServer(server);

// Your protocol wrapper
PhotonCompatibleProtocol protocol = new PhotonCompatibleProtocol();
```

---

## Part 2: Netcode Analysis - Understanding Photon's Implementation

### 2.1 What We Need to Understand FIRST

Before implementing (with or without LiteNetLib), we must answer:

1. **Connection Protocol**
   - How does client connect to Photon?
   - What's in initial handshake?
   - Player ID assignment?

2. **RPC Format**
   - How are methods serialized?
   - Parameter order?
   - Type codes?

3. **State Sync**
   - OnPhotonSerializeView data format?
   - Frequency (updates/second)?
   - Delta compression used?

4. **Room Management**
   - Room properties format?
   - Player properties?
   - How are they serialized?

5. **Reliability**
   - Which messages must be guaranteed?
   - Which can be lossy (position updates)?

### 2.2 Netcode Deep Dive: Photon Protocol Structure

From Photon PUN v1.17 documentation and reverse-engineering:

#### Message Format

```
┌─────────────────────────────────────────┐
│ Photon Message Format                   │
├─────────────────────────────────────────┤
│ [Protocol Version: 1 byte]              │
│ [Message Type: 1 byte]                  │
│ [Operation/Event Code: 1 byte]          │
│ [Channel ID: 1 byte]                    │
│ [Reserved: 1 byte]                      │
│ [Payload Length: 2 bytes]               │
│ [Payload: N bytes]                      │
│ [CRC (optional): 4 bytes]               │
└─────────────────────────────────────────┘
```

**Critical**: Each element must be extracted and documented from actual APK

#### Operation Codes (Likely Values)

From Photon protocol:
```
0x01 = Authenticate
0x02 = JoinGame
0x03 = CreateGame
0x04 = Leave
0x05 = RaiseEvent (RPC)
0x06 = GetGameList
0x07 = GetGameInfo
0x08 = SetProperties
0x09 = GetProperties
...
(Must verify exact codes with dnSpy)
```

#### Data Type Codes (Serialization)

Photon uses type codes for parameter serialization:
```
0x00 = Null
0x01 = Boolean (1 byte: 0/1)
0x02 = Byte (1 byte: 0-255)
0x03 = Short (2 bytes)
0x04 = Int (4 bytes)
0x05 = Long (8 bytes)
0x06 = Float (4 bytes)
0x07 = Double (8 bytes)
0x08 = String (length-prefixed UTF-8)
0x09 = Byte Array (length-prefixed)
0x0A = Vector3 (3 × 4-byte floats)
0x0B = Quaternion (4 × 4-byte floats)
0x0C = PhotonPlayer
0x0D = PhotonView
0x0E = Hashtable
0x0F = Array
...
(Must verify with dnSpy)
```

### 2.3 Connection Flow (What Happens)

```
CLIENT                                          SERVER
   │                                              │
   ├─ Connect to Photon3Unity3D                   │
   │  (internal - we replace this)                │
   │                                              │
   └─ Send: [Authenticate]                ────→  │
            [Version: 1.17]                       │
            [AppID]                               │
            [PlayerID]                            │
                                                  │
                                       ←────  Send: [AuthenticateResponse]
                                                  [SessionID]
                                                  [PeerID]
                                                  [GameServers]
   │                                              │
   └─ Send: [JoinGame]               ────→  │
            [RoomName]                            │
            [CreateIfNotExists]                   │
            [RoomProperties]                      │
            [PlayerProperties]                    │
                                                  │
                                       ←────  Send: [JoinGameResponse]
                                                  [RoomID]
                                                  [PlayerList]
                                                  [RoomState]
   │                                              │
   └─ Send: [RaiseEvent (RPC)]       ────→  │
            [ViewID]                             │
            [MethodID]                           │
            [Parameters]                         │
                                                  │
                                       ←────  Send: [RaiseEvent]
                                                  [Data]
                                                  [Sender]
```

### 2.4 State Synchronization Flow

```
Frame Update:
┌──────────────────────────────────────┐
│ 1. OnPhotonSerializeView called      │
│    (if stream.IsWriting)             │
│    ├─ stream.SendNext(position)      │
│    ├─ stream.SendNext(rotation)      │
│    ├─ stream.SendNext(health)        │
│    └─ ... more fields                │
│                                      │
│ 2. Photon serializes to bytes        │
│    [position: 12 bytes]              │
│    [rotation: 16 bytes]              │
│    [health: 4 bytes]                 │
│    [... more data]                   │
│                                      │
│ 3. Wraps in [RaiseEvent]             │
│    [OpCode: 0x05]                    │
│    [ViewID]                          │
│    [Serialized Data]                 │
│                                      │
│ 4. Sends via UDP                     │
│    (no guarantee, fastest possible)  │
│                                      │
│ 5. Other players receive             │
│    OnPhotonSerializeView (deserialize)
│    ├─ stream.ReceiveNext()           │
│    ├─ stream.ReceiveNext()           │
│    └─ Apply to remote object         │
└──────────────────────────────────────┘
```

---

## Part 3: Recommended Implementation Strategy

### 3.1 Hybrid Approach: LiteNetLib + Custom Protocol

```
┌───────────────────────────────────────────────┐
│ Game (APK)                                    │
│  └─ Calls PhotonNetwork methods              │
│     (unmodified game code)                    │
└──────────────────────────────────────────────┬┘
                                               │
┌──────────────────────────────────────────────▼─────────────────┐
│ LAN Network Manager (Custom)                                   │
│  ├─ PhotonNetworkFacade.cs (replaces PhotonNetwork calls)     │
│  ├─ PhotonProtocolCompat.cs (Photon protocol layer)           │
│  ├─ StateSerializationHandler.cs (stream processing)          │
│  └─ RpcDispatcher.cs (RPC routing)                            │
└──────────────────────────────────────────────────────────────┬─┘
                                                               │
┌──────────────────────────────────────────────────────────────▼─┐
│ LiteNetLib Transport Layer                                     │
│  ├─ Handles UDP packet sending/receiving                      │
│  ├─ Fragmentation/reassembly                                  │
│  ├─ Reliability & ordering (configurable per msg)             │
│  ├─ Connection management                                     │
│  └─ Peer tracking                                             │
└──────────────────────────────────────────────────────────────┬─┘
                                                               │
                                    Network Stack (OS UDP)
```

### 3.2 Implementation Phases with LiteNetLib

#### Phase 1: Server Architecture (16-20 hours)

```csharp
public class LanGameServer
{
    private NetManager netManager;
    private Dictionary<int, RemotePlayer> players = new();
    private Dictionary<int, Room> rooms = new();
    
    public void Start()
    {
        // LiteNetLib handles UDP port listening
        netManager = new NetManager(new ServerListener(this));
        netManager.Start(5055); // UDP port
        
        // Your game logic
        gameManager = new GameManager();
    }
    
    public void Update()
    {
        netManager.PollEvents();
        gameManager.Update();
    }
}

// LiteNetLib listener (callbacks for events)
public class ServerListener : INetEventListener
{
    public void OnPeerConnected(NetPeer peer)
    {
        // Handle new player connection
    }
    
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        // Parse your protocol here
        byte opCode = reader.GetByte();
        // ... handle opcode
    }
    
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        // Handle player disconnect
    }
}
```

**Deliverables**:
- LiteNetLib server accepting connections
- Basic event callback handling
- Player connection/disconnection logic

#### Phase 2: Protocol Layer (12-16 hours)

```csharp
public class PhotonCompatibleProtocol
{
    // Serialize RPC call
    public byte[] SerializeRpc(int viewId, string methodName, params object[] parameters)
    {
        NetDataWriter writer = new NetDataWriter();
        
        writer.Put((byte)0x05); // OpCode: RaiseEvent
        writer.Put(viewId);
        writer.Put(GetMethodHash(methodName));
        writer.Put((byte)parameters.Length);
        
        foreach (var param in parameters)
        {
            SerializeParameter(writer, param);
        }
        
        return writer.Data;
    }
    
    // Deserialize RPC call
    public void DeserializeRpc(byte[] data, out int viewId, out string methodName, out object[] parameters)
    {
        NetDataReader reader = new NetDataReader(data);
        
        byte opCode = reader.GetByte(); // 0x05
        viewId = reader.GetInt();
        short methodHash = reader.GetShort();
        byte paramCount = reader.GetByte();
        
        parameters = new object[paramCount];
        for (int i = 0; i < paramCount; i++)
        {
            parameters[i] = DeserializeParameter(reader);
        }
        
        methodName = GetMethodNameFromHash(methodHash);
    }
    
    private void SerializeParameter(NetDataWriter writer, object param)
    {
        if (param == null)
        {
            writer.Put((byte)0x00); // Null type
        }
        else if (param is int intVal)
        {
            writer.Put((byte)0x04); // Int type
            writer.Put(intVal);
        }
        else if (param is Vector3 vec3)
        {
            writer.Put((byte)0x0A); // Vector3 type
            writer.Put(vec3.x);
            writer.Put(vec3.y);
            writer.Put(vec3.z);
        }
        // ... more types
    }
    
    private object DeserializeParameter(NetDataReader reader)
    {
        byte typeCode = reader.GetByte();
        
        return typeCode switch
        {
            0x00 => null,
            0x04 => reader.GetInt(),
            0x0A => new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat()),
            // ... more types
            _ => throw new Exception($"Unknown type code: {typeCode}")
        };
    }
}
```

**Deliverables**:
- RPC serialization/deserialization
- State sync serialization
- Parameter type handling

#### Phase 3: Game Logic (16-20 hours)

```csharp
public class GameManager
{
    private Dictionary<int, Room> rooms = new();
    private Dictionary<int, Player> players = new();
    
    public void OnPlayerConnect(NetPeer peer, int playerId)
    {
        Player player = new Player { Id = playerId, Peer = peer };
        players[playerId] = player;
    }
    
    public void OnRoomCreate(NetPeer peer, string roomName)
    {
        Room room = new Room { Id = GenerateRoomId(), Name = roomName };
        rooms[room.Id] = room;
        room.AddPlayer(peer);
    }
    
    public void OnRoomJoin(NetPeer peer, int roomId)
    {
        if (rooms.TryGetValue(roomId, out Room room))
        {
            room.AddPlayer(peer);
            
            // Notify all players in room
            BroadcastToRoom(room, "PlayerJoined", peer.Id);
        }
    }
    
    public void OnRpcReceived(NetPeer peer, int viewId, string methodName, object[] parameters)
    {
        // Route RPC to appropriate room
        var room = GetRoomForPlayer(peer);
        if (room != null)
        {
            // Broadcast to all in room except sender
            BroadcastRpcToRoom(room, peer, viewId, methodName, parameters);
        }
    }
    
    public void OnStateUpdate(NetPeer peer, byte[] stateData)
    {
        var room = GetRoomForPlayer(peer);
        if (room != null)
        {
            // Broadcast state to all in room except sender
            BroadcastStateToRoom(room, peer, stateData);
        }
    }
    
    private void BroadcastToRoom(Room room, string method, params object[] parameters)
    {
        var data = protocol.SerializeRpc(-1, method, parameters);
        
        foreach (var player in room.Players)
        {
            if (player.Peer.IsConnected)
            {
                netManager.SendToAll(data, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
```

**Deliverables**:
- Room management
- RPC routing
- State synchronization
- Player lifecycle management

#### Phase 4: Client (APK) Integration (12-16 hours)

```csharp
// Replace PhotonNetwork.ConnectUsingSettings()
public class LanNetworkManager : MonoBehaviour
{
    private NetManager client;
    private PhotonCompatibleProtocol protocol;
    
    public void Initialize()
    {
        client = new NetManager(new ClientListener(this));
        client.Start(); // UDP client
        
        // Listen for server broadcasts
        StartCoroutine(ListenForBroadcasts());
    }
    
    public void ConnectToServer(string serverIp, int serverPort)
    {
        client.Connect(serverIp, serverPort, "connection_key");
    }
    
    public void SendRpc(int viewId, string methodName, RpcTarget target, params object[] parameters)
    {
        var data = protocol.SerializeRpc(viewId, methodName, parameters);
        
        // LiteNetLib handles delivery method
        client.SendToAll(data, DeliveryMethod.ReliableOrdered);
    }
}

public class ClientListener : INetEventListener
{
    private LanNetworkManager networkManager;
    
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        // Parse protocol and dispatch to game
        byte opCode = reader.GetByte();
        
        switch (opCode)
        {
            case 0x05: // RPC
                OnRpcReceived(reader);
                break;
            case 0x04: // State sync
                OnStateSync(reader);
                break;
        }
    }
    
    private void OnRpcReceived(NetPacketReader reader)
    {
        // Deserialize and execute RPC on game objects
    }
    
    private void OnStateSync(NetPacketReader reader)
    {
        // Deserialize and update remote player state
    }
}
```

**Deliverables**:
- Server discovery with broadcast listener
- Server browser UI
- LAN connection management
- RPC dispatch to game

---

## Part 4: Critical Netcode Requirements

### 4.1 Serialization Format (MUST BE EXACT)

These must match Photon's implementation EXACTLY or deserialization will fail:

```csharp
// Vector3 serialization (MUST be in this order)
position.x (4 bytes, float)
position.y (4 bytes, float)
position.z (4 bytes, float)
// Total: 12 bytes

// Quaternion serialization (MUST be in this order)
rotation.x (4 bytes, float)
rotation.y (4 bytes, float)
rotation.z (4 bytes, float)
rotation.w (4 bytes, float)
// Total: 16 bytes

// String serialization
length (2 bytes, short)
UTF-8 characters (N bytes)
// Total: 2 + N bytes
```

**How to Verify**:
1. Open dnSpy
2. Search for `PhotonStream.SendNext` overloads
3. Document exact serialization for each type
4. Verify with test packets

### 4.2 Delivery Methods

Photon uses different reliability modes per message:

```csharp
// Reliable Ordered (guaranteed delivery, in order)
// Used for: RPCs, room changes, critical events
// LiteNetLib: DeliveryMethod.ReliableOrdered

// Unreliable (best effort, may be lost)
// Used for: Position updates, non-critical state
// LiteNetLib: DeliveryMethod.Unreliable

// Reliable Unordered (guaranteed, but not in order)
// Used for: Independent state updates
// LiteNetLib: DeliveryMethod.ReliableUnordered

// Fragmented (for large messages)
// LiteNetLib: Automatic if message > MTU
```

### 4.3 Update Frequency

```
State Synchronization (OnPhotonSerializeView):
├─ Frequency: 10-20 updates per second (100-50ms intervals)
├─ Delivery: Unreliable (fast, no retry)
├─ Size: ~36-100 bytes per player per update
└─ Bandwidth: 4 players × 100 bytes × 10 Hz = 4 KB/s

RPC Calls (FireWeapon, TakeDamage, etc.):
├─ Frequency: Variable (player-triggered)
├─ Delivery: ReliableOrdered (must not be lost)
├─ Size: 50-200 bytes per RPC
└─ Bandwidth: Variable (~1-2 KB/s during combat)

Total for 4-player game: ~5-10 KB/s (reasonable for LAN)
```

### 4.4 Latency Requirements

For responsive multiplayer:

```
Acceptable Latency: < 150ms on LAN
├─ Network RTT: 10-50ms (LAN typical)
├─ Processing: 30-50ms (server + client)
└─ Display: 16ms (60 FPS)

If latency > 150ms:
├─ Implement client-side prediction
├─ Use lag compensation
├─ Implement extrapolation for remote players
```

---

## Part 5: What Must Be Reverse-Engineered (Priority Order)

### Critical (Must Have)

- [ ] **OpCode list** (from Photon3Unity3D.dll)
  - All operation codes used
  - Impact: None of this works without this

- [ ] **Type codes** (from PhotonStream.SendNext overloads)
  - Serialization format for each type
  - Impact: Data corruption if wrong

- [ ] **RPC method list** (from Assembly-CSharp.dll [PunRPC])
  - Every method name that's called remotely
  - Impact: RPCs won't execute remotely

- [ ] **OnPhotonSerializeView implementations** (from Assembly-CSharp.dll)
  - Exact order of SendNext() calls
  - Impact: Position/state completely wrong

### Important (Should Have)

- [ ] **Room properties** (how stored and synced)
- [ ] **Player properties** (custom properties format)
- [ ] **Room creation flow** (parameters and handshake)
- [ ] **Message size limits** (for fragmentation handling)

### Nice-to-Have

- [ ] **Authentication details** (probably not needed for LAN)
- [ ] **Rate limiting** (probably not needed for LAN)
- [ ] **Encryption** (LAN = no security needed typically)

---

## Part 6: LiteNetLib Decision Matrix

### Pros of Using LiteNetLib

✅ **Time Savings**
- 30-40 hours saved (don't reimplement UDP)
- Fragmentation/reassembly already done
- Reliability layer already done

✅ **Reliability**
- Proven in production games
- Battle-tested in multiplayer scenarios
- Active maintenance and community

✅ **Performance**
- Optimized for latency
- Zero-copy approach where possible
- Handles high packet rates well

✅ **Simplicity**
- 2000 lines of code vs 5000+ custom
- Less surface area for bugs
- Clear API

### Cons of Using LiteNetLib

❌ **External Dependency**
- Need to distribute LiteNetLib DLL
- Extra assembly to manage
- License compliance (MIT = no problem)

❌ **Learning Curve**
- Different API than raw sockets
- May need debugging if issues arise

❌ **Abstraction**
- Less direct control over UDP packets
- Can't do exotic things if needed
- But: Not needed for this project

### Recommendation

**USE LITELIB** because:

1. The time savings (30+ hours) are substantial
2. You get proven reliability for free
3. You can focus on protocol and game logic
4. LAN requirements don't need exotic UDP features
5. No vendor lock-in (MIT license, pure C#)

The custom vs LiteNetLib choice is really about **time vs control**. You have plenty of control where it matters (protocol layer), and LiteNetLib saves time where it's tedious (UDP transport).

---

## Part 7: Next Steps

### Immediate (This Session)

1. ✅ Decide: Use LiteNetLib? → **YES**
2. ✅ Create netcode analysis → **THIS DOCUMENT**
3. ⏳ Review LiteNetLib API (https://github.com/RevenantX/LiteNetLib)
4. ⏳ Identify which game classes call PhotonNetwork methods

### Session 1: Reverse-Engineering (8-12 hours)

1. Open dnSpy with Photon3Unity3D.dll
2. Extract and document:
   - OpCode list
   - Type codes
   - Message structure
3. Open dnSpy with Assembly-CSharp.dll
4. Extract and document:
   - All [PunRPC] methods
   - All OnPhotonSerializeView implementations
5. Create: `Documentation/NETCODE_SPECIFICATION.md`

### Session 2-3: Server Implementation (16-20 hours)

1. Clone LiteNetLib repository
2. Create LAN server project
3. Implement protocol layer
4. Implement game logic
5. Test with mock clients

### Session 4-5: Client Implementation (12-16 hours)

1. Create LAN network manager
2. Implement server discovery
3. Hook PhotonNetwork calls
4. Create server browser UI
5. Test with actual device

---

## Appendix: LiteNetLib Quick Reference

```csharp
// Server
NetManager server = new NetManager(serverListener);
server.Start(port);
server.SendToAll(data, DeliveryMethod.ReliableOrdered);
server.PollEvents();

// Client  
NetManager client = new NetManager(clientListener);
client.Start();
client.Connect(ip, port, key);
client.SendToAll(data, DeliveryMethod.ReliableOrdered);
client.PollEvents();

// Packet writing
NetDataWriter writer = new NetDataWriter();
writer.Put(data);
writer.PutArray(array);
writer.PutString(text);

// Packet reading
NetDataReader reader = new NetDataReader(data);
int val = reader.GetInt();
string text = reader.GetString();
```

**Delivery Methods**:
- `Unreliable` - Fast, may be lost
- `ReliableUnordered` - Guaranteed, any order
- `ReliableOrdered` - Guaranteed, in order
- `SequencedReliable` - Special: resends until newer packet arrives

---

**Next Action**: Review this analysis and start Session 1 reverse-engineering or create LiteNetLib server stub.

