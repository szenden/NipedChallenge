namespace MedicalAssessment.Domain.ValueObjects
{
    public enum HealthStatus
    {
        Optimal,
        NeedsAttention,
        SeriousIssue
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public record HealthMetric(string Name, double Value, HealthStatus Status, string Recommendation);
}