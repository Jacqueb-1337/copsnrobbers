import sys
sys.path.insert(0, 'D:\\Projects\\copsnrobbers\\.venv\\Lib\\site-packages')
from UnityPy import load
import os

datadir = 'D:/Projects/copsnrobbers/APK_Build_Active/apk_source/assets/bin/Data'
for fname in os.listdir(datadir):
    fpath = os.path.join(datadir, fname)
    if not os.path.isfile(fpath) or os.path.getsize(fpath) > 200000:
        continue
    try:
        env = load(fpath)
        for obj in env.objects:
            if obj.type.name == 'InputManager':
                print("FOUND InputManager in " + fname)
                d = obj.read()
                axes = getattr(d, 'm_Axes', None)
                if axes:
                    for ax in axes:
                        name = getattr(ax, 'm_Name', '?')
                        joy_axis = getattr(ax, 'm_Axis', '?')
                        ax_type = getattr(ax, 'm_Type', '?')
                        print("  name=" + str(name) + " joyAxis=" + str(joy_axis) + " type=" + str(ax_type))
    except Exception as e:
        pass
