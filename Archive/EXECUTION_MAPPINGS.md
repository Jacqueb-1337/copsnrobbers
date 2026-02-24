# Execution Mappings - Common User Actions

This document traces the method call chains for common user actions in the Cops & Robbers game, showing exactly what happens when players interact with the game.

---

## ACTION 1: Connect to Game Server

**User Action:** Player clicks "Join Game" button in menu

**Entry Point:** `CNRConnectMenu.SwitchToServer()` (Line 44014+)

**Step-by-Step Execution Flow:**

```
1. User selects server region (Default/US/EU/Asia/JP)
   ↓
2. CNRConnectMenu.SwitchToServer(WWServerList serverEnum)
   - Reads server mapping from enum
   - Sets PhotonNetwork.PhotonServerSettings.ServerAddress = appropriate server IP
   
3. PhotonNetwork.Connect(serverAddress, port, appId, gameVersion)
   - Calls NetworkingPeer.Connect()
   
4. NetworkingPeer.Connect()
   - Initiates TCP socket connection
   - Sends initial handshake to Photon server
   
5. TCP Connection Established (Asynchronous)
   - NetworkingPeer receives handshake response
   - Validates server compatibility
   - Sets connectionState = Connecting
   
6. Photon Authentication
   - Sends player name and user ID
   - Server validates and creates PhotonPlayer object
   - PhotonNetwork.player becomes valid
   
7. Connection Complete
   - PhotonNetwork.connectionState = Connected
   - PhotonNetwork.connected = true
   
8. OnConnectionEstablished() Callback
   - CNRConnectMenu receives confirmation
   - UI updates to show "Connected"
   - Enable room list and create room buttons
   
9. User can now browse rooms
   - Calls CNRConnectMenu.JoinRoom() or CreateRoom()
```

**Network Events:**
- TCP connection to Photon server
- Authentication packet exchange
- Update PhotonNetwork static state

**Final State:**
- Player connected to Photon server
- Can view available rooms
- Can join existing game or create new room

---

## ACTION 2: Join Existing Game Room

**User Action:** Player clicks on a room in the lobby and clicks "Join"

**Entry Point:** `CNRConnectMenu.JoinRoom()` (Line 44050+)

**Step-by-Step Execution Flow:**

```
1. Player selects room from mCNRRoomInfoList
   - Room info includes: name, player count, max players, custom properties (map, mode)
   
2. CNRConnectMenu.JoinRoom(RoomInfo selectedRoom)
   - Validates room is still open and not full
   - Calls PhotonNetwork.JoinRoom(selectedRoom.name)
   
3. PhotonNetwork.JoinRoom(roomName)
   - Calls NetworkingPeer.OpJoinRoom(roomName)
   - Sends OperationCode.JoinGame packet to Photon
   
4. Photon Server Processes Request
   - Checks room exists and is open
   - Creates PhotonPlayer entry for joining player
   - Broadcasts EventCode.PlayerJoined to all players in room
   
5. Local Player: Receive Join Success
   - NetworkingPeer.OnEvent(EventCode.PlayerJoined, newPlayer)
   - PhotonNetwork.room = Room reference
   - PhotonNetwork.room.AddPlayer(newPlayer)
   
6. Remote Players: Receive Notification
   - enemyLogic instances load on opponent screens
   - New player appears in room
   - Update player count UI
   
7. Room State Synchronized
   - Current players list updated
   - Room properties (map, mode) received
   - Game state synchronized to joining player
   
8. Load Game Scene
   - If room is already in-game, load map scene
   - If waiting room, stay in lobby UI
   
9. PlayerLogic Initialized
   - Player spawns at team spawn point
   - Networking ready via PhotonView
   - Weapon system initialized
```

**Network Events:**
- Photon OperationCode.JoinGame (UP to server)
- Photon EventCode.PlayerJoined (DOWN from server to all)
- Room state and player list synchronization

**Related Classes:**
- CNRConnectMenu (UI trigger)
- PhotonNetwork (connection state)
- Room (room data)
- PhotonPlayer (player list)
- Game (scene loading)

**Final State:**
- Player is in room with other players
- Can see other players joining
- Ready to start/play game

---

## ACTION 3: Start Game - Load Map

**User Action:** Game host clicks "Start Game" or timer counts down

**Entry Point:** `Game.StartGame()` or `CNRMultiplayerManager.StartCountdown()`

**Step-by-Step Execution Flow:**

