namespace MedicalAssessment.Domain.ValueObjects
{
    public record DietQuality(string Assessment)
    {
        public bool IsOptimal() => Assessment.Contains("Balanced") && Assessment.Contains("nutrient-rich");
        
        public bool NeedsAttention() => Assessment.Contains("Processed") || Assessment.Contains("high-sugar");
        
        public bool IsSeriousIssue() => Assessment.Contains("Poor") && Assessment.Contains("deficiencies");

        public HealthStatus GetHealthStatus()
        {
            if (IsSeriousIssue()) return HealthStatus.SeriousIssue;
            if (NeedsAttention()) return HealthStatus.NeedsAttention;
            return HealthStatus.Optimal;
        }
    }
}