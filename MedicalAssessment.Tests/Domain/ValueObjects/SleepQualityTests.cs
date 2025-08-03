using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Domain.ValueObjects;

public class SleepQualityTests
{
    [Theory]
    [InlineData("7 hours, restful sleep")]
    [InlineData("8 hours, restful sleep")]
    [InlineData("9 hours, restful sleep")]
    public void IsOptimal_ShouldReturnTrue_ForGoodSleep(string description)
    {
        var sleepQuality = new SleepQuality(description);

        Assert.True(sleepQuality.IsOptimal());
        Assert.False(sleepQuality.NeedsAttention());
        Assert.False(sleepQuality.IsSeriousIssue());
        Assert.Equal(HealthStatus.Optimal, sleepQuality.GetHealthStatus());
    }

    [Theory]
    [InlineData("5 hours, frequent disturbances")]
    [InlineData("6 hours, frequent disturbances")]
    [InlineData("6.5 hours, mild disturbances")]
    [InlineData("5 hours of sleep")]
    [InlineData("6 hours of sleep")]
    public void NeedsAttention_ShouldReturnTrue_ForModerateSleepIssues(string description)
    {
        var sleepQuality = new SleepQuality(description);

        Assert.False(sleepQuality.IsOptimal());
        Assert.True(sleepQuality.NeedsAttention());
        Assert.Equal(HealthStatus.NeedsAttention, sleepQuality.GetHealthStatus());
    }

    [Theory]
    [InlineData("4 hours, severe sleep issues")]
    [InlineData("4.5 hours, severe sleep issues")]
    [InlineData("3 hours with severe disturbances")]
    [InlineData("<5 hours of sleep")]
    [InlineData("severe insomnia")]
    public void IsSeriousIssue_ShouldReturnTrue_ForSevereSleepIssues(string description)
    {
        var sleepQuality = new SleepQuality(description);

        Assert.False(sleepQuality.IsOptimal());
        Assert.True(sleepQuality.IsSeriousIssue());
        Assert.Equal(HealthStatus.SeriousIssue, sleepQuality.GetHealthStatus());
    }

    [Fact]
    public void GetHealthStatus_ShouldPrioritizeSeriousIssue_WhenMultipleConditionsMatch()
    {
        var sleepQuality = new SleepQuality("4 hours with frequent disturbances");

        Assert.Equal(HealthStatus.SeriousIssue, sleepQuality.GetHealthStatus());
    }

    [Theory]
    [InlineData("Normal sleep pattern")]
    [InlineData("Good quality sleep")]
    [InlineData("Regular sleep schedule")]
    public void GetHealthStatus_ShouldReturnOptimal_WhenNoSpecificConditionsMatch(string description)
    {
        var sleepQuality = new SleepQuality(description);

        Assert.Equal(HealthStatus.Optimal, sleepQuality.GetHealthStatus());
    }

    [Fact]
    public void SleepQuality_ShouldHandleEmptyString()
    {
        var sleepQuality = new SleepQuality("");

        Assert.Equal(HealthStatus.Optimal, sleepQuality.GetHealthStatus());
    }

    [Fact]
    public void SleepQuality_ShouldBeCaseSensitive_ForKeywords()
    {
        var sleepQuality1 = new SleepQuality("severe sleep issues");
        var sleepQuality2 = new SleepQuality("frequent disturbances during sleep");

        Assert.True(sleepQuality1.IsSeriousIssue());
        Assert.True(sleepQuality2.NeedsAttention());
    }
}