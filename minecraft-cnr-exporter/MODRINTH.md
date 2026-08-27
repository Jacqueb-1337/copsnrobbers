# Modrinth Publishing Sheet

## Project

**Title:** CNR Map Exporter

**Suggested slug:** `cnr-map-exporter`

**Project type:** Mod

**Loader:** Fabric

**Minecraft version:** 1.21.1

**Environment:** Client required, server unsupported

**License:** CC0-1.0

**Suggested category:** Utility

**Summary:**

Export Minecraft builds into compact Cops N Robbers DLC map packages with textures, collision, team spawns, and climbable surfaces.

## Description

CNR Map Exporter is a client-side Fabric mod for Minecraft 1.21.1 that converts Minecraft builds into self-contained DLC map packages for **Cops N Robbers Revived**.

Select any rectangular region using `/cnr pos1` and `/cnr pos2`, then run `/cnr export <name>`. The exporter captures block render geometry, a compact texture atlas, collision, Cops/Robbers spawn markers, and Minecraft climbable blocks such as ladders and vines. The resulting JSON can be uploaded to a CNR content portal and loaded by CNRMod.

### Features

- Visual in-world selection outline
- Absolute and relative coordinate selection
- Compact packed/LZ4 map output
- Air blocks omitted from export
- Export limit based on non-air blocks rather than bounding-box volume
- Minecraft block collision exported for CNR physics
- `Cops` and `Robbers` armor-stand spawn markers
- Automatic export of blocks in `#minecraft:climbable`
- Large exports run incrementally instead of blocking the render thread
- Live progress via `/cnr status` and cancellation via `/cnr cancel`
- Single-player chunk loading/scanning is scheduled through the integrated server thread
- Expensive final packing/compression/JSON writing runs on a background writer thread
- Single-player exports can read beyond current render distance through the integrated server world
- Safe multiplayer behavior that refuses unloaded selected chunks instead of treating them as air

### Basic usage

```text
/cnr pos1
/cnr pos2
/cnr export my_map
```

You can also specify coordinates directly, including relative coordinates:

```text
/cnr pos1 100 64 100
/cnr pos2 ~20 ~10 ~-30
```

Exported files are saved under `.minecraft/cnr_exports/`.

### Requirements

- Minecraft 1.21.1
- Fabric Loader 0.19.3+
- Fabric API
- Java 21

This mod is client-side only and does not need to be installed on a Minecraft server.

### Links

- Website: https://play.jacqueb.me/
- Source: https://github.com/Jacqueb-1337/copsnrobbers
- Issues: https://github.com/Jacqueb-1337/copsnrobbers/issues

## Version 0.1.10

**Version number:** `0.1.10`

**Version title:** `CNR Map Exporter 0.1.10`

**Version type:** Beta

**Game version:** 1.21.1

**Loader:** Fabric

**Primary file:** `cnrmapexporter-0.1.10.jar`

**Required dependency:** Fabric API

**Changelog:**

- Water rendering now culls faces that are flush against full solid neighboring blocks, eliminating water/terrain z-fighting around pools.
- Water gameplay volumes and the 0.1.9 biome-tint fallback remain unchanged.

## Version 0.1.9

**Version number:** `0.1.9`

**Version title:** `CNR Map Exporter 0.1.9`

**Version type:** Beta

**Game version:** 1.21.1

**Loader:** Fabric

**Primary file:** `cnrmapexporter-0.1.9.jar`

**Required dependency:** Fabric API

**Changelog:**

- Biome-tinted grass, foliage, vines, and lily pads now receive a default Minecraft tint even if the baked quad incorrectly reports itself as untinted.
- A failed biome lookup that returns pure white no longer suppresses the known fallback tint for vegetation.
- Water gameplay-volume export from 0.1.8 remains unchanged.

## Version 0.1.8

**Version number:** `0.1.8`

**Version title:** `CNR Map Exporter 0.1.8`

**Version type:** Beta

**Game version:** 1.21.1

**Loader:** Fabric

**Primary file:** `cnrmapexporter-0.1.8.jar`

**Required dependency:** Fabric API

**Changelog:**

- Water now exports compact gameplay volumes in addition to its transparent render geometry.
- Adjacent water blocks are merged before packing, avoiding thousands of runtime water objects on large maps.
- CNR can use the exported volumes for classic upright swimming, surface bobbing, and water-aware AI navigation.
- Existing biome water tint, vegetation tint fallback, collision, climbables, barriers, packed LZ4 geometry, and incremental export behavior remain unchanged.

## Icon

Use `src/main/resources/assets/cnrmapexporter/icon.png` for the Modrinth project icon. It is the same icon served by `https://play.jacqueb.me/icon.png`.

## Content disclosure

Modrinth currently requires disclosure when generative AI contributed significantly to project code or written project content. A concise disclosure for the project form is:

> Generative AI was used as a development assistant for code iteration, debugging, and documentation. Project requirements, testing, and release decisions were human-directed.

Modrinth currently prohibits AI-generated or AI-assisted project icons. Use the existing `play.jacqueb.me` icon only if that source image itself was not AI-generated.

## Before submitting for review

- Upload the primary JAR from `build/libs/cnrmapexporter-0.1.10.jar`.
- Mark Fabric API as a required dependency.
- Set client environment to required and server environment to unsupported.
- Add the project icon.
- Complete Modrinth's content disclosure form accurately.
