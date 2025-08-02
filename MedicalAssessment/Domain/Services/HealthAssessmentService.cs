using System.Collections.Generic;
using System.Linq;
using MedicalAssessment.Domain.Entities;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Domain.Services
{
    public class HealthAssessmentService : IHealthAssessmentService
    {
        public HealthReport GenerateReport(Client client, Assessment assessment)
        {
            var metrics = new List<HealthMetric>
            {
                EvaluateBloodPressureMetric(assessment.BloodPressure),
                EvaluateCholesterolMetric(assessment.CholesterolTotal),
                EvaluateBloodSugarMetric(assessment.BloodSugar)
            };

            var overallRisk = CalculateOverallRisk(metrics);
            
            return new HealthReport(
                client.Id,
                client.Name,
                assessment.AssessmentDate,
                metrics,
                overallRisk,
                GenerateRecommendations(metrics)
            );
        }

        public HealthStatus EvaluateBloodPressure(BloodPressure bloodPressure)
        {
            if (bloodPressure.IsOptimal()) return HealthStatus.Optimal;
            if (bloodPressure.Systolic >= 130 || bloodPressure.Diastolic >= 80) return HealthStatus.SeriousIssue;
            return HealthStatus.NeedsAttention;
        }

        public HealthStatus EvaluateCholesterol(int totalCholesterol)
        {
            return totalCholesterol switch
            {
                < 200 => HealthStatus.Optimal,
                >= 200 and < 240 => HealthStatus.NeedsAttention,
                _ => HealthStatus.SeriousIssue
            };
        }

        public HealthStatus EvaluateBloodSugar(int bloodSugar)
        {
            return bloodSugar switch
            {
                >= 70 and <= 99 => HealthStatus.Optimal,
                >= 100 and <= 125 => HealthStatus.NeedsAttention,
                _ => HealthStatus.SeriousIssue
            };
        }

        private HealthMetric EvaluateBloodPressureMetric(BloodPressure bp)
        {
            var status = EvaluateBloodPressure(bp);
            var recommendation = status switch
            {
                HealthStatus.Optimal => "Maintain current lifestyle",
                HealthStatus.NeedsAttention => "Monitor regularly and consider lifestyle changes",
                HealthStatus.SeriousIssue => "Consult physician immediately",
                _ => "Unknown"
            };
            
            return new HealthMetric("Blood Pressure", bp.Systolic, status, recommendation);
        }

        private HealthMetric EvaluateCholesterolMetric(int cholesterol)
        {
            var status = EvaluateCholesterol(cholesterol);
            var recommendation = status switch
            {
                HealthStatus.Optimal => "Continue healthy diet",
                HealthStatus.NeedsAttention => "Reduce saturated fats, increase exercise",
                HealthStatus.SeriousIssue => "Consult physician for treatment options",
                _ => "Unknown"
            };
            
            return new HealthMetric("Total Cholesterol", cholesterol, status, recommendation);
        }

        private HealthMetric EvaluateBloodSugarMetric(int bloodSugar)
        {
            var status = EvaluateBloodSugar(bloodSugar);
            var recommendation = status switch
            {
                HealthStatus.Optimal => "Maintain current diet and exercise",
                HealthStatus.NeedsAttention => "Monitor carbohydrate intake",
                HealthStatus.SeriousIssue => "Consult physician for diabetes screening",
                _ => "Unknown"
            };
            
            return new HealthMetric("Blood Sugar", bloodSugar, status, recommendation);
        }

        private string CalculateOverallRisk(List<HealthMetric> metrics)
        {
            var seriousCount = metrics.Count(m => m.Status == HealthStatus.SeriousIssue);
            var attentionCount = metrics.Count(m => m.Status == HealthStatus.NeedsAttention);

            return seriousCount switch
            {
                >= 2 => "High Risk",
                1 => "Moderate Risk",
                0 when attentionCount >= 2 => "Moderate Risk",
                0 when attentionCount == 1 => "Low Risk",
                _ => "Low Risk"
            };
        }

        private List<string> GenerateRecommendations(List<HealthMetric> metrics)
        {
            var recommendations = new List<string>();
            
            foreach (var metric in metrics.Where(m => m.Status != HealthStatus.Optimal))
            {
                recommendations.Add($"{metric.Name}: {metric.Recommendation}");
            }

            if (!recommendations.Any())
            {
                recommendations.Add("Continue maintaining your healthy lifestyle!");
            }

            return recommendations;
        }
    }
}