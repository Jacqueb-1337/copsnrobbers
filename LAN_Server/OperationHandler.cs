using System;
using System.Collections;
using LiteNetLib;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Handles incoming Photon operations and routes to appropriate handlers
    /// </summary>
    public class OperationHandler
    {
        private readonly GameServerState _gameState;
        private readonly LanGameServer _server;

        public OperationHandler(GameServerState gameState, LanGameServer server)
        {
            _gameState = gameState;
            _server = server;
        }

        /// <summary>
        /// Route operation to appropriate handler
        /// </summary>
        public void HandleOperation(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("📨 Operation 0x{0:X2} from {1}", message.OperationCode, peer.EndPoint);

            switch (message.OperationCode)
            {
                case PhotonProtocol.OperationCode.Authenticate:
                    HandleAuthenticate(message, peer);
                    break;

                case PhotonProtocol.OperationCode.JoinLobby:
                    HandleJoinLobby(message, peer);
                    break;

                case PhotonProtocol.OperationCode.LeaveLobby:
                    HandleLeaveLobby(message, peer);
                    break;

                case PhotonProtocol.OperationCode.CreateGame:
                    HandleCreateGame(message, peer);
                    break;

                case PhotonProtocol.OperationCode.JoinGame:
                    HandleJoinGame(message, peer);
                    break;

                case PhotonProtocol.OperationCode.JoinRandomGame:
                    HandleJoinRandomGame(message, peer);
                    break;

                case PhotonProtocol.OperationCode.Leave:
                    HandleLeave(message, peer);
                    break;

                case PhotonProtocol.OperationCode.RaiseEvent:
                    HandleRaiseEvent(message, peer);
                    break;

                case PhotonProtocol.OperationCode.SetProperties:
                    HandleSetProperties(message, peer);
                    break;

                case PhotonProtocol.OperationCode.GetProperties:
                    HandleGetProperties(message, peer);
                    break;

                default:
                    Console.WriteLine("⚠️  Unknown operation: 0x{0:X2}", message.OperationCode);
                    break;
            }
        }

        // ===== Operation Handlers =====

        private void HandleAuthenticate(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  🔐 Authenticate from {0}", peer.EndPoint);
            
            string? appId = message.GetString(PhotonProtocol.ParameterCode.ApplicationId);
            string? version = message.GetString(PhotonProtocol.ParameterCode.AppVersion);
            string? userId = message.GetString(PhotonProtocol.ParameterCode.UserId);
            
            Console.WriteLine("    AppID: {0}, Version: {1}, UserID: {2}", appId, version, userId);

            // TODO: Validate and send AppStats response
        }

        private void HandleJoinLobby(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  🏛️  Join Lobby from {0}", peer.EndPoint);
            
            // TODO: Send current game list to peer
        }

        private void HandleLeaveLobby(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  🏛️  Leave Lobby from {0}", peer.EndPoint);
            
            // TODO: Clean up lobby state for peer
        }

        private void HandleCreateGame(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  🎮 Create Game from {0}", peer.EndPoint);
            
            string? roomName = message.GetString(PhotonProtocol.ParameterCode.RoomName);
            Console.WriteLine("    Room: {0}", roomName);

            // TODO: Create room and add player
        }

        private void HandleJoinGame(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  🎮 Join Game from {0}", peer.EndPoint);
            
            string? roomName = message.GetString(PhotonProtocol.ParameterCode.RoomName);
            Console.WriteLine("    Room: {0}", roomName);

            // TODO: Add player to existing room
        }

        private void HandleJoinRandomGame(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  🎲 Join Random Game from {0}", peer.EndPoint);
            
            // TODO: Find random room and add player
        }

        private void HandleLeave(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  🚪 Leave from {0}", peer.EndPoint);
            
            // TODO: Remove player from room
        }

        private void HandleRaiseEvent(PhotonMessage message, NetPeer peer)
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

        private void HandleSetProperties(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  ⚙️  Set Properties from {0}", peer.EndPoint);
            
            var gameProps = message.GetHashtable(PhotonProtocol.ParameterCode.GameProperties);
            var playerProps = message.GetHashtable(PhotonProtocol.ParameterCode.PlayerProperties);
            
            // TODO: Update properties and broadcast change
        }

        private void HandleGetProperties(PhotonMessage message, NetPeer peer)
        {
            Console.WriteLine("  ⚙️  Get Properties from {0}", peer.EndPoint);
            
            // TODO: Send current properties to peer
        }
    }
}
