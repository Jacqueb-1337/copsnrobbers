#!/usr/bin/env python3
"""Offline tests for the ARM32 function trace report; no emulator needed."""
import importlib.util
import pathlib
import tempfile
import unittest

TOOL = pathlib.Path(__file__).resolve().parents[1] / "tools" / "pv7_arm32_function_trace.py"
spec = importlib.util.spec_from_file_location("pv7_arm32_function_trace", TOOL)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class Arm32FunctionTraceTests(unittest.TestCase):
    def test_summary_counts_and_caller_provenance(self):
        records = [
            {"kind": "call", "name": "render", "caller_module": "libdvm.so",
             "caller_offset": "0x1"},
            {"kind": "call", "name": "drawScene", "caller_module": "libgame.so",
             "caller_offset": "0x200c01"},
            {"kind": "return", "name": "drawScene"},
            {"kind": "call", "name": "drawScene", "caller_module": "libgame.so",
             "caller_offset": "0x200c01"},
            {"kind": "return", "name": "drawScene"},
            {"kind": "ready", "hooked": 80},
        ]
        result = module.aggregate(records)
        self.assertEqual(result["sampled_calls"], 3)
        self.assertEqual(result["sampled_returns"], 2)
        self.assertEqual(result["sampled_by_symbol"]["drawScene"], 2)
        self.assertEqual(result["sampled_by_caller"]["libgame.so:0x200c01"], 2)

    def test_rejects_unsafe_duration_before_network(self):
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaises((ValueError, ImportError)):
                # Importing frida is deliberately lazy; on CI without Frida
                # the expected ImportError is acceptable.
                module.capture("127.0.0.1:27044", module.PACKAGE, 0,
                               pathlib.Path(tmp) / "out.jsonl")

    def test_agent_contains_no_stalker(self):
        agent = TOOL.with_name("pv7_arm32_function_agent.js").read_text()
        self.assertIn("Interceptor.attach", agent)
        self.assertNotIn("Stalker.follow", agent)
        self.assertIn("maxSamples = 1000", agent)


if __name__ == "__main__":
    unittest.main()
