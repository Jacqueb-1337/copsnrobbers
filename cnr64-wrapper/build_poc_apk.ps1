param(
    [switch]$SkipNativeBuild
)

# Native build tools write non-fatal warnings to stderr. Their exit codes are checked below.
$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
$buildTools = Join-Path $sdk 'build-tools\36.1.0'
$androidJar = Join-Path $sdk 'platforms\android-36\android.jar'
$ndk = Join-Path $sdk 'ndk\28.2.13676358'
$toolchain = Join-Path $ndk 'build\cmake\android.toolchain.cmake'
$ninjaCommand = Get-Command ninja.exe -ErrorAction Stop
$ninja = $ninjaCommand.Source

$aapt2 = Join-Path $buildTools 'aapt2.exe'
$d8 = Join-Path $buildTools 'd8.bat'
$zipalign = Join-Path $buildTools 'zipalign.exe'
$apksigner = Join-Path $buildTools 'apksigner.bat'
$keytool = Join-Path $env:JAVA_HOME 'bin\keytool.exe'
if (-not (Test-Path $keytool)) { $keytool = 'keytool.exe' }

$debugKey = Join-Path $env:USERPROFILE '.android\debug.keystore'
$nativeBuild = Join-Path $root 'build-android-arm64'
$out = Join-Path $root 'dist'
$work = Join-Path $root 'apk-build'
$staging = Join-Path $work 'staging'
$classes = Join-Path $work 'classes'
$dex = Join-Path $work 'dex'

if (-not $SkipNativeBuild) {
    cmake -S $root -B $nativeBuild -G Ninja `
        -DCMAKE_MAKE_PROGRAM="$ninja" `
        -DCMAKE_TOOLCHAIN_FILE="$toolchain" `
        -DANDROID_ABI=arm64-v8a `
        -DANDROID_PLATFORM=android-24 `
        -DCMAKE_BUILD_TYPE=Release `
        -DBoost_INCLUDE_DIR="$root\third_party\boost_1_85_0" `
        -DDYNARMIC_USE_BUNDLED_EXTERNALS=ON `
        -DDYNARMIC_TESTS=OFF
    if ($LASTEXITCODE -ne 0) { throw 'CMake configure failed.' }

    cmake --build $nativeBuild --target cnr64poc -j 8
    if ($LASTEXITCODE -ne 0) { throw 'Native build failed.' }
}

$nativeLib = Join-Path $nativeBuild 'libcnr64poc.so'
if (-not (Test-Path $nativeLib)) { throw "Missing native library: $nativeLib" }
$guestLibMain = Join-Path (Split-Path $root -Parent) 'APK_Build_Active\apk_source\lib\armeabi-v7a\libmain.so'
$guestLibUnity = Join-Path (Split-Path $root -Parent) 'APK_Build_Active\apk_source\lib\armeabi-v7a\libunity.so'
$guestLibMono = Join-Path (Split-Path $root -Parent) 'APK_Build_Active\apk_source\lib\armeabi-v7a\libmono.so'
$guestData = Join-Path (Split-Path $root -Parent) 'APK_Build_Active\apk_source\assets\bin\Data'
$guestManaged = Join-Path $guestData 'Managed'
if (-not (Test-Path $guestLibMain)) { throw "Missing original ARM32 guest library: $guestLibMain" }
if (-not (Test-Path $guestLibUnity)) { throw "Missing original ARM32 Unity guest library: $guestLibUnity" }
if (-not (Test-Path $guestLibMono)) { throw "Missing original ARM32 Mono guest library: $guestLibMono" }
if (-not (Test-Path (Join-Path $guestData 'mainData'))) { throw "Missing original Unity Data directory: $guestData" }
if (-not (Test-Path (Join-Path $guestManaged 'mscorlib.dll'))) { throw "Missing original Managed runtime directory: $guestManaged" }

Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $classes, $dex, $staging, (Join-Path $staging 'lib\arm64-v8a'), (Join-Path $staging 'assets\guest'), (Join-Path $staging 'assets\bin\Data'), $out | Out-Null

$javaSource = Join-Path $root 'app\src\me\jacqueb\cnr64poc\MainActivity.java'
& javac -encoding UTF-8 -source 8 -target 8 -classpath $androidJar -d $classes $javaSource
if ($LASTEXITCODE -ne 0) { throw 'javac failed.' }

$classesJar = Join-Path $work 'classes.jar'
Push-Location $classes
try {
    & jar cf $classesJar .
    if ($LASTEXITCODE -ne 0) { throw 'jar failed.' }
} finally {
    Pop-Location
}

& $d8 --lib $androidJar --min-api 24 --output $dex $classesJar
if ($LASTEXITCODE -ne 0) { throw 'd8 failed.' }

Copy-Item (Join-Path $dex 'classes.dex') (Join-Path $staging 'classes.dex')
Copy-Item $nativeLib (Join-Path $staging 'lib\arm64-v8a\libcnr64poc.so')
Copy-Item $guestLibMain (Join-Path $staging 'assets\guest\libmain.so')
Copy-Item $guestLibUnity (Join-Path $staging 'assets\guest\libunity.so')
Copy-Item $guestLibMono (Join-Path $staging 'assets\guest\libmono.so')
Copy-Item (Join-Path $guestData '*') (Join-Path $staging 'assets\bin\Data') -Recurse -Force

$baseApk = Join-Path $work 'base-unsigned.apk'
& $aapt2 link -o $baseApk -I $androidJar --manifest (Join-Path $root 'app\AndroidManifest.xml') --min-sdk-version 24 --target-sdk-version 36
if ($LASTEXITCODE -ne 0) { throw 'aapt2 link failed.' }

Push-Location $staging
try {
    & jar uf $baseApk classes.dex lib assets
    if ($LASTEXITCODE -ne 0) { throw 'APK ZIP update failed.' }
} finally {
    Pop-Location
}

$alignedApk = Join-Path $work 'cnr64-poc-aligned.apk'
& $zipalign -f -p 4 $baseApk $alignedApk
if ($LASTEXITCODE -ne 0) { throw 'zipalign failed.' }

if (-not (Test-Path $debugKey)) {
    New-Item -ItemType Directory -Force (Split-Path $debugKey -Parent) | Out-Null
    & $keytool -genkeypair -v -keystore $debugKey -storepass android -alias androiddebugkey -keypass android `
        -dname 'CN=Android Debug,O=Android,C=US' -keyalg RSA -keysize 2048 -validity 10000
    if ($LASTEXITCODE -ne 0) { throw 'Debug keystore creation failed.' }
}

$finalApk = Join-Path $out 'CNR64-Arm64-Dynarmic-PoC.apk'
Copy-Item $alignedApk $finalApk -Force
& $apksigner sign --ks $debugKey --ks-pass pass:android --key-pass pass:android --ks-key-alias androiddebugkey $finalApk
if ($LASTEXITCODE -ne 0) { throw 'APK signing failed.' }

& $apksigner verify --verbose $finalApk
if ($LASTEXITCODE -ne 0) { throw 'APK verification failed.' }

Write-Host "APK READY: $finalApk"
