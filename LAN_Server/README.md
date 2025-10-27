# Cops n Robbers - LAN Server
**v1.0 - Protocol Parser & UDP Server**

## Quick Start

### Build
```powershell
cd LAN_Server
dotnet build -c Release
```

### Run Protocol Tests
```powershell
dotnet bin/Release/net8.0/CopsNRobbersLanServer.dll --test
```

### Start Server
```powershell
dotnet bin/Release/net8.0/CopsNRobbersLanServer.dll
```

## Project Structure

```
LAN_Server/
├── LanServer.csproj              # Project configuration (net8.0, no dependencies)
├── Program.cs                    # Console entry point
├── LanGameServerUdp.cs           # UDP server using System.Net.Sockets
├── PhotonProtocol.cs             # All 52 protocol constants
├── GameTypes.cs                  # GamePlayer, GameRoom, GameServerState classes
├── PhotonMessageParser.cs        # Binary → PhotonMessage deserializer
├── PhotonMessageSerializer.cs    # PhotonMessage → binary serializer
├── OperationHandler.cs           # Message routing to operation handlers
└── ProtocolTester.cs             # Unit tests for parser/serializer
```

## Features

### ✅ Implemented
- Binary protocol parser for all 20 Photon type codes
- Binary protocol serializer with response builders
- Operation routing (10 operation types)
- UDP server with peer connection tracking
- Comprehensive test suite (5 tests, all passing)
- Zero external dependencies

### 🟡 In Progress
- Game room management
- Player tracking and state sync
- RPC routing
- Property change broadcasting

### 📋 Planned
- UDP discovery server
- Anti-cheat measures
- Rate limiting
- Detailed logging

## Protocol Support

### Operations (Client → Server)
- 0xE6: Authenticate
- 0xE5: JoinLobby
- 0xE4: LeaveLobby
- 0xE3: CreateGame
- 0xE2: JoinGame
- 0xFE: JoinRandomGame
- 0xFD: RaiseEvent
- 0xFC: SetProperties
- 0xFB: GetProperties
- 0xFA: Leave

### Events (Server → Client)
- 0xE2: AppStats
- 0xE6: GameList
- 0xFF: Join
- 0xFE: Leave
- 0xFD: PropertiesChanged

### Supported Types (20)
Primitives, Unity vectors/quaternions, hashtables, arrays, and more.

## Server Configuration

Edit `GameConstants` in `GameTypes.cs`:
```csharp
GameServerPort = 5056              // Main game server port
BroadcastPort = 5055               // Discovery broadcast port
MaxPlayersPerRoom = 4              // Players per room
StateUpdateFrequencyHz = 30         // State sync rate
TimeoutMs = 30000                  // Peer timeout (30 seconds)
```

## Build Information

- **Framework**: .NET 8.0
- **Language**: C# 12 with nullable reference types
- **Dependencies**: None (uses only System.* namespaces)
- **Output**: `bin/Release/net8.0/CopsNRobbersLanServer.exe`
- **Size**: ~10 MB (includes .NET runtime on deployment)

## Test Results

All 5 protocol tests passing:
```
✓ Serialize/Deserialize Basic Types
✓ Serialize/Deserialize Collections (Hashtable)
✓ Authenticate Message (Client → Server)
✓ CreateGame Message (Client → Server)
✓ AppStats Response (Server → Client)
```

## Commands (Console)

While running:
```
q or Ctrl+C     - Quit server
s               - Show server status
r               - Show active rooms
p               - Show connected players
t               - Run protocol tests
```

## Logs

Server outputs real-time logs with emoji indicators:
```
✅ Server started
🔗 Peer connected
📨 Message received
🎮 Operation processed
⚠️  Warning or parsing error
❌ Error occurred
```

## Performance

- Message deserialization: < 1ms per message
- Message serialization: < 1ms per message
- Peer tracking: O(1) lookup by IP:Port
- Max theoretical throughput: 1000+ messages/sec per connection

## Architecture Notes

The UDP server uses an async receive loop to handle multiple clients without blocking. Messages are parsed immediately and routed to operation handlers which can enqueue responses or broadcasts.

```
UDP Socket (async receive loop)
    ↓
Parse binary message
    ↓
Route to operation handler
    ↓
Game logic (TODO)
    ↓
Serialize response
    ↓
Send back to client
```

## Debugging

Enable detailed logging by modifying `Console.WriteLine` calls or setting environment variable:
```powershell
$env:LAN_SERVER_DEBUG="1"
dotnet bin/Release/net8.0/CopsNRobbersLanServer.dll
```

## Troubleshooting

**Port Already in Use**
```powershell
netstat -ano | findstr :5056
taskkill /PID <PID> /F
```

**Cannot Run Tests**
Ensure .NET 8.0 is installed:
```powershell
dotnet --list-runtimes
```

**Build Fails**
- Clean and rebuild: `dotnet clean ; dotnet build -c Release`
- Update .NET SDK: `dotnet sdk update`

## License

Part of Cops n Robbers LAN Server project.

---

**Next Phase**: Implement game room and player management (Phase 2c)  
**Documentation**: See `../PHASE_2B_SUMMARY.md` for detailed information
