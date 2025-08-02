namespace MedicalAssessment.Domain.Services
{
    public interface IHealthAssessmentService
    {
        HealthReport GenerateReport(Client client, Assessment assessment);
        HealthStatus EvaluateBloodPressure(BloodPressure bloodPressure);
        HealthStatus EvaluateCholesterol(int totalCholesterol);
        HealthStatus EvaluateBloodSugar(int bloodSugar);
    }
}