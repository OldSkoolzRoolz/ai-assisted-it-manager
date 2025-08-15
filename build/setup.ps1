param(
  [string]$SolutionName = "EnterprisePolicyManager"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Push-Location (Split-Path -Parent $MyInvocation.MyCommand.Path)
Push-Location ..

if (-not (Test-Path "$SolutionName.sln")) {
  dotnet new sln -n $SolutionName | Out-Null
}

$projects = @(
  "src/Shared/Shared.csproj",
  "src/Security/Security.csproj",
  "src/AILayer/AILayer.csproj",
  "src/CorePolicyEngine/CorePolicyEngine.csproj",
  "src/EnterpriseDashboard/EnterpriseDashboard.csproj",
  "src/ClientApp/ClientApp.csproj",
  "tests/Shared.Tests/Shared.Tests.csproj",
  "tests/CorePolicyEngine.Tests/CorePolicyEngine.Tests.csproj"
)

foreach ($p in $projects) {
  dotnet sln $SolutionName.sln add $p | Out-Null
}

Write-Host "Solution ready: $SolutionName.sln"
Pop-Location
Pop-Location