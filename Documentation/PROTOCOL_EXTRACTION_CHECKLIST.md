# Photon Protocol Reverse-Engineering Specification

## Purpose

This document defines exactly what must be extracted from the game's DLLs to make a LiteNetLib-based server work. Follow this checklist to ensure complete protocol understanding.

---

## Part 1: Protocol Extraction Checklist

### 1.1 Operation Codes (From Photon3Unity3D.dll)

**What These Are**: Numeric codes that identify message types

**Where to Find**: 
- dnSpy → Search for `OperationCode` enum or constants
- Look in: `ExitGames.Client.Photon` namespace

**What to Document**:
```
OpCode: 0x?? = Meaning
Example:
OpCode: 0x01 = Authenticate
OpCode: 0x02 = JoinGame
OpCode: 0x03 = CreateGame
OpCode: 0x04 = Leave
OpCode: 0x05 = RaiseEvent (THIS IS RPCS!)
...
```

**Why Important**: Server must recognize these to parse messages

**Verification**: Count should be 10-20 codes typically

**Extraction Instructions**:
1. Open dnSpy
2. Load: `target/assets/bin/Data/Managed/Photon3Unity3D.dll`
3. Search: `OperationCode`
4. Find: Enum or constant definitions
5. Document: All codes and their names
6. Screenshot: Full list for reference

---

### 1.2 Event Codes (From Photon3Unity3D.dll)

**What These Are**: Numeric codes for events sent from server to clients

**Where to Find**:
- dnSpy → Search for `EventCode` enum
- Look in: `ExitGames.Client.Photon` namespace

**What to Document**:
```
EventCode: 0x?? = Meaning
Example:
EventCode: 0x01 = PlayerConnected
EventCode: 0x02 = PlayerDisconnected
EventCode: 0x03 = PropertiesChanged
EventCode: 0x04 = RaiseEvent
...
```

**Why Important**: Clients must recognize these in responses

**Verification**: Count should be 5-15 codes typically

---

### 1.3 Type Codes (From PhotonStream Class)

**What These Are**: Serialization format identifiers for data types

**Where to Find**:
- dnSpy → Search for `PhotonStream`
- Look at: `SendNext()` method overloads
- Search for: Type code constants or byte values

**What to Document**:

```csharp
// Format for each type:
Type Code: 0x?? = Type Name
Size: N bytes (fixed) or variable
Serialization: [description]

Examples:
Type Code: 0x00 = Null
Type Code: 0x01 = Boolean (1 byte: 0x00=false, 0x01=true)
Type Code: 0x02 = Byte (1 byte: 0x00-0xFF)
Type Code: 0x03 = Short (2 bytes, big-endian)
Type Code: 0x04 = Int (4 bytes, big-endian)
Type Code: 0x05 = Long (8 bytes, big-endian)
Type Code: 0x06 = Float (4 bytes, IEEE 754)
Type Code: 0x07 = Double (8 bytes, IEEE 754)
Type Code: 0x08 = String (2-byte length + UTF-8 chars)
Type Code: 0x09 = ByteArray (2-byte length + bytes)
Type Code: 0x0A = Vector3 (3 × 4-byte floats)
Type Code: 0x0B = Quaternion (4 × 4-byte floats)
Type Code: 0x0C = Color (4 bytes RGBA)
...
```

**Why Important**: ALL parameter serialization depends on this

**Verification**: Should have ~15-20 type codes

**Extraction Instructions**:
1. Search for `SendNext` implementations
2. Look for each overload (one per type)
3. Note: There's often a common pattern like `writer.Put(type_code)` followed by `writer.Put(value)`
4. Document the type code byte for each
5. Create table with all codes

---

### 1.4 RPC Methods (From Assembly-CSharp.dll)

**What These Are**: Game methods called remotely via [PunRPC] attribute

**Where to Find**:
- dnSpy → Load Assembly-CSharp.dll
- Search: `[PunRPC]` attribute
- Or search: "PunRPC"

**What to Document** (FOR EACH METHOD):

```
Class: ClassName
Method: MethodName
Signature: void MethodName(ParamType1 param1, ParamType2 param2, ...)

Examples:
[PlayerController.cs]
  void FireWeapon(Vector3 position, Vector3 direction, int damage)
  void TakeDamage(int amount, int shooterID)
  void PlayAnimation(string animName)
  void PlaySound(string soundName)
  void Die(int killerID)
  
[GameManager.cs]
  void RoundStart()
  void RoundEnd(int winnerID)
  void UpdateScore(int playerID, int newScore)
  
[WeaponController.cs]
  void Reload()
  void Equip(int weaponID)
  void Fire()
```

**Why Important**: Server must know which methods exist to route calls correctly

**Verification** Count: 15-40 methods typical for FPS

**Extraction Instructions**:
1. Open dnSpy with Assembly-CSharp.dll
2. Search: `[PunRPC]` (exact search)
3. For each result:
   - Note the class name
   - Copy the method signature exactly
   - Document parameter types (int, string, Vector3, etc.)
4. Create comprehensive list

---

### 1.5 OnPhotonSerializeView Implementations (From Assembly-CSharp.dll)

