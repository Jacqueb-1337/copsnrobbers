# build_mod.ps1 — compile a CNRMod .cs file against the game's managed DLLs and
# push the resulting DLL to the connected Android device via adb.
#
# FULL RELEASE WORKFLOW (after this script succeeds):
#   1. Copy the built DLL to:
#        cnr-revived-web/mods/<ModName>/<ModName>.dll          (latestUrl target)
#        cnr-revived-web/mods/<ModName>/<ModName>-<ver>.dll    (versioned URL)
#   2. Update cnr-revived-web/mods/repo.json:
#        - bump "latestVersion"
#        - add a new entry to "versions" with url + changelog
#   3. Commit + push so play.jacqueb.me serves the updated DLL and repo.json.
#
# Usage examples:
#   .\build_mod.ps1 -ModFile CNRMod.cs
#   .\build_mod.ps1 -ModFile ..\cnr-revived-web\mods\CNRMod\CNRMod.cs -OutName CNRMod
#   .\build_mod.ps1 -ModFile CNRMod.cs -NoDeploy
#   .\build_mod.ps1 -ModFile CNRMod.cs -Device 192.168.1.5:5555

param(
    [Parameter(Mandatory=$true)]
    [string]$ModFile,          # e.g. CNRSettingsMod.cs  or  C:\full\path\MyMod.cs

    [string]$OutName = "",     # output DLL name (with or without .dll); defaults to source file basename

    [string]$Device = "",      # adb device serial; leave empty to use default connected device

    [switch]$NoDeploy          # skip adb push (build only)
)

$ErrorActionPreference = "Stop"

# ---- Paths ------------------------------------------------------------------
# $ManagedDir — Unity game DLLs extracted from the APK; used as compilation references.
# $OutDir     — compiled DLLs land here before being pushed to the device.
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
# csc is the .NET Framework 4 C# compiler; /nostdlib + explicit references keep
# the output compatible with the Mono runtime embedded in the game.
Write-Host "Building $OutName from $([System.IO.Path]::GetFileName($ModFile)) ..." -ForegroundColor Cyan

$ModDir = [System.IO.Path]::GetDirectoryName($ModFile)

# CNRMod carries baked Zombies overlay content. Regenerate the C# registry from
# maps/zombies/configs + assets immediately before source discovery/compile.
if ([System.IO.Path]::GetFileName($ModDir) -ieq "CNRMod") {
    $zombieGen = Join-Path $ScriptDir "generate_zombie_builtin.ps1"
    if (Test-Path $zombieGen) {
        & $zombieGen
        $generatedZombieSource = Join-Path $ModDir "ZombieBuiltinContent.generated.cs"
        if (-not (Test-Path $generatedZombieSource)) {
            Write-Host "ERROR: Zombies built-in content generation did not produce $generatedZombieSource" -ForegroundColor Red
            exit 1
        }
    }
}
$sourceFiles = Get-ChildItem -Path $ModDir -File -Filter "*.cs" |
    Where-Object { $_.Name -notmatch '-\d+\.\d+\.\d+\.cs$' } |
    Sort-Object Name

if ($sourceFiles.Count -eq 0) {
    Write-Host "ERROR: No source files found in $ModDir" -ForegroundColor Red
    exit 1
}

Write-Host "Compiling source files:" -ForegroundColor Cyan
foreach ($sf in $sourceFiles) { Write-Host "  $($sf.Name)" -ForegroundColor Gray }

$compileArgs = @(
    "/nostdlib", "/noconfig",
    "/out:$OutDll",
    "/target:library",
    "/reference:$ManagedDir\mscorlib.dll",
    "/reference:$ManagedDir\System.dll",
    "/reference:$ManagedDir\System.Core.dll",
    "/reference:$ManagedDir\UnityEngine.dll",
    "/reference:$ManagedDir\JsonFx.Json.dll",
    "/reference:$ManagedDir\Assembly-CSharp-firstpass.dll",
    "/reference:$ManagedDir\Assembly-CSharp.dll",
    "/reference:$ManagedDir\Photon3Unity3D.dll"
)
foreach ($sf in $sourceFiles) { $compileArgs += $sf.FullName }

& $Csc @compileArgs 2>&1 | Where-Object { $_ -notmatch "go.microsoft.com|only supports language" }

if ($LASTEXITCODE -ne 0) {
    Write-Host "BUILD FAILED" -ForegroundColor Red
    exit 1
}

$size = (Get-Item $OutDll).Length
Write-Host "BUILD OK  ($size bytes)  ->  $OutDll" -ForegroundColor Green

# Derive the mod's source directory so we can show the exact copy destinations.
$ModDir     = [System.IO.Path]::GetDirectoryName($ModFile)
$ModBase    = [System.IO.Path]::GetFileNameWithoutExtension($OutName)  # e.g. CNRMod
$RepoMods   = "cnr-revived-web\mods"

Write-Host ""
Write-Host "Next steps to publish a release:" -ForegroundColor Yellow
Write-Host "  1. Copy DLL to repo:" -ForegroundColor Yellow
Write-Host "       $ModDir\$OutName" -ForegroundColor Gray
Write-Host "       $ModDir\$ModBase-<version>.dll" -ForegroundColor Gray
Write-Host "  2. Update $RepoMods\repo.json - bump latestVersion, add versions entry + changelog" -ForegroundColor Yellow
Write-Host "  3. git commit + push  (play.jacqueb.me will serve the updated DLL and repo.json)" -ForegroundColor Yellow
Write-Host ""

# ---- Deploy to device -------------------------------------------------------
if ($NoDeploy) {
    Write-Host "Skipping deploy (-NoDeploy)" -ForegroundColor Yellow
    exit 0
}

Write-Host "Deploying to device ..." -ForegroundColor Cyan

# Auto-connect to WSA over TCP if no device is currently attached via adb.
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
Write-Host "DEPLOY OK  ->  /sdcard/CNRMods/$OutName" -ForegroundColor Green
