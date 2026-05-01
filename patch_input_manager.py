"""
patch_input_manager.py

Reads Unity's mainData binary, patches the InputManager to add
joystick axes 3-10 (axis index 2-9 zero-based), then saves back.

Run from project root:
    .venv\Scripts\python.exe patch_input_manager.py
"""
import sys, shutil, copy
sys.path.insert(0, r'D:\Projects\copsnrobbers\.venv\Lib\site-packages')

from UnityPy import load

MAIN_DATA = r'D:\Projects\copsnrobbers\APK_Build_Active\apk_source\assets\bin\Data\mainData'

# Load the file
env = load(MAIN_DATA)

patched = False
for obj in env.objects:
    if obj.type.name != 'InputManager':
        continue

    d = obj.read()

    # Find a JoystickAxis template (Horizontal or Vertical entry, type=2)
    template = next((ax for ax in d.m_Axes if ax.type == 2), None)
    if template is None:
        print("ERROR: no existing JoystickAxis entry found to use as template")
        sys.exit(1)

    # Check which axis indices are already present
    existing_indices = {ax.axis for ax in d.m_Axes if ax.type == 2}
    existing_names   = {ax.m_Name for ax in d.m_Axes}

    print("Existing JOY axes:", sorted(existing_indices),
          "names:", sorted(n for n in existing_names))

    added = []
    for i in range(2, 10):                # axes 2-9 (0-indexed) == "Joystick Axis 3"-"Joystick Axis 10"
        name = 'Joystick Axis ' + str(i + 1)
        if i in existing_indices or name in existing_names:
            print("  SKIP (already present): " + name)
            continue
        new_ax = copy.deepcopy(template)
        new_ax.m_Name = name
        new_ax.axis   = i
        d.m_Axes.append(new_ax)
        added.append(name)
        print("  ADDED: " + name + "  (axis=" + str(i) + ")")

    if not added:
        print("Nothing to add - all axes already present.")
        sys.exit(0)

    obj.save_typetree(d)
    patched = True
    print("save_typetree() OK -", len(added), "new axes added")
    break

if not patched:
    print("ERROR: InputManager object not found in mainData")
    sys.exit(1)

# Write modified file back
data = env.file.save()
with open(MAIN_DATA, 'wb') as f:
    f.write(data)

print("Written:", MAIN_DATA, "  size:", len(data), "bytes")
print("Patch complete.")
