#!/bin/bash

# Script to create GitHub labels from settings.yml
# Requires GitHub CLI (gh) to be installed and authenticated
# Usage: ./scripts/create-labels.sh

set -e

echo "=============================================="
echo "GitHub Labels Creation Script"
echo "=============================================="
echo ""

# Check if gh is installed
if ! command -v gh &> /dev/null; then
    echo "❌ Error: GitHub CLI (gh) is not installed."
    echo ""
    echo "Please install GitHub CLI first:"
    echo "  - Windows: winget install --id GitHub.cli"
    echo "  - macOS: brew install gh"
    echo "  - Linux: See https://github.com/cli/cli/blob/trunk/docs/install_linux.md"
    echo ""
    exit 1
fi

# Check if authenticated
if ! gh auth status &> /dev/null; then
    echo "❌ Error: Not authenticated with GitHub CLI."
    echo ""
    echo "Please authenticate first:"
    echo "  gh auth login"
    echo ""
    exit 1
fi

echo "✅ GitHub CLI is installed and authenticated"
echo ""

# Function to create a label
create_label() {
    local name=$1
    local color=$2
    local description=$3
    
    echo -n "Creating label: '$name'... "
    
    if gh label create "$name" --color "$color" --description "$description" 2>/dev/null; then
        echo "✅ Created"
    else
        # Check if label already exists
        if gh label list --json name --jq '.[].name' | grep -q "^$name$"; then
            echo "⚠️  Already exists (skipped)"
        else
            echo "❌ Failed"
        fi
    fi
}

echo "Creating labels from settings.yml..."
echo "=============================================="
echo ""

echo "📌 Priority Labels:"
create_label "priority: critical" "d73a4a" "Critical priority - must be addressed immediately"
create_label "priority: high" "ff6b6b" "High priority - should be addressed soon"
create_label "priority: medium" "ffa500" "Medium priority - normal timeline"
create_label "priority: low" "90ee90" "Low priority - can be addressed when time permits"
echo ""

echo "📋 Type Labels:"
create_label "type: bug" "d73a4a" "Something isn't working"
create_label "type: feature" "0e8a16" "New feature or request"
create_label "type: enhancement" "a2eeef" "Improvement to existing functionality"
create_label "type: documentation" "0075ca" "Documentation changes or additions"
create_label "type: refactoring" "fbca04" "Code refactoring without changing functionality"
create_label "type: security" "d73a4a" "Security-related changes or fixes"
echo ""

echo "🔄 Status Labels:"
create_label "status: in progress" "fbca04" "Work is currently in progress"
create_label "status: blocked" "d73a4a" "Blocked by dependencies or other issues"
create_label "status: needs review" "0e8a16" "Ready for code review"
create_label "status: needs testing" "ffa500" "Needs testing before merge"
echo ""

echo "🧩 Component Labels:"
create_label "component: core" "bfdadc" "Core policy engine"
create_label "component: client" "bfdadc" "Client application"
create_label "component: security" "bfdadc" "Security module"
create_label "component: dashboard" "bfdadc" "Enterprise dashboard"
create_label "component: ci/cd" "bfdadc" "CI/CD and workflows"
echo ""

echo "📦 Dependency Labels:"
create_label "dependencies" "0366d6" "Dependency updates"
create_label "nuget" "0366d6" "NuGet package updates"
create_label "github-actions" "0366d6" "GitHub Actions updates"
echo ""

echo "⭐ Special Labels:"
create_label "good first issue" "7057ff" "Good for newcomers"
create_label "help wanted" "008672" "Extra attention is needed"
create_label "question" "d876e3" "Further information is requested"
create_label "wontfix" "ffffff" "This will not be worked on"
create_label "duplicate" "cfd3d7" "This issue or pull request already exists"
echo ""

echo "=============================================="
echo "✅ Label creation complete!"
echo ""
echo "To view all labels, run:"
echo "  gh label list"
echo ""
echo "Or visit:"
echo "  https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/labels"
echo "=============================================="
