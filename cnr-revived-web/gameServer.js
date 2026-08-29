const net = require('net');
const {peekFrameSize, parseFrame, buildOpResponse, buildPingResponse, buildEvent, ParamKey, OpCode, intKeyHashtable} = require('./protocol');
const {RoomManager, RoomInfo} = require('./masterServer');

let sessionCounter = 1;

class PlayerSession {
  constructor(socket) {
    this.socket = socket;
    this.buffer = Buffer.alloc(0);
    this.id = sessionCounter++;
    this.isAuthenticated = false;
    this.userId = '';
    this.currentRoom = null;
    this.actorNr = 0;
    this.playerProps = {};
    this.connectTime = Date.now();
    socket.on('error', err => {
      // suppress per-socket errors in production
    });
  }
  appendAndExtract(data) {
    this.buffer = Buffer.concat([this.buffer, data]);
    const frames = [];
    while (true) {
      const sz = peekFrameSize(this.buffer);
      if (sz < 0) break;
      frames.push(this.buffer.slice(0, sz));
      this.buffer = this.buffer.slice(sz);
    }
    return frames;
  }
  send(buf) {
    this.socket.write(buf);
  }
}

function getPlayerNameFromProps(props) {
  if (!props || typeof props !== 'object') return '';
  const name = props[255] ?? props['255'];
  return name === undefined || name === null ? '' : String(name);
}

