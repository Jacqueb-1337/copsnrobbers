from PIL import Image

for fname, label in [('Skin_1_1','CNR native'), ('Res_Skin_1','Res/Minecraft')]:
    img = Image.open(f'temp/extracted_textures/{fname}.png').convert('RGB')
    print(f'=== {fname} ({label}) {img.size} ===')
    for row_start in range(0, 32, 8):
        row_end = row_start + 8
        cols = []
        for col_start in range(0, 64, 8):
            col_end = col_start + 8
            block = img.crop((col_start, row_start, col_end, row_end))
            pixels = list(block.getdata())
            avg = tuple(int(sum(p[c] for p in pixels)/len(pixels)) for c in range(3))
            cols.append('#{:02x}{:02x}{:02x}'.format(*avg))
        sep = '  '
        print(f'  y={row_start:02d}-{row_end-1:02d}: {sep.join(cols)}')
    print()

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


