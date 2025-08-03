using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Domain.ValueObjects;

public class DietQualityTests
{
    [Theory]
    [InlineData("Balanced, nutrient-rich diet")]
    [InlineData("Well Balanced and nutrient-rich meals")]
    [InlineData("Healthy Balanced diet with nutrient-rich foods")]
    public void IsOptimal_ShouldReturnTrue_ForBalancedNutrientRichDiet(string assessment)
    {
        var dietQuality = new DietQuality(assessment);

        Assert.True(dietQuality.IsOptimal());
        Assert.False(dietQuality.NeedsAttention());
        Assert.False(dietQuality.IsSeriousIssue());
        Assert.Equal(HealthStatus.Optimal, dietQuality.GetHealthStatus());
    }

    [Theory]
    [InlineData("Processed or high-sugar diet")]
    [InlineData("Mostly Processed foods")]
    [InlineData("Diet with high-sugar content")]
    [InlineData("Frequent Processed meals")]
    [InlineData("Too much high-sugar snacks")]
    public void NeedsAttention_ShouldReturnTrue_ForProcessedOrHighSugarDiet(string assessment)
    {
        var dietQuality = new DietQuality(assessment);

        Assert.False(dietQuality.IsOptimal());
        Assert.True(dietQuality.NeedsAttention());
        Assert.False(dietQuality.IsSeriousIssue());
        Assert.Equal(HealthStatus.NeedsAttention, dietQuality.GetHealthStatus());
    }

    [Theory]
    [InlineData("Poor nutrition with deficiencies")]
    [InlineData("Very Poor nutrition with vitamin deficiencies")]
    [InlineData("Poor dietary choices with nutritional deficiencies")]
    public void IsSeriousIssue_ShouldReturnTrue_ForPoorNutritionWithDeficiencies(string assessment)
    {
        var dietQuality = new DietQuality(assessment);

        Assert.False(dietQuality.IsOptimal());
        Assert.False(dietQuality.NeedsAttention());
        Assert.True(dietQuality.IsSeriousIssue());
        Assert.Equal(HealthStatus.SeriousIssue, dietQuality.GetHealthStatus());
    }

    [Theory]
    [InlineData("Good nutrition with deficiencies")]
    [InlineData("Poor eating habits")]
    public void IsSeriousIssue_ShouldReturnFalse_WhenNotAllKeywordsPresent(string assessment)
    {
        var dietQuality = new DietQuality(assessment);

        Assert.False(dietQuality.IsSeriousIssue());
    }

    [Theory]
    [InlineData("Nutrient-rich but not balanced")]
    [InlineData("Good diet")]
    public void IsOptimal_ShouldReturnFalse_WhenNotAllKeywordsPresent(string assessment)
    {
        var dietQuality = new DietQuality(assessment);

        Assert.False(dietQuality.IsOptimal());
    }

    [Theory]
    [InlineData("Regular meals")]
    [InlineData("Eating healthy")]
    [InlineData("Good eating habits")]
    [InlineData("")]
    public void GetHealthStatus_ShouldReturnOptimal_WhenNoSpecificKeywordsMatch(string assessment)
    {
        var dietQuality = new DietQuality(assessment);

        Assert.Equal(HealthStatus.Optimal, dietQuality.GetHealthStatus());
    }

    [Fact]
    public void GetHealthStatus_ShouldPrioritizeSeriousIssue_OverNeedsAttention()
    {
        var dietQuality = new DietQuality("Poor nutrition with deficiencies and some Processed foods");

        Assert.Equal(HealthStatus.SeriousIssue, dietQuality.GetHealthStatus());
    }

    [Fact]
    public void GetHealthStatus_ShouldPrioritizeNeedsAttention_OverOptimal()
    {
        var dietQuality = new DietQuality("Mostly Balanced, nutrient-rich diet with some Processed snacks");

        Assert.Equal(HealthStatus.NeedsAttention, dietQuality.GetHealthStatus());
    }

    [Fact]
    public void DietQuality_ShouldHandleComplexDescriptions()
    {
        var complexDescription = "Generally Balanced, nutrient-rich diet with occasional treats but avoiding Poor nutrition";
        var dietQuality = new DietQuality(complexDescription);

        Assert.True(dietQuality.IsOptimal());
        Assert.Equal(HealthStatus.Optimal, dietQuality.GetHealthStatus());
    }
}