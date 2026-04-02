# build_and_release.ps1 - interactive build + release pipeline for CNRMod DLLs.
#
# Interactive mode (no flags): prompts for mod selection, version bump, repo.json update, changelog.
# One-command mode (flags):    provide -ModName and optional flags to skip all prompts.
#
# Usage examples:
#   .\build_and_release.ps1
#   .\build_and_release.ps1 -ModName CNRMod
#   .\build_and_release.ps1 -ModName CNRMod -Bump patch -Changelog "Fixed a bug"
#   .\build_and_release.ps1 -ModName CNRMod -Bump minor -UpdateRepo -Changelog "New feature" -Deploy -Commit
#   .\build_and_release.ps1 -ModName CNRMod -NoBump -NoRepo -NoDeploy -NoCommit
#
# Flags:
#   -ModName <string>     Mod folder/file basename (e.g. CNRMod). Skips mod picker.
#   -Bump <string>        major | minor | patch | none  -- skips bump prompt.
#   -NoBump               Alias for -Bump none.
#   -UpdateRepo           Auto-answer yes to "update repo.json?" prompt.
#   -NoRepo               Auto-answer no  to "update repo.json?" prompt.
#   -Changelog <string>   Changelog text to insert; skips changelog prompt (implies -UpdateRepo).
#   -Deploy               Auto-answer yes to "push to device?" prompt.
#   -NoDeploy             Auto-answer no  to "push to device?" prompt.
#   -Device <string>      adb device serial (e.g. 192.168.1.5:5555).
#   -Commit               Auto-answer yes to "git commit+push?" prompt.
#   -NoCommit             Auto-answer no  to "git commit+push?" prompt.

