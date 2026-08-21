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

        -- Optional username/password login. Accounts remain valid guest accounts
        -- unless a row exists here for their account_id.
        CREATE TABLE IF NOT EXISTS account_logins (
            username      TEXT    PRIMARY KEY COLLATE NOCASE,
            account_id    TEXT    NOT NULL UNIQUE,
            password_hash TEXT    NOT NULL,
            created_at    INTEGER NOT NULL,
            last_login    INTEGER NOT NULL DEFAULT 0
        );
    ");
    // Add columns to existing databases (idempotent)
    try { $pdo->exec("ALTER TABLE content_items ADD COLUMN thumbnail_url TEXT NOT NULL DEFAULT ''"); } catch (Exception $e) {}
    try { $pdo->exec("ALTER TABLE content_items ADD COLUMN file_hash TEXT NOT NULL DEFAULT ''"); } catch (Exception $e) {}
    try { $pdo->exec("ALTER TABLE content_items ADD COLUMN thumbnail_hash TEXT NOT NULL DEFAULT ''"); } catch (Exception $e) {}
    try { $pdo->exec("ALTER TABLE player_mail ADD COLUMN spins INTEGER NOT NULL DEFAULT 0"); } catch (Exception $e) {}
    try { $pdo->exec("ALTER TABLE wheel_spins ADD COLUMN bonus_spins INTEGER NOT NULL DEFAULT 0"); } catch (Exception $e) {}
}

function account_stats(PDO $pdo, string $account_id): array {
    $stmt = $pdo->prepare("SELECT id, display_name, coins, gems, registered_at, last_seen FROM accounts WHERE id=?");
    $stmt->execute([$account_id]);
    $acct = $stmt->fetch();
    if (!$acct) return [];

    $stmt = $pdo->prepare("SELECT * FROM account_progression WHERE account_id=?");
    $stmt->execute([$account_id]);
    $prog = $stmt->fetch();

    $owned = 0;
    $level = 1;
    $exp = 0;
    if ($prog) {
        $level = (int)$prog['level'];
        $exp = (int)$prog['exp'];
        $skins = json_decode($prog['skin_unlocks'] ?: '[]', true) ?: [];
        $armors = json_decode($prog['armor_unlocks'] ?: '[]', true) ?: [];
        $weapons = json_decode($prog['weapon_levels'] ?: '{}', true) ?: [];
        $owned += count(array_unique(array_filter($skins, 'is_string')));
        $owned += count(array_unique(array_filter($armors, 'is_string')));
        foreach ($weapons as $lvl) {
            if ((int)$lvl > 0) $owned++;
        }
    }

    return [
        'account_id' => $acct['id'],
        'display_name' => $acct['display_name'],
        'coins' => (int)$acct['coins'],
        'gems' => (int)$acct['gems'],
        'level' => $level,
        'exp' => $exp,
        'owned_items' => $owned,
        'registered_at' => (int)$acct['registered_at'],
        'last_seen' => (int)$acct['last_seen'],
    ];
}

