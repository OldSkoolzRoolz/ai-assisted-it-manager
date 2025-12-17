# Quick Start: Applying Branch Protection Rules

This guide provides step-by-step instructions for repository administrators to apply the branch protection rules documented in this repository.

## Prerequisites

- Repository admin access to `OldSkoolzRoolz/ai-assisted-it-manager`
- GitHub account with appropriate permissions
- Familiarity with GitHub repository settings

## Option 1: Apply via GitHub Web UI (Recommended)

### Step 1: Access Branch Protection Settings

1. Navigate to the repository: https://github.com/OldSkoolzRoolz/ai-assisted-it-manager
2. Click on **Settings** (tab at the top)
3. In the left sidebar, click **Branches**
4. Find the "Branch protection rules" section

### Step 2: Protect Master Branch

Click **Add branch protection rule** and configure:

#### Basic Settings
- **Branch name pattern**: `master`

#### Protect Matching Branches

**Required Pull Request Reviews:**
- ✅ Require a pull request before merging
- ✅ Require approvals: `1`
- ✅ Dismiss stale pull request approvals when new commits are pushed
- ✅ Require review from Code Owners
- ❌ Restrict who can dismiss pull request reviews (leave unchecked)
- ❌ Allow specified actors to bypass required pull requests (leave unchecked)
- ✅ Require approval of the most recent reviewable push

**Required Status Checks:**
- ✅ Require status checks to pass before merging
- ✅ Require branches to be up to date before merging
- **Status checks that are required:**
  - `build-and-test` (from dotnet-ci.yml)
  - `Analyze Code` (from codeql.yml)
  - `validate-doc-versions` (from doc-version-validation.yml)

**Additional Rules:**
- ✅ Require conversation resolution before merging
- ❌ Require signed commits (optional, enable if desired)
- ❌ Require linear history (not required)
- ❌ Require deployments to succeed before merging (not applicable)

**Enforce Settings for Administrators:**
- ✅ Do not allow bypassing the above settings
- ❌ Allow force pushes (keep disabled)
  - ❌ Specify who can force push (keep disabled)
- ❌ Allow deletions (keep disabled)

Click **Create** to save the rule.

### Step 3: Protect Feature Branches (Optional)

Click **Add branch protection rule** again and configure:

#### Basic Settings
- **Branch name pattern**: `feature/*`

#### Protect Matching Branches

**Required Pull Request Reviews:**
- ✅ Require a pull request before merging
- ✅ Require approvals: `1`
- ❌ Dismiss stale pull request approvals when new commits are pushed
- ❌ Require review from Code Owners
- ❌ Require approval of the most recent reviewable push

**Required Status Checks:**
- ✅ Require status checks to pass before merging
- ❌ Require branches to be up to date before merging
- **Status checks that are required:**
  - `build-and-test`

**Additional Rules:**
- ❌ Require conversation resolution before merging
- ❌ Require signed commits
- ❌ Require linear history

**Enforce Settings for Administrators:**
- ❌ Do not allow bypassing the above settings
- ✅ Allow force pushes
- ✅ Allow deletions

Click **Create** to save the rule.

### Step 4: Configure Repository Labels

1. In repository Settings, click **Labels** in the left sidebar
2. Create the following labels (or update existing ones):

#### Priority Labels
| Name | Color | Description |
|------|-------|-------------|
| priority: critical | `#d73a4a` | Critical priority - must be addressed immediately |
| priority: high | `#ff6b6b` | High priority - should be addressed soon |
| priority: medium | `#ffa500` | Medium priority - normal timeline |
| priority: low | `#90ee90` | Low priority - can be addressed when time permits |

#### Type Labels
| Name | Color | Description |
|------|-------|-------------|
| type: bug | `#d73a4a` | Something isn't working |
| type: feature | `#0e8a16` | New feature or request |
| type: enhancement | `#a2eeef` | Improvement to existing functionality |
| type: documentation | `#0075ca` | Documentation changes or additions |
| type: refactoring | `#fbca04` | Code refactoring without changing functionality |
| type: security | `#d73a4a` | Security-related changes or fixes |

