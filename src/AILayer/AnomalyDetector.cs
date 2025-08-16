namespace AILayer;

public class AnomalyDetector
{
    // Placeholder anomaly scoring
    public AnomalyResult Score(SystemSnapshot snapshot)
    {
        var score = snapshot.Metrics.Count > 0 ? 0.01 : 0.0; // trivial
        return new AnomalyResult(score, score > 0.8);
    }
}

public record AnomalyResult(double Score, bool IsAnomalous);
