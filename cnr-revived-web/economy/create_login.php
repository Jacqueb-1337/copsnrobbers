<?php
// Converts the current guest account into a username/password account.
// Existing guest auth remains valid.

require __DIR__ . '/_db.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') fail('POST only', 405);

$player = require_auth();
$pdo = db();
$username = strtolower(trim($_POST['username'] ?? ''));
$password = trim($_POST['password'] ?? '');

if (!preg_match('/^[a-z0-9_]{3,24}$/', $username)) fail('username must be 3-24 letters/numbers/_');
if (strlen($password) < 6) fail('password must be at least 6 characters');

$existing = $pdo->prepare("SELECT account_id FROM account_logins WHERE username=?");
$existing->execute([$username]);
if ($existing->fetch()) fail('username already exists', 409);

$already = $pdo->prepare("SELECT username FROM account_logins WHERE account_id=?");
$already->execute([$player['id']]);
if ($row = $already->fetch()) fail('this guest is already login account: ' . $row['username'], 409);

$hash = password_hash($password, PASSWORD_BCRYPT, ['cost' => 10]);
$now = time();
$pdo->prepare("
    INSERT INTO account_logins (username, account_id, password_hash, created_at, last_login)
    VALUES (?, ?, ?, ?, ?)
")->execute([$username, $player['id'], $hash, $now, $now]);

ok(['username' => $username, 'guest_converted' => true, 'stats' => account_stats($pdo, $player['id'])]);
