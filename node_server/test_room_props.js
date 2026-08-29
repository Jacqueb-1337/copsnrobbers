const assert = require('assert');
const { RoomManager } = require('./masterServer');

const rooms = new RoomManager();
const room = rooms.getOrCreateRoom('raid-test');
const advertised = {
  map: 'FreeRun3_1',
  version: '3.0.9',
  mode: '0',
  cnrp: '1',
  cnrm: '4.2.51',
  cnra: '3.0.9',
  cnrr: '',
  cnrg: 'tdm',
  cnmi: 'raid',
  cnmn: 'Raid',
  cnmt: 'https://play.jacqueb.me/economy/uploads/thumbnails/raid.png',
  cnmty: 'dlc',
  cnrs: 'packed-settings',
  cnru: '',
  cnrh: '',
  cnrx: 'packed-resources'
};
const lobbyKeys = Object.keys(advertised);
// Match the real Photon CreateRoom wire shape: the lobby-property list is a
// reserved game property (byte key 250), not a separate operation parameter.
const createProps = Object.assign({}, advertised, { 250: lobbyKeys });

room.applyCreateProperties(createProps);
const full = room.getGamePropsHashtable();
const lobby = room.getLobbyPropsHashtable();

for (const key of lobbyKeys) {
  assert.strictEqual(full[key], advertised[key], `full room property lost: ${key}`);
  assert.strictEqual(lobby[key], advertised[key], `lobby room property lost: ${key}`);
}
assert.strictEqual(lobby.cnmi, 'raid');
assert.strictEqual(lobby.cnmn, 'Raid');
assert.strictEqual(lobby.cnmt, advertised.cnmt);
assert.strictEqual(lobby.cnmty, 'dlc');
assert.strictEqual(lobby.cnrs, 'packed-settings');
assert.strictEqual(lobby.cnrx, 'packed-resources');

console.log('room property preservation tests passed');
