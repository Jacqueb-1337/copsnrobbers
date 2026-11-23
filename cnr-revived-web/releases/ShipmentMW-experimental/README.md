# Shipment MW (CODM source import)

This map is distinct from the Minecraft recreation of Shipment. The original shipment DLC is unchanged.

- Map ID: shipmentmw
- Display name: Shipment MW (CODM, Experimental)
- Map file: https://play.jacqueb.me/economy/uploads/maps/shipmentmw.json
- Required CNRMod: 4.2.56 or newer
- Stable CNRMod: https://play.jacqueb.me/mods/CNRMod/CNRMod-4.2.56.dll

The normal 4.2.56 update supersedes the 4.2.55 experimental core test build. Prior CNRMod versions cannot decode native collisionPacked meshes. This map still needs in-game testing: collision physics, team spawn assignment, full materials, lighting and performance.

## Ground-collision repair, November 23, 2026

The original imported 42 native colliders did not supply any walkable ground beneath the 31 spawn points. Seven additional static collider meshes were reconstructed from matching visible ground surfaces (8,342 triangles). This covers 31/31 spawns and 499/499 sampled walkable grid points in the main area. Rendered triangles remain unchanged. The public map hash changed, prompting a fresh download. No new CNRMod core DLL is required beyond 4.2.56.

The patch is geometrically verified and parsed with the production CNRMod JsonFx contract; complete in-game physics has not been verified because an extended phone test reached excessive CPU temperatures.

CNRSettings 3.1.111 separately suppresses the inaccurate 'APK update required' warning when the PV7 Unity JNI bridge reports an empty versionName. On actual confirmed non-CNR versions the warning remains.

## Full ground-footprint collision coverage, November 23, 2026

The first ground repair left small uncovered visual-floor regions. A denser verification sampled 79,605 points over x=-21..44 and z=-87..-11 at 0.25-metre spacing. The refined map provides collision geometry at all 45,944 points identified as visible walkable surfaces, while preserving all 31 original spawns. Its 24 added floor collision meshes are reconstructed from existing upward-facing triangles (10,675 added floor triangles); along with 42 original native collider meshes, the package contains 66 collider parts. Render geometry and textures are unchanged. The updated DLC hash prompts clients to refresh the map. Production CNRMod 4.2.56 JsonFx typed deserialization passes. On-device Unity physics testing is still pending because sustained PV7 startup tests cause excessive phone CPU temperatures. This map-only update does not require another CNRMod DLL.
