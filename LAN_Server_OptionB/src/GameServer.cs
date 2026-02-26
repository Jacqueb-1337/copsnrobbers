using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace CNRLanServer;

/// <summary>
/// Listens on port 5056 (Photon game server port).
///
/// Client flow after redirecting from master:
///   1. TCP connect
///   2. Send init packet (48 bytes, 0xFB header)  →  we reply with InitResponse
///   3. Send Op 230 Authenticate                  →  we reply ok
///   4. Send Op 226/227 JoinGame / CreateGame     →  we reply with actorNr + props
///                                                    then send Event 255 (Join)
///   5. Send Op 253 RaiseEvent                    →  relay to other players in room
///   6. Send Op 254 Leave                         →  remove from room, send Event 254
///
/// op-response param keys (confirmed from GameEnteredOnGameServer):
///   (byte)254 = actorNr  (int)
///   (byte)249 = playerProps (Hashtable)
///   (byte)248 = gameProps   (Hashtable)
///
/// Event 255 (Join) param keys (confirmed from OnEvent handler):
///   (byte)254 = actorNr      (int)   — used to identify which player joined
///   (byte)249 = playerProps  (Hashtable)
///   (byte)252 = actorList    (int[])  — list of all actors after this join
/// </summary>
public class GameServer
{
    private readonly int           _port;
    private readonly RoomManager   _rooms;
    private readonly CancellationTokenSource _cts = new();

    private readonly List<PlayerSession> _sessions = new();
    private readonly object              _sessLock  = new();

    public GameServer(int port, RoomManager rooms)
    {
        _port  = port;
        _rooms = rooms;
    }

