|[![.NET](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/dotnet.yml/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/dotnet.yml)|[![CodeQL](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/github-code-scanning/codeql)|[![.NET CI](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/dotnet-ci.yml)|[![CodeQL Security Analysis](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/codeql.yml/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/codeql.yml)|[![Copilot code review](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/copilot-pull-request-reviewer/copilot-pull-request-reviewer/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/copilot-pull-request-reviewer/copilot-pull-request-reviewer)|[![Copilot coding agent](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/copilot-swe-agent/copilot/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/copilot-swe-agent/copilot)|[![Pull Request Validation](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/pr-validation.yml)|[![Secret Scanning](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/secret-scanning.yml/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/secret-scanning.yml)|[![Validate Documentation Versions](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/doc-version-validation.yml/badge.svg)](https://github.com/OldSkoolzRoolz/ai-assisted-it-manager/actions/workflows/doc-version-validation.yml)|
---
## 🛡️ AI-Assisted Policy Manager & IT Companion

Enterprise-grade toolkit for policy management, system monitoring, and AI-assisted automation. Designed for IT managers who want clarity, control, and self-healing environments.

---

## 📦 Project Overview

This repo is the foundation for a **multi-phase product**:

| Phase | Focus |
|-------|-------|
| **1** | Core Policy Manager (ADMX/ADML parsing, live deployment, rollback) |
| **2** | AI Monitoring Layer (anomaly detection, predictive alerts, auto-fix) |
| **3** | Enterprise Dashboard (remote control, RBAC, audit reports) |
| **4** | Monetization & Market Strategy (free teaser → paid cloud edition) |

---

## 🧱 Tech Stack

- **Frontend**: Initial WPF UI - WinUI 3 (Fluent UI for modern desktop) when SDK stabilizes
- **Backend**: C# .NET 9 (Registry, WMI, ADMX parsing)
- **Storage**: SQL Server (centralized policy store), Deployable to Enterprise SQL Server
- **AI Layer**: ONNX models for pattern recognition & natural language queries
- **Web Dashboard (Phase 3)**: Blazor Server

---

## 🚀 Getting Started

### Prerequisites
- Windows 10/11 or Server 2019+
- Visual Studio 2022+
- .NET 9 SDK
- PowerShell 7+
- Git (modern PR workflows recommended)

### Setup
```bash
git clone https://github.com/OldSkoolzRoolz/ai-assisted-it-manager.git
```
Then open the solution in Visual Studio and run `docs/setup.ps1` to configure your workspace.

---

## 🧠 Core Features (Phase 1)

- ADMX/ADML parser with visual editor
- Real-time validation & syntax highlighting
- Live policy deployment to local/OU targets
- Rollback & version history
- Workspace presets for onboarding
- Initial public release with limited AI features - Free local-only edition

---

## 🤖 AI Capabilities (Phase 2 Preview)

- Snapshot system configs at intervals
- Detect policy drift and misconfigurations
- Predictive alerts for risky GPOs
- Natural language queries (e.g. “Show all machines with disabled Defender”)
- Self-healing automation hooks

---

## 📊 Enterprise Dashboard (Phase 3)

- Blazor-based remote control panel
- Role-based access (admin, auditor, help desk)
- Push notifications to Teams/Slack
- Compliance reports (ISO, NIST, CIS)
- TLS 1.3 encryption & signed policy packages

---

## 💼 Monetization Strategy (Phase 4)

- Free local-only edition → Paid cloud-connected version
- Per-device subscription model for enterprises
- White-label offering for MSPs
- Contributor incentives: early collaborators may be offered paid roles

---

## 🧪 Testing

```bash
dotnet test tests/
```

---

## 🤝 Contributing

We welcome contributors of all skill levels! This project follows standard branch protection rules and collaboration practices.

### Quick Links
- 📖 [Contributing Guidelines](docs/CONTRIBUTING.md) - How to contribute
- 🔒 [Branch Protection](docs/BRANCH_PROTECTION.md) - Branch rules and PR requirements
- 🐛 [Issue Templates](.github/ISSUE_TEMPLATE/) - Report bugs or request features
- 🔧 [Security Policy](.github/SECURITY.md) - Report security vulnerabilities

### Getting Started
1. Fork the repository
2. Create a feature branch (`feature/your-feature-name`)
3. Make your changes following our [coding standards](docs/CONTRIBUTING.md#coding-standards)
4. Submit a pull request with a clear description

**Incentive**: Stick with us through Phase 1 and you may be invited to join the paid release team.

---

## 📄 License

All Rights Reserved. See [`LICENSE`](LICENSE) for details.

---

