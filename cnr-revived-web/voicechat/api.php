<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('X-Content-Type-Options: nosniff');
if (($_SERVER['HTTP_ORIGIN'] ?? '') === 'https://play.jacqueb.me') {
    header('Access-Control-Allow-Origin: https://play.jacqueb.me');
}
if (($_SERVER['REQUEST_METHOD'] ?? 'GET') === 'OPTIONS') {
    http_response_code(204);
    exit;
}

$root = __DIR__ . '/../tmp/voicechat';
$roomDir = $root . '/rooms';
if (!is_dir($roomDir) && !mkdir($roomDir, 0775, true) && !is_dir($roomDir)) {
    json_error('Failed to create voicechat storage');
}

$action = $_GET['action'] ?? '';
try {
    switch ($action) {
        case 'rooms':
            $resp = list_rooms();
            echo json_encode($resp);
            break;

        case 'join':
            require_method('POST');
            $body = read_json();
            $room = room_key($body['room'] ?? '');
            $peer = peer_key($body['peer'] ?? '');
            $name = trim((string)($body['name'] ?? $peer));
            $level = normalize_level($body['level'] ?? 0);
            $resp = with_room($room, function (&$roomState) use ($peer, $name) {
                init_room($roomState);
                $peerState = $roomState['peers'][$peer] ?? [];
                $roomState['peers'][$peer] = array_merge($peerState, [
                    'peer' => $peer,
                    'name' => mb_substr($name, 0, 64),
                    'seen' => time(),
                    'micMuted' => false,
                    'speakerMuted' => false,
                    'level' => 0,
                ]);
                append_message($roomState, [
                    'type' => 'join',
                    'from' => $peer,
                    'to' => null,
                    'data' => ['peer' => $peer, 'name' => mb_substr($name, 0, 64)],
                ]);
                cleanup_room($roomState);
                return [
                    'ok' => true,
                    'seq' => $roomState['seq'],
                    'peers' => array_values($roomState['peers']),
                ];
            });
            echo json_encode($resp);
            break;

        case 'level':
            require_method('POST');
            $body = read_json();
            $room = room_key($body['room'] ?? '');
            $peer = peer_key($body['peer'] ?? '');
            $level = normalize_level($body['level'] ?? 0);
            $resp = with_room($room, function (&$roomState) use ($peer, $level) {
                init_room($roomState);
                if (!isset($roomState['peers'][$peer])) {
                    $roomState['peers'][$peer] = [
                        'peer' => $peer,
                        'name' => $peer,
                        'seen' => time(),
                        'micMuted' => false,
                        'speakerMuted' => false,
                        'level' => 0,
                    ];
                }
                $roomState['peers'][$peer]['seen'] = time();
                $roomState['peers'][$peer]['level'] = $level;
                cleanup_room($roomState);
                return [
                    'ok' => true,
                    'seq' => $roomState['seq'],
                    'peers' => array_values($roomState['peers']),
                ];
            });
            echo json_encode($resp);
            break;

        case 'poll':
            $room = room_key((string)($_GET['room'] ?? ''));
            $peer = peer_key((string)($_GET['peer'] ?? ''));
            $since = max(0, (int)($_GET['since'] ?? 0));
            $resp = with_room($room, function (&$roomState) use ($peer, $since) {
                init_room($roomState);
                if ($peer !== '') {
                    $roomState['peers'][$peer]['seen'] = time();
                }
                cleanup_room($roomState);
                $messages = [];
                foreach ($roomState['messages'] as $msg) {
                    if ($msg['seq'] <= $since) continue;
                    if (!empty($msg['to']) && $msg['to'] !== $peer) continue;
                    $messages[] = $msg;
                }
                return [
                    'ok' => true,
                    'seq' => $roomState['seq'],
                    'messages' => $messages,
                    'peers' => array_values($roomState['peers']),
                ];
            });
            echo json_encode($resp);
            break;

        case 'send':
            require_method('POST');
            $body = read_json();
            $room = room_key($body['room'] ?? '');
            $peer = peer_key($body['peer'] ?? '');
            $type = trim((string)($body['type'] ?? ''));
            if ($type === '') json_error('Missing type');
            $to = trim((string)($body['to'] ?? ''));
            $data = $body['data'] ?? [];
            $resp = with_room($room, function (&$roomState) use ($peer, $type, $to, $data) {
                init_room($roomState);
                append_message($roomState, [
                    'type' => $type,
                    'from' => $peer,
                    'to' => $to !== '' ? $to : null,
                    'data' => $data,
                ]);
                return ['ok' => true, 'seq' => $roomState['seq']];
            });
            echo json_encode($resp);
            break;

        case 'leave':
            require_method('POST');
            $body = read_json();
            $room = room_key($body['room'] ?? '');
            $peer = peer_key($body['peer'] ?? '');
            $resp = with_room($room, function (&$roomState) use ($peer) {
                init_room($roomState);
                unset($roomState['peers'][$peer]);
                append_message($roomState, [
                    'type' => 'leave',
                    'from' => $peer,
                    'to' => null,
                    'data' => ['peer' => $peer],
                ]);
                cleanup_room($roomState);
                return ['ok' => true];
            });
            echo json_encode($resp);
            break;

        case 'log':
            require_method('POST');
            $body = read_json();
            $room = room_key($body['room'] ?? 'voicechat');
            $peer = peer_key($body['peer'] ?? 'page');
            $text = trim((string)($body['text'] ?? ''));
            if ($text === '') json_error('Missing text');
            $resp = with_log($room, function (&$logState) use ($peer, $text) {
                init_log($logState);
                append_log_entry($logState, [
                    'peer' => $peer,
                    'text' => mb_substr($text, 0, 256),
                ]);
                cleanup_log($logState);
                return ['ok' => true, 'count' => count($logState['entries'])];
            });
            echo json_encode($resp);
            break;

        case 'logview':
            $room = room_key((string)($_GET['room'] ?? 'voicechat'));
            $since = max(0, (int)($_GET['since'] ?? 0));
            $resp = with_log($room, function (&$logState) use ($since) {
                init_log($logState);
                cleanup_log($logState);
                $entries = [];
                foreach ($logState['entries'] as $entry) {
                    if (($entry['seq'] ?? 0) <= $since) continue;
                    $entries[] = $entry;
                }
                return ['ok' => true, 'entries' => $entries, 'seq' => $logState['seq']];
            });
            echo json_encode($resp);
            break;

        default:
            json_error('Unknown action', 400);
    }
} catch (Throwable $e) {
    json_error($e->getMessage(), 500);
}

