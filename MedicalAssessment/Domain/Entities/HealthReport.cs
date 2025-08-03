using System;
using System.Collections.Generic;
using MedicalAssessment.Domain.ValueObjects;

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