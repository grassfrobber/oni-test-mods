$ErrorActionPreference = "Stop"
Set-StrictMode -Version 3.0

Push-Location $PSScriptRoot


## Config ##

$logPath = "$env:USERPROFILE\AppData\LocalLow\Klei\Oxygen Not Included"

$logFileNames = @("Player.log", "Player-prev.log")

$editor = "codium"  # VSCodium


## Open logs ##

$warn = $false

foreach ($logFileName in $logFileNames) {
    $fullPath = "$logPath\$logFileName"

    if (Test-Path $fullPath) {
        Start-Process $editor -ArgumentList "`"$fullPath`""
    } else {
        Write-Host "$logFileName not found in $logPath"
        $warn = $true
    }
}

if ($warn) {
    pause
}
