$f = "d:\Projects\copsnrobbers\cnr-revived-web\mods\CNRMod\CNRMod.cs"
$b64raw = Get-Content "d:\Projects\copsnrobbers\dl_b64_new.txt" -Raw -Encoding ASCII
$chunkSize = 100
$chunks = [System.Collections.Generic.List[string]]::new()
for ($i = 0; $i -lt $b64raw.Length; $i += $chunkSize) {
    $chunks.Add($b64raw.Substring($i, [Math]::Min($chunkSize, $b64raw.Length - $i)))
}
$newConst = [System.Collections.Generic.List[string]]::new()
$newConst.Add("        private static readonly string _DlIconB64 =")
for ($i = 0; $i -lt $chunks.Count - 1; $i++) { $newConst.Add("            `"$($chunks[$i])`" +") }
$newConst.Add("            `"$($chunks[-1])`";")

$lines = [System.IO.File]::ReadAllLines($f, [System.Text.Encoding]::UTF8)
$start = -1; $end = -1
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i].TrimStart().StartsWith("private static readonly string _DlIconB64")) { $start = $i }
    if ($start -ge 0 -and $i -gt $start -and $lines[$i].TrimEnd().EndsWith('";')) { $end = $i; break }
}
Write-Host "start=$start end=$end old=$($end-$start+1) new=$($newConst.Count)"

$out = [System.Collections.Generic.List[string]]::new()
for ($i = 0; $i -lt $start; $i++) { $out.Add($lines[$i]) }
foreach ($l in $newConst) { $out.Add($l) }
for ($i = $end + 1; $i -lt $lines.Count; $i++) { $out.Add($lines[$i]) }

[System.IO.File]::WriteAllLines($f, $out, [System.Text.Encoding]::UTF8)
Write-Host "Done. Total lines: $($out.Count)"
