param(
    [Parameter(Mandatory = $true)][string]$GuestApk,
    [string]$OutDir = 'C:\Users\Jacqueb\projects\pv7-cocos-android-build\jni-activity'
)
# Developer-only reproduction path. Never commit guest APK/native binaries.
$ErrorActionPreference = 'Continue'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
$androidJar = Join-Path $sdk 'platforms\android-35\android.jar'
$bt = Join-Path $sdk 'build-tools\36.1.0'
$nativeRoot = 'C:\Users\Jacqueb\projects'
$stage = Join-Path $OutDir 'stage'
$classes = Join-Path $OutDir 'classes'
$dex = Join-Path $OutDir 'dex'
New-Item -ItemType Directory -Force $OutDir,$stage,$classes,$dex,
    (Join-Path $stage 'lib\x86_64'),(Join-Path $stage 'lib\arm64-v8a'),
    (Join-Path $stage 'assets\guest') | Out-Null

$activity = Join-Path $root 'devtools\cocos_probe\android\me\jacqueb\pv7\cocos\CocosProbeActivity.java'
$renderer = Join-Path $root 'compat_modules\cocos2dx\android\Pv7CocosBitmapRenderer.java'
& javac -encoding UTF-8 -source 8 -target 8 -classpath $androidJar -d $classes $activity $renderer
if ($LASTEXITCODE -ne 0) { throw 'Java compile failed' }
$classesJar=Join-Path $OutDir 'classes.jar'
Push-Location $classes
try { & jar cf $classesJar . } finally { Pop-Location }
if ($LASTEXITCODE -ne 0) { throw 'Java jar failed' }
& (Join-Path $bt 'd8.bat') --lib $androidJar --min-api 26 --output $dex $classesJar
if ($LASTEXITCODE -ne 0) { throw 'D8 failed' }
Copy-Item (Join-Path $dex 'classes.dex') (Join-Path $stage 'classes.dex') -Force
Copy-Item (Join-Path $nativeRoot 'pv7-cocos-host-x86-build\libpv7cocosprobe.so') (Join-Path $stage 'lib\x86_64\libpv7cocosprobe.so') -Force
Copy-Item (Join-Path $nativeRoot 'pv7-cocos-host-arm64-build\libpv7cocosprobe.so') (Join-Path $stage 'lib\arm64-v8a\libpv7cocosprobe.so') -Force

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead((Resolve-Path $GuestApk))
try {
    $entry = $zip.GetEntry('lib/armeabi/libgame.so')
    if (-not $entry) { $entry = $zip.GetEntry('lib/armeabi-v7a/libgame.so') }
    if (-not $entry) { throw 'Best Park APK has no ARM32 libgame.so' }
    [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,
        (Join-Path $stage 'assets\guest\libgame.so'), $true)
} finally { $zip.Dispose() }

$base = Join-Path $OutDir 'base-unsigned.apk'
& (Join-Path $bt 'aapt2.exe') link -o $base -I $androidJar --manifest (Join-Path $root 'devtools\cocos_probe\android\AndroidManifest.xml') --min-sdk-version 26 --target-sdk-version 28
if ($LASTEXITCODE -ne 0) { throw 'aapt2 link failed' }
Push-Location $stage
try { & jar uf $base classes.dex lib assets } finally { Pop-Location }
if ($LASTEXITCODE -ne 0) { throw 'APK update failed' }
$aligned = Join-Path $OutDir 'pv7-cocos-activity-aligned.apk'
$apk = Join-Path $OutDir 'pv7-cocos-activity-debug.apk'
& (Join-Path $bt 'zipalign.exe') -f -p 4 $base $aligned
if ($LASTEXITCODE -ne 0) { throw 'zipalign failed' }
& (Join-Path $bt 'apksigner.bat') sign --ks (Join-Path $env:USERPROFILE '.android\debug.keystore') --ks-key-alias androiddebugkey --ks-pass pass:android --key-pass pass:android --out $apk $aligned
if ($LASTEXITCODE -ne 0) { throw 'APK signing failed' }
& (Join-Path $bt 'apksigner.bat') verify --verbose $apk
if ($LASTEXITCODE -ne 0) { throw 'APK verification failed' }
Write-Output "PV7_ANDROID_COCOS_ACTIVITY_APK=$apk"