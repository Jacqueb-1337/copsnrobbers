import argparse
from pathlib import Path
from zipfile import ZipFile

p = argparse.ArgumentParser()
for name in ("template", "arm64", "x86", "output"):
    p.add_argument("--" + name, type=Path, required=True)
a = p.parse_args()
replacements = {
    "lib/arm64-v8a/libpv7host.so": a.arm64.read_bytes(),
    "lib/x86_64/libpv7host.so": a.x86.read_bytes(),
}
a.output.parent.mkdir(parents=True, exist_ok=True)
seen = set()
with ZipFile(a.template) as source, ZipFile(a.output, "w", allowZip64=True) as target:
    for item in source.infolist():
        if item.filename.startswith("META-INF/") and item.filename.upper().endswith((".SF", ".RSA", ".DSA", ".EC", ".MF")):
            continue
        if item.filename in replacements:
            data = replacements[item.filename]
            seen.add(item.filename)
        else:
            data = source.read(item.filename)
        target.writestr(item, data)
if seen != replacements.keys():
    a.output.unlink(missing_ok=True)
    raise RuntimeError("Missing native ABI entries: " + str(replacements.keys() - seen))
print("PV7_COCOS_TEMPLATE_REPACK_OK", a.output)