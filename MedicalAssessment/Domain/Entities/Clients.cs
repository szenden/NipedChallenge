using System;
using System.Collections.Generic;
using System.Linq;
using MedicalAssessment.Domain.ValueObjects;

namespace MedicalAssessment.Domain.Entities
{
    public class Client
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public Gender Gender { get; private set; }
        public DateTime CreatedAt { get; private set; }
        
        private readonly List<Assessment> _assessments = new();
        public IReadOnlyList<Assessment> Assessments => _assessments.AsReadOnly();

        private Client() { } // EF Core

        public Client(string name, DateTime dateOfBirth, Gender gender)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DateOfBirth = dateOfBirth;
            Gender = gender;
            CreatedAt = DateTime.UtcNow;
        }

        public int CalculateAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        public Assessment AddAssessment(BloodPressure bloodPressure, int cholesterolTotal, int bloodSugar,
            ExerciseMinutes exerciseMinutes, SleepQuality sleepQuality, StressLevel stressLevel, DietQuality dietQuality)
        {
            var assessment = new Assessment(Id, bloodPressure, cholesterolTotal, bloodSugar,
                exerciseMinutes, sleepQuality, stressLevel, dietQuality);
            _assessments.Add(assessment);
            return assessment;
        }

        public Assessment? GetLatestAssessment()
        {
            return _assessments.OrderByDescending(a => a.AssessmentDate).FirstOrDefault();
        }
    }
}