[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Version,
    [Parameter(Mandatory = $true)] [string] $DownloadBaseUrl,
    [Parameter(Mandatory = $true)] [string] $SigningCertificateThumbprint,
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\ErpBridge.Agent.UI\ErpBridge.Agent.UI.csproj"
$releaseRoot = Join-Path $repoRoot "release\windows-agent"
$publishRoot = Join-Path $releaseRoot "publish"
$packageName = "ErpBridge.Agent.UI-$Version.exe"
$packagePath = Join-Path $releaseRoot $packageName

Remove-Item -LiteralPath $publishRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $publishRoot -Force | Out-Null
dotnet publish $project -c $Configuration -r win-x64 --self-contained true -p:Version=$Version -o $publishRoot

$agent = Join-Path $publishRoot "ErpBridge.Agent.UI.exe"
if (-not (Test-Path -LiteralPath $agent)) { throw "Single-file agent output was not produced." }
Copy-Item -LiteralPath $agent -Destination $packagePath -Force

$certificate = Get-ChildItem "Cert:\CurrentUser\My\$SigningCertificateThumbprint" -ErrorAction Stop
if (-not $certificate.HasPrivateKey) { throw "The signing certificate has no private key." }
$downloadUrl = "$($DownloadBaseUrl.TrimEnd('/'))/$packageName"
$sha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash
$canonical = "$Version`n$downloadUrl`n$sha256"
$signatureBytes = $certificate.GetRSAPrivateKey().SignData([Text.Encoding]::UTF8.GetBytes($canonical), [Security.Cryptography.HashAlgorithmName]::SHA256, [Security.Cryptography.RSASignaturePadding]::Pkcs1)
$manifest = [ordered]@{
    version = $Version
    downloadUrl = $downloadUrl
    sha256 = $sha256
    signature = [Convert]::ToBase64String($signatureBytes)
}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $releaseRoot "agent-update.json") -Encoding utf8

Write-Host "Release ready: $packagePath"
Write-Host "Upload the EXE and agent-update.json to $DownloadBaseUrl"
