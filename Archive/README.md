# Cops n Robbers - LAN Server Override Project

A custom LAN server implementation for the Cops n Robbers mobile game, replacing cloud-based Photon networking with a local area network solution via ZeroTier VPN.

## 🎮 Project Overview

This project implements a **custom UDP/TCP game server** for Cops n Robbers that bypasses the limitations of Photon v3.0.2 SDK. The game runs on Android while the server runs on Windows PC, communicating over a ZeroTier virtual network.

### Key Features

- ✅ **Custom LAN Server** - C# .NET Core 8 server handling game logic and player management
- ✅ **ZeroTier VPN Integration** - Secure VPN tunnel for PC-to-phone communication (overcomes Firebase/Photon restrictions)
- ✅ **UDP Game Protocol** - Optimized for low-latency gameplay
- ✅ **TCP Test Protocol** - Diagnostic communication on port 5057
- ✅ **Room Management** - Room creation, player joining, and game state synchronization
- ✅ **APK Modification** - Compiled game code (Assembly-CSharp.dll) edited via dnSpy to integrate custom networking

## 📁 Project Structure

```
copsnrobbers/
├── LAN_Server/              # C# .NET Core 8 server implementation
│   ├── Program.cs           # Server entry point & configuration
│   ├── LanGameServerUdp.cs  # UDP game protocol handler
│   ├── OperationHandler.cs  # Game operations processing
│   ├── PhotonProtocol.cs    # Custom Photon-compatible protocol
│   ├── GameRoomManager.cs   # Room management logic
│   └── ...
├── target/                  # Original game APK (decompiled reference)
├── Documentation/           # Development guides and specifications
├── CNRConnectMenu.cs        # Reference game code (CNRConnectMenu MonoBehaviour)
└── .gitignore               # Git configuration
```

## 🚀 Quick Start

### Prerequisites
- Windows PC with .NET Core 8 runtime
- Android phone with ZeroTier app installed
- APK build tools: apktool, jarsigner, adb
- dnSpy for decompiling/modifying game assemblies

### Server Setup

```powershell
cd LAN_Server
dotnet build --configuration Release
dotnet run --configuration Release
```

Server will start on:
- **UDP Port 5055** - Main game protocol
- **UDP Port 5056** - Game list broadcast
- **TCP Port 5057** - Test byte protocol (diagnostic)

### Client Setup (Android)

1. Decompile APK: `apktool d cops_robbers_zerotier.apk`
2. Edit game code in dnSpy to point to server IP (172.29.16.227)
3. Rebuild APK: `apktool b cops_robbers_zerotier -o output.apk`
4. Sign APK: `jarsigner -sigalg SHA1withRSA -digestalg SHA1 -keystore debug.keystore -storepass android output.apk debugkey`
5. Install: `adb install -r output.apk`

## 🌐 Network Architecture

```
Windows PC (172.29.16.227)          Android Phone (172.29.116.209)
    [LAN Server]       <---------->       [Game Client]
    - UDP 5055/5056                    (ZeroTier VPN)
    - TCP 5057
```

**ZeroTier VPN** provides the network tunnel, eliminating Firebase/Photon API restrictions.

## 📊 Protocol Specifications

### Game Operations (UDP)
- **OpCode 0x01**: Player join/authentication
- **OpCode 0x02**: Player state update  
- **OpCode 0x03**: Room list broadcast
- **OpCode 0x04**: Game state synchronization

### Test Protocol (TCP 5057)
- Server sends: `[0xFF, 0xAA, 0xBB, counter_byte]`
- Used to verify client-server connectivity
- Android toast displays test byte reception

## 🔧 Development

### Building Modified APK

1. **Edit game code in dnSpy:**
   - Load `Assembly-CSharp.dll` from APK
   - Modify `CNRConnectMenu` class
   - Compile changes
   - **Save Module** (critical!)

2. **Rebuild APK:**
   ```powershell
   cd cops_robbers_zerotier
   apktool b . -o cops_robbers_zerotier_fixed.apk --use-aapt1
   jarsigner -sigalg SHA1withRSA -digestalg SHA1 -keystore debug.keystore -storepass android cops_robbers_zerotier_fixed.apk debugkey
   adb install -r cops_robbers_zerotier_fixed.apk
   ```

3. **Verify in logcat:**
   ```powershell
   adb logcat -d | findstr "CREATING HARDCODED FAKE ROOM IN AWAKE"
   ```

### Monitoring

- **Server console**: Real-time player connections, room events, protocol events
- **Android logcat**: Game debug logs, test byte reception, toast notifications
- **ZeroTier**: Network connectivity status and IP assignments

## 🛠️ Current Status

**Phase 2C - Test Byte Protocol Implementation** ✅

- ✅ LAN server broadcasting test bytes on TCP 5057
- ✅ Phone connected to server via ZeroTier (heartbeat confirmed)
- ✅ APK build/sign/install pipeline working
- 🟡 Game client test byte reception (code deployed, awaiting verification)
- ❌ Real room synchronization (next phase)

## 📝 Known Limitations

- **Photon SDK v3.0.2 Bug**: `PhotonNetwork.connected` never becomes true with custom servers
- **Workaround**: Direct TCP/UDP communication bypassing Photon APIs
- **APK Modification**: Game code must be manually edited via dnSpy; source code unavailable
- **ZeroTier Required**: VPN tunnel necessary for cross-network communication

## 🎯 Future Roadmap

- [ ] Real room list synchronization from server
- [ ] Player team assignment (Cop/Robber)
- [ ] In-game chat system
- [ ] Matchmaking algorithm
- [ ] Anti-cheat measures
- [ ] Server persistence layer (database)

## 📄 License

Proprietary - Game modification for personal/educational use only.

## 👤 Author

Developed as a networking solution for bypassing cloud-based game infrastructure limitations.

---

**Last Updated**: October 30, 2025  
**Server Version**: 1.0.0  
**Photon SDK**: v3.0.2  
**Platform**: Android + Windows PC
