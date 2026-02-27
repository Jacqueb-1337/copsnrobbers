// entrypoint for Node.js reimplementation
//
// Usage: node index.js <ip1> [ip2] [ip3] ...
//
// Pass one IP per network interface you want clients to reach the server on.
// Typically: node index.js 172.28.48.1 172.29.99.63
//   172.28.48.1  — WSA (Windows Subsystem for Android) virtual adapter
//   172.29.99.63 — ZeroTier adapter (physical phone)
//
// The master server binds to 0.0.0.0:5055 (all interfaces).
// The game server  binds to 0.0.0.0:5056 (all interfaces).
// When a client creates/joins a room the master server picks the advertised
// game-server address whose IP best matches the client's source address
// (longest-prefix / same subnet wins).

const { MasterServer, RoomManager } = require('./masterServer');
const { GameServer }               = require('./gameServer');
const webConsole                   = require('./webConsole');
const os                           = require('os');
const http                         = require('http');
const fs                           = require('fs');
const path                         = require('path');

// ── Parse / auto-detect IPs ──────────────────────────────────────────────────
const args = process.argv.slice(2);

// If no IPs supplied, auto-detect all non-loopback IPv4 addresses.
let advertisedIps = args.length > 0
  ? args
  : Object.values(os.networkInterfaces())
      .flat()
      .filter(n => n.family === 'IPv4' && !n.internal)
      .map(n => n.address);

if (advertisedIps.length === 0) advertisedIps = ['0.0.0.0'];

const gameServerAddresses = advertisedIps.map(ip => `${ip}:5056`);

// ── Startup banner ────────────────────────────────────────────────────────────
console.log('Node-based CNR LAN Server (multi-network)');
console.log(`  Master server    : 0.0.0.0:5055  (all interfaces)`);
console.log(`  Game server      : 0.0.0.0:5056  (all interfaces)`);
console.log(`  Maps HTTP server : 0.0.0.0:8080  (GET/PUT /maps/<name>.json)`);
console.log(`  Advertised game-server addresses:`);
for (const addr of gameServerAddresses) {
  console.log(`    ${addr}`);
}
console.log();
console.log('  Set server.cfg on each client:');
for (const ip of advertisedIps) {
  console.log(`    SERVER_IP=${ip}  SERVER_PORT=5055`);
}
console.log();

// ── Start servers ─────────────────────────────────────────────────────────────
const rooms  = new RoomManager();
const master = new MasterServer('0.0.0.0', 5055, gameServerAddresses, rooms);
master.start();

const game = new GameServer('0.0.0.0', 5056, rooms);
game.start();

// ── Web console ───────────────────────────────────────────────────────────────
webConsole.start({
  getStats: () => ({
    masterSessions: master.sessions.size,
    gameSessions:   game.sessions.size,
    rooms:          rooms.rooms.size,
    players:        [...rooms.rooms.values()].reduce((n, r) => n + r.playerCount, 0),
  }),
  getClients: () => ({
    master: [...master.sessions].map(s => ({
      id:            s.id,
      userId:        s.userId,
      ip:            s.socket.remoteAddress,
      connectTime:   s.connectTime,
      authenticated: s.isAuthenticated,
    })),
    game: [...game.sessions].map(s => ({
      id:            s.id,
      userId:        s.userId,
      ip:            s.socket.remoteAddress,
      connectTime:   s.connectTime,
      authenticated: s.isAuthenticated,
      room:          s.currentRoom ? s.currentRoom.name : null,
      actorNr:       s.actorNr,
    })),
  }),
});

// ── Maps HTTP server (port 8080) ──────────────────────────────────────────────
// Serves JSON files from ./maps/ with CORS so the Android WWW class can fetch them.
// PUT  /maps/<name>.json   — upload/replace a map (Content-Type: application/json)
// GET  /maps/<name>.json   — download a map
// GET  /maps/              — list all available maps as JSON array
const mapsDir = path.join(__dirname, 'maps');
if (!fs.existsSync(mapsDir)) fs.mkdirSync(mapsDir, { recursive: true });

const mapsServer = http.createServer((req, res) => {
  const cors = {
    'Access-Control-Allow-Origin':  '*',
    'Access-Control-Allow-Methods': 'GET, PUT, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type',
  };

  if (req.method === 'OPTIONS') {
    res.writeHead(204, cors);
    res.end();
    return;
  }

  const url = req.url.replace(/\?.*$/, '');   // strip query string

  // GET /maps/  → list
  if (req.method === 'GET' && (url === '/maps' || url === '/maps/')) {
    try {
      const files = fs.readdirSync(mapsDir).filter(f => f.endsWith('.json'));
      res.writeHead(200, { ...cors, 'Content-Type': 'application/json' });
      res.end(JSON.stringify(files));
    } catch (e) {
      res.writeHead(500, cors); res.end('error listing maps');
    }
    return;
  }

  // /maps/<filename>.json
  const m = url.match(/^\/maps\/([^/]+\.json)$/);
  if (!m) { res.writeHead(404, cors); res.end('not found'); return; }

  const filename = m[1].replace(/[^a-zA-Z0-9_.\-]/g, '_');   // sanitise
  const filepath  = path.join(mapsDir, filename);

  if (req.method === 'GET') {
    if (!fs.existsSync(filepath)) { res.writeHead(404, cors); res.end('map not found'); return; }
    const data = fs.readFileSync(filepath);
    res.writeHead(200, { ...cors, 'Content-Type': 'application/json', 'Content-Length': data.length });
    res.end(data);

  } else if (req.method === 'PUT') {
    const chunks = [];
    req.on('data', c => chunks.push(c));
    req.on('end', () => {
      try {
        const body = Buffer.concat(chunks).toString('utf8');
        JSON.parse(body);   // validate JSON
        fs.writeFileSync(filepath, body, 'utf8');
        console.log(`[maps] uploaded ${filename} (${body.length} bytes)`);
        res.writeHead(200, cors); res.end('ok');
      } catch (e) {
        res.writeHead(400, cors); res.end('invalid JSON');
      }
    });

  } else {
    res.writeHead(405, cors); res.end('method not allowed');
  }
});

mapsServer.listen(8080, '0.0.0.0', () => {
  console.log('  Maps HTTP server : 0.0.0.0:8080  (GET/PUT /maps/<name>.json)');
  console.log(`  Maps directory   : ${mapsDir}`);
  console.log();
});

module.exports = { master, game, rooms, mapsServer };
