<?php
// api.php — JSON REST API for the CNR Issues tracker.
define('CNR_ISSUES', 1);
require_once __DIR__ . '/db.php';
require_once __DIR__ . '/config.php';

session_start();
header('Content-Type: application/json; charset=utf-8');
header('X-Content-Type-Options: nosniff');

// CORS: allow same-origin browser requests; admin key requests have no origin
$origin = $_SERVER['HTTP_ORIGIN'] ?? '';
if ($origin === 'https://play.jacqueb.me') {
    header('Access-Control-Allow-Origin: https://play.jacqueb.me');
}
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(204); exit;
}

$action = $_GET['action'] ?? '';
$method = $_SERVER['REQUEST_METHOD'];

// Read JSON body for POST requests
$body = [];
if ($method === 'POST') {
    $raw = file_get_contents('php://input');
    $body = json_decode($raw, true) ?? [];
}

try {
    switch ($action) {

        // ── READ ──────────────────────────────────────────────────────────

        case 'list_issues':
            echo json_encode(api_list_issues());
            break;

        case 'get_issue':
            $id = trim($_GET['id'] ?? '');
            if (!$id) json_error('Missing id', 400);
            echo json_encode(api_get_issue($id));
            break;

        case 'get_mods':
            echo json_encode(['mods' => get_mods()]);
            break;

        // ── WRITE ─────────────────────────────────────────────────────────

        case 'create_issue':
            require_post();
            echo json_encode(api_create_issue($body));
            break;

        case 'edit_issue':
            require_post();
            echo json_encode(api_edit_issue($body));
            break;

        case 'delete_issue':
            require_post();
            echo json_encode(api_delete_issue($body));
            break;

        case 'set_status':
            require_post();
            echo json_encode(api_set_status($body));
            break;

        case 'upload_attachment':
            echo json_encode(api_upload_attachment());
            break;

        case 'delete_attachment':
            require_post();
            if (!is_admin()) json_error('Admin required', 403);
            echo json_encode(api_delete_attachment($body));
            break;

        case 'add_comment':
            require_post();
            echo json_encode(api_add_comment($body));
            break;

        case 'edit_comment':
            require_post();
            echo json_encode(api_edit_comment($body));
            break;

        case 'delete_comment':
            require_post();
            echo json_encode(api_delete_comment($body));
            break;

        case 'mark_answer':
            require_post();
            if (!is_admin()) json_error('Admin required', 403);
            echo json_encode(api_mark_answer($body));
            break;

        case 'subscribe':
            require_post();
            echo json_encode(api_subscribe($body));
            break;

        case 'unsubscribe':
            $token = trim($_GET['token'] ?? '');
            echo json_encode(api_unsubscribe($token));
            break;

        // ── ADMIN ─────────────────────────────────────────────────────────

        case 'admin_get_token':
            if (!is_admin()) json_error('Unauthorized', 403);
            $id = trim($_GET['id'] ?? '');
            if (!$id) json_error('Missing id', 400);
            echo json_encode(api_admin_get_token($id));
            break;

        case 'admin_check':
            echo json_encode(['admin' => is_admin()]);
            break;

        case 'admin_list':
            if (!is_admin()) json_error('Unauthorized', 403);
            echo json_encode(api_admin_list());
            break;

        default:
            json_error('Unknown action', 400);
    }
} catch (Throwable $e) {
    json_error('Server error: ' . $e->getMessage(), 500);
}

// ── Helpers ────────────────────────────────────────────────────────────────

function is_admin(): bool {
    // API key (server-to-server via X-API-Key header or ?key= param)
    $key = $_SERVER['HTTP_X_API_KEY'] ?? ($_GET['key'] ?? '');
    if (defined('ADMIN_API_KEY') && ADMIN_API_KEY !== '' && hash_equals(ADMIN_API_KEY, (string)$key)) {
        return true;
    }
    // Browser session (set by login.php)
    return !empty($_SESSION['cnr_admin']);
}

function require_post(): void {
    if ($_SERVER['REQUEST_METHOD'] !== 'POST') json_error('POST required', 405);
}

function json_error(string $msg, int $code = 400): never {
    http_response_code($code);
    echo json_encode(['error' => $msg]);
    exit;
}

function ok(array $data = []): array {
    return array_merge(['ok' => true], $data);
}

