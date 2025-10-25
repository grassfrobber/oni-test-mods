$ErrorActionPreference = "Stop"
Set-StrictMode -Version 3.0

Push-Location $PSScriptRoot


## Config ##

$projectName = "FirstMod"

# DLL edition to build. Can be "Debug" or "Release"
$buildEdition = "Debug"


## Standard stuff ##

$projectPath = "$projectName.csproj"

$buildPath = "bin\$buildEdition"

$dllFileName = "$projectName.dll"

$manifestFileNames = @("mod_info.yaml", "mod.yaml")

# Mod folder destination
$modFolder = "$env:USERPROFILE\Documents\Klei\OxygenNotIncluded\mods\Dev\$projectName"

# Steam URL to launch the game (trying to launch the exe directly will cause it to fail with
# error "Another instance is already running" due to a DRM check failure)
$oniSteamUrl = "steam://run/457140"


try {
    ## Build project ##

    Write-Host "Building project..."

    dotnet build $projectPath -c $buildEdition

    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    $buildOutput = Join-Path (Split-Path (Resolve-Path $projectPath) -Parent) $buildPath

    $dllFile = Get-ChildItem -Path $buildOutput -Recurse -Filter $dllFileName |
        Where-Object { $_.Name -notmatch "ref|deps" } |
        Select-Object -First 1

    if (-not $dllFile) {
        throw "Build succeeded but no DLL found!"
    }


    ## Copy files to mod folder ##

    Write-Host "Copying DLL to mod folder ($modFolder)..."

    if (!(Test-Path $modFolder)) {
        Write-Host "Creating mod folder"
        New-Item -ItemType Directory -Force -Path $modFolder | Out-Null
    }

    Copy-Item -Path $dllFile.FullName -Destination $modFolder -Force

    Write-Host "Copying manifest files to mod folder..."

    foreach ($manifestFileName in $manifestFileNames) {
        Copy-Item -Path $manifestFileName -Destination $modFolder -Force
    }


    ## Launch game ##

    Write-Host "Launching Oxygen Not Included..."

    Start-Process $oniSteamUrl

} catch {
    Write-Host $Error[0]
    pause
}
