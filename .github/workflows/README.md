# GitHub Actions Workflows

This directory contains automated workflows for the AI-Assisted IT Manager repository.

## Workflows Overview

### Build & Test Workflows

#### `.NET CI` (`dotnet-ci.yml`)
- **Purpose**: Continuous integration for .NET solution
- **Triggers**: Push and PR to master and feature/* branches
- **Actions**: Restore, build (Release), test, upload artifacts
- **Runner**: Windows 2022

#### `.NET` (`dotnet.yml`)
- **Purpose**: Build and test selected projects (excluding DB)
- **Triggers**: Push and PR to master
- **Actions**: Restore, build (Debug) selected projects, test
- **Runner**: Windows latest

### Security Workflows

#### `CodeQL Security Analysis` (`codeql.yml`)
- **Purpose**: Automated security code analysis for C# code
- **Triggers**: 
  - Push to master and feature/* branches
  - Pull requests to master
  - Weekly schedule (Mondays at 9 AM UTC)
- **Detects**: 
  - Hardcoded credentials and secrets
  - SQL injection vulnerabilities
  - Cross-site scripting (XSS)
  - Other security vulnerabilities
- **Output**: Results in GitHub Security tab
- **Runner**: Windows 2022
- **Timeout**: 6 hours

#### `Secret Scanning` (`secret-scanning.yml`)
- **Purpose**: Scan repository for hardcoded secrets and passwords
- **Tool**: Gitleaks
- **Triggers**: 
  - Push to master and feature/* branches
  - Pull requests to master
  - Daily schedule (2 AM UTC)
- **Detects**: 
  - Passwords and credentials
  - API keys and tokens
  - OAuth tokens
  - Private cryptographic keys
  - SQL connection strings
- **Configuration**: `.gitleaks.toml` in repository root
- **Output**: Results in GitHub Security tab (SARIF upload)
- **Runner**: Ubuntu latest

### Documentation Workflows

#### `Validate Documentation Versions` (`doc-version-validation.yml`)
- **Purpose**: Validate documentation version metadata
- **Triggers**: Push and PR affecting documentation files
- **Script**: `scripts/validate-doc-versions.ps1`
- **Runner**: Windows latest

## Required Secrets

Current workflows use only `GITHUB_TOKEN` which is automatically provided by GitHub Actions.

Optional:
- `GITLEAKS_LICENSE` - Only required for Gitleaks enterprise features. The workflow is designed to work with or without this secret. If not provided, Gitleaks will use the free version with all standard features.

## Branch Protection

The following workflows are recommended as required status checks:
- `build-and-test` (from dotnet-ci.yml)
- `analyze` (from codeql.yml)
- `Gitleaks Secret Scan` (from secret-scanning.yml)

## Security Best Practices

1. **Review Security Alerts**: Check the Security tab regularly for CodeQL and Gitleaks findings
2. **Fix Before Merge**: Address security findings before merging pull requests
3. **Don't Disable Scans**: Never disable security checks to pass CI without investigation
4. **Rotate Exposed Secrets**: If a secret is detected, rotate it immediately

## Monitoring

- **CodeQL**: Weekly scans (Mondays 9 AM UTC) catch new vulnerability patterns
- **Gitleaks**: Daily scans (2 AM UTC) perform comprehensive repository scanning for secrets
- **Build**: Every push ensures code compiles and tests pass

## Documentation

For detailed information about security scanning:
- See `docs/SECURITY-SCANNING.md`
- See `docs/SECURITY-HARDENING.md`
- See `.github/SECURITY.md`

## Troubleshooting

### CodeQL Issues
- Ensure Windows runner is available
- Check that .NET 9 SDK is properly installed
- Verify Windows App SDK workload is installed

### Gitleaks False Positives
- Update allowlist in `.gitleaks.toml`:
  - **[allowlist.paths]**: Add file paths or glob patterns to exclude
  - **[allowlist.regexes]**: Add regex patterns to exclude specific strings
  - **[allowlist.stopwords]**: Add words that indicate test/example data
- Document why it's a false positive in PR
- Example: To exclude a test file, add `"**/tests/fixtures/example-config.json"` to the paths array

### Build Failures
- Check .NET SDK version compatibility
- Ensure all NuGet packages restore correctly
- Verify Windows-specific dependencies are available

## Maintenance

- **Weekly**: Review CodeQL weekly scan results
- **Monthly**: Update action versions (Dependabot helps)
- **Quarterly**: Review and update security configurations
