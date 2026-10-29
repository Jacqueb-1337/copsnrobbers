#!/usr/bin/env python3
"""Offline ARMv7 Android ELF compatibility inventory.

No guest code is executed. Direct ARM/Thumb branch candidates are intentionally
NOT a complete, verified call graph. This tool never invents Android provenance.
"""
import argparse
import json
import re
import struct
from collections import Counter
from pathlib import Path

DT_NEEDED = 1
DT_HASH = 4
DT_STRTAB = 5
DT_SYMTAB = 6
DT_STRSZ = 10
DT_SYMENT = 11
DT_INIT = 12
DT_REL = 17
DT_RELSZ = 18
DT_RELENT = 19
DT_PLTRELSZ = 2
DT_JMPREL = 23
DT_INIT_ARRAY = 25
DT_INIT_ARRAYSZ = 27
ST_TYPES = {0: "NOTYPE", 1: "OBJECT", 2: "FUNC", 3: "SECTION",
            4: "FILE", 5: "COMMON", 6: "TLS"}
RELOC_TYPES = {2: "R_ARM_ABS32", 3: "R_ARM_REL32",
               21: "R_ARM_GLOB_DAT", 22: "R_ARM_JUMP_SLOT",
               23: "R_ARM_RELATIVE"}
MAX_FILE = 256 * 1024 * 1024


class ElfError(ValueError):
    pass


def read_u32(buf, offset):
    if offset < 0 or offset + 4 > len(buf):
        raise ElfError("Truncated ELF u32")
    return struct.unpack_from("<I", buf, offset)[0]


def signed(value, bits):
    sign = 1 << (bits - 1)
    return (value ^ sign) - sign


