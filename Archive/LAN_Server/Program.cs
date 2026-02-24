using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Main program - Console application entry point
    /// UDP-based server with no external dependencies
    /// </summary>
    class Program
    {
        static LanGameServerUdp? _server;

        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Cops n Robbers - LAN Server v1.0                ║");
            Console.WriteLine("║   Pure UDP + Custom Photon Protocol              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Run tests if requested
            if (args.Length > 0 && args[0] == "--test")
            {
                ProtocolTester.RunTests();
                return;
            }

            // Create server
            _server = new LanGameServerUdp(GameConstants.GameServerPort);

            // Setup exit handlers
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnConsoleCancel;

            // Start server
            if (!_server.Start())
            {
                Console.WriteLine("Failed to start server");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Server Configuration:");
            Console.WriteLine("  Port: {0}", GameConstants.GameServerPort);
            Console.WriteLine("  Max Players/Room: {0}", GameConstants.MaxPlayersPerRoom);
            Console.WriteLine("  State Update Rate: {0} Hz", GameConstants.StateUpdateFrequencyHz);
            Console.WriteLine("  Broadcast Port: {0}", GameConstants.BroadcastPort);
            Console.WriteLine();
            
            // Create a test room for clients to join
            var testRoom = _server.RoomManager.CreateGame("TestRoom", 4);
            Console.WriteLine("✅ Test room 'TestRoom' created (ready for players)");
            Console.WriteLine();
            
            // Start test broadcast task
            Task.Run(() => BroadcastTestBytesLoop());
            
            Console.WriteLine("Commands:");
            Console.WriteLine("  'q' or Ctrl+C - Quit");
            Console.WriteLine("  's' - Show status");
            Console.WriteLine("  'r' - Show rooms");
            Console.WriteLine("  'p' - Show players");
            Console.WriteLine("  't' - Run protocol tests");
            Console.WriteLine();

            // Main loop
            RunMainLoop();

            // Stop server
            _server.Stop();
            Console.WriteLine("\nGoodbye!");
        }

        static void RunMainLoop()
        {
            bool running = true;
            var frameTimer = new System.Diagnostics.Stopwatch();
            frameTimer.Start();

            while (running && _server != null && _server.IsRunning)
            {
                // Update server (process network events)
                _server.Update();

                // Handle console input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    switch (char.ToLower(key.KeyChar))
                    {
                        case 'q':
                            running = false;
                            break;
                        case 's':
                            ShowStatus();
                            break;
                        case 'r':
                            ShowRooms();
                            break;
                        case 'p':
                            ShowPlayers();
                            break;
                        case 't':
                            ProtocolTester.RunTests();
                            break;
                    }
                }

                // Limit to 60 FPS
                const int targetMs = 1000 / 60;  // ~16.67ms per frame
                var elapsed = frameTimer.ElapsedMilliseconds;
                if (elapsed < targetMs)
                {
                    Thread.Sleep((int)(targetMs - elapsed));
                }
                frameTimer.Restart();
            }
        }

        static void ShowStatus()
        {
            if (_server == null)
                return;

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("  SERVER STATUS");
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("  Running: {0}", _server.IsRunning ? "✅ YES" : "❌ NO");
            Console.WriteLine("  Port: {0}", GameConstants.GameServerPort);
            Console.WriteLine("  Rooms: {0}", _server.RoomManager.RoomCount);
            Console.WriteLine("  Players: {0}", _server.PlayerManager.PlayerCount);
            Console.WriteLine("  Uptime: {0}", DateTime.UtcNow);
            Console.WriteLine();
        }

        static void ShowRooms()
        {
            if (_server == null)
                return;

            _server.RoomManager.PrintRoomList();
        }

        static void ShowPlayers()
        {
            if (_server == null)
                return;

            _server.PlayerManager.PrintPlayerList();
        }

        static void OnProcessExit(object? sender, EventArgs e)
        {
            _server?.Stop();
        }

        static void OnConsoleCancel(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _server?.Stop();
        }

        static void BroadcastTestBytesLoop()
        {
            try
            {
                byte testCounter = 0;
                using (var tcpListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, 5057))
                {
                    tcpListener.Start();
                    Console.WriteLine("[TEST] TCP Test Server started on port 5057");
                    
                    while (_server != null && _server.IsRunning)
                    {
                        try
                        {
                            // Check if there's a pending connection
                            if (tcpListener.Pending())
                            {
                                var client = tcpListener.AcceptTcpClient();
                                Console.WriteLine("[TEST] Client connected from {0}", client.Client.RemoteEndPoint);
                                
                                // Send test bytes to this client
                                var stream = client.GetStream();
                                byte[] testBytes = new byte[] { 0xFF, 0xAA, 0xBB, testCounter };
                                stream.Write(testBytes, 0, testBytes.Length);
                                stream.Flush();
                                Console.WriteLine("[TEST] Sent test bytes to client: FF AA BB {0:X2}", testCounter);
                                
                                client.Close();
                            }
                            
                            testCounter++;
                            System.Threading.Thread.Sleep(5000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[TEST] Error: {0}", ex.Message);
                        }
                    }
                    
                    tcpListener.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in test server: {0}", ex.Message);
            }
        }
    }
}
