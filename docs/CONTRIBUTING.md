# Contributing to AI-Assisted IT Manager

Thank you for your interest in contributing to the AI-Assisted IT Manager project! We welcome contributions from developers of all skill levels.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Pull Request Process](#pull-request-process)
- [Testing](#testing)
- [Documentation](#documentation)
- [Community](#community)

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for all contributors. We expect everyone to:
- Be respectful and constructive
- Focus on what is best for the project and community
- Show empathy towards others
- Accept constructive criticism gracefully
- Help newcomers and be patient with questions

### Unacceptable Behavior

- Harassment or discrimination of any kind
- Trolling, insulting comments, or personal attacks
- Publishing others' private information
- Any conduct that would be inappropriate in a professional setting

## Getting Started

### Prerequisites

Before contributing, ensure you have:
- Windows 10/11 or Windows Server 2019+
- Visual Studio 2022 or later
- .NET 9 SDK
- Git for version control
- PowerShell 7+ (for build scripts)

### Initial Setup

1. **Fork the repository**
   ```bash
   # Click "Fork" on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/ai-assisted-it-manager.git
   cd ai-assisted-it-manager
   ```

2. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/OldSkoolzRoolz/ai-assisted-it-manager.git
   ```

3. **Install dependencies**
   ```bash
   dotnet restore ITCompanion.sln
   ```

4. **Build the solution**
   ```bash
   dotnet build ITCompanion.sln -c Debug -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true
   ```

5. **Run tests**
   ```bash
   dotnet test ITCompanion.sln --no-build --configuration Debug
   ```

### Development Environment

- **IDE**: Visual Studio 2022+ recommended
- **Extensions**: 
  - ReSharper (optional, for enhanced code analysis)
  - GitHub Copilot (optional, for AI-assisted coding)
- **Configuration**: Follow `.editorconfig` for code style

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

#### 🐛 Bug Reports
- Check existing issues first to avoid duplicates
- Include detailed steps to reproduce
- Provide error messages and logs
- Specify your environment (OS, .NET version, etc.)

#### ✨ Feature Requests
- Clearly describe the feature and its benefits
- Explain the use case and rationale
- Consider implementation complexity
- Be open to discussion and alternatives

#### 📝 Documentation
- Fix typos, clarify explanations
- Add examples and tutorials
- Update outdated information
- Improve onboarding guides

#### 🔧 Code Contributions
- Bug fixes
- New features
- Performance improvements
- Code refactoring
- Test coverage improvements

## Development Workflow

### 1. Create an Issue

Before starting work on a significant change:
1. Search for existing issues
2. Create a new issue describing the change
3. Discuss the approach with maintainers
4. Wait for approval before starting work

For small fixes (typos, minor bugs), you can skip this step.

### 2. Create a Branch

Follow our branch naming conventions:

```bash
# For new features
git checkout -b feature/descriptive-name

# For bug fixes
git checkout -b bugfix/issue-description

# For documentation
git checkout -b docs/what-youre-updating

# For refactoring
git checkout -b refactor/what-youre-refactoring
```

### 3. Make Changes

- Write clean, maintainable code
- Follow existing code style and patterns
- Add tests for new functionality
- Update documentation as needed
- Keep commits focused and atomic

### 4. Test Your Changes

```bash
# Build with warnings as errors
dotnet build ITCompanion.sln -c Debug -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true

# Run all tests
dotnet test ITCompanion.sln --no-build --configuration Debug

# Run specific test project
dotnet test tests/YourTestProject/YourTestProject.csproj
```

### 5. Commit Your Changes

Write clear, descriptive commit messages:

```bash
# Good commit messages
git commit -m "Fix crash when loading invalid ADMX files"
git commit -m "Add validation for policy settings dialog"
git commit -m "Update documentation for deployment process"

# Bad commit messages
git commit -m "fix bug"
git commit -m "updates"
git commit -m "WIP"
```

### 6. Keep Your Branch Updated

```bash
# Fetch latest changes
git fetch upstream

# Rebase on master
git rebase upstream/master

# Or merge if you prefer
git merge upstream/master
```

### 7. Push Your Changes

```bash
git push origin your-branch-name
```

### 8. Create a Pull Request

**Important**: All pull requests to `master` require approval from a code owner (@KyleC69 or @OldSkoolzRoolz). The master branch is treated as **release-ready** at all times.

1. Go to your fork on GitHub
2. Click "New Pull Request"
3. Select your branch
4. Fill out the PR template completely
5. Link related issues
6. Request review from code owners (@KyleC69 or @OldSkoolzRoolz)

## Coding Standards

### C# Code Style

Follow .NET coding conventions and project-specific guidelines:

#### Naming Conventions
- **PascalCase**: Classes, methods, properties, public fields
- **camelCase**: Local variables, parameters, private fields
- **_camelCase**: Private fields (with underscore prefix)
- **SCREAMING_SNAKE_CASE**: Constants

#### Code Organization
```csharp
// 1. Using directives (sorted)
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

// 2. Namespace
namespace KC.ITCompanion.ModuleName;

// 3. Class/interface with XML documentation
/// <summary>
/// Provides functionality for managing policies.
/// </summary>
public class PolicyManager
{
    // 4. Fields (private first)
    private readonly ILogger<PolicyManager> _logger;
    
    // 5. Constructors
    public PolicyManager(ILogger<PolicyManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // 6. Properties
    public string CurrentPolicy { get; set; }
    
    // 7. Methods (public first, then private)
    public void LoadPolicy() { }
    
    private void ValidatePolicy() { }
}
```

#### XML Documentation
All public APIs must have XML documentation:

```csharp
/// <summary>
/// Loads a policy from the specified file path.
/// </summary>
/// <param name="filePath">The path to the policy file.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>A task representing the asynchronous operation.</returns>
/// <exception cref="ArgumentNullException">Thrown when filePath is null.</exception>
/// <exception cref="FileNotFoundException">Thrown when file does not exist.</exception>
public async Task LoadPolicyAsync(string filePath, CancellationToken cancellationToken = default)
{
    // Implementation
}
```

#### Async/Await Guidelines
- Use `async`/`await` for I/O operations
- Methods returning Task should end with `Async` suffix
- Always pass `CancellationToken` in public async APIs
- Use `ConfigureAwait(false)` in library code (not UI code)

```csharp
public async Task<Policy> LoadPolicyAsync(string path, CancellationToken cancellationToken)
{
    var content = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
    return ParsePolicy(content);
}
```

#### Error Handling
- Use exceptions for exceptional cases
- Validate arguments and throw appropriate exceptions
- Log errors appropriately
- Don't swallow exceptions without good reason

```csharp
public void ProcessPolicy(Policy policy)
{
    ArgumentNullException.ThrowIfNull(policy);
    
    try
    {
        _logger.LogInformation("Processing policy: {PolicyName}", policy.Name);
        // Process policy
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process policy: {PolicyName}", policy.Name);
        throw;
    }
}
```

### Build Requirements

All code must:
- Build without warnings (`-warnaserror`)
- Pass all analyzer checks
- Follow `.editorconfig` rules
- Have zero code style violations

```bash
# Verify your code meets requirements
dotnet build ITCompanion.sln -c Debug -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true
dotnet format analyzers --verify-no-changes
dotnet format style --verify-no-changes
```

## Pull Request Process

### PR Checklist

Before submitting, ensure:
- [ ] Code builds without warnings
- [ ] All tests pass
- [ ] New tests added for new functionality
- [ ] Documentation updated if needed
- [ ] XML docs added for public APIs
- [ ] CHANGELOG updated (for significant changes)
- [ ] Branch is up-to-date with master
- [ ] Commit messages are clear
- [ ] PR description is complete

### PR Template

Your PR should include:

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix (non-breaking change)
- [ ] New feature (non-breaking change)
- [ ] Breaking change
- [ ] Documentation update

## Related Issues
Fixes #123, Relates to #456

## Changes Made
- Detailed list of changes
- Include technical details
- Explain design decisions

## Testing
- How were changes tested?
- What test cases were added?
- Any manual testing performed?

## Screenshots (if applicable)
Include screenshots for UI changes

## Checklist
- [ ] Code follows project standards
- [ ] Tests pass locally
- [ ] Documentation updated
- [ ] No breaking changes (or documented)
```

### Review Process

1. **Automated Checks**: CI runs automatically
2. **Code Owner Review**: Required approval from code owners
3. **Feedback**: Address reviewer comments
4. **Approval**: Get required approvals
5. **Merge**: Maintainers will merge when ready

### After Merge

- Delete your feature branch
- Pull latest changes from master
- Close related issues (if not auto-closed)
- Celebrate! 🎉

## Testing

### Test Structure

```
tests/
├── ModuleName.Tests/
│   ├── UnitTests/
│   │   ├── ServiceTests.cs
│   │   └── HelperTests.cs
│   └── IntegrationTests/
│       └── EndToEndTests.cs
```

### Writing Tests

Follow AAA pattern (Arrange, Act, Assert):

```csharp
[Fact]
public void LoadPolicy_WithValidPath_ReturnsPolicy()
{
    // Arrange
    var manager = new PolicyManager();
    var path = "test-policy.xml";
    
    // Act
    var policy = manager.LoadPolicy(path);
    
    // Assert
    Assert.NotNull(policy);
    Assert.Equal("TestPolicy", policy.Name);
}
```

### Test Naming

```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Fact]
public void Add_TwoNumbers_ReturnsSum() { }

[Fact]
public void LoadPolicy_FileNotFound_ThrowsException() { }

[Fact]
public async Task SaveAsync_ValidPolicy_SavesSuccessfully() { }
```

## Documentation

### Documentation Updates

When to update documentation:
- Adding new features or functionality
- Changing existing behavior
- Fixing bugs that affect user-facing behavior
- Updating APIs or configuration

### Documentation Files

- `README.md`: Project overview, getting started
- `docs/`: Technical documentation
- `onboarding/`: Setup guides, troubleshooting
- `BRANCH_PROTECTION.md`: Branch rules and collaboration
- `CONTRIBUTING.md`: This file

### Documentation Standards

- Use clear, concise language
- Include code examples
- Add diagrams for complex concepts
- Keep formatting consistent
- Update DOCUMENTATION_VERSION_MANIFEST.md

## Community

### Communication Channels

- **GitHub Issues**: Bug reports, feature requests
- **GitHub Discussions**: General questions, ideas
- **Pull Requests**: Code review, technical discussions

### Getting Help

- Read existing documentation
- Search closed issues
- Ask questions in Discussions
- Contact maintainers: @KyleC69, @OldSkoolzRoolz

### Recognition

Contributors who stick with us through Phase 1 may be offered:
- Paid roles in future releases
- Recognition in project documentation
- Contributor badges and credits

## Contributor Journey

### New Contributors

Start with:
- Good first issues (labeled `good first issue`)
- Documentation improvements
- Test coverage improvements
- Bug fixes

### Regular Contributors

Take on:
- New features
- Complex bug fixes
- Architecture improvements
- Code reviews

### Core Contributors

Responsibilities may include:
- Design decisions
- Roadmap planning
- Mentoring new contributors
- Release management

## Questions?

If you have questions:
1. Check documentation (README, docs/, onboarding/)
2. Search existing issues and discussions
3. Ask in GitHub Discussions
4. Contact maintainers directly

---

**Thank you for contributing to AI-Assisted IT Manager!**

We appreciate your time and effort in making this project better. Every contribution, no matter how small, helps move the project forward.

**Maintained By**: @KyleC69, @OldSkoolzRoolz  
**Last Updated**: 2025-12-17
