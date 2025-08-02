using Microsoft.EntityFrameworkCore;

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
            });
        }
    }
}