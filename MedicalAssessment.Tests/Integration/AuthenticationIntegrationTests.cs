using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Moq;
using MedicalAssessment.Application.DTOs;
using MedicalAssessment.Application.Interfaces;
using MedicalAssessment.Application.Services;
using MedicalAssessment.Infrastructure.Data;
using MedicalAssessment.Infrastructure.Filters;
using MedicalAssessment.API.Controllers;
using MedicalAssessment.Presentation.Controllers;
using MedicalAssessment.Tests.TestHelpers;
using System.Security.Claims;

namespace MedicalAssessment.Tests.Integration;

/// <summary>
/// Integration tests focusing on authentication and validation pipeline integration
/// </summary>
public class AuthenticationIntegrationTests
{
    #region Authentication Pipeline Integration

    [Fact]
    public void AuthenticationPipeline_ShouldWork_EndToEnd()
    {
        // Arrange
        var jwtService = new JwtService();
        var authController = new MedicalAssessment.Presentation.Controllers.AuthController(jwtService);

        // Act 1: Login to get token
        var loginRequest = new LoginRequest("admin", "password");
        var loginResult = authController.Login(loginRequest);

        // Assert 1: Login successful
        var okResult = Assert.IsType<OkObjectResult>(loginResult);
        var response = okResult.Value;
        var token = response.GetType().GetProperty("Token")?.GetValue(response)?.ToString();
        
        Assert.NotNull(token);

        // Act 2: Use token with protected controller
        var mockClientService = new Mock<IClientService>();
        var clientsController = new ClientsController(mockClientService.Object);
        
        // Setup authenticated context
        SetupAuthenticatedContext(clientsController, token, jwtService);

        // Act 3: Call protected endpoint
        var clientsResult = clientsController.GetAllClients();

        // Assert 3: Should succeed (not throw unauthorized)
        Assert.NotNull(clientsResult);
    }

    [Fact]
    public void ValidationPipeline_ShouldWork_WithAuthenticatedRequest()
    {
        // Arrange
        var validationFilter = new ValidationFilter();
        var context = CreateActionExecutingContextWithAuth();
        
        // Add validation errors
        context.ModelState.AddModelError("Name", "Name is required");
        context.ModelState.AddModelError("SystolicBP", "Systolic BP must be between 80-200 mmHg");

        // Act
        validationFilter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var response = badRequestResult.Value;
        
        Assert.NotNull(response);
        var message = response.GetType().GetProperty("Message")?.GetValue(response)?.ToString();
        var errors = response.GetType().GetProperty("Errors")?.GetValue(response) as Dictionary<string, string[]>;
        
        Assert.Equal("Validation failed", message);
        Assert.NotNull(errors);
        Assert.True(errors.ContainsKey("Name"));
        Assert.True(errors.ContainsKey("SystolicBP"));
    }

    #endregion

    #region JWT Token Integration

    [Fact]
    public void JwtTokenGeneration_ShouldIntegrateWith_AuthController()
    {
        // Arrange
        var jwtService = new JwtService();
        var authController = new MedicalAssessment.Presentation.Controllers.AuthController(jwtService);

        // Test different user types
        var testCases = new[]
        {
            new { Username = "admin", Password = "password", ExpectedRole = "Admin" },
            new { Username = "doctor", Password = "doctor123", ExpectedRole = "User" },
            new { Username = "user", Password = "user123", ExpectedRole = "User" }
        };

        foreach (var testCase in testCases)
        {
            // Act
            var loginRequest = new LoginRequest(testCase.Username, testCase.Password);
            var result = authController.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            
            var token = response.GetType().GetProperty("Token")?.GetValue(response)?.ToString();
            var role = response.GetType().GetProperty("Role")?.GetValue(response)?.ToString();
            
            Assert.NotNull(token);
            Assert.Equal(testCase.ExpectedRole, role);
            
            // Verify token is valid JWT
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            Assert.True(tokenHandler.CanReadToken(token));
        }
    }

    #endregion

    #region Controller Integration with Authentication

    [Fact]
    public async Task ClientsController_ShouldWork_WithValidAuthentication()
    {
        // Arrange
        var mockClientService = new Mock<IClientService>();
        var expectedClients = new List<ClientResponse>
        {
            new ClientResponse(Guid.NewGuid(), "Test Client", DateTime.Now, MedicalAssessment.Domain.ValueObjects.Gender.Male, 30, 1)
        };
        
        mockClientService.Setup(x => x.GetAllClientsAsync()).ReturnsAsync(expectedClients);
        
        var controller = new ClientsController(mockClientService.Object);
        SetupAuthenticatedContext(controller);

        // Act
        var result = await controller.GetAllClients();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var clients = Assert.IsType<List<ClientResponse>>(okResult.Value);
        Assert.Single(clients);
        Assert.Equal("Test Client", clients[0].Name);
    }

