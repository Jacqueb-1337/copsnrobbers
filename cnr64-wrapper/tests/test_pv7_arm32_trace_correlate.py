#!/usr/bin/env python3
"""Offline fixtures for real-ARM trace correlation and Thumb symbol lookup."""
import importlib.util
import json
from pathlib import Path
import struct
import tempfile
import unittest

TOOL = Path(__file__).resolve().parents[1] / "tools" / "pv7_arm32_trace_correlate.py"
spec = importlib.util.spec_from_file_location("pv7_arm32_trace_correlate", TOOL)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


def synthetic_arm_elf(path):
    blob = bytearray(0x300)
    ident = b"\x7fELF\x01\x01\x01" + b"\x00" * 9
    struct.pack_into("<16sHHIIIIIHHHHHH", blob, 0,
                     ident, 3, 40, 1, 0, 0, 0x200, 0, 52, 0, 0, 40, 3, 0)
    names = b"\x00_ZN4Game6renderEv\x00"
    blob[0x80:0x80 + len(names)] = names
    # 2 entries: null + real function at Thumb st_value=0x201.
    struct.pack_into("<IIIBBH", blob, 0x110, 1, 0x201, 0x30, 0x12, 0, 1)
    struct.pack_into("<IIIIIIIIII", blob, 0x200 + 40, 0, 3, 0, 0,
                     0x80, len(names), 0, 0, 1, 1)
    struct.pack_into("<IIIIIIIIII", blob, 0x200 + 80, 0, 11, 0, 0,
                     0x100, 32, 1, 0, 4, 16)
    path.write_bytes(blob)


class CorrelateTests(unittest.TestCase):
    def test_thumb_elf_ranges(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "libgame.so"
            synthetic_arm_elf(path)
            symbols = module.elf_functions(path)
            self.assertEqual(len(symbols), 1)
            match = module.resolve("0x211", symbols)
            self.assertEqual(match["symbol"], "_ZN4Game6renderEv")
            self.assertEqual(match["offset_in_function"], 0x10)
            self.assertIsNone(module.resolve("0x400", symbols))
            self.assertIsNone(module.resolve(None, symbols))

    def test_rejects_non_arm_or_invalid_elf(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "bad.so"
            path.write_bytes(b"not an ELF")
            with self.assertRaises(ValueError):
                module.elf_functions(path)
            synthetic_arm_elf(path)
            blob = bytearray(path.read_bytes())
            struct.pack_into("<H", blob, 18, 3)
            path.write_bytes(blob)
            with self.assertRaises(ValueError):
                module.elf_functions(path)

    def test_trace_counts_and_samples_are_not_confused(self):
        with tempfile.TemporaryDirectory() as tmp:
            folder = Path(tmp)
            elf = folder / "libgame.so"
            synthetic_arm_elf(elf)
            path = folder / "input.jsonl"
            events = [
                {"kind":"ready", "profile":"input", "hooked":2},
                {"kind":"phase", "profile":"input", "name":"before_adb_tap"},
                {"kind":"call", "profile":"input",
                 "name":"touch", "caller_module":"libgame.so", "caller_offset":"0x211"},
                {"kind":"return", "profile":"input", "name":"touch"},
                {"kind":"phase", "profile":"input", "name":"after_adb_tap"},
            ]
            path.write_text("\n".join(json.dumps(x) for x in events) + "\n")
            path.with_suffix(".summary.json").write_text(json.dumps({
                "profile":"input", "hook_setup":{"hooked":2,"failures":0},
                "observed_by_symbol":{"touch":50},
                "final":{"counts":{"0x81":50}, "names":{"0x81":"touch"}}
            }))
            result = module.correlate([path], elf)
            item = result["captures"][0]
            self.assertEqual(item["total_intercepted_selected_calls"], 50)
            self.assertEqual(item["sampled_calls"], 1)
            self.assertEqual(item["sampled_returns"], 1)
            self.assertEqual(len(item["phases"]), 2)
            self.assertEqual(item["caller_sites"][0]["caller_elf_symbol"]["symbol"],
                             "_ZN4Game6renderEv")
            self.assertIn("sampled", module.to_markdown(result).lower())
            self.assertTrue(any("not exhaustive" in x for x in result["limitations"]))

    def test_missing_summary_means_unknown_full_counts(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "partial.jsonl"
            path.write_text(json.dumps({"kind":"call","name":"drawScene",
                                        "caller_module":"libdvm.so",
                                        "caller_offset":"0x1"}) + "\n")
            result = module.correlate([path])
            sample = result["captures"][0]
            self.assertFalse(sample["has_final_counts"])
            self.assertEqual(sample["total_intercepted_selected_calls"], 0)
            self.assertEqual(sample["sampled_calls"], 1)


if __name__ == "__main__":
    unittest.main()
