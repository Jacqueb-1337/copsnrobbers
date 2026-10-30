#!/usr/bin/env python3
"""Offline PV7 ARM32 trace correlation; does not run game code or upload files.

Combines selected Frida export-call traces with their .summary.json files.
Optionally matches caller offsets to proven, sized ELF ARM function ranges.
A sampled caller edge is not an exhaustive call graph.
"""
import argparse
from collections import Counter
import json
from pathlib import Path
import struct


def elf_functions(path):
    b = Path(path).read_bytes()
    if len(b) < 52 or b[:4] != b"\x7fELF" or b[4:6] != b"\x01\x01":
        raise ValueError("ARM32 little-endian ELF required")
    h = struct.unpack_from("<16sHHIIIIIHHHHHH", b)
    if h[2] != 40 or h[12] == 0 or h[11] < 40 or h[6] + h[11] * h[12] > len(b):
        raise ValueError("Invalid ARM32 ELF section table")
    sections = [struct.unpack_from("<IIIIIIIIII", b, h[6] + i * h[11])
                for i in range(h[12])]
    symbols = {}
    for sh in sections:
        if sh[1] not in (2, 11):
            continue
        off, size, link, entsize = sh[4], sh[5], sh[6], sh[9]
        if link >= len(sections) or entsize < 16 or off + size > len(b):
            raise ValueError("Invalid ELF symbol table")
        st = sections[link]
        if st[4] + st[5] > len(b):
            raise ValueError("Invalid ELF string table")
        strings = b[st[4]:st[4] + st[5]]
        for i in range(off, off + size - 15, entsize):
            nameoff, address, nbytes, info, other, section = struct.unpack_from("<IIIBBH", b, i)
            if info & 15 != 2 or address == 0 or section == 0 or nameoff >= len(strings):
                continue
            name = strings[nameoff:].split(b"\x00", 1)[0].decode("utf-8", "replace")
            if name:
                key = (address & ~1, name)
                symbols[key] = max(nbytes, symbols.get(key, 0))
    return [{"start": key[0], "name": key[1], "size": n}
            for key, n in sorted(symbols.items())]


def resolve(offset, functions):
    if not offset:
        return None
    try:
        pc = int(offset, 0) & ~1
    except (TypeError, ValueError):
        return None
    candidates = [f for f in functions
                  if f["size"] > 0 and f["start"] <= pc < f["start"] + f["size"]]
    if not candidates:
        return None
    f = min(candidates, key=lambda x: (x["size"], x["name"]))
    return {"symbol": f["name"], "offset_in_function": pc - f["start"],
            "confidence": "inside_nonzero_ELF_symbol_range"}


def analyze_capture(path, functions=()):
    p = Path(path)
    meta_path = p.with_suffix(".summary.json")
    meta = json.loads(meta_path.read_text(encoding="utf-8")) if meta_path.exists() else {}
    sampled = Counter()
    edges = Counter()
    errors = list(meta.get("errors", []))
    events = 0
    returns = 0
    phase = []
    profile = meta.get("profile", "unknown")
    with p.open(encoding="utf-8-sig") as f:
        for line in f:
            if not line.strip():
                continue
            event = json.loads(line)
            events += 1
            typ = event.get("kind")
            if typ == "call":
                sampled[event.get("name", "?")] += 1
                edges[(event.get("caller_module", "?"),
                       event.get("caller_offset", "?"), event.get("name", "?"))] += 1
            elif typ == "return":
                returns += 1
            elif typ == "phase":
                phase.append(event.get("name"))
            elif typ in ("hook_error", "fatal", "host_error") and not meta.get("errors"):
                errors.append(event.get("error", event.get("detail", "unknown")))
            if profile == "unknown" and event.get("profile"):
                profile = event["profile"]
    actual = meta.get("observed_by_symbol", {})
    if not actual:
        final = meta.get("final", {})
        names = final.get("names", {})
        for key, count in final.get("counts", {}).items():
            name = names.get(key, key)
            actual[name] = actual.get(name, 0) + count
    sites = []
    for (module, caller, target), count in edges.most_common(100):
        item = {"caller_module": module, "caller_offset": caller,
                "callee": target, "sampled_count": count}
        if module == "libgame.so":
            mapping = resolve(caller, functions)
            if mapping:
                item["caller_elf_symbol"] = mapping
        sites.append(item)
    return {
        "file": p.name, "profile": profile,
        "hooks": meta.get("hook_setup", {}).get("hooked"),
        "hook_failures": meta.get("hook_setup", {}).get("failures"),
        "total_intercepted_selected_calls": sum(actual.values()),
        "intercepted_by_symbol": actual,
        "sampled_by_symbol": dict(sampled.most_common()),
        "sampled_calls": sum(sampled.values()), "sampled_returns": returns,
        "events": events, "phases": phase, "caller_sites": sites,
        "errors": errors, "has_final_counts": "final" in meta
    }


