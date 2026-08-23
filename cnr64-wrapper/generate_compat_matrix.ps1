$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$nmCandidates = @(
    (Join-Path $env:LOCALAPPDATA 'Android\Sdk\ndk\23.1.7779620\toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-nm.exe'),
    (Join-Path $env:LOCALAPPDATA 'Android\Sdk\ndk\28.2.13676358\toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-nm.exe')
)
$nm = $nmCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $nm) { throw 'llvm-nm.exe not found in the known Android NDK locations.' }

$projectRoot = Split-Path $root -Parent
$libMap = [ordered]@{
    'libmain.so'  = Join-Path $projectRoot 'APK_Build_Active\apk_source\lib\armeabi-v7a\libmain.so'
    'libunity.so' = Join-Path $projectRoot 'APK_Build_Active\apk_source\lib\armeabi-v7a\libunity.so'
    'libmono.so'  = Join-Path $projectRoot 'APK_Build_Active\apk_source\lib\armeabi-v7a\libmono.so'
}

$exports = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($path in $libMap.Values) {
    & $nm -D --defined-only $path | ForEach-Object {
        $parts = ($_ -split '\s+')
        if ($parts.Count -ge 3) { [void]$exports.Add($parts[-1]) }
    }
}

$bridgeSource = (Get-Content 'src\shared_guest_linker.cpp' -Raw) + "`n" +
                (Get-Content 'src\gles_bridge_methods.inc' -Raw) + "`n" +
                (Get-Content 'src\compat_bridge_methods.inc' -Raw)
$dataSymbols = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
@('__data_start','data_start','environ','__sF','_ctype_','_toupper_tab_','__page_size','__stack_chk_guard') |
    ForEach-Object { [void]$dataSymbols.Add($_) }

$section = ''
$imports = @()
foreach ($line in Get-Content 'abi_imports.txt') {
    $text = $line.Trim()
    if ($text -match '^===\s+(.+?)\s+===$') { $section = $matches[1]; continue }
    if (-not $text -or $text.StartsWith('#')) { continue }
    $imports += [pscustomobject]@{ Library = $section; Symbol = $text }
}

$rows = foreach ($group in ($imports | Group-Object Symbol | Sort-Object Name)) {
    $symbol = $group.Name
    $libraries = (($group.Group.Library | Sort-Object -Unique) -join ',')
    $status = if ($exports.Contains($symbol)) {
        'GUEST_EXPORT'
    } elseif ($dataSymbols.Contains($symbol)) {
        'DATA_SLOT'
    } elseif ($bridgeSource -match ('"' + [regex]::Escape($symbol) + '"')) {
        'HOST_BRIDGED'
    } else {
        'MISSING'
    }
    [pscustomobject]@{ Symbol = $symbol; Libraries = $libraries; Status = $status }
}

$rows | Export-Csv -Delimiter "`t" -NoTypeInformation -Path 'compat_matrix.tsv'
$summary = $rows | Group-Object Status | Sort-Object Name | ForEach-Object { "{0}={1}" -f $_.Name, $_.Count }
$missing = $rows | Where-Object Status -eq 'MISSING' | Select-Object -ExpandProperty Symbol
@(
    "generated=$([DateTime]::Now.ToString('s'))",
    "unique_imports=$($rows.Count)",
    "guest_exports=$($exports.Count)"
) + $summary + @('--- missing ---') + $missing | Set-Content 'compat_matrix_summary.txt'

Get-Content 'compat_matrix_summary.txt'
