<?php
// set_pin.php
// POST player_id, token, pin (4-8 digits)
// Saves a PIN so the player can recover their account on a new device via claim.php

require __DIR__ . '/_db.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') fail('POST only', 405);

$player    = require_auth();
$player_id = $player['id'];
$pin       = trim($_POST['pin']      ?? '');
$password  = trim($_POST['password'] ?? '');

if (!preg_match('/^\d{4,8}$/', $pin))           fail('pin must be 4-8 digits');
if (strlen($password) < 6)                       fail('password must be at least 6 characters');
if (!preg_match('/^[\w!@#$%^&*()\-+=]+$/', $password)) fail('password contains invalid characters');

$hash = password_hash($password . $pin, PASSWORD_BCRYPT, ['cost' => 10]);
db()->prepare("UPDATE accounts SET pin_hash=? WHERE id=?")->execute([$hash, $player['id']]);

ok(['message' => 'PIN set']);
