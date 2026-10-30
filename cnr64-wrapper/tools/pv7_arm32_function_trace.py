#!/usr/bin/env python3
"""Capture selected real ARM32 function calls from Best Park's libgame.so.

Requires the intentionally isolated Android 4.4 ARM32 reference emulator and
the legacy Frida 12.4.8 client/server pair. Start Frida server in the guest,
forward its TCP port to localhost, and launch the original game first.

Unlike Frida Stalker, this only hooks a curated list of *exported* function
addresses using Interceptor. It does not trace all instructions or all calls.
All arguments/return values are untyped ARM register/pointer snapshots.

No APK modification, game-data upload, or background monitoring occurs.
"""
import argparse
import collections
import datetime
import json
from pathlib import Path
import re
import subprocess
import time

AGENT = Path(__file__).with_name("pv7_arm32_function_agent.js")
PACKAGE = "com.turner.bestparkintheuniverse"
REMOTE_RE = re.compile(r"^[A-Za-z0-9_.-]+:[0-9]{2,5}$")
PROFILES = ("render", "scene", "assets", "input", "gameplay", "character")


def aggregate(records):
    """Compute deterministic summaries from JSONL event dictionaries."""
    by_symbol = collections.Counter()
    by_caller = collections.Counter()
    return_count = 0
    for record in records:
        if record.get("kind") == "call":
            by_symbol[record.get("name", "unknown")] += 1
            key = "{}:{}".format(record.get("caller_module", "unknown"),
                                  record.get("caller_offset", "?"))
            by_caller[key] += 1
        elif record.get("kind") == "return":
            return_count += 1
    return {
        "sampled_calls": sum(by_symbol.values()),
        "sampled_returns": return_count,
        "sampled_by_symbol": dict(by_symbol.most_common()),
        "sampled_by_caller": dict(by_caller.most_common(100))
    }


