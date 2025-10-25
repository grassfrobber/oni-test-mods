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


function main() {
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
            if ($manifestFileName -eq "mod_info.yaml") {
                $content = getVersionedModInfoYamlContent $manifestFileName
            } else {
                $content = Get-Content $manifestFileName -Raw
            }

            $destFile = Join-Path $modFolder $manifestFileName

            $content | Set-Content $destFile
        }


        ## Launch game ##

        Write-Host "Launching Oxygen Not Included..."

        Start-Process $oniSteamUrl

    } catch {
        Write-Host $Error[0]
        pause
    }
}

function getVersionedModInfoYamlContent($manifestFileName) {
    $srcCode = "HelloWorld.cs"

    # Find a line in the source that defines a static string variable called version,
    # and extract the version number
    $versionLine = Get-Content $srcCode | Where-Object {
        $_ -match 'static\s.*version\s*=\s*"([^"]+)"'
    }

    if ($versionLine) {
        $version = $matches[1]
        Write-Host "Found version: $version"
    } else {
        throw "Version not found in $srcCode"
    }

    $yamlContent = Get-Content $manifestFileName -Raw

    # In a multi-line search, replace the YAML's "version: #.#.#" line
    # with "version: <versionFromCsFile>"
    $yamlContent = $yamlContent -replace '(?m)^version:\s*[0-9\.]+', "version: $version"

    Write-Host "Rewrote $manifestFileName with:`n$yamlContent"

    return $yamlContent
}

main
