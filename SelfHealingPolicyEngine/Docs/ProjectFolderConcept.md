SelfHealingPolicyEngine/
├── engine/
│   ├── Monitor.ps1                 # Core monitoring loop
│   ├── Remediate.ps1               # Executes fixes based on violations
│   ├── PolicyLoader.ps1            # Loads and parses policy packs
│   ├── AuditLogger.ps1             # Writes logs to JSON/Event Viewer
│   └── RoleContext.ps1             # Determines enforcement level by user/machine
├── policies/
│   ├── Defender.yml                # Sample policy: Defender status
│   ├── Firewall.yml                # Sample policy: Firewall rules
│   ├── RegistryLock.yml            # Sample policy: Registry keys
│   └── README.md                   # Docs on writing custom policies
├── onboarding/
│   ├── setup-guide.md              # How to install and configure the engine
│   ├── troubleshooting.md          # Common issues and fixes
│   ├── workspace-presets.ps1       # Optional presets for dev/admin/guest
│   └── policy-pack.json            # Defines which policies to load
├── docs/
│   ├── ARCHITECTURE.md             # System overview and flow diagrams
│   ├── ROLE-MODEL.md               # Role-aware enforcement philosophy
│   └── CHANGELOG.md                # Version history
├── logs/
│   └── audit-log.json              # Output from AuditLogger.ps1
├── tests/
│   ├── TestPolicyLoader.ps1        # Unit test for policy parsing
│   ├── TestRemediation.ps1         # Simulated drift + fix test
│   └── README.md                   # How to run tests
├── SelfHealingPolicyEngine.sln     # Solution file with virtual folders
└── README.md                       # Project intro and philosophy