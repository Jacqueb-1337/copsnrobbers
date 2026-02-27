import UnityPy
from UnityPy.enums import ClassIDType
from pathlib import Path
from collections import Counter

data_dir = Path("D:/Projects/copsnrobbers/APK_Build_MainMenuHook/apk_source/assets/bin/Data")

# Focus on the big mystery hash file and a level file
targets = [
    "a959d20208b1a69489e9b147f982a983",  # 973 KB, 0 Texture2D
    "level20",
    "level6",
]

for name in targets:
    f = data_dir / name
    if not f.exists():
        print(f"MISSING: {name}")
        continue
    try:
        env = UnityPy.load(str(f))
        counts = Counter(str(o.type) for o in env.objects)
        print(f"\n{name} ({f.stat().st_size} bytes) type distribution:")
        for tp, c in counts.most_common(15):
            print(f"  {c:5d}  {tp}")

        # Also check all objects with image_data
        tex_like = 0
        for obj in env.objects:
            try:
                raw = obj.read()
                if hasattr(raw, 'image_data') and raw.image_data:
                    w = getattr(raw, 'm_Width', '?')
                    h = getattr(raw, 'm_Height', '?')
                    nm = getattr(raw, 'm_Name', '?')
                    tex_like += 1
                    if tex_like <= 5:
                        print(f"  image_data obj: type={obj.type} name={repr(nm)} "
                              f"size={w}x{h} data_len={len(raw.image_data)}")
            except:
                pass
        print(f"  Total objects with image_data: {tex_like}")
    except Exception as e:
        print(f"ERR {name}: {e}")


