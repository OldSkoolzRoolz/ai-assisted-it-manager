# Branch Protection and Collaboration Rules

This document describes the branch protection rules and collaboration guidelines for the AI-Assisted IT Manager repository.

## Table of Contents

- [Overview](#overview)
- [Branch Protection Rules](#branch-protection-rules)
- [Branch Naming Conventions](#branch-naming-conventions)
- [Pull Request Requirements](#pull-request-requirements)
- [Code Review Process](#code-review-process)
- [Status Checks](#status-checks)
- [Applying Branch Protection](#applying-branch-protection)

## Overview

Branch protection rules help maintain code quality and ensure that all changes go through proper review and validation before being merged into the main codebase. These rules apply to both internal team members and external contributors.

**Important**: The `master` branch is treated as **release-ready** at all times. All code merged to master must be production-quality, fully tested, and approved by code owners.

## Branch Protection Rules

### Master Branch Protection

The `master` branch is the primary development branch and is **treated as release-ready**. All code merged to master must be production-quality and has the following strict protections:

#### Required Reviews
- **Minimum 1 approval** required before merging
- **Code Owner review required** - At least one code owner (@KyleC69 or @OldSkoolzRoolz) **must** approve all changes to master
- **Stale review dismissal** - Approvals are dismissed when new commits are pushed
- **Conversation resolution** - All review comments must be resolved before merging

#### Status Checks
The following CI checks must pass before merging:
- **build-and-test** - .NET CI build and test suite (from `dotnet-ci.yml`)
- **Analyze Code** - CodeQL security analysis (from `codeql.yml`)
- **validate-doc-versions** - Documentation version validation (from `doc-version-validation.yml`)

#### Branch Requirements
- **Up-to-date branch required** - Branch must be updated with latest master before merging
- **No force pushes** - Force pushes are disabled to protect history
- **No deletions** - Branch cannot be deleted
- **Administrator enforcement** - Rules apply to repository administrators

### Feature Branch Protection

Feature branches (`feature/*`) have lighter protections to allow rapid iteration:

#### Required Reviews
- **Minimum 1 approval** required
- Code Owner review not strictly required
- Stale reviews not automatically dismissed

#### Status Checks
- **build-and-test** - Must pass before merging
- **Up-to-date branch not required** - Allows more flexibility

#### Branch Requirements
- **Force pushes allowed** - Enables rebasing and history cleanup
- **Deletions allowed** - Branches can be deleted after merge
- **Administrator enforcement disabled** - More flexibility for rapid development

## Branch Naming Conventions

Follow these naming conventions for consistency:

### Standard Prefixes

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feature/` | New features or enhancements | `feature/policy-editor-ui` |
| `bugfix/` | Bug fixes | `bugfix/crash-on-startup` |
| `hotfix/` | Critical production fixes | `hotfix/security-vulnerability` |
| `release/` | Release preparation | `release/v1.0.0` |
| `docs/` | Documentation changes | `docs/update-readme` |
| `refactor/` | Code refactoring | `refactor/extract-services` |
| `copilot/` | AI-assisted changes | `copilot/apply-branch-protections` |

### Naming Guidelines
- Use lowercase letters
- Separate words with hyphens (kebab-case)
- Keep names descriptive but concise
- Include issue number if applicable: `feature/123-add-logging`

## Pull Request Requirements

### PR Title
- Minimum 10 characters
- Should be descriptive and clear
- Use conventional commit format (optional but recommended):
  - `feat: Add new feature`
  - `fix: Fix bug in component`
  - `docs: Update documentation`
  - `refactor: Restructure code`
  - `test: Add tests for feature`
  - `chore: Update dependencies`

### PR Description
- Minimum 20 characters required
- Should include:
  - **What**: What changes are being made
  - **Why**: Why these changes are necessary
  - **How**: How the changes were implemented
  - **Testing**: How the changes were tested
  - **Breaking Changes**: Any breaking changes (if applicable)

### PR Size Recommendations
- **Small** (< 100 lines): ✅ Easy to review
- **Medium** (100-500 lines): ✅ Reasonable size
- **Large** (500-1000 lines): ⚠️ Consider breaking up
- **Extra Large** (> 1000 lines): ❌ Should be split into multiple PRs

### PR Checklist
Before submitting a PR, ensure:
- [ ] Code follows .NET coding standards
- [ ] All tests pass locally
- [ ] Documentation is updated if needed
- [ ] XML documentation added for new public APIs
- [ ] No compiler warnings
- [ ] Code reviewed personally before requesting review
- [ ] Breaking changes are documented
- [ ] Commit messages are clear and descriptive

## Code Review Process

### Review Guidelines

#### For Reviewers
1. **Be respectful and constructive** - Focus on code, not the person
2. **Be thorough** - Check logic, edge cases, security, and performance
3. **Be timely** - Review within 1-2 business days when possible
4. **Ask questions** - If something is unclear, ask for clarification
5. **Approve or request changes** - Don't leave reviews hanging

#### For Authors
1. **Respond to all comments** - Address or discuss each review comment
2. **Don't take feedback personally** - Reviews improve code quality
3. **Update the PR** - Make requested changes or explain why you disagree
4. **Resolve conversations** - Mark conversations as resolved when addressed
5. **Keep reviewers updated** - Notify when ready for re-review

### Code Owner Review

The CODEOWNERS file specifies who must approve changes to specific areas:
- **Core owners**: @KyleC69, @OldSkoolzRoolz
- All changes require approval from at least one code owner
- Critical files (workflows, CODEOWNERS, security) require extra scrutiny

## Status Checks

All status checks must pass before merging. These run automatically on every push.

### .NET CI (`build-and-test`)
- Builds the solution with Release configuration
- Runs all unit tests
- Treats warnings as errors
- Must complete successfully

### CodeQL Security Analysis (`Analyze Code`)
- Scans code for security vulnerabilities
- Checks for common security issues
- Runs weekly and on every PR
- Must not introduce new critical issues

### Documentation Validation (`validate-doc-versions`)
- Validates documentation version manifest
- Ensures documentation is up-to-date
- Checks for broken links
- Must pass for documentation changes

### PR Validation (`pr-validation`)
- Validates PR title and description
- Checks branch naming conventions
- Analyzes PR size and complexity
- Auto-detects affected components
- Provides feedback and warnings

## Applying Branch Protection

### Via GitHub UI

1. Navigate to repository **Settings** → **Branches**
2. Click **Add branch protection rule**
3. Enter branch name pattern (e.g., `master`, `feature/*`)
4. Configure protection settings based on `.github/settings.yml`
5. Save changes

### Via GitHub API

Use the settings documented in `.github/settings.yml` with GitHub API:

```bash
# Example using GitHub CLI
gh api repos/OldSkoolzRoolz/ai-assisted-it-manager/branches/master/protection \
  --method PUT \
  --field required_pull_request_reviews[required_approving_review_count]=1 \
  --field required_status_checks[strict]=true \
  --field enforce_admins=true
```

### Via GitHub Apps

Consider using:
- [Probot Settings](https://probot.github.io/apps/settings/) - Manages settings via `.github/settings.yml`
- [Branch Protector](https://github.com/apps/branch-protector) - Automated branch protection management

## Collaboration Best Practices

### Communication
- Use PR comments for code-specific discussions
- Use Issues for feature requests and bug reports
- Use Discussions for general questions and ideas
- Tag relevant team members for visibility

### Continuous Integration
- Monitor CI status checks
- Fix failing builds promptly
- Don't merge if checks are failing
- Report persistent CI issues

### Security
- Never commit secrets or credentials
- Use environment variables for sensitive data
- Report security issues via GitHub Security Advisories
- Follow secure coding practices

### Documentation
- Keep documentation in sync with code
- Update DOCUMENTATION_VERSION_MANIFEST.md when needed
- Add inline code comments for complex logic
- Write clear commit messages

## Troubleshooting

### Common Issues

#### "Branch protection prevents merging"
- Ensure all required reviews are approved
- Check that all status checks pass
- Resolve all conversations
- Update branch with latest master if required

#### "Status check failed"
- Review the failed check logs
- Fix the issues locally
- Push fixes to the PR branch
- Wait for checks to re-run

#### "Code Owner review required"
- Request review from code owners (@KyleC69, @OldSkoolzRoolz)
- Wait for approval from at least one code owner
- Address any feedback from code owners

## References

- [GitHub Branch Protection Documentation](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository)
- [CODEOWNERS File](.github/CODEOWNERS)
- [Repository Settings](.github/settings.yml)
- [Contributing Guidelines](CONTRIBUTING.md)
- [Security Policy](.github/SECURITY.md)

## Questions?

If you have questions about branch protection or collaboration:
1. Check this documentation
2. Review existing PRs as examples
3. Ask in repository Discussions
4. Contact code owners directly

---

**Last Updated**: 2025-12-17  
**Maintained By**: @KyleC69, @OldSkoolzRoolz
