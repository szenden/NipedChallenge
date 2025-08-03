using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Domain.ValueObjects;

public class ExerciseMinutesTests
{
    [Theory]
    [InlineData(150, true, false, false)]
    [InlineData(200, true, false, false)]
    [InlineData(300, true, false, false)]
    public void IsOptimal_ShouldReturnTrue_WhenMinutesAre150OrMore(int minutes, bool expectedOptimal, bool expectedNeedsAttention, bool expectedSeriousIssue)
    {
        var exerciseMinutes = new ExerciseMinutes(minutes);

        Assert.Equal(expectedOptimal, exerciseMinutes.IsOptimal());
        Assert.Equal(expectedNeedsAttention, exerciseMinutes.NeedsAttention());
        Assert.Equal(expectedSeriousIssue, exerciseMinutes.IsSeriousIssue());
    }

    [Theory]
    [InlineData(75, false, true, false)]
    [InlineData(100, false, true, false)]
    [InlineData(149, false, true, false)]
    public void NeedsAttention_ShouldReturnTrue_WhenMinutesAreBetween75And149(int minutes, bool expectedOptimal, bool expectedNeedsAttention, bool expectedSeriousIssue)
    {
        var exerciseMinutes = new ExerciseMinutes(minutes);

        Assert.Equal(expectedOptimal, exerciseMinutes.IsOptimal());
        Assert.Equal(expectedNeedsAttention, exerciseMinutes.NeedsAttention());
        Assert.Equal(expectedSeriousIssue, exerciseMinutes.IsSeriousIssue());
    }

    [Theory]
    [InlineData(0, false, false, true)]
    [InlineData(30, false, false, true)]
    [InlineData(74, false, false, true)]
    public void IsSeriousIssue_ShouldReturnTrue_WhenMinutesAreLessThan75(int minutes, bool expectedOptimal, bool expectedNeedsAttention, bool expectedSeriousIssue)
    {
        var exerciseMinutes = new ExerciseMinutes(minutes);

        Assert.Equal(expectedOptimal, exerciseMinutes.IsOptimal());
        Assert.Equal(expectedNeedsAttention, exerciseMinutes.NeedsAttention());
        Assert.Equal(expectedSeriousIssue, exerciseMinutes.IsSeriousIssue());
    }

    [Theory]
    [InlineData(150, HealthStatus.Optimal)]
    [InlineData(200, HealthStatus.Optimal)]
    [InlineData(100, HealthStatus.NeedsAttention)]
    [InlineData(75, HealthStatus.NeedsAttention)]
    [InlineData(50, HealthStatus.SeriousIssue)]
    [InlineData(0, HealthStatus.SeriousIssue)]
    public void GetHealthStatus_ShouldReturnCorrectStatus(int minutes, HealthStatus expectedStatus)
    {
        var exerciseMinutes = new ExerciseMinutes(minutes);

        var result = exerciseMinutes.GetHealthStatus();

        Assert.Equal(expectedStatus, result);
    }

    [Fact]
    public void ExerciseMinutes_ShouldAllowNegativeValues()
    {
        var exerciseMinutes = new ExerciseMinutes(-10);

        Assert.True(exerciseMinutes.IsSeriousIssue());
        Assert.Equal(HealthStatus.SeriousIssue, exerciseMinutes.GetHealthStatus());
    }

    [Fact]
    public void ExerciseMinutes_BoundaryValues_ShouldWorkCorrectly()
    {
        Assert.Equal(HealthStatus.SeriousIssue, new ExerciseMinutes(74).GetHealthStatus());
        Assert.Equal(HealthStatus.NeedsAttention, new ExerciseMinutes(75).GetHealthStatus());
        Assert.Equal(HealthStatus.NeedsAttention, new ExerciseMinutes(149).GetHealthStatus());
        Assert.Equal(HealthStatus.Optimal, new ExerciseMinutes(150).GetHealthStatus());
    }
}