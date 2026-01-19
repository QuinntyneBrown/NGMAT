using Microsoft.EntityFrameworkCore;
using Ephemeris.Core.Entities;

namespace Ephemeris.Infrastructure.Persistence;

public sealed class EphemerisDbContext : DbContext
{
    public DbSet<CelestialBody> CelestialBodies => Set<CelestialBody>();
    public DbSet<CelestialBodyPosition> CelestialBodyPositions => Set<CelestialBodyPosition>();
    public DbSet<EarthOrientationParameters> EarthOrientationParameters => Set<EarthOrientationParameters>();
    public DbSet<SpaceWeatherData> SpaceWeatherData => Set<SpaceWeatherData>();
    public DbSet<LeapSecond> LeapSeconds => Set<LeapSecond>();

    public EphemerisDbContext(DbContextOptions<EphemerisDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CelestialBody>(entity =>
        {
            entity.ToTable("CelestialBodies");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NaifId).IsRequired();
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.GravitationalParameterM3S2).IsRequired();
            entity.Property(e => e.MeanRadiusKm).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.NaifId).IsUnique();
            entity.HasIndex(e => e.Name);

            entity.HasOne(e => e.ParentBody)
                .WithMany()
                .HasForeignKey(e => e.ParentBodyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CelestialBodyPosition>(entity =>
        {
            entity.ToTable("CelestialBodyPositions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CelestialBodyId).IsRequired();
            entity.Property(e => e.Epoch).IsRequired();
            entity.Property(e => e.X).IsRequired();
            entity.Property(e => e.Y).IsRequired();
            entity.Property(e => e.Z).IsRequired();
            entity.Property(e => e.Vx).IsRequired();
            entity.Property(e => e.Vy).IsRequired();
            entity.Property(e => e.Vz).IsRequired();
            entity.Property(e => e.ReferenceFrame).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CenterNaifId).IsRequired();
            entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RecordedAt).IsRequired();

            entity.HasIndex(e => new { e.CelestialBodyId, e.Epoch, e.CenterNaifId });
            entity.HasIndex(e => e.Epoch);

            entity.HasOne(e => e.CelestialBody)
                .WithMany()
                .HasForeignKey(e => e.CelestialBodyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EarthOrientationParameters>(entity =>
        {
            entity.ToTable("EarthOrientationParameters");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Mjd).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.XPoleArcsec).IsRequired();
            entity.Property(e => e.YPoleArcsec).IsRequired();
            entity.Property(e => e.Ut1MinusUtcSeconds).IsRequired();
            entity.Property(e => e.LodMilliseconds).IsRequired();
            entity.Property(e => e.DPsiArcsec).IsRequired();
            entity.Property(e => e.DEpsilonArcsec).IsRequired();
            entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsPrediction).IsRequired();
            entity.Property(e => e.RecordedAt).IsRequired();

            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.Mjd);
        });

        modelBuilder.Entity<SpaceWeatherData>(entity =>
        {
            entity.ToTable("SpaceWeatherData");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.F107Observed).IsRequired();
            entity.Property(e => e.F107Adjusted).IsRequired();
            entity.Property(e => e.F107Average81Day).IsRequired();
            entity.Property(e => e.ApDaily).IsRequired();
            entity.Property(e => e.KpSum).IsRequired();
            entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsPrediction).IsRequired();
            entity.Property(e => e.RecordedAt).IsRequired();

            // Store 3-hourly arrays as JSON
            entity.Property(e => e.Ap3Hour)
                .HasConversion(
                    v => v != null ? string.Join(",", v) : null,
                    v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray() : null)
                .HasMaxLength(200);

            entity.Property(e => e.Kp3Hour)
                .HasConversion(
                    v => v != null ? string.Join(",", v) : null,
                    v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray() : null)
                .HasMaxLength(200);

            entity.HasIndex(e => e.Date);
        });

        modelBuilder.Entity<LeapSecond>(entity =>
        {
            entity.ToTable("LeapSeconds");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EffectiveDate).IsRequired();
            entity.Property(e => e.TaiMinusUtcSeconds).IsRequired();
            entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RecordedAt).IsRequired();

            entity.HasIndex(e => e.EffectiveDate).IsUnique();
        });
    }
}
