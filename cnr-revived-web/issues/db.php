<?php
// db.php — shared database bootstrap. Include this file; never expose it directly.
if (!defined('CNR_ISSUES')) { http_response_code(403); exit; }

define('DB_PATH',   __DIR__ . '/issues.db');
define('MODS_JSON', __DIR__ . '/../mods/repo.json');
define('BASE_URL',  'https://play.jacqueb.me/issues');
define('FROM_EMAIL','noreply@jacqueb.me');
define('SITE_NAME', 'CNR Revival — Issue Tracker');

function db(): SQLite3 {
    static $db = null;
    if ($db) return $db;
    $db = new SQLite3(DB_PATH);
    $db->enableExceptions(true);
    $db->exec('PRAGMA journal_mode=WAL');
    $db->exec('PRAGMA foreign_keys=ON');
    _init_schema($db);
    return $db;
}

function _init_schema(SQLite3 $db): void {
    $db->exec(<<<SQL
    CREATE TABLE IF NOT EXISTS issues (
        id              TEXT    PRIMARY KEY,
        number          INTEGER NOT NULL,
        title           TEXT    NOT NULL,
        body            TEXT    NOT NULL,
        status          TEXT    NOT NULL DEFAULT 'open',
        creator_token   TEXT    NOT NULL,
        creator_email   TEXT,
        related_mod     TEXT,
        related_version TEXT,
        created_at      INTEGER NOT NULL,
        updated_at      INTEGER NOT NULL
    );
    CREATE TABLE IF NOT EXISTS issue_seq (
        id  INTEGER PRIMARY KEY,
        seq INTEGER NOT NULL DEFAULT 0
    );
    INSERT OR IGNORE INTO issue_seq (id, seq) VALUES (1, 0);
    CREATE TABLE IF NOT EXISTS issue_tags (
        issue_id TEXT NOT NULL,
        tag      TEXT NOT NULL,
        PRIMARY KEY (issue_id, tag),
        FOREIGN KEY (issue_id) REFERENCES issues(id) ON DELETE CASCADE
    );
    CREATE TABLE IF NOT EXISTS issue_history (
        id        INTEGER PRIMARY KEY AUTOINCREMENT,
        issue_id  TEXT    NOT NULL,
        old_title TEXT    NOT NULL,
        new_title TEXT    NOT NULL,
        old_body  TEXT    NOT NULL,
        new_body  TEXT    NOT NULL,
        edited_at INTEGER NOT NULL,
        FOREIGN KEY (issue_id) REFERENCES issues(id) ON DELETE CASCADE
    );
    CREATE TABLE IF NOT EXISTS comments (
        id           TEXT    PRIMARY KEY,
        issue_id     TEXT    NOT NULL,
        body         TEXT    NOT NULL,
        author_token TEXT    NOT NULL,
        author_email TEXT,
        created_at   INTEGER NOT NULL,
        updated_at   INTEGER NOT NULL,
        FOREIGN KEY (issue_id) REFERENCES issues(id) ON DELETE CASCADE
    );
    CREATE TABLE IF NOT EXISTS comment_history (
        id         INTEGER PRIMARY KEY AUTOINCREMENT,
        comment_id TEXT    NOT NULL,
        old_body   TEXT    NOT NULL,
        new_body   TEXT    NOT NULL,
        edited_at  INTEGER NOT NULL
    );
    CREATE TABLE IF NOT EXISTS subscribers (
        id            TEXT    PRIMARY KEY,
        issue_id      TEXT    NOT NULL,
        email         TEXT    NOT NULL,
        opt_out_token TEXT    NOT NULL,
        created_at    INTEGER NOT NULL,
        UNIQUE (issue_id, email),
        FOREIGN KEY (issue_id) REFERENCES issues(id) ON DELETE CASCADE
    );
    CREATE TABLE IF NOT EXISTS attachments (
        id         TEXT    PRIMARY KEY,
        issue_id   TEXT,
        comment_id TEXT,
        filename   TEXT    NOT NULL,
        mime       TEXT    NOT NULL,
        size       INTEGER NOT NULL DEFAULT 0,
        created_at INTEGER NOT NULL DEFAULT 0,
        FOREIGN KEY (issue_id)   REFERENCES issues(id)   ON DELETE CASCADE,
        FOREIGN KEY (comment_id) REFERENCES comments(id) ON DELETE CASCADE
    );
    SQL);
    // Migrations: add columns to existing tables if they don't exist yet.
    try { $db->exec('ALTER TABLE comments ADD COLUMN is_answer INTEGER NOT NULL DEFAULT 0'); } catch (Throwable $e) {}
    try { $db->exec('ALTER TABLE comments ADD COLUMN is_diff   INTEGER NOT NULL DEFAULT 0'); } catch (Throwable $e) {}
}

// ── Helpers ────────────────────────────────────────────────────────────────

function gen_id(): string    { return bin2hex(random_bytes(12)); }
function gen_token(): string { return bin2hex(random_bytes(16)); }
function now(): int          { return time(); }

