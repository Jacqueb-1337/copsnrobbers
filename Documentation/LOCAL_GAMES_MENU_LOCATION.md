# Where to Find Local Games in Cops n Robbers

## Menu Navigation

**Main Flow:**
```
Main Menu
  ↓
Multiplayer Button
  ↓
CHOICE Screen (Select game mode)
  ├─ LOCAL (shown on left)
  └─ WORLDWIDE (shown on right)
    ↓
  WORLDWIDE screen
    ├─ Room Choice (default when entering Worldwide)
    │    └─ Shows list of available rooms
    │    └─ "Quick Join" button to join any available room
    │
    └─ Room Create
         └─ For creating new rooms
```

## Key Code Classes

### Entry Point: `MultiplayerSelectDirector.cs`
- **Main controller** for multiplayer menus
- Manages three UI root panels:
  - `mUIRootChoice` - The initial Choice screen (Local vs Worldwide)
  - `mUIRootLocal` - Local mode (likely WiFi/Bluetooth)
  - `mUIRootWorldWide` - Online rooms via Photon
  - `mUIRootLoading` - Loading screen

### Room List Display: `ConnectMenu.cs` / `CNRConnectMenu.cs`
- Retrieves available rooms via: `PhotonNetwork.GetRoomList()`
- Continuously updates room list
- Shows "Join Room" buttons for each available room

### Menu Buttons: `MapSelectButtonEvent.cs`
- **`LocalChoice`** button → Takes you to LOCAL mode
- **`WorldWideChoice`** button → Takes you to WORLDWIDE mode with room list
- **`WWJoin`** → Join Room screen
- **`WWQuickStart`** → Automatically join any available room (calls `WWJoinRoomNQuickStart()`)

## Code Logic for Room Discovery

```csharp
// Get current rooms
RoomInfo[] roomList = PhotonNetwork.GetRoomList();

// Filter: Only show joinable rooms matching your version
for (int i = 0; i < roomList.Length; i++)
{
    if (roomList[i].maxPlayers != roomList[i].playerCount &&  // Not full
        (string)roomList[i].customProperties["version"] == UserDataController.GetStrVersion())  // Same version
    {
        // Show this room in the list
    }
}

// If rooms exist: Display them for joining
// If no rooms: Show "<Tips: No room created...>"
// If all rooms full: Show "<Tips: All rooms are full...>"
```

## Where Local Games Will Appear

**When running a Photon LAN server on your PC:**

1. **Go to Main Menu** → **Multiplayer**
2. **Choose WORLDWIDE** (even though it says "Worldwide", this is where networked rooms appear)
3. **Wait for room list to load** (it queries Photon via UDP 5056 discovery)
4. **Available rooms appear in a scrollable list**
5. Click **"Quick Join"** or select a specific room and click **"Join"**

## Important Notes

- **Port 5056 (UDP)**: Discovery broadcast - Photon sends "I have rooms available" messages
- **Port 5055 (UDP)**: Game traffic - Actual game communication
- **LAN Discovery is automatic**: If Photon server is running on your PC and both devices are on the same WiFi, the game will automatically discover it
- **No version check failure**: Game compares version in room properties - your server needs to set this correctly
- **Room must have space**: Game won't show full rooms
- **"Quick Join"** calls `WWJoinRoomNQuickStart()` which randomly picks an available room

## Code References

| Method | File | Purpose |
|--------|------|---------|
| `ToSubScene(MSD_SubScene.Choice)` | MultiplayerSelectDirector.cs | Show Local/Worldwide choice screen |
| `ToSubScene(MSD_SubScene.WorldWide)` | MultiplayerSelectDirector.cs | Switch to Worldwide mode |
| `WWJoinRoomNQuickStart()` | MultiplayerSelectDirector.cs | Auto-join first available room |
| `WWJoinRoomNStart()` | MultiplayerSelectDirector.cs | Join specific room |
| `PhotonNetwork.GetRoomList()` | PhotonNetwork.cs | Get current available rooms |
| `OnReceivedRoomList` | PhotonNetworkingMessage.cs | Callback when room list received |

