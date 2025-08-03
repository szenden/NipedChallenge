namespace MedicalAssessment.Domain.ValueObjects
{
    public record StressLevel(string Assessment)
    {
        public bool IsOptimal() => Assessment.Contains("Low") && Assessment.Contains("stress");
        
        public bool NeedsAttention() => Assessment.Contains("Moderate") && Assessment.Contains("stress");
        
        public bool IsSeriousIssue() => Assessment.Contains("High") && Assessment.Contains("chronic") && Assessment.Contains("affecting");

        public HealthStatus GetHealthStatus()
        {
            if (IsSeriousIssue()) return HealthStatus.SeriousIssue;
            if (NeedsAttention()) return HealthStatus.NeedsAttention;
            return HealthStatus.Optimal;
        }
    }
}