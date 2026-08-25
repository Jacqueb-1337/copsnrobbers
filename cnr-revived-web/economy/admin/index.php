<?php
// admin.php — full admin dashboard with session-based login
// To change the password, run:  php -r "echo password_hash('newpass', PASSWORD_DEFAULT);"
// and replace the ADMIN_PASS_HASH constant below.

define('ADMIN_PASS_HASH',
    '\$2y\$10\$placeholderREPLACETHISHASHxxxxxxxxxxxxxxxxxxxxxxxxxxxx'
);
// ^ Replace the above with your real hash. Default password is set via LOGIN below.
// If hash is still the placeholder, we fall back to checking ADMIN_PASS_PLAIN.
define('ADMIN_PASS_PLAIN', 'cnradmin');  // change this if you haven't set a hash yet

session_start();

// ── Pick up flash from session (PRG pattern) ─────────────────────────────────
$flash    = $_SESSION['flash']    ?? '';
$flash_ok = $_SESSION['flash_ok'] ?? true;
unset($_SESSION['flash'], $_SESSION['flash_ok']);

// ── Handle login / logout ────────────────────────────────────────────────────
if ($_SERVER['REQUEST_METHOD'] === 'POST' && ($_POST['act'] ?? '') === 'login') {
    $attempt = $_POST['password'] ?? '';
    $ok = false;
    if (strpos(ADMIN_PASS_HASH, 'placeholder') === false) {
        $ok = password_verify($attempt, ADMIN_PASS_HASH);
    } else {
        $ok = ($attempt === ADMIN_PASS_PLAIN);
    }
    if ($ok) {
        session_regenerate_id(true);
        $_SESSION['cnr_admin'] = true;
    } else {
        $login_error = 'Incorrect password.';
    }
}
if (($_GET['act'] ?? '') === 'logout') {
    session_destroy();
    header('Location: ./');
    exit;
}

