#!/usr/bin/env python3
import json
import os
import re
import subprocess
from collections import Counter, defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROJECT_ROOT = ROOT.parent
APK_LIB = PROJECT_ROOT / "APK_Build_Active" / "apk_source" / "lib" / "armeabi-v7a"
LIBS = {
    "libmain.so": APK_LIB / "libmain.so",
    "libunity.so": APK_LIB / "libunity.so",
    "libmono.so": APK_LIB / "libmono.so",
}

SDK = Path(os.environ.get("LOCALAPPDATA", "")) / "Android" / "Sdk"
NDK = SDK / "ndk" / "28.2.13676358"
LLVM_BIN = NDK / "toolchains" / "llvm" / "prebuilt" / "windows-x86_64" / "bin"
READELF = LLVM_BIN / "llvm-readelf.exe"
SYSROOT = NDK / "toolchains" / "llvm" / "prebuilt" / "windows-x86_64" / "sysroot"
A64_LIBDIR = SYSROOT / "usr" / "lib" / "aarch64-linux-android" / "24"

MODERN_LIBS = [
    "libc.so", "libm.so", "libdl.so", "liblog.so", "libandroid.so",
    "libEGL.so", "libGLESv1_CM.so", "libGLESv2.so", "libz.so",
]

DATA_SYMBOLS = {
    "__stack_chk_guard", "__page_size", "__sF", "environ", "__data_start",
    "data_start", "__progname", "__progname_full", "timezone", "daylight",
}

MATH_NAMES = {
    "acos", "asin", "atan", "atan2", "ceil", "cos", "cosh", "exp", "fabs",
    "floor", "fmod", "frexp", "ldexp", "log", "log10", "modf", "pow", "rint",
    "round", "sin", "sinh", "sqrt", "tan", "tanh", "trunc",
}
MEMORY_NAMES = {
    "malloc", "calloc", "realloc", "free", "memcpy", "memmove", "memset", "memcmp",
    "mmap", "munmap", "mprotect", "mremap", "madvise", "brk", "sbrk",
}
STRING_NAMES = {
    "strlen", "strcmp", "strncmp", "strcasecmp", "strncasecmp", "strcpy", "strncpy",
    "strcat", "strchr", "strrchr", "strstr", "strpbrk", "strdup", "strtok", "strtok_r",
    "strlcpy", "strerror", "strtol", "strtoul", "strtod", "atoi", "atol", "tolower",
    "isalnum", "isalpha", "isspace", "isxdigit", "bsearch", "qsort",
}
STDIO_NAMES = {
    "fopen", "fdopen", "fclose", "fflush", "fgets", "fputc", "fputs", "fread", "fwrite",
    "fprintf", "fscanf", "printf", "vprintf", "vfprintf", "sprintf", "snprintf", "vsprintf",
    "vsnprintf", "sscanf", "vasprintf", "perror", "puts", "putchar", "popen", "pclose",
}
FILE_NAMES = {
    "open", "close", "read", "write", "lseek", "stat", "lstat", "fstat", "access", "chdir",
    "chmod", "mkdir", "rmdir", "rename", "unlink", "readlink", "fsync", "ftruncate", "sendfile",
    "mkstemp", "opendir", "readdir", "closedir", "getcwd", "utime", "ioctl", "fcntl", "dup2",
    "pipe", "poll", "select", "epoll_create", "epoll_ctl", "epoll_wait",
}
NET_NAMES = {
    "socket", "connect", "bind", "listen", "accept", "shutdown", "send", "sendto", "sendmsg",
    "recv", "recvfrom", "recvmsg", "getsockopt", "setsockopt", "getsockname", "getpeername",
    "getaddrinfo", "freeaddrinfo", "gai_strerror", "gethostbyname", "gethostbyaddr", "gethostname",
    "inet_pton", "getdtablesize",
}
TIME_NAMES = {
    "clock", "clock_getres", "clock_gettime", "gettimeofday", "time", "localtime", "mktime",
    "strftime", "nanosleep", "sleep", "usleep", "setitimer",
}
PROCESS_NAMES = {
    "getpid", "gettid", "getuid", "geteuid", "getresuid", "setresuid", "getpriority", "setpriority",
    "getrusage", "fork", "execv", "execve", "waitpid", "system", "exit", "_exit", "abort", "raise",
    "kill", "tkill", "syscall", "sysconf", "uname", "prctl", "sched_yield", "getenv", "setenv", "unsetenv",
    "getpwnam", "getpwuid", "getgrnam", "getgrgid", "setlocale", "__errno", "__get_h_errno",
}
SIGNAL_NAMES = {
    "sigaction", "sigprocmask", "pthread_sigmask", "sigsuspend", "sigsetjmp", "siglongjmp", "bsd_signal",
    "__pthread_cleanup_push", "__pthread_cleanup_pop",
}
DL_NAMES = {"dlopen", "dlclose", "dlsym", "dlerror"}
CXX_NAMES = {
    "__cxa_atexit", "__cxa_finalize", "__cxa_begin_cleanup", "__cxa_call_unexpected", "__cxa_type_match",
    "__gnu_Unwind_Find_exidx", "__aeabi_memcpy", "__aeabi_memclr", "__aeabi_memset",
}