**What These Are**: Method called every frame to sync player state

**Where to Find**:
- dnSpy → Search for `OnPhotonSerializeView`
- Look for implementations in networked classes

**What to Document** (FOR EACH CLASS):

```csharp
// Format:
Class: ClassName
Location: In which GameObject/Prefab

Stream.IsWriting (Local player sending state):
  1. stream.SendNext(dataType1) - meaning/description
  2. stream.SendNext(dataType2) - meaning/description
  ...

Stream.IsReading (Remote player receiving state):
  1. dataType1 = stream.ReceiveNext() - meaning
  2. dataType2 = stream.ReceiveNext() - meaning
  ...

Example:
Class: PlayerController
Location: Player prefab

Stream.IsWriting:
  1. stream.SendNext(transform.position)     // Vector3
  2. stream.SendNext(transform.rotation)     // Quaternion
  3. stream.SendNext(currentHealth)          // int
  4. stream.SendNext(ammunition)             // int
  5. stream.SendNext(isMoving)               // bool
  6. stream.SendNext(currentWeapon)          // int

Stream.IsReading:
  1. Vector3 pos = (Vector3)stream.ReceiveNext()
  2. Quaternion rot = (Quaternion)stream.ReceiveNext()
  3. int health = (int)stream.ReceiveNext()
  4. int ammo = (int)stream.ReceiveNext()
  5. bool moving = (bool)stream.ReceiveNext()
  6. int weapon = (int)stream.ReceiveNext()
```

**CRITICAL**: Order MUST be exact - if SendNext order doesn't match ReceiveNext order, data corruption

**Why Important**: This defines the network heartbeat of the game

**Verification**: 
- Count: 3-8 classes usually have this
- Test: Can verify by running packet capture

**Extraction Instructions**:
1. Search for `OnPhotonSerializeView`
2. For each implementation found:
   - Copy the entire method body
   - Note the class it's in
   - Document order of SendNext/ReceiveNext calls
   - Note data types
3. Create table showing all classes

---

### 1.6 Room Properties Structure (From Assembly-CSharp.dll & Photon3Unity3D.dll)

**What These Are**: Metadata about multiplayer rooms

**Where to Find**:
- Search: `RoomOptions`
- Search: `Room.SetCustomProperties`
- Search: `Hashtable` usage with room data

**What to Document**:

```
Built-in Room Properties:
  - MaxPlayers: int (maximum capacity)
  - IsOpen: bool (can new players join)
  - IsVisible: bool (appears in room list)
  - Name: string (room name)
  - PlayerCount: int (current players)
  - RemovedActors: int[] (removed player IDs)

Custom Room Properties (Game-Specific):
  - [Document any custom properties stored]
  
Example (For this game):
  - GameMode: string (capture flag, team deathmatch, etc.)
  - Map: string (level name)
  - GameState: int (lobby, playing, finished)
  - Team1Score: int
  - Team2Score: int
```

**Why Important**: Server must track these for matchmaking and state

---

### 1.7 Player Properties Structure (From Assembly-CSharp.dll & Photon3Unity3D.dll)

**What These Are**: Metadata about each player

**Where to Find**:
- Search: `Player.SetCustomProperties`
- Search: `PlayerProperties`
- Search: `CustomProperties` on Player objects

**What to Document**:

```
Built-in Player Properties:
  - Name: string (player nickname)
  - ID: int (player ID / actor number)
  - IsMasterClient: bool
  - IsLocal: bool

Custom Player Properties (Game-Specific):
  - [Document any custom properties]
  
Example (For this game):
  - Team: int (0 = cops, 1 = robbers)
  - Score: int
  - Kills: int
  - Deaths: int
  - Level: int
```

**Why Important**: Server needs these for player state management

---

### 1.8 Message Handshake Flow (From Photon3Unity3D.dll)

**What This Is**: Exact sequence of messages during connection

**Where to Find**:
- Search: `NetworkingPeer.Connect` or `Connect` method
- Look at: Protocol initialization code
- Examine: Message queue/handler

**What to Document**:

```
Connection Handshake:

CLIENT → SERVER:
  Message 1: [OpCode: 0x??] [Data: ???]
    Purpose: Initial connection
    Fields: [list each field]
  
SERVER → CLIENT:
  Response: [OpCode: 0x??] [Data: ???]
    Purpose: Connection acknowledge
    Fields: [list each field]

CLIENT → SERVER:
  Message 2: [OpCode: 0x??] [Data: ???]
    Purpose: Authenticate
    Fields: [list each field]
  
SERVER → CLIENT:
  Response: [OpCode: 0x??] [Data: ???]
    Purpose: Authentication response
    Fields: [list each field]

... (continue for full handshake)
```

**Why Important**: Connection won't work without matching this exactly

---

## Part 2: Verification Checklist

### After Extraction - Verify Each Item

**OpCodes**:
- [ ] List has 10-20 codes
- [ ] All have names/meanings
- [ ] Codes are unique (no duplicates)
- [ ] Include 0x05 (RaiseEvent) for RPCs

**Type Codes**:
- [ ] List has 15-25 codes
- [ ] All types documented with sizes
- [ ] Vector3, Quaternion, String included
- [ ] Null type code included

