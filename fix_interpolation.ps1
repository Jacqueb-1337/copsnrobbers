$file = "D:\Projects\copsnrobbers\APK_Build_MainMenuHook\CNRIPRedirectMod.cs"
$content = Get-Content $file -Raw

$result = [System.Text.RegularExpressions.Regex]::Replace(
    $content,
    '\$"((?:[^"\\]|\\.)*)"',
    [System.Text.RegularExpressions.MatchEvaluator]{
        param($m)
        $inner = $m.Groups[1].Value
        $parts = [System.Collections.Generic.List[string]]::new()
        $cur = [System.Text.StringBuilder]::new()
        $i = 0
        while ($i -lt $inner.Length) {
            if ($inner[$i] -eq '{') {
                if ($cur.Length -gt 0) { $parts.Add('"' + $cur.ToString() + '"'); $cur.Clear() | Out-Null }
                $i++
                $expr = [System.Text.StringBuilder]::new()
                $fmt  = $null
                $depth = 1
                while ($i -lt $inner.Length -and $depth -gt 0) {
                    if ($inner[$i] -eq '{') { $depth++ }
                    elseif ($inner[$i] -eq '}') {
                        $depth--
                        if ($depth -eq 0) { $i++; break }
                    }
                    # Detect format specifier: colon at depth 1 (not inside nested braces)
                    elseif ($inner[$i] -eq ':' -and $depth -eq 1 -and $fmt -eq $null) {
                        $fmt = [System.Text.StringBuilder]::new()
                        $i++
                        continue
                    }
                    if ($fmt -ne $null) { $fmt.Append($inner[$i]) | Out-Null }
                    else                { $expr.Append($inner[$i]) | Out-Null }
                    $i++
                }
                $exprStr = $expr.ToString()
                if ($fmt -ne $null) {
                    $parts.Add("(" + $exprStr + ").ToString(`"" + $fmt.ToString() + "`")")
                } else {
                    $parts.Add("(" + $exprStr + ")")
                }
            } else {
                $cur.Append($inner[$i]) | Out-Null
                $i++
            }
        }
        if ($cur.Length -gt 0) { $parts.Add('"' + $cur.ToString() + '"') }
        if ($parts.Count -eq 0) { return '""' }
        if ($parts.Count -eq 1) { return $parts[0] }
        return [string]::Join(" + ", $parts)
    }
)

[System.IO.File]::WriteAllText($file, $result, [System.Text.Encoding]::UTF8)
$remaining = (Select-String -Path $file -Pattern '\$"' | Measure-Object).Count
Write-Host "Done. Remaining dollar-strings: $remaining"
