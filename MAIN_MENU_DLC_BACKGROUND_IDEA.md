# Future idea: DLC maps as rotating main-menu backgrounds

## What revealed this possibility

While debugging Raid multiplayer joins, a prebuilt Raid map root accidentally survived the transition back to the main menu. The menu camera then rendered part of the live Raid geometry through the menu scene's normal camera movement, fog, and lighting. It looked like a rotating Raid image with an orange haze.

The cause was the DLC pre-join path calling `DontDestroyOnLoad(_mapRoot)`. The same root was later activated for the match, but it remained persistent after leaving the match. This is a lifecycle bug for normal gameplay, but it demonstrates that live DLC map geometry can be rendered inside the main-menu scene.

Relevant code is in:

- `cnr-revived-web/mods/CNRMod/CNRDLCMapLoader.cs`
- `CNRDLCMapLoader.BuildScene(bool prejoin)`
- `CNRDLCMapLoader.FinalizePrebuiltScene()`
- `CNRDLCMapLoader.ClearActive()` and scene-transition cleanup

## Captured vanilla MainMenu atmosphere

A temporary DLL-only runtime probe on WSA captured the actual `MainMenu` render state on 2026-08-29. The accidental Raid preview was not using a special DLC effect. It was simply being rendered by the existing menu atmosphere.

- Fog: enabled, `Linear`
- Fog color: `(0.706, 0.464, 0.067, 1)`, approximately RGB `(180, 118, 17)`
- Fog start: `4`
- Fog end: `80`
- Fog density field: `0.01` although linear mode uses start/end distances
- Ambient light: `(0.2, 0.2, 0.2, 1)`
- Skybox: `mysky_1.6`
- Skybox shader: `RenderFX/Skybox`
- Main camera: FOV `60`, near `0.3`, far `1000`, depth `-1`, `Skybox` clear flags
- Main camera components include `Blur` and `CameraRotation`
- Main directional light: white, intensity `0.74`, rotation `(50, 330, 0)`, no shadows
- Additional point light: white, intensity `1`, range `10`, no shadows
- A separate NGUI camera renders layer 8 at depth `0`. The main camera culling mask excludes that UI layer.

This gives us an exact recipe for intentionally recreating the same orange, hazy, slowly moving presentation later. A dedicated menu-preview map should inherit or copy these menu values instead of using the gameplay readability-lighting setup.

## Possible intentional implementation later

Do not reuse the gameplay `_mapRoot` directly. Create a separate menu-preview system so gameplay state, collisions, spawn metadata, water, projectile helpers, and Photon state cannot leak into the menu.

A reasonable design would be:

1. Add a dedicated `CNRMenuMapPreview` component that only exists in the main-menu scene.
2. Pick from downloaded DLC maps, optionally from a configured allow-list.
3. Load the selected map package through the existing DLC parser and mesh builders, but create a separate preview root such as `CNRMenuMapPreviewRoot`.
4. Build render geometry only. Skip collision boxes, water gameplay volumes, ladders, spawn remapping, projectile helpers, nav-grid invalidation, and any multiplayer hooks.
5. Position and scale the preview root specifically for the main-menu camera instead of using gameplay spawn coordinates.
6. Let the existing menu camera movement provide the slow rotating/parallax effect, or add a controlled preview camera orbit if needed.
7. Apply menu-specific lighting and fog rather than calling the gameplay readability-lighting routine.
8. Keep only one preview map instantiated at a time. When cycling, destroy the old preview root and release its textures/materials before constructing the next one.
9. Prefer already-downloaded maps. Never start a large map download just because it was selected as a background.
10. Add a setting later for Off, Random Downloaded Maps, or a user-selected map list, plus a configurable cycle interval.

## Performance considerations

The gameplay DLC loader can build a lot of collision and helper state that a menu background does not need. A render-only preview path should be much cheaper. We can also consider building lower-detail preview meshes or caching a menu-ready representation if full maps prove expensive on older devices.

Only construct the next map when the menu is idle, and destroy the previous one before building another to keep memory bounded.

## Important separation rule

The accidental behavior came from a gameplay root surviving scene changes. The intentional feature should never depend on that bug. Gameplay DLC roots must still be destroyed or made scene-local when leaving a match. The future menu background should use its own lifecycle and its own root object.
