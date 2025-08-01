param(
    [Parameter(Mandatory=$false)]
    [switch]$Help,

    [Parameter(Mandatory=$false)]
    [ValidateSet("windows", "linux", "all")]
    [string]$Target,

    [Parameter(Mandatory=$false)]
    [string]$Godot,

    [Parameter(Mandatory=$false)]
    [int]$ExportTimeout = 15
)

$SEPARATOR = "─" * ([console]::WindowWidth)
$PREFERRED_GODOT_EXE = "Godot_v4.4.1-stable_mono_win64_console.exe"

# Process user input
$wantHelp = $Help -or (-not $Target)

if ($wantHelp) {
    Write-Host "🛠️ EmuNX Export Tool"
    Write-Host ""
    Write-Host "Usage:"
    Write-Host "  .\export.ps1 -Help -Target <platform> [-Godot <path_to_godot_mono_cli_exe>]"
    Write-Host ""
    Write-Host "Available flags: (only use one)"
    Write-Host "  -Help   - Shows this message"
    Write-Host "  -Target - Compiles to specified platform"
    Write-Host "  -Godot  - Path to the executable used to compile the project."
    Write-Host "            If not set, the script will use the value of the environment variable `"`$env:EMUNX_GODOT4NET`"."
    Write-Host "            Anyways, one of them must point to `"$PREFERRED_GODOT_EXE`"."
    Write-Host ""
    Write-Host "Platforms"
    Write-Host "  all     - Compiles to every platform below"
    Write-Host "  windows - Compiles to `"Desktop Windows x86_64`""
    Write-Host "  linux   - Compiles to `"Desktop Linux x86_64`""
    exit
}

# Set Godot executable
$pathGodotExe = $Godot

if (-not $pathGodotExe) {
    $pathGodotExe = $env:EMUNX_GODOT4NET
}

Write-Host "❔ Validating the following godot exe: `"$pathGodotExe`""
if ((Test-Path $pathGodotExe) -and $pathGodotExe.ToLower().EndsWith(".exe")) {
    Write-Host "✅ The compiler is valid!"
}
else {
    Write-Host "❌ Is not valid. Please use `"-Help`" flag for more information"
    exit 1
}

# Generate paths
$pathEmuNX = $PSScriptRoot

function Get-EmuNX-Path {
    param (
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    return Join-Path -Path $pathEmuNX -ChildPath $RelativePath
}

$pathGodotProject = Get-EmuNX-Path "src\EmuNX.Frontend.Godot"

if (Test-Path $pathGodotProject) {
    Write-Host "✅ Godot project was found in `"$pathGodotProject`"!"
}
else {
    Write-Host "❌ Godot project was not found in `"$pathGodotProject`"!"
    exit 1
}

$pathEmuNXSln = Get-EmuNX-Path "EmuNX.sln"
$pathGodotSln = Get-EmuNX-Path "EmuNX.Frontend.Godot.sln"


# Functions
function Ensure-Directory {
    param (
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    # Combine the script's directory with the relative path
    $fullPath = Get-EmuNX-Path $RelativePath
    Write-Host "📁 Ensuring `"$fullPath`" existence"

    New-Item -ItemType Directory -Path $fullPath -Force | Out-Null

    if (-not (Test-Path $fullPath)) {
        Write-Host "❌ Couldn't create the path from above."
        exit 1
    }
}

function Export-GodotProject {
    param (
        [string]$Preset
    )


    Write-Host ($SEPARATOR)
    Write-Host "🚀 Exporting with preset `"$Preset`"..."

    # & $pathGodotExe --headless --verbose --export-release "$Preset" --path "$pathGodotProject" --quit

    # Start compilation as async process
    # 1. Sometimes Godot can compile succesfully but not quit
    # 2. Because this is async we can force the quitting after X seconds
    $proc = Start-Process `
        -FilePath $pathGodotExe `
        -ArgumentList "--headless", "--export-release", `"$Preset`", "--path", `"$pathGodotProject`" `
        -NoNewWindow -PassThru

    if ($ExportTimeout -gt 0) {
        Start-Sleep -Seconds $ExportTimeout

        if (-not $proc.HasExited) {
            Write-Host "⚠️ Exporting did not finish in time. This does not necessarily mean that the process failed (sometimes Godot exports but does not quit)."
            $proc.Kill()
        }
    }
    else {
        $proc.WaitForExit()
    }

    Write-Host "🛑 Exporting with preset `"$Preset`" has ended!"
    Write-Host ($SEPARATOR)
}

# Compile setup
Copy-Item -Path $pathEmuNXSln -Destination $pathGodotSln

# Compile process
$tgt = $Target.ToLower()

$compileToWindows = ($tgt -eq "all") -or ($tgt -eq "windows")
$compileToLinux = ($tgt -eq "all") -or ($tgt -eq "linux")

if ($compileToWindows) {
    Ensure-Directory -RelativePath "target/windows/x86_64"
    Export-GodotProject -Preset "Desktop Windows x86_64"
}

if ($compileToLinux) {
    Ensure-Directory -RelativePath "target/linux/x86_64"
    Export-GodotProject -Preset "Desktop Linux x86_64"
}

# Compile cleaning
Remove-Item -Path $pathGodotSln