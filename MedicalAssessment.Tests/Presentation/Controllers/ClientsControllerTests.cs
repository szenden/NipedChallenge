using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MedicalAssessment.API.Controllers;
using MedicalAssessment.Application.DTOs;
using MedicalAssessment.Application.Interfaces;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Presentation.Controllers;

public class ClientsControllerTests
{
    private readonly Mock<IClientService> _mockClientService;
    private readonly ClientsController _controller;

    public ClientsControllerTests()
    {
        _mockClientService = new Mock<IClientService>();
        _controller = new ClientsController(_mockClientService.Object);
    }

    #region GetAllClients Tests

    [Fact]
    public async Task GetAllClients_ShouldReturnOkWithClientList_WhenClientsExist()
    {
        // Arrange
        var clients = new List<ClientResponse>
        {
            new(Guid.NewGuid(), "John Doe", new DateTime(1990, 5, 15), Gender.Male, 34, 2),
            new(Guid.NewGuid(), "Jane Smith", new DateTime(1985, 8, 22), Gender.Female, 39, 1)
        };
        _mockClientService.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(clients);

        // Act
        var result = await _controller.GetAllClients();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedClients = Assert.IsType<List<ClientResponse>>(okResult.Value);
        Assert.Equal(2, returnedClients.Count);
        Assert.Equal(clients[0].Id, returnedClients[0].Id);
        Assert.Equal(clients[1].Name, returnedClients[1].Name);
    }

    [Fact]
    public async Task GetAllClients_ShouldReturnOkWithEmptyList_WhenNoClientsExist()
    {
        // Arrange
        var emptyClients = new List<ClientResponse>();
        _mockClientService.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(emptyClients);

        // Act
        var result = await _controller.GetAllClients();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedClients = Assert.IsType<List<ClientResponse>>(okResult.Value);
        Assert.Empty(returnedClients);
    }

    [Fact]
    public async Task GetAllClients_ShouldCallServiceOnce()
    {
        // Arrange
        _mockClientService.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(new List<ClientResponse>());

        // Act
        await _controller.GetAllClients();

        // Assert
        _mockClientService.Verify(s => s.GetAllClientsAsync(), Times.Once);
    }

    #endregion

    #region GetClient Tests

    [Fact]
    public async Task GetClient_ShouldReturnOkWithClient_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new ClientResponse(clientId, "John Doe", new DateTime(1990, 5, 15), Gender.Male, 34, 2);
        _mockClientService.Setup(s => s.GetClientByIdAsync(clientId)).ReturnsAsync(client);

        // Act
        var result = await _controller.GetClient(clientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedClient = Assert.IsType<ClientResponse>(okResult.Value);
        Assert.Equal(clientId, returnedClient.Id);
        Assert.Equal("John Doe", returnedClient.Name);
    }

    [Fact]
    public async Task GetClient_ShouldReturnNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _mockClientService.Setup(s => s.GetClientByIdAsync(clientId)).ReturnsAsync((ClientResponse?)null);

        // Act
        var result = await _controller.GetClient(clientId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetClient_ShouldCallServiceWithCorrectId()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _mockClientService.Setup(s => s.GetClientByIdAsync(clientId)).ReturnsAsync((ClientResponse?)null);

        // Act
        await _controller.GetClient(clientId);

        // Assert
        _mockClientService.Verify(s => s.GetClientByIdAsync(clientId), Times.Once);
    }

    [Fact]
    public async Task GetClient_ShouldHandleEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _mockClientService.Setup(s => s.GetClientByIdAsync(emptyGuid)).ReturnsAsync((ClientResponse?)null);

