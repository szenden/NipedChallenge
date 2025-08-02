namespace MedicalAssessment.Domain.Entities
{
    public record HealthReport(
        Guid ClientId,
        string ClientName,
        DateTime AssessmentDate,
        List<HealthMetric> Metrics,
        string OverallRisk,
        List<string> Recommendations
    );
}