using MedicalAssessment.Domain.Entities;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.Domain.Entities;

public class AssessmentTests
{
    [Fact]
    public void Constructor_ShouldCreateAssessment_WithValidParameters()
    {
        var clientId = Guid.NewGuid();
        var bloodPressure = new BloodPressure(120, 80);
        var cholesterolTotal = 180;
        var bloodSugar = 85;
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        var assessment = new Assessment(clientId, bloodPressure, cholesterolTotal, bloodSugar,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        Assert.NotEqual(Guid.Empty, assessment.Id);
        Assert.Equal(clientId, assessment.ClientId);
        Assert.True(assessment.AssessmentDate <= DateTime.UtcNow);
        Assert.Equal(bloodPressure, assessment.BloodPressure);
        Assert.Equal(cholesterolTotal, assessment.CholesterolTotal);
        Assert.Equal(bloodSugar, assessment.BloodSugar);
        Assert.Equal(exerciseMinutes, assessment.ExerciseMinutes);
        Assert.Equal(sleepQuality, assessment.SleepQuality);
        Assert.Equal(stressLevel, assessment.StressLevel);
        Assert.Equal(dietQuality, assessment.DietQuality);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenBloodPressureIsNull()
    {
        var clientId = Guid.NewGuid();
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        Assert.Throws<ArgumentNullException>(() => new Assessment(clientId, null!, 180, 85,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenExerciseMinutesIsNull()
    {
        var clientId = Guid.NewGuid();
        var bloodPressure = new BloodPressure(120, 80);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        Assert.Throws<ArgumentNullException>(() => new Assessment(clientId, bloodPressure, 180, 85,
            null!, sleepQuality, stressLevel, dietQuality));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenSleepQualityIsNull()
    {
        var clientId = Guid.NewGuid();
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        Assert.Throws<ArgumentNullException>(() => new Assessment(clientId, bloodPressure, 180, 85,
            exerciseMinutes, null!, stressLevel, dietQuality));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenStressLevelIsNull()
    {
        var clientId = Guid.NewGuid();
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        Assert.Throws<ArgumentNullException>(() => new Assessment(clientId, bloodPressure, 180, 85,
            exerciseMinutes, sleepQuality, null!, dietQuality));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDietQualityIsNull()
    {
        var clientId = Guid.NewGuid();
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");

        Assert.Throws<ArgumentNullException>(() => new Assessment(clientId, bloodPressure, 180, 85,
            exerciseMinutes, sleepQuality, stressLevel, null!));
    }

    [Fact]
    public void Assessment_ShouldGenerateUniqueIds()
    {
        var clientId = Guid.NewGuid();
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        var assessment1 = new Assessment(clientId, bloodPressure, 180, 85,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);
        var assessment2 = new Assessment(clientId, bloodPressure, 180, 85,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        Assert.NotEqual(assessment1.Id, assessment2.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(300)]
    [InlineData(-10)]
    public void Assessment_ShouldAcceptAnyIntegerValues_ForCholesterolAndBloodSugar(int value)
    {
        var clientId = Guid.NewGuid();
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        var assessment = new Assessment(clientId, bloodPressure, value, value,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        Assert.Equal(value, assessment.CholesterolTotal);
        Assert.Equal(value, assessment.BloodSugar);
    }

    [Fact]
    public void Assessment_ShouldSetAssessmentDateToCurrentTime()
    {
        var before = DateTime.UtcNow;
        
        var clientId = Guid.NewGuid();
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        var assessment = new Assessment(clientId, bloodPressure, 180, 85,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);
        
        var after = DateTime.UtcNow;

        Assert.True(assessment.AssessmentDate >= before);
        Assert.True(assessment.AssessmentDate <= after);
    }

    [Fact]
    public void Assessment_ShouldAcceptEmptyGuid_ForClientId()
    {
        var clientId = Guid.Empty;
        var bloodPressure = new BloodPressure(120, 80);
        var exerciseMinutes = new ExerciseMinutes(150);
        var sleepQuality = new SleepQuality("7 hours, restful sleep");
        var stressLevel = new StressLevel("Low self-reported stress");
        var dietQuality = new DietQuality("Balanced, nutrient-rich diet");

        var assessment = new Assessment(clientId, bloodPressure, 180, 85,
            exerciseMinutes, sleepQuality, stressLevel, dietQuality);

        Assert.Equal(Guid.Empty, assessment.ClientId);
    }
}