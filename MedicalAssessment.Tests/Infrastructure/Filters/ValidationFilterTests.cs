using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using MedicalAssessment.Infrastructure.Filters;

namespace MedicalAssessment.Tests.Infrastructure.Filters;

public class ValidationFilterTests
{
    private readonly ValidationFilter _filter;

    public ValidationFilterTests()
    {
        _filter = new ValidationFilter();
    }

    #region Valid Model State Tests

    [Fact]
    public void OnActionExecuting_ShouldNotSetResult_WhenModelStateIsValid()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        // ModelState is valid by default (no errors)

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }

    #endregion

    #region Invalid Model State Tests

    [Fact]
    public void OnActionExecuting_ShouldSetBadRequestResult_WhenModelStateIsInvalid()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Name", "Name is required");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        Assert.IsType<BadRequestObjectResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuting_ShouldReturnCorrectErrorMessage_WithSingleError()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Name", "Name is required");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var response = badRequestResult.Value;
        
        Assert.NotNull(response);
        var message = response.GetType().GetProperty("Message")?.GetValue(response)?.ToString();
        var errors = response.GetType().GetProperty("Errors")?.GetValue(response) as Dictionary<string, string[]>;
        
        Assert.Equal("Validation failed", message);
        Assert.NotNull(errors);
        Assert.True(errors.ContainsKey("Name"));
        Assert.Contains("Name is required", errors["Name"]);
    }

    [Fact]
    public void OnActionExecuting_ShouldReturnMultipleErrors_WhenMultipleFieldsInvalid()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Name", "Name is required");
        context.ModelState.AddModelError("Email", "Email is invalid");
        context.ModelState.AddModelError("Age", "Age must be positive");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var response = badRequestResult.Value;
        var errors = response.GetType().GetProperty("Errors")?.GetValue(response) as Dictionary<string, string[]>;
        
        Assert.NotNull(errors);
        Assert.Equal(3, errors.Count);
        Assert.True(errors.ContainsKey("Name"));
        Assert.True(errors.ContainsKey("Email"));
        Assert.True(errors.ContainsKey("Age"));
    }

    [Fact]
    public void OnActionExecuting_ShouldReturnMultipleErrorsPerField_WhenFieldHasMultipleErrors()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Password", "Password is required");
        context.ModelState.AddModelError("Password", "Password must be at least 8 characters");
        context.ModelState.AddModelError("Password", "Password must contain special characters");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var response = badRequestResult.Value;
        var errors = response.GetType().GetProperty("Errors")?.GetValue(response) as Dictionary<string, string[]>;
        
        Assert.NotNull(errors);
        Assert.True(errors.ContainsKey("Password"));
        Assert.Equal(3, errors["Password"].Length);
        Assert.Contains("Password is required", errors["Password"]);
        Assert.Contains("Password must be at least 8 characters", errors["Password"]);
        Assert.Contains("Password must contain special characters", errors["Password"]);
    }

    #endregion

    #region Medical Assessment Specific Tests

    [Fact]
    public void OnActionExecuting_ShouldValidateClientRequest_WithValidationErrors()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Name", "Name must be between 2 and 100 characters");
        context.ModelState.AddModelError("DateOfBirth", "Date of birth is required");
        context.ModelState.AddModelError("Gender", "Invalid gender value");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var response = badRequestResult.Value;
        var errors = response.GetType().GetProperty("Errors")?.GetValue(response) as Dictionary<string, string[]>;
        
        Assert.NotNull(errors);
        Assert.True(errors.ContainsKey("Name"));
        Assert.True(errors.ContainsKey("DateOfBirth"));
        Assert.True(errors.ContainsKey("Gender"));
    }

    [Fact]
    public void OnActionExecuting_ShouldValidateAssessmentRequest_WithValidationErrors()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("SystolicBP", "Systolic BP must be between 80-200 mmHg");
        context.ModelState.AddModelError("DiastolicBP", "Diastolic BP must be between 40-120 mmHg");
        context.ModelState.AddModelError("CholesterolTotal", "Cholesterol must be between 100-400 mg/dL");
        context.ModelState.AddModelError("BloodSugar", "Blood sugar must be between 50-300 mg/dL");
        context.ModelState.AddModelError("ExerciseWeeklyMinutes", "Exercise minutes must be between 0-1000 per week");
        context.ModelState.AddModelError("SleepQuality", "Sleep quality must be between 5 and 500 characters");
        context.ModelState.AddModelError("StressLevel", "Stress level must be between 5 and 500 characters");
        context.ModelState.AddModelError("DietQuality", "Diet quality must be between 5 and 500 characters");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var response = badRequestResult.Value;
        var errors = response.GetType().GetProperty("Errors")?.GetValue(response) as Dictionary<string, string[]>;
        
        Assert.NotNull(errors);
        Assert.Equal(8, errors.Count);
        Assert.True(errors.ContainsKey("SystolicBP"));
        Assert.True(errors.ContainsKey("DiastolicBP"));
        Assert.True(errors.ContainsKey("CholesterolTotal"));
        Assert.True(errors.ContainsKey("BloodSugar"));
        Assert.True(errors.ContainsKey("ExerciseWeeklyMinutes"));
        Assert.True(errors.ContainsKey("SleepQuality"));
        Assert.True(errors.ContainsKey("StressLevel"));
        Assert.True(errors.ContainsKey("DietQuality"));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void OnActionExecuting_ShouldHandleEmptyErrorMessages()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Field", "");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var response = badRequestResult.Value;
        var errors = response.GetType().GetProperty("Errors")?.GetValue(response) as Dictionary<string, string[]>;
        
        Assert.NotNull(errors);
        Assert.True(errors.ContainsKey("Field"));
        Assert.Contains("", errors["Field"]);
    }

    [Fact]
    public void OnActionExecuting_ShouldHandleNullModelState()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.Clear();

        // Act & Assert - Should not throw
        _filter.OnActionExecuting(context);
        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuting_ShouldHandleFieldsWithNoErrors()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        // Add a field with no errors (this shouldn't happen in practice)
        
        // Act
        _filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public void OnActionExecuting_ShouldReturnCorrectResponseFormat()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("TestField", "Test error message");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var response = badRequestResult.Value;
        
        Assert.NotNull(response);
        
        // Check that response has Message and Errors properties
        var messageProperty = response.GetType().GetProperty("Message");
        var errorsProperty = response.GetType().GetProperty("Errors");
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(errorsProperty);
        
        var message = messageProperty.GetValue(response)?.ToString();
        var errors = errorsProperty.GetValue(response);
        
        Assert.Equal("Validation failed", message);
        Assert.NotNull(errors);
    }

    [Fact]
    public void OnActionExecuting_ShouldSetCorrectHttpStatusCode()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("TestField", "Test error");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    #endregion

    #region Helper Methods

    private ActionExecutingContext CreateActionExecutingContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary()
        );

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object()
        );
    }

    #endregion
}