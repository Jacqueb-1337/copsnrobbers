$f = "d:\Projects\copsnrobbers\cnr-revived-web\mods\CNRMod\CNRMod.cs"
$lines = [System.IO.File]::ReadAllLines($f)
Write-Host "Original lines: $($lines.Count)"

# All indices are 0-based. Work BOTTOM-UP so inserts above don't shift later operations.

# === CHANGE C: insert yield after catch line (index 1801), before foreach close (index 1802) ===
$yieldLine = '                    if (++_p1i % 20 == 0) yield return null;'
if ($lines[1801] -notmatch 'catch.*cloneEx') { Write-Error "C: wrong line 1802: $($lines[1801])"; exit 1 }
$lines = $lines[0..1801] + @($yieldLine) + $lines[1802..($lines.Count-1)]
Write-Host "After C: $($lines.Count)"

# === CHANGE B: replace indices 1704-1714 (the if(donor==null) per-item FindObjectsOfType block) ===
if ($lines[1704] -notmatch 'if \(donor == null\)') { Write-Error "B: wrong line 1705: $($lines[1704])"; exit 1 }
if ($lines[1714] -notmatch '^\s*\}$') { Write-Error "B end: wrong line 1715: $($lines[1714])"; exit 1 }
$replB = @(
    '                    if (donor == null)',
    '                    {',
    '                        string leafName = obj.path.Contains("/")',
    '                            ? obj.path.Substring(obj.path.LastIndexOf(''/'')+1)',
    '                            : obj.path;',
    '                        goByName.TryGetValue(leafName, out donor);',
    '                    }'
)
$lines = $lines[0..1703] + $replB + $lines[1715..($lines.Count-1)]
Write-Host "After B: $($lines.Count)"

# === CHANGE A: insert pre-build dict after index 1679 (after clonedPaths declaration) ===
if ($lines[1679] -notmatch 'clonedPaths.*HashSet') { Write-Error "A: wrong line 1680: $($lines[1679])"; exit 1 }
$dictLines = @(
    '',
    '                // Pre-build name->GO lookup once so Pass 1 fallback is O(1) per item.',
    '                // Without this, every miss called FindObjectsOfType per item: O(n*scene) -> ANR on WSA.',
    '                var goByName = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);',
    '                foreach (GameObject _go in (GameObject[])FindObjectsOfType(typeof(GameObject)))',
    '                {',
    '                    if (_go != null && !string.IsNullOrEmpty(_go.name) && !goByName.ContainsKey(_go.name))',
    '                        goByName[_go.name] = _go;',
    '                }',
    '                int _p1i = 0;'
)
$lines = $lines[0..1679] + $dictLines + $lines[1680..($lines.Count-1)]
Write-Host "After A: $($lines.Count)"

[System.IO.File]::WriteAllLines($f, $lines)
Write-Host "SAVED OK"