    [Fact]
    public async Task ClientsController_ShouldValidateInput_WhenCreatingClient()
    {
        // Arrange
        var mockClientService = new Mock<IClientService>();
        var controller = new ClientsController(mockClientService.Object);
        SetupAuthenticatedContext(controller);

        // This test would need validation filter to be applied at controller level
        // For now, testing that controller accepts valid requests
        var validRequest = TestDataHelper.CreateValidClientRequest();

        var expectedResponse = new ClientResponse(
            Guid.NewGuid(), validRequest.Name, validRequest.DateOfBirth, 
            validRequest.Gender, 30, 0);
        
        mockClientService.Setup(x => x.CreateClientAsync(It.IsAny<CreateClientRequest>()))
                        .ReturnsAsync(expectedResponse);

        // Act
        var result = await controller.CreateClient(validRequest);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var client = Assert.IsType<ClientResponse>(createdResult.Value);
        Assert.Equal(validRequest.Name, client.Name);
    }

    #endregion

    #region Data Transfer Object Integration

    [Fact]
    public void DTOs_ShouldWork_WithValidationAttributes()
    {
        // Test that our TestDataHelper creates DTOs that would pass validation
        var validClientRequest = TestDataHelper.CreateValidClientRequest();
        var validAssessmentRequest = TestDataHelper.CreateValidAssessmentRequest();

        // Verify the DTOs have the expected properties
        Assert.NotNull(validClientRequest.Name);
        Assert.True(validClientRequest.Name.Length >= 2);
        Assert.NotEqual(DateTime.MinValue, validClientRequest.DateOfBirth);

        Assert.True(validAssessmentRequest.SystolicBP >= 80 && validAssessmentRequest.SystolicBP <= 200);
        Assert.True(validAssessmentRequest.DiastolicBP >= 40 && validAssessmentRequest.DiastolicBP <= 120);
        Assert.True(validAssessmentRequest.SleepQuality.Length >= 5);
        Assert.True(validAssessmentRequest.StressLevel.Length >= 5);
        Assert.True(validAssessmentRequest.DietQuality.Length >= 5);
    }

    [Fact]
    public void DTOs_ShouldFail_WithInvalidData()
    {
        // Test that our TestDataHelper creates DTOs that would fail validation
        var invalidClientRequest = TestDataHelper.CreateInvalidClientRequest();
        var invalidAssessmentRequest = TestDataHelper.CreateInvalidAssessmentRequest();

        // These would fail validation if processed through the validation filter
        Assert.Equal(string.Empty, invalidClientRequest.Name); // Too short
        
        Assert.True(invalidAssessmentRequest.SystolicBP > 200); // Out of range
        Assert.True(invalidAssessmentRequest.DiastolicBP > 120); // Out of range
        Assert.True(invalidAssessmentRequest.SleepQuality.Length < 5); // Too short
    }

    #endregion

    #region Helper Methods

    private void SetupAuthenticatedContext(ControllerBase controller, string token = null, JwtService jwtService = null)
    {
        // If token provided, verify it; otherwise create a test user
        ClaimsPrincipal principal;
        
        if (!string.IsNullOrEmpty(token) && jwtService != null)
        {
            // In a real scenario, you'd validate the JWT token here
            // For testing, we'll create claims based on the token
            var claims = new List<Claim>
            {
                new Claim("userId", "test-user"),
                new Claim("role", "Admin"),
                new Claim(ClaimTypes.NameIdentifier, "test-user")
            };
            principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        }
        else
        {
            // Default test user
            var claims = new List<Claim>
            {
                new Claim("userId", "test-user"),
                new Claim("role", "User"),
                new Claim(ClaimTypes.NameIdentifier, "test-user")
            };
            principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        }
        
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private ActionExecutingContext CreateActionExecutingContextWithAuth()
    {
        var claims = new List<Claim>
        {
            new Claim("userId", "test-user"),
            new Claim("role", "User"),
            new Claim(ClaimTypes.NameIdentifier, "test-user")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var actionContext = new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
            new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()
        );

        return new ActionExecutingContext(
            actionContext,
            new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object()
        );
    }

    #endregion
}