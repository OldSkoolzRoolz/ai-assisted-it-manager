# Script to create GitHub labels from settings.yml
# Requires GitHub CLI (gh) to be installed and authenticated
# Usage: .\scripts\Create-Labels.ps1

$ErrorActionPreference = "Stop"

Write-Host "=============================================="
Write-Host "GitHub Labels Creation Script" -ForegroundColor Cyan
Write-Host "=============================================="
Write-Host ""

# Check if gh is installed
try {
    $null = Get-Command gh -ErrorAction Stop
    Write-Host "✅ GitHub CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: GitHub CLI (gh) is not installed." -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install GitHub CLI first:" -ForegroundColor Yellow
    Write-Host "  winget install --id GitHub.cli"
    Write-Host ""
    Write-Host "Or download from: https://cli.github.com/"
    Write-Host ""
    exit 1
}

# Check if authenticated
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error: Not authenticated with GitHub CLI." -ForegroundColor Red
    Write-Host ""
    Write-Host "Please authenticate first:" -ForegroundColor Yellow
    Write-Host "  gh auth login"
    Write-Host ""
    exit 1
}

Write-Host "✅ GitHub CLI is authenticated" -ForegroundColor Green
Write-Host ""

# Function to create a label
function New-GithubLabel {
    param(
        [string]$Name,
        [string]$Color,
        [string]$Description
    )
    
    Write-Host "Creating label: '$Name'... " -NoNewline
    
    $output = gh label create $Name --color $Color --description $Description 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Created" -ForegroundColor Green
    } else {
        # Check if label already exists
        $existingLabels = gh label list --json name --jq '.[].name' 2>$null
        if ($existingLabels -match "^$Name$") {
            Write-Host "⚠️  Already exists (skipped)" -ForegroundColor Yellow
        } else {
            Write-Host "❌ Failed: $output" -ForegroundColor Red
        }
    }
}

Write-Host "Creating labels from settings.yml..."
Write-Host "=============================================="
Write-Host ""

Write-Host "📌 Priority Labels:" -ForegroundColor Cyan
New-GithubLabel -Name "priority: critical" -Color "d73a4a" -Description "Critical priority - must be addressed immediately"
New-GithubLabel -Name "priority: high" -Color "ff6b6b" -Description "High priority - should be addressed soon"
New-GithubLabel -Name "priority: medium" -Color "ffa500" -Description "Medium priority - normal timeline"
New-GithubLabel -Name "priority: low" -Color "90ee90" -Description "Low priority - can be addressed when time permits"
Write-Host ""

Write-Host "📋 Type Labels:" -ForegroundColor Cyan
New-GithubLabel -Name "type: bug" -Color "d73a4a" -Description "Something isn't working"
New-GithubLabel -Name "type: feature" -Color "0e8a16" -Description "New feature or request"
New-GithubLabel -Name "type: enhancement" -Color "a2eeef" -Description "Improvement to existing functionality"
New-GithubLabel -Name "type: documentation" -Color "0075ca" -Description "Documentation changes or additions"
New-GithubLabel -Name "type: refactoring" -Color "fbca04" -Description "Code refactoring without changing functionality"
New-GithubLabel -Name "type: security" -Color "d73a4a" -Description "Security-related changes or fixes"
Write-Host ""

Write-Host "🔄 Status Labels:" -ForegroundColor Cyan
New-GithubLabel -Name "status: in progress" -Color "fbca04" -Description "Work is currently in progress"
New-GithubLabel -Name "status: blocked" -Color "d73a4a" -Description "Blocked by dependencies or other issues"
New-GithubLabel -Name "status: needs review" -Color "0e8a16" -Description "Ready for code review"
New-GithubLabel -Name "status: needs testing" -Color "ffa500" -Description "Needs testing before merge"
Write-Host ""

Write-Host "🧩 Component Labels:" -ForegroundColor Cyan
New-GithubLabel -Name "component: core" -Color "bfdadc" -Description "Core policy engine"
New-GithubLabel -Name "component: client" -Color "bfdadc" -Description "Client application"
New-GithubLabel -Name "component: security" -Color "bfdadc" -Description "Security module"
New-GithubLabel -Name "component: dashboard" -Color "bfdadc" -Description "Enterprise dashboard"
New-GithubLabel -Name "component: ci/cd" -Color "bfdadc" -Description "CI/CD and workflows"
Write-Host ""

Write-Host "📦 Dependency Labels:" -ForegroundColor Cyan
New-GithubLabel -Name "dependencies" -Color "0366d6" -Description "Dependency updates"
New-GithubLabel -Name "nuget" -Color "0366d6" -Description "NuGet package updates"
New-GithubLabel -Name "github-actions" -Color "0366d6" -Description "GitHub Actions updates"
Write-Host ""

Write-Host "⭐ Special Labels:" -ForegroundColor Cyan
New-GithubLabel -Name "good first issue" -Color "7057ff" -Description "Good for newcomers"
New-GithubLabel -Name "help wanted" -Color "008672" -Description "Extra attention is needed"
New-GithubLabel -Name "question" -Color "d876e3" -Description "Further information is requested"
New-GithubLabel -Name "wontfix" -Color "ffffff" -Description "This will not be worked on"
New-GithubLabel -Name "duplicate" -Color "cfd3d7" -Description "This issue or pull request already exists"
Write-Host ""

Write-Host "=============================================="
Write-Host "✅ Label creation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "To view all labels, run:" -ForegroundColor Cyan
Write-Host "  gh label list"
Write-Host ""
Write-Host "Or visit:" -ForegroundColor Cyan
Write-Host "  https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/labels"
Write-Host "=============================================="
