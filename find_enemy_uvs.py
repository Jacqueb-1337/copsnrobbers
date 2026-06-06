#!/usr/bin/env python3
"""Find enemy model UV regions across all split0 asset files."""
import UnityPy
from pathlib import Path
from UnityPy.enums import ClassIDType

DATA_DIR = Path("APK_Build_Active/apk_source/assets/bin/Data")
TEX_W, TEX_H = 64, 32

ENEMY_PARTS = {'leg_left','leg_right','hand_left','hand_right','head001',
               'leg_Left','leg_Right','hand_Left','hand_Right'}

for asset_file in sorted(DATA_DIR.iterdir()):
    name = asset_file.name
    if not (name.endswith(".split0") or name.endswith(".assets")):
        continue
    try:
        env = UnityPy.load(str(asset_file))
    except Exception:
        continue

    for obj in env.objects:
        if obj.type != ClassIDType.Mesh:
            continue
        try:
            m = obj.read()
            nm = m.m_Name
            if nm not in ENEMY_PARTS:
                continue
            obj_text = m.export()
            uvs = []
            for line in obj_text.splitlines():
                if line.startswith("vt "):
                    p = line.split()
                    uvs.append((float(p[1]), float(p[2])))
            if uvs:
                us = [u for u, v in uvs]
                vs = [v for u, v in uvs]
                u0, u1 = min(us), max(us)
                v0, v1 = min(vs), max(vs)
                px_x0 = round(u0 * TEX_W)
                px_x1 = round(u1 * TEX_W)
                px_y0 = round((1 - v1) * TEX_H)
                px_y1 = round((1 - v0) * TEX_H)
                print(f"{nm:<15} px=({px_x0},{px_y0})..({px_x1},{px_y1})  uv=({u0:.4f},{v0:.4f})..({u1:.4f},{v1:.4f})  from={name}")
        except Exception:
            pass
