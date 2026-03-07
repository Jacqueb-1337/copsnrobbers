# Starts a local HTTP server for the map builder.
# Requires Python (already available in this project).
$port = 8765
Write-Host "Map Builder running at http://localhost:$port" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop." -ForegroundColor Gray
Push-Location $PSScriptRoot
python -m http.server $port
Pop-Location
