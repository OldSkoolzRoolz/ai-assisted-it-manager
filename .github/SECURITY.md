# Security Policy

## Supported Branches

We provide security updates for the following branches:

| Branch | Supported | Status |
| ------ | --------- | ------ |
| master | ✅ | Active development and security updates |
| feature/* | ⚠️ | Security fixes backported on case-by-case basis |

## Reporting a Vulnerability

We take security vulnerabilities seriously. If you discover a security vulnerability in the AI-Assisted IT Manager platform, please follow these steps:

### How to Report

1. **DO NOT** create a public GitHub issue for security vulnerabilities
2. **DO** email security reports to the repository maintainers:
   - Contact: @KyleC69 (Repository CODEOWNER)
   - Include "SECURITY VULNERABILITY" in the subject line

### What to Include

Please provide the following information in your security report:

- **Description**: A clear description of the vulnerability
- **Steps to Reproduce**: Detailed steps to reproduce the vulnerability
- **Impact Assessment**: Your assessment of the potential impact
- **Affected Components**: Which parts of the system are affected (CorePolicyEngine, ClientApp, etc.)
- **Suggested Fix**: If you have suggestions for remediation (optional)
- **Disclosure Timeline**: Your preferred timeline for disclosure

### Response Expectations

- **Acknowledgment**: We will acknowledge receipt of your report within 24 hours
- **Initial Assessment**: We will provide an initial assessment within 72 hours
- **Regular Updates**: We will provide updates on our investigation every 7 days
- **Resolution Timeline**: We aim to resolve critical vulnerabilities within 30 days
- **Public Disclosure**: We will coordinate with you on responsible disclosure timing

## Security Features

### Dependency Management

- **Dependabot**: Automated dependency updates enabled
- **Security Updates**: Daily scans for vulnerable dependencies
- **Package Scanning**: NuGet packages are automatically scanned for known vulnerabilities

### Code Analysis

- **CodeQL**: Automated security code analysis on all pull requests
  - Workflow runs on push, pull requests, and weekly schedule
  - Scans for SQL injection, XSS, hardcoded credentials, and other vulnerabilities
  - Results available in GitHub Security tab
  - Workflow file: `.github/workflows/codeql.yml`
- **Secret Scanning**: Automated Gitleaks workflow detects hardcoded secrets
  - Scans for passwords, API keys, tokens, and connection strings
  - Runs on push, pull requests, and daily schedule
  - Custom rules in `.gitleaks.toml`
  - Workflow file: `.github/workflows/secret-scanning.yml`
- **Branch Protection**: Required status checks include security analysis
- **CODEOWNERS**: Critical security files require approval from @KyleC69

### Development Security

- **No Secrets in Code**: We do not commit secrets, API keys, or credentials to the repository
- **Environment Variables**: Sensitive configuration uses environment variables or secure configuration providers
- **Database Security**: SQL Server connections use integrated security or secure connection strings

## Security Considerations for IT Managers

This platform interacts with critical Windows systems and Group Policy infrastructure. Please consider:

### Deployment Security

- **Privilege Requirements**: The platform requires administrative privileges for policy deployment
- **Network Security**: Ensure secure communication channels when deploying to domain environments
- **Audit Logging**: All policy changes and deployments are logged for audit purposes

### Data Protection

- **Local Data**: Policy configurations and history are stored in local SQL Server database
- **Encryption**: Sensitive policy data should be protected with appropriate encryption
- **Backup Security**: Ensure database backups are stored securely

## Security Updates Cadence

- **Critical Vulnerabilities**: Immediate fixes and patches
- **High Severity**: Within 7 days
- **Medium/Low Severity**: Included in regular release cycle (monthly)
- **Dependency Updates**: Weekly automated updates via Dependabot

## Contact

For security-related questions or concerns:
- Repository CODEOWNER: @KyleC69
- General repository issues: @OldSkoolzRoolz

Thank you for helping to keep the AI-Assisted IT Manager platform secure!