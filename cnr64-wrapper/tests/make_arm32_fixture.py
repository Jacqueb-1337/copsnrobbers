#!/usr/bin/env python3
"""Generate a synthetic ARM32 shared object without copyrighted game files."""
import pathlib
import struct
import sys

out = pathlib.Path(sys.argv[1])
b = bytearray(2048)
b[:16] = b"\x7fELF\x01\x01\x01" + bytes(9)
struct.pack_into("<HHIIIIIHHHHHH", b, 16, 3, 40, 1, 0, 52, 0, 0, 52, 32, 2, 40, 0, 0)
struct.pack_into("<IIIIIIII", b, 52, 1, 0, 0, 0, 2048, 2048, 5, 4096)
struct.pack_into("<IIIIIIII", b, 84, 2, 0x100, 0x100, 0x100, 0x80, 0x80, 6, 4)
names = b"\0liblog.so\0JNI_OnLoad\0Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeInit\0eglMakeCurrent\0"
b[0x200:0x200+len(names)] = names
for i, (name, value, shndx) in enumerate((
    (0, 0, 0),
    (names.index(b"JNI_OnLoad"), 0x180, 1),
    (names.index(b"Java_org_"), 0x190, 1),
    (names.index(b"eglMakeCurrent"), 0, 0)
)):
    struct.pack_into("<IIIBBH", b, 0x280+i*16, name, value, 0, 0x12, 0, shndx)
struct.pack_into("<II", b, 0x300, 1, 4)  # DT_HASH chain count
struct.pack_into("<I", b, 0x360, 0x120)
struct.pack_into("<I", b, 0x500, 0x40)
dynamic = [
    (4, 0x300), (5, 0x200), (10, len(names)), (6, 0x280), (11, 16),
    (1, names.index(b"liblog.so")), (17, 0x340), (18, 24), (19, 8),
    (25, 0x360), (27, 4), (0, 0)
]
for i, (tag, val) in enumerate(dynamic):
    struct.pack_into("<II", b, 0x100+i*8, tag, val)
for i, (offset, info) in enumerate((
    (0x500, 23), (0x504, (3 << 8) | 21), (0x360, 23)
)):
    struct.pack_into("<II", b, 0x340+i*8, offset, info)
out.write_bytes(b)
