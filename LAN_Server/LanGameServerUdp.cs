using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// LAN Game Server using raw UDP sockets
    /// No external dependencies - maximum compatibility
    /// </summary>
    public class LanGameServerUdp
    {
        private UdpClient? _udpServer;
        private readonly GameServerState _gameState;
        private OperationHandler? _operationHandler;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveTask;
        private readonly Dictionary<string, RemoteClient> _connectedClients; // IP:Port -> client info
        private int _nextActorNumber = 1;

        public bool IsRunning { get; private set; }
        public int Port { get; private set; }

        private class RemoteClient
        {
            public IPEndPoint EndPoint { get; set; }
            public int ActorNumber { get; set; }
            public string? PlayerName { get; set; }
            public DateTime LastHeartbeat { get; set; }

            public RemoteClient(IPEndPoint endPoint)
            {
                EndPoint = endPoint;
                ActorNumber = 0;
                PlayerName = null;
                LastHeartbeat = DateTime.UtcNow;
            }
        }

        public LanGameServerUdp(int port = GameConstants.GameServerPort)
        {
            Port = port;
            _gameState = new GameServerState();
            _connectedClients = new Dictionary<string, RemoteClient>();
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public bool Start()
        {
            try
            {
                _udpServer = new UdpClient(0, AddressFamily.InterNetwork);
                _udpServer.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
                _operationHandler = new OperationHandler(_gameState, this);
                _cancellationTokenSource = new CancellationTokenSource();

                // Start receive loop in background
                _receiveTask = Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token));

                IsRunning = true;
                Console.WriteLine("✅ LAN Server started on port {0}", Port);
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
            IsRunning = false;
            _cancellationTokenSource?.Cancel();
            
            if (_receiveTask != null)
            {
                try
                {
                    _receiveTask.Wait(2000);
                }
                catch { }
            }

            _udpServer?.Close();
            _udpServer?.Dispose();
            Console.WriteLine("✅ Server stopped");
        }

        /// <summary>
        /// Main receive loop - runs on background thread
        /// </summary>
        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && IsRunning)
            {
                try
                {
                    if (_udpServer == null)
                        break;

                    // Receive with 500ms timeout
                    _udpServer.Client.ReceiveTimeout = 500;
                    
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = _udpServer.Receive(ref remoteEndPoint);

                    // Log receipt
                    string clientKey = $"{remoteEndPoint.Address}:{remoteEndPoint.Port}";
                    Console.WriteLine("📨 Received {0} bytes from {1}", buffer.Length, clientKey);

                    // Track client
                    if (!_connectedClients.ContainsKey(clientKey))
                    {
                        _connectedClients[clientKey] = new RemoteClient(remoteEndPoint);
                        Console.WriteLine("🔗 New peer: {0}", clientKey);
                    }
                    else
                    {
                        _connectedClients[clientKey].LastHeartbeat = DateTime.UtcNow;
                    }

                    // Parse and handle message
                    var message = PhotonMessageParser.ParseMessage(buffer, buffer.Length);
                    if (message != null && _operationHandler != null)
                    {
                        _operationHandler.HandleOperation(message, remoteEndPoint);
                    }
                    else
                    {
                        Console.WriteLine("⚠️  Failed to parse message from {0}", clientKey);
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    // Timeout is expected - just continue
                    continue;
                }
                catch (Exception ex)
                {
                    if (IsRunning)
                    {
                        Console.WriteLine("❌ Error in receive loop: {0}", ex.Message);
                    }
                }

                await Task.Delay(10, cancellationToken);
            }
        }

        /// <summary>
        /// Send message to a client
        /// </summary>
        public void SendToClient(IPEndPoint endPoint, byte[] data)
        {
            try
            {
                if (_udpServer != null)
                {
                    _udpServer.Send(data, data.Length, endPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error sending to {0}: {1}", endPoint, ex.Message);
            }
        }

        /// <summary>
        /// Broadcast message to all clients
        /// </summary>
        public void BroadcastToAll(byte[] data, IPEndPoint? excludePeer = null)
        {
            foreach (var client in _connectedClients.Values)
            {
                if (excludePeer != null && client.EndPoint.Equals(excludePeer))
                    continue;

                SendToClient(client.EndPoint, data);
            }
        }

        /// <summary>
        /// Main update loop - can be called periodically from console
        /// </summary>
        public void Update()
        {
            if (!IsRunning)
                return;

            // Check for timeouts
            var now = DateTime.UtcNow;
            var timedOutClients = new List<string>();

            foreach (var kvp in _connectedClients)
            {
                var timeSinceUpdate = now - kvp.Value.LastHeartbeat;
                if (timeSinceUpdate.TotalMilliseconds > GameConstants.TimeoutMs)
                {
                    timedOutClients.Add(kvp.Key);
                }
            }

            // Remove timed out clients
            foreach (var clientKey in timedOutClients)
            {
                Console.WriteLine("⏱️  Client {0} timed out", clientKey);
                _connectedClients.Remove(clientKey);
                // TODO: Remove from games
            }
        }

        public int GetNextActorNumber()
        {
            if (_nextActorNumber > 4)
                _nextActorNumber = 1;
            return _nextActorNumber++;
        }

        public int ConnectedPeerCount => _connectedClients.Count;
    }
}