def capture(remote, package, seconds, output, profile="render",
            max_hooks=90, sample_limit=1000, adb_serial="emulator-5580",
            tap=None, tap_after=1):
    if not 1 <= seconds <= 90:
        raise ValueError("--seconds must be between 1 and 90")
    if profile not in PROFILES:
        raise ValueError("Unknown profile")
    if not 1 <= max_hooks <= 120 or not 1 <= sample_limit <= 5000:
        raise ValueError("Invalid hook/sample limit")
    if tap is not None and (not 0 <= tap_after < seconds or
                            any(not 0 <= n <= 10000 for n in tap)):
        raise ValueError("Tap must be within screen coordinates and capture duration")
    if not REMOTE_RE.fullmatch(remote):
        raise ValueError("Expected local Frida endpoint like 127.0.0.1:27044")
    if not AGENT.is_file():
        raise FileNotFoundError(AGENT)
    if output.exists() or output.with_suffix(".summary.json").exists():
        raise FileExistsError("Refusing to overwrite existing trace evidence")
    import frida
    output.parent.mkdir(parents=True, exist_ok=True)
    device = frida.get_device_manager().add_remote_device(remote)
    candidates = [p for p in device.enumerate_processes() if p.name == package]
    if len(candidates) != 1:
        raise RuntimeError("Expected one already-running game process, got {}"
                           .format(len(candidates)))
    pid = candidates[0].pid
    session = device.attach(pid)
    config = {"profile": profile, "maxHooks": max_hooks,
              "sampleLimit": sample_limit}
    script = session.create_script("var PV7_TRACE_CONFIG = " +
                                   json.dumps(config) + ";\n" +
                                   AGENT.read_text(encoding="utf-8-sig"))
    meta = {
        "schema": "pv7.arm32.function_trace.v1",
        "process": package, "pid": pid,
        "captured_at_utc": datetime.datetime.utcnow().isoformat() + "Z",
        "seconds_requested": seconds,
        "profile": profile,
        "max_hooks": max_hooks,
        "sample_limit": sample_limit,
        "limitations": [
            "Only selected ELF-exported ARM32 functions are instrumented",
            "Sampled per-call records are bounded; per-symbol counts continue",
            "Raw ARM argument registers/pointers have no verified C types",
            "A caller address is an observed return site, not full call stack",
            "Untested guest functions cannot be classified compatible",
            "Traces stay local and may include sensitive process values"
        ],
        "event_count": 0,
        "errors": []
    }
    samples = []
    try:
        with output.open("x", encoding="utf-8") as sink:
            def on_message(message, data):
                if message.get("type") != "send":
                    meta["errors"].append(str(message.get("description", message))[:800])
                    return
                event = message.get("payload", {})
                kind = event.get("kind")
                if kind == "ready":
                    meta["hook_setup"] = event
                if kind in ("call", "return", "ready", "hook_error", "fatal", "phase"):
                    sink.write(json.dumps(event, separators=(",", ":")) + "\n")
                    sink.flush()
                    meta["event_count"] += 1
                if kind in ("call", "return"):
                    samples.append(event)
                if kind in ("fatal", "hook_error"):
                    meta["errors"].append(str(event)[:800])
            script.on("message", on_message)
            script.load()
            try:
                if tap is not None:
                    time.sleep(tap_after)
                    script.exports.mark("before_adb_tap")
                    result = subprocess.run(["adb", "-s", adb_serial, "shell",
                                             "input", "tap", str(tap[0]), str(tap[1])],
                                            capture_output=True, text=True,
                                            timeout=20, check=False)
                    meta["tap"] = {"x": tap[0], "y": tap[1],
                                   "adb_serial": adb_serial,
                                   "exit_code": result.returncode}
                    script.exports.mark("after_adb_tap")
                    time.sleep(seconds - tap_after)
                else:
                    time.sleep(seconds)
                try:
                    meta["final"] = script.exports.snapshot()
                except Exception as exc:
                    meta["errors"].append("Snapshot failure: " + str(exc))
            finally:
                try:
                    script.unload()
                except Exception as exc:
                    meta["errors"].append(str(exc))
    finally:
        session.detach()
        meta.update(aggregate(samples))
        final = meta.get("final", {})
        names = final.get("names", {})
        counted = collections.Counter()
        for offset, count in final.get("counts", {}).items():
            counted[names.get(offset, offset)] += count
        meta["observed_by_symbol"] = dict(counted.most_common())
        meta["observed_function_count"] = len(final.get("counts", {}))
        meta["completed_at_utc"] = datetime.datetime.utcnow().isoformat() + "Z"
        output.with_suffix(".summary.json").write_text(
            json.dumps(meta, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    return meta


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--remote", default="127.0.0.1:27044")
    parser.add_argument("--package", default=PACKAGE)
    parser.add_argument("--seconds", type=int, default=12)
    parser.add_argument("--profile", choices=PROFILES, default="render")
    parser.add_argument("--max-hooks", type=int, default=90)
    parser.add_argument("--sample-limit", type=int, default=1000)
    parser.add_argument("--adb-serial", default="emulator-5580")
    parser.add_argument("--tap", type=int, nargs=2, metavar=("X", "Y"))
    parser.add_argument("--tap-after", type=int, default=1)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    report = capture(args.remote, args.package, args.seconds, args.output,
                     profile=args.profile, max_hooks=args.max_hooks,
                     sample_limit=args.sample_limit,
                     adb_serial=args.adb_serial, tap=args.tap,
                     tap_after=args.tap_after)
    print(json.dumps({
        "trace": str(args.output),
        "summary": str(args.output.with_suffix(".summary.json")),
        "hooks": report.get("hook_setup", {}).get("hooked"),
        "profile": report["profile"],
        "observed_function_count": report["observed_function_count"],
        "sampled_calls": report["sampled_calls"],
        "sampled_returns": report["sampled_returns"],
        "counted_functions": len(report.get("final", {}).get("counts", {})),
        "errors": len(report["errors"])
    }, sort_keys=True))


if __name__ == "__main__":
    main()