    public async Task RunAsync()
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();
        Console.WriteLine($"[Game]   Listening on port {_port}");

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
                Console.WriteLine($"[Game] Accept error: {ex.Message}");
            }
        }
        listener.Stop();
    }

    public void Stop() => _cts.Cancel();

    // ─── Client connection loop ───────────────────────────────────────────

    private async Task HandleClientAsync(PlayerSession session, CancellationToken ct)
    {
        Console.WriteLine($"[Game] Client {session.Id} connected from {((IPEndPoint?)session.Client.Client.RemoteEndPoint)?.Address}");
        try
        {
            var buffer = new byte[65536];
            while (session.IsAlive && !ct.IsCancellationRequested)
            {
                int read = await session.Stream.ReadAsync(buffer, 0, buffer.Length, ct);
                if (read == 0) break;

                foreach (byte[] frame in session.AppendAndExtract(buffer, read))
                {
                    await ProcessFrameAsync(session, frame, ct);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"[Game] Client {session.Id} error: {ex.Message}");
        }
        finally
        {
            await HandleLeaveAsync(session, ct);
            Console.WriteLine($"[Game] Client {session.Id} disconnected");
            lock (_sessLock) _sessions.Remove(session);
            session.Dispose();
        }
    }

    private async Task ProcessFrameAsync(PlayerSession session, byte[] frame, CancellationToken ct)
    {
        IncomingMessage? msg = PhotonBinaryProtocol.ParseFrame(frame);
        if (msg == null) return;

        if (msg.IsInit)
        {
            Console.WriteLine($"[Game] Client {session.Id} init — AppId: '{msg.AppId}'");
            await session.SendAsync(PhotonBinaryProtocol.InitResponse, ct);
            return;
        }

        if (msg.IsPing)
        {
            await session.SendAsync(PhotonBinaryProtocol.BuildPingResponse(msg.PingFrame), ct);
            return;
        }

        if (msg.OpRequest == null) return;

        switch (msg.OpRequest.OpCode)
        {
            case OpCode.Authenticate:
                await HandleAuthAsync(session, msg.OpRequest, ct);
                break;

            case OpCode.CreateGame:
                await HandleCreateGameAsync(session, msg.OpRequest, ct);
                break;

            case OpCode.JoinGame:
                await HandleJoinGameAsync(session, msg.OpRequest, ct);
                break;

            case OpCode.RaiseEvent:
                await HandleRaiseEventAsync(session, msg.OpRequest, ct);
                break;

            case OpCode.SetProperties:
                await HandleSetPropertiesAsync(session, msg.OpRequest, ct);
                break;

            case OpCode.Leave:
            case OpCode.GetProperties:
                await HandleLeaveAsync(session, ct);
                break;

            default:
                Console.WriteLine($"[Game] Client {session.Id} unhandled op: 0x{msg.OpRequest.OpCode:X2}");
                // Return a generic ok so the client doesn't hang
                await session.SendAsync(
                    PhotonBinaryProtocol.BuildOpResponse(msg.OpRequest.OpCode, 0, null), ct);
                break;
        }
    }

    // ─── Op handlers ─────────────────────────────────────────────────────

    private async Task HandleAuthAsync(PlayerSession session, OpRequestData req, CancellationToken ct)
    {
        if (req.Parameters.TryGetValue(ParamKey.userId, out var uid))
            session.UserId = uid?.ToString() ?? "";

        Console.WriteLine($"[Game] Client {session.Id} authenticated as '{session.UserId}'");
        session.IsAuthenticated = true;

        await session.SendAsync(PhotonBinaryProtocol.BuildOpResponse(OpCode.Authenticate, 0, null), ct);
    }

    private async Task HandleCreateGameAsync(PlayerSession session, OpRequestData req, CancellationToken ct)
    {
        string roomName = req.Parameters.TryGetValue(ParamKey.RoomName, out var rn)
            ? (rn?.ToString() ?? Guid.NewGuid().ToString("N")[..8])
            : Guid.NewGuid().ToString("N")[..8];

        RoomInfo room = _rooms.GetOrCreateRoom(roomName);
        ApplyRoomPropsFromRequest(room, req);

        await JoinRoomInternalAsync(session, room, OpCode.CreateGame, ct);
        Console.WriteLine($"[Game] Client {session.Id} created room '{roomName}' as actor {session.ActorNr}");
    }

    private async Task HandleJoinGameAsync(PlayerSession session, OpRequestData req, CancellationToken ct)
    {
        string? roomName = req.Parameters.TryGetValue(ParamKey.RoomName, out var rn) ? rn?.ToString() : null;

        if (string.IsNullOrEmpty(roomName))
        {
            await session.SendAsync(
                PhotonBinaryProtocol.BuildOpResponse(OpCode.JoinGame, 32765, null, "Room name missing"), ct);
            return;
        }

        RoomInfo room = _rooms.GetOrCreateRoom(roomName);

        // Apply props if client sent game props (create-if-not-exists scenario)
        ApplyRoomPropsFromRequest(room, req);

        await JoinRoomInternalAsync(session, room, OpCode.JoinGame, ct);
        Console.WriteLine($"[Game] Client {session.Id} joined room '{roomName}' as actor {session.ActorNr}");
    }

    /// <summary>Core join logic: assign actor, send op response, broadcast Event 255.</summary>
    private async Task JoinRoomInternalAsync(PlayerSession session, RoomInfo room, byte opCode, CancellationToken ct)
    {
        // Assign actor number and add to room
        int actorNr = room.AssignActorNr();
        session.ActorNr    = actorNr;
        session.CurrentRoom = room;
        lock (room.Players) room.Players.Add(session);

        // Build actor lists for the join event
        int[] actorList;
        lock (room.Players)
            actorList = room.Players.Select(p => p.ActorNr).ToArray();

        // ── 1. Send op response to the joining player ──────────────────────
        //   param 254 = actorNr
        //   param 249 = playerProps (empty Hashtable)
        //   param 248 = gameProps (room properties)
        var opParams = new Dictionary<byte, object>
        {
            [(byte)254] = actorNr,
            [(byte)249] = session.PlayerProps,          // empty Hashtable
            [(byte)248] = room.GetGamePropsHashtable()  // map, mode, version, maxPlayers, etc.
        };
        await session.SendAsync(PhotonBinaryProtocol.BuildOpResponse(opCode, 0, opParams), ct);

        // ── 2. Broadcast Event 255 (Join) to ALL players in the room ───────
        //   param 254 = the joining actor's number  (identifies "who joined")
        //   param 249 = their player props
        //   param 252 = int[] actorList (full current list)
        var joinEvtParams = new Dictionary<byte, object>
        {
            [(byte)254] = actorNr,
            [(byte)249] = session.PlayerProps,
            [(byte)252] = actorList
        };
        byte[] joinEvent = PhotonBinaryProtocol.BuildEvent(255, joinEvtParams);

        await BroadcastToRoomAsync(room, joinEvent, ct);

        Console.WriteLine($"[Game] Room '{room.Name}' now has {room.PlayerCount} player(s)");
    }

    private async Task HandleLeaveAsync(PlayerSession session, CancellationToken ct)
    {
        RoomInfo? room = session.CurrentRoom;
        if (room == null) return;

        session.CurrentRoom = null;
        lock (room.Players) room.Players.Remove(session);

        // Send Leave op ack to departing player (they may have already disconnected, ignore errors)
        try
        {
            await session.SendAsync(PhotonBinaryProtocol.BuildOpResponse(OpCode.Leave, 0, null), ct);
        }
        catch { /* ignore */ }

        // Broadcast Event 254 (Leave) to remaining players
        var leaveParams = new Dictionary<byte, object>
        {
            [(byte)254] = session.ActorNr
        };
        byte[] leaveEvent = PhotonBinaryProtocol.BuildEvent(254, leaveParams);
        await BroadcastToRoomAsync(room, leaveEvent, ct);

        Console.WriteLine($"[Game] Actor {session.ActorNr} left room '{room.Name}' ({room.PlayerCount} remaining)");

        // Clean up empty rooms
        if (room.PlayerCount == 0)
            _rooms.DeleteRoom(room.Name);
    }

    private async Task HandleRaiseEventAsync(PlayerSession session, OpRequestData req, CancellationToken ct)
    {
        if (session.CurrentRoom == null) return;

        // Extract event code and data
        byte evtCode = req.Parameters.TryGetValue(ParamKey.EventCode, out var ec) ? Convert.ToByte(ec) : (byte)0;
        object? evtData = req.Parameters.TryGetValue(ParamKey.Data, out var d) ? d : null;

        // Receiver group: 0=Others, 1=All, 2=MasterClient
        byte receivers = req.Parameters.TryGetValue(246, out var rv) ? Convert.ToByte(rv) : (byte)0;

        var evtParams = new Dictionary<byte, object>
        {
            [(byte)254] = session.ActorNr,   // sender actor nr
            [(byte)245] = evtData!,          // data
        };
        // Remove null data to avoid serialization error
        if (evtData == null) evtParams.Remove(245);

        byte[] outEvent = PhotonBinaryProtocol.BuildEvent(evtCode, evtParams);

        await RelayEventAsync(session.CurrentRoom, outEvent, session, receivers, ct);
    }

    private async Task HandleSetPropertiesAsync(PlayerSession session, OpRequestData req, CancellationToken ct)
    {
        // Ack the op
        await session.SendAsync(PhotonBinaryProtocol.BuildOpResponse(OpCode.SetProperties, 0, null), ct);

        // Broadcast property change event (253) to room
        if (session.CurrentRoom == null) return;

        var evtParams = new Dictionary<byte, object>(req.Parameters.ToDictionary(kv => kv.Key, kv => kv.Value ?? (object)""))
        {
            [(byte)254] = session.ActorNr
        };
        byte[] propsEvent = PhotonBinaryProtocol.BuildEvent(253, evtParams);
        await BroadcastToRoomAsync(session.CurrentRoom, propsEvent, ct);
    }

    // ─── Relay helpers ────────────────────────────────────────────────────

    /// <summary>Sends a pre-built event to all players in the room.</summary>
    private static async Task BroadcastToRoomAsync(RoomInfo room, byte[] frame, CancellationToken ct)
    {
        List<PlayerSession> targets;
        lock (room.Players)
            targets = new List<PlayerSession>(room.Players);

        foreach (var p in targets)
        {
            try { await p.SendAsync(frame, ct); }
            catch { /* disconnected — cleaned up elsewhere */ }
        }
    }

    /// <summary>Relays event respecting receiver group.</summary>
    private static async Task RelayEventAsync(RoomInfo room, byte[] frame, PlayerSession sender, byte receiverGroup, CancellationToken ct)
    {
        List<PlayerSession> targets;
        lock (room.Players)
            targets = new List<PlayerSession>(room.Players);

        foreach (var p in targets)
        {
            // ReceiverGroup 0 = Others (skip sender), 1 = All, 2 = MasterClient (first player)
            if (receiverGroup == 0 && p == sender) continue;
            if (receiverGroup == 2 && room.Players.Count > 0 && p != room.Players[0]) continue;

            try { await p.SendAsync(frame, ct); }
            catch { /* ignore */ }
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static void ApplyRoomPropsFromRequest(RoomInfo room, OpRequestData req)
    {
        // Photon's OpCreateRoom encodes ALL room properties inside the GameProps Hashtable
        // (param 248).  The structure from the client is:
        //   req.Parameters[(byte)248] = Hashtable {
        //       (byte)255 = maxPlayers,
        //       (byte)253 = isOpen,
        //       (byte)254 = isVisible,
        //       (byte)250 = propsListedInLobby (string[]),
        //       "version" = versionString,
        //       "map"     = mapName,
        //       "mode"    = modeString
        //   }
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
