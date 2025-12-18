# Repository Scripts

This directory contains utility scripts for repository maintenance and automation.

## Available Scripts

### Label Management

#### `create-labels.sh` (Bash)
Automatically creates all GitHub labels defined in `.github/settings.yml`.

**Requirements**:
- GitHub CLI (`gh`) installed and authenticated
- Bash shell (Linux, macOS, Git Bash on Windows)

**Usage**:
```bash
./scripts/create-labels.sh
```

**What it does**:
- Reads label definitions from `.github/settings.yml`
- Creates all labels in the GitHub repository
- Skips labels that already exist
- Provides colored output showing success/failure status

#### `Create-Labels.ps1` (PowerShell)
Windows-native version of the label creation script.

**Requirements**:
- GitHub CLI (`gh`) installed and authenticated
- PowerShell 5.1+ or PowerShell Core

**Usage**:
```powershell
.\scripts\Create-Labels.ps1
```

**Installation (if GitHub CLI not installed)**:
```powershell
winget install --id GitHub.cli
# Then authenticate:
gh auth login
```

**What it does**:
- Same functionality as bash version
- Native PowerShell experience with colored output
- Works on Windows, macOS, and Linux

### Documentation Validation

#### `validate-doc-versions.ps1`
Validates the documentation version manifest to ensure all documentation is up-to-date.

**Requirements**:
- PowerShell 5.1+ or PowerShell Core

**Usage**:
```powershell
.\scripts\validate-doc-versions.ps1
```

**What it does**:
- Checks `docs/DOCUMENTATION_VERSION_MANIFEST.md`
- Validates that all documented files exist
- Ensures version dates are recent
- Used by CI/CD workflows

## Script Development Guidelines

When adding new scripts to this directory:

1. **Follow naming conventions**:
   - Bash scripts: `kebab-case.sh`
   - PowerShell scripts: `PascalCase.ps1`

2. **Include error handling**:
   - Check for required tools/dependencies
   - Provide clear error messages
   - Use exit codes appropriately

3. **Add documentation**:
   - Include header comments explaining purpose
   - Document required parameters
   - Provide usage examples
   - Update this README

4. **Make scripts portable**:
   - Don't hard-code paths
   - Use relative paths from repository root
   - Support multiple platforms when possible

5. **Test thoroughly**:
   - Test on fresh installations
   - Test error conditions
   - Verify in CI/CD if applicable

## Troubleshooting

### GitHub CLI Not Found

**Error**: `gh: command not found` or `gh is not recognized`

**Solution**:
- **Windows**: `winget install --id GitHub.cli`
- **macOS**: `brew install gh`
- **Linux**: See [GitHub CLI installation guide](https://github.com/cli/cli/blob/trunk/docs/install_linux.md)

### GitHub CLI Not Authenticated

**Error**: `Not authenticated with GitHub CLI`

**Solution**:
```bash
gh auth login
```
Follow the prompts to authenticate.

### Permission Denied (Bash)

**Error**: `Permission denied: ./scripts/create-labels.sh`

**Solution**:
```bash
chmod +x ./scripts/create-labels.sh
./scripts/create-labels.sh
```

### Labels Already Exist

**Behavior**: Script shows "Already exists (skipped)" for existing labels

**Action**: This is normal and expected. The script will create only missing labels.

## Contributing

When contributing scripts:

1. Test your script thoroughly
2. Add appropriate error handling
3. Update this README with script documentation
4. Ensure scripts work on multiple platforms if applicable
5. Follow existing script patterns for consistency

## Support

For questions about scripts:
1. Check this documentation
2. Review script comments and help text
3. Ask in [GitHub Discussions](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/discussions)
4. Contact maintainers: @KyleC69, @OldSkoolzRoolz

---

**Maintained By**: @KyleC69, @OldSkoolzRoolz  
**Last Updated**: 2025-12-18
