-- Enterprise Policy & Audit Schema (initial draft)
-- NOTE: For future central RDBMS (placeholder SQL dialect: generic ANSI)

CREATE TABLE Client (
  ClientId TEXT PRIMARY KEY,
  HostName TEXT NOT NULL,
  DomainName TEXT,
  OSVersion TEXT,
  LastHeartbeatUtc TEXT,
  EffectivePolicyHash TEXT,
  CreatedUtc TEXT NOT NULL,
  UpdatedUtc TEXT NOT NULL
);

CREATE TABLE PolicyDefinition (
  PolicyDefId TEXT PRIMARY KEY,
  Name TEXT NOT NULL,
  Category TEXT NOT NULL,
  RegistryRoot TEXT NOT NULL,             -- HKLM or HKCU
  RegistryPath TEXT NOT NULL,             -- path under root
  ValueName TEXT NOT NULL,                -- registry value name
  ValueType TEXT NOT NULL,                -- REG_DWORD|REG_SZ|... or Composite
  Description TEXT,
  IntroducedVersion TEXT,
  Deprecated INTEGER NOT NULL DEFAULT 0,
  CreatedUtc TEXT NOT NULL,
  UpdatedUtc TEXT NOT NULL
);

CREATE TABLE PolicyGroup (
  PolicyGroupId TEXT PRIMARY KEY,
  Name TEXT NOT NULL,
  Description TEXT,
  Priority INTEGER NOT NULL DEFAULT 0,    -- higher overrides lower for conflicts
  CreatedUtc TEXT NOT NULL,
  UpdatedUtc TEXT NOT NULL
);

CREATE TABLE PolicyGroupItem (
  PolicyGroupId TEXT NOT NULL,
  PolicyDefId TEXT NOT NULL,
  Enabled INTEGER NOT NULL,               -- 0/1 (non-tattooing semantics if disabled)
  DesiredValue TEXT,                      -- normalized string form
  Enforced INTEGER NOT NULL DEFAULT 0,    -- future granular enforcement
  PRIMARY KEY (PolicyGroupId, PolicyDefId),
  FOREIGN KEY (PolicyGroupId) REFERENCES PolicyGroup(PolicyGroupId),
  FOREIGN KEY (PolicyDefId) REFERENCES PolicyDefinition(PolicyDefId)
);

CREATE TABLE ClientGroup (
  ClientGroupId TEXT PRIMARY KEY,
  Name TEXT NOT NULL,
  Description TEXT,
  CreatedUtc TEXT NOT NULL,
  UpdatedUtc TEXT NOT NULL
);

CREATE TABLE ClientGroupMember (
  ClientGroupId TEXT NOT NULL,
  ClientId TEXT NOT NULL,
  PRIMARY KEY (ClientGroupId, ClientId),
  FOREIGN KEY (ClientGroupId) REFERENCES ClientGroup(ClientGroupId),
  FOREIGN KEY (ClientId) REFERENCES Client(ClientId)
);

CREATE TABLE PolicyAssignment (
  PolicyGroupId TEXT NOT NULL,
  ClientGroupId TEXT NOT NULL,
  AssignedUtc TEXT NOT NULL,
  AssignedBy TEXT,
  PRIMARY KEY (PolicyGroupId, ClientGroupId),
  FOREIGN KEY (PolicyGroupId) REFERENCES PolicyGroup(PolicyGroupId),
  FOREIGN KEY (ClientGroupId) REFERENCES ClientGroup(ClientGroupId)
);

CREATE TABLE ClientEffectivePolicy (
  ClientId TEXT NOT NULL,
  PolicyDefId TEXT NOT NULL,
  AppliedValue TEXT,
  AppliedEnabled INTEGER NOT NULL,
  SourcePolicyGroupId TEXT,               -- last writer group
  Hash TEXT NOT NULL,
  AppliedUtc TEXT NOT NULL,
  PRIMARY KEY (ClientId, PolicyDefId),
  FOREIGN KEY (ClientId) REFERENCES Client(ClientId),
  FOREIGN KEY (PolicyDefId) REFERENCES PolicyDefinition(PolicyDefId),
  FOREIGN KEY (SourcePolicyGroupId) REFERENCES PolicyGroup(PolicyGroupId)
);

CREATE TABLE DriftEvent (
  DriftEventId TEXT PRIMARY KEY,
  ClientId TEXT NOT NULL,
  PolicyDefId TEXT NOT NULL,
  ExpectedHash TEXT NOT NULL,
  ObservedHash TEXT NOT NULL,
  ExpectedValue TEXT,
  ObservedValue TEXT,
  ExpectedEnabled INTEGER,
  ObservedEnabled INTEGER,
  DetectedUtc TEXT NOT NULL,
  Severity TEXT NOT NULL,                 -- Info|Warning|Critical
  Status TEXT NOT NULL DEFAULT 'New',     -- New|Investigating|Closed|Ignored
  FOREIGN KEY (ClientId) REFERENCES Client(ClientId),
  FOREIGN KEY (PolicyDefId) REFERENCES PolicyDefinition(PolicyDefId)
);

CREATE TABLE AuditEvent (
  AuditId TEXT PRIMARY KEY,
  EventType TEXT NOT NULL,                -- PolicyGroupCreate|PolicyGroupModify|AssignmentAdd|AssignmentRemove|PolicyDefAdd|PolicyDefModify|DriftDetected|DriftClosed|ClientAdded|ClientUpdated
  Actor TEXT,
  ActorType TEXT,                        -- User|System|ClientAgent
  ClientId TEXT,
  PolicyGroupId TEXT,
  PolicyDefId TEXT,
  DetailsJson TEXT,
  CorrelationId TEXT,
  CreatedUtc TEXT NOT NULL,
  FOREIGN KEY (ClientId) REFERENCES Client(ClientId),
  FOREIGN KEY (PolicyGroupId) REFERENCES PolicyGroup(PolicyGroupId),
  FOREIGN KEY (PolicyDefId) REFERENCES PolicyDefinition(PolicyDefId)
);

CREATE INDEX IX_Client_LastHeartbeat ON Client(LastHeartbeatUtc);
CREATE INDEX IX_DriftEvent_Status ON DriftEvent(Status);
CREATE INDEX IX_AuditEvent_EventType ON AuditEvent(EventType);
CREATE INDEX IX_ClientEffectivePolicy_Hash ON ClientEffectivePolicy(Hash);
