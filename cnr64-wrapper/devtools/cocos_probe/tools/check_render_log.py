#!/usr/bin/env python3
"""Validate a developer-only Best Park ARM32 -> GLES2 render diagnostic log.

No original game assets are read or stored. The checker validates actual
guest render returns and framebuffer readbacks instead of just GL draw logs.
"""
from __future__ import annotations

import argparse
import re
from pathlib import Path

READBACK = re.compile(
    r"COCOS_FRAME_READBACK_OK frame=(\d+) size=(\d+)x(\d+) "
    r"nonblack=(\d+) color_variation=(\d+) nonopaque=(\d+) "
    r"fnv64=0x([0-9a-fA-F]+)"
)
RETURNED = re.compile(r"COCOS_NATIVE_RENDER_RETURNED frame=(\d+)")
SEQUENCE = re.compile(r"COCOS_RENDER_SEQUENCE_COMPLETED frames=(\d+)")
PRESENTED = re.compile(r"COCOS_ONSCREEN_FRAME_PRESENTED frame=(\d+)")


def validate(log: str, *, require_onscreen: bool = False,
             require_touch: bool = False) -> tuple[bool, str]:
    if "COCOS_NATIVE_INIT_RETURNED" not in log:
        return False, "guest nativeInit did not return"
    if "COCOS_NATIVE_RENDER_STOP" in log:
        return False, next(
            line for line in log.splitlines() if "COCOS_NATIVE_RENDER_STOP" in line
        )
    if "COCOS_FRAME_READBACK_FAILED" in log or "COCOS_FRAME_SAVE_FAILED" in log:
        return False, "framebuffer capture or file save failed"
    frames = [int(f) for f in RETURNED.findall(log)]
    if not frames or frames != list(range(1, len(frames) + 1)):
        return False, "render returns are missing, duplicated, or out of order"
    done = SEQUENCE.findall(log)
    if len(done) != 1 or int(done[0]) != len(frames):
        return False, f"render sequence incomplete: {len(frames)} returns"
    readbacks = READBACK.findall(log)
    if not readbacks:
        return False, "no GLES framebuffer pixels read"
    hashes = []
    for frame, width, height, nonblack, variation, nonopaque, digest in readbacks:
        if (int(width), int(height)) != (480, 320):
            return False, f"unexpected framebuffer dimensions at frame {frame}"
        if int(nonblack) == 0 or int(variation) == 0:
            return False, f"blank or uniform framebuffer at frame {frame}"
        hashes.append(digest.lower())
    if "ANDROID_JAVA_PROBE_EXIT=0" not in log:
        return False, "Android JNI diagnostic did not return success"
    if require_onscreen:
        if ("COCOS_ONSCREEN_SURFACE_ACQUIRED dimensions=480x320" not in log or
                "HOST_GLES2_CONTEXT_READY mode=window" not in log):
            return False, "real Android EGL window surface not established"
        if "COCOS_ONSCREEN_PRESENT_FAILED" in log:
            return False, "Android window framebuffer present failed"
        actual = [int(frame) for frame in PRESENTED.findall(log)]
        expected = [1] + list(range(10, len(frames) + 1, 10))
        if actual != expected:
            return False, "window frame presentation checkpoints incomplete"
    if require_touch:
        begins = log.count("COCOS_GUEST_TOUCH_RETURNED phase=0")
        ends = log.count("COCOS_GUEST_TOUCH_RETURNED phase=1")
        if not begins or not ends or ends > begins or "COCOS_GUEST_TOUCH_FAILED" in log:
            return False, "guest touch begin/end callbacks were not verified"
    return True, (
        f"rendered={len(frames)} captured={len(readbacks)} "
        f"distinct_frame_hashes={len(set(hashes))}"
    )


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("log", type=Path)
    parser.add_argument("--require-onscreen", action="store_true")
    parser.add_argument("--require-touch", action="store_true")
    args = parser.parse_args()
    ok, message = validate(
        args.log.read_text(encoding="utf-8", errors="replace"),
        require_onscreen=args.require_onscreen, require_touch=args.require_touch)
    print(("PASS: " if ok else "FAIL: ") + message)
    return 0 if ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
