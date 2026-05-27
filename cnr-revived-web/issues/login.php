<?php
// login.php — Admin session management for CNR Issues.
define('CNR_ISSUES', 1);
session_start();
header('Content-Type: application/json; charset=utf-8');
header('X-Content-Type-Options: nosniff');

$action = $_GET['action'] ?? 'login';

// ── Status check ──────────────────────────────────────────────────────────
if ($action === 'status') {
    echo json_encode(['admin' => !empty($_SESSION['cnr_admin'])]);
    exit;
}

// ── Logout ────────────────────────────────────────────────────────────────
if ($action === 'logout') {
    session_destroy();
    echo json_encode(['ok' => true]);
    exit;
}

// ── Login (POST only) ─────────────────────────────────────────────────────
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['error' => 'POST required.']);
    exit;
}

$raw  = file_get_contents('php://input');
$body = json_decode($raw, true) ?? [];

// Parse .env
$env     = [];
$envPath = __DIR__ . '/.env';
if (file_exists($envPath)) {
    foreach (file($envPath, FILE_IGNORE_NEW_LINES | FILE_SKIP_EMPTY_LINES) as $line) {
        $line = trim($line);
        if ($line === '' || $line[0] === '#' || strpos($line, '=') === false) continue;
        [$k, $v] = explode('=', $line, 2);
        $env[trim($k)] = trim($v);
    }
}

$adminUser = $env['ADMIN_USER'] ?? '';
$adminPass = $env['ADMIN_PASS'] ?? '';

if ($adminUser === '' || $adminPass === '') {
    http_response_code(500);
    echo json_encode(['error' => 'Admin credentials not configured.']);
    exit;
}

$user = trim((string)($body['username'] ?? ''));
$pass = (string)($body['password'] ?? '');

// Constant-time comparison prevents timing attacks
if (!hash_equals($adminUser, $user) || !hash_equals($adminPass, $pass)) {
    http_response_code(403);
    echo json_encode(['error' => 'Invalid credentials.']);
    exit;
}

$_SESSION['cnr_admin'] = true;
echo json_encode(['ok' => true]);
