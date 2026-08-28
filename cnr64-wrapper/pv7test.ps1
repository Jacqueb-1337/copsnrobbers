param(
    [string]$LogPath,
    [string]$PluginName,
    [string]$CaptureMeta = 'devtools/pv7test-last-run.json',
    [switch]$SaveBaseline,
    [switch]$SelfTest,
    [string]$Baseline = 'devtools/pv7test-baseline.json'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Get-RunMetrics([string]$Text, [double]$ElapsedSec, [string]$StopReason) {
    $progress = [regex]::Matches($Text, 'Unity nativeRender(?<frame>Frame\d+)? progress slice=(?<slice>\d+).*?main_us=(?<main>\d+) worker_us=(?<worker>\d+) worker_pumps=(?<pumps>\d+)')
    $frames = @{}
    foreach ($match in $progress) {
        $frameName = if ($match.Groups['frame'].Success) { $match.Groups['frame'].Value } else { 'Frame1' }
        $frameNumber = if ($frameName -eq 'Frame1') { 1 } else { [int]($frameName -replace 'Frame','') }
        $row = [pscustomobject]@{
            Frame = $frameNumber
            Slice = [int64]$match.Groups['slice'].Value
            MainUs = [int64]$match.Groups['main'].Value
            WorkerUs = [int64]$match.Groups['worker'].Value
            WorkerPumps = [int64]$match.Groups['pumps'].Value
        }
        if (-not $frames.ContainsKey($frameNumber) -or $row.Slice -ge $frames[$frameNumber].Slice) {
            $frames[$frameNumber] = $row
        }
    }

    $maxFrame = 0
    if ($frames.Count -gt 0) { $maxFrame = [int](($frames.Keys | Measure-Object -Maximum).Maximum) }
    $maxRow = if ($maxFrame -gt 0) { $frames[[int]$maxFrame] } else { $null }
    $frame2 = if ($frames.ContainsKey(2)) { $frames[2] } else { $null }
    $frame3 = if ($frames.ContainsKey(3)) { $frames[3] } else { $null }

    $preload = [regex]::Matches($Text, 'PV7PERF preload-slice.*?us=(?<us>\d+)')
    [int64]$preloadTotal = 0
    [int64]$preloadMax = 0
    foreach ($match in $preload) {
        $value = [int64]$match.Groups['us'].Value
        $preloadTotal += $value
        if ($value -gt $preloadMax) { $preloadMax = $value }
    }

    [pscustomobject]@{
        Timestamp = (Get-Date).ToString('o')
        ElapsedSec = [math]::Round($ElapsedSec, 2)
        StopReason = $StopReason
        MaxFrame = [int]$maxFrame
        MaxFrameSlice = if ($maxRow) { [int64]$maxRow.Slice } else { 0 }
        MaxFrameMainUs = if ($maxRow) { [int64]$maxRow.MainUs } else { 0 }
        MaxFrameWorkerUs = if ($maxRow) { [int64]$maxRow.WorkerUs } else { 0 }
        MaxFrameWorkerPumps = if ($maxRow) { [int64]$maxRow.WorkerPumps } else { 0 }
        Frame2Slice = if ($frame2) { [int64]$frame2.Slice } else { 0 }
        Frame2MainUs = if ($frame2) { [int64]$frame2.MainUs } else { 0 }
        Frame2WorkerUs = if ($frame2) { [int64]$frame2.WorkerUs } else { 0 }
        Frame2WorkerPumps = if ($frame2) { [int64]$frame2.WorkerPumps } else { 0 }
        Frame3Slice = if ($frame3) { [int64]$frame3.Slice } else { 0 }
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
    $metrics = Get-RunMetrics $sample 2.0 'selftest'
    if ($metrics.MaxFrame -ne 2 -or $metrics.MaxFrameSlice -ne 256 -or $metrics.Frame2Slice -ne 256 -or
        $metrics.Frame2WorkerPumps -ne 6400 -or $metrics.CondEvents -ne 2 -or $metrics.PreloadSlices -ne 1 -or
        $metrics.PreloadTotalUs -ne 25000 -or $metrics.Fatals -ne 0) {
        throw "pv7test self-test failed: $($metrics | ConvertTo-Json -Compress)"
    }
    $base = [pscustomobject]@{ Fatals=0; MaxFrame=1; MaxFrameSlice=192; MaxFrameWorkerPumps=8000; MaxFrameWorkerUs=1500000 }
    if ((Compare-Metrics $metrics $base) -notlike 'BETTER:*') { throw 'pv7test comparison self-test failed.' }
    Write-Host 'PV7TEST_SELFTEST_OK'
    exit 0
}

$captureMetaPath = if ([IO.Path]::IsPathRooted($CaptureMeta)) { $CaptureMeta } else { Join-Path $root $CaptureMeta }
$capture = $null
if (Test-Path $captureMetaPath) {
    try { $capture = Get-Content $captureMetaPath -Raw | ConvertFrom-Json } catch { $capture = $null }
}

if (-not $LogPath) {
    if (-not $capture -or -not $capture.LogPath) { throw 'No LogPath supplied and no capture handoff is available.' }
    $LogPath = $capture.LogPath
}
if (-not [IO.Path]::IsPathRooted($LogPath)) { $LogPath = Join-Path $root $LogPath }
$resolvedLog = (Resolve-Path $LogPath).Path
$text = Get-Content $resolvedLog -Raw

$elapsedSec = if ($capture -and $capture.ElapsedSec) { [double]$capture.ElapsedSec } else { 0.0 }
$stopReason = if ($capture -and $capture.StopReason) { [string]$capture.StopReason } else { 'external-capture' }
$metrics = Get-RunMetrics $text $elapsedSec $stopReason

if (-not $PluginName) {
    $deployPath = Join-Path $root 'devtools\pv7test-last-deploy.json'
    if (Test-Path $deployPath) {
        try {
            $deploy = Get-Content $deployPath -Raw | ConvertFrom-Json
            $PluginName = $deploy.Name
        } catch { }
    }
}
if (-not $PluginName) { $PluginName = 'none' }
$metrics | Add-Member -NotePropertyName Plugin -NotePropertyValue $PluginName
$metrics | Add-Member -NotePropertyName LogPath -NotePropertyValue $resolvedLog

$baselinePath = if ([IO.Path]::IsPathRooted($Baseline)) { $Baseline } else { Join-Path $root $Baseline }
$base = $null
if (Test-Path $baselinePath) {
    try { $base = Get-Content $baselinePath -Raw | ConvertFrom-Json } catch { $base = $null }
}
$verdict = Compare-Metrics $metrics $base

$summaryPath = Join-Path (Split-Path $resolvedLog -Parent) 'summary.json'
$metrics | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 $summaryPath

Write-Host ''
Write-Host '=== ProjectV7 rapid test ==='
Write-Host ("plugin          : {0}" -f $metrics.Plugin)
Write-Host ("stop            : {0} after {1}s" -f $metrics.StopReason, $metrics.ElapsedSec)
Write-Host ("progress        : frame {0}, slice {1}" -f $metrics.MaxFrame, $metrics.MaxFrameSlice)
Write-Host ("frame2          : slice {0}, main {1}us, worker {2}us, pumps {3}" -f $metrics.Frame2Slice, $metrics.Frame2MainUs, $metrics.Frame2WorkerUs, $metrics.Frame2WorkerPumps)
Write-Host ("cond/preload    : {0} cond events, {1} preload slices ({2}us)" -f $metrics.CondEvents, $metrics.PreloadSlices, $metrics.PreloadTotalUs)
Write-Host ("fatals          : {0}" -f $metrics.Fatals)
Write-Host ("comparison      : {0}" -f $verdict)
Write-Host ("summary         : {0}" -f $summaryPath)
if ($base) {
    Write-Host ("baseline        : frame {0}/slice {1}, worker pumps {2}, worker {3}us" -f $base.MaxFrame, $base.MaxFrameSlice, $base.MaxFrameWorkerPumps, $base.MaxFrameWorkerUs)
}

if ($SaveBaseline) {
    New-Item -ItemType Directory -Force (Split-Path $baselinePath -Parent) | Out-Null
    Copy-Item $summaryPath $baselinePath -Force
    Write-Host ("baseline saved  : {0}" -f $baselinePath)
}

$metrics
