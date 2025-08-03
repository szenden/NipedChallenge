using System;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Domain.Entities
{
    public class Assessment
    {
        public Guid Id { get; private set; }
        public Guid ClientId { get; private set; }
        public DateTime AssessmentDate { get; private set; }
        public BloodPressure BloodPressure { get; private set; }
        public int CholesterolTotal { get; private set; }
        public int BloodSugar { get; private set; }
        public ExerciseMinutes ExerciseMinutes { get; private set; }
        public SleepQuality SleepQuality { get; private set; }
        public StressLevel StressLevel { get; private set; }
        public DietQuality DietQuality { get; private set; } = null!;

        private Assessment() { } // EF Core

        public Assessment(Guid clientId, BloodPressure bloodPressure, int cholesterolTotal, int bloodSugar,
            ExerciseMinutes exerciseMinutes, SleepQuality sleepQuality, StressLevel stressLevel, DietQuality dietQuality)
        {
            Id = Guid.NewGuid();
            ClientId = clientId;
            AssessmentDate = DateTime.UtcNow;
            BloodPressure = bloodPressure ?? throw new ArgumentNullException(nameof(bloodPressure));
            CholesterolTotal = cholesterolTotal;
            BloodSugar = bloodSugar;
            ExerciseMinutes = exerciseMinutes ?? throw new ArgumentNullException(nameof(exerciseMinutes));
            SleepQuality = sleepQuality ?? throw new ArgumentNullException(nameof(sleepQuality));
            StressLevel = stressLevel ?? throw new ArgumentNullException(nameof(stressLevel));
            DietQuality = dietQuality ?? throw new ArgumentNullException(nameof(dietQuality));
        }
    }
}