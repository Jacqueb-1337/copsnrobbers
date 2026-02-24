# Cops n Robbers - LAN Server Extraction Feasibility Report

## Executive Summary

**YES, the embedded LAN server CAN potentially be extracted and run as a standalone PC executable.** The game is built on **Photon PUN (Photon Unity Networking) v1.17**, which supports self-hosted server architectures.

---

## Key Findings

### 1. Networking Architecture
- **Framework**: Photon PUN (Photon Unity Networking) v1.17
- **Connection Protocol**: UDP
- **Server Types Supported**:
  - PhotonCloud (cloud-based, default)
  - SelfHosted (local LAN server)
  - OfflineMode (single-player)

**Source**: `ServerSettings.cs` - Contains HostingOption enum with these three modes

### 2. Server Configuration Classes

#### ServerSettings.cs
```csharp
public enum HostingOption {
    NotSet,
    PhotonCloud,
    SelfHosted,
    OfflineMode
}

public static string DefaultCloudServerUrl = "app.exitgamescloud.com";
public static string DefaultServerAddress = "127.0.0.1";  // ← THIS IS KEY
public static int DefaultMasterPort = 5055;

public void UseMyServer(string serverAddress, int serverPort, string application) {
    HostType = HostingOption.SelfHosted;
    AppID = ((application == null) ? DefaultAppID : application);
    ServerAddress = serverAddress;
    ServerPort = serverPort;
}
```

**Critical**: The game is ALREADY CONFIGURED to support self-hosted servers on `127.0.0.1:5055`

### 3. Room/Multiplayer Implementation

#### Key Files Found (28 files with networking code):
1. **ConnectMenu.cs** - UI for joining/creating rooms
2. **MultiplayerSelectDirector.cs** - Main multiplayer game mode logic
3. **CRMultiplayerManager.cs** - Multiplayer state management
4. **PhotonNetwork.cs** - Central networking API (1061 lines)
5. **NetworkingPeer.cs** - Low-level network peer communication
6. **LoadbalancingPeer.cs** - Load balancing and room creation

#### CreateRoom Implementation (MultiplayerSelectDirector.cs:615)
```csharp
public void WWCreateRoomNStart() {
    string[] propsToListInLobby = new string[3] { "map", "version", "mode" };
    Hashtable hashtable = new Hashtable();
    hashtable.Add("map", mCurWWMapSelect);
    hashtable.Add("version", UserDataController.GetStrVersion());
    int num = (int)curModeSet;
    hashtable.Add("mode", num.ToString());
    
    PhotonNetwork.CreateRoom(
        UserDataController.GetMyRoomName(), 
        isVisible: true, 
        isOpen: true, 
        mWWMaxPlayersNum,  // max players
        hashtable2,        // custom room properties (map, version, mode)
        propsToListInLobby
    );
    PhotonNetwork.playerName = GrowthManagerKit.GetMyNickName();
}
```

### 4. Game State Synchronization

The game uses **Photon's RPC (Remote Procedure Call)** system for multiplayer gameplay:
- `PhotonView.RPC()` - Send method calls to other players
- `OnPhotonSerializeView()` - Synchronize continuous state (likely player position, rotation, etc.)
- `BroadcastMessageOnline()` - Send game events to all players

**Files using RPC**: WhoKilledWho.cs, PlayerLogic.cs, CRPlayer.cs (and others)

### 5. Offline Mode Capability

The game has a built-in offline/local mode (OfflineMode hosting option):
```csharp
public static bool offlineMode {
    set {
        isOfflineMode = value;
        if (isOfflineMode) {
            NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectedToPhoton);
            networkingPeer.ChangeLocalID(1);
            networkingPeer.mMasterClient = player;  // Player becomes master
        }
    }
}
```

This suggests **the game CAN run completely locally without a server**.

---

## Extraction Feasibility Assessment

### ✅ FAVORABLE FACTORS

1. **Photon is a Professional Middleware Library**
   - Already separated from game logic
   - Designed for distributed multiplayer
   - Self-hosting is documented feature

2. **Server Configuration Already Built In**
   - `DefaultServerAddress = "127.0.0.1"`
   - `DefaultMasterPort = 5055`
   - `UseMyServer()` method exists to configure custom servers

3. **Game Uses Standard Photon API**
   - CreateRoom(), JoinRoom(), RPC() - standard Photon calls
   - Not custom networking - easy to understand
   - 677 C# files decompiled for analysis

4. **Photon Server is Commercially Available**
   - Exit Games provides Photon OnPremise/Self-Hosted server software
   - This game likely intended to support it

### ⚠️ CHALLENGES

