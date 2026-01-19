using Microsoft.EntityFrameworkCore;
using Maneuver.Core.Entities;

namespace Maneuver.Infrastructure.Persistence;

public sealed class ManeuverDbContext : DbContext
{
    public DbSet<ManeuverPlan> ManeuverPlans => Set<ManeuverPlan>();

    public ManeuverDbContext(DbContextOptions<ManeuverDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ManeuverPlan>(entity =>
        {
            entity.ToTable("ManeuverPlans");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SpacecraftId).IsRequired();
            entity.Property(e => e.MissionId);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.IsDeleted).IsRequired();

            // Maneuver type and status
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Timing
            entity.Property(e => e.PlannedEpoch).IsRequired();
            entity.Property(e => e.ExecutedEpoch);

            // Delta-V components
            entity.Property(e => e.DeltaVx).IsRequired();
            entity.Property(e => e.DeltaVy).IsRequired();
            entity.Property(e => e.DeltaVz).IsRequired();
            entity.Property(e => e.CoordinateFrame)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Finite burn parameters
            entity.Property(e => e.ThrustMagnitudeN);
            entity.Property(e => e.BurnDurationSeconds);
            entity.Property(e => e.SpecificImpulseS);

            // Fuel tracking
            entity.Property(e => e.EstimatedFuelMassKg).IsRequired();
            entity.Property(e => e.ActualFuelMassKg);
            entity.Property(e => e.SpacecraftMassBeforeKg).IsRequired();
            entity.Property(e => e.SpacecraftMassAfterKg).IsRequired();

            // Indexes
            entity.HasIndex(e => e.SpacecraftId);
            entity.HasIndex(e => e.MissionId);
            entity.HasIndex(e => e.PlannedEpoch);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.SpacecraftId, e.PlannedEpoch });
        });
    }
}
