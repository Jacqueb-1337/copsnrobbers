#!/usr/bin/env python3
"""Capture an observed legacy-Android reference run using ADB.

Produces local-only evidence. Does not claim to trace every native call:
logcat captures emitted messages, /proc/maps captures mappings, and an
optional screenshot captures display state. Use a separate instrumentation
backend for complete guest function-entry/return tracing.
"""
import argparse
import datetime as dt
import hashlib
import json
import re
import subprocess
import time
from pathlib import Path

SERIAL = re.compile(r"^[A-Za-z0-9_.:-]+$")
PACKAGE = re.compile(r"^[a-zA-Z_][a-zA-Z0-9_]*(?:\.[a-zA-Z_][a-zA-Z0-9_]*)+$")


def adb(serial, *arguments, timeout=30, binary=False, executable="adb"):
    command = [executable, "-s", serial, *arguments]
    result = subprocess.run(command, capture_output=True, timeout=timeout, check=False)
    if result.returncode:
        raise RuntimeError(
            "ADB command failed: " + " ".join(arguments) +
            "\n" + result.stderr.decode("utf-8", "replace")[:1800])
    if binary:
        return result.stdout
    return result.stdout.decode("utf-8", "replace")


def summarize(logcat, maps, package, pid):
    mappings = sorted(set(
        line.strip().rsplit(None, 1)[-1] for line in maps.splitlines()
        if ".so" in line and "/" in line and len(line.split()) >= 6
    ))
    app_lines = []
    native_lines = []
    pid_token = re.compile(r"^\S+\s+\S+\s+" + re.escape(pid) + r"\s+")
    for line in logcat.splitlines():
        if package in line or (pid and pid_token.search(line)):
            app_lines.append(line)
        if ("native init" in line.lower() or "jni_onload" in line.lower() or
                "cocos" in line.lower() or "libgame.so" in line.lower()):
            native_lines.append(line)
    return {
        "schema": "pv7.reference.capture.v1",
        "evidence_kind": "observed_android_logcat_and_process_snapshot",
        "package": package,
        "pid": pid or None,
        "mapped_native_modules": mappings,
        "native_observation_count": len(native_lines),
        "native_observations_sample": native_lines[-80:],
        "app_log_line_count": len(app_lines),
        "limitations": [
            "Observed log messages are not a complete native function-call trace",
            "Mapped .so files are not proof that every exported function ran",
            "Native bridge translation can differ from physical ARM32 execution",
            "The capture does not prove that the game reached a playable frame",
            "Raw logs may contain user or device information; never auto-upload"
        ]
    }


def capture(serial, package, output, duration, launch, executable):
    if not SERIAL.fullmatch(serial) or not PACKAGE.fullmatch(package):
        raise ValueError("Invalid ADB serial or Android package")
    if launch and ("/" not in launch or not launch.startswith(package + "/")):
        raise ValueError("Launch component must be package/class")
    if not 0 <= duration <= 60:
        raise ValueError("Capture duration must be between 0 and 60 seconds")
    output = Path(output)
    output.mkdir(parents=True, exist_ok=False)
    data = {
        "serial": serial,
        "captured_at_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
        "launch_component": launch,
    }
    for prop in ("ro.build.version.release", "ro.build.version.sdk",
                 "ro.product.cpu.abilist", "ro.dalvik.vm.native.bridge"):
        data[prop] = adb(serial, "shell", "getprop", prop, executable=executable).strip()
    if launch:
        data["launch_result"] = adb(serial, "shell", "am", "start", "-n", launch,
                                    executable=executable).strip()
    if duration:
        time.sleep(duration)
    pid = adb(serial, "shell", "pidof", package, executable=executable).strip().split()
    pid = pid[0] if pid else ""
    data["pid"] = pid or None
    maps = ""
    if pid and pid.isdecimal():
        maps = adb(serial, "shell", "cat", "/proc/" + pid + "/maps",
                   executable=executable)
        (output / "process-maps.txt").write_text(maps, encoding="utf-8")
        status = adb(serial, "shell", "cat", "/proc/" + pid + "/status",
                     executable=executable)
        (output / "process-status.txt").write_text(status, encoding="utf-8")
    logcat = adb(serial, "logcat", "-d", "-v", "threadtime", "-t", "5000",
                 executable=executable, timeout=45)
    (output / "logcat.txt").write_text(logcat, encoding="utf-8")
    screenshot = adb(serial, "exec-out", "screencap", "-p", binary=True,
                     executable=executable)
    (output / "screen.png").write_bytes(screenshot)
    data.update(summarize(logcat, maps, package, pid))
    data["screen_sha256"] = hashlib.sha256(screenshot).hexdigest()
    data["screen_bytes"] = len(screenshot)
    (output / "report.json").write_text(json.dumps(data, indent=2) + "\n",
                                         encoding="utf-8")
    return data


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--serial", required=True)
    ap.add_argument("--package", required=True)
    ap.add_argument("--output", required=True)
    ap.add_argument("--duration", type=int, default=4)
    ap.add_argument("--launch-component",
                    help="Optional package/class to launch before capturing")
    ap.add_argument("--adb", default="adb", help="ADB executable")
    args = ap.parse_args()
    result = capture(args.serial, args.package, args.output,
                     args.duration, args.launch_component, args.adb)
    print(json.dumps({"report": str(Path(args.output) / "report.json"),
                      "pid": result["pid"],
                      "mapped_native_modules": len(result["mapped_native_modules"]),
                      "native_observation_count": result["native_observation_count"]}))


if __name__ == "__main__":
    main()