function next_issue_number(): int {
    $db = db();
    $db->exec('UPDATE issue_seq SET seq = seq + 1 WHERE id = 1');
    $row = $db->querySingle('SELECT seq FROM issue_seq WHERE id = 1');
    return (int)$row;
}

function sanitize_tags(array $tags): array {
    $out = [];
    foreach ($tags as $t) {
        $t = mb_strtolower(trim(preg_replace('/[^a-zA-Z0-9 _\-]/', '', $t)));
        if ($t !== '' && mb_strlen($t) <= 32) $out[] = $t;
    }
    return array_unique(array_slice($out, 0, 10));
}

function validate_email(string $e): bool {
    return filter_var($e, FILTER_VALIDATE_EMAIL) !== false && mb_strlen($e) <= 255;
}

// ── Mod list (from repo.json) ──────────────────────────────────────────────

function get_mods(): array {
    if (!file_exists(MODS_JSON)) return [];
    $data = json_decode(file_get_contents(MODS_JSON), true);
    if (!$data || !isset($data['mods'])) return [];
    $out = [];
    foreach ($data['mods'] as $m) {
        $versions = [];
        if (!empty($m['versions'])) {
            foreach ($m['versions'] as $v) $versions[] = $v['version'];
        }
        $out[] = [
            'id'      => $m['id'],
            'name'    => $m['name'],
            'versions'=> $versions,
        ];
    }
    return $out;
}

// ── Email notifications ────────────────────────────────────────────────────

function notify_subscribers(string $issue_id, string $subject, string $html_body, ?string $skip_email = null): void {
    $db   = db();
    $stmt = $db->prepare('SELECT email, opt_out_token FROM subscribers WHERE issue_id = :iid');
    $stmt->bindValue(':iid', $issue_id);
    $res  = $stmt->execute();
    while ($row = $res->fetchArray(SQLITE3_ASSOC)) {
        if ($skip_email && strtolower($row['email']) === strtolower($skip_email)) continue;
        send_notification($row['email'], $subject, $html_body, $issue_id, $row['opt_out_token']);
    }
}

function send_notification(string $to, string $subject, string $html, string $issue_id, string $opt_out_token): void {
    $unsub_url = BASE_URL . '/unsubscribe.php?token=' . urlencode($opt_out_token);
    $footer = '<br><br><hr style="border:none;border-top:1px solid #333"><p style="font-size:12px;color:#777">'
            . 'You\'re receiving this because you subscribed to updates for issue #' . htmlspecialchars($issue_id) . ' on ' . SITE_NAME . '.<br>'
            . '<a href="' . $unsub_url . '">Unsubscribe</a></p>';
    $message = '<html><body style="font-family:system-ui,sans-serif;background:#0f1720;color:#e6eef8;padding:24px">'
             . $html . $footer . '</body></html>';
    $headers  = "MIME-Version: 1.0\r\n"
              . "Content-Type: text/html; charset=UTF-8\r\n"
              . "From: " . SITE_NAME . " <" . FROM_EMAIL . ">\r\n"
              . "X-Mailer: PHP/" . phpversion();
    @mail($to, '[CNR Issues] ' . $subject, $message, $headers);
}

function ensure_subscriber(string $issue_id, string $email): void {
    if (!validate_email($email)) return;
    $db   = db();
    $stmt = $db->prepare('SELECT id FROM subscribers WHERE issue_id=:iid AND email=:email');
    $stmt->bindValue(':iid',   $issue_id);
    $stmt->bindValue(':email', strtolower($email));
    if ($stmt->execute()->fetchArray()) return; // already subscribed
    $stmt2 = $db->prepare(
        'INSERT OR IGNORE INTO subscribers (id, issue_id, email, opt_out_token, created_at)
         VALUES (:id, :iid, :email, :tok, :ts)');
    $stmt2->bindValue(':id',    gen_id());
    $stmt2->bindValue(':iid',   $issue_id);
    $stmt2->bindValue(':email', strtolower($email));
    $stmt2->bindValue(':tok',   gen_token());
    $stmt2->bindValue(':ts',    now());
    $stmt2->execute();
}

// ── Issue fetch helpers ────────────────────────────────────────────────────

function get_issue_tags(string $issue_id): array {
    $db   = db();
    $stmt = $db->prepare('SELECT tag FROM issue_tags WHERE issue_id=:id ORDER BY tag');
    $stmt->bindValue(':id', $issue_id);
    $res  = $stmt->execute();
    $tags = [];
    while ($row = $res->fetchArray(SQLITE3_ASSOC)) $tags[] = $row['tag'];
    return $tags;
}

function get_issue_row(string $id): ?array {
    $db   = db();
    $stmt = $db->prepare('SELECT * FROM issues WHERE id=:id');
    $stmt->bindValue(':id', $id);
    $row  = $stmt->execute()->fetchArray(SQLITE3_ASSOC);
    if (!$row) return null;
    $row['tags'] = get_issue_tags($id);
    return $row;
}
