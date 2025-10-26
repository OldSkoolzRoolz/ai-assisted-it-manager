# Security Scanning Setup

This document describes the automated security scanning workflows implemented in this repository to detect and prevent sensitive information from being committed.

## Overview

The repository uses multiple layers of security scanning to protect against accidental exposure of sensitive data:

1. **CodeQL Security Analysis** - Analyzes code for security vulnerabilities
2. **Gitleaks Secret Scanning** - Detects hardcoded secrets, passwords, and API keys
3. **GitHub Native Secret Scanning** - Platform-level secret detection (when enabled)

## Automated Workflows

### CodeQL Security Analysis

**File:** `.github/workflows/codeql.yml`

**What it does:**
- Analyzes C# code for security vulnerabilities
- Detects hardcoded credentials, SQL injection, XSS, and other issues
- Runs comprehensive security and quality queries

**When it runs:**
- On every push to `master` and `feature/*` branches
- On every pull request to `master`
- Weekly on Monday at 9:00 AM UTC

**How to view results:**
- Navigate to the **Security** tab in GitHub
- Click on **Code scanning alerts**
- Review and address any findings

### Gitleaks Secret Scanning

**File:** `.github/workflows/secret-scanning.yml`  
**Configuration:** `.gitleaks.toml`

**What it detects:**
- Hardcoded passwords and credentials
- API keys and tokens
- OAuth tokens and access keys
- SQL connection strings with passwords
- Private cryptographic keys (RSA, DSA, EC, PGP)
- Windows registry paths containing password data
- Generic secret patterns

**When it runs:**
- On every push to `master` and `feature/*` branches
- On every pull request to `master`
- Daily at 2:00 AM UTC

**How to view results:**
- Navigate to the **Security** tab in GitHub
- Click on **Code scanning alerts**
- Filter by category: "gitleaks"

## Custom Detection Rules

The `.gitleaks.toml` configuration file includes custom rules specific to this project:

- **Windows Registry Keys** - Detects registry paths that may contain sensitive data
- **SQL Connection Strings** - Identifies connection strings with embedded credentials
- **API Key Patterns** - Detects various API key formats
- **OAuth Tokens** - Identifies OAuth access tokens
- **Windows Credentials** - Detects username/password pairs

## Allowlisted Patterns

The following are excluded from scanning to reduce false positives:

**Paths:**
- Build artifacts (`bin/`, `obj/`, `packages/`)
- Test data directories
- Documentation examples
- Generated files

**Patterns:**
- Example/placeholder values (e.g., "example_password", "your-api-key-here")
- Environment variable placeholders (e.g., `${API_KEY}`)
- Masked passwords (e.g., `password = ****`)

## What to Do If Secrets Are Detected

### If a secret is found in your PR:

1. **Do not force-push** to hide the commit - it's already in the history
2. **Remove the secret** from the code immediately
3. **Rotate/revoke the exposed secret** (change passwords, regenerate API keys)
4. **Use secure alternatives**:
   - Environment variables
   - Azure Key Vault or similar secret management
   - User secrets for local development (`dotnet user-secrets`)
   - Configuration providers with encrypted values

### If a false positive is detected:

1. Verify it's truly not sensitive information
2. Add the pattern to the allowlist in `.gitleaks.toml`
3. Document why it's safe in the PR description
4. Get approval from a code owner

## Best Practices

### DO:
- ✅ Use environment variables for sensitive configuration
- ✅ Use .NET User Secrets for local development
- ✅ Use Azure Key Vault or similar for production secrets
- ✅ Review security scan results before merging PRs
- ✅ Rotate secrets immediately if they're accidentally committed

### DON'T:
- ❌ Commit passwords, API keys, or tokens in code
- ❌ Store connection strings with credentials in appsettings.json
- ❌ Use real production secrets in test code
- ❌ Ignore security warnings without investigation
- ❌ Disable security checks to pass CI

## Configuration Files

### .NET User Secrets (for local development)

```bash
# Set a secret
dotnet user-secrets set "ApiKey" "your-secret-value"

# List secrets
dotnet user-secrets list
```

User secrets are stored outside the project directory and are never committed.

### Environment Variables (for deployment)

In production, use environment variables or secure configuration providers:

```csharp
var apiKey = configuration["ApiKey"]; // Reads from env vars
```

### Azure Key Vault (for production)

```csharp
// Add to Program.cs or Startup.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Monitoring and Maintenance

### Weekly Tasks:
- Review CodeQL alerts from the weekly scan
- Check for new vulnerabilities in dependencies

### Daily Tasks (Automated):
- Gitleaks scans run automatically
- Review any new secret detection alerts

### Per-PR Tasks:
- Ensure security checks pass before merging
- Review and address any new security findings
- Get security approval if modifying security-sensitive code

## Resources

- [CodeQL Documentation](https://codeql.github.com/docs/)
- [Gitleaks Documentation](https://github.com/gitleaks/gitleaks)
- [GitHub Secret Scanning](https://docs.github.com/en/code-security/secret-scanning)
- [.NET User Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/)

## Contact

For security concerns or questions about the scanning setup:
- Repository CODEOWNER: @KyleC69
- See also: [SECURITY.md](../.github/SECURITY.md)
