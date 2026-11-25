<?php
// Layout data is independent of numeric progression. Caller owns the transaction.
function merge_hud_snapshot(PDO $pdo, string $account, array $post): array {
    if (($post['hud_sync'] ?? '') !== '1') return [];
    $allowed = array_fill_keys(['CNRMod_HUD_Aim','CNRMod_HUD_ChatBar','CNRMod_HUD_Controls','CNRMod_HUD_ToolBar','CNRMod_PX_Aim','CNRMod_PX_Ammo','CNRMod_PX_ChatBar','CNRMod_PX_Fire','CNRMod_PX_HP','CNRMod_PX_HPack','CNRMod_PX_Jump','CNRMod_PX_NextG','CNRMod_PX_PList','CNRMod_PX_Pause','CNRMod_PX_PrevG','CNRMod_PX_Record','CNRMod_PX_Reload','CNRMod_PX_Sneak','CNRMod_PX_TS1','CNRMod_PX_TS2','CNRMod_PX_ToolBar','CNRMod_PX_WpSel','CNRMod_PY_Aim','CNRMod_PY_Ammo','CNRMod_PY_ChatBar','CNRMod_PY_Fire','CNRMod_PY_HP','CNRMod_PY_HPack','CNRMod_PY_Jump','CNRMod_PY_NextG','CNRMod_PY_PList','CNRMod_PY_Pause','CNRMod_PY_PrevG','CNRMod_PY_Record','CNRMod_PY_Reload','CNRMod_PY_Sneak','CNRMod_PY_TS1','CNRMod_PY_TS2','CNRMod_PY_ToolBar','CNRMod_PY_WpSel','CNRMod_SZ_Aim','CNRMod_SZ_Ammo','CNRMod_SZ_Fire','CNRMod_SZ_HPack','CNRMod_SZ_Jump','CNRMod_SZ_NextG','CNRMod_SZ_PList','CNRMod_SZ_Pause','CNRMod_SZ_PrevG','CNRMod_SZ_Record','CNRMod_SZ_Reload','CNRMod_SZ_Sneak','CNRMod_SZ_TS2','CNRMod_SZ_WpSel'], true);
    $layout = $post['hud_layout'] ?? null;
    $baseRevision = $post['hud_revision'] ?? null;
    $valid = false;
    if (is_string($layout) && strlen($layout) <= 8192 &&
        is_string($baseRevision) && preg_match('/^[0-9]{1,9}$/D', $baseRevision)) {
        $raw = base64_decode($layout, true);
        if ($raw !== false) {
            $lines = explode("\n", $raw);
            $valid = array_shift($lines) === 'v1';
            $values = [];
            foreach ($lines as $line) {
                if (!$valid) break;
                if ($line === '') continue;
                $parts = explode('=', $line, 2);
                if (count($parts) !== 2) { $valid = false; break; }
                [$key,$value] = $parts;
                if (!isset($allowed[$key]) || isset($values[$key]) || !is_numeric($value) ||
                    !is_finite((float)$value) || abs((float)$value) > 100000) { $valid = false; break; }
                if (strpos($key,'CNRMod_SZ_') === 0 && ((float)$value <= 0 || (float)$value > 100)) { $valid = false; break; }
                if (strpos($key,'CNRMod_HUD_') === 0 && $value !== '0' && $value !== '1') { $valid = false; break; }
                $values[$key] = $value;
            }
            if ($valid) {
                ksort($values,SORT_STRING);
                $raw = "v1\n";
                foreach ($values as $key=>$value) $raw .= $key.'='.$value."\n";
                $layout = base64_encode($raw);
            }
        }
    }
    $pdo->exec("CREATE TABLE IF NOT EXISTS account_hud_layout (account_id TEXT PRIMARY KEY, revision INTEGER NOT NULL, layout TEXT NOT NULL)");
    $stmt = $pdo->prepare("SELECT revision,layout FROM account_hud_layout WHERE account_id=?");
    $stmt->execute([$account]);
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    $rev = $row ? (int)$row['revision'] : 0;
    $current = $row ? (string)$row['layout'] : '';
    if ($valid && (int)$baseRevision === $rev && $layout !== $current) {
        $rev++;
        if ($row) {
            $stmt = $pdo->prepare("UPDATE account_hud_layout SET revision=?,layout=? WHERE account_id=?");
            $stmt->execute([$rev,$layout,$account]);
        } else {
            $stmt = $pdo->prepare("INSERT INTO account_hud_layout(account_id,revision,layout) VALUES(?,?,?)");
            $stmt->execute([$account,$rev,$layout]);
        }
        $current = $layout;
    }
    return ['hud_revision'=>$rev,'hud_layout'=>$current];
}
