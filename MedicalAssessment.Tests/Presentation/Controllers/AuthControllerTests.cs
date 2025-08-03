using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MedicalAssessment.Application.DTOs;
using MedicalAssessment.Application.Services;
using MedicalAssessment.Presentation.Controllers;

namespace MedicalAssessment.Tests.Presentation.Controllers;

public class AuthControllerTests
{
    private readonly Mock<JwtService> _mockJwtService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockJwtService = new Mock<JwtService>();
        _controller = new AuthController(_mockJwtService.Object);
    }

    #region Login Tests

    [Fact]
    public void Login_ShouldReturnOk_WithValidAdminCredentials()
    {
        // Arrange
        var request = new LoginRequest("admin", "password");
        var expectedToken = "test-jwt-token";
        _mockJwtService.Setup(x => x.GenerateToken("admin", "Admin"))
                      .Returns(expectedToken);

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        Assert.NotNull(response);
        var token = response.GetType().GetProperty("Token")?.GetValue(response)?.ToString();
        var username = response.GetType().GetProperty("Username")?.GetValue(response)?.ToString();
        var role = response.GetType().GetProperty("Role")?.GetValue(response)?.ToString();
        
        Assert.Equal(expectedToken, token);
        Assert.Equal("admin", username);
        Assert.Equal("Admin", role);
    }

    [Fact]
    public void Login_ShouldReturnOk_WithValidDoctorCredentials()
    {
        // Arrange
        var request = new LoginRequest("doctor", "doctor123");
        var expectedToken = "test-jwt-token-doctor";
        _mockJwtService.Setup(x => x.GenerateToken("doctor", "User"))
                      .Returns(expectedToken);

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        Assert.NotNull(response);
        var username = response.GetType().GetProperty("Username")?.GetValue(response)?.ToString();
        var role = response.GetType().GetProperty("Role")?.GetValue(response)?.ToString();
        
        Assert.Equal("doctor", username);
        Assert.Equal("User", role);
    }

    [Fact]
    public void Login_ShouldReturnOk_WithValidUserCredentials()
    {
        // Arrange
        var request = new LoginRequest("user", "user123");
        var expectedToken = "test-jwt-token-user";
        _mockJwtService.Setup(x => x.GenerateToken("user", "User"))
                      .Returns(expectedToken);

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        Assert.NotNull(response);
        var token = response.GetType().GetProperty("Token")?.GetValue(response)?.ToString();
        var expiresIn = response.GetType().GetProperty("ExpiresIn")?.GetValue(response)?.ToString();
        
        Assert.Equal(expectedToken, token);
        Assert.Equal("24 hours", expiresIn);
    }

    [Theory]
    [InlineData("admin", "wrongpassword")]
    [InlineData("wronguser", "password")]
    [InlineData("doctor", "wrongpassword")]
    [InlineData("user", "wrongpassword")]
    [InlineData("", "password")]
    [InlineData("admin", "")]
    [InlineData("", "")]
    public void Login_ShouldReturnUnauthorized_WithInvalidCredentials(string username, string password)
    {
        // Arrange
        var request = new LoginRequest(username, password);

        // Act
        var result = _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = unauthorizedResult.Value;
        
        Assert.NotNull(response);
        var message = response.GetType().GetProperty("Message")?.GetValue(response)?.ToString();
        Assert.Equal("Invalid username or password", message);
    }

    [Fact]
    public void Login_ShouldCallJwtService_WithCorrectParameters_ForAdmin()
    {
        // Arrange
        var request = new LoginRequest("admin", "password");
        
        // Act
        _controller.Login(request);

        // Assert
        _mockJwtService.Verify(x => x.GenerateToken("admin", "Admin"), Times.Once);
    }

    [Fact]
    public void Login_ShouldCallJwtService_WithCorrectParameters_ForUser()
    {
        // Arrange
        var request = new LoginRequest("doctor", "doctor123");
        
        // Act
        _controller.Login(request);

        // Assert
        _mockJwtService.Verify(x => x.GenerateToken("doctor", "User"), Times.Once);
    }

    [Fact]
    public void Login_ShouldNotCallJwtService_WithInvalidCredentials()
    {
        // Arrange
        var request = new LoginRequest("invalid", "invalid");
        
        // Act
        _controller.Login(request);

        // Assert
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Login_ShouldReturnCorrectRole_BasedOnUsername()
    {
        // Arrange & Act & Assert
        var adminRequest = new LoginRequest("admin", "password");
        var adminResult = _controller.Login(adminRequest) as OkObjectResult;
        var adminRole = adminResult?.Value?.GetType().GetProperty("Role")?.GetValue(adminResult.Value)?.ToString();
        Assert.Equal("Admin", adminRole);

        var userRequest = new LoginRequest("doctor", "doctor123");
        var userResult = _controller.Login(userRequest) as OkObjectResult;
        var userRole = userResult?.Value?.GetType().GetProperty("Role")?.GetValue(userResult.Value)?.ToString();
        Assert.Equal("User", userRole);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Login_ShouldHandleNullRequest()
    {
        // Act
        var result = _controller.Login(null!);

        // Assert
        // This will likely throw or return BadRequest depending on validation
        // For now, testing the current behavior
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Theory]
    [InlineData("ADMIN", "PASSWORD")] // Case sensitivity test
    [InlineData("Admin", "Password")]
    [InlineData("DOCTOR", "DOCTOR123")]
    public void Login_ShouldBeCaseSensitive(string username, string password)
    {
        // Arrange
        var request = new LoginRequest(username, password);

        // Act
        var result = _controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Theory]
    [InlineData("admin ", "password")] // Trailing space
    [InlineData(" admin", "password")] // Leading space
    [InlineData("admin", " password")] // Password with space
    public void Login_ShouldNotTrimWhitespace(string username, string password)
    {
        // Arrange
        var request = new LoginRequest(username, password);

        // Act
        var result = _controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    #endregion
}