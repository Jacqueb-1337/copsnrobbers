# LAN Multiplayer Netcode Reverse Engineering Guide

## Executive Summary
This document details what must be reverse-engineered from the game code to create a working LAN server replacement for Photon Cloud.

---

## Part 1: Photon PUN v1.17 Protocol Fundamentals

### 1.1 What We Know About the Game
- **Networking Library**: Photon PUN v1.17 (Photon3Unity3D.dll)
- **Protocol**: Proprietary binary protocol over UDP
- **Connection**: OutBound only to `app.exitgamescloud.com:5055`
- **Max Players**: Likely 4-8 per match (typical mobile FPS)
- **Update Rate**: Probably 10 updates/second (100ms tick)
- **Network Loop**: `OnPhotonSerializeView()` called per frame

### 1.2 Photon's Core Classes (What We Need to Replace)

#### PhotonNetwork (The Main Entry Point)
```csharp
// What the game calls:
public static class PhotonNetwork
{
    public static void ConnectUsingSettings(string gameVersion);
    public static void CreateRoom(string roomName, RoomOptions options, TypedLobby lobby);
    public static void JoinRandomRoom(Hashtable expectedRoomProperties, byte expectedMaxPlayers);
    public static void JoinRoom(string roomName);
    public static void LeaveRoom();
    
    public static int LocalPlayer.ID { get; }
    public static List<Player> PlayerList { get; }
    public static List<Room> GetRoomList(); // For room browser
    
    public static void SendRPC(int viewID, string methodName, RpcTarget target, params object[] parameters);
}
```

**Our Patch Strategy**: Replace these calls to talk to LAN server instead

#### PhotonView (Attached to Networked Objects)
```csharp
public class PhotonView : MonoBehaviour
{
    public int ViewID { get; set; } // Unique network identifier
    public Player Owner { get; set; } // Who owns this object
    
    public void RPC(string methodName, RpcTarget target, params object[] parameters);
    public void SerializeView(PhotonStream stream, PhotonMessageInfo info);
}
```

**Our Patch Strategy**: Intercept RPC() and SerializeView() calls

#### PhotonStream (Serialization Mechanism)
```csharp
public class PhotonStream
{
    public void SendNext(object value); // Write to stream
    public object ReceiveNext(); // Read from stream
    public void SendNext(Vector3 value); // Special handling for Vector3
    public void SendNext(Quaternion value); // Special handling for Quaternion
}
```

**Our Patch Strategy**: Must replicate this exact serialization format

---

## Part 2: Critical Network Flows to Reverse Engineer

### 2.1 Connection Handshake Flow

**Game Does**:
1. `PhotonNetwork.ConnectUsingSettings(version)`
2. Game waits for `OnConnected()` callback
3. Calls `PhotonNetwork.CreateRoom()` or `JoinRandomRoom()`
4. Waits for `OnJoinedRoom()` callback
5. Starts spawning networked objects

**We Need to Understand**:
```
CLIENT                                   SERVER
|                                         |
| ---> Initial TCP Connect (Handshake) -> |
| <--- Player ID Assignment <----------  |
|                                         |
| ---> CreateRoom Request               -> |
| <--- Room Created, Room List Updates <- |
|                                         |
| ---> Join Room Request                -> |
| <--- Room Joined, Player List Updates <- |
|                                         |
| <--- OnConnected() callback (game)      |
| <--- OnJoinedRoom() callback (game)     |
```

**Questions to Answer**:
- What exactly does the handshake contain?
- How is player ID assigned?
- What data is in room creation message?
- What player properties are sent on join?

### 2.2 Room Management Flow

**Operations to Support**:
```
CreateRoom(roomName, options)
├─ options.MaxPlayers = 4
├─ options.IsOpen = true
├─ options.IsVisible = true
└─ Custom properties?

JoinRoom(roomName)
├─ Check if space available
├─ Assign to room
├─ Notify all players in room: "PlayerX joined"
└─ Send new player all current players

JoinRandomRoom(properties, maxPlayers)
├─ Find first room matching criteria
├─ Check space available
└─ Join (same as JoinRoom)

LeaveRoom()
├─ Remove from room
├─ Notify others: "PlayerX left"
├─ If creator left, assign new owner
└─ Delete room if empty
```

