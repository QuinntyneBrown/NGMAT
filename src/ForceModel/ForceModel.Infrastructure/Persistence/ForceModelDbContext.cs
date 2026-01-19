using Microsoft.EntityFrameworkCore;
using ForceModel.Core.Entities;

namespace ForceModel.Infrastructure.Persistence;

public sealed class ForceModelDbContext : DbContext
{
    public DbSet<ForceModelConfiguration> ForceModelConfigurations => Set<ForceModelConfiguration>();

    public ForceModelDbContext(DbContextOptions<ForceModelDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ForceModelConfiguration>(entity =>
        {
            entity.ToTable("ForceModelConfigurations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MissionId);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();

            // Gravity settings
            entity.Property(e => e.EnableCentralBodyGravity).IsRequired();
            entity.Property(e => e.GravityModel)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.GravityDegree).IsRequired();
            entity.Property(e => e.GravityOrder).IsRequired();

            // Atmosphere settings
            entity.Property(e => e.EnableAtmosphericDrag).IsRequired();
            entity.Property(e => e.AtmosphereModel)
                .HasConversion<string>()
                .HasMaxLength(50);

            // SRP settings
            entity.Property(e => e.EnableSolarRadiationPressure).IsRequired();
            entity.Property(e => e.SrpModel)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.EnableEclipsing).IsRequired();

            // Third body settings
            entity.Property(e => e.EnableThirdBodySun).IsRequired();
            entity.Property(e => e.EnableThirdBodyMoon).IsRequired();
            entity.Property(e => e.EnableThirdBodyPlanets).IsRequired();

            // Advanced settings
            entity.Property(e => e.EnableRelativisticCorrections).IsRequired();
            entity.Property(e => e.EnableSolidEarthTides).IsRequired();
            entity.Property(e => e.EnableOceanTides).IsRequired();

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasIndex(e => e.MissionId);
        });
    }
}
