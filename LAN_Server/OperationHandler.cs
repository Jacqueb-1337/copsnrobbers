using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Handles incoming Photon operations and routes to appropriate handlers
    /// Supports both LiteNetLib and raw UDP based servers
    /// </summary>
    public class OperationHandler
    {
        private readonly GameServerState _gameState;
        private readonly LanGameServerUdp _server;
        private readonly GameRoomManager _roomManager;
        private readonly PlayerManager _playerManager;

        public OperationHandler(GameServerState gameState, LanGameServerUdp server, GameRoomManager roomManager, PlayerManager playerManager)
        {
            _gameState = gameState;
            _server = server;
            _roomManager = roomManager;
            _playerManager = playerManager;
        }

        /// <summary>
        /// Route operation to appropriate handler (IPEndPoint version for UDP)
        /// </summary>
        public void HandleOperation(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("📨 Operation 0x{0:X2} from {1}:{2}", message.OperationCode, endPoint.Address, endPoint.Port);

            switch (message.OperationCode)
            {
                case PhotonProtocol.OperationCode.Ping:
                    HandlePing(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.Authenticate:
                    HandleAuthenticate(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.JoinLobby:
                    HandleJoinLobby(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.LeaveLobby:
                    HandleLeaveLobby(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.CreateGame:
                    HandleCreateGame(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.JoinGame:
                    HandleJoinGame(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.JoinRandomGame:
                    HandleJoinRandomGame(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.Leave:
                    HandleLeave(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.RaiseEvent:
                    HandleRaiseEvent(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.SetProperties:
                    HandleSetProperties(message, endPoint);
                    break;

                case PhotonProtocol.OperationCode.GetProperties:
                    HandleGetProperties(message, endPoint);
                    break;

                default:
                    Console.WriteLine("⚠️  Unknown operation: 0x{0:X2}", message.OperationCode);
                    break;
            }
        }

        // ===== Operation Handlers =====

        private void HandlePing(PhotonMessage message, IPEndPoint endPoint)
        {
            // Ping is just a heartbeat - acknowledge it and update player's last activity
            string clientKey = $"{endPoint.Address}:{endPoint.Port}";
            
            // Get or create player - GetOrCreatePlayer returns existing player if they already registered
            var player = _playerManager.GetOrCreatePlayer(endPoint, $"Player_{endPoint.Port}");
            
            // Detect if this is a newly created player by checking if JoinedTime is very recent (within last 100ms)
            var timeSinceJoined = DateTime.UtcNow - player.JoinedTime;
            bool isNewPeer = timeSinceJoined.TotalMilliseconds < 100;
            
            if (isNewPeer)
            {
                Console.WriteLine("  🆕 NEW CONNECTION from {0}", clientKey);
                
                try
                {
                    // Send the simplest possible "connection OK" response
                    // Send back the exact same frame structure the client sent
                    // This should trigger the Photon SDK to recognize the connection
                    var pingResponse = new byte[] { 0x01 };
                    var framedPing = PhotonMessageSerializer.WrapInPhotonFrame(pingResponse);
                    _server.SendToClient(endPoint, framedPing);
                    Console.WriteLine("    ↔️  Sent Ping Echo - waiting for SDK state transition");
                    
                    // Also send an immediate GameList response to populate rooms (using proper format)
                    var visibleRooms = _roomManager.GetVisibleRooms().ToList();
                    var gameListState = new GameServerState
                    {
                        Rooms = visibleRooms.ToDictionary(r => r.RoomName, r => r)
                    };
                    var gameListResponse = PhotonMessageSerializer.CreateGameListResponse(gameListState);
                    var framedGameList = PhotonMessageSerializer.WrapInPhotonFrame(gameListResponse);
                    _server.SendToClient(endPoint, framedGameList);
                    Console.WriteLine("    📡 Sent GameList Response ({0} room(s))", gameListState.Rooms.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("    ❌ Handshake error: {0}", ex.Message);
                }
            }
            else
            {
                // Subsequent pings - just echo and update heartbeat
                _playerManager.UpdatePlayerHeartbeat(endPoint);
                Console.WriteLine("  💓 Heartbeat from {0} ({1})", clientKey, player.PlayerName);
                
                // Echo the ping
                var pingResponse = new byte[] { 0x01 };
                var framedPing = PhotonMessageSerializer.WrapInPhotonFrame(pingResponse);
                _server.SendToClient(endPoint, framedPing);
            }
        }

        private void HandleAuthenticate(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  🔐 Authenticate from {0}", endPoint);
            
            string? appId = message.GetString(PhotonProtocol.ParameterCode.ApplicationId);
            string? version = message.GetString(PhotonProtocol.ParameterCode.AppVersion);
            string? userId = message.GetString(PhotonProtocol.ParameterCode.UserId);
            
            Console.WriteLine("    AppID: {0}, Version: {1}, UserID: {2}", appId, version, userId);

            // Create player record
            var player = _playerManager.GetOrCreatePlayer(endPoint, userId ?? "Guest");
            
            // Send AppStats response
            var state = new GameServerState
            {
                Rooms = _roomManager.GetAllRooms().ToDictionary(r => r.RoomName, r => r),
                AllPlayers = _playerManager.GetAllPlayers().ToDictionary(p => p.PeerId, p => p)
            };
            var statsResponse = PhotonMessageSerializer.CreateAppStatsResponse(state);
            _server.SendToClient(endPoint, statsResponse);
        }

        private void HandleJoinLobby(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  🏛️  Join Lobby from {0}", endPoint);
            
            // Get player
            var player = _playerManager.GetPlayer(endPoint);
            if (player == null)
            {
                Console.WriteLine("    ⚠️  Player not authenticated");
                return;
            }

            // Send game list
            var visibleRooms = _roomManager.GetVisibleRooms().ToList();
            var state = new GameServerState
            {
                Rooms = visibleRooms.ToDictionary(r => r.RoomName, r => r)
            };
            var gameListResponse = PhotonMessageSerializer.CreateGameListResponse(state);
            _server.SendToClient(endPoint, gameListResponse);

            Console.WriteLine("    ✅ Sent game list ({0} rooms)", visibleRooms.Count);
        }

        private void HandleLeaveLobby(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  🏛️  Leave Lobby from {0}", endPoint);
            
            // Cleanup is automatic - player remains in memory but not in lobby state
        }

        private void HandleCreateGame(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  🎮 Create Game from {0}", endPoint);
            
            string? roomName = message.GetString(PhotonProtocol.ParameterCode.RoomName);
            if (string.IsNullOrEmpty(roomName))
            {
                Console.WriteLine("    ⚠️  No room name provided");
                return;
            }

            // Get player
            var player = _playerManager.GetPlayer(endPoint);
            if (player == null)
            {
                Console.WriteLine("    ⚠️  Player not authenticated");
                return;
            }

            // Create room
            var room = _roomManager.CreateGame(roomName);
            if (room == null)
            {
                Console.WriteLine("    ⚠️  Failed to create room");
                return;
            }

            // Add player to room
            if (_roomManager.JoinGame(roomName, player))
            {
                _playerManager.PlayerJoinedRoom(player, roomName);
                var roomAfterJoin = _roomManager.GetRoom(roomName);
                if (roomAfterJoin != null)
                {
                    Console.WriteLine("    ✅ Room created and player added as master");
                    
                    // Notify player of join
                    var joinEvent = PhotonMessageSerializer.CreateJoinEvent(roomAfterJoin, player);
                    _server.SendToClient(endPoint, joinEvent);
                }
            }
        }

        private void HandleJoinGame(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  🎮 Join Game from {0}", endPoint);
            
            string? roomName = message.GetString(PhotonProtocol.ParameterCode.RoomName);
            if (string.IsNullOrEmpty(roomName))
            {
                Console.WriteLine("    ⚠️  No room name provided");
                return;
            }

            // Get player
            var player = _playerManager.GetPlayer(endPoint);
            if (player == null)
            {
                Console.WriteLine("    ⚠️  Player not authenticated");
                return;
            }

            // Add player to room
            if (_roomManager.JoinGame(roomName, player))
            {
                _playerManager.PlayerJoinedRoom(player, roomName);
                Console.WriteLine("    ✅ Player added to room as Actor {0}", player.ActorNumber);
                
                var room = _roomManager.GetRoom(roomName);
                if (room != null)
                {
                    // Notify player of join
                    var joinEvent = PhotonMessageSerializer.CreateJoinEvent(room, player);
                    _server.SendToClient(endPoint, joinEvent);

                    // Broadcast to other players in room
                    BroadcastToRoom(roomName, joinEvent, excludePeer: endPoint);
                }
            }
        }

        private void HandleJoinRandomGame(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  🎲 Join Random Game from {0}", endPoint);
            
            // Get player
            var player = _playerManager.GetPlayer(endPoint);
            if (player == null)
            {
                Console.WriteLine("    ⚠️  Player not authenticated");
                return;
            }

            // Find available room
            var room = _roomManager.FindAvailableRoom();
            if (room == null)
            {
                Console.WriteLine("    ⚠️  No available rooms");
                return;
            }

            // Join that room
            if (_roomManager.JoinGame(room.RoomName, player))
            {
                _playerManager.PlayerJoinedRoom(player, room.RoomName);
                Console.WriteLine("    ✅ Player joined random room '{0}'", room.RoomName);
                
                var joinEvent = PhotonMessageSerializer.CreateJoinEvent(room, player);
                _server.SendToClient(endPoint, joinEvent);
                
                BroadcastToRoom(room.RoomName, joinEvent, excludePeer: endPoint);
            }
        }

        private void HandleLeave(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  🚪 Leave from {0}", endPoint);
            
            // Get player
            var player = _playerManager.GetPlayer(endPoint);
            if (player == null)
            {
                Console.WriteLine("    ⚠️  Player not found");
                return;
            }

            // Get player's room
            string? roomName = _playerManager.GetPlayerRoom(player);
            if (string.IsNullOrEmpty(roomName))
            {
                Console.WriteLine("    ⚠️  Player not in a room");
                return;
            }

            // Remove from room
            if (_roomManager.LeaveGame(roomName, player.ActorNumber))
            {
                Console.WriteLine("    ✅ Player removed from room");
                
                // Broadcast leave event to others in room
                var leaveEvent = PhotonMessageSerializer.CreateLeaveEvent(player.ActorNumber);
                BroadcastToRoom(roomName, leaveEvent);
            }
        }

        private void HandleRaiseEvent(PhotonMessage message, IPEndPoint endPoint)
        {
            // RaiseEvent can be:
            // 1. State synchronization (OnPhotonSerializeView data)
            // 2. RPC call (CustomEventContent with method ID)
            
            byte? eventCode = (byte?)message.GetParameter(PhotonProtocol.ParameterCode.Code);
            object? customContent = message.GetParameter(PhotonProtocol.ParameterCode.CustomEventContent);
            
            if (eventCode.HasValue && customContent != null)
            {
                Console.WriteLine("  📡 RaiseEvent - Code: {0}, Content: {1} bytes", eventCode, 
                    customContent is byte[] arr ? arr.Length : "?");
                // TODO: Route RPC call
            }
            else
            {
                Console.WriteLine("  📡 RaiseEvent - State Sync");
                // TODO: Route state update to other players
            }
        }

        private void HandleSetProperties(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  ⚙️  Set Properties from {0}", endPoint);
            
            var gameProps = message.GetHashtable(PhotonProtocol.ParameterCode.GameProperties);
            var playerProps = message.GetHashtable(PhotonProtocol.ParameterCode.PlayerProperties);
            
            // TODO: Update properties and broadcast change
        }

        private void HandleGetProperties(PhotonMessage message, IPEndPoint endPoint)
        {
            Console.WriteLine("  ⚙️  Get Properties from {0}", endPoint);
            
            // Get player's room
            var player = _playerManager.GetPlayer(endPoint);
            if (player == null)
                return;

            string? roomName = _playerManager.GetPlayerRoom(player);
            if (string.IsNullOrEmpty(roomName))
                return;

            var room = _roomManager.GetRoom(roomName);
            if (room == null)
                return;

            // TODO: Send room properties to peer
        }

        // ===== Helper Methods =====

        private void BroadcastToRoom(string roomName, byte[] messageData, IPEndPoint? excludePeer = null)
        {
            var room = _roomManager.GetRoom(roomName);
            if (room == null)
                return;

            foreach (var player in room.Players.Values)
            {
                var endPoint = new IPEndPoint(
                    System.Net.IPAddress.Parse(player.IpAddress),
                    player.Port
                );

                if (excludePeer != null && endPoint.Equals(excludePeer))
                    continue;

                _server.SendToClient(endPoint, messageData);
            }
        }

        private void BroadcastToAll(byte[] messageData, IPEndPoint? excludePeer = null)
        {
            foreach (var player in _playerManager.GetAllPlayers())
            {
                var endPoint = new IPEndPoint(
                    System.Net.IPAddress.Parse(player.IpAddress),
                    player.Port
                );

                if (excludePeer != null && endPoint.Equals(excludePeer))
                    continue;

                _server.SendToClient(endPoint, messageData);
            }
        }
    }
}