```
1. Host initiates game start
   - Calls Game.StartGame()
   - Or CNRMultiplayerManager.OnCountdownComplete()
   
2. Game.StartGame()
   - Validates all players ready
   - Selects map based on game mode (calls GetRealmapName())
   - Broadcasts EventCode.GameStarting with scene name
   
3. Photon Broadcast to All Players
   - Sends EventCode.GameStarting(mapName, gameMode, teams)
   - All players receive event
   
4. Local: Load Game Scene
   - SceneManager.LoadScene(mapName)
   - Calls AstarPath.Scan() if first time in scene
   
5. AstarPath Initialization
   - Scans terrain/obstacles to build navigation graph
   - Initializes A* node data
   - Preprocesses pathfinding heuristics
   
6. Spawn Management
   - Game instantiates PlayerLogic at team spawn point
   - Instantiates NPC enemies at strategic locations
   - Creates prop objects (boxes, barrels, etc.)
   
7. Network Synchronization Setup
   - PhotonView components register with NetworkingPeer
   - Each networked object gets unique ViewID
   - Owner assignment complete
   
8. AI Initialization
   - Each NPC creates Seeker component
   - Creates RVOController for collision avoidance
   - Attaches AIPath component for movement
   
9. Game Ready
   - Game state = InGame
   - Physics enabled
   - Player input active
   
10. Opponent Scene Load Notification
    - Remote players see you spawn in-game
    - enemyLogic creates visual representation
    - Position synchronized via PhotonView
```

**Network Events:**
- EventCode.GameStarting broadcast (map name, mode)
- Initial scene load acknowledgment
- PhotonView registration for all networked objects

**Critical System Initialization:**
- A* pathfinding graph scan (first-time initialization is expensive)
- RVO simulation start
- Physics engine activation
- Input system setup

**Related Classes:**
- Game (scene management)
- AstarPath (pathfinding)
- RVOSimulator (collision avoidance)
- PlayerLogic (player spawn)
- NPC (enemy spawn)
- PhotonView (network sync setup)

**Final State:**
- Game scene loaded and visible
- All players spawned and positioned
- Physics and AI active
- Gameplay begins

---

## ACTION 4: Move Player Character

**User Action:** Player drags joystick/swipes to move character

**Entry Point:** `JoyStickController.Update()` → `PlayerLogic.Update()`

**Step-by-Step Execution Flow:**

```
1. Touch Input Detected
   - JoyStickController.Update() reads touch position
   - Calculates joystick delta (x, y movement)
   
2. PlayerLogic.Update()
   - Reads joystick input from JoyStickController
   - Calculates desired movement direction
   
3. CharacterController.SimpleMove(direction * speed)
   - Applies movement with physics
   - Respects terrain collisions
   - Checks NavMesh constraints
   
4. RVOController.SetDesiredVelocity(direction * speed)
   - Sets desired velocity for collision avoidance
   
5. RVOSimulator.Update()
   - Scans for nearby agents (RVOController instances)
   - Computes Reciprocal Velocity Obstacles
   - Adjusts velocity to avoid collisions with other players
   - Updates RVOController.Velocity
   
6. CharacterController applies adjusted velocity
   - Final movement with collision avoidance
   - Updates transform.position
   
7. Camera Update
   - CameraRotation.Update() follows player
   - Maintains third-person view angle
   
8. Network Serialization (every 100ms or on significant change)
   - PhotonView.OnPhotonSerializeView(stream, info)
   - Writes: position, rotation, animation state
   - Sends to remote players
   
9. Remote Player Update
   - enemyLogic receives position update via OnPhotonSerializeView()
   - Interpolates movement smoothly
   - Updates visual transform
   
10. MiniMap Update
    - MiniMap.UpdateMarkers() refreshes player position markers
```

**Physics Integration:**
- CharacterController for local physics
- RVO for multi-agent coordination
- Network sync ~10 times per second (100ms intervals)

**Local vs Remote Behavior:**
- Local: Direct input → physics → network update
- Remote: Network update → interpolation → visual movement

**Related Classes:**
- PlayerLogic (input handling)
- CharacterController (local movement)
- RVOController (collision avoidance)
- RVOSimulator (multi-agent resolution)
- PhotonView (network sync)
- enemyLogic (remote player display)
- CameraRotation (camera follow)

**Final State:**
- Player moved on local screen
- Position synchronized to remote players
- No collision with other agents

---

