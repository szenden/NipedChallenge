namespace MedicalAssessment.Domain.ValueObjects
{
    public record SleepQuality(string Description)
    {
        public bool IsOptimal() => Description.Contains("7") || Description.Contains("8") || Description.Contains("9") && Description.Contains("restful");
        
        public bool NeedsAttention() => (Description.Contains("5") || Description.Contains("6")) || Description.Contains("frequent disturbances") || Description.Contains("mild disturbances");
        
        public bool IsSeriousIssue() => Description.Contains("4") || Description.Contains("severe") || Description.Contains("<5");

        public HealthStatus GetHealthStatus()
        {
            if (IsSeriousIssue()) return HealthStatus.SeriousIssue;
            if (NeedsAttention()) return HealthStatus.NeedsAttention;
            return HealthStatus.Optimal;
        }
    }
}