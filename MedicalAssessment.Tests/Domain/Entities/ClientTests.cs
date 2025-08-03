using MedicalAssessment.Domain.Entities;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Domain.Entities;

public class ClientTests
{
    [Fact]
    public void Constructor_ShouldCreateClient_WithValidParameters()
    {
        var name = "John Doe";
        var dateOfBirth = new DateTime(1990, 5, 15);
        var gender = Gender.Male;

        var client = new Client(name, dateOfBirth, gender);

        Assert.NotEqual(Guid.Empty, client.Id);
        Assert.Equal(name, client.Name);
        Assert.Equal(dateOfBirth, client.DateOfBirth);
        Assert.Equal(gender, client.Gender);
        Assert.True(client.CreatedAt <= DateTime.UtcNow);
        Assert.Empty(client.Assessments);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        var dateOfBirth = new DateTime(1990, 5, 15);
        var gender = Gender.Female;

        Assert.Throws<ArgumentNullException>(() => new Client(null!, dateOfBirth, gender));
    }

    [Theory]
    [InlineData("1990-05-15", 34)] // Current year is 2025
    [InlineData("2000-01-01", 25)]
    [InlineData("1985-12-31", 39)]
    public void CalculateAge_ShouldReturnCorrectAge(string birthDateString, int expectedAge)
    {
        var dateOfBirth = DateTime.Parse(birthDateString);
        var client = new Client("Test User", dateOfBirth, Gender.Other);

        // Note: This test might be brittle due to current date dependency
        // In a real scenario, you might want to inject a date provider
        var age = client.CalculateAge();

        // Allow for some flexibility due to current date
        Assert.True(Math.Abs(age - expectedAge) <= 1);
    }

    [Fact]
    public void CalculateAge_ShouldHandleBirthdayNotYetPassed()
    {
        var nextYear = DateTime.Today.AddYears(1);
        var dateOfBirth = new DateTime(nextYear.Year - 30, nextYear.Month, nextYear.Day);
        var client = new Client("Test User", dateOfBirth, Gender.Male);

        var age = client.CalculateAge();

        Assert.Equal(29, age); // Should be 29 if birthday hasn't passed this year
    }

    [Fact]
    public void AddAssessment_ShouldAddAssessmentToClient()
    {
        var client = new Client("John Doe", new DateTime(1990, 5, 15), Gender.Male);
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        var assessment = client.AddAssessment(bloodPressure, 180, 85, exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        Assert.Single(client.Assessments);
        Assert.Equal(client.Id, assessment.ClientId);
        Assert.Equal(bloodPressure, assessment.BloodPressure);
        Assert.Equal(180, assessment.CholesterolTotal);
        Assert.Equal(85, assessment.BloodSugar);
        Assert.Equal(exerciseMinutes, assessment.ExerciseMinutes);
        Assert.Equal(sleepQuality, assessment.SleepQuality);
        Assert.Equal(stressLevel, assessment.StressLevel);
        Assert.Equal(dietQuality, assessment.DietQuality);
    }

    [Fact]
    public void AddAssessment_ShouldAddMultipleAssessments()
    {
        var client = new Client("Jane Smith", new DateTime(1985, 8, 22), Gender.Female);
        var bloodPressure1 = new BloodPressure(110, 70);
        var bloodPressure2 = new BloodPressure(125, 85);
        var exerciseMinutes = new ExerciseMinutes(180);
        var sleepQuality = new SleepQuality("8 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        client.AddAssessment(bloodPressure1, 170, 80, exerciseMinutes, sleepQuality, stressLevel, dietQuality);
        client.AddAssessment(bloodPressure2, 190, 90, exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        Assert.Equal(2, client.Assessments.Count);
    }

    [Fact]
    public void GetLatestAssessment_ShouldReturnNull_WhenNoAssessments()
    {
        var client = new Client("Test User", new DateTime(1990, 1, 1), Gender.Other);

        var latestAssessment = client.GetLatestAssessment();

        Assert.Null(latestAssessment);
    }

    [Fact]
    public void GetLatestAssessment_ShouldReturnMostRecentAssessment()
    {
        var client = new Client("Test User", new DateTime(1990, 1, 1), Gender.Male);
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        var assessment1 = client.AddAssessment(bloodPressure, 180, 85, exerciseMinutes, sleepQuality, stressLevel, dietQuality);
        Thread.Sleep(10); // Ensure different timestamps
        var assessment2 = client.AddAssessment(bloodPressure, 190, 90, exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        var latestAssessment = client.GetLatestAssessment();

        Assert.Equal(assessment2.Id, latestAssessment!.Id);
        Assert.True(latestAssessment.AssessmentDate >= assessment1.AssessmentDate);
    }

    [Fact]
    public void Client_ShouldGenerateUniqueIds()
    {
        var client1 = new Client("User 1", new DateTime(1990, 1, 1), Gender.Male);
        var client2 = new Client("User 2", new DateTime(1990, 1, 1), Gender.Female);

        Assert.NotEqual(client1.Id, client2.Id);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.Other)]
    public void Client_ShouldSupportAllGenderTypes(Gender gender)
    {
        var client = new Client("Test User", new DateTime(1990, 1, 1), gender);

        Assert.Equal(gender, client.Gender);
    }
}