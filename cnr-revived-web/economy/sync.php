<?php
// sync.php — bidirectional game-progression sync
// POST player_id, token, + flat progression fields (see below).
// The server merges client data with existing server data using a take-max strategy
// for numeric progress (level, exp, weapon upgrade levels) and a union strategy for
// unlock flags (skins, armors).  Equipped slots and current skin/armor use last-
// write-wins based on client_updated_at vs the server's stored updated_at.
// Returns the authoritative merged progression as flat fields.
//
// POST fields:
//   player_id, token          — auth
//   level                     — CharacterLevel (int)
//   exp                       — CharacterExp   (int)
//   wl_<WeaponName>           — upgrade level for each weapon (e.g. wl_AK=5)
//   su_Skin_<N>               — 1 if skin unlocked (e.g. su_Skin_1=1)
//   au_<ArmorName>            — 1 if armor unlocked (e.g. au_BodyArmor_1=1)
//   eq_1 … eq_8               — equipped weapon slots (string, blank = empty)
//   current_skin              — CurSettedSkinName
//   current_armor             — CurSettedArmorName
//   client_updated_at         — unix timestamp when client state last changed

require __DIR__ . '/_db.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') fail('POST only', 405);

$player     = require_auth();
$account_id = $player['id'];
$pdo        = db();

// ── Parse client progression ─────────────────────────────────────────────────
$client_level      = max(1,  (int)($_POST['level'] ?? 1));
$client_exp        = max(0,  (int)($_POST['exp']   ?? 0));
$client_updated_at = max(0, (int)($_POST['client_updated_at'] ?? 0));

// Weapon upgrade levels (wl_<name> params)
$client_wl = [];
foreach ($_POST as $k => $v) {
    if (preg_match('/^wl_([A-Za-z0-9_]{1,32})$/', $k, $m)) {
        $client_wl[$m[1]] = max(0, (int)$v);
    }
}

// Skin unlocks (su_Skin_<N>=1 params)
$client_skins = [];
foreach ($_POST as $k => $v) {
    if (preg_match('/^su_(Skin_\d+)$/', $k, $m) && (int)$v === 1) {
        $client_skins[] = $m[1];
    }
}

// Armor unlocks (au_<name>=1 params)
$client_armors = [];
foreach ($_POST as $k => $v) {
    if (preg_match('/^au_([A-Za-z0-9_]{1,32})$/', $k, $m) && (int)$v === 1) {
        $client_armors[] = $m[1];
    }
}

// Equipped slots eq_1 … eq_8
$client_equipped = [];
for ($i = 1; $i <= 8; $i++) {
    $client_equipped[] = trim($_POST['eq_' . $i] ?? '');
}

$client_c_skin  = trim($_POST['current_skin']  ?? '');
$client_c_armor = trim($_POST['current_armor'] ?? '');

// ── Load server progression ──────────────────────────────────────────────────
$stmt = $pdo->prepare("SELECT * FROM account_progression WHERE account_id=?");
$stmt->execute([$account_id]);
$srv = $stmt->fetch();

if (!$srv) {
    $pdo->prepare("INSERT INTO account_progression (account_id,updated_at) VALUES (?,0)")->execute([$account_id]);
    $srv = [
        'level' => 1, 'exp' => 0,
        'weapon_levels' => '{}', 'skin_unlocks' => '[]', 'armor_unlocks' => '[]',
        'equipped_slots' => '[]', 'current_skin' => 'Skin_1', 'current_armor' => '',
        'updated_at' => 0,
    ];
}

$srv_wl     = json_decode($srv['weapon_levels'] ?: '{}', true) ?: [];
$srv_skins  = json_decode($srv['skin_unlocks']  ?: '[]', true) ?: [];
$srv_armors = json_decode($srv['armor_unlocks'] ?: '[]', true) ?: [];
$srv_updated = (int)$srv['updated_at'];

// ── Merge ────────────────────────────────────────────────────────────────────
// Scalar progress: take-max
$merged_level = max((int)$srv['level'], $client_level);
$merged_exp   = max((int)$srv['exp'],   $client_exp);

// Weapon levels: per-weapon max
$merged_wl = $srv_wl;
foreach ($client_wl as $wname => $wlevel) {
    $merged_wl[$wname] = max((int)($merged_wl[$wname] ?? 0), $wlevel);
}

// Unlocks: union (skins and armors can only be gained, never lost)
$merged_skins  = array_values(array_unique(array_merge(
    array_filter($srv_skins,  'is_string'),
    array_filter($client_skins, 'is_string')
)));
$merged_armors = array_values(array_unique(array_merge(
    array_filter($srv_armors,  'is_string'),
    array_filter($client_armors, 'is_string')
)));

// Equipped / current: last-write-wins based on updated_at
if ($client_updated_at >= $srv_updated) {
    // Client has the newer equipped state
    $merged_equipped  = $client_equipped;
    $merged_c_skin    = $client_c_skin  ?: $srv['current_skin'];
    $merged_c_armor   = $client_c_armor;
    $new_updated_at   = $client_updated_at;
} else {
    // Server has the newer equipped state
    $merged_equipped  = json_decode($srv['equipped_slots'] ?: '[]', true) ?: [];
    $merged_c_skin    = $srv['current_skin'];
    $merged_c_armor   = $srv['current_armor'];
    $new_updated_at   = $srv_updated;
}

// Pad equipped to 8 slots
while (count($merged_equipped) < 8) $merged_equipped[] = '';

// ── Persist merged state ─────────────────────────────────────────────────────
$pdo->prepare("
    UPDATE account_progression
       SET level=?, exp=?, weapon_levels=?, skin_unlocks=?, armor_unlocks=?,
           equipped_slots=?, current_skin=?, current_armor=?, updated_at=?
     WHERE account_id=?
")->execute([
    $merged_level, $merged_exp,
    json_encode($merged_wl),
    json_encode($merged_skins),
    json_encode($merged_armors),
    json_encode($merged_equipped),
    $merged_c_skin, $merged_c_armor,
    $new_updated_at,
    $account_id,
]);

// ── Build flat response ───────────────────────────────────────────────────────
$resp = [
    'level'        => $merged_level,
    'exp'          => $merged_exp,
    'current_skin' => $merged_c_skin,
    'current_armor'=> $merged_c_armor,
    'updated_at'   => $new_updated_at,
];

foreach ($merged_wl as $wname => $wlevel) {
    if (preg_match('/^[A-Za-z0-9_]{1,32}$/', $wname)) {
        $resp['wl_' . $wname] = (int)$wlevel;
    }
}

foreach ($merged_skins as $s) {
    if (preg_match('/^Skin_\d+$/', $s)) $resp['su_' . $s] = 1;
}

foreach ($merged_armors as $a) {
    if (preg_match('/^[A-Za-z0-9_]{1,32}$/', $a)) $resp['au_' . $a] = 1;
}

for ($i = 0; $i < 8; $i++) {
    $resp['eq_' . ($i + 1)] = $merged_equipped[$i] ?? '';
}

ok($resp);
