using System;
using System.Collections.Generic;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Photon Protocol Constants - Extracted from v3.0.2
    /// Source: OperationCode.cs, EventCode.cs, ParameterCode.cs
    /// </summary>
    public static class PhotonProtocol
    {
        // ===== Operation Codes (Client → Server) =====
        public static class OperationCode
        {
            public const byte Authenticate = 230;       // 0xE6
            public const byte JoinLobby = 229;          // 0xE5
            public const byte LeaveLobby = 228;         // 0xE4
            public const byte CreateGame = 227;         // 0xE3
            public const byte JoinGame = 226;           // 0xE2
            public const byte JoinRandomGame = 225;     // 0xE1
            public const byte Leave = 254;              // 0xFE
            public const byte RaiseEvent = 253;         // 0xFD (RPC/State sync)
            public const byte SetProperties = 252;      // 0xFC
            public const byte GetProperties = 251;      // 0xFB
        }

        // ===== Event Codes (Server → Clients) =====
        public static class EventCode
        {
            public const byte GameList = 230;           // 0xE6
            public const byte GameListUpdate = 229;     // 0xE5
            public const byte QueueState = 228;         // 0xE4
            public const byte Match = 227;              // 0xE3
            public const byte AppStats = 226;           // 0xE2
            public const byte AzureNodeInfo = 210;      // 0xD2
            public const byte Join = 255;               // 0xFF (Player joined)
            public const byte Leave = 254;              // 0xFE (Player left)
            public const byte PropertiesChanged = 253;  // 0xFD (Props updated)
        }

        // ===== Parameter Codes (Field identifiers in messages) =====
        public static class ParameterCode
        {
            public const byte Address = 230;            // 0xE6
            public const byte PeerCount = 229;          // 0xE5
            public const byte GameCount = 228;          // 0xE4
            public const byte MasterPeerCount = 227;    // 0xE3
            public const byte UserId = 225;             // 0xE1
            public const byte ApplicationId = 224;      // 0xE0
            public const byte Position = 223;           // 0xDF
            public const byte GameList = 222;           // 0xDE
            public const byte Secret = 221;             // 0xDD
            public const byte AppVersion = 220;         // 0xDC
            public const byte AzureNodeInfo = 210;      // 0xD2
            public const byte AzureLocalNodeId = 209;   // 0xD1
            public const byte AzureMasterNodeId = 208;  // 0xD0
            public const byte RoomName = 255;           // 0xFF
            public const byte Broadcast = 250;          // 0xFA
            public const byte ActorList = 252;          // 0xFC
            public const byte ActorNr = 254;            // 0xFE
            public const byte PlayerProperties = 249;   // 0xF9
            public const byte CustomEventContent = 245; // 0xF5 (RPC data)
            public const byte Data = 245;               // 0xF5
            public const byte Code = 244;               // 0xF4 (Event/RPC code)
            public const byte GameProperties = 248;     // 0xF8
            public const byte Properties = 251;         // 0xFB
            public const byte TargetActorNr = 253;      // 0xFD
            public const byte ReceiverGroup = 246;      // 0xF6
            public const byte Cache = 247;              // 0xF7
            public const byte CleanupCacheOnLeave = 241;// 0xF1
        }

        // ===== Type Codes (Serialization) =====
        public static class TypeCode
        {
            public const byte Null = 0x00;
            public const byte Hashtable = 0x01;
            public const byte ByteArray = 0x02;
            public const byte String = 0x03;
            public const byte Byte = 0x04;
            public const byte Int = 0x05;
            public const byte Long = 0x06;
            public const byte Float = 0x07;
            public const byte Double = 0x08;
            public const byte Bool = 0x09;
            public const byte Object = 0x0A;
            public const byte Dictionary = 0x0B;
            public const byte Vector2 = 0x0C;
            public const byte Vector3 = 0x0D;
            public const byte Quaternion = 0x0E;
            public const byte PhotonPlayer = 0x0F;
            // Additional types derived from code analysis
            public const byte PhotonViewId = 0x10;
            public const byte Int16 = 0x11;
            public const byte Char = 0x12;
        }

        // ===== Receiver Groups (for RaiseEvent) =====
        public static class ReceiverGroup
        {
            public const byte All = 0;              // All players in room
            public const byte Others = 1;           // Everyone except sender
            public const byte MasterClient = 2;     // Only master
            public const byte Specified = 3;        // Target specific player
        }

        // ===== Delivery Methods (LiteNetLib) =====
        public enum DeliveryMethod
        {
            Unreliable = 0,         // No guarantee
            ReliableOrdered = 1,    // Must arrive in order
            ReliableUnordered = 2,  // Must arrive, no order guarantee
            SequencedReliable = 3   // Latest only, in order
        }

        // ===== Standard Property Keys =====
        public static class RoomPropertyKey
        {
            public const byte MaxPlayers = 248;     // 0xF8
            public const byte IsOpen = 248;         // 0xF8
            public const byte IsVisible = 248;      // 0xF8
            public const byte GameMode = 1;         // Custom: Game mode (Cops n Robbers, etc.)
            public const byte MapName = 2;          // Custom: Level name
            public const byte AllowSpectators = 3;  // Custom: Spectators enabled?
        }

        public static class PlayerPropertyKey
        {
            public const byte PlayerName = 255;     // 0xFF (Standard)
            public const byte TeamType = 1;         // Custom: 0=Robber, 1=Cop
            public const byte Skin = 2;             // Custom: Skin ID
            public const byte Score = 3;            // Custom: Player score
        }

        // ===== Serialization Sizes =====
        public static class SerializationSize
        {
            public const int Bool = 1;              // 1 byte
            public const int Byte = 1;              // 1 byte
            public const int Short = 2;             // 2 bytes
            public const int Int = 4;               // 4 bytes
            public const int Long = 8;              // 8 bytes
            public const int Float = 4;             // 4 bytes (IEEE 754)
            public const int Double = 8;            // 8 bytes
            public const int Vector2 = 8;           // 2 floats = 8 bytes
            public const int Vector3 = 12;          // 3 floats = 12 bytes
            public const int Quaternion = 16;       // 4 floats = 16 bytes
            public const int PhotonViewId = 4;      // Composite int
            public const int PhotonPlayerId = 4;    // Actor number
        }
    }

    /// <summary>
    /// Game-specific constants for Cops n Robbers
    /// </summary>
    public static class GameConstants
    {
        public const int MaxPlayersPerRoom = 4;
        public const int MinPlayersToStart = 2;
        
        // Team types
        public const int TeamRobber = 0;
        public const int TeamCop = 1;

        // Default ports
        public const int GameServerPort = 5055;     // TCP/UDP for gameplay
        public const int BroadcastPort = 5056;      // UDP for discovery beacon

        // Timing
        public const int StateUpdateFrequencyHz = 20;  // 20 times per second
        public const int StateUpdateIntervalMs = 50;   // 1000/20 = 50ms
        public const int BroadcastIntervalMs = 1000;   // 1 second
        public const int PingIntervalMs = 15000;       // 15 seconds
        public const int TimeoutMs = 30000;            // 30 seconds no ping = disconnect

        // Bandwidth estimates
        public const int BytesPerStateUpdate = 33;     // Position(12) + Rotation(16) + Health(4) + Status(1)
        public const double BytesPerSecond = BytesPerStateUpdate * StateUpdateFrequencyHz;  // ~660 bytes/sec per client
    }
}