def correlate(traces, elf=None):
    functions = elf_functions(elf) if elf else []
    captures = [analyze_capture(t, functions) for t in traces]
    by_profile = {}
    for c in captures:
        counts = by_profile.setdefault(c["profile"], Counter())
        counts.update(c["intercepted_by_symbol"])
    return {
        "schema": "pv7.arm32.trace_correlation.v1",
        "ELF": {"supplied": bool(elf), "function_symbols": len(functions)},
        "captures": captures,
        "by_profile": {k: dict(v.most_common()) for k, v in sorted(by_profile.items())},
        "limitations": [
            "Only selected and successfully hooked functions are counted",
            "Per-call records and caller edges are sampled, not exhaustive",
            "No per-phase total call count is inferred from sampled records",
            "Unknown or zero-sized ELF symbols cannot prove caller identity",
            "Zero calls in a profile do not mean a function is unsupported",
            "Different sessions may observe repeated gameplay",
            "PV7 wrapped execution and gameplay compatibility not proven"]
    }


def to_markdown(result):
    out = ["# PV7 ARM32 observed call correlation", "",
           "Offline results from selected native hooks, not a full ARM call graph.", "",
           "| Session | Profile | Hooks | Counted calls | Sampled entry / return | Errors |",
           "|---|---|---:|---:|---:|---:|"]
    for c in result["captures"]:
        out.append("| {} | {} | {} | {} | {} / {} | {} |".format(
            c["file"], c["profile"], c["hooks"] or 0,
            c["total_intercepted_selected_calls"], c["sampled_calls"],
            c["sampled_returns"], len(c["errors"])))
    for profile, counts in result["by_profile"].items():
        out += ["", "## {} observed functions".format(profile)]
        if not counts:
            out.append("No selected functions observed in this capture window.")
        else:
            for name, count in list(counts.items())[:30]:
                out.append("- {}: {} intercepted calls".format(name, count))
    out += ["", "## Sampled caller edges"]
    for c in result["captures"]:
        if c["caller_sites"]:
            out.append("### {}".format(c["file"]))
        for edge in c["caller_sites"][:12]:
            origin = "{}:{}".format(edge["caller_module"], edge["caller_offset"])
            f = edge.get("caller_elf_symbol")
            if f:
                origin += " ({}+0x{:x})".format(f["symbol"], f["offset_in_function"])
            out.append("- {} -> {} ({} samples)".format(
                origin, edge["callee"], edge["sampled_count"]))
    out += ["", "## Limitations"]
    out += ["- " + x for x in result["limitations"]]
    out.append("")
    return "\n".join(out)


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--trace", type=Path, action="append", required=True)
    ap.add_argument("--elf", type=Path)
    ap.add_argument("--output", type=Path, required=True)
    args = ap.parse_args()
    if args.output.exists() or args.output.with_suffix(".md").exists():
        ap.error("Output already exists; refusing overwrite")
    result = correlate(args.trace, args.elf)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    args.output.with_suffix(".md").write_text(to_markdown(result), encoding="utf-8")
    print(json.dumps({"captures": len(result["captures"]), "output": str(args.output)}))


if __name__ == "__main__":
    main()
