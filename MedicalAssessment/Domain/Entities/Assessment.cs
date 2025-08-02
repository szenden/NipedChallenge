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

        private Assessment() { } // EF Core

        public Assessment(Guid clientId, BloodPressure bloodPressure, int cholesterolTotal, int bloodSugar)
        {
            Id = Guid.NewGuid();
            ClientId = clientId;
            AssessmentDate = DateTime.UtcNow;
            BloodPressure = bloodPressure ?? throw new ArgumentNullException(nameof(bloodPressure));
            CholesterolTotal = cholesterolTotal;
            BloodSugar = bloodSugar;
        }
    }
}