class Arm32Elf:
    def __init__(self, data):
        if len(data) < 52 or len(data) > MAX_FILE or data[:6] != b"\x7fELF\x01\x01":
            raise ElfError("Expected bounded little-endian ELF32")
        if struct.unpack_from("<HH", data, 16) != (3, 40):
            raise ElfError("Expected ARM ELF32 ET_DYN")
        self.data = data
        phoff = read_u32(data, 28)
        phentsize, phnum = struct.unpack_from("<HH", data, 42)
        if phentsize < 32 or not (1 <= phnum <= 4096) or phoff + phentsize * phnum > len(data):
            raise ElfError("Invalid program header table")
        self.segments = []
        self.dynamic = []
        for i in range(phnum):
            p = phoff + i * phentsize
            typ, off, vaddr, _, filesz, memsz, flags, _ = struct.unpack_from("<8I", data, p)
            if typ not in (1, 2):
                continue
            if filesz > memsz or off + filesz > len(data) or vaddr + memsz > 0x10000000:
                raise ElfError("Invalid segment bounds")
            if typ == 1:
                self.segments.append((vaddr, off, filesz, memsz, flags))
            else:
                self.dynamic.append((vaddr, filesz))
        if not self.segments:
            raise ElfError("No PT_LOAD segments")
        self.tags = {}
        self.tag_lists = {}
        for vaddr, size in self.dynamic:
            if size > MAX_FILE or size % 8:
                raise ElfError("Invalid PT_DYNAMIC size")
            for pos in range(0, size, 8):
                tag, val = struct.unpack("<II", self.at(vaddr + pos, 8))
                if tag == 0:
                    break
                self.tags[tag] = val
                self.tag_lists.setdefault(tag, []).append(val)
        if not self.tags.get(DT_STRTAB) or not self.tags.get(DT_STRSZ):
            raise ElfError("Missing dynamic string table")
        self.at(self.tags[DT_STRTAB], self.tags[DT_STRSZ])
        self.sym_count = 0
        if DT_HASH in self.tags:
            self.sym_count = struct.unpack("<II", self.at(self.tags[DT_HASH], 8))[1]
        if self.sym_count > 1000000:
            raise ElfError("Unreasonable dynamic symbol count")

    def at(self, address, size):
        if size < 0:
            raise ElfError("Negative read size")
        for v, off, filesz, _, _ in self.segments:
            if v <= address and size <= filesz and address - v <= filesz - size:
                start = off + address - v
                return self.data[start:start + size]
        raise ElfError(f"Guest address 0x{address:08x} not file-backed")

    def string(self, index):
        size = self.tags[DT_STRSZ]
        if index >= size:
            raise ElfError("Bad string index")
        raw = self.at(self.tags[DT_STRTAB] + index, size - index)
        nul = raw.find(b"\0")
        if nul < 0:
            raise ElfError("Unterminated dynamic string")
        return raw[:nul].decode("utf-8", errors="replace")

    def symbol(self, index):
        if index < 0 or index > 1000000:
            raise ElfError("Invalid symbol index")
        addr = self.tags.get(DT_SYMTAB)
        if not addr:
            raise ElfError("Missing DT_SYMTAB")
        syment = self.tags.get(DT_SYMENT, 16)
        if syment < 16 or syment > 256:
            raise ElfError("Invalid dynamic symbol entry size")
        name, value, size, info, other, section = struct.unpack(
            "<IIIBBH", self.at(addr + index * syment, 16))
        return {"name": self.string(name), "address": value, "size": size,
                "type": ST_TYPES.get(info & 15, str(info & 15)),
                "defined": section != 0,
                "visibility": other & 3}

    def imports(self):
        result = {}
        seen = set()
        for address_tag, count_tag in ((DT_REL, DT_RELSZ),
                                       (DT_JMPREL, DT_PLTRELSZ)):
            size = self.tags.get(count_tag, 0)
            if not size:
                continue
            address = self.tags.get(address_tag)
            if address is None or self.tags.get(DT_RELENT, 8) != 8 or size % 8:
                raise ElfError("Malformed ARM REL relocation table")
            if (address, size) in seen:
                continue
            seen.add((address, size))
            table = self.at(address, size)
            for off in range(0, size, 8):
                location, info = struct.unpack_from("<II", table, off)
                kind, symindex = info & 255, info >> 8
                if kind == 23 or symindex == 0:
                    continue
                symbol = self.symbol(symindex)
                if symbol["defined"] or not symbol["name"]:
                    continue
                name = symbol["name"]
                record = result.setdefault(name, {
                    "name": name, "symbol_type": symbol["type"],
                    "relocation_types": [], "relocation_sites": []})
                relocation = RELOC_TYPES.get(kind, f"R_ARM_{kind}")
                if relocation not in record["relocation_types"]:
                    record["relocation_types"].append(relocation)
                if len(record["relocation_sites"]) < 32:
                    record["relocation_sites"].append(f"0x{location:08x}")
        return sorted(result.values(), key=lambda r: r["name"])

    def exports(self):
        results = []
        for index in range(self.sym_count):
            sym = self.symbol(index)
            if sym["defined"] and sym["name"]:
                results.append({"name": sym["name"],
                                "address": f"0x{sym['address']:08x}",
                                "size": sym["size"], "type": sym["type"]})
        return results

    def candidates(self, cap=1500):
        """Best-effort direct branches from file-backed executable bytes only.

        Both ISA decodings are inspected when mapping symbols are unavailable.
        Candidates are not disassembled functions or proof of reachability.
        """
        found = []
        count = Counter()
        executable = [(v, off, size) for v, off, size, _, flags in self.segments
                      if flags & 1]
        def is_exec(x):
            return any(v <= x < v + n for v, _, n in executable)
        for v, off, size in executable:
            segment = self.data[off:off + size]
            for i in range(0, len(segment) - 3, 4):
                word = struct.unpack_from("<I", segment, i)[0]
                if word >> 28 != 15 and (word & 0x0e000000) == 0x0a000000:
                    is_call = bool(word & (1 << 24))
                    target = (v + i + 8 + signed((word & 0xffffff) << 2, 26)) & 0xffffffff
                    if is_exec(target):
                        count["arm_bl" if is_call else "arm_b"] += 1
                        if len(found) < cap:
                            found.append({"source": f"0x{v+i:08x}",
                                          "target": f"0x{target:08x}",
                                          "instruction": "ARM BL" if is_call else "ARM B",
                                          "status": "candidate_not_executed"})
            for i in range(0, len(segment) - 3, 2):
                hi, lo = struct.unpack_from("<HH", segment, i)
                if (hi & 0xf800) != 0xf000 or (lo & 0xd000) != 0xd000:
                    continue
                s = (hi >> 10) & 1
                j1, j2 = (lo >> 13) & 1, (lo >> 11) & 1
                i1, i2 = 1 ^ (j1 ^ s), 1 ^ (j2 ^ s)
                disp = (s << 24) | (i1 << 23) | (i2 << 22) | ((hi & 0x3ff) << 12) | ((lo & 0x7ff) << 1)
                target = (v + i + 4 + signed(disp, 25)) & 0xffffffff
                if is_exec(target):
                    count["thumb_bl_candidate"] += 1
                    if len(found) < cap:
                        found.append({"source": f"0x{v+i:08x}",
                                      "target": f"0x{target:08x}",
                                      "instruction": "Thumb-2 BL candidate",
                                      "status": "candidate_not_executed"})
        return dict(count), found

    def report(self, cap=1500, reference_tag=None):
        imports = self.imports()
        exports = self.exports()
        count, branches = self.candidates(cap)
        init = self.tags.get(DT_INIT)
        initarray = self.tags.get(DT_INIT_ARRAYSZ, 0)
        if initarray % 4:
            raise ElfError("Unaligned initializer array")
        if initarray:
            self.at(self.tags.get(DT_INIT_ARRAY, -1), initarray)
        initializer_count = (1 if init else 0) + initarray // 4
        by_type = Counter(x["symbol_type"] for x in imports)
        return {
            "schema": "pv7.arm32.static.compat.v1",
            "analysis_mode": "offline_static_only",
            "limitations": [
                "Imports are declared dependencies, not evidence of invocation",
                "Branch candidates can be false positives when code/data or ARM/Thumb mode is ambiguous",
                "Indirect function calls, dynamic symbol loading and runtime paths are not fully discoverable",
                "No historical Android release or specific implementation can be inferred solely from this ELF",
                "No guest code executed; runtime call coverage is unavailable"],
            "dependencies": [self.string(o) for o in self.tag_lists.get(DT_NEEDED, [])],
            "external_imports": imports,
            "import_counts": {"distinct": len(imports), "by_type": dict(by_type)},
            "exports": exports,
            "jni_entrypoints": [x for x in exports if x["name"] == "JNI_OnLoad" or x["name"].startswith("Java_")],
            "initializers": {"entries": initializer_count, "dt_init": bool(init)},
            "branch_candidate_counts": count,
            "branch_candidates_capped_at": cap,
            "branch_candidates": branches,
            "runtime_events": [],
            "runtime_tracing": "not_connected",
            "historical_android_reference": {
                "requested_aosp_tag": reference_tag,
                "status": "unverified_reference_only" if reference_tag else "version_not_identified",
                "note": "A tag is an analyst-selected reference; it is not proof of the game's build environment."}
        }


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("library", type=Path)
    parser.add_argument("--output", type=Path, help="JSON report destination (default stdout)")
    parser.add_argument("--max-branches", type=int, default=1500)
    parser.add_argument("--aosp-tag", help="Optional human-selected historical AOSP tag, never inferred")
    args = parser.parse_args(argv)
    if not 0 <= args.max_branches <= 50000:
        parser.error("--max-branches must be between 0 and 50000")
    if args.aosp_tag and not re.fullmatch(r"android-[A-Za-z0-9_.-]{1,64}", args.aosp_tag):
        parser.error("Invalid AOSP tag")
    try:
        report = Arm32Elf(args.library.read_bytes()).report(args.max_branches, args.aosp_tag)
    except (OSError, ElfError, struct.error) as exc:
        parser.exit(1, f"ELF analysis failed: {exc}\n")
    encoded = json.dumps(report, indent=2, sort_keys=True) + "\n"
    if args.output:
        args.output.write_text(encoded, encoding="utf-8")
    else:
        print(encoded, end="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
