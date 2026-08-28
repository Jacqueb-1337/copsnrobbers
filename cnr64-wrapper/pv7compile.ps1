param(
    [Parameter(Mandatory = $true)]
    [string]$Plugin,
    [string]$PluginName,
    [string]$ResultPath = 'devtools/pv7test-next-plugin.json'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$devRoot = Join-Path $root 'devtools\hotpatch'
$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
$ndk = Join-Path $sdk 'ndk\28.2.13676358'
$toolBin = Join-Path $ndk 'toolchains\llvm\prebuilt\windows-x86_64\bin'
$clang = Join-Path $toolBin 'aarch64-linux-android24-clang++.cmd'
if (-not (Test-Path $clang)) { $clang = Join-Path $toolBin 'aarch64-linux-android24-clang++.exe' }
if (-not (Test-Path $clang)) { throw "Android clang++ not found in $toolBin" }

if ([IO.Path]::IsPathRooted($Plugin)) {
    $sourcePath = (Resolve-Path $Plugin).Path
} else {
    $rootCandidate = Join-Path $root $Plugin
    $hotpatchCandidate = Join-Path $devRoot $Plugin
    if (Test-Path $rootCandidate) { $sourcePath = (Resolve-Path $rootCandidate).Path }
    elseif (Test-Path $hotpatchCandidate) { $sourcePath = (Resolve-Path $hotpatchCandidate).Path }
    else { throw "Plugin source not found: $Plugin" }
}

if (-not $PluginName) { $PluginName = [IO.Path]::GetFileNameWithoutExtension($sourcePath) }
$safeName = $PluginName -replace '[^A-Za-z0-9_.-]', '_'
$buildDir = Join-Path $devRoot 'build'
New-Item -ItemType Directory -Force $buildDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMddHHmmssfff'
$fileName = "$safeName-$stamp.so"
$outputPath = Join-Path $buildDir $fileName

Write-Host "[pv7compile] $safeName"
$previousPreference = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
try {
    & $clang -std=c++20 -O2 -fPIC -shared -static-libstdc++ -I $devRoot $sourcePath -o $outputPath -llog -landroid -ldl -lEGL -lGLESv2
    $compileCode = $LASTEXITCODE
} finally {
    $ErrorActionPreference = $previousPreference
}
if ($compileCode -ne 0) { throw "Hotpatch compile failed ($compileCode)." }

$result = [pscustomobject]@{
    Timestamp = (Get-Date).ToString('o')
    Name = $safeName
    File = $fileName
    Source = $sourcePath
    Path = (Resolve-Path $outputPath).Path
}

$resultFile = if ([IO.Path]::IsPathRooted($ResultPath)) { $ResultPath } else { Join-Path $root $ResultPath }
New-Item -ItemType Directory -Force (Split-Path $resultFile -Parent) | Out-Null
$result | ConvertTo-Json -Depth 3 | Set-Content -Encoding UTF8 $resultFile
Write-Host "[pv7compile] built $($result.Path)"
Write-Host "[pv7compile] handoff $resultFile"
$result
