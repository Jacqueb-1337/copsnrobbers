param(
    [string]$BuildDir = "build-android-arm64-validation"
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '..')).Path
Set-Location $projectRoot

$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
$ndk = Join-Path $sdk 'ndk\28.2.13676358'
$toolchain = Join-Path $ndk 'build\cmake\android.toolchain.cmake'
$ninja = 'C:\Users\Jacqueb\AppData\Local\Microsoft\WinGet\Packages\Ninja-build.Ninja_Microsoft.Winget.Source_8wekyb3d8bbwe\ninja.exe'
if (-not (Test-Path $toolchain)) { throw "Missing Android toolchain: $toolchain" }
if (-not (Test-Path $ninja)) { $ninja = 'ninja' }

cmake -S . -B $BuildDir -G Ninja `
    "-DCMAKE_MAKE_PROGRAM=$ninja" `
    "-DCMAKE_TOOLCHAIN_FILE=$toolchain" `
    -DANDROID_ABI=arm64-v8a `
    -DANDROID_PLATFORM=android-24 `
    -DCMAKE_BUILD_TYPE=Release `
    -DPROJECTV7_DEV_HOTPATCH=OFF `
    -DPROJECTV7_DEV_DIAGNOSTICS=ON
if ($LASTEXITCODE -ne 0) { throw 'ProjectV7 validation configure failed.' }

Write-Host "PROJECTV7 VALIDATION BUILD CONFIGURED: $BuildDir"
Write-Host 'Verbose diagnostics are enabled; live native hotpatch/plugin loading is excluded.'
