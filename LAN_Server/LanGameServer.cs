using System;
using System.Net;
using LiteNetLib;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Main LAN server using LiteNetLib
    /// Handles: connections, disconnections, message routing
    /// </summary>
    public class LanGameServer : INetEventListener
    {
        private NetManager? _netManager;
        private readonly GameServerState _gameState;
        private OperationHandler? _operationHandler;
        private int _nextActorNumber = 1;

        public bool IsRunning { get; private set; }
        public int Port { get; private set; }

        public LanGameServer(int port = GameConstants.GameServerPort)
        {
            Port = port;
            _gameState = new GameServerState();
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public bool Start()
        {
            try
            {
                _netManager = new NetManager(this);
                _operationHandler = new OperationHandler(_gameState, this);
                
                if (!_netManager.Start(GameConstants.GameServerPort))
                {
                    Console.WriteLine("❌ Failed to start NetManager on port {0}", GameConstants.GameServerPort);
                    return false;
                }

                IsRunning = true;
                Console.WriteLine("✅ LAN Server started on port {0}", GameConstants.GameServerPort);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error starting server: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            if (_netManager != null)
            {
                _netManager.Stop();
                IsRunning = false;
                Console.WriteLine("✅ Server stopped");
            }
        }

        /// <summary>
        /// Main update loop - process events
        /// </summary>
        public void Update()
        {
            if (_netManager == null || !IsRunning)
                return;

            _netManager.PollEvents();

            // Check for timeouts
            var now = DateTime.UtcNow;
            foreach (var player in _gameState.AllPlayers.Values)
            {
                var timeSinceUpdate = now - player.LastUpdateTime;
                if (timeSinceUpdate.TotalMilliseconds > GameConstants.TimeoutMs)
                {
                    Console.WriteLine("⏱️  Player {0} timed out", player.PlayerName);
                    // TODO: Disconnect player
                }
            }
        }

        // ===== INetEventListener Implementation =====

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine("🔗 Peer connected: {0}:{1}", peer.Address, peer.Port);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine("🔌 Peer disconnected: {0} - Reason: {1}", peer.EndPoint, disconnectInfo.Reason);
            
            // Remove player from game
            var player = FindPlayerByPeerId(peer.Id);
            if (player != null)
            {
                RemovePlayerFromRoom(player);
            }
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine("❌ Network error at {0}: {1}", endPoint, socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            byte[] buffer = reader.GetRemainingBytes();
            Console.WriteLine("📨 Received {0} bytes from {1}", buffer.Length, peer.EndPoint);
            
            // Parse the message
            var message = PhotonMessageParser.ParseMessage(buffer, buffer.Length);
            if (message != null && _operationHandler != null)
            {
                _operationHandler.HandleOperation(message, peer);
            }
            else
            {
                Console.WriteLine("⚠️  Failed to parse message from {0}", peer.EndPoint);
            }
            
            reader.Recycle();
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Console.WriteLine("📡 Unconnected message from {0}: {1}", remoteEndPoint, messageType);
            reader.Recycle();
        }

        // ===== Helper Methods =====

        private GamePlayer? FindPlayerByPeerId(int peerId)
        {
            foreach (var player in _gameState.AllPlayers.Values)
            {
                if (player.PeerId == peerId)
                    return player;
            }
            return null;
        }

        private void RemovePlayerFromRoom(GamePlayer player)
        {
            // TODO: Find room, remove player, notify others, cleanup
        }

        public int GetNextActorNumber()
        {
            // Actor numbers are 1-4
            if (_nextActorNumber > 4)
                _nextActorNumber = 1;
            return _nextActorNumber++;
        }
    }
}