function require_field(array $body, string $key, int $maxlen = 0): string {
    $v = trim($body[$key] ?? '');
    if ($v === '') json_error("Missing field: $key", 400);
    if ($maxlen > 0 && mb_strlen($v) > $maxlen) json_error("$key too long (max $maxlen)", 400);
    return $v;
}

// ── API handlers ───────────────────────────────────────────────────────────

function api_list_issues(): array {
    $db = db();

    $status = $_GET['status'] ?? 'open';
    $mod    = trim($_GET['mod']    ?? '');
    $tag    = trim($_GET['tag']    ?? '');
    $q      = trim($_GET['q']      ?? '');

    $where = [];
    $params = [];

    if ($status && $status !== 'all') {
        if ($status === 'open') {
            $where[] = "i.status IN ('open', 'confirmed', 'wip')";
        } else {
            $where[]  = 'i.status = :status';
            $params[':status'] = $status;
        }
    }
    if ($mod) {
        $where[]  = 'i.related_mod = :mod';
        $params[':mod'] = $mod;
    }
    if ($q) {
        $where[]  = '(i.title LIKE :q OR i.body LIKE :q)';
        $params[':q'] = '%' . $q . '%';
    }

    $whereSQL = $where ? 'WHERE ' . implode(' AND ', $where) : '';

    // If filtering by tag, do a subquery
    if ($tag) {
        $whereSQL = ($whereSQL ? $whereSQL . ' AND ' : 'WHERE ')
                  . 'i.id IN (SELECT issue_id FROM issue_tags WHERE tag=:tag)';
        $params[':tag'] = $tag;
    }

    $sql = "SELECT i.id, i.number, i.title, i.status, i.related_mod, i.related_version,
                   i.created_at, i.updated_at,
                   (SELECT COUNT(*) FROM comments c WHERE c.issue_id = i.id) AS comment_count
            FROM issues i $whereSQL ORDER BY i.number DESC";

    $stmt = $db->prepare($sql);
    foreach ($params as $k => $v) $stmt->bindValue($k, $v);
    $res  = $stmt->execute();

    $issues = [];
    while ($row = $res->fetchArray(SQLITE3_ASSOC)) {
        $row['tags']   = get_issue_tags($row['id']);
        $issues[] = $row;
    }

    // Get all unique tags for filter UI
    $tagRes   = $db->query('SELECT DISTINCT tag FROM issue_tags ORDER BY tag');
    $allTags  = [];
    while ($r = $tagRes->fetchArray(SQLITE3_ASSOC)) $allTags[] = $r['tag'];

    return ['issues' => $issues, 'all_tags' => $allTags];
}

