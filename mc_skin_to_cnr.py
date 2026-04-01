#!/usr/bin/env python3
"""
mc_skin_to_cnr.py  —  Convert a Minecraft skin (64x64) to CNR format.

CNR uses TWO different 64x32 texture files per skin:

  Res_Skin_N.png  — store/profile preview model (Minecraft-compatible layout)
                    This is just the top 64x32 crop of the Minecraft skin.

  Skin_N_1.png    — in-game character model (CNR native layout)
                    The head faces are stored in a different arrangement:

    Minecraft 64x64 head layout:       CNR native Skin_N_1 (64x32) layout:
      y=0-7:  [blank][top][bot][blank]   y=0-7:  [rt][fr][lt][bk][top][bot][--][--]
      y=8-15: [rt][fr][lt][bk]           y=8-15: [hrt][hfr][hlt][hbk][htop][hbot][--][--]
                                          (each block is 8x8 px)
    Body/arms/legs (y=16-31) layout is identical in both formats.

Usage:
    python mc_skin_to_cnr.py <input_skin.png>

Outputs (written next to input):
    <stem>_res.png      — Res_Skin format (RGBA, Minecraft-compatible)
    <stem>_ingame.png   — Skin_N_1 format (RGB, CNR native head layout)
"""

import sys
from pathlib import Path
from PIL import Image


def _paste(dst: Image.Image, src: Image.Image, dst_box, src_box):
    """Crop src_box from src and paste at dst_box in dst."""
    region = src.crop(src_box)
    dst.paste(region, (dst_box[0], dst_box[1]))


def convert_res(mc: Image.Image) -> Image.Image:
    """Produce Res_Skin_N.png: top 64x32 crop, RGBA."""
    return mc.crop((0, 0, 64, 32)).convert("RGBA")


def convert_ingame(mc: Image.Image) -> Image.Image:
    """Produce Skin_N_1.png: CNR native head layout + same body, RGB."""
    out = Image.new("RGB", (64, 32), (0, 0, 0))

    # --- Head faces (Minecraft y=8-15 → CNR y=0-7) ---
    _paste(out, mc, (0,  0), (0,  8,  8, 16))   # right
    _paste(out, mc, (8,  0), (8,  8, 16, 16))   # front
    _paste(out, mc, (16, 0), (16, 8, 24, 16))   # left
    _paste(out, mc, (24, 0), (24, 8, 32, 16))   # back
    _paste(out, mc, (32, 0), (8,  0, 16,  8))   # top
    _paste(out, mc, (40, 0), (16, 0, 24,  8))   # bottom
    # x=48-63, y=0-7: unused (black)

    # --- Hat/overlay faces (Minecraft x=32-63, y=0-15 → CNR y=8-15) ---
    _paste(out, mc, (0,  8), (32, 8, 40, 16))   # hat right
    _paste(out, mc, (8,  8), (40, 8, 48, 16))   # hat front
    _paste(out, mc, (16, 8), (48, 8, 56, 16))   # hat left
    _paste(out, mc, (24, 8), (56, 8, 64, 16))   # hat back
    _paste(out, mc, (32, 8), (40, 0, 48,  8))   # hat top
    _paste(out, mc, (40, 8), (48, 0, 56,  8))   # hat bottom
    # x=48-63, y=8-15: unused (black)

    # --- Body / arms / legs (y=16-31): same layout in both formats ---
    _paste(out, mc, (0, 16), (0, 16, 64, 32))

    return out


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
