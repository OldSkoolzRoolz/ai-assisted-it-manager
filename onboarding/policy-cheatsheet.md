# ADMX/ADML Policy Cheatsheet

## Modeling Tips
- Use strongly typed C# models for ADMX categories, policies, and supported values
- Separate domain logic: `PolicyDefinition`, `PolicyInstance`, `PolicyAuditTrail`
- Version policies via semantic tags (`v1.0`, `v1.1-beta`) and changelogs

## Auditability
- Every policy change logs:
  - Timestamp
  - User identity
  - Source (manual, script, AI recommendation)
- Use `PolicyAuditTrail` class to serialize logs to JSON or XML

## Common Pitfalls
- ADML localization mismatches
- Registry key collisions
