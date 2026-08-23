$adb = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"
& $adb -s localhost:58526 shell am start -n me.jacqueb.cnr64poc/.MainActivity
