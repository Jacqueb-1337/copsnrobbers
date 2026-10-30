#!/usr/bin/env python3
"""Offline unit tests for PV7 reference evidence capture (no ADB required)."""
import importlib.util
import pathlib
import tempfile
import unittest

MODULE_PATH = pathlib.Path(__file__).resolve().parents[1] / "tools" / "pv7_reference_collect.py"
spec = importlib.util.spec_from_file_location("pv7_reference_collect", MODULE_PATH)
tool = importlib.util.module_from_spec(spec)
spec.loader.exec_module(tool)


class TestReferenceCapture(unittest.TestCase):
    def test_reports_observed_messages_not_inferred_calls(self):
        log = (
            "10-29 15:11:13.146  5878  5904 D RegularShow: native init\n"
            "10-29 15:11:13.570  5878  5904 I System.out: path: /Android/obb/game\n"
            "10-29 15:11:13.571  5878  5904 D RegularShow: native init END.\n"
            "10-29 15:11:13.893  1111  1111 W Other: unrelated\n"
        )
        maps = (
            "d3dfc000-d42dc000 r--p 0 00:00 1 /data/app/game/lib/arm/libgame.so\n"
            "d42dc000-d430f000 r--p 0 00:00 1 /data/app/game/lib/arm/libgame.so\n"
            "d44c3000-d44d0000 r-xp 0 00:00 2 /system/lib/libc.so\n"
        )
        report = tool.summarize(log, maps, "com.turner.bestparkintheuniverse", "5878")
        self.assertEqual(report["schema"], "pv7.reference.capture.v1")
        self.assertEqual(report["native_observation_count"], 2)
        self.assertEqual(report["app_log_line_count"], 3)
        self.assertEqual(len(report["mapped_native_modules"]), 2)
        self.assertTrue(any("not a complete native" in limitation
                            for limitation in report["limitations"]))
        self.assertNotIn("every_function_called", report)

    def test_rejects_invalid_device_or_package_without_adb(self):
        with tempfile.TemporaryDirectory() as root:
            with self.assertRaises(ValueError):
                tool.capture("emulator-5560;rm", "com.example.app",
                             pathlib.Path(root) / "bad", 0, None, "adb")
            with self.assertRaises(ValueError):
                tool.capture("emulator-5560", "invalid/pkg",
                             pathlib.Path(root) / "bad2", 0, None, "adb")
            with self.assertRaises(ValueError):
                tool.capture("emulator-5560", "com.example.app",
                             pathlib.Path(root) / "bad3", 61, None, "adb")
            with self.assertRaises(ValueError):
                tool.capture("emulator-5560", "com.example.app",
                             pathlib.Path(root) / "bad4", 0, "other/Other", "adb")


if __name__ == "__main__":
    unittest.main()
