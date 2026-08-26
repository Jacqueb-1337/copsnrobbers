<?php
// Authenticated command-line/admin API for the CNR economy portal.
//
// Authentication:
//   Authorization: Bearer <CNR_ADMIN_API_TOKEN>
// or HTTP Basic auth using the same admin password as the web portal.
//
// Requests may use JSON, application/x-www-form-urlencoded, or multipart/form-data.
// Use `action` (or legacy alias `act`) to select an operation.

require_once __DIR__ . '/../_admin_auth.php';
require_once __DIR__ . '/../_db.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

function api_reply(array $payload, int $status = 200): void
{
    http_response_code($status);
    echo json_encode($payload, JSON_UNESCAPED_SLASHES);
    exit;
}

function api_ok(array $payload = []): void
{
    api_reply(array_merge(['ok' => true], $payload));
}

function api_fail(string $message, int $status = 400, array $extra = []): void
{
    api_reply(array_merge(['ok' => false, 'error' => $message], $extra), $status);
}

if (!cnr_admin_verify_api_request()) {
    header('WWW-Authenticate: Basic realm="CNR Economy Admin API"');
    api_fail('Unauthorized.', 401);
}

function api_params(): array
{
    $params = [];
    if (!empty($_GET)) $params = array_merge($params, $_GET);
    if (!empty($_POST)) $params = array_merge($params, $_POST);

    $contentType = strtolower(trim((string)($_SERVER['CONTENT_TYPE'] ?? '')));
    if (strpos($contentType, 'application/json') === 0) {
        $raw = file_get_contents('php://input');
        $json = json_decode((string)$raw, true);
        if (is_array($json)) $params = array_merge($params, $json);
    }
    return $params;
}

function api_bool($value, bool $default = false): bool
{
    if ($value === null || $value === '') return $default;
    if (is_bool($value)) return $value;
    $v = strtolower(trim((string)$value));
    if (in_array($v, ['1','true','yes','on','enabled'], true)) return true;
    if (in_array($v, ['0','false','no','off','disabled'], true)) return false;
    return $default;
}

function api_content_type($value): string
{
    $value = strtolower(trim((string)$value));
    $valid = ['map','dlcmap','texture','data','skin','gun'];
    if (!in_array($value, $valid, true)) api_fail('Invalid content type. Expected map, dlcmap, texture, data, skin, or gun.');
    return $value;
}

function api_content_id($value): string
{
    $id = preg_replace('/[^a-z0-9_\-]/i', '_', trim((string)$value));
    $id = trim((string)$id, '_');
    if ($id === '') api_fail('Content ID is required.');
    return $id;
}

function api_fetch_url_bytes(string $url): string
{
    $parts = parse_url($url);
    if (!filter_var($url, FILTER_VALIDATE_URL) || !in_array(strtolower((string)($parts['scheme'] ?? '')), ['http','https'], true)) {
        api_fail('Invalid URL.');
    }

    $data = false;
    if (function_exists('curl_init')) {
        $ch = curl_init($url);
        curl_setopt_array($ch, [
            CURLOPT_RETURNTRANSFER => true,
            CURLOPT_TIMEOUT => 30,
            CURLOPT_FOLLOWLOCATION => true,
            CURLOPT_MAXREDIRS => 5,
            CURLOPT_ENCODING => 'identity',
            CURLOPT_SSL_VERIFYPEER => true,
        ]);
        $data = curl_exec($ch);
        curl_close($ch);
    }
    if ($data === false || $data === null) {
        $ctx = stream_context_create([
            'http' => ['timeout' => 30, 'header' => "Accept-Encoding: identity\r\n"],
            'https' => ['timeout' => 30, 'header' => "Accept-Encoding: identity\r\n"],
        ]);
        $data = @file_get_contents($url, false, $ctx);
    }
    if ($data === false || $data === null || $data === '') api_fail('Could not fetch URL for hashing.', 502);
    return (string)$data;
}

