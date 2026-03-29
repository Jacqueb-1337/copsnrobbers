<?php
// claim.php
// POST player_id (new device's ANDROID_ID), display_name, password, pin
// Verifies account credentials and LINKS the new device to the existing account.
// Unlike the legacy migration approach, the old device(s) remain valid —
// this enables true multi-device access from a single account.
// Hash scheme: bcrypt(password . pin) — device-independent.

require __DIR__ . '/_db.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') fail('POST only', 405);

$android_id   = trim($_POST['player_id']    ?? '');
$display_name = trim($_POST['display_name'] ?? '');
$pin          = trim($_POST['pin']          ?? '');
$password     = trim($_POST['password']     ?? '');

if ($android_id === '')                                fail('missing player_id');
if (!preg_match('/^[0-9a-f]{1,64}$/i', $android_id)) fail('invalid player_id');
if ($display_name === '')                              fail('missing display_name');
if (!preg_match('/^\d{4,8}$/', $pin))                 fail('pin must be 4-8 digits');
if (strlen($password) < 6)                            fail('password must be at least 6 characters');

$pdo = db();

// Find account by display_name with a set recovery credential
$stmt = $pdo->prepare("SELECT * FROM accounts WHERE display_name=? AND pin_hash IS NOT NULL");
$stmt->execute([$display_name]);
$candidates = $stmt->fetchAll();

$matches = [];
foreach ($candidates as $c) {
    if (password_verify($password . $pin, $c['pin_hash'])) {
        $matches[] = $c;
    }
}

if (count($matches) === 0) fail('no account found with that name and credentials');
if (count($matches) >   1) fail('multiple_match');

$account = $matches[0];

// Link (or re-link) the device — if already registered to a different account, move it.
$new_token = bin2hex(random_bytes(32));
$now       = time();

$pdo->beginTransaction();
try {
    $existing = $pdo->prepare("SELECT account_id FROM devices WHERE android_id=?");
    $existing->execute([$android_id]);
    $row = $existing->fetch();
    if ($row) {
        // Device exists — update its account link and token
        $pdo->prepare("UPDATE devices SET account_id=?,token=?,last_seen=? WHERE android_id=?")
            ->execute([$account['id'], $new_token, $now, $android_id]);
    } else {
        $pdo->prepare("INSERT INTO devices (android_id,account_id,token,last_seen) VALUES (?,?,?,?)")
            ->execute([$android_id, $account['id'], $new_token, $now]);
    }
    $pdo->prepare("UPDATE accounts SET last_seen=? WHERE id=?")->execute([$now, $account['id']]);
    $pdo->commit();
} catch (Exception $e) {
    $pdo->rollBack();
    fail('db error: ' . $e->getMessage(), 500);
}

// Return account data + server progression so the new device can bootstrap
$prog_stmt = $pdo->prepare("SELECT * FROM account_progression WHERE account_id=?");
$prog_stmt->execute([$account['id']]);
$prog = $prog_stmt->fetch();

$resp = [
    'token'        => $new_token,
    'coins'        => (int)$account['coins'],
    'gems'         => (int)$account['gems'],
    'display_name' => $account['display_name'],
];

if ($prog) {
    $wl = json_decode($prog['weapon_levels'] ?: '{}', true) ?: [];
    // Flatten weapon levels into resp as wl_<weapon>
    foreach ($wl as $wname => $wlevel) {
        if (preg_match('/^[A-Za-z0-9_]{1,32}$/', $wname)) {
            $resp['wl_' . $wname] = (int)$wlevel;
        }
    }
    $skins  = json_decode($prog['skin_unlocks']  ?: '[]', true) ?: [];
    foreach ($skins as $s) {
        if (preg_match('/^Skin_\d+$/', $s)) $resp['su_' . $s] = 1;
    }
    $armors = json_decode($prog['armor_unlocks'] ?: '[]', true) ?: [];
    foreach ($armors as $a) {
        if (preg_match('/^[A-Za-z0-9_]{1,32}$/', $a)) $resp['au_' . $a] = 1;
    }
    $equipped = json_decode($prog['equipped_slots'] ?: '[]', true) ?: [];
    for ($i = 0; $i < 8; $i++) {
        $resp['eq_' . ($i + 1)] = $equipped[$i] ?? '';
    }
    $resp['level']        = (int)$prog['level'];
    $resp['exp']          = (int)$prog['exp'];
    $resp['current_skin'] = $prog['current_skin'];
    $resp['current_armor']= $prog['current_armor'];
    $resp['prog_updated_at'] = (int)$prog['updated_at'];
}

ok($resp);