1. **Asset Extraction**
   - Game assets (maps, models, etc.) are in `assets/bin/Data/` 
   - Would need to be available on both client and server
   - Asset validation/checksum likely required

2. **Game-Specific Logic**
   - Room properties include game mode configuration ("map", "version", "mode")
   - Master client likely runs game simulation
   - Complex game state (player health, ammo, position) needs synchronization
   - Anti-cheat measures (if any) may complicate extraction

3. **Unity Engine Dependency**
   - Server currently runs inside Unity game engine
   - Would need to migrate to standalone C# server (Photon On-Premise or custom)
   - Physics/animation/audio don't need to run, but game logic does

4. **Photon Server Software**
   - Exit Games Photon On-Premise (commercial, ≈€500-2000/year per core)
   - Free Photon Server Community Edition exists but with limitations
   - Open-source alternative: Fusion (newer Photon framework)

---

## Extraction Approach Options

### Option 1: Photon Cloud to Local Server (Moderate Difficulty)
**Difficulty**: 4/10
**Timeline**: 40-80 hours

Replace the cloud server connection with local Photon On-Premise server:
1. Install Photon Server Community Edition (free, open-source)
2. Configure game to connect to `127.0.0.1:5055`
3. Extract and package game assets for server distribution
4. Deploy on local network

**Pros**: Minimal code changes needed  
**Cons**: Need Photon server running on separate machine

### Option 2: Custom C# Server Implementation (Difficult)
**Difficulty**: 7/10
**Timeline**: 100-200 hours

Extract multiplayer logic into standalone C# server:
1. Port game logic from Unity to .NET Core console app
2. Implement custom protocol (replacing Photon)
3. Handle game simulation (damage, spawning, etc.)
4. Implement anti-cheat if needed

**Pros**: Full control, doesn't require Photon server software  
**Cons**: Time-intensive, complex game logic migration

### Option 3: Offline Mode Enhancement (Easy)
**Difficulty**: 2/10
**Timeline**: 5-20 hours

Extend existing OfflineMode for LAN play:
1. Enable OfflineMode on host device
2. Modify networking to use UDP broadcast for discovery
3. Implement local player connection (split-screen or networked)

**Pros**: Minimal changes, reuses existing code  
**Cons**: Limited to small LAN, more local coop than true server

---

## Code Structure Summary

**Total Decompiled C# Files**: 677

**Key Directories**:
- `Photon/` - Photon networking library (88 files)
  - LoadbalancingPeer.cs - Handles room creation/joining
  - NetworkingPeer.cs - UDP communication
  - PhotonNetwork.cs - Main API (1061 lines)
  
- `Properties/` - Main game code (many files)
  - ConnectMenu.cs - Lobby UI
  - MultiplayerSelectDirector.cs - Game mode selection
  - CRMultiplayerManager.cs - Multiplayer state
  - CRGame.cs - Game simulation
  - CRPlayer.cs - Player entity
  - PlayerLogic.cs - Player behavior
  - WhoKilledWho.cs - Death events

- `AnimationOrTween/` - Game mechanics
- `Pathfinding/` - AI navigation

---

## Next Steps if Pursuing Extraction

1. **Research Photon Server**
   - Review Photon On-Premise documentation
   - Evaluate community edition features
   - Check licensing costs

2. **Analyze Game Logic**
   - Read CRMultiplayerManager.cs fully (currently ~600 lines)
   - Read CRGame.cs for simulation logic
   - Understand master client responsibilities
   - Identify what game state must be synchronized

3. **Asset Pipeline**
   - Determine how maps/game assets are loaded
   - Check if they're version-locked
   - Evaluate portability to server

4. **Testing**
   - Set up local Photon server instance
   - Modify game client to point to localhost
   - Test room creation/joining locally
   - Verify multiplayer gameplay works

---

## Conclusion

**Technical Feasibility: YES, Extraction is Possible**

The game's use of Photon PUN (industry-standard networking) and built-in support for self-hosted servers makes extraction realistic. The challenge is not technical impossibility but rather:

- Time investment (40-200 hours depending on approach)
- Photon server software costs (if using commercial version)
- Complexity of extracting/duplicating game simulation logic

**Recommended Approach**: Start with **Option 1** (Photon On-Premise server) as it requires minimal code changes and leverages existing game architecture.

---

## Files Analyzed for This Report

- ServerSettings.cs (55 lines)
- PhotonNetwork.cs (1,061 lines) 
- MultiplayerSelectDirector.cs (733 lines)
- ConnectMenu.cs (199 lines)
- 24 additional networking files

**Total: 28 files containing networking/multiplayer code**