param(
    [string]$ModName    = "",
    [string]$Bump       = "",      # major | minor | patch | none
    [switch]$NoBump,
    [switch]$UpdateRepo,
    [switch]$NoRepo,
    [string]$Changelog  = "",
    [switch]$Deploy,
    [switch]$NoDeploy,
    [string]$Device     = "",
    [switch]$Commit,
    [switch]$NoCommit
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Off

# -- Paths --------------------------------------------------------------------
$RootDir     = $PSScriptRoot
$BuildDir    = Join-Path $RootDir "APK_Build_Active"
$ModsDir     = Join-Path $RootDir "cnr-revived-web\mods"
$RepoJson    = Join-Path $ModsDir "repo.json"
$BuildScript = Join-Path $BuildDir "build_mod.ps1"
$BaseUrl     = "https://play.jacqueb.me/mods"

# -- Helpers ------------------------------------------------------------------
function Write-Step { param($msg) Write-Host "`n-- $msg" -ForegroundColor Cyan }
function Write-Ok   { param($msg) Write-Host "   $msg" -ForegroundColor Green }
function Write-Warn { param($msg) Write-Host "   WARN: $msg" -ForegroundColor Yellow }
function Write-Err  { param($msg) Write-Host "   ERROR: $msg" -ForegroundColor Red }

# -- 1. Discover mod CS files -------------------------------------------------
Write-Step "Discovering mods in cnr-revived-web/mods/"
$csFiles = Get-ChildItem -Path $ModsDir -Recurse -Filter "*.cs" | Sort-Object FullName

if ($csFiles.Count -eq 0) {
    Write-Err "No .cs files found under $ModsDir"
    exit 1
}

# -- 2. Select mod ------------------------------------------------------------
$selectedCs = $null

if ($ModName -ne "") {
    # Flag mode: match by basename
    $selectedCs = $csFiles | Where-Object { $_.BaseName -eq $ModName } | Select-Object -First 1
    if ($null -eq $selectedCs) {
        Write-Err "No .cs file found with basename '$ModName' under $ModsDir"
        Write-Host "   Available mods:"
        foreach ($f in $csFiles) { Write-Host "     $($f.BaseName)  ($($f.FullName))" }
        exit 1
    }
    Write-Ok "Mod: $($selectedCs.BaseName)  ($($selectedCs.FullName))"
} else {
    # Interactive mode: numbered menu
    Write-Host ""
    for ($i = 0; $i -lt $csFiles.Count; $i++) {
        $rel = $csFiles[$i].FullName.Substring($RootDir.Length).TrimStart('\')
        Write-Host "  [$($i+1)] $rel" -ForegroundColor White
    }
    Write-Host ""
    $pickInt = 0
    do {
        $pick = Read-Host "Select mod [1-$($csFiles.Count)]"
        $pickInt = 0
        $valid = [int]::TryParse($pick, [ref]$pickInt) -and $pickInt -ge 1 -and $pickInt -le $csFiles.Count
        if (-not $valid) { Write-Warn "Enter a number between 1 and $($csFiles.Count)." }
    } while (-not $valid)
    $selectedCs = $csFiles[$pickInt - 1]
    Write-Ok "Selected: $($selectedCs.BaseName)"
}

$ModBase  = $selectedCs.BaseName          # e.g. CNRMod
$ModCsDir = $selectedCs.DirectoryName    # e.g. .../mods/CNRMod
$OutDll   = Join-Path $ModCsDir "$ModBase.dll"

# -- 3. Read current version string from the .cs file -------------------------
Write-Step "Reading version from $ModBase.cs"

$csContent  = Get-Content $selectedCs.FullName -Raw
$verMatch   = [regex]::Match($csContent, '(?m)(?:public\s+const\s+string\s+Version\s*=\s*|Version\s*=\s*)"([0-9]+\.[0-9]+\.[0-9]+)"')
if (-not $verMatch.Success) {
    Write-Warn "Could not detect version string in .cs -- version bump will be skipped."
    $currentVer = "0.0.0"
    $noVersion  = $true
} else {
    $currentVer = $verMatch.Groups[1].Value
    $noVersion  = $false
    Write-Ok "Current version: $currentVer"
}

# -- 4. Version bump ----------------------------------------------------------
Write-Step "Version bump"

if ($NoBump) { $Bump = "none" }

$newVer = $currentVer

if ($Bump -eq "") {
    # Interactive
    Write-Host "  Current version: $currentVer"
    Write-Host "  [1] none   [2] patch   [3] minor   [4] major"
    do {
        $bpick = (Read-Host "  Bump [1-4]").Trim()
    } while ($bpick -notmatch '^[1-4]$')
    $Bump = @("none","patch","minor","major")[[int]$bpick - 1]
}

if ($Bump -ne "none" -and -not $noVersion) {
    $parts = $currentVer.Split('.')
    $major = [int]$parts[0]; $minor = [int]$parts[1]; $patch = [int]$parts[2]
    switch ($Bump.ToLower()) {
        "major" { $major++; $minor = 0; $patch = 0 }
        "minor" { $minor++; $patch = 0 }
        "patch" { $patch++ }
    }
    $newVer = "$major.$minor.$patch"
    Write-Ok "Version: $currentVer -> $newVer"

    # Rewrite version in .cs
    # Use ${1}/${2} (not $1/$2) so the group number is not ambiguous when
    # $newVer starts with a digit (e.g. '$1' + '2.0.1' would form '$12' = group 12).
    $verPattern     = '((?:public\s+const\s+string\s+Version\s*=\s*|Version\s*=\s*)")[0-9]+\.[0-9]+\.[0-9]+(")'
    $verReplacement = '${1}' + $newVer + '${2}'
    $csContent = $csContent -replace $verPattern, $verReplacement
    Set-Content -Path $selectedCs.FullName -Value $csContent -NoNewline
    Write-Ok "Updated version in $($selectedCs.Name)"
} else {
    Write-Ok "No bump (version stays $newVer)"
}

# -- 5. Compile ---------------------------------------------------------------
Write-Step "Compiling $ModBase.dll"

& $BuildScript -ModFile $selectedCs.FullName -OutName $ModBase -NoDeploy
if ($LASTEXITCODE -ne 0) {
    Write-Err "Build failed -- aborting."
    exit 1
}

$builtDll = Join-Path $BuildDir "bin\csc_build\$ModBase.dll"

# -- 6. Copy DLL to repo mods folder ------------------------------------------
Write-Step "Copying DLL to repo"

Copy-Item $builtDll $OutDll -Force
Write-Ok "-> $OutDll"

$versionedDll = Join-Path $ModCsDir "$ModBase-$newVer.dll"
Copy-Item $builtDll $versionedDll -Force
Write-Ok "-> $versionedDll"

# -- 7. Update repo.json ------------------------------------------------------
Write-Step "repo.json update"

$doRepo = $false
if ($Changelog -ne "") {
    $doRepo = $true   # providing a changelog implies repo update
} elseif ($UpdateRepo) {
    $doRepo = $true
} elseif ($NoRepo) {
    $doRepo = $false
} else {
    $ans = (Read-Host "  Update repo.json? [y/N]").Trim().ToLower()
    $doRepo = ($ans -eq "y" -or $ans -eq "yes")
}

if ($doRepo) {
    $repoCheck = Get-Content $RepoJson
    $modFound  = $repoCheck | Where-Object { $_ -match ('"id"\s*:\s*"' + [regex]::Escape($ModBase) + '"') }
    if (-not $modFound) {
        Write-Warn "Mod id '$ModBase' not found in repo.json -- skipping repo update."
        $doRepo = $false
    }
}

if ($doRepo) {
    if ($Changelog -eq "") {
        Write-Host "  Enter changelog for v$newVer (single line, press Enter when done):"
        $Changelog = Read-Host "  Changelog"
    }

    # Line-by-line surgery: update latestVersion and insert new version entry.
    $repoLines   = Get-Content $RepoJson
    $outLines    = [System.Collections.Generic.List[string]]::new()
    $inTargetMod = $false
    $vBumped     = $false
    $vInserted   = $false

    for ($li = 0; $li -lt $repoLines.Count; $li++) {
        $line = $repoLines[$li]

        # Enter this mod's block
        if (-not $inTargetMod -and $line -match ('"id"\s*:\s*"' + [regex]::Escape($ModBase) + '"')) {
            $inTargetMod = $true
        }

        # Bump latestVersion (first occurrence after entering the block)
        if ($inTargetMod -and -not $vBumped -and $line -match '"latestVersion"\s*:\s*"[^"]*"') {
            $line    = $line -replace '"latestVersion"\s*:\s*"[^"]*"', ('"latestVersion": "' + $newVer + '"')
            $vBumped = $true
        }

        # Insert new version entry at top of versions array
        if ($inTargetMod -and -not $vInserted -and $line -match '"versions"\s*:\s*\[') {
            $outLines.Add($line)
            $cl   = $Changelog -replace '\\', '\\\\' -replace '"', '\\"'
            $vUrl = $BaseUrl + '/' + $ModBase + '/' + $ModBase + '-' + $newVer + '.dll'
            $outLines.Add('        {')
            $outLines.Add('          "version": "' + $newVer + '",')
            $outLines.Add('          "url": "' + $vUrl + '",')
            $outLines.Add('          "changelog": "' + $cl + '"')
            $outLines.Add('        },')
            $vInserted = $true
            continue
        }

        # Leave block when next top-level object opens (next mod entry)
        if ($inTargetMod -and $vBumped -and $vInserted -and $line -match '^\s*\{\s*$') {
            $inTargetMod = $false
        }

        $outLines.Add($line)
    }

    if (-not $vBumped)   { Write-Warn "latestVersion not found in repo.json for $ModBase" }
    if (-not $vInserted) { Write-Warn "versions array not found in repo.json for $ModBase" }

    Set-Content -Path $RepoJson -Value $outLines
    Write-Ok "repo.json updated (id=$ModBase, version=$newVer)"
} else {
    Write-Warn "Skipping repo.json update."
}

# -- 8. Deploy to device ------------------------------------------------------
Write-Step "Deploy to device"

$doDeploy = $false
if ($NoDeploy) {
    $doDeploy = $false
} elseif ($Deploy) {
    $doDeploy = $true
} else {
    $ans = (Read-Host "  Push DLL to connected Android device? [y/N]").Trim().ToLower()
    $doDeploy = ($ans -eq "y" -or $ans -eq "yes")
}

if ($doDeploy) {
    $adbDevices = & adb devices 2>&1
    if (-not ($adbDevices | Where-Object { $_ -match "device$" })) {
        $wsa = "127.0.0.1:58526"
        Write-Warn "No adb device -- connecting to $wsa ..."
        & adb connect $wsa
        Start-Sleep -Seconds 1
    }
    $adbArgs = @("push", $builtDll, "/sdcard/CNRMods/$ModBase.dll")
    if ($Device -ne "") { $adbArgs = @("-s", $Device) + $adbArgs }
    & adb @adbArgs
    if ($LASTEXITCODE -ne 0) { Write-Err "adb push failed." }
    else { Write-Ok "Deployed to /sdcard/CNRMods/$ModBase.dll" }
} else {
    Write-Warn "Skipping device deploy."
}

# -- 9. Git commit + push -----------------------------------------------------
Write-Step "Git commit + push"

$doCommit = $false
if ($NoCommit) {
    $doCommit = $false
} elseif ($Commit) {
    $doCommit = $true
} else {
    $ans = (Read-Host "  git commit + push? [y/N]").Trim().ToLower()
    $doCommit = ($ans -eq "y" -or $ans -eq "yes")
}

if ($doCommit) {
    Push-Location (Join-Path $RootDir "cnr-revived-web")
    try {
        $relCs   = $selectedCs.FullName.Substring((Join-Path $RootDir "cnr-revived-web\").Length)
        $relDll  = "mods/$ModBase/$ModBase.dll"
        $relVDll = "mods/$ModBase/$ModBase-$newVer.dll"

        git add $relCs $relDll $relVDll "mods/repo.json" 2>&1 | Out-Null
        $commitMsg = "$ModBase $newVer"
        if ($Changelog -ne "") { $commitMsg += " -- $Changelog" }
        git commit -m $commitMsg
        git push
        Write-Ok "Pushed: $commitMsg"
    } finally {
        Pop-Location
    }
} else {
    Write-Warn "Skipping git commit."
}

Write-Step "Done"
Write-Ok "$ModBase $newVer complete."
