#!/usr/bin/env python3
"""
merge_enemy_obj.py
Merges individual enemy character part OBJ files into a single OBJ
suitable for the skin_editor.html CHARACTER_OBJ format.
Each part gets a 'g groupname' marker, and vertex/UV/normal indices
are re-numbered so they are globally unique.
"""
import re

PARTS = [
    ("head",        "enemy_head.obj"),
    ("hat",         None),          # enemy has no hat - skip
    ("trunk",       "enemy_trunk.obj"),
    ("leg_left",    "enemy_leg_left.obj"),
    ("leg_right",   "enemy_leg_right.obj"),
    ("hand_left",   "enemy_hand_left.obj"),
    ("hand_right",  "enemy_hand_right.obj"),
]

all_lines = []
v_offset  = 0
vt_offset = 0
vn_offset = 0

for group_name, filename in PARTS:
    if filename is None:
        continue

    with open(filename) as f:
        lines = f.readlines()

    # Count v / vt / vn in this file
    nv = sum(1 for l in lines if l.startswith("v "))
    nt = sum(1 for l in lines if l.startswith("vt "))
    nn = sum(1 for l in lines if l.startswith("vn "))

    # Emit group header
    all_lines.append(f"g {group_name}\n")

    for line in lines:
        stripped = line.strip()
        if stripped.startswith("g ") or stripped.startswith("mtllib ") or stripped.startswith("usemtl ") or stripped == "":
            continue  # skip old group/material markers

        if stripped.startswith("f "):
            # Re-index face: tokens like v/vt/vn or v//vn or v/vt
            tokens = stripped.split()
            new_tokens = ["f"]
            for tok in tokens[1:]:
                indices = tok.split("/")
                vi  = int(indices[0]) + v_offset  if len(indices) > 0 and indices[0] else ""
                vti = int(indices[1]) + vt_offset if len(indices) > 1 and indices[1] else ""
                vni = int(indices[2]) + vn_offset if len(indices) > 2 and indices[2] else ""
                if vni != "" and vti != "":
                    new_tokens.append(f"{vi}/{vti}/{vni}")
                elif vti != "":
                    new_tokens.append(f"{vi}/{vti}")
                elif vni != "":
                    new_tokens.append(f"{vi}//{vni}")
                else:
                    new_tokens.append(str(vi))
            all_lines.append(" ".join(new_tokens) + "\n")
        else:
            all_lines.append(line)

    v_offset  += nv
    vt_offset += nt
    vn_offset += nn

output = "".join(all_lines)
with open("enemy_combined.obj", "w") as f:
    f.write(output)

print(f"Written enemy_combined.obj ({len(all_lines)} lines)")
print(f"Final offsets: v={v_offset} vt={vt_offset} vn={vn_offset}")
