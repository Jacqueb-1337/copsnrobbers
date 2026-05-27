<!doctype html>
<?php
define('CNR_ISSUES', 1);
require_once __DIR__ . '/db.php';

$token   = trim($_GET['token'] ?? '');
$message = '';
$isError = false;

if ($token) {
    $db   = db();
    $stmt = $db->prepare('SELECT id, issue_id FROM subscribers WHERE opt_out_token=:tok');
    $stmt->bindValue(':tok', $token);
    $row  = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if (!$row) {
        $message = 'This unsubscribe link is invalid or you have already unsubscribed.';
        $isError = true;
    } else {
        $del = $db->prepare('DELETE FROM subscribers WHERE opt_out_token=:tok');
        $del->bindValue(':tok', $token);
        $del->execute();
        $message = 'You have been unsubscribed and will no longer receive email updates for this issue.';
    }
} else {
    $isError = true;
    $message = 'Missing unsubscribe token.';
}
?>
<html lang="en">
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Unsubscribe — CNR Issues</title>
<link rel="stylesheet" href="style.css"/>
</head>
<body>
<div class="page" style="max-width:520px">
  <header class="site-header">
    <div class="logo">CNR</div>
    <div>
      <h1>Issue Tracker</h1>
      <div class="sub">Cops n Robbers Revival</div>
    </div>
  </header>
  <div class="card" style="text-align:center;padding:36px 24px">
    <div style="font-size:36px;margin-bottom:16px"><?= $isError ? '❌' : '✅' ?></div>
    <p style="font-size:15px;line-height:1.6;<?= $isError ? 'color:var(--danger)' : '' ?>">
      <?= htmlspecialchars($message) ?>
    </p>
    <a href="index.html" class="btn btn-ghost" style="margin-top:20px;display:inline-flex">&larr; Back to Issues</a>
  </div>
</div>
</body>
</html>
