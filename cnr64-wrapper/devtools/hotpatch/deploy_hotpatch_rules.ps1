param(
    [Parameter(Mandatory=$true)][string]$RulesPath,
    [string]$Serial = "localhost:58526"
)

# Developer-only ProjectV7 hotpatch rule deployment.
$ErrorActionPreference = 'Stop'
$devRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
$adb = Join-Path $sdk 'platform-tools\adb.exe'
if (-not (Test-Path $adb)) { throw "Could not find adb: $adb" }

if ([IO.Path]::IsPathRooted($RulesPath)) {
    $sourcePath = (Resolve-Path $RulesPath).Path
} else {
    $sourcePath = (Resolve-Path (Join-Path $devRoot $RulesPath)).Path
}
$remoteTmp = '/data/local/tmp/projectv7-dev-rules.txt'
$remoteDir = 'files/projectv7-dev-hotpatch'
& $adb -s $Serial push $sourcePath $remoteTmp | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Rules adb push failed.' }
& $adb -s $Serial shell run-as me.jacqueb.cnr64poc mkdir -p $remoteDir
if ($LASTEXITCODE -ne 0) { throw 'run-as failed. Install a ProjectV7 developer harness build first.' }
& $adb -s $Serial shell run-as me.jacqueb.cnr64poc cp $remoteTmp "$remoteDir/rules.txt"
if ($LASTEXITCODE -ne 0) { throw 'Could not copy rules into the developer hotpatch directory.' }
& $adb -s $Serial shell rm -f $remoteTmp | Out-Null
Write-Host 'PROJECTV7 DEV HOTPATCH RULES DEPLOYED'
