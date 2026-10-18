# GraphicsSettings for Cops N Robbers

Experimental client-side graphics controls. This is a separate CNR mod, not a Project V7 / ARM64 wrapper modification.

## Dependency

- **CNRSettingsMod 3.1.110 or newer (required)**. The repository manifest includes this dependency.
- The game mod loader places GraphicsSettings.dll alongside CNRSettingsMod.dll inside the CNRMods folder.
- The controls are under Settings > GRAPHICS, using the host's external tabs API.

## Features in 0.1.0

| Feature | Behavior |
| --- | --- |
| LOD bias | Sets Unity's global LOD bias and our manual runtime LOD switcher. Camera zoom affects selected detail. |
| Experimental automatic LOD | Vertex-cluster simplification of eligible individual single-submesh world meshes. **Off by default.** |
| Texture resolution | Full / half / quarter via Unity masterTextureLimit. Requires existing or generated mipmaps. |
| Texture filtering | Point / bilinear / trilinear on world textures. Trilinear needs mipmaps. |
| Experimental generate mipmaps | Mipmapped texture copy via GPU blit + readback, at most 32 textures and 32 MiB estimated per map. **Off by default.** |
| Ambient light | Multiplies ambient light on shaders affected by ambient illumination. |
| Screen dimming | Black overlay up to 70%; dims UI too. This is NOT true brightness or gamma. |
| Frame rate cap | Game default / 30 / 60 / 90 / 120; some mobile drivers ignore this. |
| Statistics | Counts generated geometry LODs and textures. |

## Limitations

- Built against CNR's Unity 4.6 managed DLLs. LODGroup.SetLODs() is absent; detail switching uses projected screen height and camera FOV.
- Physics colliders, PhotonView parents, players and animation hierarchies are not modified.
- Small separate cubes are not simplified. This is an *initial* geometry-reduction system, not Distant Horizons-style spatial region meshing.
- CPU-unreadable meshes are skipped.
- Mipmap generation uses synchronous GPU readbacks, and might cause brief hitches or texture seams. Test with small maps first.
- No persistent disk cache. Generated meshes and textures are retained during a scene only.
- Gamma correction requires a compatible compiled Unity-4.6 postprocessing shader and is **not yet implemented**. No fake slider is exposed.
- The mod is compiled but **not yet run-tested**. The local adb devices command returned no connected devices. Do not publish widely before validation.

## Build

Run the project's standard APK_Build_Active/build_mod.ps1, supplying the absolute path to GraphicsSettings.cs, OutName GraphicsSettings and NoDeploy. The build automatically includes MeshReduction.cs.

## Test sequence

1. Install CNRSettingsMod 3.1.110 or newer with GraphicsSettings.dll.
2. Confirm the GRAPHICS tab and restore-defaults action work.
3. Check texture filtering first, then mipmap generation alone on a downloaded map. Watch for texture seams, visual changes and hitches.
4. Enable automatic LOD separately; inspect generator statistics and verify scoped views and long-distance windows.
5. Confirm scene changes, restarts, persisted prefs, and multiplayer state remain intact.
6. Compare frame times with both experimental switches OFF and ON, including the ARM64 wrapper.