function merge_account_into(PDO $pdo, string $source, string $target): void {
    if ($source === $target) return;

    $src = account_stats($pdo, $source);
    if (!$src) return;

    $pdo->prepare("UPDATE accounts SET coins=coins + ?, gems=gems + ?, last_seen=? WHERE id=?")
        ->execute([(int)$src['coins'], (int)$src['gems'], time(), $target]);

    $tp = $pdo->prepare("SELECT * FROM account_progression WHERE account_id=?");
    $tp->execute([$target]);
    $target_prog = $tp->fetch();
    if (!$target_prog) {
        $pdo->prepare("INSERT INTO account_progression (account_id,updated_at) VALUES (?,0)")->execute([$target]);
        $tp->execute([$target]);
        $target_prog = $tp->fetch();
    }

    $sp = $pdo->prepare("SELECT * FROM account_progression WHERE account_id=?");
    $sp->execute([$source]);
    $source_prog = $sp->fetch();

    if ($source_prog && $target_prog) {
        $target_wl = json_decode($target_prog['weapon_levels'] ?: '{}', true) ?: [];
        $source_wl = json_decode($source_prog['weapon_levels'] ?: '{}', true) ?: [];
        foreach ($source_wl as $weapon => $level) {
            $target_wl[$weapon] = max((int)($target_wl[$weapon] ?? 0), (int)$level);
        }

        $target_skins = json_decode($target_prog['skin_unlocks'] ?: '[]', true) ?: [];
        $source_skins = json_decode($source_prog['skin_unlocks'] ?: '[]', true) ?: [];
        $target_armors = json_decode($target_prog['armor_unlocks'] ?: '[]', true) ?: [];
        $source_armors = json_decode($source_prog['armor_unlocks'] ?: '[]', true) ?: [];
        $merged_skins = array_values(array_unique(array_merge(
            array_filter($target_skins, 'is_string'),
            array_filter($source_skins, 'is_string')
        )));
        $merged_armors = array_values(array_unique(array_merge(
            array_filter($target_armors, 'is_string'),
            array_filter($source_armors, 'is_string')
        )));

        $target_updated = (int)$target_prog['updated_at'];
        $source_updated = (int)$source_prog['updated_at'];
        if ($source_updated > $target_updated) {
            $equipped = $source_prog['equipped_slots'];
            $skin = $source_prog['current_skin'];
            $armor = $source_prog['current_armor'];
            $updated = $source_updated;
        } else {
            $equipped = $target_prog['equipped_slots'];
            $skin = $target_prog['current_skin'];
            $armor = $target_prog['current_armor'];
            $updated = $target_updated;
        }

        $pdo->prepare("
            UPDATE account_progression
               SET level=?, exp=?, weapon_levels=?, skin_unlocks=?, armor_unlocks=?,
                   equipped_slots=?, current_skin=?, current_armor=?, updated_at=?
             WHERE account_id=?
        ")->execute([
            max((int)$target_prog['level'], (int)$source_prog['level']),
            max((int)$target_prog['exp'], (int)$source_prog['exp']),
            json_encode($target_wl),
            json_encode($merged_skins),
            json_encode($merged_armors),
            $equipped,
            $skin,
            $armor,
            $updated,
            $target,
        ]);
    }

    $sw = $pdo->prepare("SELECT * FROM wheel_spins WHERE player_id=?");
    $sw->execute([$source]);
    $source_wheel = $sw->fetch();
    if ($source_wheel) {
        $tw = $pdo->prepare("SELECT * FROM wheel_spins WHERE player_id=?");
        $tw->execute([$target]);
        $target_wheel = $tw->fetch();
        if ($target_wheel) {
            $pdo->prepare("UPDATE wheel_spins SET last_spin_at=?, bonus_spins=? WHERE player_id=?")
                ->execute([
                    max((int)$target_wheel['last_spin_at'], (int)$source_wheel['last_spin_at']),
                    max((int)($target_wheel['bonus_spins'] ?? 0), (int)($source_wheel['bonus_spins'] ?? 0)),
                    $target,
                ]);
            $pdo->prepare("DELETE FROM wheel_spins WHERE player_id=?")->execute([$source]);
        } else {
            $pdo->prepare("UPDATE wheel_spins SET player_id=? WHERE player_id=?")->execute([$target, $source]);
        }
    }

    $pdo->prepare("UPDATE OR IGNORE transactions SET player_id=? WHERE player_id=?")->execute([$target, $source]);
    $pdo->prepare("DELETE FROM transactions WHERE player_id=?")->execute([$source]);
    $pdo->prepare("UPDATE player_mail SET player_id=? WHERE player_id=?")->execute([$target, $source]);
}

function response_account_payload(PDO $pdo, array $account, string $token): array {
    $resp = [
        'token' => $token,
        'account_id' => $account['id'],
        'coins' => (int)$account['coins'],
        'gems' => (int)$account['gems'],
        'display_name' => $account['display_name'],
    ];

    $stmt = $pdo->prepare("SELECT * FROM account_progression WHERE account_id=?");
    $stmt->execute([$account['id']]);
    $prog = $stmt->fetch();
    if (!$prog) return $resp;

    $resp['level'] = (int)$prog['level'];
    $resp['exp'] = (int)$prog['exp'];
    $resp['current_skin'] = $prog['current_skin'];
    $resp['current_armor'] = $prog['current_armor'];
    $resp['prog_updated_at'] = (int)$prog['updated_at'];
    foreach ((json_decode($prog['weapon_levels'] ?: '{}', true) ?: []) as $wname => $wlevel) {
        if (preg_match('/^[A-Za-z0-9_]{1,32}$/', $wname)) $resp['wl_' . $wname] = (int)$wlevel;
    }
    foreach ((json_decode($prog['skin_unlocks'] ?: '[]', true) ?: []) as $s) {
        if (preg_match('/^Skin_\d+$/', $s)) $resp['su_' . $s] = 1;
    }
    foreach ((json_decode($prog['armor_unlocks'] ?: '[]', true) ?: []) as $a) {
        if (preg_match('/^[A-Za-z0-9_]{1,32}$/', $a)) $resp['au_' . $a] = 1;
    }
    $equipped = json_decode($prog['equipped_slots'] ?: '[]', true) ?: [];
    for ($i = 0; $i < 8; $i++) $resp['eq_' . ($i + 1)] = $equipped[$i] ?? '';
    return $resp;
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
