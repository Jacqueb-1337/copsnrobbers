using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Manages game rooms: creation, joining, leaving, properties
    /// Tracks room state and player assignments
    /// </summary>
    public class GameRoomManager
    {
        private readonly Dictionary<string, GameRoom> _rooms;
        private int _nextRoomId = 1;

        public GameRoomManager()
        {
            _rooms = new Dictionary<string, GameRoom>();
        }

        /// <summary>
        /// Create a new game room
        /// </summary>
        public GameRoom? CreateGame(string roomName, int maxPlayers = GameConstants.MaxPlayersPerRoom)
        {
            // Check if room already exists
            if (_rooms.ContainsKey(roomName))
            {
                Console.WriteLine("⚠️  Room '{0}' already exists", roomName);
                return null;
            }

            try
            {
                var room = new GameRoom
                {
                    RoomName = roomName,
                    MaxPlayers = maxPlayers,
                    IsOpen = true,
                    IsVisible = true,
                    GameMode = "CopsNRobbers",
                    MapName = "level_01",
                    CreatedTime = DateTime.UtcNow,
                    MasterClientActorNumber = 0
                };

                _rooms[roomName] = room;
                Console.WriteLine("✅ Room '{0}' created - Max: {1} players", roomName, maxPlayers);
                return room;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error creating room: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get room by name
        /// </summary>
        public GameRoom? GetRoom(string roomName)
        {
            GameRoom? room;
            if (_rooms.TryGetValue(roomName, out room))
            {
                return room;
            }
            return null;
        }

        /// <summary>
        /// Add player to room
        /// </summary>
        public bool JoinGame(string roomName, GamePlayer player)
        {
            var room = GetRoom(roomName);
            if (room == null)
            {
                Console.WriteLine("⚠️  Room '{0}' not found", roomName);
                return false;
            }

            // Check if room is full
            if (room.Players.Count >= room.MaxPlayers)
            {
                Console.WriteLine("⚠️  Room '{0}' is full", roomName);
                return false;
            }

            // Check if room is open
            if (!room.IsOpen)
            {
                Console.WriteLine("⚠️  Room '{0}' is not open for joining", roomName);
                return false;
            }

            // Assign actor number (1-4)
            int nextActorNumber = 1;
            while (room.Players.Any(p => p.Value.ActorNumber == nextActorNumber) && nextActorNumber <= 4)
            {
                nextActorNumber++;
            }

            if (nextActorNumber > 4)
            {
                Console.WriteLine("⚠️  No more actor numbers available in room '{0}'", roomName);
                return false;
            }

            // Set master client if first player
            if (room.Players.Count == 0)
            {
                room.MasterClientActorNumber = nextActorNumber;
                Console.WriteLine("👑 Player assigned as master client");
            }

            // Add player to room
            player.ActorNumber = nextActorNumber;
            player.JoinedTime = DateTime.UtcNow;
            room.Players[nextActorNumber] = player;

            Console.WriteLine("✅ Player joined room '{0}' as Actor {1}", roomName, nextActorNumber);
            return true;
        }

        /// <summary>
        /// Remove player from room
        /// </summary>
        public bool LeaveGame(string roomName, int actorNumber)
        {
            var room = GetRoom(roomName);
            if (room == null)
            {
                return false;
            }

            if (!room.Players.ContainsKey(actorNumber))
            {
                return false;
            }

            var player = room.Players[actorNumber];
            room.Players.Remove(actorNumber);

            Console.WriteLine("🚪 Player {0} left room '{1}'", player.PlayerName, roomName);

            // Reassign master if master left
            if (room.MasterClientActorNumber == actorNumber)
            {
                if (room.Players.Count > 0)
                {
                    room.MasterClientActorNumber = room.Players.Keys.First();
                    Console.WriteLine("👑 Master client reassigned to Actor {0}", room.MasterClientActorNumber);
                }
                else
                {
                    room.MasterClientActorNumber = 0;
                }
            }

            // Delete room if empty
            if (room.Players.Count == 0)
            {
                _rooms.Remove(roomName);
                Console.WriteLine("🗑️  Empty room '{0}' deleted", roomName);
            }

            return true;
        }

        /// <summary>
        /// Get all active rooms
        /// </summary>
        public IEnumerable<GameRoom> GetAllRooms()
        {
            return _rooms.Values;
        }

        /// <summary>
        /// Find room with available slot
        /// </summary>
        public GameRoom? FindAvailableRoom()
        {
            return _rooms.Values
                .Where(r => r.IsOpen && r.Players.Count < r.MaxPlayers)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get room list for lobby (simplified - no Hashtable)
        /// </summary>
        public IEnumerable<GameRoom> GetVisibleRooms()
        {
            return _rooms.Values.Where(r => r.IsVisible);
        }

        /// <summary>
        /// Update room properties
        /// </summary>
        public void UpdateRoomProperties(string roomName, Hashtable properties)
        {
            var room = GetRoom(roomName);
            if (room == null)
                return;

            foreach (DictionaryEntry entry in properties)
            {
                byte key = (byte)entry.Key;
                object? value = entry.Value;

                // Simple key-based property update
                switch (key)
                {
                    case 1: // IsOpen
                        room.IsOpen = (bool)value;
                        break;
                    case 2: // IsVisible
                        room.IsVisible = (bool)value;
                        break;
                    case 3: // GameMode
                        room.GameMode = (string)value;
                        break;
                    case 4: // MapName
                        room.MapName = (string)value;
                        break;
                }
            }
        }

        /// <summary>
        /// Get player by room and actor number
        /// </summary>
        public GamePlayer? GetPlayer(string roomName, int actorNumber)
        {
            var room = GetRoom(roomName);
            if (room == null)
                return null;

            GamePlayer? player;
            if (room.Players.TryGetValue(actorNumber, out player))
            {
                return player;
            }
            return null;
        }

        /// <summary>
        /// Get all players in room
        /// </summary>
        public IEnumerable<GamePlayer> GetPlayersInRoom(string roomName)
        {
            var room = GetRoom(roomName);
            if (room == null)
                return new List<GamePlayer>();

            return room.Players.Values;
        }

        /// <summary>
        /// Get room count
        /// </summary>
        public int RoomCount => _rooms.Count;

        /// <summary>
        /// Get total players in all rooms
        /// </summary>
        public int TotalPlayerCount => _rooms.Values.Sum(r => r.Players.Count);

        /// <summary>
        /// Show all rooms (debug)
        /// </summary>
        public void PrintRoomList()
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════╗");
            Console.WriteLine("║          Active Game Rooms ({0})               ║", _rooms.Count);
            Console.WriteLine("╚═══════════════════════════════════════════════╝");

            if (_rooms.Count == 0)
            {
                Console.WriteLine("  (no rooms)");
                return;
            }

            foreach (var room in _rooms.Values)
            {
                Console.WriteLine("\n📌 {0}", room.RoomName);
                Console.WriteLine("   Players: {0}/{1}", room.Players.Count, room.MaxPlayers);
                Console.WriteLine("   Mode: {0} | Map: {1}", room.GameMode, room.MapName);
                Console.WriteLine("   Open: {0} | Visible: {1}", room.IsOpen ? "Yes" : "No", room.IsVisible ? "Yes" : "No");
                Console.WriteLine("   Master: Actor {0}", room.MasterClientActorNumber);

                if (room.Players.Count > 0)
                {
                    Console.WriteLine("   Players:");
                    foreach (var player in room.Players.Values.OrderBy(p => p.ActorNumber))
                    {
                        Console.WriteLine("     • {0} (Actor {1}) - Health: {2}", 
                            player.PlayerName, player.ActorNumber, player.Health);
                    }
                }
            }
            Console.WriteLine();
        }
    }
}