## ACTION 5: Fire Weapon / Attack

**User Action:** Player taps fire button to shoot

**Entry Point:** `PlayerLogic.OnFireButtonDown()` → `WeaponController.Fire()`

**Step-by-Step Execution Flow:**

```
1. Fire Input Detected
   - PlayerLogic.OnFireButtonDown()
   - Checks: ammunition available, weapon cooldown expired, target valid
   
2. WeaponController.Fire()
   - Gets current Gun properties (damage, range, fireRate)
   - Checks fireRate cooldown (can't fire faster than rate)
   - Creates bullet/projectile
   
3. Instantiate Bullet
   - new GameObject BulletEnemy at weapon muzzle
   - Sets velocity toward aim direction
   - Assigns damage value from Gun
   
4. Deduct Ammunition
   - WeaponController.ammo -= 1
   - Syncs ammo count to UI
   - Checks if ammo depleted → swap weapon or add ammo
   
5. Fire Animation
   - PlayerLogic.PlayAnimation("Fire")
   - Weapon recoil effect
   - Visual/audio feedback
   
6. Network Broadcast Fire Event
   - PhotonView.RPC("Fire", PhotonTargets.Others, targetPosition)
   - Or: Sends OperationCode.Fire packet
   - Tells other players: "I fired in this direction"
   
7. Remote: enemyLogic Receives Fire Event
   - Plays fire animation on remote player
   - Creates visual muzzle flash
   - Audio plays for other players
   
8. Bullet Physics Update (Local)
   - BulletEnemy.Update() moves bullet each frame
   - Raycasts ahead to detect collisions
   
9. Hit Detection
   - BulletEnemy.OnTriggerEnter(collider)
   - Checks if hit: player, NPC, prop, terrain
   
10. If Hit Player:
    - Identifies target PlayerLogic or enemyLogic
    - Calls target.TakeDamage(damageAmount)
    - Broadcasts EventCode.PlayerDamaged with victim
    
11. If Hit NPC:
    - Calls NPC.TakeDamage(damageAmount)
    - NPC health -= damageAmount
    - If health <= 0: NPC.OnDeath() → despawn & add points
    
12. If Hit Prop:
    - Calls PropLogic.TakeDamage(damageAmount)
    - If destroyable: PropLogic.Destroy() → award points
    
13. Impact Effect
    - Instantiate Detonator explosion at hit point
    - Creates visual effects (smoke, sparks)
    - Plays impact sound
    
14. Bullet Cleanup
    - Destroy(bulletObject) after impact
```

**Network Synchronization:**
- Fire action broadcast to all players
- Damage event broadcast to update target health
- Death events synchronized

**Damage Calculation:**
- Damage = Gun.damage
- Reduced by distance if ranged weapon
- Critical multiplier if headshot
- Armor reduction if target has armor

**Related Classes:**
- PlayerLogic (input, animation)
- WeaponController (ammo, fire rate management)
- Gun (damage, range, fire rate properties)
- BulletEnemy (projectile physics)
- Detonator (visual effects)
- NPC (enemy health)
- PropLogic (destroyable objects)
- PhotonView (network sync)

**Final State:**
- Ammunition decreased
- Bullet fired toward target
- Impact creates damage/effects
- Other players see fire animation

---

## ACTION 6: Take Damage / Die

**User Action:** Player is hit by bullet or takes damage

**Entry Point:** `PlayerLogic.TakeDamage(damageAmount)` (called from bullet hit)

**Step-by-Step Execution Flow:**

```
1. Bullet Hits Player
   - BulletEnemy.OnTriggerEnter(PlayerLogic collider)
   - Calls PlayerLogic.TakeDamage(Gun.damage)
   
2. PlayerLogic.TakeDamage(damage)
   - health -= damage
   - Updates health bar UI
   - Plays damage reaction animation
   - Camera shake effect
   - Sound effect (pain grunt)
   
3. Broadcast Damage Event
   - PhotonView.RPC("TakeDamage", PhotonTargets.Others, damage)
   - Network: "I took X damage"
   
4. Remote Player Update
   - enemyLogic.TakeDamage(damage) receives network call
   - Updates remote player visual health bar
   - Plays animation on remote side
   
5. Check if Dead
   - If health <= 0:
     - OnDeath() called
   - Else:
     - Player continues playing (damaged but alive)
```

**Death Sequence (if health <= 0):**

