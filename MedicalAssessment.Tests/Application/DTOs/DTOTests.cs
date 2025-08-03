using System;
using System.Collections.Generic;
using MedicalAssessment.Application.DTOs;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Application.DTOs;

public class DTOTests
{
    #region CreateClientRequest Tests

    [Fact]
    public void CreateClientRequest_ShouldInitializeCorrectly_WithValidData()
    {
        // Arrange & Act
        var request = new CreateClientRequest("John Doe", new DateTime(1990, 5, 15), Gender.Male);

        // Assert
        Assert.Equal("John Doe", request.Name);
        Assert.Equal(new DateTime(1990, 5, 15), request.DateOfBirth);
        Assert.Equal(Gender.Male, request.Gender);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.Other)]
    public void CreateClientRequest_ShouldSupportAllGenderTypes(Gender gender)
    {
        // Arrange & Act
        var request = new CreateClientRequest("Test User", new DateTime(1990, 1, 1), gender);

        // Assert
        Assert.Equal(gender, request.Gender);
    }

    [Fact]
    public void CreateClientRequest_ShouldAllowEmptyName()
    {
        // Arrange & Act
        var request = new CreateClientRequest("", new DateTime(1990, 1, 1), Gender.Male);

        // Assert
        Assert.Equal("", request.Name);
    }

    [Fact]
    public void CreateClientRequest_ShouldAllowNullName()
    {
        // Arrange & Act
        var request = new CreateClientRequest(null!, new DateTime(1990, 1, 1), Gender.Female);

        // Assert
        Assert.Null(request.Name);
    }

    [Fact]
    public void CreateClientRequest_ShouldHandleFutureDates()
    {
        // Arrange
        var futureDate = DateTime.Today.AddYears(1);

        // Act
        var request = new CreateClientRequest("Future Baby", futureDate, Gender.Other);

        // Assert
        Assert.Equal(futureDate, request.DateOfBirth);
    }

    [Fact]
    public void CreateClientRequest_ShouldHandleVeryOldDates()
    {
        // Arrange
        var veryOldDate = new DateTime(1900, 1, 1);

        // Act
        var request = new CreateClientRequest("Very Old Person", veryOldDate, Gender.Male);

        // Assert
        Assert.Equal(veryOldDate, request.DateOfBirth);
    }

    [Fact]
    public void CreateClientRequest_ShouldBeEquatable_WhenValuesAreSame()
    {
        // Arrange
        var request1 = new CreateClientRequest("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        var request2 = new CreateClientRequest("John Doe", new DateTime(1990, 5, 15), Gender.Male);

        // Act & Assert
        Assert.Equal(request1, request2);
        Assert.True(request1 == request2);
        Assert.False(request1 != request2);
        Assert.Equal(request1.GetHashCode(), request2.GetHashCode());
    }

    [Fact]
    public void CreateClientRequest_ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange
        var request1 = new CreateClientRequest("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        var request2 = new CreateClientRequest("Jane Smith", new DateTime(1990, 5, 15), Gender.Male);

        // Act & Assert
        Assert.NotEqual(request1, request2);
        Assert.False(request1 == request2);
        Assert.True(request1 != request2);
    }

    #endregion

    #region CreateAssessmentRequest Tests

    [Fact]
    public void CreateAssessmentRequest_ShouldInitializeCorrectly_WithValidData()
    {
        // Arrange & Act
        var request = new CreateAssessmentRequest(
            120, 80, 180, 85, 150,
            "7 hours, restful sleep",
            "Low self-reported stress",
            "Balanced, nutrient-rich diet"
        );

        // Assert
        Assert.Equal(120, request.SystolicBP);
        Assert.Equal(80, request.DiastolicBP);
        Assert.Equal(180, request.CholesterolTotal);
        Assert.Equal(85, request.BloodSugar);
        Assert.Equal(150, request.ExerciseWeeklyMinutes);
        Assert.Equal("7 hours, restful sleep", request.SleepQuality);
        Assert.Equal("Low self-reported stress", request.StressLevel);
        Assert.Equal("Balanced, nutrient-rich diet", request.DietQuality);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(200, 120, 300, 200, 500)]
    [InlineData(-10, -5, -100, -50, -20)]
    [InlineData(int.MaxValue, int.MinValue, int.MaxValue, int.MinValue, int.MaxValue)]
    public void CreateAssessmentRequest_ShouldHandleExtremeNumericValues(
        int systolic, int diastolic, int cholesterol, int bloodSugar, int exercise)
    {
        // Arrange & Act
        var request = new CreateAssessmentRequest(
            systolic, diastolic, cholesterol, bloodSugar, exercise,
            "Test sleep", "Test stress", "Test diet"
        );

        // Assert
        Assert.Equal(systolic, request.SystolicBP);
        Assert.Equal(diastolic, request.DiastolicBP);
        Assert.Equal(cholesterol, request.CholesterolTotal);
        Assert.Equal(bloodSugar, request.BloodSugar);
        Assert.Equal(exercise, request.ExerciseWeeklyMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Very long description that might test string handling capabilities and memory usage")]
    public void CreateAssessmentRequest_ShouldHandleVariousStringInputs(string testString)
    {
        // Arrange & Act
        var request = new CreateAssessmentRequest(
            120, 80, 180, 85, 150,
            testString, testString, testString
        );

        // Assert
        Assert.Equal(testString, request.SleepQuality);
        Assert.Equal(testString, request.StressLevel);
        Assert.Equal(testString, request.DietQuality);
    }

    [Fact]
    public void CreateAssessmentRequest_ShouldAllowNullStrings()
    {
        // Arrange & Act
        var request = new CreateAssessmentRequest(
            120, 80, 180, 85, 150,
            null!, null!, null!
        );

        // Assert
        Assert.Null(request.SleepQuality);
        Assert.Null(request.StressLevel);
        Assert.Null(request.DietQuality);
    }

    [Fact]
    public void CreateAssessmentRequest_ShouldBeEquatable_WhenValuesAreSame()
    {
        // Arrange
        var request1 = new CreateAssessmentRequest(120, 80, 180, 85, 150,
            "7 hours, restful sleep", "Low stress", "Balanced diet");
        var request2 = new CreateAssessmentRequest(120, 80, 180, 85, 150,
            "7 hours, restful sleep", "Low stress", "Balanced diet");

        // Act & Assert
        Assert.Equal(request1, request2);
        Assert.True(request1 == request2);
        Assert.Equal(request1.GetHashCode(), request2.GetHashCode());
    }

    #endregion

    #region ClientResponse Tests

    [Fact]
    public void ClientResponse_ShouldInitializeCorrectly_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dateOfBirth = new DateTime(1990, 5, 15);

        // Act
        var response = new ClientResponse(id, "John Doe", dateOfBirth, Gender.Male, 34, 2);

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal("John Doe", response.Name);
        Assert.Equal(dateOfBirth, response.DateOfBirth);
        Assert.Equal(Gender.Male, response.Gender);
        Assert.Equal(34, response.Age);
        Assert.Equal(2, response.AssessmentCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(150)]
    public void ClientResponse_ShouldHandleVariousAges(int age)
    {
        // Arrange & Act
        var response = new ClientResponse(Guid.NewGuid(), "Test", DateTime.Today, Gender.Male, age, 0);

        // Assert
        Assert.Equal(age, response.Age);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-1)] // Edge case - shouldn't happen in practice but DTO should handle it
    public void ClientResponse_ShouldHandleVariousAssessmentCounts(int count)
    {
        // Arrange & Act
        var response = new ClientResponse(Guid.NewGuid(), "Test", DateTime.Today, Gender.Male, 30, count);

        // Assert
        Assert.Equal(count, response.AssessmentCount);
    }

    [Fact]
    public void ClientResponse_ShouldBeEquatable_WhenValuesAreSame()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dateOfBirth = new DateTime(1990, 5, 15);
        var response1 = new ClientResponse(id, "John Doe", dateOfBirth, Gender.Male, 34, 2);
        var response2 = new ClientResponse(id, "John Doe", dateOfBirth, Gender.Male, 34, 2);

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.Equal(response1.GetHashCode(), response2.GetHashCode());
    }

    #endregion

    #region HealthReportResponse Tests

    [Fact]
    public void HealthReportResponse_ShouldInitializeCorrectly_WithValidData()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var assessmentDate = DateTime.UtcNow;
        var metrics = new List<HealthMetricResponse>
        {
            new("Blood Pressure", 120, "Optimal", "Maintain current lifestyle"),
            new("Exercise Minutes", 150, "Optimal", "Great job!")
        };
        var recommendations = new List<string> { "Keep up the good work!" };

        // Act
        var response = new HealthReportResponse(
            clientId, "John Doe", assessmentDate, metrics, "Low Risk", recommendations);

        // Assert
        Assert.Equal(clientId, response.ClientId);
        Assert.Equal("John Doe", response.ClientName);
        Assert.Equal(assessmentDate, response.AssessmentDate);
        Assert.Equal(metrics, response.Metrics);
        Assert.Equal("Low Risk", response.OverallRisk);
        Assert.Equal(recommendations, response.Recommendations);
    }

    [Fact]
    public void HealthReportResponse_ShouldHandleEmptyCollections()
    {
        // Arrange & Act
        var response = new HealthReportResponse(
            Guid.NewGuid(), "Test Client", DateTime.UtcNow,
            new List<HealthMetricResponse>(), "Unknown Risk", new List<string>());

        // Assert
        Assert.Empty(response.Metrics);
        Assert.Empty(response.Recommendations);
    }

    [Fact]
    public void HealthReportResponse_ShouldHandleNullCollections()
    {
        // Arrange & Act
        var response = new HealthReportResponse(
            Guid.NewGuid(), "Test Client", DateTime.UtcNow,
            null!, "Unknown Risk", null!);

        // Assert
        Assert.Null(response.Metrics);
        Assert.Null(response.Recommendations);
    }

    [Fact]
    public void HealthReportResponse_ShouldHandleLargeCollections()
    {
        // Arrange
        var metrics = new List<HealthMetricResponse>();
        var recommendations = new List<string>();

        for (int i = 0; i < 100; i++)
        {
            metrics.Add(new HealthMetricResponse($"Metric {i}", i, "Status", "Recommendation"));
            recommendations.Add($"Recommendation {i}");
        }

        // Act
        var response = new HealthReportResponse(
            Guid.NewGuid(), "Test Client", DateTime.UtcNow,
            metrics, "Test Risk", recommendations);

        // Assert
        Assert.Equal(100, response.Metrics.Count);
        Assert.Equal(100, response.Recommendations.Count);
    }

    #endregion

    #region HealthMetricResponse Tests

    [Fact]
    public void HealthMetricResponse_ShouldInitializeCorrectly_WithValidData()
    {
        // Arrange & Act
        var metric = new HealthMetricResponse("Blood Pressure", 120.5, "Optimal", "Maintain current lifestyle");

        // Assert
        Assert.Equal("Blood Pressure", metric.Name);
        Assert.Equal(120.5, metric.Value);
        Assert.Equal("Optimal", metric.Status);
        Assert.Equal("Maintain current lifestyle", metric.Recommendation);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-123.45)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.NaN)]
    public void HealthMetricResponse_ShouldHandleExtremeDoubleValues(double value)
    {
        // Arrange & Act
        var metric = new HealthMetricResponse("Test Metric", value, "Test Status", "Test Recommendation");

        // Assert
        Assert.Equal(value, metric.Value);
    }

    [Fact]
    public void HealthMetricResponse_ShouldHandleNullStrings()
    {
        // Arrange & Act
        var metric = new HealthMetricResponse(null!, 0, null!, null!);

        // Assert
        Assert.Null(metric.Name);
        Assert.Null(metric.Status);
        Assert.Null(metric.Recommendation);
    }

    [Fact]
    public void HealthMetricResponse_ShouldHandleEmptyStrings()
    {
        // Arrange & Act
        var metric = new HealthMetricResponse("", 0, "", "");

        // Assert
        Assert.Equal("", metric.Name);
        Assert.Equal("", metric.Status);
        Assert.Equal("", metric.Recommendation);
    }

    [Fact]
    public void HealthMetricResponse_ShouldBeEquatable_WhenValuesAreSame()
    {
        // Arrange
        var metric1 = new HealthMetricResponse("Blood Pressure", 120, "Optimal", "Maintain");
        var metric2 = new HealthMetricResponse("Blood Pressure", 120, "Optimal", "Maintain");

        // Act & Assert
        Assert.Equal(metric1, metric2);
        Assert.Equal(metric1.GetHashCode(), metric2.GetHashCode());
    }

    [Fact]
    public void HealthMetricResponse_ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange
        var metric1 = new HealthMetricResponse("Blood Pressure", 120, "Optimal", "Maintain");
        var metric2 = new HealthMetricResponse("Blood Pressure", 130, "High", "Consult physician");

        // Act & Assert
        Assert.NotEqual(metric1, metric2);
    }

    #endregion

    #region Cross-DTO Integration Tests

    [Fact]
    public void DTOs_ShouldWorkTogether_InTypicalWorkflow()
    {
        // Arrange - Create client request
        var createClientRequest = new CreateClientRequest("Integration Test", new DateTime(1990, 1, 1), Gender.Male);

        // Arrange - Create assessment request
        var createAssessmentRequest = new CreateAssessmentRequest(
            120, 80, 180, 85, 150,
            "7 hours, restful sleep",
            "Low self-reported stress",
            "Balanced, nutrient-rich diet"
        );

        // Arrange - Create expected responses
        var clientId = Guid.NewGuid();
        var clientResponse = new ClientResponse(clientId, "Integration Test", new DateTime(1990, 1, 1), Gender.Male, 35, 1);

        var healthMetrics = new List<HealthMetricResponse>
        {
            new("Blood Pressure", 120, "Optimal", "Maintain current lifestyle"),
            new("Exercise Minutes", 150, "Optimal", "Great job maintaining regular exercise!")
        };

        var healthReportResponse = new HealthReportResponse(
            clientId,
            "Integration Test",
            DateTime.UtcNow,
            healthMetrics,
            "Low Risk",
            new List<string> { "Continue maintaining your healthy lifestyle!" }
        );

        // Assert - All DTOs should work together seamlessly
        Assert.Equal("Integration Test", createClientRequest.Name);
        Assert.Equal(120, createAssessmentRequest.SystolicBP);
        Assert.Equal(clientId, clientResponse.Id);
        Assert.Equal(clientId, healthReportResponse.ClientId);
        Assert.Equal(2, healthReportResponse.Metrics.Count);
    }

    [Fact]
    public void DTOs_ShouldHandleComplexScenarios_WithAllNewHealthFactors()
    {
        // Arrange - Assessment with all new health factors in various states
        var assessmentRequest = new CreateAssessmentRequest(
            140, 90,  // High blood pressure
            240, 126, // High cholesterol and blood sugar
            50,       // Low exercise
            "4 hours, severe sleep issues",                    // Poor sleep
            "High chronic stress affecting well-being",       // High stress
            "Poor nutrition with deficiencies"                // Poor diet
        );

        var healthMetrics = new List<HealthMetricResponse>
        {
            new("Blood Pressure", 140, "SeriousIssue", "Consult physician immediately"),
            new("Total Cholesterol", 240, "SeriousIssue", "Consult physician for treatment options"),
            new("Blood Sugar", 126, "SeriousIssue", "Consult physician for diabetes screening"),
            new("Exercise Minutes", 50, "SeriousIssue", "Start with light exercise and gradually increase activity"),
            new("Sleep Quality", 0, "SeriousIssue", "Consult physician about sleep disorders"),
            new("Stress Level", 0, "SeriousIssue", "Seek professional help for stress management"),
            new("Diet Quality", 0, "SeriousIssue", "Consult nutritionist for comprehensive diet plan")
        };

        var healthReport = new HealthReportResponse(
            Guid.NewGuid(),
            "High Risk Client",
            DateTime.UtcNow,
            healthMetrics,
            "High Risk",
            new List<string>
            {
                "Blood Pressure: Consult physician immediately",
                "Multiple serious health issues detected - seek immediate medical attention"
            }
        );

        // Assert - Complex scenario with all health factors
        Assert.Equal(50, assessmentRequest.ExerciseWeeklyMinutes);
        Assert.Contains("severe sleep issues", assessmentRequest.SleepQuality);
        Assert.Contains("chronic stress", assessmentRequest.StressLevel);
        Assert.Contains("deficiencies", assessmentRequest.DietQuality);
        Assert.Equal(7, healthReport.Metrics.Count); // All 7 health metrics
        Assert.Equal("High Risk", healthReport.OverallRisk);
        Assert.All(healthReport.Metrics, m => Assert.Equal("SeriousIssue", m.Status));
    }

    #endregion
}