function api_get_issue(string $id): array {
    $issue = get_issue_row($id);
    if (!$issue) json_error('Issue not found', 404);

    $db = db();

    // Comments
    $stmt = $db->prepare(
        'SELECT id, body, is_answer, is_diff, created_at, updated_at FROM comments WHERE issue_id=:id ORDER BY created_at ASC');
    $stmt->bindValue(':id', $id);
    $res  = $stmt->execute();
    $comments = [];
    while ($row = $res->fetchArray(SQLITE3_ASSOC)) $comments[] = $row;

    // Edit history
    $stmt2 = $db->prepare(
        'SELECT id, old_title, new_title, old_body, new_body, edited_at
         FROM issue_history WHERE issue_id=:id ORDER BY edited_at ASC');
    $stmt2->bindValue(':id', $id);
    $res2  = $stmt2->execute();
    $history = [];
    while ($row = $res2->fetchArray(SQLITE3_ASSOC)) $history[] = $row;

    // Comment edit history (keyed by comment id)
    $commentHistory = [];
    foreach ($comments as $c) {
        $s = $db->prepare(
            'SELECT old_body, new_body, edited_at FROM comment_history WHERE comment_id=:id ORDER BY edited_at ASC');
        $s->bindValue(':id', $c['id']);
        $r = $s->execute();
        $ch = [];
        while ($row = $r->fetchArray(SQLITE3_ASSOC)) $ch[] = $row;
        if ($ch) $commentHistory[$c['id']] = $ch;
    }

    // Subscriber count (no emails exposed)
    $cntStmt = $db->prepare('SELECT COUNT(*) FROM subscribers WHERE issue_id=:id');
    $cntStmt->bindValue(':id', $id);
    $cnt = $cntStmt->execute()->fetchArray(SQLITE3_NUM)[0] ?? 0;

    // Attachments for issue and each comment
    $issue['attachments'] = get_attachments_for_issue($id);
    foreach ($comments as &$c) {
        $c['attachments'] = get_attachments_for_comment($c['id']);
    }
    unset($c);

    return [
        'issue'           => $issue,
        'comments'        => $comments,
        'history'         => $history,
        'comment_history' => $commentHistory,
        'subscriber_count'=> (int)$cnt,
    ];
}

function api_create_issue(array $body): array {
    $title   = require_field($body, 'title', 200);
    $text    = require_field($body, 'body',  20000);
    $email   = isset($body['email']) ? trim($body['email']) : '';
    $relMod  = trim($body['related_mod']     ?? '');
    $relVer  = trim($body['related_version'] ?? '');
    $tags    = sanitize_tags((array)($body['tags'] ?? []));

    if ($email && !validate_email($email)) json_error('Invalid email', 400);

    $db    = db();
    $id    = gen_id();
    $token = gen_token();
    $num   = next_issue_number();
    $ts    = now();

    $stmt = $db->prepare(
        'INSERT INTO issues (id,number,title,body,status,creator_token,creator_email,related_mod,related_version,created_at,updated_at)
         VALUES (:id,:num,:title,:body,"open",:tok,:email,:mod,:ver,:ts,:ts)');
    $stmt->bindValue(':id',    $id);
    $stmt->bindValue(':num',   $num);
    $stmt->bindValue(':title', $title);
    $stmt->bindValue(':body',  $text);
    $stmt->bindValue(':tok',   $token);
    $stmt->bindValue(':email', $email ?: null);
    $stmt->bindValue(':mod',   $relMod ?: null);
    $stmt->bindValue(':ver',   $relVer ?: null);
    $stmt->bindValue(':ts',    $ts);
    $stmt->execute();

    foreach ($tags as $tag) {
        $s = $db->prepare('INSERT OR IGNORE INTO issue_tags (issue_id, tag) VALUES (:iid, :tag)');
        $s->bindValue(':iid', $id);
        $s->bindValue(':tag', $tag);
        $s->execute();
    }

    // Auto-subscribe creator
    if ($email) ensure_subscriber($id, $email);

    return ok(['id' => $id, 'number' => $num, 'token' => $token]);
}

function api_edit_issue(array $body): array {
    $id    = require_field($body, 'id');
    $token = trim($body['token'] ?? '');
    $title = require_field($body, 'title', 200);
    $text  = require_field($body, 'body',  20000);
    $relMod  = trim($body['related_mod']     ?? '');
    $relVer  = trim($body['related_version'] ?? '');
    $tags    = sanitize_tags((array)($body['tags'] ?? []));

    $db    = db();
    $issue = get_issue_row($id);
    if (!$issue) json_error('Issue not found', 404);
    if (!is_admin() && !hash_equals($issue['creator_token'], $token)) json_error('Unauthorized', 403);

    $ts = now();

    // Store history if title or body changed
    if ($issue['title'] !== $title || $issue['body'] !== $text) {
        $h = $db->prepare(
            'INSERT INTO issue_history (issue_id,old_title,new_title,old_body,new_body,edited_at)
             VALUES (:iid,:ot,:nt,:ob,:nb,:ts)');
        $h->bindValue(':iid', $id);
        $h->bindValue(':ot',  $issue['title']);
        $h->bindValue(':nt',  $title);
        $h->bindValue(':ob',  $issue['body']);
        $h->bindValue(':nb',  $text);
        $h->bindValue(':ts',  $ts);
        $h->execute();
    }

    $stmt = $db->prepare(
        'UPDATE issues SET title=:title,body=:body,related_mod=:mod,related_version=:ver,updated_at=:ts WHERE id=:id');
    $stmt->bindValue(':title', $title);
    $stmt->bindValue(':body',  $text);
    $stmt->bindValue(':mod',   $relMod ?: null);
    $stmt->bindValue(':ver',   $relVer ?: null);
    $stmt->bindValue(':ts',    $ts);
    $stmt->bindValue(':id',    $id);
    $stmt->execute();

    // Replace tags
    $db->prepare('DELETE FROM issue_tags WHERE issue_id=:id')
       ->execute() ?: null;
    $del = $db->prepare('DELETE FROM issue_tags WHERE issue_id=:id');
    $del->bindValue(':id', $id);
    $del->execute();
    foreach ($tags as $tag) {
        $s = $db->prepare('INSERT OR IGNORE INTO issue_tags (issue_id, tag) VALUES (:iid, :tag)');
        $s->bindValue(':iid', $id);
        $s->bindValue(':tag', $tag);
        $s->execute();
    }

    // Notify subscribers
    if ($issue['title'] !== $title || $issue['body'] !== $text) {
        $subject = "Issue #$issue[number] edited: " . htmlspecialchars($title);
        $html    = "<h3>Issue #$issue[number] was edited</h3>"
                 . "<p><b>Title:</b> " . htmlspecialchars($title) . "</p>"
                 . "<p><a href='" . BASE_URL . "/view.php?id=" . urlencode($id) . "'>View issue</a></p>";
        notify_subscribers($id, "Issue #$issue[number] edited", $html, $issue['creator_email'] ?? null);
    }

    return ok();
}

function api_delete_issue(array $body): array {
    $id    = require_field($body, 'id');
    $token = trim($body['token'] ?? '');

    $db    = db();
    $issue = get_issue_row($id);
    if (!$issue) json_error('Issue not found', 404);
    if (!is_admin() && !hash_equals($issue['creator_token'], $token)) json_error('Unauthorized', 403);

    $stmt = $db->prepare('DELETE FROM issues WHERE id=:id');
    $stmt->bindValue(':id', $id);
    $stmt->execute();

    return ok();
}

function api_set_status(array $body): array {
    $id     = require_field($body, 'id');
    $token  = trim($body['token'] ?? '');
    $status = require_field($body, 'status');

    $validStatuses = ['open', 'confirmed', 'wip', 'resolved', 'wontfix', 'closed'];
    if (!in_array($status, $validStatuses)) json_error('Invalid status', 400);

    $db    = db();
    $issue = get_issue_row($id);
    if (!$issue) json_error('Issue not found', 404);
    if (!is_admin() && !hash_equals($issue['creator_token'], $token)) json_error('Unauthorized', 403);

    $stmt = $db->prepare('UPDATE issues SET status=:s,updated_at=:ts WHERE id=:id');
    $stmt->bindValue(':s',  $status);
    $stmt->bindValue(':ts', now());
    $stmt->bindValue(':id', $id);
    $stmt->execute();

    $labels = [
        'open'      => 'reopened',
        'confirmed' => 'confirmed as a bug',
        'wip'       => 'marked as in progress',
        'resolved'  => 'marked as resolved',
        'wontfix'   => 'marked as won\'t fix',
        'closed'    => 'closed',
    ];
    $html   = "<h3>Issue #$issue[number] was {$labels[$status]}</h3>"
            . "<p><b>" . htmlspecialchars($issue['title']) . "</b></p>"
            . "<p><a href='" . BASE_URL . "/view.php?id=" . urlencode($id) . "'>View issue</a></p>";
    notify_subscribers($id, "Issue #$issue[number] {$labels[$status]}", $html);

    return ok();
}

function api_add_comment(array $body): array {
    $issue_id = require_field($body, 'issue_id');
    $text     = trim($body['body'] ?? '');
    $email    = trim($body['email'] ?? '');
    $is_diff  = !empty($body['is_diff']) ? 1 : 0;

    // body may be empty if comment is diff-only
    if ($text === '' && !$is_diff) json_error('Missing field: body', 400);
    if (mb_strlen($text) > 20000) json_error('body too long (max 20000)', 400);
    if ($email && !validate_email($email)) json_error('Invalid email', 400);

    $db    = db();
    $issue = get_issue_row($issue_id);
    if (!$issue) json_error('Issue not found', 404);

    $id    = gen_id();
    $token = gen_token();
    $ts    = now();

    $stmt = $db->prepare(
        'INSERT INTO comments (id,issue_id,body,author_token,author_email,is_diff,created_at,updated_at)
         VALUES (:id,:iid,:body,:tok,:email,:diff,:ts,:ts)');
    $stmt->bindValue(':id',    $id);
    $stmt->bindValue(':iid',   $issue_id);
    $stmt->bindValue(':body',  $text);
    $stmt->bindValue(':tok',   $token);
    $stmt->bindValue(':email', $email ?: null);
    $stmt->bindValue(':diff',  $is_diff, SQLITE3_INTEGER);
    $stmt->bindValue(':ts',    $ts);
    $stmt->execute();

    // Auto-subscribe commenter
    if ($email) ensure_subscriber($issue_id, $email);

    // Notify existing subscribers
    $html = "<h3>New comment on Issue #$issue[number]</h3>"
          . "<p><b>" . htmlspecialchars($issue['title']) . "</b></p>"
          . "<blockquote style='border-left:3px solid #6ee7b7;padding-left:12px;color:#ccc'>"
          . nl2br(htmlspecialchars(mb_substr($text, 0, 500))) . (mb_strlen($text) > 500 ? '…' : '')
          . "</blockquote>"
          . "<p><a href='" . BASE_URL . "/view.php?id=" . urlencode($issue_id) . "'>View issue</a></p>";
    notify_subscribers($issue_id, "New comment on Issue #$issue[number]", $html, $email ?: null);

    return ok(['id' => $id, 'token' => $token]);
}

function api_mark_answer(array $body): array {
    $comment_id = require_field($body, 'comment_id');
    $db = db();

    $stmt = $db->prepare('SELECT issue_id FROM comments WHERE id=:id');
    $stmt->bindValue(':id', $comment_id);
    $row = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if (!$row) json_error('Comment not found', 404);
    $issue_id = $row['issue_id'];

    // Clear any existing answer on this issue
    $unmark = $db->prepare('UPDATE comments SET is_answer=0 WHERE issue_id=:iid');
    $unmark->bindValue(':iid', $issue_id);
    $unmark->execute();

    // Mark this comment as the answer
    $mark = $db->prepare('UPDATE comments SET is_answer=1 WHERE id=:id');
    $mark->bindValue(':id', $comment_id);
    $mark->execute();

    return ok();
}

function api_edit_comment(array $body): array {
    $id    = require_field($body, 'id');
    $token = trim($body['token'] ?? '');
    $text  = require_field($body, 'body', 20000);

    $db   = db();
    $stmt = $db->prepare('SELECT * FROM comments WHERE id=:id');
    $stmt->bindValue(':id', $id);
    $row  = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if (!$row) json_error('Comment not found', 404);
    if (!is_admin() && !hash_equals($row['author_token'], $token)) json_error('Unauthorized', 403);

    $ts = now();

    if ($row['body'] !== $text) {
        $h = $db->prepare(
            'INSERT INTO comment_history (comment_id,old_body,new_body,edited_at) VALUES (:cid,:ob,:nb,:ts)');
        $h->bindValue(':cid', $id);
        $h->bindValue(':ob',  $row['body']);
        $h->bindValue(':nb',  $text);
        $h->bindValue(':ts',  $ts);
        $h->execute();
    }

    $stmt2 = $db->prepare('UPDATE comments SET body=:body,updated_at=:ts WHERE id=:id');
    $stmt2->bindValue(':body', $text);
    $stmt2->bindValue(':ts',   $ts);
    $stmt2->bindValue(':id',   $id);
    $stmt2->execute();

    return ok();
}

function api_delete_comment(array $body): array {
    $id    = require_field($body, 'id');
    $token = trim($body['token'] ?? '');

    $db   = db();
    $stmt = $db->prepare('SELECT author_token FROM comments WHERE id=:id');
    $stmt->bindValue(':id', $id);
    $row  = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if (!$row) json_error('Comment not found', 404);
    if (!is_admin() && !hash_equals($row['author_token'], $token)) json_error('Unauthorized', 403);

    $del = $db->prepare('DELETE FROM comments WHERE id=:id');
    $del->bindValue(':id', $id);
    $del->execute();

    return ok();
}

function api_subscribe(array $body): array {
    $issue_id = require_field($body, 'issue_id');
    $email    = require_field($body, 'email', 255);

    if (!validate_email($email)) json_error('Invalid email', 400);

    $issue = get_issue_row($issue_id);
    if (!$issue) json_error('Issue not found', 404);

    ensure_subscriber($issue_id, $email);

    // Send confirmation
    $db    = db();
    $stmt  = $db->prepare('SELECT opt_out_token FROM subscribers WHERE issue_id=:iid AND email=:email');
    $stmt->bindValue(':iid',   $issue_id);
    $stmt->bindValue(':email', strtolower($email));
    $row   = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if ($row) {
        $unsub  = BASE_URL . '/unsubscribe.php?token=' . urlencode($row['opt_out_token']);
        $html   = "<h3>Subscribed to Issue #$issue[number]</h3>"
                . "<p>You'll receive email updates for: <b>" . htmlspecialchars($issue['title']) . "</b></p>"
                . "<p><a href='" . BASE_URL . "/view.php?id=" . urlencode($issue_id) . "'>View issue</a></p>"
                . "<p><a href='$unsub'>Unsubscribe</a></p>";
        send_notification($email, "Subscribed to Issue #$issue[number]", $html, $issue_id, $row['opt_out_token']);
    }

    return ok(['message' => 'Subscribed. A confirmation email has been sent.']);
}

function api_unsubscribe(string $token): array {
    if (!$token) json_error('Missing token', 400);

    $db   = db();
    $stmt = $db->prepare('SELECT id FROM subscribers WHERE opt_out_token=:tok');
    $stmt->bindValue(':tok', $token);
    $row  = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if (!$row) json_error('Token not found or already unsubscribed', 404);

    $del = $db->prepare('DELETE FROM subscribers WHERE opt_out_token=:tok');
    $del->bindValue(':tok', $token);
    $del->execute();

    return ok(['message' => 'You have been unsubscribed.']);
}

// ── Admin handlers ─────────────────────────────────────────────────────────

/**
 * Retrieve the creator/author token for any issue or comment by ID.
 * Admin-only. Useful for managing records that were created programmatically.
 */
function api_admin_get_token(string $id): array {
    $db = db();

    $stmt = $db->prepare('SELECT id, number, title, creator_token FROM issues WHERE id=:id');
    $stmt->bindValue(':id', $id);
    $row  = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if ($row) {
        return ok([
            'type'   => 'issue',
            'id'     => $row['id'],
            'number' => $row['number'],
            'title'  => $row['title'],
            'token'  => $row['creator_token'],
        ]);
    }

    $stmt2 = $db->prepare('SELECT id, issue_id, author_token FROM comments WHERE id=:id');
    $stmt2->bindValue(':id', $id);
    $row2  = $stmt2->execute()->fetchArray(SQLITE3_ASSOC);
    if ($row2) {
        return ok([
            'type'     => 'comment',
            'id'       => $row2['id'],
            'issue_id' => $row2['issue_id'],
            'token'    => $row2['author_token'],
        ]);
    }

    json_error('Not found', 404);
}

/**
 * List all issues (all statuses) with full detail, sorted by number desc.
 * Admin-only. Skips the standard status/tag/q filters.
 */
function api_admin_list(): array {
    $db = db();

    $status = trim($_GET['status'] ?? 'all');
    $where  = '';
    $params = [];

    if ($status && $status !== 'all') {
        $where = 'WHERE i.status = :status';
        $params[':status'] = $status;
    }

    $sql = "SELECT i.id, i.number, i.title, i.status, i.body,
                   i.related_mod, i.related_version,
                   i.created_at, i.updated_at,
                   (SELECT COUNT(*) FROM comments c WHERE c.issue_id = i.id) AS comment_count
            FROM issues i $where ORDER BY i.number DESC";

    $stmt = $db->prepare($sql);
    foreach ($params as $k => $v) $stmt->bindValue($k, $v);
    $res  = $stmt->execute();

    $issues = [];
    while ($row = $res->fetchArray(SQLITE3_ASSOC)) {
        $row['tags'] = get_issue_tags($row['id']);
        $issues[] = $row;
    }

    return ['issues' => $issues, 'total' => count($issues)];
}

// ── Attachment helpers ─────────────────────────────────────────────────────

function get_attachments_for_issue(string $issue_id): array {
    $db   = db();
    $stmt = $db->prepare(
        'SELECT id, filename, mime, size FROM attachments WHERE issue_id=:id AND comment_id IS NULL ORDER BY created_at');
    $stmt->bindValue(':id', $issue_id);
    $res = $stmt->execute();
    $rows = [];
    while ($r = $res->fetchArray(SQLITE3_ASSOC)) $rows[] = $r;
    return $rows;
}

function get_attachments_for_comment(string $comment_id): array {
    $db   = db();
    $stmt = $db->prepare(
        'SELECT id, filename, mime, size FROM attachments WHERE comment_id=:id ORDER BY created_at');
    $stmt->bindValue(':id', $comment_id);
    $res = $stmt->execute();
    $rows = [];
    while ($r = $res->fetchArray(SQLITE3_ASSOC)) $rows[] = $r;
    return $rows;
}

function api_upload_attachment(): array {
    $issue_id   = trim($_POST['issue_id']   ?? '');
    $comment_id = trim($_POST['comment_id'] ?? '');

    if (!$issue_id && !$comment_id) json_error('Missing issue_id or comment_id', 400);
    if ($issue_id  && $comment_id)  json_error('Specify issue_id or comment_id, not both', 400);

    $db = db();
    if ($issue_id) {
        $chk = $db->prepare('SELECT id FROM issues WHERE id=:id');
        $chk->bindValue(':id', $issue_id);
        if (!$chk->execute()->fetchArray()) json_error('Issue not found', 404);
    } else {
        $chk = $db->prepare('SELECT id FROM comments WHERE id=:id');
        $chk->bindValue(':id', $comment_id);
        if (!$chk->execute()->fetchArray()) json_error('Comment not found', 404);
    }

    if (empty($_FILES['file']) || $_FILES['file']['error'] !== UPLOAD_ERR_OK) {
        json_error('Upload failed or no file provided', 400);
    }

    $file     = $_FILES['file'];
    $origExt  = strtolower(pathinfo($file['name'] ?? '', PATHINFO_EXTENSION));

    // Validate MIME via magic bytes (not just Content-Type header)
    $finfo    = new finfo(FILEINFO_MIME_TYPE);
    $mime     = $finfo->file($file['tmp_name']);

    $id        = gen_id();
    $uploadDir = __DIR__ . '/uploads/';
    if (!is_dir($uploadDir)) mkdir($uploadDir, 0755, true);

    if ($origExt === 'cs') {
        // C# source file — must be plain text, max 2 MB
        $csTextMimes = ['text/plain', 'text/x-csrc', 'text/x-csharp', 'application/octet-stream'];
        if (!in_array($mime, $csTextMimes)) json_error('C# file must be a plain-text source file', 400);
        if ($file['size'] > 2 * 1024 * 1024) json_error('Source file too large (max 2 MB)', 400);
        $filename  = $id . '.cs';
        $storeMime = 'text/plain';
    } else {
        // Image
        $allowed = ['image/jpeg' => 'jpg', 'image/png' => 'png', 'image/gif' => 'gif', 'image/webp' => 'webp'];
        if (!array_key_exists($mime, $allowed)) json_error('Only JPEG, PNG, GIF, WebP images and .cs source files are allowed', 400);
        if ($file['size'] > 10 * 1024 * 1024) json_error('File too large (max 10 MB)', 400);
        $filename  = $id . '.' . $allowed[$mime];
        $storeMime = $mime;
    }

    if (!move_uploaded_file($file['tmp_name'], $uploadDir . $filename)) {
        json_error('Failed to save file', 500);
    }

    $stmt = $db->prepare(
        'INSERT INTO attachments (id, issue_id, comment_id, filename, mime, size, created_at)
         VALUES (:id, :iid, :cid, :fn, :mime, :size, :ts)');
    $stmt->bindValue(':id',   $id);
    if ($issue_id)   $stmt->bindValue(':iid', $issue_id);   else $stmt->bindValue(':iid', null, SQLITE3_NULL);
    if ($comment_id) $stmt->bindValue(':cid', $comment_id); else $stmt->bindValue(':cid', null, SQLITE3_NULL);
    $stmt->bindValue(':fn',   $filename);
    $stmt->bindValue(':mime', $storeMime);
    $stmt->bindValue(':size', $file['size']);
    $stmt->bindValue(':ts',   now());
    $stmt->execute();

    return ok(['attachment' => ['id' => $id, 'filename' => $filename, 'mime' => $storeMime, 'size' => $file['size']]]);
}

function api_delete_attachment(array $body): array {
    $id = require_field($body, 'id');
    $db = db();

    $stmt = $db->prepare('SELECT filename FROM attachments WHERE id=:id');
    $stmt->bindValue(':id', $id);
    $row = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if (!$row) json_error('Attachment not found', 404);

    $path = __DIR__ . '/uploads/' . $row['filename'];
    if (file_exists($path)) unlink($path);

    $del = $db->prepare('DELETE FROM attachments WHERE id=:id');
    $del->bindValue(':id', $id);
    $del->execute();

    return ok();
}