```
6. PlayerLogic.OnDeath()
   - Animation: death ragdoll or death animation
   - PlayerLogic.alive = false
   - Disables player input
   - Disables WeaponController
   - Starts respawn timer (10 seconds)
   
7. Broadcast Death Event
   - PhotonView.RPC("OnDeath", PhotonTargets.Others)
   - Or: EventCode.PlayerDeath with victim/killer info
   
8. Game State Update
   - Game.OnPlayerDeath(killer, victim)
   - Award kill points to killer
   - Increment victim death counter
   - Check win condition
   - Update scoreboard UI
   
9. Remote: enemyLogic Death
   - enemyLogic.OnDeath() called
   - Plays death animation on other screens
   - Removes from local combat consideration
   - Name appears on kill log
   
10. Respawn Management
    - Wait respawnTimer (e.g., 10 seconds)
    - Select new spawn point
    - Reset health to max
    - Clear status effects
    - Create new visual representation at spawn
    
11. Respawn Broadcast
    - EventCode.PlayerRespawned sent to all players
    - Other players see you respawn at new location
```

**Damage Types:**
- Bullet damage: WeaponController weapons
- Explosive damage: Detonator splash radius
- Environmental damage: Lava, traps
- Fall damage: Height collision

**Related Classes:**
- PlayerLogic (health management)
- BulletEnemy (hit detection)
- Game (scoring)
- WeaponController (disable on death)
- Detonator (explosive damage)
- PhotonView (network sync)

**Final State:**
- If not dead: Health decreased, animation played
- If dead: Removed from gameplay, respawn timer started, kill added to scoreboard

---

## ACTION 7: Interact with Objective / Capture Stronghold

**User Action:** Player enters objective zone and holds button to capture

**Entry Point:** `PlayerLogic.OnTriggerStay()` → Stronghold capture logic

**Step-by-Step Execution Flow:**

```
1. Player Enters Objective Zone
   - Collider.OnTriggerEnter() called
   - Objective zone collider detects PlayerLogic
   
2. PlayerLogic checks if in capture zone
   - Calls OnObjectiveEnter()
   - UI shows "Press [button] to capture"
   - Objective state = AvailableForCapture
   
3. Player Holds Capture Button
   - PlayerLogic.OnCaptureHold() called each frame
   - captureProgress += captureRate * deltaTime
   
4. Capture Progress Increase
   - Visual: Progress bar fills from 0-100%
   - captureProgress reaches 100%
   
5. Capture Complete
   - PlayerLogic.OnCaptureComplete()
   - Broadcasts EventCode.ObjectiveCaptured(playerTeam)
   
6. Game State Update
   - Game.OnObjectiveCaptured(team)
   - Award objective points (+100 points)
   - Update team score
   - Check win condition (did team reach score limit?)
   
7. Remote: All Players Notified
   - Objective UI updates on all screens
   - Announcement: "Team [X] captured objective!"
   - Color changes on minimap
   
8. Objective Lockout
   - Objective locked by capturing team for duration (30 seconds)
   - Enemy team cannot recapture
   - Other team can contest (interrupt capture)
   
9. Defender Contest
   - If enemy player enters zone during capture:
     - captureProgress -= contestRate * deltaTime
     - Capture can be interrupted if enemy player present
```

**Win Condition Check:**

```
10. After Capture:
    - Game checks: Does capturing team now have winning score?
    - If yes: Game.OnGameEnd(winner)
```

**GameEnd Sequence (if objective captures to win):**

```
11. Game.OnGameEnd(winningTeam)
    - Disables all weapons
    - Displays score screen
    - Announces winner
    - Broadcasts EventCode.GameEnded to all players
    
12. Remote: All Players See Results
    - Game ends on all clients simultaneously
    - Score screen shows ranking
    - Respawn disabled
    - Return to menu button enabled
```

**Related Classes:**
- PlayerLogic (capture detection)
- Game (scoring, win condition)
- Objective component (capture zone)
- PhotonView (network sync)

**Final State:**
- Objective captured by team
- Team score increased
- May trigger game end if reaching win condition

---

## ACTION 8: Switch Server Region

**User Action:** Player clicks different region button (US/EU/Asia/JP) in server select menu

**Entry Point:** `CNRConnectMenu.SwitchToServer(serverEnum)` (re-entry)

**Step-by-Step Execution Flow:**

