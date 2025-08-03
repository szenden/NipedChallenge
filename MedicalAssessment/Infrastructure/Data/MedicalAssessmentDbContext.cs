using Microsoft.EntityFrameworkCore;
using MedicalAssessment.Domain.Entities;

namespace MedicalAssessment.Infrastructure.Data
{
    public class MedicalAssessmentDbContext : DbContext
    {
        public MedicalAssessmentDbContext(DbContextOptions<MedicalAssessmentDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Assessment> Assessments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Client Configuration
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DateOfBirth).IsRequired();
                entity.Property(e => e.Gender).HasConversion<string>().IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                
                entity.HasMany(c => c.Assessments)
                      .WithOne()
                      .HasForeignKey(a => a.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Assessment Configuration
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClientId).IsRequired();
                entity.Property(e => e.AssessmentDate).IsRequired();
                entity.Property(e => e.CholesterolTotal).IsRequired();
                entity.Property(e => e.BloodSugar).IsRequired();
                
                entity.OwnsOne(e => e.BloodPressure, bp =>
                {
                    bp.Property(p => p.Systolic).HasColumnName("SystolicBP").IsRequired();
                    bp.Property(p => p.Diastolic).HasColumnName("DiastolicBP").IsRequired();
                });
                
                entity.OwnsOne(e => e.ExerciseMinutes, em =>
                {
                    em.Property(p => p.WeeklyMinutes).HasColumnName("ExerciseWeeklyMinutes").IsRequired();
                });
                
                entity.OwnsOne(e => e.SleepQuality, sq =>
                {
                    sq.Property(p => p.Description).HasColumnName("SleepQuality").HasMaxLength(500).IsRequired();
                });
                
                entity.OwnsOne(e => e.StressLevel, sl =>
                {
                    sl.Property(p => p.Assessment).HasColumnName("StressLevel").HasMaxLength(500).IsRequired();
                });
                
                entity.OwnsOne(e => e.DietQuality, dq =>
                {
                    dq.Property(p => p.Assessment).HasColumnName("DietQuality").HasMaxLength(500).IsRequired();
                });
            });
        }
    }
}