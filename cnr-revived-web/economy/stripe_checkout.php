<?php

require __DIR__ . '/_db.php';
require __DIR__ . '/stripe_lib.php';

if ($_SERVER['REQUEST_METHOD'] !== 'GET' && $_SERVER['REQUEST_METHOD'] !== 'POST') {
    stripe_http_fail('Method not allowed', 405);
}

$product_id   = trim($_GET['product_id']   ?? $_POST['product_id']   ?? '');
$title        = trim($_GET['title']        ?? $_POST['title']        ?? '');
$description  = trim($_GET['description']  ?? $_POST['description']  ?? '');
$price        = trim($_GET['price']        ?? $_POST['price']        ?? '');
$currency     = strtoupper(trim($_GET['currency'] ?? $_POST['currency'] ?? 'USD'));
$player_id    = trim($_GET['player_id']    ?? $_POST['player_id']    ?? '');
$display_name = trim($_GET['display_name'] ?? $_POST['display_name'] ?? '');
$payload      = trim($_GET['payload']      ?? $_POST['payload']      ?? '');

if ($product_id === '') stripe_http_fail('Missing product_id');
if ($title === '')      $title = $product_id;
if ($description === '') $description = $title;
if ($price === '' || !is_numeric($price) || (float)$price <= 0) stripe_http_fail('Invalid price');

$session = stripe_api_request('POST', '/v1/checkout/sessions', [
    'mode' => 'payment',
    'client_reference_id' => $player_id,
    'success_url' => stripe_origin() . '/economy/stripe_verify.php?status=paid&session_id={CHECKOUT_SESSION_ID}&product_id=' . rawurlencode($product_id),
    'cancel_url'  => stripe_origin() . '/economy/stripe_verify.php?status=cancel&product_id=' . rawurlencode($product_id),
    'metadata[product_id]'   => $product_id,
    'metadata[player_id]'    => $player_id,
    'metadata[display_name]' => $display_name,
    'metadata[payload]'      => $payload,
    'line_items[0][quantity]' => 1,
    'line_items[0][price_data][currency]' => strtolower($currency),
    'line_items[0][price_data][unit_amount]' => stripe_minor_units($price, $currency),
    'line_items[0][price_data][product_data][name]' => $title,
    'line_items[0][price_data][product_data][description]' => $description,
]);

if (empty($session['url'])) {
    stripe_http_fail('Stripe did not return a checkout URL.', 502);
}

header('Cache-Control: no-store');
header('Location: ' . $session['url'], true, 302);
exit;

