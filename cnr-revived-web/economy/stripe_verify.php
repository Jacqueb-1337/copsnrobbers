<?php

require __DIR__ . '/_db.php';
require __DIR__ . '/stripe_lib.php';

header('Cache-Control: no-store');

$status     = strtolower(trim($_GET['status'] ?? ''));
$session_id = trim($_GET['session_id'] ?? '');
$product_id = trim($_GET['product_id'] ?? '');

if ($status === 'cancel') {
    stripe_render_page('Purchase cancelled', 'The Stripe checkout was cancelled. You can return to the game.');
}

if ($status !== '' && $status !== 'paid' && $status !== 'verified') {
    stripe_render_page(
        'Payment pending',
        'Stripe returned an unexpected checkout status. The session will be verified directly before granting rewards.'
    );
}

if ($session_id === '') {
    stripe_http_fail('Missing session_id.');
}

$session = stripe_api_request('GET', '/v1/checkout/sessions/' . rawurlencode($session_id));

$paid = (($session['payment_status'] ?? '') === 'paid') && (($session['status'] ?? '') === 'complete');
$sessionProduct = (string)($session['metadata']['product_id'] ?? '');
$clientRef = (string)($session['client_reference_id'] ?? '');

if ($product_id !== '' && $sessionProduct !== '' && !hash_equals($sessionProduct, $product_id)) {
    $paid = false;
}

if ($paid) {
    $verifiedUrl = stripe_origin() . '/economy/stripe_verify.php?status=verified&session_id=' . rawurlencode($session_id) . '&product_id=' . rawurlencode($product_id);
    if (!isset($_GET['verified'])) {
        header('Location: ' . $verifiedUrl, true, 302);
        exit;
    }

    $amount = '';
    if (isset($session['amount_total'])) {
        $amount = number_format(((int)$session['amount_total']) / 100, 2, '.', '');
    }
    $currency = strtoupper((string)($session['currency'] ?? ''));
    $summary = 'Checkout complete for ' . htmlspecialchars($product_id ?: $sessionProduct, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
    if ($amount !== '' && $currency !== '') {
        $summary .= '<br>Paid ' . htmlspecialchars($currency . ' ' . $amount, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
    }
    stripe_render_page('Payment complete', $summary . '<br>You can return to the game.');
}

stripe_render_page('Payment not completed', 'Stripe did not mark this session as paid.');

function stripe_render_page(string $title, string $body): never
{
    http_response_code(200);
    header('Content-Type: text/html; charset=utf-8');
    echo '<!doctype html><html><head><meta charset="utf-8"><title>' . htmlspecialchars($title, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8') . '</title>';
    echo '<style>body{font-family:system-ui,sans-serif;background:#0f1116;color:#f3f4f6;padding:32px;line-height:1.5}';
    echo 'a{color:#7dd3fc}</style></head><body>';
    echo '<h1>' . htmlspecialchars($title, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8') . '</h1>';
    echo '<p>' . $body . '</p>';
    echo '</body></html>';
    exit;
}
