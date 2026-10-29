#!/usr/bin/env python3
"""Regression tests: all inputs are synthetic, not copyrighted game assets."""
import importlib.util
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

HERE = Path(__file__).resolve().parent
TOOL = HERE.parent / "tools" / "arm32_compat_analyzer.py"
FIXTURE = HERE / "make_arm32_fixture.py"
spec = importlib.util.spec_from_file_location("arm32_compat_analyzer", TOOL)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class CompatAnalyzerTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.tmp = tempfile.TemporaryDirectory()
        cls.library = Path(cls.tmp.name) / "fixture.so"
        subprocess.run([sys.executable, str(FIXTURE), str(cls.library)], check=True)

    @classmethod
    def tearDownClass(cls):
        cls.tmp.cleanup()

    def test_import_inventory(self):
        report = module.Arm32Elf(self.library.read_bytes()).report(10)
        self.assertEqual(report["analysis_mode"], "offline_static_only")
        self.assertEqual(report["dependencies"], ["liblog.so"])
        self.assertEqual(report["import_counts"]["distinct"], 1)
        self.assertEqual(report["external_imports"][0]["name"], "eglMakeCurrent")
        self.assertEqual(report["external_imports"][0]["symbol_type"], "FUNC")
        self.assertIn("R_ARM_GLOB_DAT",
                      report["external_imports"][0]["relocation_types"])
        self.assertEqual(report["external_imports"][0]["relocation_sites"], ["0x00000504"])
        self.assertEqual(report["initializers"]["entries"], 1)
        self.assertIn("JNI_OnLoad", [s["name"] for s in report["jni_entrypoints"]])
        self.assertEqual(report["runtime_events"], [])
        self.assertEqual(report["runtime_tracing"], "not_connected")
        self.assertEqual(report["historical_android_reference"]["status"],
                         "version_not_identified")

    def test_reference_is_explicit_not_inferred(self):
        report = module.Arm32Elf(self.library.read_bytes()).report(0, "android-4.2.2_r1")
        self.assertEqual(report["branch_candidates"], [])
        self.assertEqual(report["historical_android_reference"]["status"],
                         "unverified_reference_only")

    def test_rejects_truncated_and_wrong_arch(self):
        with self.assertRaises(module.ElfError):
            module.Arm32Elf(b"ELF")
        wrong = bytearray(self.library.read_bytes())
        wrong[18:20] = b"\x3e\x00"  # x86_64
        with self.assertRaises(module.ElfError):
            module.Arm32Elf(bytes(wrong))

    def test_cli_json_and_validation(self):
        target = Path(self.tmp.name) / "report.json"
        subprocess.run([sys.executable, str(TOOL), str(self.library),
                        "--output", str(target), "--max-branches", "5"],
                       check=True)
        saved = json.loads(target.read_text(encoding="utf-8"))
        self.assertEqual(saved["schema"], "pv7.arm32.static.compat.v1")
        bad = subprocess.run([sys.executable, str(TOOL), str(self.library),
                              "--aosp-tag", "../../unsafe"], capture_output=True)
        self.assertNotEqual(bad.returncode, 0)


if __name__ == "__main__":
    unittest.main()
