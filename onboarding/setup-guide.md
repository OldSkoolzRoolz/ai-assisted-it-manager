# Setup Guide

## Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or JetBrains Rider
- PowerShell 7+
- Git CLI or GitHub Desktop

## Initial Setup
1. Clone the repo
2. Run `onboarding/workspace-presets.ps1`
3. Open `ai-assisted-it-manager.sln`
4. Set `ClientApp` or `EnterpriseDashboard` as startup project
5. Build and run in Debug mode

## Notes
- Defender exclusions applied to `bin/`, `obj/`, and `packages/`
- Registry access requires elevated privileges
- CLI tools available in `src/Tools/`