param([string]$BuildDir = 'build-android-arm64')

$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
$buildTools = Join-Path $sdk 'build-tools\36.1.0'
$zipalign = Join-Path $buildTools 'zipalign.exe'
$apksigner = Join-Path $buildTools 'apksigner.bat'
$nativeBuild = Join-Path $root $BuildDir
$nativeLib = Join-Path $nativeBuild 'libcnr64poc.so'
$work = Join-Path $root 'apk-build'
$staging = Join-Path $work 'staging'
$baseApk = Join-Path $work 'base-unsigned.apk'
$alignedApk = Join-Path $work 'cnr64-poc-aligned-fast.apk'
$finalApk = Join-Path $root 'dist\CNR64-Arm64-Dynarmic-PoC.apk'
$debugKey = Join-Path $env:USERPROFILE '.android\debug.keystore'

if (-not (Test-Path $baseApk)) {
    throw 'Missing apk-build/base-unsigned.apk. Run build_poc_apk.ps1 once first.'
}

cmake --build $nativeBuild --target cnr64poc -j 8
if ($LASTEXITCODE -ne 0) { throw 'Native build failed.' }
if (-not (Test-Path $nativeLib)) { throw "Missing native library: $nativeLib" }

$stagedLib = Join-Path $staging 'lib\arm64-v8a\libcnr64poc.so'
New-Item -ItemType Directory -Force (Split-Path $stagedLib -Parent) | Out-Null
Copy-Item $nativeLib $stagedLib -Force

Push-Location $staging
try {
    & jar uf $baseApk 'lib/arm64-v8a/libcnr64poc.so'
    if ($LASTEXITCODE -ne 0) { throw 'APK native library update failed.' }
} finally {
    Pop-Location
}

& $zipalign -f -p 4 $baseApk $alignedApk
if ($LASTEXITCODE -ne 0) { throw 'zipalign failed.' }
Copy-Item $alignedApk $finalApk -Force
& $apksigner sign --ks $debugKey --ks-pass pass:android --key-pass pass:android --ks-key-alias androiddebugkey $finalApk
if ($LASTEXITCODE -ne 0) { throw 'APK signing failed.' }
& $apksigner verify --verbose $finalApk
if ($LASTEXITCODE -ne 0) { throw 'APK verification failed.' }
Write-Host "FAST APK READY: $finalApk"