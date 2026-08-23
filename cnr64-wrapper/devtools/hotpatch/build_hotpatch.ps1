param(
    [Parameter(Mandatory=$true)][string]$Source,
    [string]$PluginName = "compat",
    [string]$Serial = "localhost:58526"
)

# Developer-only ProjectV7 hotpatch compiler/deployer. Production builds do not
# contain the loader that consumes these plugins.
$ErrorActionPreference = 'Stop'
$devRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = (Resolve-Path (Join-Path $devRoot '..\..')).Path
Set-Location $projectRoot

$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
$ndk = Join-Path $sdk 'ndk\28.2.13676358'
$toolBin = Join-Path $ndk 'toolchains\llvm\prebuilt\windows-x86_64\bin'
$clang = Join-Path $toolBin 'aarch64-linux-android24-clang++.cmd'
if (-not (Test-Path $clang)) {
    $clang = Join-Path $toolBin 'aarch64-linux-android24-clang++.exe'
}
if (-not (Test-Path $clang)) { throw "Could not find Android clang++ in $toolBin" }

$adb = Join-Path $sdk 'platform-tools\adb.exe'
if (-not (Test-Path $adb)) { throw "Could not find adb: $adb" }

if ([IO.Path]::IsPathRooted($Source)) {
    $sourcePath = (Resolve-Path $Source).Path
} else {
    $sourcePath = (Resolve-Path (Join-Path $devRoot $Source)).Path
}
$outDir = Join-Path $devRoot 'build'
New-Item -ItemType Directory -Force $outDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMddHHmmssfff'
$safeName = ($PluginName -replace '[^A-Za-z0-9_.-]', '_')
$fileName = "$safeName-$stamp.so"
$output = Join-Path $outDir $fileName

& $clang -std=c++20 -O2 -fPIC -shared -static-libstdc++ `
    -I $devRoot `
    $sourcePath -o $output `
    -llog -landroid -ldl -lEGL -lGLESv2
if ($LASTEXITCODE -ne 0) { throw 'ProjectV7 developer hotpatch compile failed.' }

$remoteTmp = "/data/local/tmp/$fileName"
& $adb -s $Serial push $output $remoteTmp | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Hotpatch adb push failed.' }

$remoteDir = 'files/projectv7-dev-hotpatch'
& $adb -s $Serial shell run-as me.jacqueb.cnr64poc mkdir -p $remoteDir
if ($LASTEXITCODE -ne 0) {
    throw 'run-as failed. Install a ProjectV7 developer harness build first.'
}
& $adb -s $Serial shell run-as me.jacqueb.cnr64poc cp $remoteTmp "$remoteDir/$fileName"
if ($LASTEXITCODE -ne 0) { throw 'Could not copy developer hotpatch into app-private directory.' }
& $adb -s $Serial shell rm -f $remoteTmp | Out-Null

Write-Host "PROJECTV7 DEV HOTPATCH DEPLOYED: $fileName"
Write-Host "This plugin is developer-only and is not part of a production compatibility package."
