# GitHub Configuration

This directory contains GitHub-specific configuration files for the AI-Assisted IT Manager repository.

## Directory Structure

```
.github/
├── ISSUE_TEMPLATE/          # Issue templates for bug reports, features, etc.
│   ├── bug_report.md        # Template for bug reports
│   ├── feature_request.md   # Template for feature requests
│   ├── documentation.md     # Template for documentation issues
│   └── config.yml           # Issue template configuration
├── workflows/               # GitHub Actions workflows
│   ├── codeql.yml           # CodeQL security analysis
│   ├── doc-version-validation.yml  # Documentation validation
│   ├── dotnet-ci.yml        # .NET CI build and test
│   ├── dotnet.yml           # .NET build workflow
│   ├── pr-validation.yml    # Pull request validation (NEW)
│   └── secret-scanning.yml  # Secret scanning workflow
├── CODEOWNERS               # Code ownership definitions
├── PULL_REQUEST_TEMPLATE.md # Template for pull requests
├── SECURITY.md              # Security policy and vulnerability reporting
├── copilot-instructions.md  # GitHub Copilot configuration
├── dependabot.yml           # Dependabot configuration
└── settings.yml             # Repository settings and branch protection (NEW)
```

## Files Overview

### settings.yml (NEW)
Documents the recommended repository settings including:
- Branch protection rules for `master` and `feature/*` branches
- Required status checks
- Pull request review requirements
- Repository labels for issues and PRs
- Collaborator permissions

**Note**: This file documents the desired settings but must be applied manually via GitHub UI or API.

### workflows/pr-validation.yml (NEW)
Automated validation workflow that runs on pull requests to:
- Validate PR title and description format
- Check branch naming conventions
- Detect breaking changes
- Monitor PR size and complexity
- Identify affected components
- Auto-suggest labels based on changed files

### PULL_REQUEST_TEMPLATE.md (NEW)
Template used when creating pull requests, including:
- Description and related issues
- Type of change checkboxes
- Testing checklist
- Documentation requirements
- Security and performance considerations

### ISSUE_TEMPLATE/
Contains templates for different types of issues:

#### bug_report.md (NEW)
Template for reporting bugs with sections for:
- Bug description
- Steps to reproduce
- Expected vs actual behavior
- Environment details
- Logs and error messages

#### feature_request.md (NEW)
Template for requesting new features with sections for:
- Feature description
- Problem statement
- Proposed solution
- Use cases and priority

#### documentation.md (NEW)
Template for documentation issues with sections for:
- Affected documentation
- Current vs desired state
- Proposed changes

#### config.yml (NEW)
Configures issue template options and contact links:
- Discussion forum link
- Security advisory link
- Documentation link

## Branch Protection Rules

The repository uses branch protection rules documented in `settings.yml`:

### Master Branch (Release-Ready)
**The master branch is treated as release-ready at all times.**
- **Required reviews**: 1 approval from code owners (@KyleC69 or @OldSkoolzRoolz) - **MANDATORY**
- **Required status checks**: build-and-test, CodeQL, doc-validation
- **Additional rules**: Dismiss stale reviews, require conversation resolution, enforce for admins
- **Restrictions**: No force pushes, no deletions

### Feature Branches
- **Required reviews**: 1 approval (code owner not required)
- **Required status checks**: build-and-test
- **Additional rules**: More flexible for rapid development
- **Restrictions**: Force pushes allowed, deletions allowed

## Status Checks

All pull requests must pass these automated checks before merging:

### build-and-test
- Workflow: `dotnet-ci.yml`
- Purpose: Build solution and run tests
- Configuration: Release mode with warnings as errors

### Analyze Code
- Workflow: `codeql.yml`
- Purpose: Security vulnerability scanning
- Configuration: Security and quality queries

### validate-doc-versions
- Workflow: `doc-version-validation.yml`
- Purpose: Validate documentation version manifest
- Configuration: Checks for outdated docs

### pr-validation (NEW)
- Workflow: `pr-validation.yml`
- Purpose: Validate PR format and quality
- Configuration: Checks title, description, size, and affected components

## CODEOWNERS

Defines code ownership for different parts of the repository:
- **Global owners**: @KyleC69, @OldSkoolzRoolz
- **Source directories**: Require review from owners
- **GitHub configuration**: Requires owner review for security
- **Documentation**: Special attention for manifest changes

## Workflows

### Continuous Integration
- **dotnet-ci.yml**: Main CI workflow (build + test on push/PR)
- **dotnet.yml**: Secondary build workflow
- **pr-validation.yml**: PR quality checks (NEW)

### Security
- **codeql.yml**: CodeQL security analysis (weekly + on PR)
- **secret-scanning.yml**: Scan for leaked secrets

### Documentation
- **doc-version-validation.yml**: Validate documentation versions

## Applying Settings

### Via GitHub UI
1. Go to repository **Settings**
2. Navigate to specific sections:
   - **Branches** → Configure branch protection rules
   - **Labels** → Add/modify issue labels
   - **Collaborators** → Manage access
3. Apply settings from `settings.yml`

### Via GitHub API
Use GitHub CLI or API to apply settings programmatically:

```bash
# Example: Apply branch protection
gh api repos/OldSkoolzRoolz/ai-assisted-it-manager/branches/master/protection \
  --method PUT \
  --input settings.json
```

### Via GitHub Apps
Consider using automation apps:
- **Probot Settings**: Manages repository settings via config file
- **Branch Protector**: Automated branch protection management

## Labels

The repository uses a comprehensive labeling system:

### Priority Labels
- `priority: critical`, `priority: high`, `priority: medium`, `priority: low`

### Type Labels
- `type: bug`, `type: feature`, `type: enhancement`, `type: documentation`
- `type: refactoring`, `type: security`

### Status Labels
- `status: in progress`, `status: blocked`, `status: needs review`, `status: needs testing`

### Component Labels
- `component: core`, `component: client`, `component: security`
- `component: dashboard`, `component: ci/cd`

### Special Labels
- `dependencies`, `nuget`, `github-actions`
- `good first issue`, `help wanted`, `question`

## Contributing

When contributing to this repository:
1. Follow branch naming conventions (see `docs/BRANCH_PROTECTION.md`)
2. Use appropriate issue templates when reporting issues
3. Fill out the PR template completely when submitting PRs
4. Ensure all status checks pass before requesting review
5. Address code owner feedback promptly

## Documentation

For more information, see:
- [Branch Protection Rules](../docs/BRANCH_PROTECTION.md)
- [Contributing Guidelines](../docs/CONTRIBUTING.md)
- [Security Policy](SECURITY.md)
- [Architecture Documentation](../docs/ARCHITECTURE.md)

## Maintenance

This configuration should be reviewed and updated:
- When adding new workflows or automation
- When changing branch protection requirements
- When modifying contributor guidelines
- When updating security policies

**Maintained By**: @KyleC69, @OldSkoolzRoolz  
**Last Updated**: 2025-12-17
