namespace CorePolicyEngine.Services;

public interface IComplianceService
{
    ComplianceResult Compare(string desired, string actual);
}

public sealed class ComplianceService : IComplianceService
{
    public ComplianceResult Compare(string desired, string actual)
    {
        if (desired == actual)
        {
            return new ComplianceResult { IsCompliant = true, Differences = Array.Empty<string>() };
        }

        // Very naive diff: line based presence/absence
        var desiredLines = desired.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var actualLines = actual.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        var diffs = new List<string>();

        foreach (var l in desiredLines.Except(actualLines))
        {
            diffs.Add($"+ {l}");
        }

        foreach (var l in actualLines.Except(desiredLines))
        {
            diffs.Add($"- {l}");
        }

        if (diffs.Count == 0 && desired != actual)
        {
            diffs.Add("Content differs (non-line variation)");
        }

        return new ComplianceResult { IsCompliant = false, Differences = diffs.ToArray() };
    }
}