using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Application.DTOs
{
    public record CreateClientRequest(string Name, DateTime DateOfBirth, Gender Gender);
    
    public record CreateAssessmentRequest(
        int SystolicBP,
        int DiastolicBP,
        int CholesterolTotal,
        int BloodSugar,
        int ExerciseWeeklyMinutes,
        string SleepQuality,
        string StressLevel,
        string DietQuality
    );

    public record ClientResponse(
        Guid Id,
        string Name,
        DateTime DateOfBirth,
        Gender Gender,
        int Age,
        int AssessmentCount
    );

    public record HealthReportResponse(
        Guid ClientId,
        string ClientName,
        DateTime AssessmentDate,
        List<HealthMetricResponse> Metrics,
        string OverallRisk,
        List<string> Recommendations
    );

    public record HealthMetricResponse(
        string Name,
        double Value,
        string Status,
        string Recommendation
    );
}