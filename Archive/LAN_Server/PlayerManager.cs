using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Manages player connections and state
    /// Tracks peer IP:Port to player mappings and player sessions
    /// </summary>
    public class PlayerManager
    {
        private readonly Dictionary<string, GamePlayer> _playersByPeerId;  // IP:Port -> Player
        private readonly Dictionary<int, string> _playerRoomMap;           // ActorNumber -> RoomName
        private int _nextPlayerId = 1000;

        public PlayerManager()
        {
            _playersByPeerId = new Dictionary<string, GamePlayer>();
            _playerRoomMap = new Dictionary<int, string>();
        }

        /// <summary>
        /// Create or get player from peer
        /// </summary>
        public GamePlayer GetOrCreatePlayer(IPEndPoint endPoint, string? playerName = null)
        {
            string peerId = FormatPeerId(endPoint);

            GamePlayer? player;
            if (_playersByPeerId.TryGetValue(peerId, out player))
            {
                return player;
            }

            // Create new player
            player = new GamePlayer
            {
                PlayerName = playerName ?? $"Player_{_nextPlayerId}",
                IpAddress = endPoint.Address.ToString(),
                Port = endPoint.Port,
                Health = 100,
                IsAlive = true,
                TeamType = 0,
                Score = 0,
                JoinedTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow,
                PeerId = _nextPlayerId++
            };

            _playersByPeerId[peerId] = player;
            Console.WriteLine("✅ Player '{0}' created from {1}", player.PlayerName, peerId);
            return player;
        }

        /// <summary>
        /// Get player by peer endpoint
        /// </summary>
        public GamePlayer? GetPlayer(IPEndPoint endPoint)
        {
            string peerId = FormatPeerId(endPoint);
            GamePlayer? player;
            _playersByPeerId.TryGetValue(peerId, out player);
            return player;
        }

        /// <summary>
        /// Update player last heartbeat
        /// </summary>
        public void UpdatePlayerHeartbeat(IPEndPoint endPoint)
        {
            var player = GetPlayer(endPoint);
            if (player != null)
            {
                player.LastUpdateTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Remove player
        /// </summary>
        public void RemovePlayer(IPEndPoint endPoint)
        {
            string peerId = FormatPeerId(endPoint);
            GamePlayer? player;
            if (_playersByPeerId.TryGetValue(peerId, out player))
            {
                if (player.ActorNumber > 0)
                {
                    _playerRoomMap.Remove(player.ActorNumber);
                }
                _playersByPeerId.Remove(peerId);
                Console.WriteLine("🗑️  Player '{0}' removed", player.PlayerName);
            }
        }

        /// <summary>
        /// Track player in room
        /// </summary>
        public void PlayerJoinedRoom(GamePlayer player, string roomName)
        {
            if (player.ActorNumber > 0)
            {
                _playerRoomMap[player.ActorNumber] = roomName;
            }
        }

        /// <summary>
        /// Get room for player
        /// </summary>
        public string? GetPlayerRoom(GamePlayer player)
        {
            string? roomName;
            if (player.ActorNumber > 0 && _playerRoomMap.TryGetValue(player.ActorNumber, out roomName))
            {
                return roomName;
            }
            return null;
        }

        /// <summary>
        /// Update player properties
        /// </summary>
        public void UpdatePlayerProperties(GamePlayer player, Hashtable properties)
        {
            foreach (DictionaryEntry entry in properties)
            {
                if (entry.Key == null || entry.Value == null) continue;
                
                byte key = (byte)entry.Key;
                object value = entry.Value;

                try
                {
                    switch (key)
                    {
                        case (byte)0x01: // Health
                            if (value is int healthVal) player.Health = healthVal;
                            break;
                        case (byte)0x02: // IsAlive
                            if (value is bool aliveVal) player.IsAlive = aliveVal;
                            break;
                        case (byte)0x03: // TeamType
                            if (value is int teamVal) player.TeamType = teamVal;
                            break;
                        case (byte)0x04: // Score
                            if (value is int scoreVal) player.Score = scoreVal;
                            break;
                        case (byte)0x05: // Skin
                            if (value is int skinVal) player.Skin = skinVal;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️  Error updating player property {0}: {1}", key, ex.Message);
                }
            }

            player.LastUpdateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Check for timeout players
        /// </summary>
        public List<GamePlayer> GetTimeoutPlayers(int timeoutMs = GameConstants.TimeoutMs)
        {
            var now = DateTime.UtcNow;
            return _playersByPeerId.Values
                .Where(p => (now - p.LastUpdateTime).TotalMilliseconds > timeoutMs)
                .ToList();
        }

        /// <summary>
        /// Get all connected players
        /// </summary>
        public IEnumerable<GamePlayer> GetAllPlayers()
        {
            return _playersByPeerId.Values;
        }

        /// <summary>
        /// Get player count
        /// </summary>
        public int PlayerCount => _playersByPeerId.Count;

        /// <summary>
        /// Format peer ID as IP:Port
        /// </summary>
        private static string FormatPeerId(IPEndPoint endPoint)
        {
            return $"{endPoint.Address}:{endPoint.Port}";
        }

        /// <summary>
        /// Print player list (debug)
        /// </summary>
        public void PrintPlayerList()
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════╗");
            Console.WriteLine("║          Connected Players ({0})               ║", _playersByPeerId.Count);
            Console.WriteLine("╚═══════════════════════════════════════════════╝");

            if (_playersByPeerId.Count == 0)
            {
                Console.WriteLine("  (no players)");
                return;
            }

            foreach (var player in _playersByPeerId.Values.OrderBy(p => p.PeerId))
            {
                string? room = GetPlayerRoom(player);
                string roomInfo = room ?? "(no room)";
                
                Console.WriteLine("\n👤 {0}", player.PlayerName);
                Console.WriteLine("   Address: {0}:{1}", player.IpAddress, player.Port);
                Console.WriteLine("   Room: {0}", roomInfo);
                if (player.ActorNumber > 0)
                {
                    Console.WriteLine("   Actor: {0}", player.ActorNumber);
                }
                Console.WriteLine("   Health: {0} | Alive: {1} | Team: {2}", 
                    player.Health, player.IsAlive ? "Yes" : "No", player.TeamType == 0 ? "Robber" : "Cop");
            }
            Console.WriteLine();
        }
    }
}
