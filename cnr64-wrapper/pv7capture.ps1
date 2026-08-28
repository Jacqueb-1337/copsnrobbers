param(
    [string]$Serial,
    [int]$TimeoutSec = 30,
    [int]$StallSec = 10,
    [string]$TargetPattern,
    [switch]$NoRestart,
    [string]$OutputDir
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$package = 'me.jacqueb.cnr64poc'
$activity = "$package/.MainActivity"
$runsRoot = Join-Path $root 'devtools\pv7test-runs'

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

function Read-RunLog {
    $lines = Invoke-Adb @('logcat','-d','-v','brief','-s','CNR64POC:I','CNR64HOTPATCH:I','*:S') -AllowFailure
    return ($lines -join "`n")
}

if (-not $OutputDir) {
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $OutputDir = Join-Path $runsRoot $stamp
} elseif (-not [IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $root $OutputDir
}
New-Item -ItemType Directory -Force $OutputDir | Out-Null
$logPath = Join-Path $OutputDir 'logcat.txt'
$metaPath = Join-Path $OutputDir 'capture.json'

Invoke-Adb @('logcat','-c') | Out-Null
if (-not $NoRestart) {
    Invoke-Adb @('shell','am','force-stop',$package) | Out-Null
    Invoke-Adb @('shell','am','start','-n',$activity) | Out-Null
}

$started = Get-Date
$lastAdvance = $started
$lastSignature = ''
$stopReason = 'timeout'
$text = ''

Write-Host "[pv7capture] device $serialValue, up to ${TimeoutSec}s"
while ($true) {
    Start-Sleep -Milliseconds 750
    $text = Read-RunLog
    $progress = [regex]::Matches($text, 'Unity nativeRender(?:Frame\d+)? progress slice=\d+')
    $signature = "$($progress.Count):" + $(if ($progress.Count -gt 0) { $progress[$progress.Count - 1].Value } else { '' })
    if ($signature -ne $lastSignature) {
        $lastSignature = $signature
        $lastAdvance = Get-Date
    }

    if ($TargetPattern -and [regex]::IsMatch($text, $TargetPattern)) {
        $stopReason = "target:$TargetPattern"
        break
    }
    if ([regex]::IsMatch($text, 'Fatal signal|FATAL EXCEPTION|SIGSEGV|SIGABRT')) {
        $stopReason = 'fatal'
        break
    }

    $now = Get-Date
    if (($now - $started).TotalSeconds -ge $TimeoutSec) { break }
    if ($progress.Count -gt 0 -and ($now - $lastAdvance).TotalSeconds -ge $StallSec) {
        $stopReason = 'stall'
        break
    }
}

$text = Read-RunLog
$elapsedSec = [math]::Round(((Get-Date) - $started).TotalSeconds, 2)
[IO.File]::WriteAllText($logPath, $text, [Text.UTF8Encoding]::new($false))
$meta = [pscustomobject]@{
    Timestamp = (Get-Date).ToString('o')
    Serial = $serialValue
    ElapsedSec = $elapsedSec
    StopReason = $stopReason
    LogPath = $logPath
    OutputDir = $OutputDir
}
$meta | ConvertTo-Json -Depth 3 | Set-Content -Encoding UTF8 $metaPath
$lastRun = Join-Path $root 'devtools\pv7test-last-run.json'
$meta | ConvertTo-Json -Depth 3 | Set-Content -Encoding UTF8 $lastRun
Write-Host "[pv7capture] $stopReason after ${elapsedSec}s"
Write-Host "[pv7capture] $logPath"
$meta