def run(*args: str) -> str:
    cp = subprocess.run(args, check=True, capture_output=True, text=True, errors="replace")
    return cp.stdout


def strip_version(name: str) -> str:
    return name.split("@", 1)[0]


def parse_needed(path: Path):
    text = run(str(READELF), "-d", str(path))
    return re.findall(r"Shared library: \[([^\]]+)\]", text)


def parse_dynsyms(path: Path):
    text = run(str(READELF), "--dyn-syms", "--wide", str(path))
    undefined = {}
    exports = set()
    # Num: Value Size Type Bind Vis Ndx Name
    rx = re.compile(r"^\s*\d+:\s+[0-9A-Fa-f]+\s+\d+\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(.+?)\s*$")
    for line in text.splitlines():
        m = rx.match(line)
        if not m:
            continue
        typ, bind, vis, ndx, raw_name = m.groups()
        name = strip_version(raw_name.strip())
        if not name:
            continue
        if ndx == "UND":
            undefined[name] = typ
        elif bind in ("GLOBAL", "WEAK") and ndx != "UND":
            exports.add(name)
    return undefined, exports


def parse_relocation_counts(path: Path):
    text = run(str(READELF), "-r", "--wide", str(path))
    counts = Counter()
    # llvm-readelf typically ends each symbol-bearing relocation line with symbol name or "symbol + addend".
    for line in text.splitlines():
        if "R_ARM_" not in line:
            continue
        parts = line.split()
        # Find relocation type then use the first token after value as symbol when present.
        try:
            idx = next(i for i, p in enumerate(parts) if p.startswith("R_ARM_"))
        except StopIteration:
            continue
        if idx + 2 >= len(parts):
            continue
        candidate = strip_version(parts[idx + 2])
        if candidate and not candidate.startswith("0x") and candidate != "0":
            counts[candidate] += 1
    return counts


def modern_exports():
    providers = defaultdict(list)
    for lib in MODERN_LIBS:
        path = A64_LIBDIR / lib
        if not path.exists():
            continue
        try:
            _, exports = parse_dynsyms(path)
        except subprocess.CalledProcessError:
            continue
        for sym in exports:
            providers[sym].append(lib)
    return providers


def classify(name: str, typ: str) -> str:
    if typ == "OBJECT" or name in DATA_SYMBOLS:
        return "data-object"
    if name.startswith("gl") and not name.startswith("glob"):
        return "gles"
    if name.startswith("egl"):
        return "egl"
    if name.startswith(("ANativeWindow_", "AInputEvent_", "AMotionEvent_", "AKeyEvent_", "ALooper_", "AAsset_", "AConfiguration_", "AInputQueue_")):
        return "android-native"
    if name.startswith("__android_log_"):
        return "android-log"
    if name.startswith("pthread_") or name.startswith("sem_"):
        return "threading"
    if name in MEMORY_NAMES:
        return "memory"
    if name in STRING_NAMES:
        return "string"
    if name in MATH_NAMES:
        return "math"
    if name in STDIO_NAMES:
        return "stdio"
    if name in FILE_NAMES:
        return "filesystem"
    if name in NET_NAMES:
        return "network"
    if name in TIME_NAMES:
        return "time"
    if name in DL_NAMES:
        return "dynamic-loader"
    if name in SIGNAL_NAMES:
        return "signals"
    if name in PROCESS_NAMES:
        return "process"
    if name in CXX_NAMES or name.startswith(("__cxa_", "_Unwind_", "__gnu_Unwind_", "__aeabi_")):
        return "cxx-runtime"
    return "other"


def strategy(category: str, modern: bool) -> str:
    if category in ("math",):
        return "direct-host"
    if category in ("memory", "string"):
        return "guest-pointer-adapter"
    if category in ("egl", "gles", "android-native", "android-log"):
        return "host-api-adapter"
    if category in ("threading", "signals"):
        return "runtime-special"
    if category in ("filesystem", "network", "time", "process"):
        return "struct/fd-adapter"
    if category == "stdio":
        return "opaque-handle-adapter"
    if category == "dynamic-loader":
        return "guest-linker-adapter"
    if category == "data-object":
        return "guest-data-slot"
    if category == "cxx-runtime":
        return "guest-runtime-or-special"
    return "review" if not modern else "host-signature-review"


