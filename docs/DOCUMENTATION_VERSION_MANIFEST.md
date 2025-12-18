# Documentation Version Manifest

| **Field** | **Value** |
|-----------|-----------|
| **Version** | 2025-12-17.v13 |

**Purpose**: This document maintains version control and audit trails for all documentation files in the `/docs` folder.

**Last Updated**: 2025-12-17  
**Maintained By**: @KyleC69 (Repository CODEOWNER)  
**Version Format**: YYYY-MM-DD.vX (e.g., 2025-08-25.v1)

---

## Current Documentation Inventory

| Document | Current Version | Last Modified | Status | Description |
|----------|----------------|---------------|---------|-------------|
| `ARCHITECTURE.md` | v5.0 | 2025-08-25 | Active | Architecture + async & localization strategies |
| `PhaseOne.md` | v3.0 | 2025-08-25 | Active | Phase 1 scope, feature list, and implementation planning |
| `BehaviorSettingsRegistry.md` | v3.0 | 2025-08-25 | Active | Registry mapping and ADMX behavior settings documentation |
| `DOCUMENTATION_VERSION_MANIFEST.md` | v13.0 | 2025-12-17 | Active | This versioning manifest document |
| `ConfigSettings.md` | v3.0 | 2025-08-25 | Active | Client application configurable settings and registry mappings |
| `.github/copilot-instructions.md` | v4.0 | 2025-12-17 | Active | GitHub Copilot coding agent instructions following best practices |
| `WinUITheming.md` | v2.0 | 2025-08-25 | Active | WinUI theming standards for AI code generation |
| `Log-file-interface-design.md` | v1.0 | 2025-08-25 | Draft | Log file interface design specification |
| `CONTRIBUTING.md` | v1.0 | 2025-12-17 | Active | Contributor guidelines with coding standards and workflow |

---

## Change Log

### 2025-12-17.v13
**Modified By**: @copilot
**Change Type**: Major - Copilot instructions setup and cleanup
**Files Affected**: `.github/copilot-instructions.md` (updated), `obsolete-copilot-instructions.md` (removed), `DOCUMENTATION_VERSION_MANIFEST.md`
**Description**: Set up and optimized GitHub Copilot instructions following official best practices. Added repository overview section, standardized section numbering and formatting, improved consistency in section markers (ENFORCED/FLEXIBLE). Removed obsolete instructions file to eliminate confusion. Updated instructions to v4.0 with comprehensive coverage of build, testing, coding standards, logging, localization, and theming guidelines.

### 2025-10-26.v12
**Modified By**: @copilot
**Change Type**: Major - Security scanning workflows implementation
**Files Affected**: `SECURITY-HARDENING.md`, `SECURITY-SCANNING.md` (new), `.github/SECURITY.md`, `.github/workflows/codeql.yml` (new), `.github/workflows/secret-scanning.yml` (new), `.gitleaks.toml` (new), `.gitignore`
**Description**: Implemented automated security scanning workflows including CodeQL for code analysis and Gitleaks for secret detection. Added comprehensive documentation for security scanning setup and configuration. Updated security hardening guide to reflect implemented workflows.

### 2025-08-25.v12
**Modified By**: @KyleC69
**Change Type**: Minor 
**Files Affected**: `DOCUMENTATION_VERSION_MANIFEST.md`, `copilot-instructions.md`
**Description**: Relaxed AI code generation instructions to allow for more creative solutions; updated manifest version.

### 2025-08-25.v11
**Modified By**: @copilot  
**Change Type**: Removal  
**Files Affected**: `DOCUMENTATION_VERSION_MANIFEST.md`, removed `LOCALIZATION_STEPS_TEMP.md`  
**Description**: Removed temporary localization steps document after integration; updated manifest inventory and version.

### 2025-08-25.v10
**Modified By**: @copilot  
**Change Type**: Minor - Added version metadata row & aligned manifest table versions (vX.Y pattern)  
**Files Affected**: `DOCUMENTATION_VERSION_MANIFEST.md`  
**Description**: Introduced explicit version row for manifest; updated manifest entry to v10.0; corrected Last Modified date for `.github/copilot-instructions.md` to 2025-08-25.

### 2025-08-25.v9
**Modified By**: @copilot  
**Change Type**: Minor - Standard table metadata header applied across docs  
**Files Affected**: `ARCHITECTURE.md`, `PhaseOne.md`, `BehaviorSettingsRegistry.md`, `SECURITY-HARDENING.md`, `ConfigSettings.md`, `WinUITheming.md`, `Log-file-interface-design.md`, `DOCUMENTATION_VERSION_MANIFEST.md`  
**Description**: Converted all documentation to unified table-form metadata header and bumped versions accordingly.

### 2025-08-25.v8
**Modified By**: @copilot  
**Change Type**: Minor - Applied standard metadata headers to multiple docs  
**Files Affected**: `PhaseOne.md`, `BehaviorSettingsRegistry.md`, `SECURITY-HARDENING.md`, `ConfigSettings.md`, `DOCUMENTATION_VERSION_MANIFEST.md`  
**Description**: Added unified Document Metadata header block and bumped versions accordingly.

<!-- Older entries retained --><!-- Older entries retained -->
