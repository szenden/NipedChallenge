namespace MedicalAssessment.Domain.ValueObjects
{
    public record BloodPressure(int Systolic, int Diastolic)
    {
        public BloodPressure(int systolic, int diastolic) : this(systolic, diastolic)
        {
            if (systolic <= 0 || diastolic <= 0)
                throw new ArgumentException("Blood pressure values must be positive");
            
            if (systolic <= diastolic)
                throw new ArgumentException("Systolic pressure must be higher than diastolic");
        }

        public bool IsHigh() => Systolic >= 130 || Diastolic >= 80;
        public bool IsOptimal() => Systolic < 120 && Diastolic < 80;
    }
}