# ProjectV7 developer hotpatch environment

This directory contains developer-only live compatibility tooling. It is intentionally excluded from normal ProjectV7 production builds.

The workflow is:
1. Reproduce a compatibility failure in the developer harness.
2. Use a temporary rule or native hotpatch plugin to confirm the behavior change without rebuilding the full wrapper.
3. Once confirmed, implement the compatibility behavior in the normal bridge/runtime code under `src/` (or a normal production compatibility module).
4. Rebuild with `PROJECTV7_DEV_HOTPATCH=OFF` to verify the production path has no live plugin loader, rule loader, trap wait loop, or arbitrary native extension loading.

`PROJECTV7_DEV_HOTPATCH` defaults to OFF. Enable it only in a dedicated developer build.