function require_method(string $method): void {
    if (strtoupper($_SERVER['REQUEST_METHOD'] ?? 'GET') !== strtoupper($method)) {
        json_error($method . ' required', 405);
    }
}

function read_json(): array {
    $raw = file_get_contents('php://input');
    $data = json_decode($raw ?: '[]', true);
    return is_array($data) ? $data : [];
}

function room_key($room): string {
    $room = trim((string)$room);
    if ($room === '') json_error('Missing room');
    return preg_replace('/[^a-zA-Z0-9_.-]+/', '_', $room);
}

function peer_key($peer): string {
    $peer = trim((string)$peer);
    if ($peer === '') json_error('Missing peer');
    return preg_replace('/[^a-zA-Z0-9_.-]+/', '_', $peer);
}

function room_path(string $room): string {
    return __DIR__ . '/../tmp/voicechat/rooms/' . $room . '.json';
}

function log_path(string $room): string {
    return __DIR__ . '/../tmp/voicechat/logs/' . $room . '.json';
}

function with_room(string $room, callable $fn): array {
    $path = room_path($room);
    $fh = fopen($path, 'c+');
    if (!$fh) json_error('Unable to open room store', 500);
    try {
        if (!flock($fh, LOCK_EX)) json_error('Unable to lock room store', 500);
        $json = stream_get_contents($fh);
        $state = $json ? json_decode($json, true) : null;
        if (!is_array($state)) $state = [];
        $result = $fn($state);
        if (is_array($result)) {
            rewind($fh);
            ftruncate($fh, 0);
            fwrite($fh, json_encode($state, JSON_UNESCAPED_SLASHES));
            fflush($fh);
        }
        flock($fh, LOCK_UN);
        fclose($fh);
        return is_array($result) ? $result : ['ok' => true];
    } catch (Throwable $e) {
        flock($fh, LOCK_UN);
        fclose($fh);
        throw $e;
    }
}