#### Status Labels
| Name | Color | Description |
|------|-------|-------------|
| status: in progress | `#fbca04` | Work is currently in progress |
| status: blocked | `#d73a4a` | Blocked by dependencies or other issues |
| status: needs review | `#0e8a16` | Ready for code review |
| status: needs testing | `#ffa500` | Needs testing before merge |

#### Component Labels
| Name | Color | Description |
|------|-------|-------------|
| component: core | `#bfdadc` | Core policy engine |
| component: client | `#bfdadc` | Client application |
| component: security | `#bfdadc` | Security module |
| component: dashboard | `#bfdadc` | Enterprise dashboard |
| component: ci/cd | `#bfdadc` | CI/CD and workflows |

#### Special Labels
| Name | Color | Description |
|------|-------|-------------|
| good first issue | `#7057ff` | Good for newcomers |
| help wanted | `#008672` | Extra attention is needed |

### Step 5: Verify Configuration

1. Create a test branch and PR to verify rules work
2. Check that status checks are required
3. Verify code owner review is required for master
4. Confirm conversation resolution is enforced

## Option 2: Apply via GitHub CLI

If you have GitHub CLI installed, you can apply settings via command line:

```bash
# Install GitHub CLI if not already installed
# Windows: winget install --id GitHub.cli
# Or download from: https://cli.github.com/

# Authenticate
gh auth login

# Apply branch protection for master
gh api repos/OldSkoolzRoolz/ai-assisted-it-manager/branches/master/protection \
  --method PUT \
  --field required_pull_request_reviews[required_approving_review_count]=1 \
  --field required_pull_request_reviews[dismiss_stale_reviews]=true \
  --field required_pull_request_reviews[require_code_owner_reviews]=true \
  --field required_status_checks[strict]=true \
  --field required_status_checks[contexts][]=build-and-test \
  --field required_status_checks[contexts][]=Analyze\ Code \
  --field required_status_checks[contexts][]=validate-doc-versions \
  --field enforce_admins=true \
  --field required_conversation_resolution=true \
  --field allow_force_pushes[enabled]=false \
  --field allow_deletions[enabled]=false
```

## Option 3: Use GitHub App (Advanced)

For automated management, consider:

### Probot Settings App

1. Install the Probot Settings app: https://probot.github.io/apps/settings/
2. The app reads from `.github/settings.yml` and applies settings automatically
3. Any changes to `settings.yml` will be applied on commit

## Verification Checklist

After applying settings, verify:

- [ ] Master branch has protection enabled
- [ ] Pull requests require 1 approval
- [ ] Code owner review is required for master
- [ ] Status checks are required before merging
- [ ] Stale reviews are dismissed on new commits
- [ ] Conversation resolution is required
- [ ] Force pushes are disabled on master
- [ ] Branch deletions are disabled on master
- [ ] All repository labels are created
- [ ] Test PR validates rules correctly

## Testing Branch Protection

1. Create a test branch:
   ```bash
   git checkout -b test/branch-protection
   echo "test" > test.txt
   git add test.txt
   git commit -m "Test branch protection"
   git push origin test/branch-protection
   ```

2. Create a pull request to master

3. Verify:
   - PR requires approval before merging
   - Status checks must pass
   - Code owner review is required
   - Cannot merge until all conditions are met

4. Delete test branch after verification

## Troubleshooting

### Status Checks Not Appearing

If required status checks don't appear:
1. Ensure workflows have run at least once
2. Check workflow names match exactly (case-sensitive)
3. Wait a few minutes for GitHub to register the checks
4. Re-run workflows if needed

### Cannot Apply Branch Protection

If you cannot apply branch protection:
1. Verify you have admin access to the repository
2. Check that the branch exists
3. Ensure you're logged in with the correct account
4. Try using GitHub CLI as an alternative

### Code Owner Review Not Working

If code owner reviews aren't required:
1. Verify CODEOWNERS file exists in `.github/CODEOWNERS`
2. Check CODEOWNERS syntax is correct
3. Ensure code owners have write access
4. Re-save branch protection settings

## Support

For issues or questions:
- Check [Branch Protection Documentation](../docs/BRANCH_PROTECTION.md)
- Review [GitHub Branch Protection Docs](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository)
- Contact repository administrators: @KyleC69, @OldSkoolzRoolz

---

**Last Updated**: 2025-12-17  
**Maintained By**: @KyleC69, @OldSkoolzRoolz