class GameServer {
  constructor(bindAddress, port, rooms) {
    this.bindAddress = bindAddress;
    this.port = port;
    this.rooms = rooms;
    this.sessions = new Set();
    // Sweep idle sessions every 10s; drop any that haven't sent data in 60s.
    // Keeps the Set clean and prevents accumulation of ghost sockets.
    this.idleTimer = setInterval(() => {
      const now = Date.now();
      for (const session of this.sessions) {
        if (now - session.lastDataTime > 60000) {
          console.log(`[Game] Session ${session.id} (${session.userId || 'anon'}) idle >60s, closing`);
          session.socket.destroy();
        }
      }
    }, 10000);
  }
  start() {
    this.server = net.createServer(sock => this.handleConnect(sock));
    this.server.listen(this.port, this.bindAddress, () => {
      console.log(`[Game] Listening on ${this.bindAddress}:${this.port}`);
    });
  }
  stop() {
    clearInterval(this.idleTimer);
    this.server.close();
  }
  handleConnect(sock) {
    // Keep NAT entries alive and prevent Nagle-induced latency spikes on small frames.
    sock.setKeepAlive(true, 10000);
    sock.setNoDelay(true);
    const session = new PlayerSession(sock);
    session.lastDataTime = Date.now();
    this.sessions.add(session);
    console.log(`[Game] Client ${session.id} connected from ${sock.remoteAddress}`);
    sock.on('data', data => {
      session.lastDataTime = Date.now();
      this.handleData(session, data);
    });
    sock.on('close', () => {
      console.log(`[Game] Client ${session.id} (${session.userId || 'anon'}) disconnected`);
      this.sessions.delete(session);
      this.handleLeave(session);
    });
  }
  handleData(session, data) {
    for (const frame of session.appendAndExtract(data)) {
      const msg = parseFrame(frame);
      if (!msg) continue;
      if (msg.IsInit) {
        session.send(Buffer.from([0xF3,0,0,0,9,0,0,0xF3,0x01]));
        continue;
      }
      if (msg.IsPing) {
        session.send(buildPingResponse(msg.PingFrame));
        continue;
      }
      if (msg.OpRequest) this.processOp(session, msg.OpRequest);
    }
  }
  processOp(session, req) {
    switch (req.OpCode) {
      case OpCode.Authenticate:
        this.handleAuth(session, req);
        break;
      case OpCode.CreateGame:
      case OpCode.JoinGame:
        this.handleJoin(session, req);
        break;
      case OpCode.RaiseEvent:
        this.handleRaiseEvent(session, req);
        break;
      case OpCode.SetProperties:
        this.handleSetProperties(session, req);
        break;
      case OpCode.GetProperties:
        // Ack and do nothing — client just re-syncs props
        session.send(buildOpResponse(OpCode.GetProperties, 0, null));
        break;
      case OpCode.Leave:
        this.handleLeave(session);
        break;
      default:
        console.log(`[Game] unhandled op 0x${req.OpCode.toString(16)}`);
        session.send(buildOpResponse(req.OpCode, 0, null));
        break;
    }
  }
  handleAuth(session, req) {
    if (req.Parameters && req.Parameters[ParamKey.userId]) {
      session.userId = String(req.Parameters[ParamKey.userId]);
    }
    session.isAuthenticated = true;
    console.log(`[Game] Session ${session.id} auth '${session.userId}'`);
    session.send(buildOpResponse(OpCode.Authenticate, 0, null));
  }
  handleJoin(session, req) {
    const roomName = (req.Parameters && req.Parameters[ParamKey.RoomName]) || Math.random().toString(36).slice(2,10);
    const room = this.rooms.getOrCreateRoom(roomName);
    // Preserve the complete room payload. CNRMod advertises custom map identity,
    // thumbnail, match settings and resource requirements through these properties.
    const gameProps = req.Parameters && req.Parameters[ParamKey.GameProps];
    // Photon embeds the lobby-property key list in GameProps[(byte)250].
    let lobbyKeys = req.Parameters && req.Parameters[ParamKey.PropsListedInLobby];
    if (!Array.isArray(lobbyKeys) && gameProps)
      lobbyKeys = gameProps[250] ?? gameProps['250'];
    room.applyCreateProperties(gameProps, lobbyKeys);
    const playerProps = req.Parameters && req.Parameters[ParamKey.PlayerProps];
    const advertisedName = getPlayerNameFromProps(playerProps);
    if (advertisedName) session.userId = advertisedName;
    if (playerProps && typeof playerProps === 'object') {
      session.playerProps = playerProps;
    }
    // assign actor
    const actor = room.assignActorNr();
    session.actorNr = actor;
    session.currentRoom = room;
    room.players.add(session);
    // build response: actorNr, playerProps, gameProps
    const actorProps = intKeyHashtable([...room.players].map(p => [p.actorNr, p.playerProps || {}]));
    const opParams = {
      [254]: actor,
      [249]: actorProps,
      [248]: room.getGamePropsHashtable()
    };
    session.send(buildOpResponse(req.OpCode, 0, opParams));
    // broadcast Event 255 (Join) to ALL players in the room
    const actorList = [...room.players].map(p=>p.actorNr);
    const joinEvt = buildEvent(255, {254: actor, 249: session.playerProps, 252: actorList});
    for (const p of room.players) {
      p.send(joinEvt);
    }
    console.log(`[Game] Room '${roomName}' now has ${room.playerCount} players`);
  }
  handleRaiseEvent(session, req) {
    const room = session.currentRoom;
    if (!room) return;
    const evtCode = req.Parameters[ParamKey.EventCode] || 0;
    const data    = req.Parameters[ParamKey.Data];
    // ReceiverGroup: 0=Others (default), 1=All, 2=MasterClient
    const receivers = req.Parameters[246] !== undefined ? req.Parameters[246] : 0;
    const evtParams = {[254]: session.actorNr};
    if (data !== undefined) evtParams[245] = data;
    const outEvt = buildEvent(evtCode, evtParams);
    const authorityActorNr = room.getAuthorityActorNr ? room.getAuthorityActorNr() : 0;
    for (const p of room.players) {
      if (receivers === 0 && p === session) continue;          // Others: skip sender
      if (receivers === 2 && p.actorNr !== authorityActorNr) continue; // MasterClient: lowest actor
      p.send(outEvt);
    }
  }
  handleSetProperties(session, req) {
    // Ack the op
    session.send(buildOpResponse(OpCode.SetProperties, 0, null));
    // Broadcast PropsChanged event 253 to room (mirrors C# HandleSetPropertiesAsync)
    const room = session.currentRoom;
    if (!room) return;
    const props = req.Parameters[251] ?? req.Parameters['251'];
    const advertisedName = getPlayerNameFromProps(props);
    if (advertisedName) {
      session.userId = advertisedName;
      session.playerProps = Object.assign({}, session.playerProps, props);
    }
    const evtParams = Object.assign({}, req.Parameters, {[254]: session.actorNr});
    const propsEvt = buildEvent(253, evtParams);
    for (const p of room.players) p.send(propsEvt);
  }
  handleLeave(session) {
    const room = session.currentRoom;
    if (!room) return;
    room.players.delete(session);
    // Ack to departing player (C# sends this before the leave broadcast)
    try { session.send(buildOpResponse(OpCode.Leave, 0, null)); } catch(_){}
    // Broadcast Leave event 254 to remaining players
    const leaveEvt = buildEvent(254, {[254]: session.actorNr});
    for (const p of room.players) {
      p.send(leaveEvt);
    }
    session.currentRoom = null;
    console.log(`[Game] Actor ${session.actorNr} left room '${room.name}' (${room.playerCount} remaining)`);
    if (room.playerCount === 0) this.rooms.deleteRoom(room.name);
  }
}

module.exports = { GameServer };
