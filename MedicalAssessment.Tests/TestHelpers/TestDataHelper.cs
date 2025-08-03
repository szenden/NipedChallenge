using MedicalAssessment.Application.DTOs;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Tests.TestHelpers
{
    public static class TestDataHelper
    {
        public static CreateClientRequest CreateValidClientRequest(
            string name = "John Doe",
            DateTime? dateOfBirth = null,
            Gender gender = Gender.Male)
        {
            return new CreateClientRequest
            {
                Name = name,
                DateOfBirth = dateOfBirth ?? new DateTime(1990, 5, 15),
                Gender = gender
            };
        }

        public static CreateAssessmentRequest CreateValidAssessmentRequest(
            int systolicBP = 120,
            int diastolicBP = 80,
            int cholesterolTotal = 180,
            int bloodSugar = 85,
            int exerciseWeeklyMinutes = 150,
            string sleepQuality = "7 hours, restful sleep",
            string stressLevel = "Low self-reported stress",
            string dietQuality = "Balanced, nutrient-rich diet")
        {
            return new CreateAssessmentRequest
            {
                SystolicBP = systolicBP,
                DiastolicBP = diastolicBP,
                CholesterolTotal = cholesterolTotal,
                BloodSugar = bloodSugar,
                ExerciseWeeklyMinutes = exerciseWeeklyMinutes,
                SleepQuality = sleepQuality,
                StressLevel = stressLevel,
                DietQuality = dietQuality
            };
        }

        public static CreateClientRequest CreateInvalidClientRequest(
            string name = "",
            DateTime? dateOfBirth = null,
            Gender gender = Gender.Male)
        {
            return new CreateClientRequest
            {
                Name = name,
                DateOfBirth = dateOfBirth ?? DateTime.MinValue,
                Gender = gender
            };
        }

        public static CreateAssessmentRequest CreateInvalidAssessmentRequest(
            int systolicBP = 300,
            int diastolicBP = 200,
            int cholesterolTotal = 500,
            int bloodSugar = 400,
            int exerciseWeeklyMinutes = 1500,
            string sleepQuality = "Bad",
            string stressLevel = "High",
            string dietQuality = "Poor")
        {
            return new CreateAssessmentRequest
            {
                SystolicBP = systolicBP,
                DiastolicBP = diastolicBP,
                CholesterolTotal = cholesterolTotal,
                BloodSugar = bloodSugar,
                ExerciseWeeklyMinutes = exerciseWeeklyMinutes,
                SleepQuality = sleepQuality,
                StressLevel = stressLevel,
                DietQuality = dietQuality
            };
        }
    }
}