**Critical Details to Reverse-Engineer**:
1. **Room Name Format**: String, max length?
2. **Room Properties**: Which ones? Serialization format?
3. **Search Criteria**: Can filter on what fields?
4. **Player Limit**: Enforced how?
5. **Room Ownership**: Transferred how when owner leaves?

### 2.3 State Synchronization Flow

This is the MOST CRITICAL part. This is where player position, rotation, health, ammo, etc. are synced.

**In Every Frame**:
```csharp
// Game calls this on each networked object
void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
{
    if (stream.IsWriting)
    {
        // I'm sending my state to others
        stream.SendNext(transform.position);
        stream.SendNext(transform.rotation);
        stream.SendNext(health);
        stream.SendNext(ammunition);
        // ... whatever else
    }
    else
    {
        // I'm receiving another player's state
        Vector3 pos = (Vector3)stream.ReceiveNext();
        Quaternion rot = (Quaternion)stream.ReceiveNext();
        int hp = (int)stream.ReceiveNext();
        int ammo = (int)stream.ReceiveNext();
        // Apply to this networked object
    }
}
```

**What We Must Capture**:
1. **Order of SendNext() calls** - Must be exact, or receiving end gets corrupted data
2. **Data Types** - Vector3, Quaternion, int, float, string, bool, etc.
3. **Frequency** - How many times per second?
4. **Size** - Bytes per update per player
5. **Filtering** - Does server send all states to all, or per-player?

**Example Analysis Tool** (use dnSpy):
- Open `Assembly-CSharp.dll`
- Search for `OnPhotonSerializeView`
- Find every implementation
- Document the exact order and types

### 2.4 RPC (Remote Procedure Call) Flow

RPCs are individual method calls like "fire weapon" or "play animation".

**In Game Code Likely**:
```csharp
[PunRPC]
void FireWeapon(Vector3 position, Vector3 direction, int damage)
{
    // Create bullet at position, move in direction
    // Deal damage to hits
}

// When player fires locally:
photonView.RPC("FireWeapon", RpcTarget.AllBuffered, 
    gunPos, gunDir, damageAmount);
```

**What Happens**:
1. Game calls `photonView.RPC("FireWeapon", RpcTarget.AllBuffered, ...)`
2. Photon serializes the method name and parameters
3. Sends to server
4. Server routes to all players in room (AllBuffered = store for latecomers)
5. Each client deserializes and calls local `FireWeapon()` method

**Critical Details to Reverse-Engineer**:
1. **Method Name Hashing**: Does it send method name as string or hash?
2. **Parameter Serialization**: How are parameters encoded?
3. **RPC Targets**: AllBuffered, Others, MasterClient, etc. - which does game use?
4. **Reliable Delivery**: Must this be 100% guaranteed?

**Example RPC Message Format** (likely):
```
OpCode: 0x03 (RPC)
ViewID: 4 bytes (which networked object)
Target: 1 byte (AllBuffered=0x00, Others=0x01, etc.)
MethodID: 2 bytes (hash of "FireWeapon")
ParamCount: 1 byte
  Param1Type: 1 byte (Vector3=0x07)
  Param1Data: 12 bytes (3 floats)
  Param2Type: 1 byte (Vector3=0x07)
  Param2Data: 12 bytes
  Param3Type: 1 byte (int=0x03)
  Param3Data: 4 bytes
```

---

## Part 3: File Analysis Roadmap

### 3.1 First: Examine Photon3Unity3D.dll (The Library)

**Using dnSpy**:
1. Open `Photon3Unity3D.dll`
2. Search for class `ExitGames.Client.Photon.PhotonNetwork`
3. Examine these key methods:
   - `ConnectUsingSettings()`
   - `CreateRoom()`
   - `JoinRoom()`
   - `SendRPC()`
