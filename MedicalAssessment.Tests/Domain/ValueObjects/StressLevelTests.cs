using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Domain.ValueObjects;

public class StressLevelTests
{
    [Theory]
    [InlineData("Low self-reported stress")]
    [InlineData("Low stress levels")]
    [InlineData("Very Low stress in daily life")]
    public void IsOptimal_ShouldReturnTrue_ForLowStress(string assessment)
    {
        var stressLevel = new StressLevel(assessment);

        Assert.True(stressLevel.IsOptimal());
        Assert.False(stressLevel.NeedsAttention());
        Assert.False(stressLevel.IsSeriousIssue());
        Assert.Equal(HealthStatus.Optimal, stressLevel.GetHealthStatus());
    }

    [Theory]
    [InlineData("Moderate self-reported stress")]
    [InlineData("Moderate stress levels")]
    [InlineData("Moderate work-related stress")]
    public void NeedsAttention_ShouldReturnTrue_ForModerateStress(string assessment)
    {
        var stressLevel = new StressLevel(assessment);

        Assert.False(stressLevel.IsOptimal());
        Assert.True(stressLevel.NeedsAttention());
        Assert.False(stressLevel.IsSeriousIssue());
        Assert.Equal(HealthStatus.NeedsAttention, stressLevel.GetHealthStatus());
    }

    [Theory]
    [InlineData("High chronic stress affecting well-being")]
    [InlineData("High chronic stress affecting daily life")]
    [InlineData("Severe High chronic stress affecting health")]
    public void IsSeriousIssue_ShouldReturnTrue_ForHighChronicStress(string assessment)
    {
        var stressLevel = new StressLevel(assessment);

        Assert.False(stressLevel.IsOptimal());
        Assert.False(stressLevel.NeedsAttention());
        Assert.True(stressLevel.IsSeriousIssue());
        Assert.Equal(HealthStatus.SeriousIssue, stressLevel.GetHealthStatus());
    }

    [Theory]
    [InlineData("High stress but not chronic")]
    [InlineData("Chronic stress but not affecting")]
    public void IsSeriousIssue_ShouldReturnFalse_WhenNotAllKeywordsPresent(string assessment)
    {
        var stressLevel = new StressLevel(assessment);

        Assert.False(stressLevel.IsSeriousIssue());
    }

    [Theory]
    [InlineData("Feeling overwhelmed")]
    [InlineData("Normal daily pressures")]
    [InlineData("Relaxed and calm")]
    [InlineData("")]
    public void GetHealthStatus_ShouldReturnOptimal_WhenNoSpecificKeywordsMatch(string assessment)
    {
        var stressLevel = new StressLevel(assessment);

        Assert.Equal(HealthStatus.Optimal, stressLevel.GetHealthStatus());
    }

    [Fact]
    public void GetHealthStatus_ShouldPrioritizeSeriousIssue_OverNeedsAttention()
    {
        var stressLevel = new StressLevel("High chronic stress affecting well-being with Moderate stress");

        Assert.Equal(HealthStatus.SeriousIssue, stressLevel.GetHealthStatus());
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("stress")]
    [InlineData("Low anxiety")]
    [InlineData("No stress")]
    public void IsOptimal_ShouldReturnFalse_WhenOnlyPartialKeywordsPresent(string assessment)
    {
        var stressLevel = new StressLevel(assessment);

        Assert.False(stressLevel.IsOptimal());
    }

    [Fact]
    public void StressLevel_ShouldHandleComplexDescriptions()
    {
        var complexDescription = "Generally Low stress with occasional periods, but nothing major";
        var stressLevel = new StressLevel(complexDescription);

        Assert.True(stressLevel.IsOptimal());
        Assert.Equal(HealthStatus.Optimal, stressLevel.GetHealthStatus());
    }
}