function api_public_base(): string
{
    $configured = rtrim(cnr_admin_env('CNR_PUBLIC_BASE_URL', ''), '/');
    if ($configured !== '') return $configured;

    if (!empty($_SERVER['HTTP_X_FORWARDED_PROTO'])) {
        $scheme = strtolower(trim((string)$_SERVER['HTTP_X_FORWARDED_PROTO'])) === 'https' ? 'https' : 'http';
    } else {
        $scheme = (!empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off') ? 'https' : 'http';
    }
    $host = trim((string)($_SERVER['HTTP_HOST'] ?? 'play.jacqueb.me'));
    if ($host === '') $host = 'play.jacqueb.me';
    return $scheme . '://' . $host . '/economy';
}

function api_storage_bucket(string $type): string
{
    switch ($type) {
        case 'map':
        case 'dlcmap': return 'maps';
        case 'gun': return 'guns';
        case 'texture': return 'textures';
        case 'skin': return 'skins';
        case 'data': return 'data';
    }
    api_fail('Unsupported content type.');
}

function api_allowed_extensions(string $type): array
{
    switch ($type) {
        case 'map':
        case 'dlcmap':
            return ['json','bin','dat','bytes','cnrmap','cnrpack'];
        case 'gun':
            return ['json','bin','dat','bytes','cnrgun','cnrpack','png','jpg','jpeg','webp'];
        case 'texture':
        case 'skin':
            return ['png','jpg','jpeg','gif','webp','dds'];
        case 'data':
            return ['json','bin','dat','bytes','txt','cnrpack'];
    }
    return [];
}

function api_require_upload(string $field): array
{
    if (!isset($_FILES[$field])) api_fail('Missing uploaded file field: ' . $field . '.');
    $file = $_FILES[$field];
    if (!is_array($file) || (int)($file['error'] ?? UPLOAD_ERR_NO_FILE) !== UPLOAD_ERR_OK) {
        api_fail('Upload failed for ' . $field . ' (code ' . (int)($file['error'] ?? UPLOAD_ERR_NO_FILE) . ').');
    }
    if ((int)($file['size'] ?? 0) <= 0) api_fail('Uploaded file is empty.');
    if ((int)$file['size'] > 64 * 1024 * 1024) api_fail('Uploaded file exceeds the 64 MB API safety limit.', 413);
    if (!is_uploaded_file((string)$file['tmp_name'])) api_fail('Upload source is invalid.');
    return $file;
}

function api_store_content_upload(array $file, string $type, string $contentId): array
{
    $original = (string)($file['name'] ?? '');
    $ext = strtolower((string)pathinfo($original, PATHINFO_EXTENSION));
    if ($ext === '') $ext = 'bin';
    if (!in_array($ext, api_allowed_extensions($type), true)) {
        api_fail('File extension .' . $ext . ' is not allowed for content type ' . $type . '.');
    }

    $bucket = api_storage_bucket($type);
    $dir = __DIR__ . '/../uploads/' . $bucket . '/';
    if (!is_dir($dir) && !mkdir($dir, 0755, true) && !is_dir($dir)) api_fail('Could not create upload directory.', 500);

    // Remove stale same-ID files with other allowed extensions before replacing.
    foreach (api_allowed_extensions($type) as $oldExt) {
        $old = $dir . $contentId . '.' . $oldExt;
        if (is_file($old)) @unlink($old);
    }

    $dest = $dir . $contentId . '.' . $ext;
    if (!move_uploaded_file((string)$file['tmp_name'], $dest)) api_fail('Could not move uploaded file into content storage.', 500);

    $hash = md5_file($dest);
    if ($hash === false) api_fail('Could not hash uploaded file.', 500);
    return [
        'path' => $dest,
        'hash' => strtolower($hash),
        'url' => api_public_base() . '/uploads/' . rawurlencode($bucket) . '/' . rawurlencode($contentId . '.' . $ext),
        'size' => filesize($dest),
        'extension' => $ext,
    ];
}

function api_store_thumbnail(PDO $pdo, string $contentId, string $field = 'thumb_file'): array
{
    $file = api_require_upload($field);
    if ((int)$file['size'] > 512 * 1024) api_fail('Thumbnail exceeds 512 KB.', 413);

    $allowed = ['image/jpeg' => 'jpg', 'image/png' => 'png', 'image/gif' => 'gif', 'image/webp' => 'webp'];
    $mime = function_exists('mime_content_type') ? @mime_content_type((string)$file['tmp_name']) : '';
    if (!isset($allowed[$mime])) api_fail('Thumbnail must be jpg, png, gif, or webp.');

    $dir = __DIR__ . '/../uploads/thumbnails/';
    if (!is_dir($dir) && !mkdir($dir, 0755, true) && !is_dir($dir)) api_fail('Could not create thumbnail directory.', 500);
    foreach (['jpg','png','gif','webp'] as $oldExt) {
        $old = $dir . $contentId . '.' . $oldExt;
        if (is_file($old)) @unlink($old);
    }

    $ext = $allowed[$mime];
    $dest = $dir . $contentId . '.' . $ext;
    if (!move_uploaded_file((string)$file['tmp_name'], $dest)) api_fail('Could not move thumbnail.', 500);
    $hash = md5_file($dest);
    $url = api_public_base() . '/uploads/thumbnails/' . rawurlencode($contentId . '.' . $ext);
    $pdo->prepare('UPDATE content_items SET thumbnail_url = ?, thumbnail_hash = ? WHERE id = ?')
        ->execute([$url, strtolower((string)$hash), $contentId]);
    return ['url' => $url, 'hash' => strtolower((string)$hash), 'size' => filesize($dest)];
}

function api_content_row(PDO $pdo, string $id): ?array
{
    $stmt = $pdo->prepare('SELECT * FROM content_items WHERE id = ?');
    $stmt->execute([$id]);
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    return $row ?: null;
}

$params = api_params();
$action = strtolower(trim((string)($params['action'] ?? $params['act'] ?? 'status')));
$pdo = db();

switch ($action) {
    case 'status':
    case 'help':
        api_ok([
            'api_version' => 1,
            'auth' => cnr_admin_api_token() !== '' ? ['bearer','basic'] : ['basic'],
            'actions' => [
                'status','list_content','list_players','list_mail','list_transactions',
                'send_mail','grant','add_content','upload_content','upload_content_chunk','upload_thumbnail','upload_thumb',
                'toggle_content','set_content_enabled','delete_content','reorder_content','set_price',
                'update_hash','sync_hash','calc_hash'
            ],
        ]);
        break;

    case 'list_content':
        $rows = $pdo->query('SELECT id,type,name,url,thumbnail_url,file_hash,thumbnail_hash,base_scene,material_name,data_key,sort_order,enabled,created_at FROM content_items ORDER BY type,sort_order ASC,created_at ASC')
            ->fetchAll(PDO::FETCH_ASSOC);
        api_ok(['content' => $rows]);
        break;

    case 'list_players':
        $limit = max(1, min(1000, (int)($params['limit'] ?? 200)));
        $stmt = $pdo->prepare('SELECT id,display_name,coins,gems,registered_at,last_seen FROM accounts ORDER BY last_seen DESC LIMIT ?');
        $stmt->bindValue(1, $limit, PDO::PARAM_INT);
        $stmt->execute();
        api_ok(['players' => $stmt->fetchAll(PDO::FETCH_ASSOC)]);
        break;

    case 'list_mail':
        $limit = max(1, min(1000, (int)($params['limit'] ?? 100)));
        $stmt = $pdo->prepare('SELECT m.id,m.player_id,m.sent_at,m.subject,m.body,m.coins,m.gems,m.spins,m.claimed,a.display_name FROM player_mail m LEFT JOIN accounts a ON a.id=m.player_id ORDER BY m.id DESC LIMIT ?');
        $stmt->bindValue(1, $limit, PDO::PARAM_INT);
        $stmt->execute();
        api_ok(['mail' => $stmt->fetchAll(PDO::FETCH_ASSOC)]);
        break;

    case 'list_transactions':
        $limit = max(1, min(1000, (int)($params['limit'] ?? 100)));
        $stmt = $pdo->prepare('SELECT t.id,t.player_id,t.created_at,t.delta_coins,t.delta_gems,t.reason,t.match_id,a.display_name FROM transactions t LEFT JOIN accounts a ON a.id=t.player_id ORDER BY t.id DESC LIMIT ?');
        $stmt->bindValue(1, $limit, PDO::PARAM_INT);
        $stmt->execute();
        api_ok(['transactions' => $stmt->fetchAll(PDO::FETCH_ASSOC)]);
        break;

    case 'send_mail':
        $playerId = trim((string)($params['player_id'] ?? ''));
        $subject = trim((string)($params['subject'] ?? ''));
        $body = trim((string)($params['body'] ?? ''));
        $coins = max(0, (int)($params['coins'] ?? 0));
        $gems = max(0, (int)($params['gems'] ?? 0));
        $spins = max(0, (int)($params['spins'] ?? 0));
        if ($subject === '') api_fail('Subject is required.');
        if ($playerId === '') api_fail('player_id is required. Use * for a broadcast.');

        $stmt = $pdo->prepare('INSERT INTO player_mail (player_id,subject,body,coins,gems,spins,claimed,sent_at) VALUES (?,?,?,?,?,?,0,?)');
        $now = time();
        if ($playerId === '*') {
            $players = $pdo->query('SELECT id FROM accounts')->fetchAll(PDO::FETCH_ASSOC);
            $pdo->beginTransaction();
            try {
                foreach ($players as $player) $stmt->execute([$player['id'],$subject,$body,$coins,$gems,$spins,$now]);
                $pdo->commit();
            } catch (Exception $e) {
                if ($pdo->inTransaction()) $pdo->rollBack();
                api_fail('Could not send broadcast: ' . $e->getMessage(), 500);
            }
            api_ok(['sent' => count($players), 'broadcast' => true]);
        }
        $exists = $pdo->prepare('SELECT id FROM accounts WHERE id = ?');
        $exists->execute([$playerId]);
        if (!$exists->fetch()) api_fail('Player not found.', 404);
        $stmt->execute([$playerId,$subject,$body,$coins,$gems,$spins,$now]);
        api_ok(['sent' => 1, 'player_id' => $playerId, 'mail_id' => (int)$pdo->lastInsertId()]);
        break;

    case 'grant':
        $playerId = trim((string)($params['player_id'] ?? ''));
        $coins = (int)($params['coins'] ?? 0);
        $gems = (int)($params['gems'] ?? 0);
        $mode = strtolower(trim((string)($params['mode'] ?? 'add')));
        if (!in_array($mode, ['add','set'], true)) api_fail('mode must be add or set.');
        $stmt = $pdo->prepare('SELECT id,display_name,coins,gems FROM accounts WHERE id = ?');
        $stmt->execute([$playerId]);
        $player = $stmt->fetch(PDO::FETCH_ASSOC);
        if (!$player) api_fail('Player not found.', 404);

        if ($mode === 'set') {
            $deltaCoins = $coins - (int)$player['coins'];
            $deltaGems = $gems - (int)$player['gems'];
            $pdo->prepare('UPDATE accounts SET coins=?,gems=? WHERE id=?')->execute([$coins,$gems,$playerId]);
        } else {
            $deltaCoins = $coins;
            $deltaGems = $gems;
            $pdo->prepare('UPDATE accounts SET coins=coins+?,gems=gems+? WHERE id=?')->execute([$coins,$gems,$playerId]);
        }
        $pdo->prepare("INSERT INTO transactions (player_id,delta_coins,delta_gems,reason,created_at) VALUES (?,?,?,'admin_grant',?)")
            ->execute([$playerId,$deltaCoins,$deltaGems,time()]);
        $balance = $pdo->prepare('SELECT coins,gems FROM accounts WHERE id=?');
        $balance->execute([$playerId]);
        api_ok(['player_id' => $playerId, 'display_name' => $player['display_name'], 'mode' => $mode, 'balance' => $balance->fetch(PDO::FETCH_ASSOC)]);
        break;

    case 'add_content':
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        $type = api_content_type($params['ctype'] ?? $params['type'] ?? '');
        $name = trim((string)($params['cname'] ?? $params['name'] ?? ''));
        $url = trim((string)($params['curl'] ?? $params['url'] ?? ''));
        if ($url === '') api_fail('URL is required.');
        $baseScene = trim((string)($params['base_scene'] ?? 'FreeRun3_1'));
        $material = trim((string)($params['material_name'] ?? ''));
        $dataKey = trim((string)($params['data_key'] ?? ''));
        $sort = (int)($params['sort_order'] ?? $params['price'] ?? 0);
        $enabled = api_bool($params['enabled'] ?? null, true) ? 1 : 0;
        $hash = strtolower(preg_replace('/[^a-f0-9]/i', '', trim((string)($params['file_hash'] ?? $params['hash'] ?? ''))));
        if ($hash === '') $hash = md5(api_fetch_url_bytes($url));
        if (strlen($hash) !== 32) api_fail('file_hash must be a 32-character MD5 hash.');
        if (api_content_row($pdo, $id)) api_fail('Content ID already exists. Use upload_content with replace=1 or delete the existing item first.', 409);

        $pdo->prepare('INSERT INTO content_items (id,type,name,url,base_scene,material_name,data_key,sort_order,enabled,created_at,file_hash) VALUES (?,?,?,?,?,?,?,?,?,?,?)')
            ->execute([$id,$type,$name,$url,$baseScene,$material,$dataKey,$sort,$enabled,time(),$hash]);
        $thumb = null;
        if (isset($_FILES['thumb_file']) && (int)$_FILES['thumb_file']['error'] === UPLOAD_ERR_OK && in_array($type, ['map','dlcmap'], true)) {
            $thumb = api_store_thumbnail($pdo, $id, 'thumb_file');
        }
        api_ok(['content' => api_content_row($pdo, $id), 'thumbnail' => $thumb]);
        break;

    case 'upload_content_chunk':
        $file = api_require_upload('chunk');
        $type = api_content_type($params['ctype'] ?? $params['type'] ?? '');
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        $existing = api_content_row($pdo, $id);
        $replace = api_bool($params['replace'] ?? null, false);
        if ($existing && !$replace) api_fail('Content ID already exists. Re-run with replace=1 to replace it.', 409);

        $uploadId = trim((string)($params['upload_id'] ?? ''));
        if (!preg_match('/^[a-zA-Z0-9_-]{8,80}$/', $uploadId)) api_fail('upload_id must be 8-80 letters, numbers, underscores, or hyphens.');
        $chunkIndex = (int)($params['chunk_index'] ?? -1);
        $chunkCount = (int)($params['chunk_count'] ?? 0);
        if ($chunkCount < 1 || $chunkCount > 256 || $chunkIndex < 0 || $chunkIndex >= $chunkCount) api_fail('Invalid chunk_index/chunk_count.');

        $originalName = trim((string)($params['filename'] ?? $params['file_name'] ?? ''));
        if ($originalName === '') $originalName = $id . '.bin';
        $ext = strtolower((string)pathinfo($originalName, PATHINFO_EXTENSION));
        if ($ext === '') $ext = 'bin';
        if (!in_array($ext, api_allowed_extensions($type), true)) api_fail('File extension .' . $ext . ' is not allowed for content type ' . $type . '.');

        $chunkRoot = rtrim(sys_get_temp_dir(), DIRECTORY_SEPARATOR) . DIRECTORY_SEPARATOR . 'cnr_admin_upload_chunks' . DIRECTORY_SEPARATOR;
        $sessionDir = $chunkRoot . $uploadId . DIRECTORY_SEPARATOR;
        if (!is_dir($sessionDir) && !mkdir($sessionDir, 0700, true) && !is_dir($sessionDir)) api_fail('Could not create chunk upload session.', 500);
        $metaPath = $sessionDir . 'meta.json';
        $meta = ['id' => $id, 'type' => $type, 'count' => $chunkCount, 'ext' => $ext];
        if (is_file($metaPath)) {
            $existingMeta = json_decode((string)file_get_contents($metaPath), true);
            if (!is_array($existingMeta) || $existingMeta !== $meta) api_fail('upload_id is already being used for a different upload.', 409);
        } else {
            if (file_put_contents($metaPath, json_encode($meta), LOCK_EX) === false) api_fail('Could not initialize chunk upload session.', 500);
        }

        $partPath = $sessionDir . sprintf('%06d.part', $chunkIndex);
        if (!move_uploaded_file((string)$file['tmp_name'], $partPath)) api_fail('Could not store uploaded chunk.', 500);

        $received = 0;
        $totalBytes = 0;
        for ($i = 0; $i < $chunkCount; $i++) {
            $candidate = $sessionDir . sprintf('%06d.part', $i);
            if (!is_file($candidate)) continue;
            $received++;
            $totalBytes += (int)filesize($candidate);
        }
        if ($totalBytes > 128 * 1024 * 1024) api_fail('Chunked upload exceeds the 128 MB API safety limit.', 413);
        if ($received < $chunkCount) {
            api_ok(['content_id' => $id, 'upload_id' => $uploadId, 'received_chunks' => $received, 'chunk_count' => $chunkCount, 'received_bytes' => $totalBytes, 'completed' => false]);
        }

        $bucket = api_storage_bucket($type);
        $dir = __DIR__ . '/../uploads/' . $bucket . '/';
        if (!is_dir($dir) && !mkdir($dir, 0755, true) && !is_dir($dir)) api_fail('Could not create upload directory.', 500);
        $tempDest = $dir . '.' . $id . '.' . $uploadId . '.uploading';
        $out = @fopen($tempDest, 'wb');
        if (!$out) api_fail('Could not create assembled upload file.', 500);
        $assembledBytes = 0;
        try {
            for ($i = 0; $i < $chunkCount; $i++) {
                $candidate = $sessionDir . sprintf('%06d.part', $i);
                if (!is_file($candidate)) throw new RuntimeException('Upload chunk disappeared during assembly.');
                $in = @fopen($candidate, 'rb');
                if (!$in) throw new RuntimeException('Could not read upload chunk.');
                $copied = stream_copy_to_stream($in, $out);
                fclose($in);
                if ($copied === false) throw new RuntimeException('Could not assemble upload chunk.');
                $assembledBytes += (int)$copied;
                if ($assembledBytes > 128 * 1024 * 1024) throw new RuntimeException('Assembled upload exceeds the 128 MB API safety limit.');
            }
            fclose($out);
        } catch (Throwable $e) {
            if (is_resource($out)) fclose($out);
            @unlink($tempDest);
            api_fail($e->getMessage(), 500);
        }

        foreach (api_allowed_extensions($type) as $oldExt) {
            $old = $dir . $id . '.' . $oldExt;
            if (is_file($old)) @unlink($old);
        }
        $dest = $dir . $id . '.' . $ext;
        if (!@rename($tempDest, $dest)) {
            @unlink($dest);
            if (!@rename($tempDest, $dest)) api_fail('Could not finalize assembled upload.', 500);
        }
        $hash = md5_file($dest);
        if ($hash === false) api_fail('Could not hash assembled upload.', 500);
        $stored = [
            'path' => $dest,
            'hash' => strtolower($hash),
            'url' => api_public_base() . '/uploads/' . rawurlencode($bucket) . '/' . rawurlencode($id . '.' . $ext),
            'size' => filesize($dest),
            'extension' => $ext,
        ];

        $name = array_key_exists('cname', $params) || array_key_exists('name', $params)
            ? trim((string)($params['cname'] ?? $params['name']))
            : (string)($existing['name'] ?? $id);
        $baseScene = array_key_exists('base_scene', $params) ? trim((string)$params['base_scene']) : (string)($existing['base_scene'] ?? 'FreeRun3_1');
        $material = array_key_exists('material_name', $params) ? trim((string)$params['material_name']) : (string)($existing['material_name'] ?? '');
        $dataKey = array_key_exists('data_key', $params) ? trim((string)$params['data_key']) : (string)($existing['data_key'] ?? '');
        $sort = array_key_exists('sort_order', $params) || array_key_exists('price', $params)
            ? (int)($params['sort_order'] ?? $params['price'])
            : (int)($existing['sort_order'] ?? 0);
        $enabled = array_key_exists('enabled', $params) ? (api_bool($params['enabled'], true) ? 1 : 0) : (int)($existing['enabled'] ?? 1);

        if ($existing) {
            $pdo->prepare('UPDATE content_items SET type=?,name=?,url=?,base_scene=?,material_name=?,data_key=?,sort_order=?,enabled=?,file_hash=? WHERE id=?')
                ->execute([$type,$name,$stored['url'],$baseScene,$material,$dataKey,$sort,$enabled,$stored['hash'],$id]);
        } else {
            $pdo->prepare('INSERT INTO content_items (id,type,name,url,base_scene,material_name,data_key,sort_order,enabled,created_at,file_hash) VALUES (?,?,?,?,?,?,?,?,?,?,?)')
                ->execute([$id,$type,$name,$stored['url'],$baseScene,$material,$dataKey,$sort,$enabled,time(),$stored['hash']]);
        }

        for ($i = 0; $i < $chunkCount; $i++) @unlink($sessionDir . sprintf('%06d.part', $i));
        @unlink($metaPath);
        @rmdir($sessionDir);
        api_ok(['content' => api_content_row($pdo, $id), 'file' => $stored, 'replaced' => (bool)$existing,
            'upload_id' => $uploadId, 'received_chunks' => $chunkCount, 'chunk_count' => $chunkCount, 'completed' => true]);
        break;

    case 'upload_content':
        $file = api_require_upload('file');
        $type = api_content_type($params['ctype'] ?? $params['type'] ?? '');
        $rawId = trim((string)($params['content_id'] ?? $params['id'] ?? ''));
        if ($rawId === '') $rawId = (string)pathinfo((string)$file['name'], PATHINFO_FILENAME);
        $id = api_content_id($rawId);
        $existing = api_content_row($pdo, $id);
        $replace = api_bool($params['replace'] ?? null, false);
        if ($existing && !$replace) api_fail('Content ID already exists. Re-run with replace=1 to replace it.', 409);

        $stored = api_store_content_upload($file, $type, $id);
        $name = array_key_exists('cname', $params) || array_key_exists('name', $params)
            ? trim((string)($params['cname'] ?? $params['name']))
            : (string)($existing['name'] ?? $id);
        $baseScene = array_key_exists('base_scene', $params) ? trim((string)$params['base_scene']) : (string)($existing['base_scene'] ?? 'FreeRun3_1');
        $material = array_key_exists('material_name', $params) ? trim((string)$params['material_name']) : (string)($existing['material_name'] ?? '');
        $dataKey = array_key_exists('data_key', $params) ? trim((string)$params['data_key']) : (string)($existing['data_key'] ?? '');
        $sort = array_key_exists('sort_order', $params) || array_key_exists('price', $params)
            ? (int)($params['sort_order'] ?? $params['price'])
            : (int)($existing['sort_order'] ?? 0);
        $enabled = array_key_exists('enabled', $params) ? (api_bool($params['enabled'], true) ? 1 : 0) : (int)($existing['enabled'] ?? 1);

        if ($existing) {
            $pdo->prepare('UPDATE content_items SET type=?,name=?,url=?,base_scene=?,material_name=?,data_key=?,sort_order=?,enabled=?,file_hash=? WHERE id=?')
                ->execute([$type,$name,$stored['url'],$baseScene,$material,$dataKey,$sort,$enabled,$stored['hash'],$id]);
        } else {
            $pdo->prepare('INSERT INTO content_items (id,type,name,url,base_scene,material_name,data_key,sort_order,enabled,created_at,file_hash) VALUES (?,?,?,?,?,?,?,?,?,?,?)')
                ->execute([$id,$type,$name,$stored['url'],$baseScene,$material,$dataKey,$sort,$enabled,time(),$stored['hash']]);
        }

        $thumb = null;
        if (isset($_FILES['thumb_file']) && (int)$_FILES['thumb_file']['error'] === UPLOAD_ERR_OK && in_array($type, ['map','dlcmap'], true)) {
            $thumb = api_store_thumbnail($pdo, $id, 'thumb_file');
        }
        api_ok(['content' => api_content_row($pdo, $id), 'file' => $stored, 'thumbnail' => $thumb, 'replaced' => (bool)$existing]);
        break;

    case 'upload_thumbnail':
    case 'upload_thumb':
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        $row = api_content_row($pdo, $id);
        if (!$row) api_fail('Content item not found.', 404);
        if (!in_array($row['type'], ['map','dlcmap'], true)) api_fail('Thumbnails are only supported for map and dlcmap items.');
        api_ok(['content_id' => $id, 'thumbnail' => api_store_thumbnail($pdo, $id, 'thumb_file')]);
        break;

    case 'toggle_content':
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        if (!api_content_row($pdo, $id)) api_fail('Content item not found.', 404);
        $pdo->prepare('UPDATE content_items SET enabled = 1 - enabled WHERE id = ?')->execute([$id]);
        api_ok(['content' => api_content_row($pdo, $id)]);
        break;

    case 'set_content_enabled':
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        if (!api_content_row($pdo, $id)) api_fail('Content item not found.', 404);
        $enabled = api_bool($params['enabled'] ?? null, true) ? 1 : 0;
        $pdo->prepare('UPDATE content_items SET enabled = ? WHERE id = ?')->execute([$enabled,$id]);
        api_ok(['content' => api_content_row($pdo, $id)]);
        break;

    case 'delete_content':
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        if (!api_content_row($pdo, $id)) api_fail('Content item not found.', 404);
        $pdo->prepare('DELETE FROM content_items WHERE id = ?')->execute([$id]);
        api_ok(['deleted' => $id]);
        break;

    case 'reorder_content':
    case 'set_price':
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        if (!api_content_row($pdo, $id)) api_fail('Content item not found.', 404);
        $sort = (int)($params['sort_order'] ?? $params['price'] ?? 0);
        $pdo->prepare('UPDATE content_items SET sort_order = ? WHERE id = ?')->execute([$sort,$id]);
        api_ok(['content' => api_content_row($pdo, $id)]);
        break;

    case 'update_hash':
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        if (!api_content_row($pdo, $id)) api_fail('Content item not found.', 404);
        $field = strtolower(trim((string)($params['field'] ?? 'file_hash')));
        if (!in_array($field, ['file_hash','thumbnail_hash'], true)) api_fail('field must be file_hash or thumbnail_hash.');
        $hash = strtolower(preg_replace('/[^a-f0-9]/i', '', trim((string)($params['file_hash'] ?? $params['hash'] ?? ''))));
        if (strlen($hash) !== 32) api_fail('hash must be a 32-character MD5 hash.');
        $pdo->prepare('UPDATE content_items SET ' . $field . ' = ? WHERE id = ?')->execute([$hash,$id]);
        api_ok(['content' => api_content_row($pdo, $id)]);
        break;

    case 'sync_hash':
    case 'calc_hash':
        $id = api_content_id($params['content_id'] ?? $params['id'] ?? '');
        $row = api_content_row($pdo, $id);
        if (!$row) api_fail('Content item not found.', 404);
        $field = strtolower(trim((string)($params['field'] ?? 'file_hash')));
        if (!in_array($field, ['file_hash','thumbnail_hash'], true)) api_fail('field must be file_hash or thumbnail_hash.');
        $url = trim((string)($params['url'] ?? ($field === 'thumbnail_hash' ? ($row['thumbnail_url'] ?? '') : ($row['url'] ?? ''))));
        if ($url === '') api_fail('No URL is available for the selected hash field.');
        $hash = md5(api_fetch_url_bytes($url));
        $pdo->prepare('UPDATE content_items SET ' . $field . ' = ? WHERE id = ?')->execute([$hash,$id]);
        api_ok(['content_id' => $id, 'field' => $field, 'url' => $url, 'hash' => $hash]);
        break;

    default:
        api_fail('Unknown action: ' . $action . '.', 404);
}