4. Search for `ExitGames.Client.Photon.PhotonStream`
5. Examine `SendNext()` and `ReceiveNext()` overloads
6. Look for operation codes (0x01, 0x02, 0x03, etc.)

**What to Document**:
```
PhotonStream.SendNext(object value) does:
  1. Get type of value
  2. Write type code (1 byte)
  3. Write value data
  
Example:
  SendNext(Vector3.one) writes:
    [0x07] [3F800000][3F800000][3F800000]
  Where 0x07 = Vector3 type code
  And each 0x3F800000 = float 1.0
```

### 3.2 Second: Find RPC Definitions in Assembly-CSharp.dll

**Search Pattern**:
```csharp
[PunRPC]
void MethodName(...) { }
```

**Using dnSpy**:
1. Open `Assembly-CSharp.dll`
2. Search for `[PunRPC]` attributes
3. List every method with this attribute
4. Document signature: name, parameters, types

**Expected Findings** (typical mobile FPS):
```
[PunRPC] void PlayAnimation(string animName)
[PunRPC] void FireWeapon(Vector3 pos, Vector3 dir, int damage)
[PunRPC] void PlaySound(string soundName, Vector3 pos)
[PunRPC] void TakeDamage(int amount, int shooterID)
[PunRPC] void RespawnPlayer()
[PunRPC] void UpdateScore(int newScore)
[PunRPC] void KillNotification(int killerID, int victimID)
```

### 3.3 Third: Find OnPhotonSerializeView Implementations

**Search Pattern**:
```csharp
void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
{
    if (stream.IsWriting) { ... }
    else { ... }
}
```

**Using dnSpy**:
1. Search for `OnPhotonSerializeView` in Assembly-CSharp.dll
2. For each implementation, document:
   - Class name
   - Order of SendNext/ReceiveNext calls
   - Data types
   - Comments or logic

**Example Findings**:
```csharp
// In class PlayerController
void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
{
    if (stream.IsWriting)
    {
        stream.SendNext(transform.position);      // Vector3
        stream.SendNext(transform.rotation);      // Quaternion
        stream.SendNext(currentWeapon);           // int
        stream.SendNext(currentAmmo);             // int
        stream.SendNext(playerHealth);            // int
        stream.SendNext(isMoving);                // bool
    }
    else
    {
        networkPosition = (Vector3)stream.ReceiveNext();
        networkRotation = (Quaternion)stream.ReceiveNext();
        // ... etc
    }
}
```

### 3.4 Fourth: Identify Player Properties

**Search for**:
- `ExitGames.Client.Photon.Player`
- `Player.SetCustomProperties()`
- `Player.CustomProperties`

**Questions to Answer**:
1. What player properties does game store?
2. How are they serialized?
3. When are they updated?

**Likely Properties**:
```
PlayerProperties:
  - nickName: string
  - playerID: int
  - teamID: int (cops vs robbers?)
  - isReady: bool
  - score: int
  - customLevel: int
```

---

## Part 4: Binary Protocol Design

Based on reverse engineering, we need to design our LAN server protocol.

### 4.1 Operation Codes (Your Server Must Recognize)

```
0x01 = Connect (client initial connection)
0x02 = CreateRoom
0x03 = RPC Call
0x04 = StateSync (OnPhotonSerializeView data)
0x05 = JoinRoom
0x06 = LeaveRoom
0x07 = PlayerList Update
0x08 = Ping / Heartbeat
0x09 = Disconnect
0x0A = RoomList Request
```

### 4.2 Message Structure

**Header (Same for All)**:
```
Offset  Size  Field
0       1     OpCode
1       2     Payload Length (big-endian)
3       N     Payload (operation-specific)
```

**OpCode 0x01 - Connect**:
```
Offset  Size  Field
0       1     OpCode = 0x01
1       2     Payload Length = 8 + strlen(nickName)
3       1     Protocol Version (e.g., 0x01)
4       4     GameVersion Hash or Timestamp
8       N     NickName (null-terminated string)

Response from Server:
0       1     OpCode = 0x01 (echo)
1       2     Payload Length = 6
3       2     Player ID (assigned by server)
5       1     Result (0x00=success, 0x01=rejected)
6       4     Server Timestamp
```

