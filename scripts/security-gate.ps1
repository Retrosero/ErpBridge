[CmdletBinding()]
param(
    [switch] $SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repositoryRoot
try {
    function Invoke-GateCommand {
        param([string] $Name, [scriptblock] $Command)

        Write-Host "[security-gate] $Name"
        & $Command
        if ($LASTEXITCODE -ne 0) {
            throw "Security gate failed: $Name"
        }
    }

    # These signatures identify committed credential material, not ordinary
    # configuration keys or test fixtures. More exhaustive secret scanning is
    # intentionally delegated to the CI secret-scanning provider.
    $secretPatterns = @(
        '-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',
        '"private_key"\s*:\s*"-----BEGIN',
        'AKIA[0-9A-Z]{16}',
        'AIza[0-9A-Za-z_-]{35}'
    )
    $trackedFiles = @(git ls-files)
    $secretMatches = $trackedFiles |
        Where-Object { $_ -notmatch '(^|/)(bin|obj)/' } |
        ForEach-Object {
            Select-String -LiteralPath $_ -Pattern $secretPatterns -AllMatches -ErrorAction SilentlyContinue
        }
    if ($secretMatches) {
        $secretMatches | ForEach-Object { Write-Error "Potential secret: $($_.Path):$($_.LineNumber)" }
        throw 'Tracked source contains material that matches a credential signature.'
    }
    Write-Host '[security-gate] secret signature scan passed'

    Invoke-GateCommand 'locked restore' { dotnet restore ErpBridge.sln --locked-mode --verbosity minimal }
    Write-Host '[security-gate] known vulnerable packages'
    $vulnerabilityReport = (& dotnet list ErpBridge.sln package --vulnerable --include-transitive | Out-String)
    $vulnerabilityReport.TrimEnd() | Write-Host
    if ($LASTEXITCODE -ne 0 -or $vulnerabilityReport -match 'has the following vulnerable packages|güvenlik açığı olan paketlere sahip') {
        throw 'Security gate failed: known vulnerable packages'
    }
    Invoke-GateCommand 'build' { dotnet build ErpBridge.sln --no-restore --verbosity minimal }
    if (-not $SkipTests) {
        Invoke-GateCommand 'unit tests' { dotnet test ErpBridge.sln --no-restore --logger 'console;verbosity=minimal' }
    }

    Write-Host '[security-gate] passed'
}
finally {
    Pop-Location
}
