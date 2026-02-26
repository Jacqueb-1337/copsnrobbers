using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace CNRLanServer;

/// <summary>
/// Listens on port 5055 (Photon master port).
/// Handles: Authenticate → JoinLobby → GameList event → CreateGame / JoinGame redirect.
///
/// After auth the client receives a game-list event immediately.
/// CreateGame and JoinGame both return the game-server address (port 5056).
/// The client then disconnects from master and connects to the game server.
/// </summary>
public class MasterServer
{
    private readonly int           _port;
    private readonly string        _gameServerAddress; // e.g. "192.168.1.10:5056"
    private readonly RoomManager   _rooms;
    private readonly CancellationTokenSource _cts = new();

    private readonly List<PlayerSession> _sessions = new();
    private readonly object              _sessLock  = new();

    public MasterServer(string bindAddress, int port, string gameServerAddress, RoomManager rooms)
    {
        _port              = port;
        _gameServerAddress = gameServerAddress;
        _rooms             = rooms;
    }

    public async Task RunAsync()
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();
        Console.WriteLine($"[Master] Listening on port {_port}  →  redirecting to game server at {_gameServerAddress}");

        // Periodic room-list broadcast and prune
        _ = Task.Run(() => PeriodicBroadcastLoopAsync(_cts.Token));

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync(_cts.Token);
                var session = new PlayerSession(client);
                lock (_sessLock) _sessions.Add(session);
                _ = Task.Run(() => HandleClientAsync(session, _cts.Token));
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.WriteLine($"[Master] Accept error: {ex.Message}");
            }
        }
        listener.Stop();
    }

    public void Stop() => _cts.Cancel();

    // ─── Client connection loop ───────────────────────────────────────────

    private async Task HandleClientAsync(PlayerSession session, CancellationToken ct)
    {
        Console.WriteLine($"[Master] Client {session.Id} connected from {((IPEndPoint?)session.Client.Client.RemoteEndPoint)?.Address}");
        try
        {
            var buffer = new byte[4096];
            while (session.IsAlive && !ct.IsCancellationRequested)
            {
                int read = await session.Stream.ReadAsync(buffer, 0, buffer.Length, ct);
                if (read == 0) break;

                Console.WriteLine($"[Master] Client {session.Id} raw recv ({read}b): {BitConverter.ToString(buffer, 0, Math.Min(read, 48))}");

                foreach (byte[] frame in session.AppendAndExtract(buffer, read))
                {
                    Console.WriteLine($"[Master] Client {session.Id} frame ({frame.Length}b): {BitConverter.ToString(frame, 0, Math.Min(frame.Length, 32))}");
                    await ProcessFrameAsync(session, frame, ct);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"[Master] Client {session.Id} error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"[Master] Client {session.Id} disconnected");
            lock (_sessLock) _sessions.Remove(session);
            session.Dispose();
        }
    }

    private async Task ProcessFrameAsync(PlayerSession session, byte[] frame, CancellationToken ct)
    {
        IncomingMessage? msg = PhotonBinaryProtocol.ParseFrame(frame);
        if (msg == null)
        {
            Console.WriteLine($"[Master] Client {session.Id} ParseFrame returned null for frame: {BitConverter.ToString(frame, 0, Math.Min(frame.Length, 32))}");
            return;
        }

        if (msg.IsInit)
        {
            Console.WriteLine($"[Master] Client {session.Id} init — AppId: '{msg.AppId}'");
            Console.WriteLine($"[Master] Sending InitResponse: {BitConverter.ToString(PhotonBinaryProtocol.InitResponse)}");
            await session.SendAsync(PhotonBinaryProtocol.InitResponse, ct);
            return;
        }

        if (msg.IsPing)
        {
            // Reply with proper 9-byte ping response (TConnect.Run always reads 9-byte header)
            await session.SendAsync(PhotonBinaryProtocol.BuildPingResponse(msg.PingFrame), ct);
            return;
        }

        if (msg.OpRequest == null) return;

        switch (msg.OpRequest.OpCode)
        {
            case OpCode.Authenticate:
                await HandleAuthAsync(session, msg.OpRequest, ct);
                break;

            case OpCode.JoinLobby:
                await HandleJoinLobbyAsync(session, ct);
                break;

            case OpCode.LeaveLobby:
                // No-op from our side; client just stops getting room updates
                await SendOkResponseAsync(session, OpCode.LeaveLobby, ct);
                break;

            case OpCode.CreateGame:
                await HandleMasterCreateGameAsync(session, msg.OpRequest, ct);
                break;

            case OpCode.JoinGame:
                await HandleMasterJoinGameAsync(session, msg.OpRequest, ct);
                break;

            case OpCode.JoinRandom:
                await HandleMasterJoinRandomAsync(session, ct);
                break;

            default:
                Console.WriteLine($"[Master] Client {session.Id} unhandled op: 0x{msg.OpRequest.OpCode:X2}");
                break;
        }
    }

    // ─── Op handlers ─────────────────────────────────────────────────────

    private async Task HandleAuthAsync(PlayerSession session, OpRequestData req, CancellationToken ct)
    {
        // Extract userId / app version if present (informational only)
        if (req.Parameters.TryGetValue(ParamKey.userId, out var uid))
            session.UserId = uid?.ToString() ?? "";
        if (req.Parameters.TryGetValue(225, out var nick)) // UserId sometimes carries nickname
            session.Nickname = nick?.ToString() ?? "";

        Console.WriteLine($"[Master] Client {session.Id} authenticated as '{session.UserId}'");

        // ReturnCode 0 = success; no required params needed for auth on master
        var resp = PhotonBinaryProtocol.BuildOpResponse(OpCode.Authenticate, 0, null);
        await session.SendAsync(resp, ct);
        session.IsAuthenticated = true;
    }

    private async Task HandleJoinLobbyAsync(PlayerSession session, CancellationToken ct)
    {
        await SendOkResponseAsync(session, OpCode.JoinLobby, ct);

        // Immediately push the current room list
        await SendRoomListEventAsync(session, ct, fullList: true);
    }

    private async Task HandleMasterCreateGameAsync(PlayerSession session, OpRequestData req, CancellationToken ct)
    {
        string roomName = req.Parameters.TryGetValue(ParamKey.RoomName, out var rn)
            ? (rn?.ToString() ?? Guid.NewGuid().ToString("N")[..8])
            : Guid.NewGuid().ToString("N")[..8];

        // Pre-create the room so it's visible via list before the client reaches the game server
        RoomInfo room = _rooms.GetOrCreateRoom(roomName);
        ApplyRoomPropsFromRequest(room, req);

        Console.WriteLine($"[Master] Client {session.Id} creating room '{roomName}' → redirecting to game server");

        var p = new Dictionary<byte, object>
        {
            [ParamKey.GameServer] = _gameServerAddress     // game server "ip:port"
        };
        await session.SendAsync(PhotonBinaryProtocol.BuildOpResponse(OpCode.CreateGame, 0, p), ct);
    }

    private async Task HandleMasterJoinGameAsync(PlayerSession session, OpRequestData req, CancellationToken ct)
    {
        string? roomName = req.Parameters.TryGetValue(ParamKey.RoomName, out var rn) ? rn?.ToString() : null;

        if (string.IsNullOrEmpty(roomName))
        {
            await session.SendAsync(
                PhotonBinaryProtocol.BuildOpResponse(OpCode.JoinGame, 32765, null, "Room name missing"), ct);
            return;
        }

        // We don't need the room to exist on master already; the game server is authoritative
        Console.WriteLine($"[Master] Client {session.Id} joining room '{roomName}' → redirecting to game server");

        var p = new Dictionary<byte, object>
        {
            [ParamKey.GameServer] = _gameServerAddress
        };
        await session.SendAsync(PhotonBinaryProtocol.BuildOpResponse(OpCode.JoinGame, 0, p), ct);
    }

    private async Task HandleMasterJoinRandomAsync(PlayerSession session, CancellationToken ct)
    {
        var rooms = _rooms.GetVisibleRooms();
        if (rooms.Count == 0)
        {
            // ErrorCode 32760 = NoRandomMatchFound
            await session.SendAsync(
                PhotonBinaryProtocol.BuildOpResponse(OpCode.JoinRandom, 32760, null, "NoRandomMatchFound"), ct);
            return;
        }

        var room = rooms[0]; // pick first available
        var p = new Dictionary<byte, object>
        {
            [ParamKey.RoomName]   = room.Name,
            [ParamKey.GameServer] = _gameServerAddress
        };
        await session.SendAsync(PhotonBinaryProtocol.BuildOpResponse(OpCode.JoinRandom, 0, p), ct);
    }

    // ─── Room list events ─────────────────────────────────────────────────

    public async Task SendRoomListEventAsync(PlayerSession session, CancellationToken ct, bool fullList = false)
    {
        if (!session.IsAuthenticated) return;

        byte eventCode = fullList ? EvtCode.GameList : EvtCode.GameListUpd;
        var roomsHt = BuildRoomListHashtable(fullList);

        var p = new Dictionary<byte, object>
        {
            [222] = roomsHt   // ParameterCode.GameList = 222
        };
        await session.SendAsync(PhotonBinaryProtocol.BuildEvent(eventCode, p), ct);
    }

    private Hashtable BuildRoomListHashtable(bool fullList)
    {
        var ht = new Hashtable();
        foreach (var room in _rooms.GetVisibleRooms())
        {
            // Per-room Hashtable: standard byte keys + string custom props
            var roomHt = new Hashtable
            {
                [(byte)255] = room.MaxPlayers,    // maxPlayers (byte)
                [(byte)253] = room.IsOpen,        // isOpen
                [(byte)254] = room.IsVisible,     // isVisible
                [(byte)252] = (byte)room.PlayerCount, // playerCount (byte)
                [(byte)251] = false,              // removedFromList = false
                // Custom string-keyed props
                ["map"]     = room.Map,
                ["version"] = room.Version,
                ["mode"]    = room.Mode
            };
            ht[room.Name] = roomHt;
        }
        return ht;
    }

    private async Task PeriodicBroadcastLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct).ContinueWith(_ => { });
            _rooms.Prune();

            List<PlayerSession> snapshot;
            lock (_sessLock) snapshot = new List<PlayerSession>(_sessions);

            foreach (var s in snapshot)
            {
                try { await SendRoomListEventAsync(s, ct, fullList: false); }
                catch { /* ignore disconnected sessions */ }
            }
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static Task SendOkResponseAsync(PlayerSession session, byte opCode, CancellationToken ct) =>
        session.SendAsync(PhotonBinaryProtocol.BuildOpResponse(opCode, 0, null), ct);

    private static void ApplyRoomPropsFromRequest(RoomInfo room, OpRequestData req)
    {
        // Photon's OpCreateRoom encodes ALL room properties inside the GameProps Hashtable
        // (param 248), not at top-level params.  The structure sent by the client is:
        //
        //   req.Parameters[(byte)248] = Hashtable {
        //       (byte)255 = maxPlayers,
        //       (byte)253 = isOpen,
        //       (byte)254 = isVisible,
        //       (byte)250 = propsListedInLobby  (string[]),
        //       "version" = versionString,
        //       "map"     = mapName,
        //       "mode"    = modeString
        //   }
        //
        // Reading from top-level param (byte)239 / 237 / 236 finds nothing,
        // leaving maxPlayers=0 which causes the room-list filter (maxPlayers==playerCount)
        // to always hide the room.
        if (req.Parameters.TryGetValue(ParamKey.GameProps, out var gp) && gp is Hashtable gameProps)
        {
            if      (gameProps[byte.MaxValue] is byte   maxPb) room.MaxPlayers = maxPb;
            else if (gameProps[byte.MaxValue] is int    maxPi) room.MaxPlayers = (byte)maxPi;
            else if (gameProps[byte.MaxValue] is short  maxPs) room.MaxPlayers = (byte)maxPs;

            if (gameProps[(byte)253] is bool isOpenVal)  room.IsOpen    = isOpenVal;
            if (gameProps[(byte)254] is bool isVisVal)   room.IsVisible = isVisVal;

            if (gameProps["map"]     is string map)   room.Map     = map;
            if (gameProps["version"] is string ver)   room.Version = ver;
            if (gameProps["mode"]    is string mode)  room.Mode    = mode;
        }
    }
}
