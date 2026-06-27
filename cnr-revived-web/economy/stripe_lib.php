<?php

function stripe_env_map(): array
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
        $parsed = parse_ini_file($path, false, INI_SCANNER_RAW);
        if (is_array($parsed)) {
            foreach ($parsed as $k => $v) {
                $cache[$k] = is_string($v) ? trim($v) : $v;
            }
        }
    }

    return $cache;
}

function stripe_env(string $key, string $default = ''): string
{
    $map = stripe_env_map();
    if (array_key_exists($key, $map) && $map[$key] !== '') return trim((string)$map[$key]);
    $value = getenv($key);
    if ($value !== false && trim((string)$value) !== '') return trim((string)$value);
    return $default;
}

function stripe_secret_key(): string
{
    $primary = stripe_env('STRIPE_SECRET_KEY', '');
    if ($primary !== '') return $primary;

    $alt = stripe_env('STRIPE_SECRET_KEY_ALT', '');
    if ($alt !== '') return $alt;

    stripe_http_fail('Stripe secret key is not configured.', 500);
}

function stripe_origin(): string
{
    if (!empty($_SERVER['HTTP_X_FORWARDED_PROTO'])) {
        $scheme = strtolower(trim((string)$_SERVER['HTTP_X_FORWARDED_PROTO'])) === 'https' ? 'https' : 'http';
    } else {
        $scheme = (!empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off') ? 'https' : 'http';
    }
    $host = $_SERVER['HTTP_HOST'] ?? 'play.jacqueb.me';
    return $scheme . '://' . $host;
}

function stripe_zero_decimal_currency(string $currency): bool
{
    static $zero = [
        'BIF','CLP','DJF','GNF','JPY','KMF','KRW','MGA','PYG','RWF',
        'UGX','VND','VUV','XAF','XOF','XPF'
    ];
    return in_array(strtoupper(trim($currency)), $zero, true);
}

function stripe_minor_units(string $amount, string $currency): int
{
    $value = (float)$amount;
    if (stripe_zero_decimal_currency($currency)) {
        return (int)round($value, 0);
    }
    return (int)round($value * 100, 0);
}

function stripe_api_request(string $method, string $path, array $fields = []): array
{
    $secret = stripe_secret_key();
    $url    = 'https://api.stripe.com' . $path;
    $body   = http_build_query($fields, '', '&', PHP_QUERY_RFC3986);
    $method = strtoupper($method);
    if ($method === 'GET' && $body !== '') {
        $url .= '?' . $body;
    }

    $ch = curl_init($url);
    $opts = [
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_CUSTOMREQUEST  => $method,
        CURLOPT_HTTPHEADER     => [
            'Authorization: Bearer ' . $secret,
            'Content-Type: application/x-www-form-urlencoded',
        ],
        CURLOPT_TIMEOUT        => 30,
        CURLOPT_CONNECTTIMEOUT => 10,
    ];
    if ($method !== 'GET') {
        $opts[CURLOPT_POSTFIELDS] = $body;
    }
    curl_setopt_array($ch, $opts);

    $raw  = curl_exec($ch);
    $err  = curl_error($ch);
    $code = (int)curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);

    if ($raw === false) {
        stripe_http_fail('Stripe request failed: ' . $err, 502);
    }

    $json = json_decode($raw, true);
    if (!is_array($json)) {
        stripe_http_fail('Stripe returned invalid JSON.', 502);
    }

    if ($code < 200 || $code >= 300) {
        $message = $json['error']['message'] ?? ('Stripe HTTP ' . $code);
        stripe_http_fail($message, 502);
    }

    return $json;
}

function stripe_http_fail(string $message, int $http = 400): never
{
    http_response_code($http);
    header('Content-Type: text/html; charset=utf-8');
    header('Cache-Control: no-store');
    echo '<!doctype html><html><head><meta charset="utf-8"><title>Stripe checkout</title></head><body>';
    echo '<h1>Stripe checkout error</h1><p>' . htmlspecialchars($message, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8') . '</p>';
    echo '</body></html>';
    exit;
}
