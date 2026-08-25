<?php
// content.php — public content manifest endpoint
// GET: returns the full list of enabled content items (maps, textures, data files)
// No authentication required — returns only enabled items, URLs are admin-controlled.

require_once '_db.php';

header('Content-Type: application/json');
header('Cache-Control: no-store');

if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    http_response_code(405);
    echo json_encode(['ok' => false, 'error' => 'Method not allowed']);
    exit;
}

$pdo = db();

$rows = $pdo->query(
    "SELECT id, type, name, url, thumbnail_url, file_hash, thumbnail_hash, material_name, data_key, sort_order
       FROM content_items
      WHERE enabled = 1
      ORDER BY type, sort_order ASC, created_at ASC"
)->fetchAll(PDO::FETCH_ASSOC);

$maps     = [];
$dlc_maps = [];
$textures = [];
$data     = [];
$skins    = [];
$guns     = [];

foreach ($rows as $r) {
    switch ($r['type']) {
        case 'map':
            $maps[] = [
                'id'             => $r['id'],
                'name'           => $r['name'],
                'url'            => $r['url'],
                'thumbnail_url'  => $r['thumbnail_url']  ?? '',
                'hash'           => $r['file_hash']      ?? '',
                'thumbnail_hash' => $r['thumbnail_hash'] ?? '',
            ];
            break;
        case 'dlcmap':
            $dlc_maps[] = [
                'id'             => $r['id'],
                'name'           => $r['name'],
                'url'            => $r['url'],
                'thumbnail_url'  => $r['thumbnail_url']  ?? '',
                'hash'           => $r['file_hash']      ?? '',
                'thumbnail_hash' => $r['thumbnail_hash'] ?? '',
            ];
            break;
        case 'texture':
            $textures[] = [
                'id'            => $r['id'],
                'material_name' => $r['material_name'],
                'url'           => $r['url'],
                'hash'          => $r['file_hash'] ?? '',
            ];
            break;
        case 'data':
            $data[] = [
                'id'   => $r['id'],
                'key'  => $r['data_key'],
                'url'  => $r['url'],
                'hash' => $r['file_hash'] ?? '',
            ];
            break;
        case 'skin':
            $skins[] = [
                'id'            => $r['id'],
                'slot_key'      => $r['data_key'],
                'name'          => $r['name'],
                'material_name' => $r['material_name'],
                'url'           => $r['url'],
                'hash'          => $r['file_hash'] ?? '',
                'price'         => (int)$r['sort_order'],
            ];
            break;
        case 'gun':
            // Keep legacy gun_key while also emitting the field names the client
            // manifest parser consumes: name=internal key, display_name=shop label.
            $guns[] = [
                'id'            => $r['id'],
                'gun_key'       => $r['data_key'],
                'name'          => $r['data_key'] ?: $r['name'],
                'display_name'  => $r['name'],
                'material_name' => $r['material_name'],
                'url'           => $r['url'],
                'hash'          => $r['file_hash'] ?? '',
                'price'         => (int)$r['sort_order'],
            ];
            break;
    }
}

// Bussin' ships with CNR itself so a fresh install can discover it even if the
// admin DB row has not been created yet. A real admin entry with id=bussin wins.
$has_bussin = false;
foreach ($guns as $g) {
    if (($g['id'] ?? '') === 'bussin') { $has_bussin = true; break; }
}
if (!$has_bussin) {
    $guns[] = [
        'id'            => 'bussin',
        'gun_key'       => 'Bussin',
        'name'          => 'Bussin',
        'display_name'  => "Bussin'",
        'material_name' => 'bussin',
        'url'           => 'https://raw.githubusercontent.com/Jacqueb-1337/copsnrobbers/master/cnr-revived-web/economy/uploads/guns/bussin.json',
        'hash'          => '11b84dd8d0b706dd5018420291b26184',
        'price'         => 1000,
    ];
}

// manifest_version is a hash of the public content so clients know when to re-sync.
$version = substr(md5(json_encode([$rows, $maps, $dlc_maps, $guns])), 0, 12);

echo json_encode([
    'ok'               => true,
    'manifest_version' => $version,
    'maps'             => $maps,
    'dlc_maps'         => $dlc_maps,
    'textures'         => $textures,
    'data'             => $data,
    'skins'            => $skins,
    'guns'             => $guns,
]);