        // Act
        var result = await _controller.GetClient(emptyGuid);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockClientService.Verify(s => s.GetClientByIdAsync(emptyGuid), Times.Once);
    }

    #endregion

    #region CreateClient Tests

    [Fact]
    public async Task CreateClient_ShouldReturnCreatedAtAction_WhenClientIsCreatedSuccessfully()
    {
        // Arrange
        var request = new CreateClientRequest("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        var createdClient = new ClientResponse(Guid.NewGuid(), "John Doe", new DateTime(1990, 5, 15), Gender.Male, 34, 0);
        _mockClientService.Setup(s => s.CreateClientAsync(request)).ReturnsAsync(createdClient);

        // Act
        var result = await _controller.CreateClient(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ClientsController.GetClient), createdResult.ActionName);
        Assert.NotNull(createdResult.RouteValues);
        Assert.True(createdResult.RouteValues.ContainsKey("id"));
        Assert.Equal(createdClient.Id, createdResult.RouteValues["id"]);
        
        var returnedClient = Assert.IsType<ClientResponse>(createdResult.Value);
        Assert.Equal(createdClient.Id, returnedClient.Id);
        Assert.Equal("John Doe", returnedClient.Name);
    }

    [Fact]
    public async Task CreateClient_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
    {
        // Arrange
        var request = new CreateClientRequest("", new DateTime(1990, 5, 15), Gender.Male);
        var exceptionMessage = "Name cannot be empty";
        _mockClientService.Setup(s => s.CreateClientAsync(request))
            .ThrowsAsync(new ArgumentException(exceptionMessage));

        // Act
        var result = await _controller.CreateClient(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(exceptionMessage, badRequestResult.Value);
    }

    [Fact]
    public async Task CreateClient_ShouldCallServiceWithCorrectRequest()
    {
        // Arrange
        var request = new CreateClientRequest("Jane Smith", new DateTime(1985, 8, 22), Gender.Female);
        var createdClient = new ClientResponse(Guid.NewGuid(), "Jane Smith", new DateTime(1985, 8, 22), Gender.Female, 39, 0);
        _mockClientService.Setup(s => s.CreateClientAsync(request)).ReturnsAsync(createdClient);

        // Act
        await _controller.CreateClient(request);

        // Assert
        _mockClientService.Verify(s => s.CreateClientAsync(request), Times.Once);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.Other)]
    public async Task CreateClient_ShouldHandleAllGenderTypes(Gender gender)
    {
        // Arrange
        var request = new CreateClientRequest("Test User", new DateTime(1990, 1, 1), gender);
        var createdClient = new ClientResponse(Guid.NewGuid(), "Test User", new DateTime(1990, 1, 1), gender, 35, 0);
        _mockClientService.Setup(s => s.CreateClientAsync(request)).ReturnsAsync(createdClient);

        // Act
        var result = await _controller.CreateClient(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedClient = Assert.IsType<ClientResponse>(createdResult.Value);
        Assert.Equal(gender, returnedClient.Gender);
    }

    [Fact]
    public async Task CreateClient_ShouldHandleFutureDateOfBirth()
    {
        // Arrange
        var futureDate = DateTime.Today.AddYears(1);
        var request = new CreateClientRequest("Future Baby", futureDate, Gender.Other);
        var createdClient = new ClientResponse(Guid.NewGuid(), "Future Baby", futureDate, Gender.Other, -1, 0);
        _mockClientService.Setup(s => s.CreateClientAsync(request)).ReturnsAsync(createdClient);

        // Act
        var result = await _controller.CreateClient(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedClient = Assert.IsType<ClientResponse>(createdResult.Value);
        Assert.Equal(futureDate, returnedClient.DateOfBirth);
    }

    #endregion

    #region CreateAssessment Tests

    [Fact]
    public async Task CreateAssessment_ShouldReturnOkWithHealthReport_WhenAssessmentIsCreatedSuccessfully()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(120, 80, 180, 85, 150, 
            "7 hours, restful sleep", "Low self-reported stress", "Balanced, nutrient-rich diet");
        
        var healthReport = new HealthReportResponse(
            clientId, 
            "John Doe", 
            DateTime.UtcNow,
            new List<HealthMetricResponse>
            {
                new("Blood Pressure", 120, "Optimal", "Maintain current lifestyle"),
                new("Exercise Minutes", 150, "Optimal", "Great job maintaining regular exercise!")
            },
            "Low Risk",
            new List<string> { "Continue maintaining your healthy lifestyle!" }
        );

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ReturnsAsync(healthReport);

        // Act
        var result = await _controller.CreateAssessment(clientId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedReport = Assert.IsType<HealthReportResponse>(okResult.Value);
        Assert.Equal(clientId, returnedReport.ClientId);
        Assert.Equal("John Doe", returnedReport.ClientName);
        Assert.Equal("Low Risk", returnedReport.OverallRisk);
        Assert.Equal(2, returnedReport.Metrics.Count);
    }

    [Fact]
    public async Task CreateAssessment_ShouldReturnNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(120, 80, 180, 85, 150,
            "7 hours, restful sleep", "Low self-reported stress", "Balanced, nutrient-rich diet");

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ReturnsAsync((HealthReportResponse?)null);

        // Act
        var result = await _controller.CreateAssessment(clientId, request);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAssessment_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(120, 80, 180, 85, -10,
            "Invalid sleep", "Invalid stress", "Invalid diet");
        var exceptionMessage = "Invalid assessment data";

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ThrowsAsync(new ArgumentException(exceptionMessage));

        // Act
        var result = await _controller.CreateAssessment(clientId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(exceptionMessage, badRequestResult.Value);
    }

    [Fact]
    public async Task CreateAssessment_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(140, 90, 240, 126, 50,
            "4 hours, severe sleep issues", "High chronic stress affecting well-being", "Poor nutrition with deficiencies");

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ReturnsAsync((HealthReportResponse?)null);

        // Act
        await _controller.CreateAssessment(clientId, request);

        // Assert
        _mockClientService.Verify(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request), Times.Once);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0)] // All minimum values
    [InlineData(200, 120, 300, 200, 500)] // All high values
    [InlineData(-10, -5, -100, -50, -20)] // Negative values
    public async Task CreateAssessment_ShouldHandleVariousNumericValues(int systolic, int diastolic, int cholesterol, int bloodSugar, int exercise)
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(systolic, diastolic, cholesterol, bloodSugar, exercise,
            "Test sleep", "Test stress", "Test diet");

        var healthReport = new HealthReportResponse(clientId, "Test Client", DateTime.UtcNow,
            new List<HealthMetricResponse>(), "Test Risk", new List<string>());

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ReturnsAsync(healthReport);

        // Act
        var result = await _controller.CreateAssessment(clientId, request);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        _mockClientService.Verify(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Very long sleep description that might test string handling capabilities")]
    public async Task CreateAssessment_ShouldHandleVariousStringValues(string testString)
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(120, 80, 180, 85, 150,
            testString, testString, testString);

        var healthReport = new HealthReportResponse(clientId, "Test Client", DateTime.UtcNow,
            new List<HealthMetricResponse>(), "Test Risk", new List<string>());

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ReturnsAsync(healthReport);

        // Act
        var result = await _controller.CreateAssessment(clientId, request);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateAssessment_ShouldHandleEmptyGuidForClientId()
    {
        // Arrange
        var clientId = Guid.Empty;
        var request = new CreateAssessmentRequest(120, 80, 180, 85, 150,
            "7 hours, restful sleep", "Low self-reported stress", "Balanced, nutrient-rich diet");

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ReturnsAsync((HealthReportResponse?)null);

        // Act
        var result = await _controller.CreateAssessment(clientId, request);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockClientService.Verify(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request), Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task GetAllClients_ShouldPropagateException_WhenServiceThrowsNonArgumentException()
    {
        // Arrange
        _mockClientService.Setup(s => s.GetAllClientsAsync())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAllClients());
    }

    [Fact]
    public async Task GetClient_ShouldPropagateException_WhenServiceThrowsNonArgumentException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _mockClientService.Setup(s => s.GetClientByIdAsync(clientId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetClient(clientId));
    }

    [Fact]
    public async Task CreateClient_ShouldPropagateException_WhenServiceThrowsNonArgumentException()
    {
        // Arrange
        var request = new CreateClientRequest("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        _mockClientService.Setup(s => s.CreateClientAsync(request))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateClient(request));
    }

    [Fact]
    public async Task CreateAssessment_ShouldPropagateException_WhenServiceThrowsNonArgumentException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(120, 80, 180, 85, 150,
            "7 hours, restful sleep", "Low self-reported stress", "Balanced, nutrient-rich diet");
        
        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateAssessment(clientId, request));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CreateClient_ThenGetClient_ShouldReturnSameClient()
    {
        // Arrange
        var request = new CreateClientRequest("Integration Test", new DateTime(1990, 1, 1), Gender.Male);
        var createdClient = new ClientResponse(Guid.NewGuid(), "Integration Test", new DateTime(1990, 1, 1), Gender.Male, 35, 0);
        
        _mockClientService.Setup(s => s.CreateClientAsync(request)).ReturnsAsync(createdClient);
        _mockClientService.Setup(s => s.GetClientByIdAsync(createdClient.Id)).ReturnsAsync(createdClient);

        // Act
        var createResult = await _controller.CreateClient(request);
        var getResult = await _controller.GetClient(createdClient.Id);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        var createdClientResponse = Assert.IsType<ClientResponse>(createdResult.Value);
        
        var okResult = Assert.IsType<OkObjectResult>(getResult.Result);
        var retrievedClientResponse = Assert.IsType<ClientResponse>(okResult.Value);
        
        Assert.Equal(createdClientResponse.Id, retrievedClientResponse.Id);
        Assert.Equal(createdClientResponse.Name, retrievedClientResponse.Name);
    }

    [Fact]
    public async Task CreateAssessment_WithAllNewHealthFactors_ShouldSucceed()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(
            125, // systolicBP
            85,  // diastolicBP
            220, // cholesterolTotal
            105, // bloodSugar
            120, // exerciseWeeklyMinutes
            "6.5 hours, mild disturbances", // sleepQuality
            "Moderate self-reported stress", // stressLevel
            "Processed or high-sugar diet"   // dietQuality
        );

        var expectedMetrics = new List<HealthMetricResponse>
        {
            new("Blood Pressure", 125, "NeedsAttention", "Monitor regularly and consider lifestyle changes"),
            new("Total Cholesterol", 220, "NeedsAttention", "Reduce saturated fats, increase exercise"),
            new("Blood Sugar", 105, "NeedsAttention", "Monitor carbohydrate intake"),
            new("Exercise Minutes", 120, "NeedsAttention", "Increase weekly exercise to at least 150 minutes"),
            new("Sleep Quality", 0, "NeedsAttention", "Improve sleep environment and establish bedtime routine"),
            new("Stress Level", 0, "NeedsAttention", "Consider stress reduction techniques like meditation"),
            new("Diet Quality", 0, "NeedsAttention", "Reduce processed foods and added sugars")
        };

        var healthReport = new HealthReportResponse(
            clientId,
            "Test Client",
            DateTime.UtcNow,
            expectedMetrics,
            "Moderate Risk",
            new List<string> { "Multiple areas need attention" }
        );

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ReturnsAsync(healthReport);

        // Act
        var result = await _controller.CreateAssessment(clientId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedReport = Assert.IsType<HealthReportResponse>(okResult.Value);
        
        Assert.Equal(7, returnedReport.Metrics.Count); // All 7 health metrics
        Assert.Equal("Moderate Risk", returnedReport.OverallRisk);
        Assert.Contains("Exercise Minutes", returnedReport.Metrics.Select(m => m.Name));
        Assert.Contains("Sleep Quality", returnedReport.Metrics.Select(m => m.Name));
        Assert.Contains("Stress Level", returnedReport.Metrics.Select(m => m.Name));
        Assert.Contains("Diet Quality", returnedReport.Metrics.Select(m => m.Name));
    }

    #endregion

    #region Performance and Edge Cases

    [Fact]
    public async Task GetAllClients_WithLargeClientList_ShouldReturnAllClients()
    {
        // Arrange
        var clients = new List<ClientResponse>();
        for (int i = 0; i < 1000; i++)
        {
            clients.Add(new ClientResponse(
                Guid.NewGuid(),
                $"Client {i}",
                new DateTime(1990, 1, 1).AddDays(i),
                i % 2 == 0 ? Gender.Male : Gender.Female,
                35 - (i % 50),
                i % 10
            ));
        }

        _mockClientService.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(clients);

        // Act
        var result = await _controller.GetAllClients();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedClients = Assert.IsType<List<ClientResponse>>(okResult.Value);
        Assert.Equal(1000, returnedClients.Count);
    }

    [Fact]
    public async Task CreateAssessment_WithExtremeValues_ShouldHandleGracefully()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(
            int.MaxValue,  // systolicBP
            int.MinValue,  // diastolicBP
            0,             // cholesterolTotal
            int.MaxValue,  // bloodSugar
            int.MinValue,  // exerciseWeeklyMinutes
            new string('A', 1000), // sleepQuality - Very long string
            "",            // stressLevel
            "Test"         // dietQuality
        );

        var healthReport = new HealthReportResponse(clientId, "Extreme Test", DateTime.UtcNow,
            new List<HealthMetricResponse>(), "High Risk", new List<string>());

        _mockClientService.Setup(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request))
            .ReturnsAsync(healthReport);

        // Act
        var result = await _controller.CreateAssessment(clientId, request);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        _mockClientService.Verify(s => s.CreateAssessmentAndGenerateReportAsync(clientId, request), Times.Once);
    }

    #endregion
}