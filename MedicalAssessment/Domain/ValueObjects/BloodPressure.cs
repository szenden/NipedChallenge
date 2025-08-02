namespace MedicalAssessment.Domain.ValueObjects
{
    public record BloodPressure(int Systolic, int Diastolic)
    {

        public bool IsHigh() => Systolic >= 130 || Diastolic >= 80;
        public bool IsOptimal() => Systolic < 120 && Diastolic < 80;
    }
}