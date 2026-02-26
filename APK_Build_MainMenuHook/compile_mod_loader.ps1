#!/usr/bin/env powershell
# Compile CNR Mod Loader DLLs

$modLoaderDir = "d:\Projects\copsnrobbers\hooking\mod_loader"
$managedDir = "d:\Projects\copsnrobbers\target copy\assets\bin\Data\Managed"
$outputDir = "d:\Projects\copsnrobbers\hooking\mod_loader\compiled"

# Create output directory
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

Write-Host "Compiling CNR Mod Loader DLLs" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green
Write-Host ""

# Find csc.exe (C# compiler)
$cscPaths = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

$csc = $null
foreach ($path in $cscPaths) {
    if (Test-Path $path) {
        $csc = $path
        Write-Host "Found C# compiler: $csc" -ForegroundColor Cyan
        break
    }
}

if (-not $csc) {
    Write-Host "ERROR: Could not find C# compiler (csc.exe)" -ForegroundColor Red
    Write-Host "Please install Visual Studio or .NET SDK"
    exit 1
}

Write-Host ""
Write-Host "Compiling CNRModLoader.dll..." -ForegroundColor Yellow

$args = @(
    "/out:$outputDir\CNRModLoader.dll",
    "/target:library",
    "/reference:`"$managedDir\mscorlib.dll`"",
    "/reference:`"$managedDir\System.dll`"",
    "/reference:`"$managedDir\System.Core.dll`"",
    "/reference:`"$managedDir\UnityEngine.dll`"",
    "`"$modLoaderDir\CNRModLoader.cs`""
)

& $csc $args
if ($? -eq $false) {
    Write-Host "ERROR: Failed to compile CNRModLoader.dll" -ForegroundColor Red
    exit 1
}
Write-Host "✓ CNRModLoader.dll compiled successfully" -ForegroundColor Green

Write-Host ""
Write-Host "Compiling CNRHooking.dll..." -ForegroundColor Yellow

$args = @(
    "/out:$outputDir\CNRHooking.dll",
    "/target:library",
    "/reference:`"$managedDir\mscorlib.dll`"",
    "/reference:`"$managedDir\System.dll`"",
    "/reference:`"$managedDir\System.Core.dll`"",
    "/reference:`"$managedDir\UnityEngine.dll`"",
    "`"$modLoaderDir\CNRHooking.cs`""
)

& $csc $args
if ($? -eq $false) {
    Write-Host "ERROR: Failed to compile CNRHooking.dll" -ForegroundColor Red
    exit 1
}
Write-Host "✓ CNRHooking.dll compiled successfully" -ForegroundColor Green

Write-Host ""
Write-Host "Compiling CNRModLoaderInitializer.dll..." -ForegroundColor Yellow

$args = @(
    "/out:$outputDir\CNRModLoaderInitializer.dll",
    "/target:library",
    "/reference:`"$managedDir\mscorlib.dll`"",
    "/reference:`"$managedDir\System.dll`"",
    "/reference:`"$managedDir\System.Core.dll`"",
    "/reference:`"$managedDir\UnityEngine.dll`"",
    "/reference:`"$outputDir\CNRModLoader.dll`"",
    "`"$modLoaderDir\CNRModLoaderInitializer.cs`""
)

& $csc $args
if ($? -eq $false) {
    Write-Host "ERROR: Failed to compile CNRModLoaderInitializer.dll" -ForegroundColor Red
    exit 1
}
Write-Host "✓ CNRModLoaderInitializer.dll compiled successfully" -ForegroundColor Green

Write-Host ""
Write-Host "Copying compiled DLLs to APK assets..." -ForegroundColor Yellow

Copy-Item "$outputDir\CNRModLoader.dll" "$managedDir\CNRModLoader.dll" -Force
Copy-Item "$outputDir\CNRHooking.dll" "$managedDir\CNRHooking.dll" -Force
Copy-Item "$outputDir\CNRModLoaderInitializer.dll" "$managedDir\CNRModLoaderInitializer.dll" -Force

Write-Host "✓ DLLs copied to $managedDir" -ForegroundColor Green

Write-Host ""
Write-Host "All mod loader DLLs compiled and ready!" -ForegroundColor Green
Write-Host "Next: Run apktool b and jarsigner"