**RPC Methods**:
- [ ] List has 15-40 methods
- [ ] All have full signatures with parameters
- [ ] All have types documented
- [ ] No duplicates

**OnPhotonSerializeView**:
- [ ] All implementations found (3-8 typical)
- [ ] Order documented for each
- [ ] Data types match between Send and Receive
- [ ] Same count of Send and Receive calls per method

**Properties**:
- [ ] Room properties documented
- [ ] Player properties documented
- [ ] Built-in vs custom marked
- [ ] Types documented

**Handshake**:
- [ ] All messages in sequence
- [ ] All fields documented
- [ ] Expected responses noted

---

## Part 3: Documentation Template

### Create File: `PROTOCOL_SPECIFICATION.md`

```markdown
# Photon Protocol Specification (Reverse-Engineered)

## 1. Operation Codes

[Your extracted OpCodes here]

## 2. Event Codes

[Your extracted EventCodes here]

## 3. Type Codes

[Your extracted Type codes here]

## 4. RPC Methods

[Your extracted RPC list here]

## 5. OnPhotonSerializeView Implementations

[Your extracted Serialize implementations here]

## 6. Properties

### Room Properties
[Documented here]

### Player Properties
[Documented here]

## 7. Message Handshake

[Your handshake flow documented here]

## 8. Data Structure Diagrams

[ASCII diagrams of message formats here]

## 9. Verification Test Cases

[Test cases you'll use to verify here]
```

---

## Part 4: How to Use This During Implementation

### Server Implementation

```csharp
// 1. Parse incoming message
byte opCode = reader.GetByte();

// 2. Use your documented OpCodes
switch (opCode)
{
    case 0x05: // From your OpCodes table = RaiseEvent
        HandleRpcCall(reader);
        break;
    case 0x02: // From your OpCodes table = JoinGame
        HandleJoinGame(reader);
        break;
}

// 3. When serializing response
NetDataWriter writer = new NetDataWriter();
writer.Put((byte)0x04); // From your EventCodes table
writer.Put(playerId);
// ... more fields

// 4. When deserializing parameters
byte typeCode = reader.GetByte();
switch (typeCode)
{
    case 0x04: // From your Type codes table = Int
        int value = reader.GetInt();
        break;
    case 0x0A: // From your Type codes table = Vector3
        var vector = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        break;
}
```

### Client Implementation

```csharp
// 1. When sending RPC
var writer = new NetDataWriter();
writer.Put((byte)0x05); // RaiseEvent OpCode from your table
writer.Put(viewId);
writer.Put(GetMethodHash(methodName));
writer.Put((byte)paramCount);

foreach (var param in parameters)
{
    byte typeCode = GetTypeCode(param); // Use your type table
    writer.Put(typeCode);
    SerializeValue(writer, param, typeCode);
}

// 2. When receiving state update
for (int i = 0; i < expectedCount; i++) // From your SerializeView list
{
    byte typeCode = reader.GetByte();
    object value = DeserializeValue(reader, typeCode); // Use your type table
}
```

---

## Part 5: Common Extraction Mistakes

**❌ WRONG**: Guessing at OpCodes
**✅ RIGHT**: Using dnSpy to extract exact values

**❌ WRONG**: Assuming Vector3 is 12 bytes
**✅ RIGHT**: Verifying exact byte count from code

**❌ WRONG**: Getting RPC order wrong (1 parameter vs 2)
**✅ RIGHT**: Copying exact method signature from dnSpy

**❌ WRONG**: OnPhotonSerializeView Send/Receive count mismatch
**✅ RIGHT**: Counting exact calls in both branches

**❌ WRONG**: Trusting documentation
**✅ RIGHT**: Verifying with actual game code

---

## Part 6: Quick Reference Format

After extraction, create this quick reference:

```
OpCode Reference:
  0x01: Authenticate      | 0x02: JoinGame      | 0x03: CreateGame
  0x04: Leave            | 0x05: RaiseEvent    | 0x06: GetGameList

Type Code Reference:
  0x04: Int (4 bytes)    | 0x06: Float (4 bytes) | 0x0A: Vector3 (12 bytes)
  0x0B: Quaternion (16)  | 0x08: String (var)   | 0x00: Null

RPC Methods (Count: XX):
  PlayerController.FireWeapon(Vector3, Vector3, int)
  PlayerController.TakeDamage(int, int)
  PlayerController.PlayAnimation(string)
  [... more]

SerializeView Classes (Count: X):
  PlayerController: [pos, rot, health, ammo, moving, weapon]
  WeaponController: [equipped, ammo, reload_time]
  [... more]
```

---

## Success Criteria

You're done reverse-engineering when:

✅ Every OpCode has a documented meaning
✅ Every Type Code has format and size documented
✅ Every RPC method has full signature
✅ Every OnPhotonSerializeView has exact order and types
✅ You can explain handshake sequence from memory
✅ You created a spreadsheet/document you reference constantly
✅ You can write a parser for each message type
✅ Test packets parse without errors

---

**This becomes your source of truth during implementation.**
Keep it updated as you discover more details.

