using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using MedicalAssessment.Application.Services;

namespace MedicalAssessment.Tests.Application.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtServiceTests()
    {
        _jwtService = new JwtService();
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    #region Token Generation Tests

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken_WithUserIdAndDefaultRole()
    {
        // Arrange
        var userId = "test-user";

        // Act
        var token = _jwtService.GenerateToken(userId);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(_tokenHandler.CanReadToken(token));
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken_WithUserIdAndCustomRole()
    {
        // Arrange
        var userId = "admin-user";
        var role = "Admin";

        // Act
        var token = _jwtService.GenerateToken(userId, role);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(_tokenHandler.CanReadToken(token));
    }

    [Theory]
    [InlineData("user1", "User")]
    [InlineData("admin", "Admin")]
    [InlineData("doctor", "Doctor")]
    [InlineData("test-user-123", "CustomRole")]
    public void GenerateToken_ShouldCreateValidTokens_ForDifferentUsersAndRoles(string userId, string role)
    {
        // Act
        var token = _jwtService.GenerateToken(userId, role);

        // Assert
        Assert.NotNull(token);
        Assert.True(_tokenHandler.CanReadToken(token));
        
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        Assert.NotNull(jwtToken);
    }

    #endregion

    #region Token Claims Tests

    [Fact]
    public void GenerateToken_ShouldIncludeUserIdClaim()
    {
        // Arrange
        var userId = "test-user-123";

        // Act
        var token = _jwtService.GenerateToken(userId);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
        
        Assert.NotNull(userIdClaim);
        Assert.Equal(userId, userIdClaim.Value);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeRoleClaim_WithDefaultRole()
    {
        // Arrange
        var userId = "test-user";

        // Act
        var token = _jwtService.GenerateToken(userId);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
        
        Assert.NotNull(roleClaim);
        Assert.Equal("User", roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeRoleClaim_WithCustomRole()
    {
        // Arrange
        var userId = "admin-user";
        var role = "Admin";

        // Act
        var token = _jwtService.GenerateToken(userId, role);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
        
        Assert.NotNull(roleClaim);
        Assert.Equal(role, roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeNameIdentifierClaim()
    {
        // Arrange
        var userId = "name-identifier-test";

        // Act
        var token = _jwtService.GenerateToken(userId);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        
        Assert.NotNull(nameIdentifierClaim);
        Assert.Equal(userId, nameIdentifierClaim.Value);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeAllRequiredClaims()
    {
        // Arrange
        var userId = "complete-claims-test";
        var role = "TestRole";

        // Act
        var token = _jwtService.GenerateToken(userId, role);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var claims = jwtToken.Claims.ToList();
        
        Assert.Contains(claims, c => c.Type == "userId" && c.Value == userId);
        Assert.Contains(claims, c => c.Type == "role" && c.Value == role);
        Assert.Contains(claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
    }

    #endregion

    #region Token Expiration Tests

    [Fact]
    public void GenerateToken_ShouldSetExpirationTo24Hours()
    {
        // Arrange
        var userId = "expiration-test";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtService.GenerateToken(userId);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var expiration = jwtToken.ValidTo;
        var expectedExpiration = beforeGeneration.AddHours(24);
        
        // Allow for small time differences in test execution
        var timeDifference = Math.Abs((expiration - expectedExpiration).TotalMinutes);
        Assert.True(timeDifference < 1, $"Expiration time difference: {timeDifference} minutes");
    }

    [Fact]
    public void GenerateToken_ShouldCreateTokenValidFromNow()
    {
        // Arrange
        var userId = "validity-test";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtService.GenerateToken(userId);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var validFrom = jwtToken.ValidFrom;
        
        // Token should be valid from around the time it was created
        var timeDifference = Math.Abs((validFrom - beforeGeneration).TotalMinutes);
        Assert.True(timeDifference < 1, $"Valid from time difference: {timeDifference} minutes");
    }

    #endregion

    #region Token Uniqueness Tests

    [Fact]
    public void GenerateToken_ShouldCreateUniqueTokens_ForSameUser()
    {
        // Arrange
        var userId = "uniqueness-test";

        // Act
        var token1 = _jwtService.GenerateToken(userId);
        var token2 = _jwtService.GenerateToken(userId);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateToken_ShouldCreateUniqueTokens_ForDifferentUsers()
    {
        // Arrange
        var userId1 = "user1";
        var userId2 = "user2";

        // Act
        var token1 = _jwtService.GenerateToken(userId1);
        var token2 = _jwtService.GenerateToken(userId2);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateToken_ShouldCreateUniqueTokens_ForDifferentRoles()
    {
        // Arrange
        var userId = "role-test-user";

        // Act
        var userToken = _jwtService.GenerateToken(userId, "User");
        var adminToken = _jwtService.GenerateToken(userId, "Admin");

        // Assert
        Assert.NotEqual(userToken, adminToken);
    }

    #endregion

    #region Edge Cases Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void GenerateToken_ShouldHandleEmptyOrWhitespaceUserId(string userId)
    {
        // Act
        var token = _jwtService.GenerateToken(userId);

        // Assert
        Assert.NotNull(token);
        Assert.True(_tokenHandler.CanReadToken(token));
        
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
        Assert.Equal(userId, userIdClaim?.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GenerateToken_ShouldHandleEmptyOrNullRole(string role)
    {
        // Arrange
        var userId = "role-edge-test";

        // Act
        var token = _jwtService.GenerateToken(userId, role);

        // Assert
        Assert.NotNull(token);
        Assert.True(_tokenHandler.CanReadToken(token));
        
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
        Assert.Equal(role ?? "", roleClaim?.Value ?? "");
    }

    [Fact]
    public void GenerateToken_ShouldHandleLongUserId()
    {
        // Arrange
        var longUserId = new string('a', 1000);

        // Act
        var token = _jwtService.GenerateToken(longUserId);

        // Assert
        Assert.NotNull(token);
        Assert.True(_tokenHandler.CanReadToken(token));
    }

    [Fact]
    public void GenerateToken_ShouldHandleSpecialCharactersInUserId()
    {
        // Arrange
        var specialUserId = "user@domain.com-123_test!#$%";

        // Act
        var token = _jwtService.GenerateToken(specialUserId);

        // Assert
        Assert.NotNull(token);
        Assert.True(_tokenHandler.CanReadToken(token));
        
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
        Assert.Equal(specialUserId, userIdClaim?.Value);
    }

    #endregion

    #region Secret Key Tests

    [Fact]
    public void GetSecretKey_ShouldReturnNonEmptyString()
    {
        // Act
        var secretKey = _jwtService.GetSecretKey();

        // Assert
        Assert.NotNull(secretKey);
        Assert.NotEmpty(secretKey);
    }

    [Fact]
    public void GetSecretKey_ShouldReturnConsistentValue()
    {
        // Act
        var key1 = _jwtService.GetSecretKey();
        var key2 = _jwtService.GetSecretKey();

        // Assert
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void GetSecretKey_ShouldReturnSufficientlyLongKey()
    {
        // Act
        var secretKey = _jwtService.GetSecretKey();

        // Assert - Should be long enough for HMAC-SHA256 (at least 256 bits = 32 bytes)
        Assert.True(secretKey.Length >= 32, $"Secret key length: {secretKey.Length}");
    }

    #endregion
}