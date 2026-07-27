[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $SourceExe,
    [string] $SourceServiceExe,
    [string] $SourceServiceSettings
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path -LiteralPath $SourceExe)) { throw "Agent EXE was not found: $SourceExe" }
$installDirectory = Join-Path $env:LOCALAPPDATA "AppsGo\ErpBridge Agent"
$targetExe = Join-Path $installDirectory "ErpBridge.Agent.UI.exe"
New-Item -ItemType Directory -Path $installDirectory -Force | Out-Null
Copy-Item -LiteralPath $SourceExe -Destination $targetExe -Force

if (-not [string]::IsNullOrWhiteSpace($SourceServiceExe)) {
    if (-not (Test-Path -LiteralPath $SourceServiceExe)) {
        throw "Agent service EXE was not found: $SourceServiceExe"
    }
    $serviceExe = Join-Path $installDirectory "ErpBridge.Agent.Service.exe"
    $serviceSettings = Join-Path $installDirectory "appsettings.json"
    $existing = Get-Service -Name "ErpBridgeAgent" -ErrorAction SilentlyContinue
    if ($null -ne $existing) {
        if ($existing.Status -ne "Stopped") {
            Stop-Service -Name "ErpBridgeAgent" -Force
            $existing.WaitForStatus("Stopped", [TimeSpan]::FromSeconds(20))
        }
    }
    Copy-Item -LiteralPath $SourceServiceExe -Destination $serviceExe -Force
    if (-not [string]::IsNullOrWhiteSpace($SourceServiceSettings)) {
        Copy-Item -LiteralPath $SourceServiceSettings -Destination $serviceSettings -Force
    }
    if ($null -eq $existing) {
        New-Service -Name "ErpBridgeAgent" `
            -BinaryPathName ('"{0}"' -f $serviceExe) `
            -DisplayName "ErpBridge Canlı Senkronizasyon" `
            -Description "Mikro ERP değişikliklerini lisans sunucusuna canlı olarak aktarır." `
            -StartupType Automatic | Out-Null
    } else {
        Set-Service -Name "ErpBridgeAgent" -StartupType Automatic
    }
    Start-Service -Name "ErpBridgeAgent"
}

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut((Join-Path ([Environment]::GetFolderPath("Desktop")) "ErpBridge Agent.lnk"))
$shortcut.TargetPath = $targetExe
$shortcut.WorkingDirectory = $installDirectory
$shortcut.IconLocation = "$targetExe,0"
$shortcut.Save()
Write-Host "Installed. Use the single desktop shortcut: ErpBridge Agent"
