#!/usr/bin/env python3
"""Capture observed Android native API calls in a running reference process.

Requires an opt-in Frida server running on the selected emulator and a matching
Python frida package. Captures host-library boundary calls, NOT every ARM guest
function or instruction. Does not modify the APK or upload trace files.
"""
import argparse
import datetime as dt
import json
from pathlib import Path
import re
import subprocess
import time

DEFAULT_AGENT = Path(__file__).with_name("pv7_reference_frida_agent.js")
SAFE_SERIAL = re.compile(r"^[A-Za-z0-9_.:-]+$")
SAFE_PACKAGE = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)+$")


def get_pid(serial, package):
    result = subprocess.run(["adb", "-s", serial, "shell", "pidof", package],
                            capture_output=True, text=True, timeout=15)
    if result.returncode:
        raise RuntimeError(f"Package {package} not running on {serial}")
    pids = [x for x in result.stdout.split() if x.isdecimal()]
    if not pids:
        raise RuntimeError(f"No valid PID for {package}")
    return int(pids[0])


def capture(serial, package, seconds, output, agent=DEFAULT_AGENT):
    if not SAFE_SERIAL.fullmatch(serial) or not SAFE_PACKAGE.fullmatch(package):
        raise ValueError("Invalid serial or package name")
    if not 1 <= seconds <= 120:
        raise ValueError("Duration must be 1 to 120 seconds")
    if not agent.is_file():
        raise FileNotFoundError(agent)
    import frida
    if output.exists():
        raise FileExistsError("Output exists; refusing to overwrite a trace")
    output.parent.mkdir(parents=True, exist_ok=True)
    pid = get_pid(serial, package)
    device = frida.get_usb_device(timeout=15)
    session = device.attach(pid)
    meta = {
        "schema": "pv7.reference.native_api.v1",
        "scope": "native host API callbacks in original game's Android process",
        "limitations": [
            "Does not capture all internal ARMv7 guest function calls",
            "Host calls may originate from Java/Android libraries, not libgame.so",
            "Raw native values are untyped; pointer values are not decoded",
            "Capped trace discards per-event records after initial sample",
            "Runtime overhead can affect timing and game behavior",
            "Private trace data stays local and must not be auto-uploaded"],
        "package": package, "pid": pid, "serial": serial,
        "started_at_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
        "events_written": 0,
    }
    script = session.create_script(agent.read_text(encoding="utf-8"))
    try:
        with output.open("w", encoding="utf-8") as sink:
            def on_message(message, data):
                if message.get("type") == "error":
                    meta.setdefault("errors", []).append(message.get("description", "unknown"))
                    return
                payload = message.get("payload", {})
                if payload.get("kind") == "ready":
                    meta["ready"] = payload
                elif payload.get("kind") == "summary":
                    meta["latest_summary"] = payload
                elif payload.get("kind") == "batch":
                    for event in payload.get("events", []):
                        sink.write(json.dumps(event, separators=(",", ":")) + "\n")
                        meta["events_written"] += 1
                    sink.flush()
            script.on("message", on_message)
            script.load()
            try:
                time.sleep(seconds)
            finally:
                try:
                    meta["final"] = script.exports_sync.flush()
                except Exception as exc:
                    meta.setdefault("errors", []).append(str(exc))
                script.unload()
    finally:
        session.detach()
        meta["ended_at_utc"] = dt.datetime.now(dt.timezone.utc).isoformat()
        output.with_suffix(".summary.json").write_text(json.dumps(meta, indent=2) + "\n",
                                                     encoding="utf-8")
    return meta


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--serial", default="emulator-5560")
    parser.add_argument("--package", required=True)
    parser.add_argument("--seconds", type=int, default=8)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    result = capture(args.serial, args.package, args.seconds, args.output)
    print(json.dumps({"output": str(args.output),
                      "events_written": result["events_written"],
                      "final": result.get("final")}, sort_keys=True))


if __name__ == "__main__":
    main()