```
1. User Selects New Server Region
   - Clicks "US Server" / "EU Server" / etc.
   - Calls CNRConnectMenu.SwitchToServer(WWServerList.US)
   
2. Disconnect Current Server
   - PhotonNetwork.Disconnect()
   - NetworkingPeer closes TCP connection
   - Clears current Room reference
   - PlayerLogic destroyed (if in game)
   
3. Wait for Disconnection
   - Network buffer drained
   - All pending messages sent
   - Connection closed cleanly
   
4. Pause (0.5 seconds)
   - Ensures clean disconnect
   
5. Read New Server Address
   - switch(serverEnum) selects new server IP/hostname
   - Default: "app.exitgamescloud.com:5055"
   - US: "app-us.exitgamescloud.com:5055"
   - EU: "app-eu.exitgamescloud.com:5055"
   - ASIA: "app-asia.exitgamescloud.com:5055"
   - JP: "app-jp.exitgamescloud.com:5055"
   
6. Update Server Settings
   - PhotonNetwork.PhotonServerSettings.ServerAddress = newAddress
   
7. Connect to New Server
   - PhotonNetwork.Connect() called
   - Follows same connection flow as ACTION 1
   
8. Re-establish Connection
   - TCP handshake with new server
   - Authentication with player ID/name
   - PhotonNetwork.connectionState = Connected
   
9. Refresh Room List
   - CNRConnectMenu.RequestRoomList()
   - Photon sends available rooms on new server
   - Populates mCNRRoomInfoList with new rooms
   
10. UI Updates
    - Server button highlights new region
    - Room list refreshes with servers's games
    - Connection status shows "Connected to [Region]"
```

**Network Behavior:**
- Complete TCP/UDP connection reset
- New session created on different physical Photon server
- Room list is per-server (cannot join rooms from different servers)

**Related Classes:**
- CNRConnectMenu (UI and server selection)
- PhotonNetwork (connection management)
- ServerSettings (server address configuration)

**Final State:**
- Connected to different regional server
- New room list displayed
- Can join games on new server

---

## ACTION 9: Create New Game Room

**User Action:** Player fills in game name and clicks "Create Room"

**Entry Point:** `CNRConnectMenu.CreateRoom()` or `CNRMultiplayerManager.CreateGame()`

**Step-by-Step Execution Flow:**

```
1. User Enters Room Details
   - Game name: "My Game"
   - Max players: 4
   - Game mode: "Team Deathmatch"
   - Map: "Death Platform"
   - Clicks "Create" button
   
2. Validation
   - Checks room name not empty
   - Checks name not already in use
   - Validates player count in range (2-6)
   - Checks map exists
   
3. CreateRoom Request
   - Calls PhotonNetwork.CreateRoom(roomName, maxPlayers, properties)
   - Calls NetworkingPeer.OpCreateRoom()
   - Sends OperationCode.CreateGame to Photon
   - Includes custom properties (map name, game mode)
   
4. Photon Server Creates Room
   - Allocates new Room object
   - Sets properties (name, player limit, custom properties)
   - Sets room owner to creating player (host)
   - Sets room.IsOpen = true (can others join)
   
5. Local: Room Creation Success
   - PhotonNetwork.room = newly created Room
   - Player becomes room host
   - Enters room UI/lobby screen
   
6. Room Properties Set
   - Room.CustomProperties["map"] = "FreeRun3_1"
   - Room.CustomProperties["gameMode"] = "Deathmatch"
   - These broadcast to all connected players
   
7. Server Broadcast Room Created
   - Photon broadcasts EventCode.RoomListUpdate
   - Sends to all connected players
   - Shows up in CNRConnectMenu.mCNRRoomInfoList for other players
   
8. Host Ready
   - Host sees "Waiting for players..." screen
   - Can see players joining in real-time
   - Can edit game settings (if team count, etc.)
   
9. Other Players: See New Room
   - CNRConnectMenu.OnRoomListUpdated()
   - New room appears in available rooms list
   - Shows: name, player count (1/4), ping
   
10. Players Can Join (See ACTION 2)
    - New players click room and join
    - Joins success calls EventCode.PlayerJoined
    - All players see new player in list
```

**Host Privileges:**
- Start game (no one else can initiate)
- Kick players
- Change settings before game starts
- Assign teams
- Remove room if empty

**Room Properties (Custom Data):**
- Map name (scene to load)
- Game mode (Deathmatch, Objective, etc.)
- Required items (specific weapons available)
- Password (if private room)
- Team assignments

