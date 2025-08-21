# Workspace Preset Script

Write-Host "Applying workspace presets..."

$excludePaths = @("bin", "obj", ".vs", ".git", "packages")
foreach ($path in $excludePaths) {
    Add-MpPreference -ExclusionPath "$PSScriptRoot\$path"
}

# Symbolic link for markdown visibility
$linkPath = "$PSScriptRoot\src\ClientApp\OnboardingDocs"
$docsPath = "$PSScriptRoot\docs"
New-Item -ItemType SymbolicLink -Path $linkPath -Target $docsPath -Force

Start-Process "devenv.exe" "$PSScriptRoot\ai-assisted-it-manager.sln"