**OpCode 0x02 - CreateRoom**:
```
Request:
0       1     OpCode = 0x02
1       2     Payload Length
3       1     Max Players
4       1     Is Open (0/1)
5       1     Is Visible (0/1)
6       N     Room Name (null-terminated string)

Response:
0       1     OpCode = 0x02
1       2     Payload Length
3       4     Room ID
7       1     Result (0x00=success, 0x01=name taken, etc.)
```

**OpCode 0x03 - RPC Call**:
```
0       1     OpCode = 0x03
1       2     Payload Length
3       4     View ID (which networked object)
7       1     Target (0x00=AllBuffered, 0x01=Others, 0xFF=MasterClient)
8       2     Method ID (hash of method name)
10      1     Parameter Count
11      N     Parameters (each: 1 byte type + data)
```

**OpCode 0x04 - StateSync**:
```
0       1     OpCode = 0x04
1       2     Payload Length
3       2     Player ID (who's sending state)
5       4     View ID
9       1     Parameter Count
10      N     Serialized state data (exactly as OnPhotonSerializeView sends)
```

### 4.3 Parameter Type Codes (from Photon)

Replicate these exactly:
```
0x00 = Null
0x01 = Boolean (1 byte)
0x02 = Byte (1 byte)
0x03 = Int (4 bytes)
0x04 = Long (8 bytes)
0x05 = Float (4 bytes)
0x06 = Double (8 bytes)
0x07 = Vector3 (12 bytes: 3 floats)
0x08 = Quaternion (16 bytes: 4 floats)
0x09 = String (2 bytes length + string data)
0x0A = Object[] (array)
0x0B = Hashtable (key-value pairs)
0x0C = Boolean[] (array)
0x0D = Byte[] (binary data)
0x0E = Float[] (array)
```

---

## Part 5: Implementation Checklist by File

### C# Server (if using C#)

**File 1: ServerCore.cs**
- [ ] TCP server socket binding to port 5055
- [ ] Accept incoming connections in thread
- [ ] Maintain list of connected clients
- [ ] Route messages to appropriate handlers

**File 2: MessageRouter.cs**
- [ ] Parse OpCode from message
- [ ] Dispatch to handler based on OpCode
- [ ] Error handling for malformed messages

**File 3: RoomManager.cs**
- [ ] Dictionary of active rooms
- [ ] CreateRoom(name, maxPlayers) → returns Room
- [ ] GetRoomList() → List<RoomInfo>
- [ ] FindPlayerById(id) → Player
- [ ] BroadcastToRoom(roomId, message) → sends to all players

**File 4: StateSync.cs**
- [ ] OnPlayerStateUpdate(playerId, viewId, state)
- [ ] Deserialize state data
- [ ] Broadcast to all other players in room
- [ ] Don't send back to sender

**File 5: RPCExecutor.cs**
- [ ] OnRPCReceived(viewId, methodId, params)
- [ ] Route to correct networked object
- [ ] Deserialize parameters matching type codes
- [ ] Broadcast to all in room

**File 6: BroadcastServer.cs**
- [ ] UDP socket on port 5056
- [ ] Broadcast server advertisement every 1 second
- [ ] Include: server name, current players, max players, TCP port

### Python Server (if using Python)

Same structure but with socketserver:
- [ ] `TCPServerRequestHandler` for connections
- [ ] `UDPBroadcaster` thread for UDP discovery
- [ ] `RoomManager` class
- [ ] `MessageRouter` class
- [ ] `StateSync` class

### APK Client Patches

**Java/Kotlin Modifications**:
1. [ ] Disable `PhotonNetwork.ConnectUsingSettings()`
   - Stub it out or redirect to LAN connection
2. [ ] Create `LanNetworkManager` class
   - Replicate PhotonNetwork interface
   - Direct TCP connection instead of Photon
