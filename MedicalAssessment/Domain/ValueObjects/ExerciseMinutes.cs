namespace MedicalAssessment.Domain.ValueObjects
{
    public record ExerciseMinutes(int WeeklyMinutes)
    {
        public bool IsOptimal() => WeeklyMinutes >= 150;
        public bool NeedsAttention() => WeeklyMinutes >= 75 && WeeklyMinutes < 150;
        public bool IsSeriousIssue() => WeeklyMinutes < 75;

        public HealthStatus GetHealthStatus()
        {
            if (IsOptimal()) return HealthStatus.Optimal;
            if (NeedsAttention()) return HealthStatus.NeedsAttention;
            return HealthStatus.SeriousIssue;
        }
    }
}