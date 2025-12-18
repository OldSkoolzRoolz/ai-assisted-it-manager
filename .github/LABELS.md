# Repository Labels Guide

## Overview

This document explains the label system used in the AI-Assisted IT Manager repository, where labels are defined, and how to apply them to GitHub.

## Where Are Labels Located?

Repository labels are **defined** in `.github/settings.yml` (lines 126-240). However, these labels must be **manually created** in the GitHub repository to be available for use in issues and pull requests.

## Label Categories

### Priority Labels
Used to indicate the urgency and importance of an issue:

| Label | Color | Description |
|-------|-------|-------------|
| `priority: critical` | ![#d73a4a](https://via.placeholder.com/15/d73a4a/000000?text=+) `#d73a4a` | Critical priority - must be addressed immediately |
| `priority: high` | ![#ff6b6b](https://via.placeholder.com/15/ff6b6b/000000?text=+) `#ff6b6b` | High priority - should be addressed soon |
| `priority: medium` | ![#ffa500](https://via.placeholder.com/15/ffa500/000000?text=+) `#ffa500` | Medium priority - normal timeline |
| `priority: low` | ![#90ee90](https://via.placeholder.com/15/90ee90/000000?text=+) `#90ee90` | Low priority - can be addressed when time permits |

### Type Labels
Used to categorize the type of issue or change:

| Label | Color | Description |
|-------|-------|-------------|
| `type: bug` | ![#d73a4a](https://via.placeholder.com/15/d73a4a/000000?text=+) `#d73a4a` | Something isn't working |
| `type: feature` | ![#0e8a16](https://via.placeholder.com/15/0e8a16/000000?text=+) `#0e8a16` | New feature or request |
| `type: enhancement` | ![#a2eeef](https://via.placeholder.com/15/a2eeef/000000?text=+) `#a2eeef` | Improvement to existing functionality |
| `type: documentation` | ![#0075ca](https://via.placeholder.com/15/0075ca/000000?text=+) `#0075ca` | Documentation changes or additions |
| `type: refactoring` | ![#fbca04](https://via.placeholder.com/15/fbca04/000000?text=+) `#fbca04` | Code refactoring without changing functionality |
| `type: security` | ![#d73a4a](https://via.placeholder.com/15/d73a4a/000000?text=+) `#d73a4a` | Security-related changes or fixes |

### Status Labels
Used to track the current state of an issue or PR:

| Label | Color | Description |
|-------|-------|-------------|
| `status: in progress` | ![#fbca04](https://via.placeholder.com/15/fbca04/000000?text=+) `#fbca04` | Work is currently in progress |
| `status: blocked` | ![#d73a4a](https://via.placeholder.com/15/d73a4a/000000?text=+) `#d73a4a` | Blocked by dependencies or other issues |
| `status: needs review` | ![#0e8a16](https://via.placeholder.com/15/0e8a16/000000?text=+) `#0e8a16` | Ready for code review |
| `status: needs testing` | ![#ffa500](https://via.placeholder.com/15/ffa500/000000?text=+) `#ffa500` | Needs testing before merge |

### Component Labels
Used to identify which part of the codebase is affected:

| Label | Color | Description |
|-------|-------|-------------|
| `component: core` | ![#bfdadc](https://via.placeholder.com/15/bfdadc/000000?text=+) `#bfdadc` | Core policy engine |
| `component: client` | ![#bfdadc](https://via.placeholder.com/15/bfdadc/000000?text=+) `#bfdadc` | Client application |
| `component: security` | ![#bfdadc](https://via.placeholder.com/15/bfdadc/000000?text=+) `#bfdadc` | Security module |
| `component: dashboard` | ![#bfdadc](https://via.placeholder.com/15/bfdadc/000000?text=+) `#bfdadc` | Enterprise dashboard |
| `component: ci/cd` | ![#bfdadc](https://via.placeholder.com/15/bfdadc/000000?text=+) `#bfdadc` | CI/CD and workflows |

### Dependency Labels
Used for dependency-related issues and PRs:

| Label | Color | Description |
|-------|-------|-------------|
| `dependencies` | ![#0366d6](https://via.placeholder.com/15/0366d6/000000?text=+) `#0366d6` | Dependency updates |
| `nuget` | ![#0366d6](https://via.placeholder.com/15/0366d6/000000?text=+) `#0366d6` | NuGet package updates |
| `github-actions` | ![#0366d6](https://via.placeholder.com/15/0366d6/000000?text=+) `#0366d6` | GitHub Actions updates |

### Special Labels
Used for specific purposes:

| Label | Color | Description |
|-------|-------|-------------|
| `good first issue` | ![#7057ff](https://via.placeholder.com/15/7057ff/000000?text=+) `#7057ff` | Good for newcomers |
| `help wanted` | ![#008672](https://via.placeholder.com/15/008672/000000?text=+) `#008672` | Extra attention is needed |
| `question` | ![#d876e3](https://via.placeholder.com/15/d876e3/000000?text=+) `#d876e3` | Further information is requested |
| `wontfix` | ![#ffffff](https://via.placeholder.com/15/ffffff/000000?text=+) `#ffffff` | This will not be worked on |
| `duplicate` | ![#cfd3d7](https://via.placeholder.com/15/cfd3d7/000000?text=+) `#cfd3d7` | This issue or pull request already exists |

## How to Apply Labels to GitHub

Labels defined in `settings.yml` are **not automatically created** in GitHub. They must be manually applied using one of the following methods:

### Method 1: Manual Creation via GitHub UI (Recommended for Initial Setup)

1. Navigate to the repository on GitHub
2. Go to **Issues** → **Labels** (or visit `https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/labels`)
3. For each label in `settings.yml`:
   - Click **New label**
   - Enter the **Label name** (e.g., `priority: critical`)
   - Enter the **Color** code (e.g., `d73a4a`)
   - Enter the **Description**
   - Click **Create label**

### Method 2: Using GitHub CLI (Fastest for Bulk Creation)

If you have [GitHub CLI](https://cli.github.com/) installed:

```bash
# Navigate to repository directory
cd /path/to/ai-assisted-it-manager

# Create labels from settings.yml
# Priority labels
gh label create "priority: critical" --color "d73a4a" --description "Critical priority - must be addressed immediately"
gh label create "priority: high" --color "ff6b6b" --description "High priority - should be addressed soon"
gh label create "priority: medium" --color "ffa500" --description "Medium priority - normal timeline"
gh label create "priority: low" --color "90ee90" --description "Low priority - can be addressed when time permits"

# Type labels
gh label create "type: bug" --color "d73a4a" --description "Something isn't working"
gh label create "type: feature" --color "0e8a16" --description "New feature or request"
gh label create "type: enhancement" --color "a2eeef" --description "Improvement to existing functionality"
gh label create "type: documentation" --color "0075ca" --description "Documentation changes or additions"
gh label create "type: refactoring" --color "fbca04" --description "Code refactoring without changing functionality"
gh label create "type: security" --color "d73a4a" --description "Security-related changes or fixes"

# Status labels
gh label create "status: in progress" --color "fbca04" --description "Work is currently in progress"
gh label create "status: blocked" --color "d73a4a" --description "Blocked by dependencies or other issues"
gh label create "status: needs review" --color "0e8a16" --description "Ready for code review"
gh label create "status: needs testing" --color "ffa500" --description "Needs testing before merge"

# Component labels
gh label create "component: core" --color "bfdadc" --description "Core policy engine"
gh label create "component: client" --color "bfdadc" --description "Client application"
gh label create "component: security" --color "bfdadc" --description "Security module"
gh label create "component: dashboard" --color "bfdadc" --description "Enterprise dashboard"
gh label create "component: ci/cd" --color "bfdadc" --description "CI/CD and workflows"

# Dependency labels
gh label create "dependencies" --color "0366d6" --description "Dependency updates"
gh label create "nuget" --color "0366d6" --description "NuGet package updates"
gh label create "github-actions" --color "0366d6" --description "GitHub Actions updates"

# Special labels
gh label create "good first issue" --color "7057ff" --description "Good for newcomers"
gh label create "help wanted" --color "008672" --description "Extra attention is needed"
gh label create "question" --color "d876e3" --description "Further information is requested"
gh label create "wontfix" --color "ffffff" --description "This will not be worked on"
gh label create "duplicate" --color "cfd3d7" --description "This issue or pull request already exists"
```

**Note**: If a label already exists, GitHub CLI will report an error but continue with the next label.

### Method 3: Using GitHub API

For automated setup or CI/CD integration, you can use the GitHub REST API:

```bash
# Set your GitHub token
GITHUB_TOKEN="your_token_here"
REPO="OldSkoolzRoolz/ai-assisted-it-manager"

# Example: Create a single label
curl -X POST \
  -H "Authorization: token $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  "https://api.github.com/repos/$REPO/labels" \
  -d '{"name":"priority: critical","color":"d73a4a","description":"Critical priority - must be addressed immediately"}'
```

### Method 4: Using Label Sync Tools

Consider using automated tools for managing labels:

- **[github-label-sync](https://github.com/Financial-Times/github-label-sync)**: Sync labels from a JSON configuration file
- **[Probot Settings](https://probot.github.io/apps/settings/)**: GitHub App that manages repository settings including labels
- **[Label Actions](https://github.com/dessant/label-actions)**: GitHub Action for managing labels

## Using Labels

### When Creating Issues

Issue templates automatically apply labels:
- Bug reports: `type: bug`, `status: needs review`
- Feature requests: `type: feature`, `status: needs review`
- Documentation issues: `type: documentation`, `status: needs review`

You can add additional labels manually:
1. Open or create an issue
2. In the right sidebar, click **Labels**
3. Select appropriate labels (priority, component, etc.)

### When Creating Pull Requests

Apply labels to PRs to indicate:
- **Type**: What kind of change (bug, feature, etc.)
- **Component**: Which part of the codebase is affected
- **Status**: Current state of the PR

The PR validation workflow may also suggest labels based on file changes.

## Label Best Practices

### For Issue Authors
1. **Always apply a type label** (`type: bug`, `type: feature`, etc.)
2. **Add priority** if urgent (`priority: critical`, `priority: high`)
3. **Specify component** if known (`component: core`, `component: client`, etc.)
4. **Update status** as work progresses

### For Maintainers
1. **Review and adjust labels** on new issues
2. **Add component labels** based on affected code
3. **Set priority** based on impact and urgency
4. **Update status** as issues move through workflow
5. **Use `good first issue`** for newcomer-friendly tasks

### Label Combinations

Common combinations:
- `type: bug` + `priority: critical` + `component: security` = Critical security bug
- `type: feature` + `component: core` + `status: in progress` = Core feature being developed
- `type: documentation` + `good first issue` = Good starting point for new contributors

## Troubleshooting

### Labels Not Available When Creating Issues

**Problem**: Issue templates reference labels that don't exist in the repository.

**Solution**: 
1. Check if labels exist: Visit `https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/labels`
2. If labels are missing, create them using one of the methods above
3. Labels must match exactly (including spaces and colons) as defined in `settings.yml`

### Labels Not Auto-Applied from Templates

**Problem**: Creating an issue from a template doesn't automatically apply labels.

**Possible Causes**:
1. Labels don't exist in the repository (must be created first)
2. Label names in template don't match existing labels (case-sensitive)
3. GitHub may have synchronization delays (refresh the page)

**Solution**:
1. Ensure all labels from `settings.yml` are created in GitHub
2. Verify label names match exactly
3. Apply labels manually if auto-application fails

## Maintenance

### Adding New Labels

When adding new labels:
1. Update `.github/settings.yml` with the new label definition
2. Create the label in GitHub using one of the methods above
3. Update this document (`LABELS.md`) with the new label
4. Document the purpose and usage of the new label

### Renaming Labels

GitHub does not support label renaming via templates. To rename:
1. Create the new label in GitHub
2. Manually apply new label to all issues/PRs with old label
3. Delete the old label
4. Update `settings.yml` and documentation

### Removing Labels

When deprecating labels:
1. Remove from `settings.yml`
2. Remove from this documentation
3. Manually remove from all issues/PRs
4. Delete from GitHub (cannot be undone)

## References

- [GitHub Labels Documentation](https://docs.github.com/en/issues/using-labels-and-milestones-to-track-work/managing-labels)
- [GitHub CLI Labels Commands](https://cli.github.com/manual/gh_label)
- [GitHub REST API - Labels](https://docs.github.com/en/rest/issues/labels)
- Repository Settings: `.github/settings.yml`

## Support

If you have questions about labels:
1. Check this guide first
2. Review existing issues with similar labels
3. Ask in [GitHub Discussions](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/discussions)
4. Contact maintainers: @KyleC69, @OldSkoolzRoolz

---

**Maintained By**: @KyleC69, @OldSkoolzRoolz  
**Last Updated**: 2025-12-18
