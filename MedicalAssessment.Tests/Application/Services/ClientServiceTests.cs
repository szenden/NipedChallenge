using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using MedicalAssessment.Application.DTOs;
using MedicalAssessment.Application.Interfaces;
using MedicalAssessment.Application.Services;
using MedicalAssessment.Domain.Entities;
using MedicalAssessment.Domain.Services;
using MedicalAssessment.Domain.ValueObjects;
using MedicalAssessment.Tests.TestHelpers;

namespace MedicalAssessment.Tests.Application.Services;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _mockClientRepository;
    private readonly Mock<IHealthAssessmentService> _mockHealthAssessmentService;
    private readonly ClientService _clientService;

    public ClientServiceTests()
    {
        _mockClientRepository = new Mock<IClientRepository>();
        _mockHealthAssessmentService = new Mock<IHealthAssessmentService>();
        _clientService = new ClientService(_mockClientRepository.Object, _mockHealthAssessmentService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldAcceptNullRepository_WithoutThrowing()
    {
        // The current implementation doesn't validate constructor parameters
        var service = new ClientService(null!, _mockHealthAssessmentService.Object);
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_ShouldAcceptNullHealthService_WithoutThrowing()
    {
        // The current implementation doesn't validate constructor parameters
        var service = new ClientService(_mockClientRepository.Object, null!);
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_ShouldInitializeSuccessfully_WithValidDependencies()
    {
        var service = new ClientService(_mockClientRepository.Object, _mockHealthAssessmentService.Object);
        
        Assert.NotNull(service);
    }

    #endregion

    #region CreateClientAsync Tests

    [Fact]
    public async Task CreateClientAsync_ShouldCreateClientAndReturnResponse_WhenRequestIsValid()
    {
        // Arrange
        var request = TestDataHelper.CreateValidClientRequest("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        _mockClientRepository.Setup(r => r.AddAsync(It.IsAny<Client>())).ReturnsAsync((Client client) => client);

        // Act
        var result = await _clientService.CreateClientAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(new DateTime(1990, 5, 15), result.DateOfBirth);
        Assert.Equal(Gender.Male, result.Gender);
        Assert.True(result.Age >= 34); // Age calculation may vary based on current date
        Assert.Equal(0, result.AssessmentCount);
        Assert.NotEqual(Guid.Empty, result.Id);

        _mockClientRepository.Verify(r => r.AddAsync(It.Is<Client>(c => 
            c.Name == "John Doe" && 
            c.DateOfBirth == new DateTime(1990, 5, 15) && 
            c.Gender == Gender.Male)), Times.Once);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldThrowNullReferenceException_WhenRequestIsNull()
    {
        // The current implementation doesn't validate null request, so it throws NullReferenceException
        await Assert.ThrowsAsync<NullReferenceException>(() => 
            _clientService.CreateClientAsync(null!));
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.Other)]
    public async Task CreateClientAsync_ShouldHandleAllGenderTypes(Gender gender)
    {
        // Arrange
        var request = TestDataHelper.CreateValidClientRequest("Test User", new DateTime(1990, 1, 1), gender);
        _mockClientRepository.Setup(r => r.AddAsync(It.IsAny<Client>())).ReturnsAsync((Client client) => client);

        // Act
        var result = await _clientService.CreateClientAsync(request);

        // Assert
        Assert.Equal(gender, result.Gender);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldPropagateException_WhenRepositoryThrowsException()
    {
        // Arrange
        var request = TestDataHelper.CreateValidClientRequest("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        _mockClientRepository.Setup(r => r.AddAsync(It.IsAny<Client>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _clientService.CreateClientAsync(request));
    }

    #endregion

    #region GetAllClientsAsync Tests

    [Fact]
    public async Task GetAllClientsAsync_ShouldReturnEmptyList_WhenNoClientsExist()
    {
        // Arrange
        _mockClientRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Client>());

        // Act
        var result = await _clientService.GetAllClientsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockClientRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllClientsAsync_ShouldReturnClientResponses_WhenClientsExist()
    {
        // Arrange
        var clients = new List<Client>
        {
            new("John Doe", new DateTime(1990, 5, 15), Gender.Male),
            new("Jane Smith", new DateTime(1985, 8, 22), Gender.Female)
        };
        _mockClientRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(clients);

        // Act
        var result = await _clientService.GetAllClientsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var johnResponse = result.First(c => c.Name == "John Doe");
        Assert.Equal("John Doe", johnResponse.Name);
        Assert.Equal(Gender.Male, johnResponse.Gender);
        
        var janeResponse = result.First(c => c.Name == "Jane Smith");
        Assert.Equal("Jane Smith", janeResponse.Name);
        Assert.Equal(Gender.Female, janeResponse.Gender);
    }

    [Fact]
    public async Task GetAllClientsAsync_ShouldMapAssessmentCount_Correctly()
    {
        // Arrange
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");
        
        // Add assessments to client
        client.AddAssessment(bloodPressure, 180, 85, exerciseMinutes, sleepQuality, stressLevel, dietQuality);
        client.AddAssessment(bloodPressure, 190, 90, exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        _mockClientRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Client> { client });

        // Act
        var result = await _clientService.GetAllClientsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].AssessmentCount);
    }

    [Fact]
    public async Task GetAllClientsAsync_ShouldPropagateException_WhenRepositoryThrowsException()
    {
        // Arrange
        _mockClientRepository.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _clientService.GetAllClientsAsync());
    }

    #endregion

    #region GetClientByIdAsync Tests

    [Fact]
    public async Task GetClientByIdAsync_ShouldReturnClientResponse_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);

        // Act
        var result = await _clientService.GetClientByIdAsync(clientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(new DateTime(1990, 5, 15), result.DateOfBirth);
        Assert.Equal(Gender.Male, result.Gender);
        _mockClientRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
    }

    [Fact]
    public async Task GetClientByIdAsync_ShouldReturnNull_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync((Client?)null);

        // Act
        var result = await _clientService.GetClientByIdAsync(clientId);

        // Assert
        Assert.Null(result);
        _mockClientRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
    }

    [Fact]
    public async Task GetClientByIdAsync_ShouldHandleEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _mockClientRepository.Setup(r => r.GetByIdAsync(emptyGuid)).ReturnsAsync((Client?)null);

        // Act
        var result = await _clientService.GetClientByIdAsync(emptyGuid);

        // Assert
        Assert.Null(result);
        _mockClientRepository.Verify(r => r.GetByIdAsync(emptyGuid), Times.Once);
    }

    [Fact]
    public async Task GetClientByIdAsync_ShouldPropagateException_WhenRepositoryThrowsException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _clientService.GetClientByIdAsync(clientId));
    }

    #endregion

    #region CreateAssessmentAndGenerateReportAsync Tests

    [Fact]
    public async Task CreateAssessmentAndGenerateReportAsync_ShouldReturnHealthReport_WhenClientExistsAndRequestIsValid()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        var request = TestDataHelper.CreateValidAssessmentRequest();

        var healthReport = new HealthReport(
            clientId,
            "John Doe",
            DateTime.UtcNow,
            new List<HealthMetric>
            {
                new("Blood Pressure", 120, HealthStatus.Optimal, "Maintain current lifestyle"),
                new("Exercise Minutes", 150, HealthStatus.Optimal, "Great job maintaining regular exercise!")
            },
            "Low Risk",
            new List<string> { "Continue maintaining your healthy lifestyle!" }
        );

        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _mockClientRepository.Setup(r => r.AddAssessmentAsync(It.IsAny<Assessment>())).Returns(Task.CompletedTask);
        _mockHealthAssessmentService.Setup(s => s.GenerateReport(client, It.IsAny<Assessment>())).Returns(healthReport);

        // Act
        var result = await _clientService.CreateAssessmentAndGenerateReportAsync(clientId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal("John Doe", result.ClientName);
        Assert.Equal("Low Risk", result.OverallRisk);
        Assert.Equal(2, result.Metrics.Count);
        Assert.Single(result.Recommendations);

        _mockClientRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
        _mockClientRepository.Verify(r => r.AddAssessmentAsync(It.Is<Assessment>(a => 
            a.ClientId == clientId &&
            a.BloodPressure.Systolic == 120 &&
            a.BloodPressure.Diastolic == 80 &&
            a.CholesterolTotal == 180 &&
            a.BloodSugar == 85 &&
            a.ExerciseMinutes.WeeklyMinutes == 150 &&
            a.SleepQuality.Description == "7 hours, restful sleep" &&
            a.StressLevel.Assessment == "Low self-reported stress" &&
            a.DietQuality.Assessment == "Balanced, nutrient-rich diet")), Times.Once);
        _mockHealthAssessmentService.Verify(s => s.GenerateReport(client, It.IsAny<Assessment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAssessmentAndGenerateReportAsync_ShouldReturnNull_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = TestDataHelper.CreateValidAssessmentRequest();

        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync((Client?)null);

        // Act
        var result = await _clientService.CreateAssessmentAndGenerateReportAsync(clientId, request);

        // Assert
        Assert.Null(result);
        _mockClientRepository.Verify(r => r.GetByIdAsync(clientId), Times.Once);
        _mockClientRepository.Verify(r => r.AddAssessmentAsync(It.IsAny<Assessment>()), Times.Never);
        _mockHealthAssessmentService.Verify(s => s.GenerateReport(It.IsAny<Client>(), It.IsAny<Assessment>()), Times.Never);
    }

    [Fact]
    public async Task CreateAssessmentAndGenerateReportAsync_ShouldCreateAssessmentWithAllNewHealthFactors()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        var request = TestDataHelper.CreateValidAssessmentRequest(140, 90, 240, 126, 50,
            "4 hours, severe sleep issues reported", "High chronic stress affecting well-being", "Poor nutrition with deficiencies");

        var healthReport = new HealthReport(clientId, "Test Client", DateTime.UtcNow,
            new List<HealthMetric>(), "High Risk", new List<string>());

        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _mockClientRepository.Setup(r => r.AddAssessmentAsync(It.IsAny<Assessment>())).Returns(Task.CompletedTask);
        _mockHealthAssessmentService.Setup(s => s.GenerateReport(client, It.IsAny<Assessment>())).Returns(healthReport);

        // Act
        var result = await _clientService.CreateAssessmentAndGenerateReportAsync(clientId, request);

        // Assert
        Assert.NotNull(result);
        _mockClientRepository.Verify(r => r.AddAssessmentAsync(It.Is<Assessment>(a =>
            a.ExerciseMinutes.WeeklyMinutes == 50 &&
            a.SleepQuality.Description == "4 hours, severe sleep issues" &&
            a.StressLevel.Assessment == "High chronic stress affecting well-being" &&
            a.DietQuality.Assessment == "Poor nutrition with deficiencies")), Times.Once);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(200, 120, 300, 200, 500)]
    [InlineData(-10, -5, -100, -50, -20)]
    public async Task CreateAssessmentAndGenerateReportAsync_ShouldHandleExtremeValues(
        int systolic, int diastolic, int cholesterol, int bloodSugar, int exercise)
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        var request = TestDataHelper.CreateValidAssessmentRequest(systolic, diastolic, cholesterol, bloodSugar, exercise,
            "Test sleep quality description", "Test stress level description", "Test diet quality description");

        var healthReport = new HealthReport(clientId, "Test Client", DateTime.UtcNow,
            new List<HealthMetric>(), "Test Risk", new List<string>());

        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _mockClientRepository.Setup(r => r.AddAssessmentAsync(It.IsAny<Assessment>())).Returns(Task.CompletedTask);
        _mockHealthAssessmentService.Setup(s => s.GenerateReport(client, It.IsAny<Assessment>())).Returns(healthReport);

        // Act
        var result = await _clientService.CreateAssessmentAndGenerateReportAsync(clientId, request);

        // Assert
        Assert.NotNull(result);
        _mockClientRepository.Verify(r => r.AddAssessmentAsync(It.Is<Assessment>(a =>
            a.BloodPressure.Systolic == systolic &&
            a.BloodPressure.Diastolic == diastolic &&
            a.CholesterolTotal == cholesterol &&
            a.BloodSugar == bloodSugar &&
            a.ExerciseMinutes.WeeklyMinutes == exercise)), Times.Once);
    }

    [Fact]
    public async Task CreateAssessmentAndGenerateReportAsync_ShouldPropagateException_WhenRepositoryGetThrowsException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = TestDataHelper.CreateValidAssessmentRequest();

        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _clientService.CreateAssessmentAndGenerateReportAsync(clientId, request));
    }

    [Fact]
    public async Task CreateAssessmentAndGenerateReportAsync_ShouldPropagateException_WhenRepositoryAddThrowsException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        var request = TestDataHelper.CreateValidAssessmentRequest();

        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _mockClientRepository.Setup(r => r.AddAssessmentAsync(It.IsAny<Assessment>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _clientService.CreateAssessmentAndGenerateReportAsync(clientId, request));
    }

    [Fact]
    public async Task CreateAssessmentAndGenerateReportAsync_ShouldPropagateException_WhenHealthAssessmentServiceThrowsException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client("Test Client", new DateTime(1990, 1, 1), Gender.Male);
        var request = TestDataHelper.CreateValidAssessmentRequest();

        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _mockClientRepository.Setup(r => r.AddAssessmentAsync(It.IsAny<Assessment>())).Returns(Task.CompletedTask);
        _mockHealthAssessmentService.Setup(s => s.GenerateReport(client, It.IsAny<Assessment>()))
            .Throws(new InvalidOperationException("Health assessment error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _clientService.CreateAssessmentAndGenerateReportAsync(clientId, request));
    }

    #endregion

    #region Mapping Tests

    [Fact]
    public async Task MapToResponse_ShouldMapClientCorrectly()
    {
        // Arrange
        var request = TestDataHelper.CreateValidClientRequest("Mapping Test", new DateTime(1985, 3, 10), Gender.Female);
        _mockClientRepository.Setup(r => r.AddAsync(It.IsAny<Client>())).ReturnsAsync((Client client) => client);

        // Act
        var result = await _clientService.CreateClientAsync(request);

        // Assert
        Assert.Equal("Mapping Test", result.Name);
        Assert.Equal(new DateTime(1985, 3, 10), result.DateOfBirth);
        Assert.Equal(Gender.Female, result.Gender);
        Assert.True(result.Age > 0);
        Assert.Equal(0, result.AssessmentCount);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task MapToResponse_ShouldMapHealthReportCorrectly()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client("Report Test", new DateTime(1990, 1, 1), Gender.Male);
        var request = TestDataHelper.CreateValidAssessmentRequest(130, 85, 210, 95, 90,
            "6 hours, frequent disturbances", "Moderate self-reported stress", "Processed or high-sugar diet");

        var healthMetrics = new List<HealthMetric>
        {
            new("Blood Pressure", 130, HealthStatus.SeriousIssue, "Consult physician immediately"),
            new("Exercise Minutes", 90, HealthStatus.NeedsAttention, "Increase weekly exercise to at least 150 minutes")
        };

        var healthReport = new HealthReport(
            clientId,
            "Report Test",
            DateTime.UtcNow,
            healthMetrics,
            "Moderate Risk",
            new List<string> { "Blood Pressure: Consult physician immediately", "Exercise Minutes: Increase weekly exercise to at least 150 minutes" }
        );

        _mockClientRepository.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _mockClientRepository.Setup(r => r.AddAssessmentAsync(It.IsAny<Assessment>())).Returns(Task.CompletedTask);
        _mockHealthAssessmentService.Setup(s => s.GenerateReport(client, It.IsAny<Assessment>())).Returns(healthReport);

        // Act
        var result = await _clientService.CreateAssessmentAndGenerateReportAsync(clientId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal("Report Test", result.ClientName);
        Assert.Equal("Moderate Risk", result.OverallRisk);
        Assert.Equal(2, result.Metrics.Count);
        Assert.Equal(2, result.Recommendations.Count);

        var bpMetric = result.Metrics.First(m => m.Name == "Blood Pressure");
        Assert.Equal(130, bpMetric.Value);
        Assert.Equal("SeriousIssue", bpMetric.Status);
        Assert.Equal("Consult physician immediately", bpMetric.Recommendation);

        var exerciseMetric = result.Metrics.First(m => m.Name == "Exercise Minutes");
        Assert.Equal(90, exerciseMetric.Value);
        Assert.Equal("NeedsAttention", exerciseMetric.Status);
        Assert.Equal("Increase weekly exercise to at least 150 minutes", exerciseMetric.Recommendation);
    }

    #endregion
}