**Related Classes:**
- CNRConnectMenu (UI form)
- CNRMultiplayerManager (game coordination)
- PhotonNetwork (room creation)
- Room (room data)
- Game (room settings management)

**Final State:**
- New room created on Photon server
- Host is owner
- Room visible in lobby to other players
- Waiting for first player to join

---

## ACTION 10: Disconnect / Return to Main Menu

**User Action:** Player clicks "Exit Game" or "Disconnect" button

**Entry Point:** `PlayerLogic.OnExitGame()` or `Game.OnPlayerQuit()`

**Step-by-Step Execution Flow:**

```
1. Exit Button Clicked
   - Menu → "Disconnect" or "Exit Game"
   - Calls PlayerLogic.OnExitGame()
   
2. Clean Up Local Game
   - Destroys all local GameObjects
   - Stops input handling
   - Stops physics simulation
   - Frees audio resources
   
3. Remove from Game
   - If in room: Calls PhotonNetwork.LeaveRoom()
   - Sends OperationCode.LeaveGame to Photon
   
4. Photon: Player Left
   - Removes player from Room.Players
   - Broadcasts EventCode.PlayerLeft to remaining players
   
5. Remote: Other Players See You Leave
   - CNRLobby updates player list (you removed)
   - enemyLogic for your character destroyed
   - Name removed from scoreboard
   - Message: "[PlayerName] left the game"
   
6. Disconnect Network
   - Calls PhotonNetwork.Disconnect()
   - NetworkingPeer.Close()
   - TCP connection closed
   - All pending messages sent first
   
7. Clear Network State
   - PhotonNetwork.room = null
   - PhotonNetwork.player = null
   - Clear all player references
   
8. Load Menu Scene
   - SceneManager.LoadScene("MainMenu")
   - Returns to server select / room browser
   
9. Menu Ready
   - Player back at CNRConnectMenu
   - Can select different server or room
   - Connection status shows "Disconnected"
```

**If Network Unexpectedly Lost:**

```
Alternative: NetworkingPeer.OnConnectionFail()
- Photon connection dropped (network error)
- Automatic disconnect
- UI shows error message: "Connection lost. Try again?"
- Return to menu option
```

**Related Classes:**
- PlayerLogic (player cleanup)
- Game (game state)
- PhotonNetwork (connection)
- NetworkingPeer (low-level disconnect)
- CNRConnectMenu (menu return)

**Final State:**
- Disconnected from Photon server
- Game scene unloaded
- Back at main menu
- Ready to join different game or server

---

## COMMON NETWORK PACKET SUMMARY

| Action | OperationCode | EventCode | Content |
|--------|--------------|-----------|---------|
| Connect to Server | - | - | TCP handshake |
| Join Room | JoinGame | PlayerJoined | Player info |
| Leave Room | LeaveGame | PlayerLeft | Player ID |
| Start Game | - | GameStarting | Map, mode |
| Fire Weapon | - | Fire / ProjectileSpawned | Position, direction |
| Take Damage | - | PlayerDamaged | Victim, damage amount |
| Die | - | PlayerDeath | Killer, victim |
| Capture Objective | - | ObjectiveCaptured | Team, objective ID |
| Game End | - | GameEnded | Winner, score |
| Change Server | - | - | TCP reconnect |
| Sync Position | - | OnPhotonSerializeView | Position, rotation |

---

## STATE MACHINE OVERVIEW

```
DISCONNECTED
    ↓ (Connect button)
CONNECTING
    ↓ (Photon accepts)
CONNECTED (in menu/lobby)
    ↓ (Join/Create room)
IN_ROOM (waiting in lobby)
    ↓ (Host starts game)
IN_GAME (gameplay)
    ↓ (Game over or exit)
IN_ROOM (back to waiting)
    ↓ (Leave room)
CONNECTED (back to menu)
    ↓ (Disconnect)
DISCONNECTED
```

---

## PERFORMANCE NOTES

- **Network Tick Rate**: ~10 Hz (100ms between updates)
  - Position sync sent every 100ms
  - Faster for critical events (damage, death)

- **RVO Simulation**: 60 Hz (every frame)
  - Collision avoidance very responsive

- **A* Pathfinding**: Background threads
  - Searches happen asynchronously
  - No frame drops from pathfinding

- **Physics**: 60 Hz Unity fixed timestep
  - Matches animation framerate

- **UI Response**: ~30-60 Hz (game framerate)
  - Menu updates at framerate
