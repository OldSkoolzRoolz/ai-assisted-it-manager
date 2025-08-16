namespace CorePolicyEngine.Services;

public class ComplianceChecker
{
    // Phase 1 stub: trivial compare (string equality)
    public ComplianceResult Compare(string desired, string actual)
    {
        var compliant = string.Equals(desired, actual, StringComparison.Ordinal);
        return new ComplianceResult
        {
            IsCompliant = compliant,
            Differences = compliant ? Array.Empty<string>() : new[] { "Content differs" }
        };
    }
}

public class ComplianceResult
{
    public bool IsCompliant { get; set; }
    public string[] Differences { get; set; } = Array.Empty<string>();
}
