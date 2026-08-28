@echo off
setlocal
if "%~1"=="" (
  echo Usage: pv7run plugins\job_signal_prefer.cpp [plugin-name]
  exit /b 2
)
set "PLUGIN=%~1"
set "NAME=%~2"
if "%NAME%"=="" set "NAME=%~n1"

powershell.exe -NoProfile -File "%~dp0pv7compile.ps1" -Plugin "%PLUGIN%" -PluginName "%NAME%"
if errorlevel 1 exit /b %errorlevel%
powershell.exe -NoProfile -File "%~dp0pv7deploy.ps1" -Clean
if errorlevel 1 exit /b %errorlevel%
powershell.exe -NoProfile -File "%~dp0pv7capture.ps1"
if errorlevel 1 exit /b %errorlevel%
powershell.exe -NoProfile -File "%~dp0pv7test.ps1"
exit /b %errorlevel%
