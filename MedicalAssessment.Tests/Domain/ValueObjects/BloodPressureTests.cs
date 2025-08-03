using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Domain.ValueObjects;

public class BloodPressureTests
{
    [Theory]
    [InlineData(110, 70)]
    [InlineData(119, 79)]
    [InlineData(100, 60)]
    public void IsOptimal_ShouldReturnTrue_WhenBothValuesAreOptimal(int systolic, int diastolic)
    {
        var bloodPressure = new BloodPressure(systolic, diastolic);

        Assert.True(bloodPressure.IsOptimal());
        Assert.False(bloodPressure.IsHigh());
    }

    [Theory]
    [InlineData(120, 70)]
    [InlineData(130, 85)]
    [InlineData(140, 90)]
    public void IsOptimal_ShouldReturnFalse_WhenEitherValueIsNotOptimal(int systolic, int diastolic)
    {
        var bloodPressure = new BloodPressure(systolic, diastolic);

        Assert.False(bloodPressure.IsOptimal());
    }

    [Theory]
    [InlineData(130, 75)]
    [InlineData(125, 80)]
    [InlineData(140, 90)]
    [InlineData(150, 95)]
    public void IsHigh_ShouldReturnTrue_WhenEitherValueIsHigh(int systolic, int diastolic)
    {
        var bloodPressure = new BloodPressure(systolic, diastolic);

        Assert.True(bloodPressure.IsHigh());
        Assert.False(bloodPressure.IsOptimal());
    }

    [Theory]
    [InlineData(119, 79)]
    [InlineData(100, 60)]
    [InlineData(115, 75)]
    public void IsHigh_ShouldReturnFalse_WhenBothValuesAreBelowThreshold(int systolic, int diastolic)
    {
        var bloodPressure = new BloodPressure(systolic, diastolic);

        Assert.False(bloodPressure.IsHigh());
    }

    [Fact]
    public void BloodPressure_BoundaryValues_ShouldWorkCorrectly()
    {
        var optimalBoundary = new BloodPressure(119, 79);
        Assert.True(optimalBoundary.IsOptimal());
        Assert.False(optimalBoundary.IsHigh());

        var highBoundary = new BloodPressure(130, 80);
        Assert.False(highBoundary.IsOptimal());
        Assert.True(highBoundary.IsHigh());

        var systolicHigh = new BloodPressure(130, 70);
        Assert.False(systolicHigh.IsOptimal());
        Assert.True(systolicHigh.IsHigh());

        var diastolicHigh = new BloodPressure(110, 80);
        Assert.False(diastolicHigh.IsOptimal());
        Assert.True(diastolicHigh.IsHigh());
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-10, -5)]
    public void BloodPressure_ShouldHandleInvalidValues(int systolic, int diastolic)
    {
        var bloodPressure = new BloodPressure(systolic, diastolic);

        // Invalid values still follow the logic: < 120 and < 80 = optimal, >= 130 or >= 80 = high
        Assert.True(bloodPressure.IsOptimal()); // 0 < 120 and 0 < 80, so it's optimal
        Assert.False(bloodPressure.IsHigh());
    }

    [Theory]
    [InlineData(200, 120)]
    [InlineData(180, 110)]
    public void BloodPressure_ShouldHandleVeryHighValues(int systolic, int diastolic)
    {
        var bloodPressure = new BloodPressure(systolic, diastolic);

        Assert.True(bloodPressure.IsHigh());
        Assert.False(bloodPressure.IsOptimal());
    }
}