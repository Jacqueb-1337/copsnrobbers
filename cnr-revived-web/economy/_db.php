<?php
// _db.php — shared DB connection + helpers (not publicly accessible by name convention)
// SQLite file lives one level above the web root to prevent direct download.
// Adjust DB_PATH if your host layout differs.

define('DB_PATH', __DIR__ . '/../../cnr_economy.db');
define('API_VERSION', 1);

// ---------- open / init -------------------------------------------------------
function db(): PDO {
    static $pdo = null;
    if ($pdo) return $pdo;

    $pdo = new PDO('sqlite:' . DB_PATH, null, null, [
        PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
    ]);
    // WAL mode: safe for concurrent PHP requests.
    // FK enforcement is intentionally OFF: the legacy tables (transactions,
    // wheel_spins, player_mail) carried FKs to the old 'players' table whose
    // player_id column now stores account UUIDs.  Application logic ensures integrity.
    $pdo->exec("PRAGMA journal_mode=WAL");
    init_schema($pdo);
    maybe_migrate($pdo);
    return $pdo;
}

function init_schema(PDO $pdo): void {
    $pdo->exec("
        -- Legacy single-device table (kept for migration source; not used by new code)
        CREATE TABLE IF NOT EXISTS players (
            id            TEXT    PRIMARY KEY,
            display_name  TEXT    NOT NULL DEFAULT '',
            token         TEXT    NOT NULL,
            pin_hash      TEXT    DEFAULT NULL,
            coins         INTEGER NOT NULL DEFAULT 0,
            gems          INTEGER NOT NULL DEFAULT 0,
            registered_at INTEGER NOT NULL,
            last_seen     INTEGER NOT NULL
        );

        -- Multi-device account (the authoritative identity)
        CREATE TABLE IF NOT EXISTS accounts (
            id            TEXT    PRIMARY KEY,  -- random 32-byte hex UUID
            display_name  TEXT    NOT NULL DEFAULT '',
            pin_hash      TEXT    DEFAULT NULL,
            coins         INTEGER NOT NULL DEFAULT 0,
            gems          INTEGER NOT NULL DEFAULT 0,
            registered_at INTEGER NOT NULL,
            last_seen     INTEGER NOT NULL
        );

        -- Per-device auth tokens (one account can have many devices)
        CREATE TABLE IF NOT EXISTS devices (
            android_id  TEXT    PRIMARY KEY,
            account_id  TEXT    NOT NULL,
            token       TEXT    NOT NULL,
            last_seen   INTEGER NOT NULL
        );

        -- Game progression stored per account
        CREATE TABLE IF NOT EXISTS account_progression (
            account_id      TEXT    PRIMARY KEY,
            level           INTEGER NOT NULL DEFAULT 1,
            exp             INTEGER NOT NULL DEFAULT 0,
            weapon_levels   TEXT    NOT NULL DEFAULT '{}',
            skin_unlocks    TEXT    NOT NULL DEFAULT '[]',
            armor_unlocks   TEXT    NOT NULL DEFAULT '[]',
            equipped_slots  TEXT    NOT NULL DEFAULT '[]',
            current_skin    TEXT    NOT NULL DEFAULT 'Skin_1',
            current_armor   TEXT    NOT NULL DEFAULT '',
            updated_at      INTEGER NOT NULL DEFAULT 0
        );

        -- Transactions: player_id stores account_id after migration
        CREATE TABLE IF NOT EXISTS transactions (
            id          INTEGER PRIMARY KEY AUTOINCREMENT,
            player_id   TEXT    NOT NULL,
            delta_coins INTEGER NOT NULL DEFAULT 0,
            delta_gems  INTEGER NOT NULL DEFAULT 0,
            reason      TEXT    NOT NULL DEFAULT '',
            match_id    TEXT    DEFAULT NULL,
            created_at  INTEGER NOT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS ux_tx_match
            ON transactions(player_id, match_id)
            WHERE match_id IS NOT NULL;

        -- Wheel spins: player_id stores account_id after migration
        CREATE TABLE IF NOT EXISTS wheel_spins (
            player_id    TEXT    PRIMARY KEY,
            last_spin_at INTEGER NOT NULL
        );

        -- Mail: player_id stores account_id after migration
        CREATE TABLE IF NOT EXISTS player_mail (
            id        INTEGER PRIMARY KEY AUTOINCREMENT,
            player_id TEXT    NOT NULL,
            subject   TEXT    NOT NULL DEFAULT '',
            body      TEXT    NOT NULL DEFAULT '',
            coins     INTEGER NOT NULL DEFAULT 0,
            gems      INTEGER NOT NULL DEFAULT 0,
            claimed   INTEGER NOT NULL DEFAULT 0,
            sent_at   INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS content_items (
            id            TEXT    PRIMARY KEY,
            type          TEXT    NOT NULL DEFAULT 'map',
            name          TEXT    NOT NULL DEFAULT '',
            url           TEXT    NOT NULL DEFAULT '',
            base_scene    TEXT    NOT NULL DEFAULT 'FreeRun3_1',
            material_name TEXT    NOT NULL DEFAULT '',
            data_key      TEXT    NOT NULL DEFAULT '',
            sort_order    INTEGER NOT NULL DEFAULT 0,
            enabled       INTEGER NOT NULL DEFAULT 1,
            created_at    INTEGER NOT NULL DEFAULT 0
        );
    ");
    // Add columns to existing databases (idempotent)
    try { $pdo->exec("ALTER TABLE content_items ADD COLUMN thumbnail_url TEXT NOT NULL DEFAULT ''"); } catch (Exception $e) {}
    try { $pdo->exec("ALTER TABLE content_items ADD COLUMN file_hash TEXT NOT NULL DEFAULT ''"); } catch (Exception $e) {}
    try { $pdo->exec("ALTER TABLE content_items ADD COLUMN thumbnail_hash TEXT NOT NULL DEFAULT ''"); } catch (Exception $e) {}
}

// ---------- one-time migration: players → accounts + devices ------------------
function maybe_migrate(PDO $pdo): void {
    // Run only when the legacy players table has rows but accounts is still empty
    $acct_count = (int)$pdo->query("SELECT COUNT(*) FROM accounts")->fetchColumn();
    if ($acct_count > 0) return;

    $player_count = (int)$pdo->query("SELECT COUNT(*) FROM players")->fetchColumn();
    if ($player_count === 0) return;

    $players = $pdo->query("SELECT * FROM players")->fetchAll();
    $pdo->beginTransaction();
    try {
        foreach ($players as $p) {
            $account_id = bin2hex(random_bytes(16)); // 32-char hex UUID

            $pdo->prepare("INSERT INTO accounts (id,display_name,pin_hash,coins,gems,registered_at,last_seen)
                           VALUES (?,?,?,?,?,?,?)")
                ->execute([$account_id, $p['display_name'], $p['pin_hash'],
                           $p['coins'], $p['gems'], $p['registered_at'], $p['last_seen']]);

            $pdo->prepare("INSERT INTO devices (android_id,account_id,token,last_seen)
                           VALUES (?,?,?,?)")
                ->execute([$p['id'], $account_id, $p['token'], $p['last_seen']]);

            $pdo->prepare("INSERT INTO account_progression (account_id,updated_at) VALUES (?,?)")
                ->execute([$account_id, 0]);

            // Repoint legacy transaction/spin/mail rows to the new account UUID
            $pdo->prepare("UPDATE transactions SET player_id=? WHERE player_id=?")->execute([$account_id, $p['id']]);
            $pdo->prepare("UPDATE wheel_spins  SET player_id=? WHERE player_id=?")->execute([$account_id, $p['id']]);
            $pdo->prepare("UPDATE player_mail  SET player_id=? WHERE player_id=?")->execute([$account_id, $p['id']]);
        }
        $pdo->commit();
        error_log("CNR: migrated " . count($players) . " player(s) to accounts/devices schema.");
    } catch (Exception $e) {
        $pdo->rollBack();
        error_log("CNR migration error: " . $e->getMessage());
    }
}

// ---------- response helpers -------------------------------------------------
function ok(array $data = []): never {
    header('Content-Type: application/json');
    echo json_encode(array_merge(['ok' => true, 'v' => API_VERSION], $data));
    exit;
}

function fail(string $msg, int $http = 200): never {
    http_response_code($http);
    header('Content-Type: application/json');
    echo json_encode(['ok' => false, 'error' => $msg, 'v' => API_VERSION]);
    exit;
}

// ---------- auth -------------------------------------------------------------
// Returns account row with 'id' = account_id.  All downstream code uses $player['id']
// as the authoritative account identifier for transactions, mail, etc.
function require_auth(): array {
    $android_id = trim($_POST['player_id'] ?? $_GET['player_id'] ?? '');
    $token      = trim($_POST['token']     ?? $_GET['token']     ?? '');

    if ($android_id === '' || $token === '') fail('missing player_id or token', 401);
    if (!preg_match('/^[0-9a-f]{1,64}$/i', $android_id)) fail('invalid player_id', 401);
    if (!preg_match('/^[0-9a-f]{64}$/i', $token))        fail('invalid token', 401);

    $pdo  = db();
    $stmt = $pdo->prepare("
        SELECT a.id, a.display_name, a.pin_hash, a.coins, a.gems, a.registered_at, a.last_seen
          FROM accounts a
          JOIN devices  d ON d.account_id = a.id
         WHERE d.android_id = ? AND d.token = ?
    ");
    $stmt->execute([$android_id, strtolower($token)]);
    $player = $stmt->fetch();
    if (!$player) fail('unauthorized', 401);

    $now = time();
    $pdo->prepare("UPDATE accounts SET last_seen=? WHERE id=?")->execute([$now, $player['id']]);
    $pdo->prepare("UPDATE devices  SET last_seen=? WHERE android_id=?")->execute([$now, $android_id]);

    return $player;
}

// ---------- CORS (game client sends from Android, no browser origin) ---------
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') { http_response_code(204); exit; }
