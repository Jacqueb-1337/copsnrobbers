<?php
// Validates username/password and returns guest-vs-login stats.
// This endpoint intentionally does not change device linkage.

require __DIR__ . '/_db.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') fail('POST only', 405);

$guest = require_auth();
$pdo = db();
$username = strtolower(trim($_POST['username'] ?? ''));
$password = trim($_POST['password'] ?? '');

if (!preg_match('/^[a-z0-9_]{3,24}$/', $username)) fail('invalid username');
if ($password === '') fail('missing password');

$stmt = $pdo->prepare("
    SELECT l.username, l.account_id, l.password_hash,
           a.id, a.display_name, a.coins, a.gems, a.registered_at, a.last_seen
      FROM account_logins l
      JOIN accounts a ON a.id = l.account_id
     WHERE l.username=?
");
$stmt->execute([$username]);
$login = $stmt->fetch();
if (!$login || !password_verify($password, $login['password_hash'])) fail('invalid username or password', 401);

$guest_stats = account_stats($pdo, $guest['id']);
$login_stats = account_stats($pdo, $login['account_id']);

ok([
    'username' => $username,
    'same_account' => $guest['id'] === $login['account_id'],
    'guest_stats' => $guest_stats,
    'login_stats' => $login_stats,
    'guest_coins' => $guest_stats['coins'] ?? 0,
    'guest_gems' => $guest_stats['gems'] ?? 0,
    'guest_owned_items' => $guest_stats['owned_items'] ?? 0,
    'guest_level' => $guest_stats['level'] ?? 1,
    'login_coins' => $login_stats['coins'] ?? 0,
    'login_gems' => $login_stats['gems'] ?? 0,
    'login_owned_items' => $login_stats['owned_items'] ?? 0,
    'login_level' => $login_stats['level'] ?? 1,
]);
