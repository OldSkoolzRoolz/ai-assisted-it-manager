namespace Shared.Policies;

public record PolicySetting(string Key, string Value);

public record PolicyDefinition(string Name, IReadOnlyList<PolicySetting> Settings);
