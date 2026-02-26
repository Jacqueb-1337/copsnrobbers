#!/usr/bin/env powershell
# Compile IPRedirectMod.cs -> bin\Release\IPRedirectMod.dll
# Uses csc.exe (Framework 4.x) against the game's Mono DLLs so it loads on the old Unity runtime.
# DO NOT use 'dotnet build' here — netstandard2.0 assemblies won't load in this Unity version.

$managed = "$PSScriptRoot\apk_source\assets\bin\Data\Managed"
$src     = "$PSScriptRoot\IPRedirectMod.cs"
$outDir  = "$PSScriptRoot\bin\Release"
$out     = "$outDir\IPRedirectMod.dll"

# Find csc.exe
$cscCandidates = @(
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
)
$csc = $cscCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $csc) { Write-Error "csc.exe not found"; exit 1 }
Write-Host "Using compiler: $csc" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

& $csc `
    /out:"$out" `
    /target:library `
    /noconfig /nostdlib `
    /reference:"$managed\mscorlib.dll" `
    /reference:"$managed\System.dll" `
    /reference:"$managed\System.Core.dll" `
    /reference:"$managed\UnityEngine.dll" `
    "$src"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Compilation failed (exit $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "OK: $out" -ForegroundColor Green

# Optional: push to device if ADB is available
$adb = Get-Command adb -ErrorAction SilentlyContinue
if ($adb) {
    $device = "localhost:58526"
    $connected = adb -s $device get-state 2>$null
    if ($connected -eq "device") {
        Write-Host "Pushing to device ($device)..." -ForegroundColor Yellow
        adb -s $device push $out /sdcard/CNRMods/IPRedirectMod.dll
        adb -s $device shell "echo '' > /sdcard/CNRMods/redir.log"
        Write-Host "Pushed. Restart the game to pick up the new DLL." -ForegroundColor Green
    } else {
        Write-Host "ADB device not connected — skipping push." -ForegroundColor Yellow
    }
}