3. [ ] Create `BroadcastListener` thread
   - Listen on UDP 5056
   - Parse server advertisements
   - Update UI with server list
4. [ ] Intercept `photonView.RPC()` calls
   - Serialize to our protocol
   - Send to server instead of Photon
5. [ ] Intercept `OnPhotonSerializeView()` 
   - Capture state data
   - Send to server at 10 Hz

---

## Part 6: Test Cases for Reverse Engineering

### Pre-Testing: Gather These Artifacts

Using dnSpy, extract and save:

1. **RPC Method List**
   ```
   Copy-paste from dnSpy search results:
   - FireWeapon(Vector3, Vector3, int)
   - PlayAnimation(string)
   - TakeDamage(int, int)
   - etc.
   ```

2. **OnPhotonSerializeView Code**
   ```
   Copy full method body for each networked class:
   - PlayerController
   - WeaponController
   - etc.
   ```

3. **PhotonView References**
   ```
   Find where photonView.RPC() is called:
   - In what methods?
   - With what parameters?
   ```

### Testing Protocol

**Test 1: Single Client Connection**
- [ ] Client connects to server
- [ ] Server assigns Player ID
- [ ] Client receives ID
- [ ] Client can list rooms

**Test 2: Room Creation**
- [ ] Create room (max 4 players)
- [ ] Server stores room
- [ ] Other clients see room in list

**Test 3: Room Join**
- [ ] Second client joins room
- [ ] Both players get PlayerList update
- [ ] Both see each other's presence

**Test 4: State Sync**
- [ ] Player 1 moves (changes position)
- [ ] Player 1's `OnPhotonSerializeView` called
- [ ] State data sent to server
- [ ] Server broadcasts to Player 2
- [ ] Player 2 receives and updates position

**Test 5: RPC Call**
- [ ] Player 1 calls `FireWeapon` RPC
- [ ] RPC serialized correctly
- [ ] Server receives
- [ ] Server broadcasts to Player 2
- [ ] Player 2's `FireWeapon` method called with correct params

---

## Part 7: Troubleshooting Guide

### Problem: "Server receives garbage data"
**Diagnosis**:
- [ ] Message OpCode not recognized
- [ ] Payload length mismatch
- [ ] Type codes wrong

**Solution**: 
- Capture raw bytes from Photon client using Wireshark
- Compare to server expectations
- Adjust parsing

### Problem: "State desync - players see each other at different positions"
**Diagnosis**:
- [ ] OnPhotonSerializeView not called at right time
- [ ] Type order wrong (receiving Vector3 as Quaternion)
- [ ] Network latency causing timing issues

**Solution**:
- Verify exact OnPhotonSerializeView implementation
- Log both send and receive data
- Add frame-by-frame verification

### Problem: "RPC methods not executing on remote clients"
**Diagnosis**:
- [ ] Method ID hashing wrong
- [ ] Parameters not deserializing
- [ ] RPC not reaching all players

**Solution**:
- Verify method ID calculation
- Log parameter types received
- Add RPC routing logging

---

## Conclusion

To successfully create a LAN server, you must:

1. **Reverse-Engineer Photon's Protocol**
   - Use dnSpy to examine Photon3Unity3D.dll
   - Document message format
   - Extract type codes

2. **Map Game's Network Usage**
   - Find all RPC methods in Assembly-CSharp.dll
   - Find all OnPhotonSerializeView implementations
   - Document exact order and types

3. **Design Compatible Protocol**
   - Binary format compatible with game's expectations
   - Same operation codes
   - Same parameter serialization

4. **Build Server & Client**
   - Server: TCP + UDP broadcast
   - Client: LAN discovery UI + message interception

5. **Test Thoroughly**
   - Start with 1-player (connection)
   - Grow to 2-player (state sync)
   - Expand to 4+ players (RPC routing)

**Estimated Effort**:
- Reverse engineering: 8-12 hours
- Server implementation: 16-20 hours
- Client patching: 12-16 hours
- Testing: 8-12 hours
- **Total: 44-60 hours of focused work**

