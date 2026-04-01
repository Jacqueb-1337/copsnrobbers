import UnityPy
from UnityPy.enums import ClassIDType
from pathlib import Path

DATA_DIR = Path("APK_Build_Active/apk_source/assets/bin/Data")
TEX_W, TEX_H = 64, 32

TARGETS = {"head","trunk","legleft","legright","handleft","handright","hat","head001"}

f = DATA_DIR / "sharedassets1.assets.split0"
env = UnityPy.load(str(f))
seen = set()

print(f"{'Part':<14} {'px_x0':>5} {'px_y0':>5} {'px_x1':>5} {'px_y1':>5}   uv_u0  uv_v0  uv_u1  uv_v1")
print("-" * 80)

for obj in env.objects:
    if obj.type != ClassIDType.Mesh:
        continue
    m = obj.read()
    name = m.m_Name
    if name.lower() not in TARGETS or name in seen:
        continue
    seen.add(name)

    obj_text = m.export()
    # parse vt lines
    uvs = []
    for line in obj_text.splitlines():
        if line.startswith("vt "):
            parts = line.split()
            uvs.append((float(parts[1]), float(parts[2])))

    if not uvs:
        print(f"{name:<14}  no UVs")
        continue

    us = [uv[0] for uv in uvs]
    vs = [uv[1] for uv in uvs]
    u0, u1 = min(us), max(us)
    v0, v1 = min(vs), max(vs)

    # OBJ vt v=0 = bottom of image → flip for PNG coords
    px_x0 = round(u0 * TEX_W)
    px_x1 = round(u1 * TEX_W)
    px_y0 = round((1 - v1) * TEX_H)
    px_y1 = round((1 - v0) * TEX_H)

    print(f"{name:<14} {px_x0:>5} {px_y0:>5} {px_x1:>5} {px_y1:>5}   {u0:.4f} {v0:.4f} {u1:.4f} {v1:.4f}")
