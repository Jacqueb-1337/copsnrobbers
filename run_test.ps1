param(
    [string]$Device = "localhost:58526",
    [string]$LanIp  = "172.28.48.1",
    [switch]$SkipServerStart
)

Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win32Focus {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool AllowSetForegroundWindow(int dwProcessId);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("kernel32.dll")] public static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
    [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
    public const int SW_RESTORE  = 9;
    public const int SW_MINIMIZE = 6;
    public static bool ForceSetForeground(IntPtr hWnd) {
        uint dummy;
        uint targetThread = GetWindowThreadProcessId(hWnd, out dummy);
        uint currentThread = GetCurrentThreadId();
        if (targetThread == currentThread) return SetForegroundWindow(hWnd);
        AttachThreadInput(currentThread, targetThread, true);
        bool result = BringWindowToTop(hWnd) && SetForegroundWindow(hWnd);
        AttachThreadInput(currentThread, targetThread, false);
        return result;
    }
}
"@

function Focus-GameWindow {
    $proc = Get-Process WsaClient -ErrorAction SilentlyContinue |
        Where-Object { $_.MainWindowTitle -eq "CopNRobber" }
    if ($proc) {
        [Win32Focus]::ShowWindow($proc.MainWindowHandle, [Win32Focus]::SW_RESTORE)
        Start-Sleep -Milliseconds 200
        $result = [Win32Focus]::ForceSetForeground($proc.MainWindowHandle)
        Start-Sleep -Milliseconds 400
        Write-Host "[Test] Game window focused (result=$result)"
        return $true
    }
    Write-Host "[Test] WARNING: CopNRobber window not found"
    return $false
}

$serverLog = "D:\Projects\copsnrobbers\server_run.log"

# Step 0: Start server
if (-not $SkipServerStart) {
    Write-Host "[Test] Stopping any existing server..."
    Get-Process -Name "LAN_Server_OptionB" -ErrorAction SilentlyContinue | Stop-Process -Force
    # Also kill anything holding ports 5055/5056
    @(5055, 5056) | ForEach-Object {
        $port = $_
        $pids = netstat -ano 2>$null | Select-String ":$port " | ForEach-Object {
            ($_ -split '\s+')[-1]
        } | Where-Object { $_ -match '^\d+$' } | Sort-Object -Unique
        $pids | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }
    }
    Start-Sleep -Milliseconds 800
    Write-Host "[Test] Building server..."
    dotnet build D:\Projects\copsnrobbers\LAN_Server_OptionB -c Release -v q 2>&1 | Select-Object -Last 3

    Write-Host "[Test] Opening firewall ports 5055/5056 (requires admin - skipping if not elevated)..."
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if ($isAdmin) {
        netsh advfirewall firewall delete rule name="CNR LAN 5055" 2>$null
        netsh advfirewall firewall delete rule name="CNR LAN 5056" 2>$null
        netsh advfirewall firewall add rule name="CNR LAN 5055" dir=in action=allow protocol=TCP localport=5055 | Out-Null
        netsh advfirewall firewall add rule name="CNR LAN 5056" dir=in action=allow protocol=TCP localport=5056 | Out-Null
        Write-Host "[Test] Firewall rules added."
    } else {
        Write-Host "[Test] Not admin - skipping firewall rules (run once as admin to add them)."
    }

    Write-Host "[Test] Starting server on $LanIp ..."
    $serverProc = Start-Process -FilePath "dotnet" `
        -ArgumentList "run --project D:\Projects\copsnrobbers\LAN_Server_OptionB --no-build -c Release -- $LanIp" `
        -PassThru -NoNewWindow -RedirectStandardOutput $serverLog
    Write-Host "[Test] Server PID: $($serverProc.Id)"
    Start-Sleep -Seconds 4
}

# Step 1: Stop game and clear logcat
Write-Host "[Test] Stopping game..."
adb -s $Device shell am force-stop com.joydo.minestrikenew
adb -s $Device logcat --clear 2>$null
Start-Sleep -Seconds 1

# Step 2: Launch game
Write-Host "[Test] Launching game..."
adb -s $Device shell monkey -p com.joydo.minestrikenew -c android.intent.category.LAUNCHER 1 2>$null
Start-Sleep -Seconds 8

# Step 3: Focus WSA window and tap Multiplayer
Write-Host "[Test] Tapping Multiplayer button..."
Focus-GameWindow
adb -s $Device shell input tap 695 490
Start-Sleep -Milliseconds 500
Focus-GameWindow   # re-focus after tap so Unity doesn't get paused

# Step 4: Minimize terminal so game has full focus for 40s (Android won't pause Unity)
Write-Host "[Test] Keeping game in foreground for 40s..."
$consoleHwnd = [Win32Focus]::GetConsoleWindow()
[Win32Focus]::ShowWindow($consoleHwnd, [Win32Focus]::SW_MINIMIZE) | Out-Null
# Active focus loop: re-focus game every 500ms so Android stays unpaused
$deadline = (Get-Date).AddSeconds(40)
while ((Get-Date) -lt $deadline) {
    $gp = Get-Process WsaClient -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -eq "CopNRobber" }
    if ($gp) {
        $fg = [Win32Focus]::GetForegroundWindow()
        if ($fg -ne $gp.MainWindowHandle) {
            [Win32Focus]::ForceSetForeground($gp.MainWindowHandle) | Out-Null
        }
    }
    Start-Sleep -Milliseconds 500
}
[Win32Focus]::ShowWindow($consoleHwnd, [Win32Focus]::SW_RESTORE) | Out-Null
[Win32Focus]::ShowWindow($consoleHwnd, [Win32Focus]::SW_RESTORE) | Out-Null
Write-Host "[Test] Terminal restored."

# Step 5: Collect logcat
Write-Host "[Test] Collecting logcat..."
$log = adb -s $Device logcat -d 2>$null
$relevant = $log | Select-String "IPRedirect|connState|detailState|SystemException|Photon|Exception|Error"
Write-Host ""
Write-Host "=== RELEVANT LOGCAT ==="
$relevant | ForEach-Object { Write-Host $_.Line }
Write-Host ""

# Step 6: Screenshot
adb -s $Device shell screencap -p /sdcard/test_result.png
adb -s $Device pull /sdcard/test_result.png D:\Projects\copsnrobbers\test_result.png 2>$null
Write-Host "[Test] Screenshot saved to test_result.png"

# Step 7: Server log
if (-not $SkipServerStart -and (Test-Path $serverLog)) {
    Write-Host ""
    Write-Host "=== SERVER LOG ==="
    Get-Content $serverLog
}

Write-Host "[Test] Done."
