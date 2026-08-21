<?php
// Completes a username/password login after login_start.php stats review.
// mode=merge merges current guest progress into the login account.
// mode=switch links this device to the login account without merging guest data.

require __DIR__ . '/_db.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') fail('POST only', 405);

$guest = require_auth();
$pdo = db();
$android_id = trim($_POST['player_id'] ?? '');
$username = strtolower(trim($_POST['username'] ?? ''));
$password = trim($_POST['password'] ?? '');
$mode = trim($_POST['mode'] ?? '');

if (!preg_match('/^[0-9a-f]{1,64}$/i', $android_id)) fail('invalid player_id', 401);
if (!preg_match('/^[a-z0-9_]{3,24}$/', $username)) fail('invalid username');
if ($mode !== 'merge' && $mode !== 'switch') fail('invalid mode');

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

$target = $login['account_id'];
$source = $guest['id'];
$new_token = bin2hex(random_bytes(32));
$now = time();

$pdo->beginTransaction();
try {
    if ($mode === 'merge' && $source !== $target) {
        merge_account_into($pdo, $source, $target);
    }

    $pdo->prepare("UPDATE devices SET account_id=?, token=?, last_seen=? WHERE android_id=?")
        ->execute([$target, $new_token, $now, $android_id]);
    $pdo->prepare("UPDATE accounts SET last_seen=? WHERE id=?")->execute([$now, $target]);
    $pdo->prepare("UPDATE account_logins SET last_login=? WHERE username=?")->execute([$now, $username]);

    if ($mode === 'merge' && $source !== $target) {
        $remaining = $pdo->prepare("SELECT COUNT(*) AS c FROM devices WHERE account_id=?");
        $remaining->execute([$source]);
        if ((int)$remaining->fetch()['c'] === 0) {
            $pdo->prepare("DELETE FROM account_progression WHERE account_id=?")->execute([$source]);
            $pdo->prepare("DELETE FROM accounts WHERE id=?")->execute([$source]);
        }
    }

    $pdo->commit();
} catch (Exception $e) {
    $pdo->rollBack();
    fail('login failed: ' . $e->getMessage(), 500);
}

$acct = $pdo->prepare("SELECT * FROM accounts WHERE id=?");
$acct->execute([$target]);
$account = $acct->fetch();
ok(array_merge(['username' => $username, 'mode' => $mode], response_account_payload($pdo, $account, $new_token)));
