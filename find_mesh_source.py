import UnityPy
from UnityPy.enums import ClassIDType
from pathlib import Path

DATA_DIR = Path("APK_Build_Active/apk_source/assets/bin/Data")
targets = {"head","trunk","legleft","legright","handleft","handright","hat","head001",
           "leg_left","leg_right","hand_left","hand_right"}

for f in sorted(DATA_DIR.iterdir()):
    n = f.name
    # skip split1+ to avoid duplicate loading, but allow all .assets and split0
    pass
    try:
        env = UnityPy.load(str(f))
    except Exception:
        continue
    for obj in env.objects:
        if obj.type != ClassIDType.Mesh:
            continue
        try:
            m = obj.read()
            if m.m_Name.lower() in targets:
                try:
                    uvs = m.m_UV0
                    uv_count = len(uvs) if uvs else 0
                except Exception as e:
                    uv_count = f"ERR:{e}"
                print(f"{f.name}  mesh={m.m_Name!r}  uvs={uv_count}")
        except Exception as e:
            pass
