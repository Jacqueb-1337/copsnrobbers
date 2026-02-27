#!/usr/bin/env python3
"""
extract_textures.py
Extracts all Texture2D assets from CNR's Unity asset bundles (APK bin/Data/).
Handles both plain .assets files and .assets.splitN / levelN.splitN split files.
Outputs one PNG per unique texture name into out_dir.

Usage:
    python extract_textures.py [data_dir] [out_dir]

Defaults:
    data_dir = APK_Build_MainMenuHook/apk_source/assets/bin/Data
    out_dir  = temp/extracted_textures
"""

import sys
from pathlib import Path
import UnityPy
from UnityPy.enums import ClassIDType

# ── Config ────────────────────────────────────────────────────────────────────
DEFAULT_DATA = Path(__file__).parent / "APK_Build_MainMenuHook/apk_source/assets/bin/Data"
DEFAULT_OUT  = Path(__file__).parent / "temp/extracted_textures"

data_dir = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_DATA
out_dir  = Path(sys.argv[2]) if len(sys.argv) > 2 else DEFAULT_OUT

out_dir.mkdir(parents=True, exist_ok=True)


def fmt_name(tex_data):
    """Return texture format as string; handles raw int (Unity 4.x) or enum."""
    try:
        fmt = tex_data.m_TextureFormat
        return fmt.name if hasattr(fmt, "name") else str(fmt)
    except Exception:
        return "?"


def extract_from_env(env, source_label, seen, saved, skipped, errors):
    for obj in env.objects:
        if obj.type != ClassIDType.Texture2D:
            continue
        try:
            data = obj.read()
            name = data.m_Name
            if not name:
                continue
            if name in seen:
                skipped += 1
                continue
            seen.add(name)

            img = data.image  # PIL Image (None if decode failed)
            if img is None:
                skipped += 1
                continue

            safe = "".join(c if (c.isalnum() or c in "_-") else "_" for c in name)
            out_path = out_dir / f"{safe}.png"
            img.save(str(out_path))
            saved += 1
            print(f"  [{saved:4d}] {name}  ({data.m_Width}x{data.m_Height} {fmt_name(data)})  <- {source_label}")
        except Exception as e:
            errors += 1
            print(f"  ERR  {source_label}: {e}")
    return saved, skipped, errors


# ── Inventory all files ───────────────────────────────────────────────────────
plain_files   = []   # files with no suffix (hash-named bundles)
assets_files  = []   # plain .assets files without splits
split_groups  = {}   # base_stem -> sorted list of (split_num, Path)

for f in sorted(data_dir.iterdir()):
    if not f.is_file():
        continue
    if f.suffix == "":
        plain_files.append(f)
    elif f.suffix == ".assets":
        assets_files.append(f)
    elif f.suffix.startswith(".split"):
        try:
            split_num = int(f.suffix[6:])   # ".split0" -> 0, ".split10" -> 10
        except ValueError:
            continue
        base = f.stem                        # "sharedassets1.assets" or "level1"
        split_groups.setdefault(base, []).append((split_num, f))

# Sort each group by split index
for base in split_groups:
    split_groups[base].sort(key=lambda x: x[0])

total_groups = len(plain_files) + len(assets_files) + len(split_groups)
print(f"Asset sources: {len(plain_files)} hash bundles, {len(assets_files)} plain .assets, "
      f"{len(split_groups)} split groups")
print(f"Output: {out_dir}\n")

saved   = 0
skipped = 0
errors  = 0
seen    = set()   # deduplicate by texture name


# ── 1. Hash-named bundle files ────────────────────────────────────────────────
for bundle_path in plain_files:
    try:
        env = UnityPy.load(str(bundle_path))
        saved, skipped, errors = extract_from_env(env, bundle_path.name, seen, saved, skipped, errors)
    except Exception as e:
        errors += 1
        print(f"  ERR loading {bundle_path.name}: {e}")


# ── 2. Plain .assets files ────────────────────────────────────────────────────
for assets_path in assets_files:
    try:
        env = UnityPy.load(str(assets_path))
        saved, skipped, errors = extract_from_env(env, assets_path.name, seen, saved, skipped, errors)
    except Exception as e:
        errors += 1
        print(f"  ERR loading {assets_path.name}: {e}")


# ── 3. Split asset groups (concatenate in memory, then load) ──────────────────
for base, splits in sorted(split_groups.items()):
    label = f"{base} ({len(splits)} splits)"
    print(f"Loading {label}...")
    try:
        combined = b"".join(f.read_bytes() for _, f in splits)
        env = UnityPy.load(combined)
        before = saved
        saved, skipped, errors = extract_from_env(env, base, seen, saved, skipped, errors)
        if saved == before:
            print(f"  (no new Texture2D)")
    except Exception as e:
        errors += 1
        print(f"  ERR loading {base}: {e}")


print(f"\nDone. Saved={saved}  Skipped(dupe/undecodable)={skipped}  Errors={errors}")
print(f"Output: {out_dir}")
