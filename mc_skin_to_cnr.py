#!/usr/bin/env python3
"""
mc_skin_to_cnr.py  —  Convert a Minecraft skin (64x64) to CNR format.

CNR uses TWO different 64x32 texture files per skin:

  Res_Skin_N.png  — store/profile preview model (RGBA, Minecraft-compatible)
                    The top 64x32 crop of the Minecraft skin.

  Skin_N_1.png    — in-game character model (RGB, same 64x32 layout)
                    CNR mesh UVs match the Minecraft classic 64x32 layout
                    exactly (verified via dump_skin_uvs.py):
                      head/hat:  x=0-64, y=0-16  (identical face ordering)
                      body:      x=16-40, y=16-32
                      legs:      x=0-16, y=16-32  (both legs mirror right)
                      arms:      x=40-56, y=16-32 (both arms mirror right)
                    The only difference from Res_Skin is RGB vs RGBA.

Usage:
    python mc_skin_to_cnr.py <input_skin.png>

Outputs (written next to input):
    <stem>_res.png      — Res_Skin format (RGBA, Minecraft-compatible)
    <stem>_ingame.png   — Skin_N_1 format (RGB, same layout)
"""

import sys
from pathlib import Path
from PIL import Image


def convert_res(mc: Image.Image) -> Image.Image:
    """Produce Res_Skin_N.png: top 64x32 crop, RGBA."""
    return mc.crop((0, 0, 64, 32)).convert("RGBA")


def convert_ingame(mc: Image.Image) -> Image.Image:
    """Produce Skin_N_1.png: top 64x32 crop, RGB.

    CNR mesh UVs use the same layout as Minecraft classic 64x32 (confirmed by
    dump_skin_uvs.py).  The only difference from convert_res() is RGB vs RGBA.
    """
    return mc.crop((0, 0, 64, 32)).convert("RGB")


def convert(src_path: Path):
    mc = Image.open(src_path).convert("RGBA")
    w, h = mc.size

    if w != 64:
        print(f"WARNING: expected width 64, got {w}. Resizing to 64-wide.")
        mc = mc.resize((64, h), Image.NEAREST)

    if h == 32:
        print("Input is 64x32 (classic format). Treating as modern top-half only.")
        # Pad to 64x64 so face coordinates work correctly
        padded = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        padded.paste(mc, (0, 0))
        mc = padded
    elif h != 64:
        print(f"ERROR: unexpected skin height {h}. Expected 32 or 64.")
        sys.exit(1)

    res_out = src_path.with_name(src_path.stem + "_res.png")
    ingame_out = src_path.with_name(src_path.stem + "_ingame.png")

    convert_res(mc).save(res_out)
    print(f"Res_Skin format:  {res_out}")

    convert_ingame(mc).save(ingame_out)
    print(f"Ingame format:    {ingame_out}")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    src = Path(sys.argv[1])
    if not src.exists():
        print(f"ERROR: file not found: {src}")
        sys.exit(1)

    convert(src)