function with_log(string $room, callable $fn): array {
    $path = log_path($room);
    $dir = dirname($path);
    if (!is_dir($dir) && !mkdir($dir, 0775, true) && !is_dir($dir)) {
        json_error('Failed to create log storage');
    }
    $fh = fopen($path, 'c+');
    if (!$fh) json_error('Unable to open log store', 500);
    try {
        if (!flock($fh, LOCK_EX)) json_error('Unable to lock log store', 500);
        $json = stream_get_contents($fh);
        $state = $json ? json_decode($json, true) : null;
        if (!is_array($state)) $state = [];
        $result = $fn($state);
        if (is_array($result)) {
            rewind($fh);
            ftruncate($fh, 0);
            fwrite($fh, json_encode($state, JSON_UNESCAPED_SLASHES));
            fflush($fh);
        }
        flock($fh, LOCK_UN);
        fclose($fh);
        return is_array($result) ? $result : ['ok' => true];
    } catch (Throwable $e) {
        flock($fh, LOCK_UN);
        fclose($fh);
        throw $e;
    }
}

function init_room(array &$roomState): void {
    if (!isset($roomState['seq'])) $roomState['seq'] = 0;
    if (!isset($roomState['peers']) || !is_array($roomState['peers'])) $roomState['peers'] = [];
    if (!isset($roomState['messages']) || !is_array($roomState['messages'])) $roomState['messages'] = [];
}

function init_log(array &$logState): void {
    if (!isset($logState['seq'])) $logState['seq'] = 0;
    if (!isset($logState['entries']) || !is_array($logState['entries'])) $logState['entries'] = [];
}

function append_log_entry(array &$logState, array $entry): void {
    $logState['seq'] = (int)($logState['seq'] ?? 0) + 1;
    $entry['seq'] = $logState['seq'];
    $entry['ts'] = time();
    $logState['entries'][] = $entry;
}

function cleanup_log(array &$logState): void {
    $now = time();
    $logState['entries'] = array_values(array_filter($logState['entries'], function ($entry) use ($now) {
        return isset($entry['ts']) && ($now - (int)$entry['ts']) < 86400;
    }));
}

function append_message(array &$roomState, array $msg): void {
    $roomState['seq'] = (int)($roomState['seq'] ?? 0) + 1;
    $msg['seq'] = $roomState['seq'];
    $msg['ts'] = time();
    $roomState['messages'][] = $msg;
}

function cleanup_room(array &$roomState): void {
    $now = time();
    $roomState['messages'] = array_values(array_filter($roomState['messages'], function ($msg) use ($now) {
        return isset($msg['ts']) && ($now - (int)$msg['ts']) < 300;
    }));
    foreach ($roomState['peers'] as $peer => $info) {
        if (($now - (int)($info['seen'] ?? 0)) > 30) {
            unset($roomState['peers'][$peer]);
            continue;
        }
        if (!isset($roomState['peers'][$peer]['level'])) {
            $roomState['peers'][$peer]['level'] = 0;
        }
    }
}

function normalize_level($value): float {
    $level = is_numeric($value) ? (float)$value : 0.0;
    if ($level < 0) $level = 0;
    if ($level > 1) $level = 1;
    return $level;
}

function list_rooms(): array {
    $root = __DIR__ . '/../tmp/voicechat/rooms';
    $rooms = [];
    if (!is_dir($root)) {
        return ['ok' => true, 'rooms' => []];
    }
    foreach (glob($root . '/*.json') ?: [] as $path) {
        $room = basename($path, '.json');
        $json = @file_get_contents($path);
        $state = $json ? json_decode($json, true) : null;
        if (!is_array($state)) continue;
        init_room($state);
        cleanup_room($state);
        if (empty($state['peers'])) continue;
        $rooms[] = [
            'room' => $room,
            'seq' => (int)($state['seq'] ?? 0),
            'peers' => array_values($state['peers']),
            'peerCount' => count($state['peers']),
        ];
    }
    usort($rooms, function ($a, $b) {
        return $b['peerCount'] <=> $a['peerCount'] ?: strcmp($a['room'], $b['room']);
    });
    return ['ok' => true, 'rooms' => $rooms];
}

function json_error(string $msg, int $code = 400): never {
    http_response_code($code);
    echo json_encode(['ok' => false, 'error' => $msg]);
    exit;
}
