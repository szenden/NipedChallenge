using System.ComponentModel.DataAnnotations;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Application.DTOs
{
    public record CreateClientRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; init; }
        
        [Required(ErrorMessage = "Gender is required")]
        [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender value")]
        public Gender Gender { get; init; }
    }
    
    public record CreateAssessmentRequest
    {
        [Required(ErrorMessage = "Systolic blood pressure is required")]
        [Range(80, 200, ErrorMessage = "Systolic BP must be between 80-200 mmHg")]
        public int SystolicBP { get; init; }
        
        [Required(ErrorMessage = "Diastolic blood pressure is required")]
        [Range(40, 120, ErrorMessage = "Diastolic BP must be between 40-120 mmHg")]
        public int DiastolicBP { get; init; }
        
        [Required(ErrorMessage = "Total cholesterol is required")]
        [Range(100, 400, ErrorMessage = "Cholesterol must be between 100-400 mg/dL")]
        public int CholesterolTotal { get; init; }
        
        [Required(ErrorMessage = "Blood sugar is required")]
        [Range(50, 300, ErrorMessage = "Blood sugar must be between 50-300 mg/dL")]
        public int BloodSugar { get; init; }
        
        [Required(ErrorMessage = "Exercise weekly minutes is required")]
        [Range(0, 1000, ErrorMessage = "Exercise minutes must be between 0-1000 per week")]
        public int ExerciseWeeklyMinutes { get; init; }
        
        [Required(ErrorMessage = "Sleep quality description is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Sleep quality must be between 5 and 500 characters")]
        public string SleepQuality { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "Stress level description is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Stress level must be between 5 and 500 characters")]
        public string StressLevel { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "Diet quality description is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Diet quality must be between 5 and 500 characters")]
        public string DietQuality { get; init; } = string.Empty;
    }

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