// ── Show login page if not authed ────────────────────────────────────────────
if (empty($_SESSION['cnr_admin'])) {
    $login_error = $login_error ?? '';
    header('Content-Type: text/html; charset=utf-8');
?>
<!DOCTYPE html>
<html lang="en">
<head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>CNR Admin — Login</title>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{font-family:monospace;background:#0d1117;color:#c9d1d9;
  display:flex;align-items:center;justify-content:center;min-height:100vh}
.card{background:#161b22;border:1px solid #30363d;border-radius:8px;
  padding:32px 36px;width:320px}
h1{color:#58a6ff;font-size:18px;margin-bottom:24px;text-align:center}
label{display:block;color:#8b949e;font-size:12px;margin-bottom:6px}
input[type=password]{width:100%;background:#0d1117;border:1px solid #30363d;
  border-radius:4px;color:#e6edf3;padding:9px 12px;font-size:14px;margin-bottom:16px}
button{width:100%;background:#238636;border:1px solid #2ea043;border-radius:4px;
  color:#fff;padding:10px;font-size:14px;font-weight:bold;cursor:pointer}
button:hover{background:#2ea043}
.err{color:#f85149;font-size:13px;margin-bottom:14px;text-align:center}
</style>
</head>
<body>
<div class="card">
  <h1>CNR Economy Admin</h1>
  <?php if ($login_error): ?>
  <div class="err"><?= htmlspecialchars($login_error) ?></div>
  <?php endif; ?>
  <form method="POST">
    <input type="hidden" name="act" value="login">
    <label>Password</label>
    <input type="password" name="password" autofocus autocomplete="current-password">
    <button type="submit">Sign in</button>
  </form>
</div>
</body></html>
<?php
    exit;
}

// ── AJAX: compute MD5 hash of a URL (server-side fetch) ────────────────────
if ($_SERVER['REQUEST_METHOD'] === 'POST' && ($_POST['act'] ?? '') === 'fetch_hash') {
    header('Content-Type: application/json');
    $url = trim($_POST['url'] ?? '');
    $p   = parse_url($url);
    if (!filter_var($url, FILTER_VALIDATE_URL) || !in_array($p['scheme'] ?? '', ['http','https'])) {
        echo json_encode(['error' => 'Invalid URL']); exit;
    }
    $ctx  = stream_context_create(['http'=>['timeout'=>15],'https'=>['timeout'=>15]]);
    $data = @file_get_contents($url, false, $ctx);
    if ($data === false && function_exists('curl_init')) {
        $ch = curl_init($url);
        curl_setopt_array($ch, [CURLOPT_RETURNTRANSFER=>true,CURLOPT_TIMEOUT=>15,CURLOPT_FOLLOWLOCATION=>true,CURLOPT_SSL_VERIFYPEER=>false]);
        $data = curl_exec($ch); if ($data === false) $data = null; curl_close($ch);
    }
    if (!$data) { echo json_encode(['error' => 'Could not fetch URL']); exit; }
    echo json_encode(['hash' => md5($data)]);
    exit;
}

require __DIR__ . '/../_db.php';

$pdo = db();

// ── Helper: set flash and PRG-redirect ───────────────────────────────────────
function flash_redirect(string $msg, bool $ok = true, string $tab = ''): void {
    $_SESSION['flash']    = $msg;
    $_SESSION['flash_ok'] = $ok;
    $loc = './' . ($tab ? '?tab=' . $tab : '');
    header('Location: ' . $loc, true, 303);
    exit;
}

// ── Handle POST actions ───────────────────────────────────────────────────────
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $act = trim($_POST['act'] ?? '');

    if ($act === 'send_mail') {
        $pid     = trim($_POST['player_id'] ?? '');
        $subject = trim($_POST['subject']   ?? '');
        $body    = trim($_POST['body']      ?? '');
        $coins   = (int)($_POST['coins']    ?? 0);
        $gems    = (int)($_POST['gems']     ?? 0);
        $spins   = (int)($_POST['spins']    ?? 0);

        if ($subject === '') {
            flash_redirect('Subject is required.', false, 'mail');
        } elseif ($pid === '*') {
            // Global broadcast — insert a row for every registered player
            $all = $pdo->query("SELECT id FROM accounts")->fetchAll();
            $stmt = $pdo->prepare(
                "INSERT INTO player_mail (player_id, subject, body, coins, gems, spins, claimed, sent_at)
                 VALUES (?, ?, ?, ?, ?, ?, 0, ?)"
            );
            $now = time();
            foreach ($all as $p) {
                $stmt->execute([$p['id'], $subject, $body, max(0,$coins), max(0,$gems), max(0,$spins), $now]);
            }
            $flash = 'Global mail sent to ' . count($all) . ' players.';
            flash_redirect($flash, true, 'mail');
        } else {
            $row = $pdo->prepare("SELECT id FROM accounts WHERE id = ?");
            $row->execute([$pid]);
            if (!$row->fetch()) {
                flash_redirect('Player not found.', false, 'mail');
            } else {
                $pdo->prepare(
                    "INSERT INTO player_mail (player_id, subject, body, coins, gems, spins, claimed, sent_at)
                     VALUES (?, ?, ?, ?, ?, ?, 0, ?)"
                )->execute([$pid, $subject, $body, max(0, $coins), max(0, $gems), max(0, $spins), time()]);
                flash_redirect('Mail sent to player ' . htmlspecialchars($pid) . '.', true, 'mail');
            }
        }
    }

    if ($act === 'add_content') {
        $cid   = preg_replace('/[^a-z0-9_\-]/i', '_', trim($_POST['content_id']   ?? ''));
        $ctype = in_array($_POST['ctype'] ?? '', ['map','dlcmap','texture','data','skin','gun']) ? $_POST['ctype'] : 'map';
        $cname = trim($_POST['cname'] ?? '');
        $curl  = trim($_POST['curl']  ?? '');
        $base  = trim($_POST['base_scene']    ?? 'FreeRun3_1');
        $mat   = trim($_POST['material_name'] ?? '');
        $dkey  = trim($_POST['data_key']      ?? '');
        $sort  = (int)($_POST['sort_order']   ?? 0);
        $fhash = strtolower(preg_replace('/[^a-fA-F0-9]/', '', trim($_POST['file_hash'] ?? '')));
        if ($cid === '' || $curl === '') {
            flash_redirect('ID and URL are required.', false, 'content');
        } else {
            try {
                $pdo->prepare(
                    "INSERT INTO content_items (id,type,name,url,base_scene,material_name,data_key,sort_order,enabled,created_at,file_hash)
                     VALUES (?,?,?,?,?,?,?,?,1,?,?)"
                )->execute([$cid,$ctype,$cname,$curl,$base,$mat,$dkey,$sort,time(),$fhash]);
                // Handle optional thumbnail upload for maps
                if (($ctype === 'map' || $ctype === 'dlcmap') && isset($_FILES['thumb_file']) && $_FILES['thumb_file']['error'] === UPLOAD_ERR_OK) {
                    $file    = $_FILES['thumb_file'];
                    $allowed = ['image/jpeg' => 'jpg', 'image/png' => 'png', 'image/gif' => 'gif', 'image/webp' => 'webp'];
                    $mime    = mime_content_type($file['tmp_name']);
                    if (isset($allowed[$mime]) && $file['size'] <= 512 * 1024) {
                        $ext        = $allowed[$mime];
                        $upload_dir = __DIR__ . '/uploads/thumbnails/';
                        if (!is_dir($upload_dir)) mkdir($upload_dir, 0755, true);
                        $dest = $upload_dir . $cid . '.' . $ext;
                        if (move_uploaded_file($file['tmp_name'], $dest)) {
                            $thumb_url  = 'https://play.jacqueb.me/economy/uploads/thumbnails/' . $cid . '.' . $ext;
                            $thumb_hash = md5_file($dest);
                            $pdo->prepare("UPDATE content_items SET thumbnail_url = ?, thumbnail_hash = ? WHERE id = ?")
                                ->execute([$thumb_url, $thumb_hash, $cid]);
                        }
                    }
                }
                $flash = 'Content item "' . htmlspecialchars($cid) . '" added.';
                flash_redirect($flash, true, 'content');
            } catch (Exception $e) {
                flash_redirect('Error: ' . $e->getMessage(), false, 'content');
            }
        }
    }

    if ($act === 'toggle_content') {
        $cid = trim($_POST['content_id'] ?? '');
        $pdo->prepare("UPDATE content_items SET enabled = 1 - enabled WHERE id = ?")->execute([$cid]);
        flash_redirect('Toggled "' . htmlspecialchars($cid) . '".', true, 'content');
    }

    if ($act === 'delete_content') {
        $cid = trim($_POST['content_id'] ?? '');
        $pdo->prepare("DELETE FROM content_items WHERE id = ?")->execute([$cid]);
        flash_redirect('Deleted "' . htmlspecialchars($cid) . '".', true, 'content');
    }

    if ($act === 'reorder_content') {
        $cid  = trim($_POST['content_id']  ?? '');
        $sort = (int)($_POST['sort_order'] ?? 0);
        $pdo->prepare("UPDATE content_items SET sort_order = ? WHERE id = ?")->execute([$sort,$cid]);
        flash_redirect('Price for "' . htmlspecialchars($cid) . '" set to ' . $sort . ' coins.', true, 'content');
    }

    if ($act === 'update_hash') {
        $cid  = trim($_POST['content_id'] ?? '');
        $hash = strtolower(preg_replace('/[^a-fA-F0-9]/', '', trim($_POST['file_hash'] ?? '')));
        if ($cid === '') { flash_redirect('Missing ID.', false, 'content'); }
        else {
            $pdo->prepare("UPDATE content_items SET file_hash = ? WHERE id = ?")->execute([$hash, $cid]);
            flash_redirect('Hash updated for "' . htmlspecialchars($cid) . '".', true, 'content');
        }
    }

    if ($act === 'upload_thumbnail') {
        $cid = preg_replace('/[^a-z0-9_\-]/i', '_', trim($_POST['content_id'] ?? ''));
        if ($cid === '') {
            flash_redirect('Missing item ID.', false, 'content');
        } elseif (!isset($_FILES['thumb_file']) || $_FILES['thumb_file']['error'] !== UPLOAD_ERR_OK) {
            flash_redirect('Upload error (code ' . ($_FILES['thumb_file']['error'] ?? 'none') . ').', false, 'content');
        } else {
            $file    = $_FILES['thumb_file'];
            $allowed = ['image/jpeg' => 'jpg', 'image/png' => 'png', 'image/gif' => 'gif', 'image/webp' => 'webp'];
            $mime    = mime_content_type($file['tmp_name']);
            if (!isset($allowed[$mime]) || $file['size'] > 512 * 1024) {
                flash_redirect('Invalid file type or too large (max 512 KB, jpg/png/gif/webp).', false, 'content');
            } else {
                $ext        = $allowed[$mime];
                $upload_dir = __DIR__ . '/uploads/thumbnails/';
                if (!is_dir($upload_dir)) mkdir($upload_dir, 0755, true);
                // Remove old thumbnail with any extension
                foreach (['jpg','png','gif','webp'] as $e) {
                    $old = $upload_dir . $cid . '.' . $e;
                    if (file_exists($old)) unlink($old);
                }
                $dest = $upload_dir . $cid . '.' . $ext;
                if (move_uploaded_file($file['tmp_name'], $dest)) {
                    $thumb_url  = 'https://play.jacqueb.me/economy/uploads/thumbnails/' . $cid . '.' . $ext;
                    $thumb_hash = md5_file($dest);
                    $pdo->prepare("UPDATE content_items SET thumbnail_url = ?, thumbnail_hash = ? WHERE id = ?")->execute([$thumb_url, $thumb_hash, $cid]);
                    flash_redirect('Thumbnail uploaded for "' . htmlspecialchars($cid) . '".', true, 'content');
                } else {
                    flash_redirect('File move failed (check server permissions).', false, 'content');
                }
            }
        }
    }

    if ($act === 'grant') {
        $pid   = trim($_POST['player_id'] ?? '');
        $coins = (int)($_POST['coins']    ?? 0);
        $gems  = (int)($_POST['gems']     ?? 0);
        $mode  = $_POST['mode'] ?? 'add';   // add | set

        $row = $pdo->prepare("SELECT id, display_name FROM accounts WHERE id = ?");
        $row->execute([$pid]);
        $player = $row->fetch();
        if (!$player) {
            flash_redirect('Player not found.', false, 'players');
        } else {
            if ($mode === 'set') {
                $pdo->prepare("UPDATE accounts SET coins = ?, gems = ? WHERE id = ?")
                    ->execute([$coins, $gems, $pid]);
            } else {
                $pdo->prepare("UPDATE accounts SET coins = coins + ?, gems = gems + ? WHERE id = ?")
                    ->execute([$coins, $gems, $pid]);
            }
            $pdo->prepare(
                "INSERT INTO transactions (player_id, delta_coins, delta_gems, reason, created_at)
                 VALUES (?, ?, ?, 'admin_grant', ?)"
            )->execute([$pid, $coins, $gems, time()]);
            $flash = ($mode === 'set' ? 'Set' : 'Granted') . ' coins=' . $coins . ' gems=' . $gems
                   . ' to ' . htmlspecialchars($player['display_name']) . '.';
            flash_redirect($flash, true, 'players');
        }
    }
}

// ── AJAX: sync hash (fetch + save in one request) ─────────────────────────────
if ($_SERVER['REQUEST_METHOD'] === 'POST' && ($_POST['act'] ?? '') === 'sync_hash') {
    header('Content-Type: application/json');
    $cid   = trim($_POST['content_id'] ?? '');
    $url   = trim($_POST['url'] ?? '');
    $field = in_array($_POST['field'] ?? '', ['file_hash','thumbnail_hash']) ? $_POST['field'] : 'file_hash';
    if (!$cid || !filter_var($url, FILTER_VALIDATE_URL)) { echo json_encode(['error'=>'Bad params']); exit; }
    $data = false;
    if (function_exists('curl_init')) {
        $ch = curl_init($url);
        curl_setopt_array($ch,[CURLOPT_RETURNTRANSFER=>true,CURLOPT_TIMEOUT=>15,CURLOPT_FOLLOWLOCATION=>true,CURLOPT_SSL_VERIFYPEER=>false,CURLOPT_ENCODING=>'identity']);
        $data = curl_exec($ch); if ($data===false) $data=null; curl_close($ch);
    }
    if (!$data) {
        $ctx = stream_context_create(['http'=>['timeout'=>15,'header'=>"Accept-Encoding: identity\r\n"],'https'=>['timeout'=>15,'header'=>"Accept-Encoding: identity\r\n"]]);
        $data = @file_get_contents($url, false, $ctx);
    }
    if (!$data) { echo json_encode(['error'=>'Could not fetch URL']); exit; }
    $hash = md5($data);
    require_once __DIR__ . '/../_db.php';
    $pdo2 = db();
    $pdo2->prepare("UPDATE content_items SET {$field} = ? WHERE id = ?")->execute([$hash, $cid]);
    echo json_encode(['hash'=>$hash]);
    exit;
}

// ── Query data ────────────────────────────────────────────────────────────────
$total_players = (int)$pdo->query("SELECT COUNT(*) FROM accounts")->fetchColumn();
$total_tx      = (int)$pdo->query("SELECT COUNT(*) FROM transactions")->fetchColumn();
$total_mail    = (int)$pdo->query("SELECT COUNT(*) FROM player_mail")->fetchColumn();
$unread_mail   = (int)$pdo->query("SELECT COUNT(*) FROM player_mail WHERE claimed=0")->fetchColumn();
$total_content = (int)$pdo->query("SELECT COUNT(*) FROM content_items")->fetchColumn();

$content_items = $pdo->query(
    "SELECT id, type, name, url, thumbnail_url, file_hash, thumbnail_hash, material_name, data_key, sort_order, enabled
       FROM content_items ORDER BY type, sort_order ASC, created_at ASC"
)->fetchAll(PDO::FETCH_ASSOC);

$players = $pdo->query(
    "SELECT id, display_name, coins, gems, last_seen FROM accounts ORDER BY last_seen DESC LIMIT 200"
)->fetchAll();

$recent_mail = $pdo->query("
    SELECT m.id, m.sent_at, m.subject, m.coins AS m_coins, m.gems AS m_gems,
           m.spins AS m_spins, m.claimed, p.display_name
      FROM player_mail m JOIN accounts p ON p.id = m.player_id
     ORDER BY m.id DESC LIMIT 100
")->fetchAll();

$recent_tx = $pdo->query("
    SELECT t.created_at, p.display_name, t.delta_coins, t.delta_gems, t.reason
      FROM transactions t JOIN accounts p ON p.id = t.player_id
     ORDER BY t.id DESC LIMIT 100
")->fetchAll();

header('Content-Type: text/html; charset=utf-8');
?>
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>CNR Economy Admin</title>
<style>
/* ── Reset & vars ─────────────────────────────────────────── */
*{box-sizing:border-box;margin:0;padding:0}
:root{
  --bg:#0d1117;--surface:#161b22;--border:#30363d;--border2:#21262d;
  --text:#c9d1d9;--text2:#8b949e;--hi:#e6edf3;
  --blue:#58a6ff;--green:#3fb950;--red:#f85149;--yellow:#e3a53a;
}
body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;background:var(--bg);color:var(--text);font-size:13px;min-height:100vh}
/* ── Layout ────────────────────────────────────────────────── */
.topbar{display:flex;align-items:center;gap:12px;background:var(--surface);border-bottom:1px solid var(--border);padding:0 20px;height:46px;position:sticky;top:0;z-index:200}
.topbar-title{font-weight:700;font-size:14px;color:var(--blue);flex:1}
.topbar a{color:var(--text2);font-size:12px;text-decoration:none}
.topbar a:hover{color:var(--text)}
.layout{display:flex;height:calc(100vh - 46px)}
.sidebar{width:172px;flex-shrink:0;border-right:1px solid var(--border);padding:10px 0;overflow-y:auto;background:var(--bg)}
.nav{display:flex;flex-direction:column;gap:2px;padding:0 8px}
.nav-btn{display:flex;align-items:center;gap:8px;padding:8px 10px;border-radius:6px;cursor:pointer;color:var(--text2);font-size:13px;user-select:none;transition:all .1s;border:none;background:none;width:100%;text-align:left}
.nav-btn:hover{background:var(--surface);color:var(--text)}
.nav-btn.active{background:rgba(88,166,255,.1);color:var(--blue);font-weight:600}
.nav-btn .badge{background:rgba(255,255,255,.06);border-radius:20px;padding:1px 7px;font-size:10px;margin-left:auto;color:var(--text2)}
.nav-btn.active .badge{background:rgba(88,166,255,.15);color:var(--blue)}
.main{flex:1;overflow-y:auto;padding:20px 24px}
/* ── Panes ─────────────────────────────────────────────────── */
.pane{display:none}.pane.active{display:block}
/* ── Stats ─────────────────────────────────────────────────── */
.stats{display:flex;gap:10px;flex-wrap:wrap;margin-bottom:20px}
.stat{background:var(--surface);border:1px solid var(--border);border-radius:8px;padding:10px 16px;min-width:100px}
.stat span{display:block;color:var(--text2);font-size:10px;text-transform:uppercase;letter-spacing:.05em;margin-bottom:3px}
.stat strong{color:var(--hi);font-size:20px;font-weight:700}
/* ── Flash ─────────────────────────────────────────────────── */
.flash{padding:10px 14px;border-radius:6px;margin-bottom:16px;font-size:13px;font-weight:500}
.flash.ok{background:#0d2a14;border:1px solid var(--green);color:var(--green)}
.flash.err{background:#2d0d0d;border:1px solid var(--red);color:var(--red)}
/* ── Panel (form containers) ───────────────────────────────── */
.panel{background:var(--surface);border:1px solid var(--border);border-radius:8px;padding:16px 20px;margin-bottom:18px}
.panel-title{font-size:13px;font-weight:600;color:var(--hi);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border2)}
/* ── Form grid ─────────────────────────────────────────────── */
.fg{display:grid;grid-template-columns:130px 1fr;gap:8px 12px;align-items:center;max-width:600px}
.fg label{color:var(--text2);font-size:12px;text-align:right;line-height:1.3}
.fg .full{grid-column:1/-1}
.fg .actions{grid-column:2}
input[type=text],input[type=url],input[type=number],input[type=email],input[type=search],select,textarea{
  background:var(--bg);border:1px solid var(--border);border-radius:5px;
  color:var(--hi);padding:6px 10px;font-size:13px;font-family:inherit;width:100%}
input:focus,select:focus,textarea:focus{outline:none;border-color:var(--blue)}
textarea{resize:vertical;min-height:60px}
/* ── Buttons ───────────────────────────────────────────────── */
.btn{display:inline-flex;align-items:center;gap:5px;border-radius:5px;padding:5px 12px;font-size:12px;cursor:pointer;border:1px solid var(--border);background:#21262d;color:var(--text);transition:background .1s;font-family:inherit;white-space:nowrap}
.btn:hover{background:var(--border)}
.btn:disabled{opacity:.5;cursor:default}
.btn-green{background:#0d2a14;border-color:var(--green);color:var(--green)}.btn-green:hover{background:#163b1e}
.btn-red  {background:#2d0d0d;border-color:var(--red);  color:var(--red)  }.btn-red:hover  {background:#3d1515}
.btn-blue {background:rgba(88,166,255,.08);border-color:var(--blue);color:var(--blue)}.btn-blue:hover{background:rgba(88,166,255,.18)}
.btn-sm{padding:3px 8px;font-size:11px}
/* ── Tables ────────────────────────────────────────────────── */
table{border-collapse:collapse;width:100%;font-size:12px}
th,td{padding:7px 10px;text-align:left;border-bottom:1px solid var(--border2)}
th{color:var(--text2);background:var(--surface);font-weight:600;font-size:11px;text-transform:uppercase;letter-spacing:.04em;position:sticky;top:0;z-index:2}
tr:hover td{background:rgba(255,255,255,.025)}
.pos{color:var(--green)}.neg{color:var(--red)}.dim{color:var(--text2)}
.claimed{color:var(--green)}.unclaimed{color:var(--yellow)}
/* ── Content item rows ─────────────────────────────────────── */
.ci-list{display:flex;flex-direction:column;gap:6px;margin-bottom:6px}
.ci-item{background:var(--surface);border:1px solid var(--border);border-radius:7px;display:flex;align-items:stretch;overflow:hidden}
.ci-thumb{width:52px;flex-shrink:0;background:var(--bg);display:flex;align-items:center;justify-content:center;border-right:1px solid var(--border2);font-size:22px}
.ci-thumb img{width:52px;height:52px;object-fit:cover}
.ci-body{flex:1;padding:8px 12px;display:grid;grid-template-columns:140px 1fr 1fr;gap:4px 12px;align-items:center}
.ci-id{font-family:monospace;color:var(--blue);font-size:12px;font-weight:600}
.ci-name{color:var(--text);font-size:12px}
.ci-meta{color:var(--text2);font-size:11px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.ci-url a{color:var(--text2);font-size:11px;text-decoration:none;word-break:break-all}
.ci-url a:hover{color:var(--text)}
.ci-actions{flex-shrink:0;padding:6px 10px;display:flex;align-items:center;gap:5px;flex-wrap:wrap;border-left:1px solid var(--border2);background:var(--bg);min-width:180px;justify-content:flex-end}
.hash-pill{font-family:monospace;font-size:10px;background:var(--bg);border:1px solid var(--border2);border-radius:4px;padding:2px 6px;color:var(--text2);display:inline-block;cursor:help}
.hash-pill.ok{border-color:var(--green);color:var(--green)}
.hash-pill.miss{border-color:var(--yellow);color:var(--yellow)}
.dot{width:7px;height:7px;border-radius:50%;display:inline-block;flex-shrink:0}
.dot.on{background:var(--green)}.dot.off{background:var(--red)}
/* ── Type filter pills ─────────────────────────────────────── */
.pills{display:flex;gap:6px;flex-wrap:wrap;margin-bottom:14px}
.pill{padding:4px 13px;border-radius:20px;font-size:12px;cursor:pointer;border:1px solid var(--border);background:transparent;color:var(--text2);transition:all .1s;font-family:inherit}
.pill:hover{border-color:var(--blue);color:var(--text)}
.pill.active{background:rgba(88,166,255,.1);border-color:var(--blue);color:var(--blue);font-weight:600}
/* ── Section label ─────────────────────────────────────────── */
.sec-label{color:var(--text2);font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.08em;margin:16px 0 6px;padding-left:2px}
/* ── Inline hash row ───────────────────────────────────────── */
.hr{display:flex;align-items:center;gap:5px;flex-wrap:nowrap}
.hr input{width:96px;font-size:11px;padding:3px 6px;font-family:monospace}
</style>
</head>
<body>

<div class="topbar">
  <div class="topbar-title">CNR Economy Admin</div>
  <a href="?act=logout">Sign out</a>
</div>

<div class="layout">

<nav class="sidebar">
  <div class="nav">
    <button class="nav-btn active" id="nav-content"      onclick="showTab('content')">Content      <span class="badge"><?= $total_content ?></span></button>
    <button class="nav-btn"        id="nav-players"      onclick="showTab('players')">Players      <span class="badge"><?= $total_players ?></span></button>
    <button class="nav-btn"        id="nav-mail"         onclick="showTab('mail')">Mail         <span class="badge"><?= $total_mail ?></span></button>
    <button class="nav-btn"        id="nav-transactions" onclick="showTab('transactions')">Transactions <span class="badge"><?= $total_tx ?></span></button>
  </div>
</nav>

<div class="main">

<?php if ($flash): ?>
<div class="flash <?= $flash_ok ? 'ok' : 'err' ?>"><?= htmlspecialchars($flash) ?></div>
<?php endif; ?>

<div class="stats">
  <div class="stat"><span>Players</span><strong><?= $total_players ?></strong></div>
  <div class="stat"><span>Transactions</span><strong><?= $total_tx ?></strong></div>
  <div class="stat"><span>Mail sent</span><strong><?= $total_mail ?></strong></div>
  <div class="stat"><span>Unclaimed</span><strong style="color:var(--yellow)"><?= $unread_mail ?></strong></div>
  <div class="stat"><span>Content</span><strong><?= $total_content ?></strong></div>
</div>

<!-- ══════════════════════════════════════════════════════
     CONTENT
══════════════════════════════════════════════════════ -->
<div class="pane active" id="pane-content">

  <div class="pills">
    <button class="pill active" onclick="filterContent('all',this)">All (<?= $total_content ?>)</button>
    <?php
    $tc = []; foreach ($content_items as $c) $tc[$c['type']] = ($tc[$c['type']] ?? 0) + 1;
    $tl = ['map'=>'Maps','dlcmap'=>'DLC Maps','texture'=>'Textures','data'=>'Data','skin'=>'Skins','gun'=>'Guns'];
    foreach ($tl as $k => $v): ?>
    <button class="pill" onclick="filterContent('<?= $k ?>',this)"><?= $v ?> (<?= $tc[$k] ?? 0 ?>)</button>
    <?php endforeach; ?>
  </div>

  <?php foreach (['map','dlcmap','texture','data','skin','gun'] as $st):
    $items = array_values(array_filter($content_items, fn($c) => $c['type'] === $st));
    if (!$items) continue; ?>
  <div class="content-section" data-type="<?= $st ?>">
    <div class="sec-label"><?= $tl[$st] ?></div>
    <div class="ci-list">
    <?php foreach ($items as $c):
      $eid  = htmlspecialchars($c['id'], ENT_QUOTES);
      $eurl = htmlspecialchars($c['url'], ENT_QUOTES);
      $efh  = htmlspecialchars($c['file_hash'] ?? '', ENT_QUOTES);
      $eth  = htmlspecialchars($c['thumbnail_hash'] ?? '', ENT_QUOTES);
      // pick preview image
      $preview = '';
      if ($st === 'skin')  $preview = $c['url'];
      if (($st === 'map' || $st === 'dlcmap') && !empty($c['thumbnail_url'])) $preview = $c['thumbnail_url'];
    ?>
    <div class="ci-item">

      <!-- thumbnail -->
      <div class="ci-thumb">
        <?php if ($preview): ?>
          <img src="<?= htmlspecialchars($preview) ?>" loading="lazy" onerror="this.parentNode.textContent='?'">
        <?php else: ?>
          <?= strtoupper(substr($st,0,1)) ?>
        <?php endif; ?>
      </div>

      <!-- info -->
      <div class="ci-body">
        <div>
          <div class="ci-id"><?= htmlspecialchars($c['id']) ?></div>
          <?php if ($c['name']): ?><div class="ci-name"><?= htmlspecialchars($c['name']) ?></div><?php endif; ?>
        </div>
        <div>
          <?php if ($st === 'texture'): ?><div class="ci-meta">mat: <?= htmlspecialchars($c['material_name']) ?></div><?php endif; ?>
          <?php if ($st === 'data'):    ?><div class="ci-meta">key: <?= htmlspecialchars($c['data_key']) ?></div><?php endif; ?>
          <?php if ($st === 'skin'):    ?><div class="ci-meta"><?= htmlspecialchars($c['material_name']) ?> / <?= htmlspecialchars($c['data_key']) ?></div><?php endif; ?>
          <?php if ($st === 'gun'):     ?><div class="ci-meta"><?= htmlspecialchars($c['data_key']) ?> / <?= htmlspecialchars($c['material_name']) ?></div><?php endif; ?>
          <div class="ci-url"><a href="<?= htmlspecialchars($c['url']) ?>" target="_blank" title="<?= htmlspecialchars($c['url']) ?>"><?= htmlspecialchars(substr(preg_replace('#^https?://[^/]+#','', $c['url']), 0, 60)) ?></a></div>
        </div>
        <div>
          <!-- file hash -->
          <div class="hr" style="margin-bottom:4px">
            <span class="hash-pill <?= $c['file_hash']?'ok':'miss' ?>" title="<?= $efh ?>"><?= $c['file_hash'] ? substr($c['file_hash'],0,8).'…' : 'no hash' ?></span>
            <button type="button" class="btn btn-sm btn-blue" onclick="syncHash('<?= $eid ?>','<?= $eurl ?>','file_hash',this)">Sync</button>
          </div>
          <?php if (($st === 'map' || $st === 'dlcmap') && !empty($c['thumbnail_url'])): ?>
          <!-- thumbnail hash -->
          <div class="hr">
            <span style="font-size:10px;color:var(--text2)">thumb:</span>
            <span class="hash-pill <?= $c['thumbnail_hash']?'ok':'miss' ?>" title="<?= $eth ?>"><?= $c['thumbnail_hash'] ? substr($c['thumbnail_hash'],0,8).'…' : 'no hash' ?></span>
            <button type="button" class="btn btn-sm btn-blue" onclick="syncHash('<?= $eid ?>','<?= htmlspecialchars($c['thumbnail_url'],ENT_QUOTES) ?>','thumbnail_hash',this)">Sync</button>
          </div>
          <?php elseif ($st === 'map' || $st === 'dlcmap'): ?>
          <form method="POST" enctype="multipart/form-data" style="display:flex;align-items:center;gap:4px;margin-top:4px">
            <input type="hidden" name="act" value="upload_thumbnail">
            <input type="hidden" name="content_id" value="<?= $eid ?>">
            <input type="file" name="thumb_file" accept="image/*" id="tf-<?= $eid ?>" style="display:none" onchange="this.form.submit()">
            <button type="button" class="btn btn-sm" onclick="document.getElementById('tf-<?= $eid ?>').click()">Upload thumb</button>
          </form>
          <?php endif; ?>
        </div>
      </div>

      <!-- actions -->
      <div class="ci-actions">
        <span class="dot <?= $c['enabled']?'on':'off' ?>"></span>
        <form method="POST" style="display:inline">
          <input type="hidden" name="act" value="toggle_content">
          <input type="hidden" name="content_id" value="<?= $eid ?>">
          <button class="btn btn-sm" type="submit"><?= $c['enabled']?'Disable':'Enable' ?></button>
        </form>
        <?php if ($st === 'skin' || $st === 'gun'): ?>
        <form method="POST" style="display:flex;align-items:center;gap:4px">
          <input type="hidden" name="act" value="reorder_content">
          <input type="hidden" name="content_id" value="<?= $eid ?>">
          <input type="number" name="sort_order" value="<?= (int)$c['sort_order'] ?>" min="0" title="Price in coins" style="width:62px;font-size:11px;padding:3px 5px">
          <button class="btn btn-sm btn-green" type="submit">Set $</button>
        </form>
        <?php endif; ?>
        <form method="POST" onsubmit="return confirm('Delete <?= $eid ?>?')" style="display:inline">
          <input type="hidden" name="act" value="delete_content">
          <input type="hidden" name="content_id" value="<?= $eid ?>">
          <button class="btn btn-sm btn-red" type="submit">Del</button>
        </form>
      </div>

    </div>
    <?php endforeach; ?>
    </div>
  </div>
  <?php endforeach; ?>

  <!-- Add content -->
  <div class="panel" style="margin-top:24px">
    <div class="panel-title">+ Add Content Item</div>
    <form method="POST" enctype="multipart/form-data">
      <input type="hidden" name="act" value="add_content">
      <div class="fg">
        <label>Type</label>
        <select name="ctype" id="ctype-sel" onchange="updateAddForm()" style="max-width:140px;width:auto">
          <option value="map">map</option><option value="dlcmap">dlcmap</option><option value="texture">texture</option>
          <option value="data">data</option><option value="skin">skin</option><option value="gun">gun</option>
        </select>
        <label>ID</label>
        <input type="text" name="content_id" placeholder="official_map_1" pattern="[a-zA-Z0-9_\-]+" required>
        <label>Display name</label>
        <input type="text" name="cname" placeholder="Snow Reimagined" maxlength="80">
        <label>URL</label>
        <div style="display:flex;gap:6px">
          <input type="url" name="curl" id="add-curl" placeholder="https://…" required>
        </div>
        <label>MD5 hash</label>
        <div style="display:flex;gap:6px;align-items:center">
          <input type="text" name="file_hash" id="add-fhash" placeholder="auto-fill" maxlength="32" pattern="[a-fA-F0-9]{0,32}" style="font-family:monospace;flex:1">
          <button type="button" class="btn btn-blue btn-sm" onclick="calcIntoField(document.getElementById('add-curl').value,document.getElementById('add-fhash'),this)">Calc</button>
        </div>
        <label id="add-thumb-lbl">Thumbnail</label>
        <div id="add-thumb-row">
          <input type="file" name="thumb_file" accept="image/jpeg,image/png,image/gif,image/webp" style="width:auto">
        </div>
        <label id="add-mat-lbl" style="display:none">Material name</label>
        <input type="text" name="material_name" id="add-mat" placeholder="Skin_34_2" style="display:none">
        <label id="add-key-lbl" style="display:none">Data / slot key</label>
        <input type="text" name="data_key" id="add-key" placeholder="Skin_34" style="display:none">
        <label id="add-sort-lbl">Sort order</label>
        <input type="number" name="sort_order" value="0" style="max-width:100px;width:100px">
        <div></div>
        <div class="actions"><button type="submit" class="btn btn-green">Add Item</button></div>
      </div>
    </form>
  </div>
</div><!-- /pane-content -->

<!-- ══════════════════════════════════════════════════════
     PLAYERS
══════════════════════════════════════════════════════ -->
<div class="pane" id="pane-players">
  <div class="panel">
    <div class="panel-title">Grant / Set Currency</div>
    <form method="POST">
      <input type="hidden" name="act" value="grant">
      <div class="fg">
        <label>Player</label>
        <select name="player_id" id="grant-player" required>
          <option value="">— select —</option>
          <?php foreach ($players as $p): ?>
          <option value="<?= htmlspecialchars($p['id'],ENT_QUOTES) ?>"><?= htmlspecialchars($p['display_name']) ?> (<?= substr(htmlspecialchars($p['id']),0,8) ?>…)</option>
          <?php endforeach; ?>
        </select>
        <label>Coins</label><input type="number" name="coins" value="0" min="-99999" max="99999" style="max-width:120px;width:120px">
        <label>Gems</label> <input type="number" name="gems"  value="0" min="-9999"  max="9999"  style="max-width:120px;width:120px">
        <label>Mode</label>
        <select name="mode" style="max-width:180px;width:auto">
          <option value="add">Add to balance</option>
          <option value="set">Set balance to</option>
        </select>
        <div></div><div class="actions"><button type="submit" class="btn btn-green">Apply</button></div>
      </div>
    </form>
  </div>
  <div style="display:flex;align-items:center;gap:10px;margin-bottom:10px">
    <input type="search" id="player-search" placeholder="Search players…" oninput="filterTable('ptbl',this.value)" style="width:220px">
  </div>
  <table id="ptbl">
    <tr><th>Name</th><th>ID</th><th>Coins</th><th>Gems</th><th>Last seen</th><th>Actions</th></tr>
    <?php foreach ($players as $p): ?>
    <tr>
      <td><?= htmlspecialchars($p['display_name']) ?></td>
      <td><code style="font-size:11px;color:var(--blue)" title="<?= htmlspecialchars($p['id']) ?>"><?= substr(htmlspecialchars($p['id']),0,10) ?>…</code></td>
      <td class="pos"><?= number_format((int)$p['coins']) ?></td>
      <td class="pos"><?= number_format((int)$p['gems']) ?></td>
      <td class="dim"><?= date('M d H:i', (int)$p['last_seen']) ?></td>
      <td style="white-space:nowrap;display:flex;gap:4px;padding:4px 10px">
        <button class="btn btn-sm" onclick="prefillMail('<?= htmlspecialchars($p['id'],ENT_QUOTES) ?>')">Mail</button>
        <button class="btn btn-sm" onclick="prefillGrant('<?= htmlspecialchars($p['id'],ENT_QUOTES) ?>')">Grant</button>
        <button class="btn btn-sm" onclick="copyText('<?= htmlspecialchars($p['id'],ENT_QUOTES) ?>',this)">Copy ID</button>
      </td>
    </tr>
    <?php endforeach; ?>
  </table>
</div><!-- /pane-players -->

<!-- ══════════════════════════════════════════════════════
     MAIL
══════════════════════════════════════════════════════ -->
<div class="pane" id="pane-mail">
  <div class="panel">
    <div class="panel-title">Send Mail</div>
    <form method="POST" id="mail-form">
      <input type="hidden" name="act" value="send_mail">
      <div class="fg">
        <label>Player</label>
        <select name="player_id" id="mail-player" required>
          <option value="">— select —</option>
          <option value="*" style="color:var(--red);font-weight:bold">ALL PLAYERS (broadcast)</option>
          <?php foreach ($players as $p): ?>
          <option value="<?= htmlspecialchars($p['id'],ENT_QUOTES) ?>"><?= htmlspecialchars($p['display_name']) ?> (<?= substr(htmlspecialchars($p['id']),0,8) ?>…)</option>
          <?php endforeach; ?>
        </select>
        <label>Subject</label>
        <input type="text" name="subject" maxlength="100" placeholder="You earned a reward!">
        <label>Body</label>
        <textarea name="body" maxlength="500" placeholder="Message text…"></textarea>
        <label>Coins / Gems / Spins</label>
        <div style="display:flex;gap:8px">
          <input type="number" name="coins" value="0" min="0" max="99999" style="width:90px" placeholder="Coins">
          <input type="number" name="gems"  value="0" min="0" max="9999"  style="width:90px" placeholder="Gems">
          <input type="number" name="spins" value="0" min="0" max="99"    style="width:70px" placeholder="Spins">
        </div>
        <div></div><div class="actions"><button type="submit" class="btn btn-green">Send Mail</button></div>
      </div>
    </form>
  </div>
  <div class="sec-label">Mail Log</div>
  <table>
    <tr><th>#</th><th>Sent</th><th>To</th><th>Subject</th><th>Coins</th><th>Gems</th><th>Spins</th><th>Status</th></tr>
    <?php foreach ($recent_mail as $m): ?>
    <tr>
      <td class="dim"><?= (int)$m['id'] ?></td>
      <td class="dim"><?= date('M d H:i', (int)$m['sent_at']) ?></td>
      <td><?= htmlspecialchars($m['display_name']) ?></td>
      <td><?= htmlspecialchars($m['subject']) ?></td>
      <td class="<?= (int)$m['m_coins']>0?'pos':'' ?>"><?= (int)$m['m_coins'] ?></td>
      <td class="<?= (int)$m['m_gems'] >0?'pos':'' ?>"><?= (int)$m['m_gems'] ?></td>
      <td class="<?= (int)($m['m_spins']??0)>0?'pos':'' ?>"><?= (int)($m['m_spins']??0) ?></td>
      <td class="<?= $m['claimed']?'claimed':'unclaimed' ?>"><?= $m['claimed']?'claimed':'pending' ?></td>
    </tr>
    <?php endforeach; ?>
  </table>
</div><!-- /pane-mail -->

<!-- ══════════════════════════════════════════════════════
     TRANSACTIONS
══════════════════════════════════════════════════════ -->
<div class="pane" id="pane-transactions">
  <div class="sec-label" style="margin-top:0">Recent Transactions</div>
  <table>
    <tr><th>Time</th><th>Player</th><th>Coins</th><th>Gems</th><th>Reason</th></tr>
    <?php foreach ($recent_tx as $r): ?>
    <tr>
      <td class="dim"><?= date('M d H:i', (int)$r['created_at']) ?></td>
      <td><?= htmlspecialchars($r['display_name']) ?></td>
      <td class="<?= (int)$r['delta_coins']>=0?'pos':'neg' ?>"><?= (int)$r['delta_coins']>=0?'+':'' ?><?= (int)$r['delta_coins'] ?></td>
      <td class="<?= (int)$r['delta_gems'] >=0?'pos':'neg' ?>"><?= (int)$r['delta_gems'] >=0?'+':'' ?><?= (int)$r['delta_gems'] ?></td>
      <td class="dim"><?= htmlspecialchars($r['reason']) ?></td>
    </tr>
    <?php endforeach; ?>
  </table>
</div><!-- /pane-transactions -->

</div><!-- /.main -->
</div><!-- /.layout -->

<script>
// ── Tabs ───────────────────────────────────────────────────────────────────
var _tab = new URLSearchParams(location.search).get('tab') || 'content';
function showTab(name) {
  document.querySelectorAll('.pane').forEach(p    => p.classList.toggle('active', p.id==='pane-'+name));
  document.querySelectorAll('.nav-btn').forEach(b => b.classList.toggle('active', b.id==='nav-'+name));
  history.replaceState(null,'','?tab='+name);
  _tab = name;
}
(function(){ showTab(_tab); })();

// ── Content filter ─────────────────────────────────────────────────────────
function filterContent(type, pill) {
  document.querySelectorAll('.pills .pill').forEach(p => p.classList.remove('active'));
  pill.classList.add('active');
  document.querySelectorAll('.content-section').forEach(s => {
    s.style.display = (type==='all' || s.dataset.type===type) ? '' : 'none';
  });
}

// ── Sync hash (fetch + DB save, no page reload) ────────────────────────────
function syncHash(cid, url, field, btn) {
  if (!url) { alert('No URL on this item.'); return; }
  var orig = btn.textContent;
  btn.disabled = true; btn.textContent = '…';
  var fd = new FormData();
  fd.append('act','sync_hash'); fd.append('content_id',cid);
  fd.append('url',url); fd.append('field',field);
  fetch('',{method:'POST',body:fd})
    .then(r=>r.json())
    .then(d=>{
      if (d.hash) {
        // update the pill to the right of this button
        var row = btn.closest('.hr');
        if (row) {
          var pill = row.querySelector('.hash-pill');
          if (pill) { pill.textContent=d.hash.slice(0,8)+'…'; pill.title=d.hash; pill.className='hash-pill ok'; }
        }
        btn.textContent='ok'; btn.classList.replace('btn-blue','btn-green');
        setTimeout(()=>{ btn.textContent='Sync'; btn.disabled=false; btn.classList.replace('btn-green','btn-blue'); }, 2000);
      } else {
        btn.textContent=orig; btn.disabled=false;
        alert('Error: '+(d.error||'unknown'));
      }
    })
    .catch(e=>{ btn.textContent=orig; btn.disabled=false; alert('Fetch failed: '+e); });
}

// ── Calc hash into add-form field ──────────────────────────────────────────
function calcIntoField(url, inp, btn) {
  if (!url) { alert('Enter a URL first.'); return; }
  var orig = btn.textContent; btn.disabled=true; btn.textContent='…';
  var fd = new FormData(); fd.append('act','fetch_hash'); fd.append('url',url);
  fetch('',{method:'POST',body:fd})
    .then(r=>r.json())
    .then(d=>{
      btn.disabled=false;
      if (d.hash) { inp.value=d.hash; btn.textContent='ok'; }
      else { btn.textContent=orig; alert('Error: '+(d.error||'unknown')); }
    })
    .catch(e=>{ btn.textContent=orig; btn.disabled=false; alert(''+e); });
}

// ── Table search ───────────────────────────────────────────────────────────
function filterTable(id, q) {
  q = q.toLowerCase();
  document.querySelectorAll('#'+id+' tr:not(:first-child)').forEach(r=>{
    r.style.display = r.textContent.toLowerCase().includes(q) ? '' : 'none';
  });
}

// ── Prefill helpers ────────────────────────────────────────────────────────
function prefillMail(pid) {
  showTab('mail');
  setTimeout(()=>{ var e=document.getElementById('mail-player'); if(e){e.value=pid;e.scrollIntoView({behavior:'smooth',block:'center'});} }, 60);
}
function prefillGrant(pid) {
  showTab('players');
  setTimeout(()=>{ var e=document.getElementById('grant-player'); if(e){e.value=pid;e.scrollIntoView({behavior:'smooth',block:'center'});} }, 60);
}
function copyText(t, btn) {
  navigator.clipboard.writeText(t).then(()=>{ var o=btn.textContent; btn.textContent='Copied!'; setTimeout(()=>btn.textContent=o,1400); });
}

// ── Add form dynamic fields ────────────────────────────────────────────────
function updateAddForm() {
  var t = document.getElementById('ctype-sel').value;
  var show = (id,v)=>{ var el=document.getElementById(id); if(el) el.style.display=v?'':'none'; };
  var isMap = t==='map'||t==='dlcmap';
  show('add-thumb-lbl', isMap); show('add-thumb-row', isMap);
  var mat = t==='texture'||t==='skin'||t==='gun';
  var key = t==='data'||t==='skin'||t==='gun';
  show('add-mat-lbl',mat); show('add-mat',mat);
  show('add-key-lbl',key); show('add-key',key);
  document.getElementById('add-mat').placeholder = t==='skin'?'Skin_34_2':(t==='gun'?'gun_body':'mat_name');
  document.getElementById('add-key').placeholder = t==='skin'?'Skin_34':(t==='gun'?'AK':'data_key');
  document.getElementById('add-sort-lbl').textContent = (t==='skin'||t==='gun') ? 'Price (coins)' : 'Sort order';
}
updateAddForm();
</script>
</body>
</html>
