# LAN Server Project - Cops n Robbers

## Overview

This is a custom LAN server implementation for Cops n Robbers that replicates the Photon PUN v1.17 protocol using **LiteNetLib** for UDP networking.

**Purpose**: Enable seamless multiplayer gaming without Photon Cloud dependency.

## Project Structure

```
LAN_Server/
├─ LanServer.csproj          Project file with NuGet dependencies
├─ PhotonProtocol.cs         All Photon protocol constants (extracted)
├─ GameTypes.cs              Core game data structures
├─ LanGameServer.cs          Main server with LiteNetLib integration
├─ Program.cs                Console entry point
├─ README.md                 This file
└─ bin/
   └─ Release/
      └─ CopsNRobbersLanServer.exe
```

## Status

### ✅ Phase 2a: Foundation Complete
- [x] Project structure created
- [x] NuGet dependencies configured (LiteNetLib)
- [x] Protocol constants extracted and documented
- [x] Game data types defined
- [x] Server skeleton with LiteNetLib integration
- [x] Console UI with status commands

### ⏳ Phase 2b: In Progress
- [ ] Binary protocol parser (OpCodes, EventCodes)
- [ ] Parameter deserialization
- [ ] Type code handling
- [ ] RPC routing system

### ⏳ Phase 2c: Pending
- [ ] Room management (create, join, leave)
- [ ] Player management (add, remove, properties)
- [ ] State synchronization (OnPhotonSerializeView)
- [ ] RPC buffering (AllBuffered)

### ⏳ Phase 2d: Pending
- [ ] UDP broadcast discovery
- [ ] Server announcement
- [ ] Client discovery listener

## Building

### Requirements
- .NET 6.0 SDK or later
- Visual Studio 2022 / Visual Studio Code

### Build Command
```bash
cd LAN_Server
dotnet build -c Release
```

### Output
```
bin/Release/net6.0/CopsNRobbersLanServer.exe
```

## Running

### Start Server
```bash
.\bin\Release\net6.0\CopsNRobbersLanServer.exe
```

### Console Commands
```
q - Quit
s - Show status
r - Show rooms
p - Show players
```

### Log Output
```
✅ LAN Server started on port 5055
🔗 Peer connected: 192.168.1.100:54321
📨 Received 124 bytes from 192.168.1.100:54321
🔌 Peer disconnected: 192.168.1.100:54321
```

## Architecture

### Network Stack
```
Game Client
    ↓
PhotonNetwork API
    ↓
LAN Protocol Wrapper (future)
    ↓
LiteNetLib Transport
    ↓
UDP Socket (port 5055)
    ↓
LAN Network
```

### Message Flow
```
Client Operation
    ↓
Serialize to binary (PhotonProtocol format)
    ↓
Send via NetManager.SendToAll()
    ↓
Server OnNetworkReceive()
    ↓
Parse OpCode / EventCode
    ↓
Route to appropriate handler
    ↓
Execute game logic
    ↓
Generate response event
    ↓
Broadcast to relevant clients
```

## Protocol Constants

### OperationCodes (Client → Server)
- `0xE6` Authenticate - Initial connection
- `0xE5` JoinLobby - Enter lobby
- `0xE4` LeaveLobby - Leave lobby
- `0xE3` CreateGame - Create new room
- `0xE2` JoinGame - Join existing room
- `0xE1` JoinRandomGame - Random join
- `0xFE` Leave - Disconnect
- `0xFD` RaiseEvent - Send gameplay message (RPC, position update)
- `0xFC` SetProperties - Update room/player properties
- `0xFB` GetProperties - Request properties

### EventCodes (Server → Clients)
- `0xFF` Join - Player joined room
- `0xFE` Leave - Player left room
- `0xFD` PropertiesChanged - Room/player properties updated
- `0xE6` GameList - Room list (from lobby)
- `0xE5` GameListUpdate - Room list changed
- `0xE4` QueueState - Queue status
- `0xE3` Match - Random match found
- `0xE2` AppStats - Server statistics
- `0xD2` AzureNodeInfo - Azure cluster info (rarely used)

