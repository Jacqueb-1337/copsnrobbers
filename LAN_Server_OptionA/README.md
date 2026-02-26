# Option A: Photon Server SDK (Self-Hosted)

## Overview
Use Photon's official on-premises SDK. Speaks the exact same protocol the game expects. No CCU limit on your own hardware.

## Steps
1. Download **Photon Server SDK** from: https://www.photonengine.com/sdks#server
2. Extract and run `PhotonControl.exe`
3. Deploy the `LoadBalancing` application (comes bundled)
4. Edit `LoadBalancing/GameServer/bin/Photon.LoadBalancing.dll.config`:
   - Set `PublicIPAddress` to your LAN IP
5. Companion mod sets `PhotonNetwork.PhotonServerSettings.ServerAddress` to your LAN IP
6. AppID can be anything (e.g. "CNRLan") - set same value in server config and mod

## Pros
- Full protocol compatibility guaranteed
- Handles all Photon edge cases (reconnect, encryption, etc.)
- No CCU limit on your own hardware (the SDK free tier limit only applies to Photon Cloud)

## Cons
- Requires Photon Server SDK download and setup
- Less control over server logic
