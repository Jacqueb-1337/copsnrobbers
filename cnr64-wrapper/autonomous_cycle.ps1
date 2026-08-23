$ErrorActionPreference = 'Stop'
Set-Location 'D:\Projects\copsnrobbers\cnr64-wrapper'
$adb = Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe'
Remove-Item 'autocycle.done','autocycle.status','autocycle.error.txt','autocycle-build.txt','autocycle-adb.txt','autocycle-install.txt','autocycle-launch.txt','autocycle-logcat.txt' -ErrorAction SilentlyContinue
try {
    & .\build_poc_apk.ps1 *>&1 | Out-File -Encoding utf8 autocycle-build.txt
    if ($LASTEXITCODE -ne 0) { throw "build failed with exit $LASTEXITCODE" }
    & $adb connect localhost:58526 *>&1 | Out-File -Encoding utf8 autocycle-adb.txt
    & $adb -s localhost:58526 install -r .\dist\CNR64-Arm64-Dynarmic-PoC.apk *>&1 | Out-File -Encoding utf8 autocycle-install.txt
    if ($LASTEXITCODE -ne 0) { throw "install failed with exit $LASTEXITCODE" }
    & $adb -s localhost:58526 logcat -c
    & $adb -s localhost:58526 shell am force-stop me.jacqueb.cnr64poc
    & $adb -s localhost:58526 shell monkey -p me.jacqueb.cnr64poc -c android.intent.category.LAUNCHER 1 *>&1 | Out-File -Encoding utf8 autocycle-launch.txt
    Start-Sleep -Seconds 20
    & $adb -s localhost:58526 logcat -d -v threadtime *>&1 | Out-File -Encoding utf8 autocycle-logcat.txt
    '0' | Set-Content autocycle.status
} catch {
    ($_ | Out-String) | Set-Content autocycle.error.txt
    '1' | Set-Content autocycle.status
}
'done' | Set-Content autocycle.done