### Type Codes (Serialization)
```
0x00 Null
0x01 Hashtable
0x02 byte[]
0x03 string
0x04 byte
0x05 int
0x06 long
0x07 float
0x08 double
0x09 bool
0x0A object
0x0B Dictionary
0x0C Vector2 (2 floats = 8 bytes)
0x0D Vector3 (3 floats = 12 bytes)
0x0E Quaternion (4 floats = 16 bytes)
0x0F PhotonPlayer
0x10 PhotonViewID
0x11 Int16
0x12 Char
```

## Data Structures

### GamePlayer
```csharp
ActorNumber     (1-4)
PlayerName      (string)
TeamType        (0=Robber, 1=Cop)
Skin            (int)
Score           (int)
Health          (int)
IsAlive         (bool)
IpAddress       (string)
Port            (int)
PeerId          (int - LiteNetLib ID)
```

### GameRoom
```csharp
RoomName        (string)
MaxPlayers      (int)
CurrentPlayerCount (derived)
IsOpen          (bool)
IsVisible       (bool)
GameMode        (string)
MapName         (string)
Players         (Dictionary<int, GamePlayer>)
MasterClientActorNumber (int)
```

### GameServerState
```csharp
Rooms           (Dictionary<string, GameRoom>)
AllPlayers      (Dictionary<int, GamePlayer>)
TotalPlayerCount (derived)
TotalRoomCount  (derived)
CachedRpcs      (List<CachedRpcCall>)
```

## Network Performance

### Bandwidth
- **State Update**: 33 bytes (Position 12 + Rotation 16 + Health 4 + Status 1)
- **Frequency**: 20 Hz (once every 50ms)
- **Per Client**: 33 bytes × 20 = 660 bytes/sec
- **4-Player Game**: ~8 KB/sec total

### Latency
- **LAN (100 Mbps)**: < 10ms network delay
- **Target**: < 150ms for smooth gameplay
- **Keep-Alive Ping**: Every 15 seconds
- **Timeout**: 30 seconds no response

### Throughput
- **Port**: 5055 (UDP)
- **Broadcast Port**: 5056 (UDP discovery)
- **Connection Keep-Alive**: LiteNetLib built-in

## Implementation Phases

### Phase 2a: ✅ Foundation
- LiteNetLib integration
- Protocol constants
- Game data types
- Server skeleton

### Phase 2b: ⏳ Protocol Parsing
- Binary message parser
- OpCode/EventCode handling
- Parameter deserialization
- Type code conversion

### Phase 2c: ⏳ Game Logic
- Room management (create, join, leave)
- Player management (add, remove, track)
- RPC routing (all, others, specific)
- State synchronization
- Property broadcasting

### Phase 2d: ⏳ Discovery
- UDP broadcast server announcement
- Server discovery listener (client-side)
- Server list response format

### Phase 3: ⏳ APK Modification
- Disable Photon Cloud connection
- Redirect PhotonNetwork to LAN server
- Modify APK with modified game code

### Phase 4: ⏳ Testing & QA
- Single player connection test
- 2-player gameplay test
- 4-player gameplay test
- Stress testing
- Latency measurement

## Next Steps

1. **Protocol Parser** - Implement message parsing in `OnNetworkReceive()`
2. **Room Manager** - Handle CreateGame, JoinGame, Leave operations
3. **RPC Router** - Route RaiseEvent messages to appropriate players
4. **State Broadcaster** - Sync OnPhotonSerializeView data
5. **Discovery Server** - Announce server via UDP broadcast
6. **Error Handling** - Graceful disconnects and timeouts
7. **Testing** - Validate with multiple connected clients

## Related Documents

- **PROTOCOL_SPECIFICATION.md** - Complete Photon protocol documentation
- **LITELIB_ANALYSIS_NETCODE_DEEP_DIVE.md** - LiteNetLib evaluation and guide
- **IMPLEMENTATION_ROADMAP_WITH_LITELIB.md** - Project phases and timeline
- **PHASE_1_SUMMARY.md** - Protocol extraction summary

## References

- **LiteNetLib GitHub**: https://github.com/RevenantX/LiteNetLib
- **LiteNetLib NuGet**: https://www.nuget.org/packages/LiteNetLib/
- **Photon PUN v1.17**: Original game networking library

## Status

**Current Phase**: 2a (Foundation) ✅
**Time Spent**: ~1 hour
**Time Remaining**: ~20-24 hours
**Overall Progress**: 5% → 10%

---

**Last Updated**: October 27, 2025
**Maintainer**: GitHub Copilot
**License**: MIT
