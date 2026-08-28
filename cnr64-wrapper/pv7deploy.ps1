param(
    [string]$PluginPath,
    [string]$PluginName,
    [string]$Serial,
    [switch]$Clean,
    [string]$HandoffPath = 'devtools/pv7test-next-plugin.json'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$package = 'me.jacqueb.cnr64poc'
$remotePluginDir = 'files/projectv7-dev-hotpatch'

function Get-AdbPath {
    $candidate = Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe'
    if (Test-Path $candidate) { return $candidate }
    $cmd = Get-Command adb.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    throw 'adb.exe was not found.'
}

function Resolve-Serial([string]$Requested, [string]$Adb) {
    $devices = @((& $Adb devices) | ForEach-Object {
        if ($_ -match '^([^\s]+)\s+device$') { $matches[1] }
    })
    if ($Requested) {
        if ($devices -notcontains $Requested) { throw "ADB device '$Requested' is not connected." }
        return $Requested
    }
    foreach ($preferred in @('127.0.0.1:58526', 'localhost:58526')) {
        if ($devices -contains $preferred) { return $preferred }
    }
    if ($devices.Count -eq 0) { throw 'No authorized ADB device is connected.' }
    return $devices[0]
}

$adb = Get-AdbPath
$serialValue = Resolve-Serial $Serial $adb

function Invoke-Adb([string[]]$AdbArgs, [switch]$AllowFailure) {
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & $adb -s $serialValue @AdbArgs 2>&1
        $code = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previousPreference
    }
    if (-not $AllowFailure -and $code -ne 0) {
        throw "adb failed ($code): $($AdbArgs -join ' ')`n$($output -join "`n")"
    }
    return ,$output
}

$handoffFile = if ([IO.Path]::IsPathRooted($HandoffPath)) { $HandoffPath } else { Join-Path $root $HandoffPath }
$handoff = $null
if ((-not $PluginPath) -and (Test-Path $handoffFile)) {
    $handoff = Get-Content $handoffFile -Raw | ConvertFrom-Json
    $PluginPath = $handoff.Path
    if (-not $PluginName) { $PluginName = $handoff.Name }
}

if (-not $PluginPath) { throw 'PluginPath was not supplied and no compile handoff exists.' }
$resolvedPlugin = (Resolve-Path $PluginPath).Path
if (-not $PluginName) { $PluginName = [IO.Path]::GetFileNameWithoutExtension($resolvedPlugin) }
$safeName = $PluginName -replace '[^A-Za-z0-9_.-]', '_'
$fileName = [IO.Path]::GetFileName($resolvedPlugin)

Invoke-Adb @('shell','run-as',$package,'mkdir','-p',$remotePluginDir) | Out-Null
$existing = Invoke-Adb @('shell','run-as',$package,'ls','-1',$remotePluginDir) -AllowFailure
if ($Clean) {
    foreach ($file in $existing) {
        $name = $file.ToString().Trim()
        if ($name -like '*.so') {
            Invoke-Adb @('shell','run-as',$package,'rm','-f',"$remotePluginDir/$name") | Out-Null
        }
    }
} else {
    foreach ($file in $existing) {
        $name = $file.ToString().Trim()
        if ($name -like "$safeName-*.so" -or $name -like ".cnr64-loaded-*-$safeName-*.so") {
            Invoke-Adb @('shell','run-as',$package,'rm','-f',"$remotePluginDir/$name") | Out-Null
        }
    }
}

$remoteTmp = "/data/local/tmp/$fileName"
Invoke-Adb @('push',$resolvedPlugin,$remoteTmp) | Out-Null
Invoke-Adb @('shell','run-as',$package,'cp',$remoteTmp,"$remotePluginDir/$fileName") | Out-Null
Invoke-Adb @('shell','rm','-f',$remoteTmp) | Out-Null

$result = [pscustomobject]@{
    Timestamp = (Get-Date).ToString('o')
    Serial = $serialValue
    Name = $safeName
    File = $fileName
    Path = $resolvedPlugin
    Clean = [bool]$Clean
}
$resultPath = Join-Path $root 'devtools\pv7test-last-deploy.json'
$result | ConvertTo-Json -Depth 3 | Set-Content -Encoding UTF8 $resultPath
Write-Host "[pv7deploy] deployed $fileName to $serialValue"
$result
