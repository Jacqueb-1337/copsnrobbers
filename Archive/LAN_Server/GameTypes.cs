using System;
using System.Collections.Generic;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Represents a single player in the game
    /// </summary>
    public class GamePlayer
    {
        public int ActorNumber { get; set; }                 // 1-4 in room
        public string PlayerName { get; set; } = "Player";
        public int TeamType { get; set; }                    // 0=Robber, 1=Cop
        public int Skin { get; set; }
        public int Score { get; set; }
        public int Health { get; set; }
        public bool IsAlive { get; set; }
        public DateTime JoinedTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        // Networking
        public int PeerId { get; set; }                      // LiteNetLib peer ID
        public string IpAddress { get; set; } = "";
        public int Port { get; set; }

        public override string ToString() => $"Actor{ActorNumber}: {PlayerName} (Team:{TeamType}, Skin:{Skin})";
    }

    /// <summary>
    /// Represents a game room
    /// </summary>
    public class GameRoom
    {
        public string RoomName { get; set; } = "";
        public int MaxPlayers { get; set; } = 4;
        public int CurrentPlayerCount => Players.Count;
        public bool IsOpen { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public string GameMode { get; set; } = "CopsNRobbers";
        public string MapName { get; set; } = "level_01";
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        // Players in room (ActorNumber -> GamePlayer)
        public Dictionary<int, GamePlayer> Players { get; set; } = new();

        public int MasterClientActorNumber { get; set; } = 1;  // First player is master
        
        public bool IsFull => CurrentPlayerCount >= MaxPlayers;
        public bool IsEmpty => CurrentPlayerCount == 0;
        public bool CanJoin => IsOpen && !IsFull;

        public override string ToString() => $"{RoomName} ({CurrentPlayerCount}/{MaxPlayers}) [{GameMode}]";
    }

    /// <summary>
    /// Represents a queued message waiting to be sent
    /// </summary>
    public class GameMessage
    {
        public byte OperationCode { get; set; }
        public Dictionary<byte, object> Parameters { get; set; } = new();
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        
        public int ToActorNumber { get; set; } = -1;    // -1 = all in room
        public bool BufferThis { get; set; } = false;    // Photon AllBuffered flag
    }

    /// <summary>
    /// Represents a cached RPC call
    /// </summary>
    public class CachedRpcCall
    {
        public int SendingActorNumber { get; set; }
        public int RpcCode { get; set; }
        public object[] Parameters { get; set; } = Array.Empty<object>();
        public byte ReceiverGroup { get; set; }          // All, Others, MasterClient, etc.
        public int TargetActorNumber { get; set; } = -1;
        public DateTime CachedTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents the overall game server state
    /// </summary>
    public class GameServerState
    {
        public Dictionary<string, GameRoom> Rooms { get; set; } = new();
        public Dictionary<int, GamePlayer> AllPlayers { get; set; } = new();  // All players, all rooms
        
        public int TotalPlayerCount => AllPlayers.Count;
        public int TotalRoomCount => Rooms.Count;
        
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        // Cached RPCs - for new joiners
        public List<CachedRpcCall> CachedRpcs { get; set; } = new();
    }
}
