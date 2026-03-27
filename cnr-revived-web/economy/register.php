<?php
// register.php
// POST player_id (ANDROID_ID), display_name, [token]
// Re-login if token valid; register fresh if new device.
// Returns: token, coins, gems, display_name (canonical from server), new (bool)
//
// Name sync: on re-login the server's canonical display_name is authoritative.
// The client must update its local name if the server returns a different value.
// To set a new display name the player must use a dedicated update endpoint;
// re-registering does NOT overwrite the server's stored name.

require __DIR__ . '/_db.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') fail('POST only', 405);

$android_id   = trim($_POST['player_id']   ?? '');
$display_name = substr(trim($_POST['display_name'] ?? 'Player'), 0, 32);
$token_in     = strtolower(trim($_POST['token'] ?? ''));

if ($android_id === '') fail('missing player_id');
if (!preg_match('/^[0-9a-f]{1,64}$/i', $android_id)) fail('invalid player_id');

$pdo = db();

// Look up device
$stmt = $pdo->prepare("
    SELECT d.token, d.account_id,
           a.display_name AS acct_name, a.coins, a.gems
      FROM devices  d
      JOIN accounts a ON a.id = d.account_id
     WHERE d.android_id = ?
");
$stmt->execute([$android_id]);
$device = $stmt->fetch();

if ($device) {
    // Known device: token must match
    if ($token_in !== '' && $token_in === $device['token']) {
        // Valid re-login — touch timestamps; server's name is authoritative
        $now = time();
        $pdo->prepare("UPDATE accounts SET last_seen=? WHERE id=?")->execute([$now, $device['account_id']]);
        $pdo->prepare("UPDATE devices  SET last_seen=? WHERE android_id=?")->execute([$now, $android_id]);
        ok([
            'token'        => $device['token'],
            'coins'        => (int)$device['coins'],
            'gems'         => (int)$device['gems'],
            'display_name' => $device['acct_name'],   // client should sync to this value
            'new'          => false,
        ]);
    }
    // Wrong / missing token
    fail('device already registered — use your stored token or claim.php to link account', 409);
}

// New device — create account + device
$token      = bin2hex(random_bytes(32));
$account_id = bin2hex(random_bytes(16));
$now        = time();

$pdo->beginTransaction();
try {
    $pdo->prepare("
        INSERT INTO accounts (id,display_name,pin_hash,coins,gems,registered_at,last_seen)
        VALUES (?,?,NULL,0,0,?,?)
    ")->execute([$account_id, $display_name, $now, $now]);

    $pdo->prepare("
        INSERT INTO devices (android_id,account_id,token,last_seen)
        VALUES (?,?,?,?)
    ")->execute([$android_id, $account_id, $token, $now]);

    $pdo->prepare("
        INSERT INTO account_progression (account_id,updated_at) VALUES (?,0)
    ")->execute([$account_id]);

    $pdo->commit();
} catch (Exception $e) {
    $pdo->rollBack();
    fail('registration error: ' . $e->getMessage(), 500);
}

ok(['token' => $token, 'coins' => 0, 'gems' => 0, 'display_name' => $display_name, 'new' => true]);

