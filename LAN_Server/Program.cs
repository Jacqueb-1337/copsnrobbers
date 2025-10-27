using System;
using System.Threading;

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
            Console.WriteLine("  Uptime: {0}", DateTime.UtcNow);
            Console.WriteLine();
        }

        static void ShowRooms()
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("  GAME ROOMS");
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("  (TODO: Implement)");
            Console.WriteLine();
        }

        static void ShowPlayers()
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("  PLAYERS");
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("  (TODO: Implement)");
            Console.WriteLine();
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
    }
}
