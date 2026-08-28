param(
    [string]$Plugin,
    [string]$PluginName,
    [string]$Serial,
    [int]$TimeoutSec = 30,
    [int]$StallSec = 10,
    [string]$TargetPattern,
    [switch]$NoRestart,
    [switch]$CleanDevicePlugins,
    [switch]$SaveBaseline,
    [switch]$SelfTest,
    [string]$Baseline = "devtools/pv7test-baseline.json"
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$package = 'me.jacqueb.cnr64poc'
$activity = "$package/.MainActivity"
$remotePluginDir = 'files/projectv7-dev-hotpatch'
$runsDir = Join-Path $root 'devtools\pv7test-runs'
New-Item -ItemType Directory -Force $runsDir | Out-Null

function Get-AdbPath {
    $candidate = Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe'
    if (Test-Path $candidate) { return $candidate }
    $cmd = Get-Command adb.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    throw 'adb.exe was not found.'
}

function Resolve-Serial([string]$Requested, [string]$Adb) {
    $lines = & $Adb devices
    $devices = @($lines | ForEach-Object {
        if ($_ -match '^([^\s]+)\s+device$') { $matches[1] }
    })
    if ($Requested) {
        if ($devices -notcontains $Requested) {
            throw "ADB device '$Requested' is not connected. Connected: $($devices -join ', ')"
        }
        return $Requested
    }
    if ($devices.Count -eq 0) { throw 'No authorized ADB device is connected.' }
    $preferred = @('127.0.0.1:58526', 'localhost:58526')
    foreach ($item in $preferred) {
        if ($devices -contains $item) { return $item }
    }
    return $devices[0]
}

function Invoke-Adb([string[]]$AdbArgs, [switch]$AllowFailure) {
    $output = & $script:adb -s $script:serial @AdbArgs 2>&1
    $code = $LASTEXITCODE
    if (-not $AllowFailure -and $code -ne 0) {
        throw "adb failed ($code): adb -s $script:serial $($AdbArgs -join ' ')`n$($output -join "`n")"
    }
    return ,$output
}

function Get-RemotePluginFiles {
    $output = Invoke-Adb @('shell','run-as',$script:package,'ls','-1',$script:remotePluginDir) -AllowFailure
    if ($LASTEXITCODE -ne 0) { return @() }
    return @($output | ForEach-Object { $_.ToString().Trim() } | Where-Object { $_ })
}

function Remove-RemotePluginFiles([scriptblock]$Predicate) {
    $files = Get-RemotePluginFiles
    foreach ($file in $files) {
        if (& $Predicate $file) {
            Invoke-Adb @('shell','run-as',$script:package,'rm','-f',"$script:remotePluginDir/$file") | Out-Null
        }
    }
}

function Install-HotpatchPlugin([string]$Source, [string]$Name) {
    $devRoot = Join-Path $script:root 'devtools\hotpatch'
    $sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
    $ndk = Join-Path $sdk 'ndk\28.2.13676358'
    $toolBin = Join-Path $ndk 'toolchains\llvm\prebuilt\windows-x86_64\bin'
    $clang = Join-Path $toolBin 'aarch64-linux-android24-clang++.cmd'
    if (-not (Test-Path $clang)) { $clang = Join-Path $toolBin 'aarch64-linux-android24-clang++.exe' }
    if (-not (Test-Path $clang)) { throw "Android clang++ not found in $toolBin" }

    if ([IO.Path]::IsPathRooted($Source)) {
        $sourcePath = (Resolve-Path $Source).Path
    } else {
        $rootCandidate = Join-Path $script:root $Source
        $hotpatchCandidate = Join-Path $devRoot $Source
        if (Test-Path $rootCandidate) { $sourcePath = (Resolve-Path $rootCandidate).Path }
        elseif (Test-Path $hotpatchCandidate) { $sourcePath = (Resolve-Path $hotpatchCandidate).Path }
        else { throw "Plugin source not found: $Source" }
    }

    if (-not $Name) { $Name = [IO.Path]::GetFileNameWithoutExtension($sourcePath) }
    $safeName = $Name -replace '[^A-Za-z0-9_.-]', '_'
    $buildDir = Join-Path $devRoot 'build'
    New-Item -ItemType Directory -Force $buildDir | Out-Null
    $stamp = Get-Date -Format 'yyyyMMddHHmmssfff'
    $fileName = "$safeName-$stamp.so"
    $output = Join-Path $buildDir $fileName

    Write-Host "[pv7test] compile $safeName"
    & $clang -std=c++20 -O2 -fPIC -shared -static-libstdc++ -I $devRoot $sourcePath -o $output -llog -landroid -ldl -lEGL -lGLESv2
    if ($LASTEXITCODE -ne 0) { throw 'Hotpatch compile failed.' }

    Invoke-Adb @('shell','run-as',$script:package,'mkdir','-p',$script:remotePluginDir) | Out-Null
    Remove-RemotePluginFiles { param($f) $f -like "$safeName-*.so" -or $f -like ".cnr64-loaded-*-$safeName-*.so" }

    $remoteTmp = "/data/local/tmp/$fileName"
    Invoke-Adb @('push',$output,$remoteTmp) | Out-Null
    Invoke-Adb @('shell','run-as',$script:package,'cp',$remoteTmp,"$script:remotePluginDir/$fileName") | Out-Null
    Invoke-Adb @('shell','rm','-f',$remoteTmp) | Out-Null
    Write-Host "[pv7test] deployed $fileName"
    return [pscustomobject]@{ Source=$sourcePath; Name=$safeName; File=$fileName; Path=$output }
}

function Read-RunLog {
    $lines = Invoke-Adb @('logcat','-d','-v','brief','-s','CNR64POC:I','CNR64HOTPATCH:I','*:S') -AllowFailure
    return ($lines -join "`n")
}

function Get-RunMetrics([string]$Text, [double]$ElapsedSec, [string]$StopReason) {
    $progress = [regex]::Matches($Text, 'Unity nativeRender(?<frame>Frame\d+)? progress slice=(?<slice>\d+).*?main_us=(?<main>\d+) worker_us=(?<worker>\d+) worker_pumps=(?<pumps>\d+)')
    $frames = @{}
    foreach ($m in $progress) {
        $frameName = if ($m.Groups['frame'].Success) { $m.Groups['frame'].Value } else { 'Frame1' }
        $frameNumber = if ($frameName -eq 'Frame1') { 1 } else { [int]($frameName -replace 'Frame','') }
        $row = [pscustomobject]@{
            Frame = $frameNumber
            Slice = [int64]$m.Groups['slice'].Value
            MainUs = [int64]$m.Groups['main'].Value
            WorkerUs = [int64]$m.Groups['worker'].Value
            WorkerPumps = [int64]$m.Groups['pumps'].Value
        }
        if (-not $frames.ContainsKey($frameNumber) -or $row.Slice -ge $frames[$frameNumber].Slice) {
            $frames[$frameNumber] = $row
        }
    }

    $maxFrame = 0
    if ($frames.Count -gt 0) { $maxFrame = ($frames.Keys | Measure-Object -Maximum).Maximum }
    $maxRow = if ($maxFrame -gt 0) { $frames[$maxFrame] } else { $null }
    $f2 = if ($frames.ContainsKey(2)) { $frames[2] } else { $null }
    $f3 = if ($frames.ContainsKey(3)) { $frames[3] } else { $null }

    $preload = [regex]::Matches($Text, 'PV7PERF preload-slice.*?us=(?<us>\d+)')
    [int64]$preloadTotal = 0
    [int64]$preloadMax = 0
    foreach ($m in $preload) {
        $value = [int64]$m.Groups['us'].Value
        $preloadTotal += $value
        if ($value -gt $preloadMax) { $preloadMax = $value }
    }

    return [pscustomobject]@{
        Timestamp = (Get-Date).ToString('o')
        ElapsedSec = [math]::Round($ElapsedSec, 2)
        StopReason = $StopReason
        MaxFrame = [int]$maxFrame
        MaxFrameSlice = if ($maxRow) { [int64]$maxRow.Slice } else { 0 }
        MaxFrameMainUs = if ($maxRow) { [int64]$maxRow.MainUs } else { 0 }
        MaxFrameWorkerUs = if ($maxRow) { [int64]$maxRow.WorkerUs } else { 0 }
        MaxFrameWorkerPumps = if ($maxRow) { [int64]$maxRow.WorkerPumps } else { 0 }
        Frame2Slice = if ($f2) { [int64]$f2.Slice } else { 0 }
        Frame2MainUs = if ($f2) { [int64]$f2.MainUs } else { 0 }
        Frame2WorkerUs = if ($f2) { [int64]$f2.WorkerUs } else { 0 }
        Frame2WorkerPumps = if ($f2) { [int64]$f2.WorkerPumps } else { 0 }
        Frame3Slice = if ($f3) { [int64]$f3.Slice } else { 0 }
        CondEvents = [regex]::Matches($Text, 'PV7COND').Count
        HotpatchLogs = [regex]::Matches($Text, 'CNR64HOTPATCH').Count
        PreferenceLogs = [regex]::Matches($Text, 'preference').Count
        SlowWorkers = [regex]::Matches($Text, 'slow-worker').Count
        PreloadSlices = $preload.Count
        PreloadTotalUs = $preloadTotal
        PreloadMaxUs = $preloadMax
        Fatals = [regex]::Matches($Text, 'Fatal signal|FATAL EXCEPTION|SIGSEGV|SIGABRT').Count
    }
}

function Compare-Metrics($Current, $Base) {
    if (-not $Base) { return 'NO BASELINE' }
    if ($Current.Fatals -gt 0 -and $Base.Fatals -eq 0) { return 'WORSE: fatal regression' }
    if ($Current.MaxFrame -gt $Base.MaxFrame) { return 'BETTER: reached a later frame' }
    if ($Current.MaxFrame -lt $Base.MaxFrame) { return 'WORSE: reached an earlier frame' }
    if ($Current.MaxFrameSlice -gt ($Base.MaxFrameSlice + 64)) { return 'BETTER: advanced farther' }
    if ($Current.MaxFrameSlice -lt ($Base.MaxFrameSlice - 64)) { return 'WORSE: advanced less far' }
    if ($Base.MaxFrameWorkerPumps -gt 0) {
        $pumpRatio = [double]$Current.MaxFrameWorkerPumps / [double]$Base.MaxFrameWorkerPumps
        $workerRatio = if ($Base.MaxFrameWorkerUs -gt 0) { [double]$Current.MaxFrameWorkerUs / [double]$Base.MaxFrameWorkerUs } else { 1.0 }
        if ($pumpRatio -le 0.90 -and $workerRatio -le 1.10) { return 'BETTER: same progress with less worker pumping' }
        if ($pumpRatio -ge 1.15 -and $Current.MaxFrameSlice -le $Base.MaxFrameSlice) { return 'WORSE: more worker pumping without more progress' }
    }
    return 'MIXED/NEUTRAL'
}

if ($SelfTest) {
    $sample = @'
I/CNR64POC: Unity nativeRender progress slice=192 pc=0x1 main_us=8000000 worker_us=100000 worker_pumps=300
I/CNR64POC: Unity nativeRenderFrame2 progress slice=256 pc=0x2 main_us=950000 worker_us=1200000 worker_pumps=6400
I/CNR64POC: PV7COND wait-enter thread=2 cond=0x01ba30ac
I/CNR64POC: PV7COND wait-wake thread=2 cond=0x01ba30ac
I/CNR64POC: PV7PERF preload-slice us=25000 slice=7 pc_before=0x1 pc_after=0x2
'@
    $m = Get-RunMetrics $sample 2.0 'selftest'
    if ($m.MaxFrame -ne 2 -or $m.Frame2Slice -ne 256 -or $m.Frame2WorkerPumps -ne 6400 -or
        $m.CondEvents -ne 2 -or $m.PreloadSlices -ne 1 -or $m.PreloadTotalUs -ne 25000 -or $m.Fatals -ne 0) {
        throw "pv7test self-test failed: $($m | ConvertTo-Json -Compress)"
    }
    $base = [pscustomobject]@{ Fatals=0; MaxFrame=2; MaxFrameSlice=128; MaxFrameWorkerPumps=8000; MaxFrameWorkerUs=1500000 }
    if ((Compare-Metrics $m $base) -notlike 'BETTER:*') { throw 'pv7test comparison self-test failed.' }
    Write-Host 'PV7TEST_SELFTEST_OK'
    exit 0
}

$script:root = $root
$script:package = $package
$script:remotePluginDir = $remotePluginDir
$script:adb = Get-AdbPath
$script:serial = Resolve-Serial $Serial $script:adb
Write-Host "[pv7test] device $script:serial"

Invoke-Adb @('shell','run-as',$package,'mkdir','-p',$remotePluginDir) | Out-Null
if ($CleanDevicePlugins) {
    Write-Host '[pv7test] clearing device hotpatch plugins'
    Remove-RemotePluginFiles { param($f) $f -like '*.so' }
}

$deployed = $null
if ($Plugin) {
    $deployed = Install-HotpatchPlugin $Plugin $PluginName
}

Invoke-Adb @('logcat','-c') | Out-Null
if (-not $NoRestart) {
    Invoke-Adb @('shell','am','force-stop',$package) | Out-Null
    Invoke-Adb @('shell','am','start','-n',$activity) | Out-Null
} else {
    Write-Host '[pv7test] keeping current app process alive'
}

$started = Get-Date
$lastAdvance = $started
$lastSignature = ''
$stopReason = 'timeout'
$text = ''
Write-Host "[pv7test] profiling up to ${TimeoutSec}s (stall ${StallSec}s)"

while ($true) {
    Start-Sleep -Milliseconds 750
    $text = Read-RunLog
    $progressMatches = [regex]::Matches($text, 'Unity nativeRender(?:Frame\d+)? progress slice=\d+')
    $signature = "$($progressMatches.Count):" + $(if ($progressMatches.Count -gt 0) { $progressMatches[$progressMatches.Count - 1].Value } else { '' })
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
    $elapsed = ($now - $started).TotalSeconds
    if ($elapsed -ge $TimeoutSec) { break }
    if ($progressMatches.Count -gt 0 -and ($now - $lastAdvance).TotalSeconds -ge $StallSec) {
        $stopReason = 'stall'
        break
    }
}

$elapsedSec = ((Get-Date) - $started).TotalSeconds
$text = Read-RunLog
$metrics = Get-RunMetrics $text $elapsedSec $stopReason
if ($deployed) {
    $metrics | Add-Member -NotePropertyName Plugin -NotePropertyValue $deployed.Name
    $metrics | Add-Member -NotePropertyName PluginFile -NotePropertyValue $deployed.File
} else {
    $metrics | Add-Member -NotePropertyName Plugin -NotePropertyValue 'none'
    $metrics | Add-Member -NotePropertyName PluginFile -NotePropertyValue ''
}
$metrics | Add-Member -NotePropertyName Serial -NotePropertyValue $script:serial

$runStamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir = Join-Path $runsDir $runStamp
New-Item -ItemType Directory -Force $runDir | Out-Null
$logPath = Join-Path $runDir 'logcat.txt'
$summaryPath = Join-Path $runDir 'summary.json'
[IO.File]::WriteAllText($logPath, $text, [Text.UTF8Encoding]::new($false))
$metrics | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 $summaryPath

$baselinePath = if ([IO.Path]::IsPathRooted($Baseline)) { $Baseline } else { Join-Path $root $Baseline }
$base = $null
if (Test-Path $baselinePath) {
    try { $base = Get-Content $baselinePath -Raw | ConvertFrom-Json } catch { $base = $null }
}
$verdict = Compare-Metrics $metrics $base

Write-Host ''
Write-Host '=== ProjectV7 rapid test ==='
Write-Host ("plugin          : {0}" -f $metrics.Plugin)
Write-Host ("stop            : {0} after {1}s" -f $metrics.StopReason, $metrics.ElapsedSec)
Write-Host ("progress        : frame {0}, slice {1}" -f $metrics.MaxFrame, $metrics.MaxFrameSlice)
Write-Host ("frame2          : slice {0}, main {1}us, worker {2}us, pumps {3}" -f $metrics.Frame2Slice, $metrics.Frame2MainUs, $metrics.Frame2WorkerUs, $metrics.Frame2WorkerPumps)
Write-Host ("cond/preload    : {0} cond events, {1} preload slices ({2}us)" -f $metrics.CondEvents, $metrics.PreloadSlices, $metrics.PreloadTotalUs)
Write-Host ("fatals          : {0}" -f $metrics.Fatals)
Write-Host ("comparison      : {0}" -f $verdict)
Write-Host ("artifacts       : {0}" -f $runDir)

if ($base) {
    Write-Host ("baseline        : frame {0}/slice {1}, worker pumps {2}, worker {3}us" -f $base.MaxFrame, $base.MaxFrameSlice, $base.MaxFrameWorkerPumps, $base.MaxFrameWorkerUs)
}

if ($SaveBaseline) {
    New-Item -ItemType Directory -Force (Split-Path $baselinePath -Parent) | Out-Null
    Copy-Item $summaryPath $baselinePath -Force
    Write-Host ("baseline saved  : {0}" -f $baselinePath)
}

$metrics
