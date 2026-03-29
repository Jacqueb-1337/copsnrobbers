param(
    [Parameter(Mandatory=$true)]
    [string]$ModFile,          # e.g. CNRSettingsMod.cs  or  C:\full\path\MyMod.cs

    [string]$OutName = "",     # output DLL name; defaults to source file basename

    [string]$Device = "",      # adb device serial; leave empty to use default connected device

    [switch]$NoDeploy          # pass -NoDeploy to skip adb push
)

$ErrorActionPreference = "Stop"

# ---- Paths ------------------------------------------------------------------
$ScriptDir  = $PSScriptRoot
$ManagedDir = "$ScriptDir\apk_source\assets\bin\Data\Managed"
$OutDir     = "$ScriptDir\bin\csc_build"
$Csc        = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

# ---- Resolve source file ----------------------------------------------------
if (-not [System.IO.Path]::IsPathRooted($ModFile)) {
    $ModFile = Join-Path $ScriptDir $ModFile
}
if (-not (Test-Path $ModFile)) {
    Write-Host "ERROR: Source file not found: $ModFile" -ForegroundColor Red
    exit 1
}

# ---- Resolve output name ----------------------------------------------------
if ($OutName -eq "") {
    $OutName = [System.IO.Path]::GetFileNameWithoutExtension($ModFile) + ".dll"
}
if (-not $OutName.EndsWith(".dll")) { $OutName += ".dll" }
$OutDll = "$OutDir\$OutName"

# ---- Ensure output dir exists -----------------------------------------------
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

# ---- Compile ----------------------------------------------------------------
Write-Host "Building $OutName from $([System.IO.Path]::GetFileName($ModFile)) ..." -ForegroundColor Cyan

& $Csc /nostdlib /noconfig `
    "/out:$OutDll" `
    "/target:library" `
    "/reference:$ManagedDir\mscorlib.dll" `
    "/reference:$ManagedDir\System.dll" `
    "/reference:$ManagedDir\System.Core.dll" `
    "/reference:$ManagedDir\UnityEngine.dll" `
    "/reference:$ManagedDir\JsonFx.Json.dll" `
    "/reference:$ManagedDir\Assembly-CSharp.dll" `
    "$ModFile" 2>&1 | Where-Object { $_ -notmatch "go.microsoft.com|only supports language" }

if ($LASTEXITCODE -ne 0) {
    Write-Host "BUILD FAILED" -ForegroundColor Red
    exit 1
}

$size = (Get-Item $OutDll).Length
Write-Host "BUILD OK  ($size bytes)  ->  $OutDll" -ForegroundColor Green

# ---- Deploy -----------------------------------------------------------------
if ($NoDeploy) {
    Write-Host "Skipping deploy (-NoDeploy)" -ForegroundColor Yellow
    exit 0
}

Write-Host "Deploying ..." -ForegroundColor Cyan

# Auto-connect to WSA/device over TCP if no device is currently attached
$adbDevices = & adb devices 2>&1
if (-not ($adbDevices | Where-Object { $_ -match "device$" })) {
    $wsa_addr = "127.0.0.1:58526"
    Write-Host "No adb device found - connecting to $wsa_addr ..." -ForegroundColor Yellow
    & adb connect $wsa_addr
    Start-Sleep -Seconds 1
}

$adbArgs = @("push", $OutDll, "/sdcard/CNRMods/$OutName")
if ($Device -ne "") { $adbArgs = @("-s", $Device) + $adbArgs }
& adb @adbArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "DEPLOY FAILED" -ForegroundColor Red
    exit 1
}
Write-Host "DEPLOY OK" -ForegroundColor Green
