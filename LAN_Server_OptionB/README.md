# LAN Server — Option B (Custom TCP / Photon Protocol)

A custom server that speaks the Photon binary TCP protocol, built from wire-level
reverse engineering of `Photon3Unity3D.dll`. No Photon Server SDK required.

## Components

| File | Description |
|------|-------------|
| `src/PhotonBinaryProtocol.cs` | Full TCP framing + GpType serialization |
| `src/RoomManager.cs`          | Thread-safe room state |
| `src/PlayerSession.cs`        | Per-client TCP receive buffer + send queue |
| `src/MasterServer.cs`         | Port 5055 — auth, lobby, room list, redirect |
| `src/GameServer.cs`           | Port 5056 — room create/join, event relay |
| `src/Program.cs`              | Entry point, auto-detects LAN IP |

## How to run (Windows / .NET 8)

```powershell
# Auto-detect LAN IP:
dotnet run

# Explicit IP:
dotnet run -- 192.168.1.10
```

The server prints the IP to configure on the Android device.

## Companion mod: CNRIPRedirectMod.cs

Located at `APK_Build_Active/CNRIPRedirectMod.cs`. Compile and deploy it:

```powershell
# From APK_Build_Active:
dotnet-csc /r:UnityEngine.dll /r:Assembly-CSharp.dll /r:Photon3Unity3D.dll `
    CNRIPRedirectMod.cs /out:CNRIPRedirectMod.dll

adb push CNRIPRedirectMod.dll /storage/emulated/0/CNRMods/CNRIPRedirectMod.dll
```

Create config on the device:

```
adb shell "echo 'SERVER_IP=192.168.1.10' > /sdcard/CNRMods/server.cfg"
adb shell "echo 'SERVER_PORT=5055' >> /sdcard/CNRMods/server.cfg"
```

## Wire protocol summary (confirmed from Photon3Unity3D.dll IL)

### TCP frame (all messages)
```
[0]     0xF3            magic
[1..4]  total_len (BE)  includes 9-byte header
[5]     0x00            channel
[6]     0x00            flags (bit7=encrypted — we never set it)
[7]     0x00            reserved
[8]     msg_type        3=OpResponse, 4=EventData
[9+]    payload
```

### Init packet (client → server, 48 bytes)
```
[0]     0xFB
[1..4]  0x00 0x00 0x00 0x30   (= 48)
[5..6]  0x00 0x01
[7..15] 0xF3 0x00 0x01 0x06 0x01 0x03 0x00 0x01 0x07
[16..47] AppID padded to 32 bytes (zero-padded)
```

### Init response (server → client, 9 bytes)
```
0xF3 0x00 0x00 0x00 0x09 0x00 0x00 0x00 0x01
```

### Operation codes
| Hex | Dec | Name        | Direction |
|-----|-----|-------------|-----------|
| 0xE6 | 230 | Authenticate | both |
| 0xE5 | 229 | JoinLobby    | both |
| 0xE3 | 227 | CreateGame   | both |
| 0xE2 | 226 | JoinGame     | both |
| 0xFD | 253 | RaiseEvent   | C→S |
| 0xFE | 254 | Leave        | both |

### Key events from server
| Code | Name      | Key params |
|------|-----------|-----------|
| 230  | GameList  | 222=Hashtable{name→roomHt} |
| 255  | Join      | 254=actorNr, 249=props, 252=int[]actorList |
| 254  | Leave     | 254=actorNr |

### Room list Hashtable per-room entry keys
```
(byte)255  maxPlayers (byte)
(byte)253  isOpen (bool)
(byte)254  isVisible (bool)
(byte)252  playerCount (byte)
(byte)251  removedFromList (bool)
"map"      scene name string
"version"  must match client version
"mode"     "0"=TDM "1"=Stronghold "2"=KC
```

## Connection flow

```
Client                            Master (5055)         Game (5056)
  │── Init packet ──────────────────▶│                      │
  │◀─ Init response ─────────────────│                      │
  │── Op230 Authenticate ───────────▶│                      │
  │◀─ Op230 Response (ok) ───────────│                      │
  │── Op229 JoinLobby ──────────────▶│                      │
  │◀─ Op229 Response (ok) ───────────│                      │
  │◀─ Event230 GameList ─────────────│                      │
  │── Op226 JoinGame ───────────────▶│                      │
  │◀─ Op226 Response (gameServer) ───│                      │
  │═══════════ reconnect ════════════════════════════════════│
  │── Init packet ───────────────────────────────────────────▶
  │◀─ Init response ─────────────────────────────────────────│
  │── Op230 Authenticate ────────────────────────────────────▶
  │◀─ Op230 Response (ok) ───────────────────────────────────│
  │── Op226 JoinGame ────────────────────────────────────────▶
  │◀─ Op226 Response (actorNr, props) ───────────────────────│
  │◀─ Event255 Join (actorNr, actorList) ────────────────────│
  │   [OnJoinedRoom fires — game starts!]
```

## Known limitations / TODO

- No encryption (setType encryption bit 0x80 not supported — game uses plaintext by default)
- No auth token validation (we accept any auth)  
- No persistent room state (server restart = all rooms gone)
- RaiseEvent relay is complete but untested with game-specific event codes
- GpType.Dictionary, Vector, Custom types are read-only (no server-side send needed)