def main():
    if not READELF.exists():
        raise SystemExit(f"llvm-readelf not found: {READELF}")

    modern = modern_exports()
    guest_exports = {}
    per_lib = {}
    all_undef = {}
    for libname, path in LIBS.items():
        undef, exports = parse_dynsyms(path)
        guest_exports[libname] = exports
        rel_counts = parse_relocation_counts(path)
        per_lib[libname] = {
            "path": str(path),
            "needed": parse_needed(path),
            "undefined": undef,
            "relocation_counts": rel_counts,
        }
        for name, typ in undef.items():
            all_undef.setdefault(name, {"types": set(), "owners": set(), "relocations": 0})
            all_undef[name]["types"].add(typ)
            all_undef[name]["owners"].add(libname)
            all_undef[name]["relocations"] += rel_counts.get(name, 0)

    rows = []
    for name in sorted(all_undef):
        info = all_undef[name]
        guest_provider = None
        for libname, exports in guest_exports.items():
            if name in exports:
                guest_provider = libname
                break
        typ = "OBJECT" if "OBJECT" in info["types"] else next(iter(info["types"]), "NOTYPE")
        cat = classify(name, typ)
        providers = modern.get(name, [])
        rows.append({
            "symbol": name,
            "type": typ,
            "owners": sorted(info["owners"]),
            "relocation_count": info["relocations"],
            "guest_provider": guest_provider,
            "modern_arm64_providers": providers,
            "modern_available": bool(providers),
            "category": cat,
            "strategy": "guest-cross-link" if guest_provider else strategy(cat, bool(providers)),
        })

    unresolved = [r for r in rows if not r["guest_provider"]]
    summary = {
        "unique_undefined_symbols": len(rows),
        "guest_cross_link_symbols": sum(1 for r in rows if r["guest_provider"]),
        "system_symbols": len(unresolved),
        "modern_arm64_available": sum(1 for r in unresolved if r["modern_available"]),
        "modern_arm64_missing": sum(1 for r in unresolved if not r["modern_available"]),
        "categories": dict(Counter(r["category"] for r in unresolved)),
        "strategies": dict(Counter(r["strategy"] for r in unresolved)),
    }

    outdir = ROOT / "generated"
    outdir.mkdir(parents=True, exist_ok=True)
    payload = {
        "ndk": str(NDK),
        "modern_sysroot": str(A64_LIBDIR),
        "summary": summary,
        "libraries": {
            name: {
                "needed": data["needed"],
                "undefined_count": len(data["undefined"]),
            }
            for name, data in per_lib.items()
        },
        "symbols": rows,
    }
    (outdir / "abi_inventory.json").write_text(json.dumps(payload, indent=2), encoding="utf-8")

    lines = [
        "# CNR64 ARM32→ARM64 ABI inventory",
        "",
        f"Unique undefined symbols: **{summary['unique_undefined_symbols']}**",
        f"Resolved by another bundled guest library: **{summary['guest_cross_link_symbols']}**",
        f"Remaining system/API symbols: **{summary['system_symbols']}**",
        f"Still exported by Android ARM64 API 24 stubs: **{summary['modern_arm64_available']}**",
        f"Missing from modern ARM64 stubs: **{summary['modern_arm64_missing']}**",
        "",
        "## Categories",
        "",
    ]
    for key, count in sorted(summary["categories"].items(), key=lambda kv: (-kv[1], kv[0])):
        lines.append(f"- {key}: {count}")
    lines += ["", "## Strategy", ""]
    for key, count in sorted(summary["strategies"].items(), key=lambda kv: (-kv[1], kv[0])):
        lines.append(f"- {key}: {count}")
    lines += ["", "## Missing from modern ARM64 Android", ""]
    for row in unresolved:
        if not row["modern_available"]:
            lines.append(f"- `{row['symbol']}` — {row['category']} — {row['strategy']}")
    (outdir / "abi_inventory.md").write_text("\n".join(lines) + "\n", encoding="utf-8")

    # C++ include with stable symbol/category/strategy strings for diagnostics and dispatch policy.
    inc = [
        "// Generated by tools/generate_abi_inventory.py. Do not edit by hand.",
        "static constexpr AbiInventoryEntry kGeneratedAbiInventory[] = {",
    ]
    for row in unresolved:
        providers = ",".join(row["modern_arm64_providers"])
        def esc(s: str) -> str:
            return s.replace("\\", "\\\\").replace('"', '\\"')
        inc.append(
            f'    {{"{esc(row["symbol"])}", "{esc(row["category"])}", "{esc(row["strategy"])}", '
            f'{str(row["modern_available"]).lower()}, "{esc(providers)}"}},'
        )
    inc.append("};")
    (outdir / "abi_inventory.inc").write_text("\n".join(inc) + "\n", encoding="utf-8")

    print(json.dumps(summary, indent=2))


if __name__ == "__main__":
    main()
