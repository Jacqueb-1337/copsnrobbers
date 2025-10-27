# Understanding "Local WiFi" vs "Worldwide" Multiplayer

## What You're Actually Using

The game has TWO multiplayer modes:

### 1. "Local WiFi" (Local Server - WiFi)
- **Technology**: Apple GameKit P2P (peer-to-peer)
- **How it works**: Direct connection between 2 devices without a server
- **Port**: NOT a standard port - uses custom peer discovery
- **Why you can't see it from PC**: 
  - It's not listening on ports 5055/5056
  - GameKit doesn't use those ports
  - PC can't participate (GameKit is iOS-only in this game)
  - No discoverable "room list" - just direct peer connections

### 2. "Worldwide" (Online Rooms via Photon Cloud)
- **Technology**: Photon PUN v1.17 connecting to cloud servers
- **How it works**: Both devices connect to `app.exitgamescloud.com` and find each other there
- **Port**: 5055/5056 but OUTBOUND only (phone connects OUT to cloud, not IN from PC)
- **Why PC can't see it**:
  - Game doesn't listen on those ports
  - PC would need to connect to same Photon cloud account
  - Version mismatch filtering may hide the room anyway

## Why Your Setup Won't Work "Out of the Box"

You had the right idea - UDP 5055/5056 should be where a local server opens. But:

1. **"Local WiFi" mode**: Doesn't use Photon or UDP 5055/5056 at all
2. **"Worldwide" mode**: Uses Photon Cloud, not local networking

## To Actually Make LAN Multiplayer Work

You would need to:

**Option A: Modify the game to use Photon SelfHosted mode instead of PhotonCloud**
- Change `ServerSettings.HostType` from `PhotonCloud` to `SelfHosted`
- Point it to `127.0.0.1:5055` (on phone) or your PC's IP
- This requires binary patching of the PhotonServerSettings asset file

**Option B: Accept "Worldwide" mode limitations**
- Keep using Photon Cloud
- Ensure both apps have identical version strings (they might already if they're the same APK)
- Both devices must have internet to reach `app.exitgamescloud.com`
- Rooms will appear in the Worldwide menu, but only if versions match

**Option C: Use "Local WiFi" mode**
- This actually works for local multiplayer
- But it's peer-to-peer, not client-server
- Only supports direct connections between discovered peers
- No "rooms list" like traditional servers

## Code Evidence

**LocalWiFi mode just loads the level:**
```csharp
public IEnumerator StartLWFScene()
{
    ToSubScene(MSD_SubScene.Loading);
    yield return new WaitForSeconds(0.5f);
    Application.LoadLevel(mCurLWFMapSelect);  // ← No network code, just load level
}
```

**It doesn't even use Photon:**
```csharp
if (multiplayerMode == MultiMode.LocalWiFi && GameKitManager.isConnected())
{
    // GameKit = Apple peer-to-peer networking
    // NOT Photon
}
```

## The Bottom Line

- **Local WiFi**: P2P direct connection, can't see from PC
- **Worldwide**: Cloud-based, needs Photon subscription/account
- **Neither** actually listens on local UDP 5055/5056 for LAN discovery

To make true LAN work, you'd need to modify the game to use Photon's SelfHosted mode, which is the binary asset patching approach we discussed earlier.

