#!/usr/bin/env python3
"""
dump_skin_uvs.py
Scans all Unity asset files for Mesh objects whose names match CNR character
parts (head, trunk, leg, hand) and prints the UV bounding box for each in
both 0-1 space and pixel space (assuming 64x32 texture = Skin_N_1.png).
"""
import sys
from pathlib import Path
import UnityPy
from UnityPy.enums import ClassIDType

DATA_DIR = Path("APK_Build_Active/apk_source/assets/bin/Data")
TEX_W, TEX_H = 64, 32   # Skin_N_1.png dimensions

PART_KEYWORDS = ["head", "trunk", "legleft", "legright", "handleft", "handright",
                 "leg_left", "leg_right", "hand_left", "hand_right", "hat"]

results = {}

for asset_file in sorted(DATA_DIR.iterdir()):
    # Only scan the split0 + unsplit .assets files (skip split1-N to avoid dupes)
    name = asset_file.name
    if not (name.endswith(".assets") or name.endswith(".split0")):
        continue
    try:
        env = UnityPy.load(str(asset_file))
    except Exception:
        continue

    for obj in env.objects:
        if obj.type != ClassIDType.Mesh:
            continue
        try:
            mesh = obj.read()
            mname = (mesh.m_Name or "").lower()
        except Exception:
            continue

        if not any(mname == kw for kw in PART_KEYWORDS):
            continue

        try:
            uvs = mesh.m_UV0  # list of (u, v)
            if not uvs:
                continue
        except Exception:
            continue

        us = [uv[0] for uv in uvs]
        vs = [uv[1] for uv in uvs]
        u_min, u_max = min(us), max(us)
        v_min, v_max = min(vs), max(vs)

        # Unity UV: v=0 is bottom, v=1 is top → flip for image coords
        px_x0 = round(u_min * TEX_W)
        px_x1 = round(u_max * TEX_W)
        # Flip V: image row 0 = UV v=1
        px_y0 = round((1 - v_max) * TEX_H)
        px_y1 = round((1 - v_min) * TEX_H)

        key = mesh.m_Name
        if key not in results:
            results[key] = {
                "uv": (u_min, v_min, u_max, v_max),
                "px": (px_x0, px_y0, px_x1, px_y1),
                "source": name,
                "num_uvs": len(uvs),
            }

if not results:
    print("No matching meshes found.")
    sys.exit(1)

print(f"{'Mesh':<30} {'px x0':>5} {'px y0':>5} {'px x1':>5} {'px y1':>5}  {'UV u0':>6} {'UV v0':>6} {'UV u1':>6} {'UV v1':>6}  source")
print("-" * 115)
for mname, d in sorted(results.items(), key=lambda x: x[0].lower()):
    px = d["px"]
    uv = d["uv"]
    print(f"{mname:<30} {px[0]:>5} {px[1]:>5} {px[2]:>5} {px[3]:>5}  {uv[0]:>6.3f} {uv[1]:>6.3f} {uv[2]:>6.3f} {uv[3]:>6.3f}  {d['source']}")
