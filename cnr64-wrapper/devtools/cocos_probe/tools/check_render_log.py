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
             require_touch: bool = False,
             require_move: bool = False,
             require_audio_stop: bool = False,
             require_cutscene_trace: bool = False) -> tuple[bool, str]:
    if "COCOS_NATIVE_INIT_RETURNED" not in log:
        return False, "guest nativeInit did not return"
    if "COCOS_NATIVE_RENDER_STOP" in log:
        return False, next(
            line for line in log.splitlines() if "COCOS_NATIVE_RENDER_STOP" in line
        )
    if "COCOS_FRAME_READBACK_FAILED" in log or "COCOS_FRAME_SAVE_FAILED" in log:
        return False, "framebuffer capture or file save failed"
    frames = [int(f) for f in RETURNED.findall(log)]
    done = SEQUENCE.findall(log)
    if len(done) != 1:
        return False, "bounded render sequence did not complete"
    count = int(done[0])
    # Log every frame through 240, then sample every 300 frames to keep
    # continuous mode from writing millions of lines over a long session.
    expected_returns = list(range(1, min(240, count) + 1))
    expected_returns += list(range(300, count + 1, 300))
    if not frames or frames != expected_returns:
        return False, "render returns are missing, duplicated, or out of order"
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
        expected = [1] + list(range(10, min(count, 240) + 1, 10))
        expected += list(range(300, count + 1, 300))
        if actual != expected:
            return False, "window frame presentation checkpoints incomplete"
    if require_touch:
        begins = log.count("COCOS_GUEST_TOUCH_RETURNED phase=0")
        ends = log.count("COCOS_GUEST_TOUCH_RETURNED phase=1")
        if not begins or not ends or ends > begins or "COCOS_GUEST_TOUCH_FAILED" in log:
            return False, "guest touch begin/end callbacks were not verified"
    if require_move and "COCOS_GUEST_TOUCH_RETURNED phase=2" not in log:
        return False, "original ARM32 touch-move JNI callback never returned"
    if require_audio_stop and (
        "JNI_METHOD_IDENTIFIED stopEffect" not in log or
        "COCOS_EFFECT_SOUNDPOOL_STOPPED stream=" not in log or
        "JNI_StopEffect_" in log
    ):
        return False, "original Cocos2dxHelper.stopEffect/SoundPool callback failed"
    if require_cutscene_trace:
        for name in ("showNextScene", "onAnimationCompleted",
                     "setAndPlayCurrent", "onEat"):
            if f"COCOS_CUTSCENE_TRACE_INSTALLED function={name}" not in log:
                return False, f"original cutscene trace not installed: {name}"
        for name in ("showNextScene", "onAnimationCompleted",
                     "setAndPlayCurrent", "onEat"):
            if f"COCOS_CUTSCENE_ENTER function={name}" not in log:
                return False, f"original cutscene entry not observed: {name}"
        indices = {int(index) for index in re.findall(
            r"COCOS_CUTSCENE_ENTER function=setAndPlayCurrent "
            r"this=0x[0-9a-fA-F]+ scene_index=(\d+)", log
        )}
        if not {1, 2}.issubset(indices):
            return False, f"original cutscene never advanced to scene 2: {indices}"
        if "COCOS_CUTSCENE_TRACE_OPCODE_MISMATCH" in log or (
            "COCOS_CUTSCENE_TRACE_BAD_THIS" in log
        ):
            return False, "cutscene probe encountered unexpected guest state"
    return True, (
        f"rendered={count} captured={len(readbacks)} "
        f"distinct_frame_hashes={len(set(hashes))}"
    )


def validate_clean_shutdown(log: str) -> tuple[bool, str]:
    if "COCOS_RENDER_MODE continuous max_frames=0" not in log:
        return False, "diagnostic was not launched in continuous mode"
    match = re.search(r"COCOS_RUNTIME_STOPPED_BY_SURFACE at_frame=(\d+)", log)
    if not match:
        return False, "guest runtime did not acknowledge Android surface cancellation"
    if "COCOS_ONSCREEN_PRESENT_FAILED" in log or "COCOS_NATIVE_RENDER_STOP" in log:
        return False, "rendering or EGL failed during activity shutdown"
    if "ANDROID_JAVA_PROBE_EXIT=0" not in log:
        return False, "guest runtime did not terminate cleanly"
    if not PRESENTED.search(log) or not READBACK.search(log):
        return False, "no verified visible gameplay frames prior to shutdown"
    return True, f"continuous runtime cleanly stopped at frame={match.group(1)}"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("log", type=Path)
    parser.add_argument("--require-onscreen", action="store_true")
    parser.add_argument("--require-touch", action="store_true")
    parser.add_argument("--require-move", action="store_true")
    parser.add_argument("--require-audio-stop", action="store_true")
    parser.add_argument("--require-cutscene-trace", action="store_true")
    parser.add_argument("--require-clean-stop", action="store_true")
    args = parser.parse_args()
    log = args.log.read_text(encoding="utf-8", errors="replace")
    if args.require_clean_stop:
        ok, message = validate_clean_shutdown(log)
    else:
        ok, message = validate(
            log,
            require_onscreen=args.require_onscreen,
            require_touch=args.require_touch,
            require_move=args.require_move,
            require_audio_stop=args.require_audio_stop,
            require_cutscene_trace=args.require_cutscene_trace)
    print(("PASS: " if ok else "FAIL: ") + message)
    return 0 if ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
