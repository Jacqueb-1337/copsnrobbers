# CNR Map Exporter

A client-side Fabric mod for Minecraft 1.21.1 that exports Minecraft builds into compact DLC map packages for **Cops N Robbers Revived**.

The exporter captures visible block geometry, textures, collision, team spawn markers, and Minecraft climbable blocks. Exported maps are written as self-contained JSON files that can be uploaded to a CNR content portal and loaded by CNRMod.

## Requirements

- Minecraft 1.21.1
- Fabric Loader 0.19.3 or newer
- Fabric API
- Java 21

This is a client-side mod. It does not need to be installed on a Minecraft server.

## Installation

1. Install Fabric Loader for Minecraft 1.21.1.
2. Install Fabric API.
3. Put the CNR Map Exporter JAR in your Minecraft `mods` folder.
4. Launch Minecraft with the Fabric profile.

## Usage

Set the two corners of the area you want to export:

```text
/cnr pos1
/cnr pos2
```

With no coordinates, each command uses the block you are looking at. If you are not looking at a block, it uses your current block position.

You can also provide absolute or relative coordinates:

```text
/cnr pos1 100 64 100
/cnr pos2 ~20 ~10 ~-30
```

The selected region is outlined in-game. Clear it with:

```text
/cnr clear
```

Export it with:

```text
/cnr export shipment
```

Exports run incrementally so large selections do not lock up the game. Check progress or cancel a running export with:

```text
/cnr status
/cnr cancel
```

Exports are written to:

```text
.minecraft/cnr_exports/<name>.json
```

## Team spawn markers

Place armor stands inside the selection and give them one of these exact custom names, case-insensitive:

- `Cops`
- `Robbers`

Their positions are exported as team spawn locations. If no markers are present, the CNR loader uses its normal fallback behavior.

## Climbable blocks

Blocks included in Minecraft's `#minecraft:climbable` tag are exported as climbable volumes. This includes vanilla ladders and vines, and can also include modded blocks that correctly use the tag.

## Barrier blocks

Minecraft barrier blocks are exported as invisible solid collision, but in a separate bullet-pass-through collision channel. With CNRMod 4.2.4 or newer, players cannot walk through barriers while normal bullets can pass through them.

## Large and sparse selections

The export limit is based on **non-air blocks**, not the raw rectangular selection volume. Large regions containing mostly air can therefore be exported without wasting the limit on empty space.

The current limit is 4,000,000 non-air blocks per export.

In single-player, the exporter reads the integrated server world so it can access the full selected area beyond the current render distance. In multiplayer, all selected chunks must already be loaded to avoid silently exporting unloaded chunks as air.

## Output format

The exporter produces compact CNR DLC map JSON containing:

- block render geometry and texture atlas data
- packed LZ4 mesh data
- block collision volumes
- bullet-pass-through barrier collision volumes
- climbable volumes
- Cops and Robbers spawn markers
- map metadata used by CNRMod

Air blocks are omitted.

## Links

- Cops N Robbers Revived: https://play.jacqueb.me/
- Source: https://github.com/Jacqueb-1337/copsnrobbers
- Issues: https://github.com/Jacqueb-1337/copsnrobbers/issues

## License

CC0-1.0. See `LICENSE`.
