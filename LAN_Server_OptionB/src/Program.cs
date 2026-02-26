using CNRLanServer;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

// ─────────────────────────────────────────────────────────────────────────────
// CNR LAN Server — Option B (custom TCP Photon-protocol server)
//
// Usage:
//   dotnet run                              (auto-detects LAN IP)
//   dotnet run -- 192.168.1.10             (explicit LAN IP, all clients use same IP)
//   dotnet run -- 172.28.48.1 192.168.18.8 (WSA IP + WiFi IP for mixed devices)
//     arg[0] = IP printed for WSA server.cfg hint
//     arg[1] = advertised game-server IP (clients redirect here after CreateGame)
//              Use your WiFi IP so both phone and WSA can reach the game server.
//
// The server always binds to 0.0.0.0 so all network interfaces are covered.
// The companion mod on Android reads /storage/emulated/0/CNRMods/server.cfg
// for the master server IP.
// ─────────────────────────────────────────────────────────────────────────────

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║      CNR LAN Server — Option B               ║");
Console.WriteLine("║  Custom TCP Photon-protocol implementation   ║");
Console.WriteLine("╚══════════════════════════════════════════════╝");
Console.WriteLine();

// ── Determine this machine's LAN IP ──────────────────────────────────────────
string lanIp          = args.Length > 0 ? args[0] : DetectLanIp();
string advertisedIp   = args.Length > 1 ? args[1] : lanIp;   // game-server IP sent to clients
int masterPort        = 5055;
int gameServerPort    = 5056;
string gameServerAddress = $"{advertisedIp}:{gameServerPort}";

Console.WriteLine($"  LAN IP (WSA)     : {lanIp}");
Console.WriteLine($"  Advertised IP    : {advertisedIp}");
Console.WriteLine($"  Master server    : 0.0.0.0:{masterPort}  (all interfaces)");
Console.WriteLine($"  Game server      : 0.0.0.0:{gameServerPort}  (all interfaces)");
Console.WriteLine($"  Clients redirect : {gameServerAddress}");
Console.WriteLine();
Console.WriteLine($"  For WSA:   SERVER_IP={lanIp}  SERVER_PORT={masterPort}");
Console.WriteLine($"  For phone: SERVER_IP={advertisedIp}  SERVER_PORT={masterPort}");
Console.WriteLine();

// ── Shared room manager ───────────────────────────────────────────────────────
var rooms = new RoomManager();

// ── Create and run both servers concurrently ──────────────────────────────────
var master = new MasterServer(lanIp, masterPort, gameServerAddress, rooms);
var game   = new GameServer(gameServerPort, rooms);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\n[*] Shutting down...");
    master.Stop();
    game.Stop();
    cts.Cancel();
};

Console.WriteLine("[*] Starting servers... Press Ctrl+C to stop.");
Console.WriteLine();

await Task.WhenAll(
    master.RunAsync(),
    game.RunAsync()
);

Console.WriteLine("[*] Servers stopped.");

// ── Helper ────────────────────────────────────────────────────────────────────
static string DetectLanIp()
{
    // If we're on a ZeroTier network, prefer its assigned IPv4 (non-link-local).
    // This makes the server advertise the ZT address automatically, which keeps
    // mobile clients reachable when they hop onto the same virtual network.
    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (ni.OperationalStatus != OperationalStatus.Up) continue;
        // look for ZeroTier interface by name/description
        if (ni.Name.StartsWith("ZeroTier", StringComparison.OrdinalIgnoreCase) ||
            ni.Description.Contains("ZeroTier", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var ua in ni.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                string s = ua.Address.ToString();
                if (!s.StartsWith("169.254.")) // skip link-local
                    return s;
            }
        }
    }

    // Prefer the first IPv4 that starts with 192.168 or 10. (typical LAN ranges)
    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (ni.OperationalStatus != OperationalStatus.Up) continue;
        if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

        foreach (var ua in ni.GetIPProperties().UnicastAddresses)
        {
            if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;
            string s = ua.Address.ToString();
            if (s.StartsWith("192.168.") || s.StartsWith("10.") || s.StartsWith("172."))
                return s;
        }
    }

    // Fallback: use the UDP trick to find the outbound IP
    try
    {
        using var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sock.Connect("8.8.8.8", 80);
        return ((IPEndPoint)sock.LocalEndPoint!).Address.ToString();
    }
    catch { return "127.0.0.1"; }
}
