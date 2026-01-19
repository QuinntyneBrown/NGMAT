using Microsoft.EntityFrameworkCore;
using Propagation.Core.Entities;

namespace Propagation.Infrastructure.Persistence;

public sealed class PropagationDbContext : DbContext
{
    public DbSet<PropagationConfiguration> PropagationConfigurations => Set<PropagationConfiguration>();

    public PropagationDbContext(DbContextOptions<PropagationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PropagationConfiguration>(entity =>
        {
            entity.ToTable("PropagationConfigurations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MissionId);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();

            // Integrator settings
            entity.Property(e => e.Integrator)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.InitialStepSizeSeconds).IsRequired();
            entity.Property(e => e.MinStepSizeSeconds).IsRequired();
            entity.Property(e => e.MaxStepSizeSeconds).IsRequired();
            entity.Property(e => e.RelativeTolerance).IsRequired();
            entity.Property(e => e.AbsoluteTolerance).IsRequired();

            // Output settings
            entity.Property(e => e.OutputMode)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.OutputStepSizeSeconds).IsRequired();

            // Force model reference
            entity.Property(e => e.ForceModelConfigurationId);

            // Stopping conditions
            entity.Property(e => e.MaxPropagationDurationSeconds);
            entity.Property(e => e.MaxStepCount);
            entity.Property(e => e.MinAltitudeMeters);

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasIndex(e => e.MissionId);
        });
    }
}
