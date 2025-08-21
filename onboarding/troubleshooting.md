# Troubleshooting Guide

## Common Errors

### ❌ WinRT or UI Runtime Errors
- Check if `ClientApp` is referencing correct SDK
- Validate `App.xaml.cs` entry points

### ❌ ADMX Parsing Failures
- Ensure schema files are present in `CorePolicyEngine/Schemas`
- Use `PolicyValidator.exe` in `Tools/` for dry runs

### ❌ Git Issues
- Markdown not visible in Solution Explorer: Use symbolic links
- Locked files: Close Visual Studio and retry

### ❌ Defender Interference
- Add exclusions via `workspace-presets.ps1`