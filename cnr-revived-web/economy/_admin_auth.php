<?php
// Shared authentication helpers for the web admin portal and command-line admin API.
// Prefer setting CNR_ADMIN_PASS_HASH and CNR_ADMIN_API_TOKEN in economy/.env or the
// web-server environment. The plain-password fallback preserves the legacy portal
// configuration until a hash/token is configured.

function cnr_admin_env_map(): array
{
    static $cache = null;
    if ($cache !== null) return $cache;

    $cache = [];
    $paths = [
        __DIR__ . '/.env',
        dirname(__DIR__) . '/.env',
    ];
    foreach ($paths as $path) {
        if (!is_file($path)) continue;
        $parsed = @parse_ini_file($path, false, INI_SCANNER_RAW);
        if (!is_array($parsed)) continue;
        foreach ($parsed as $key => $value) {
            $cache[$key] = is_string($value) ? trim($value) : $value;
        }
    }
    return $cache;
}

function cnr_admin_env(string $key, string $default = ''): string
{
    $map = cnr_admin_env_map();
    if (array_key_exists($key, $map) && trim((string)$map[$key]) !== '') {
        return trim((string)$map[$key]);
    }
    $value = getenv($key);
    if ($value !== false && trim((string)$value) !== '') return trim((string)$value);
    return $default;
}

function cnr_admin_equals(string $expected, string $actual): bool
{
    if (function_exists('hash_equals')) return hash_equals($expected, $actual);
    return $expected === $actual;
}

function cnr_admin_verify_password(string $attempt): bool
{
    $fallbackHash = '$2y$10$placeholderREPLACETHISHASHxxxxxxxxxxxxxxxxxxxxxxxxxxxx';
    $hash = cnr_admin_env('CNR_ADMIN_PASS_HASH', $fallbackHash);
    if ($hash !== '' && strpos($hash, 'placeholder') === false) {
        return password_verify($attempt, $hash);
    }

    // Legacy fallback. Move this into CNR_ADMIN_PASS_PLAIN or, preferably,
    // CNR_ADMIN_PASS_HASH in .env when rotating the admin credentials.
    $plain = cnr_admin_env('CNR_ADMIN_PASS_PLAIN', 'cnradmin');
    return cnr_admin_equals($plain, $attempt);
}

function cnr_admin_api_token(): string
{
    return cnr_admin_env('CNR_ADMIN_API_TOKEN', '');
}

function cnr_admin_authorization_header(): string
{
    if (!empty($_SERVER['HTTP_AUTHORIZATION'])) return trim((string)$_SERVER['HTTP_AUTHORIZATION']);
    if (!empty($_SERVER['REDIRECT_HTTP_AUTHORIZATION'])) return trim((string)$_SERVER['REDIRECT_HTTP_AUTHORIZATION']);
    if (function_exists('apache_request_headers')) {
        $headers = apache_request_headers();
        if (is_array($headers)) {
            foreach ($headers as $key => $value) {
                if (strcasecmp((string)$key, 'Authorization') === 0) return trim((string)$value);
            }
        }
    }
    return '';
}

function cnr_admin_verify_api_request(): bool
{
    $authorization = cnr_admin_authorization_header();
    $token = cnr_admin_api_token();

    if ($token !== '' && stripos($authorization, 'Bearer ') === 0) {
        $supplied = trim(substr($authorization, 7));
        if ($supplied !== '' && cnr_admin_equals($token, $supplied)) return true;
    }

    $user = isset($_SERVER['PHP_AUTH_USER']) ? (string)$_SERVER['PHP_AUTH_USER'] : '';
    $pass = isset($_SERVER['PHP_AUTH_PW']) ? (string)$_SERVER['PHP_AUTH_PW'] : '';
    if ($pass === '' && stripos($authorization, 'Basic ') === 0) {
        $decoded = base64_decode(trim(substr($authorization, 6)), true);
        if ($decoded !== false && strpos($decoded, ':') !== false) {
            list($user, $pass) = explode(':', $decoded, 2);
        }
    }

    if ($pass !== '' && ($user === '' || strcasecmp($user, 'admin') === 0 || strcasecmp($user, 'cnradmin') === 0)) {
        return cnr_admin_verify_password($pass);
    }
    return false;
}
