# build.ps1 — convenience wrapper around APK_Build_Active\build_mod.ps1
# Usage:
#   .\build.ps1 settings      # CNRSettingsMod
#   .\build.ps1 mod           # CNRMod
#   .\build.ps1 manager       # CNRModManager
#   .\build.ps1 all           # all three
#   .\build.ps1 settings -NoDeploy
#   .\build.ps1 mod -Device 192.168.1.5:5555

param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet("settings","mod","manager","all")]
    [string]$Target,

    [string]$Device   = "",
    [switch]$NoDeploy
)

$Build = "d:\Projects\copsnrobbers\APK_Build_Active\build_mod.ps1"
$Mods  = "d:\Projects\copsnrobbers\cnr-revived-web\mods"

$map = @{
    settings = "$Mods\CNRSettingsMod\CNRSettingsMod.cs"
    mod      = "$Mods\CNRMod\CNRMod.cs"
    manager  = "$Mods\CNRModManager\CNRModManager.cs"
}

$targets = if ($Target -eq "all") { "settings","mod","manager" } else { @($Target) }

foreach ($t in $targets) {
    $buildArgs = @{ ModFile = $map[$t] }
    if ($Device   -ne "")  { $buildArgs["Device"]   = $Device }
    if ($NoDeploy)          { $buildArgs["NoDeploy"] = $true   }
    & $Build @buildArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
