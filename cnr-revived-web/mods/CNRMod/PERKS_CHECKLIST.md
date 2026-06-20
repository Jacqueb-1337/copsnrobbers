# CNRMod Perks Checklist

## Current foundation
- Profile armor tab is reserved for CNR perks.
- Players can equip up to 3 perks.
- Equipped perk ids are saved in `PlayerPrefs` as `CNR_PerkSlot_0..2`.

## Multiplayer perks to implement
- Toughened: reduce incoming player damage by a fixed percent.
- Quick Hands: faster reload.
- Lightweight: faster movement only.
- Medic: chance to spawn a health pack at a killed player's body.
- Ammo Reserve: extra reserve ammo on spawn.
- Deadeye: slightly larger headshot hitbox for the equipped shooter's hit detection.
- Hardline: 50 percent chance to award one extra coin per kill.
- Scavenger: chance to spawn an ammo box at a killed player's body.
- Nimble: faster weapon swap and knife timing.
- Evasive: short movement boost after taking damage.
- Tracker: red through-wall outline on damaged enemies, visible to teammates; repeat hits refresh timer.

## Deferred
- Field Medic: revisit when revive/downed behavior exists outside zombies.
- Zombies-specific perks: add later as a separate mode-focused pass.

## Notes
- Ambient random health/ammo packs should be disabled before Medic and Scavenger are wired, but the existing pack spawn/sync/pickup code should be reused.
- Tracker should be implemented last because it combines hit events, team filtering, Photon sync, timers, and custom visible-through-wall rendering.
