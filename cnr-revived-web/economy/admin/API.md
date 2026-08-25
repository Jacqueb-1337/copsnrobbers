# CNR Economy Admin API

Endpoint: `/economy/admin/api.php`

The API returns JSON and requires authentication on every request.

## Authentication

Preferred production setup: configure `CNR_ADMIN_API_TOKEN` in `economy/.env` or the web-server environment and send it as a Bearer token:

```bash
curl -H "Authorization: Bearer $CNR_ADMIN_API_TOKEN" \
  "https://play.jacqueb.me/economy/admin/api.php?action=status"
```

Until a dedicated token is configured, HTTP Basic auth uses the same password verifier as the web admin portal:

```bash
curl -u admin:"$CNR_ADMIN_PASSWORD" \
  "https://play.jacqueb.me/economy/admin/api.php?action=status"
```

Do not put credentials in query strings or commit them to scripts.

## Common commands

List content:

```bash
curl -u admin:"$CNR_ADMIN_PASSWORD" \
  "https://play.jacqueb.me/economy/admin/api.php?action=list_content"
```

Send mail to one player:

```bash
curl -u admin:"$CNR_ADMIN_PASSWORD" -X POST \
  -F action=send_mail \
  -F player_id=PLAYER_ID \
  -F subject="Test mail" \
  -F body="Hello from the admin API" \
  -F coins=100 \
  -F gems=0 \
  -F spins=0 \
  "https://play.jacqueb.me/economy/admin/api.php"
```

Broadcast mail uses `player_id=*`.

Add currency to a player:

```bash
curl -u admin:"$CNR_ADMIN_PASSWORD" -X POST \
  -F action=grant \
  -F player_id=PLAYER_ID \
  -F mode=add \
  -F coins=500 \
  -F gems=10 \
  "https://play.jacqueb.me/economy/admin/api.php"
```

Upload a DLC map and have the server calculate its MD5 automatically:

```bash
curl -u admin:"$CNR_ADMIN_PASSWORD" -X POST \
  -F action=upload_content \
  -F type=dlcmap \
  -F id=TestMinecraftMap \
  -F name="Test Minecraft Map" \
  -F base_scene=FreeRun3_1 \
  -F replace=1 \
  -F file=@testmap1.cnrpack \
  -F thumb_file=@testmap1.png \
  "https://play.jacqueb.me/economy/admin/api.php"
```

Upload a gun definition/file:

```bash
curl -u admin:"$CNR_ADMIN_PASSWORD" -X POST \
  -F action=upload_content \
  -F type=gun \
  -F id=bussin \
  -F name="Bussin'" \
  -F replace=1 \
  -F file=@bussin.json \
  "https://play.jacqueb.me/economy/admin/api.php"
```

Register content that is hosted elsewhere. If `hash` is omitted the server downloads the URL and calculates its MD5:

```bash
curl -u admin:"$CNR_ADMIN_PASSWORD" -X POST \
  -F action=add_content \
  -F type=data \
  -F id=my_data \
  -F name="My Data" \
  -F url="https://example.invalid/my_data.json" \
  "https://play.jacqueb.me/economy/admin/api.php"
```

Recalculate the stored hash from an item's current URL:

```bash
curl -u admin:"$CNR_ADMIN_PASSWORD" -X POST \
  -F action=sync_hash \
  -F id=TestMinecraftMap \
  "https://play.jacqueb.me/economy/admin/api.php"
```

## Actions

`status`, `list_content`, `list_players`, `list_mail`, `list_transactions`, `send_mail`, `grant`, `add_content`, `upload_content`, `upload_thumbnail`, `toggle_content`, `set_content_enabled`, `delete_content`, `reorder_content`, `set_price`, `update_hash`, and `sync_hash`.

Content types are `map`, `dlcmap`, `texture`, `data`, `skin`, and `gun`.

`upload_content` stores the file in the appropriate `economy/uploads/...` directory, builds the public URL, computes the MD5 from the exact stored bytes, and creates or replaces the corresponding `content_items` row. Use `replace=1` to replace an existing ID.
