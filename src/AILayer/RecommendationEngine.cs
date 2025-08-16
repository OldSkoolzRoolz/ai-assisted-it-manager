namespace AILayer;

public class RecommendationEngine
{
    // Phase 1: basic rule engine placeholder
    public IEnumerable<PolicyRecommendation> GetRecommendations(SystemSnapshot snapshot)
    {
        var list = new List<PolicyRecommendation>();
        if (snapshot.Metrics.TryGetValue("ProcessorCount", out var p) && int.TryParse(p, out var pc) && pc < 4)
        {
            list.Add(new PolicyRecommendation("IncreaseHardwareProfileLogging", "Enable verbose logging on low core machines"));
        }
        // Additional placeholder rules...
        return list;
    }
}

public record PolicyRecommendation(string Key, string Description);
