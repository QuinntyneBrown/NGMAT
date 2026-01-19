using Microsoft.EntityFrameworkCore;
using Spacecraft.Core.Entities;

namespace Spacecraft.Infrastructure.Persistence;

public sealed class SpacecraftDbContext : DbContext
{
    public DbSet<SpacecraftEntity> Spacecraft => Set<SpacecraftEntity>();
    public DbSet<SpacecraftState> SpacecraftStates => Set<SpacecraftState>();
    public DbSet<Thruster> Thrusters => Set<Thruster>();
    public DbSet<FuelTank> FuelTanks => Set<FuelTank>();
    public DbSet<SolarPanel> SolarPanels => Set<SolarPanel>();
    public DbSet<Battery> Batteries => Set<Battery>();

    public SpacecraftDbContext(DbContextOptions<SpacecraftDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SpacecraftEntity>(entity =>
        {
            entity.ToTable("Spacecraft");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MissionId).IsRequired();

            entity.Property(e => e.DryMassKg).IsRequired();
            entity.Property(e => e.FuelMassKg).IsRequired();
            entity.Property(e => e.DragCoefficient).IsRequired();
            entity.Property(e => e.DragAreaM2).IsRequired();
            entity.Property(e => e.SrpAreaM2).IsRequired();
            entity.Property(e => e.ReflectivityCoefficient).IsRequired();

            entity.Property(e => e.InitialEpoch).IsRequired();
            entity.Property(e => e.InitialX).IsRequired();
            entity.Property(e => e.InitialY).IsRequired();
            entity.Property(e => e.InitialZ).IsRequired();
            entity.Property(e => e.InitialVx).IsRequired();
            entity.Property(e => e.InitialVy).IsRequired();
            entity.Property(e => e.InitialVz).IsRequired();
            entity.Property(e => e.CoordinateFrameId).IsRequired();

            entity.Property(e => e.AttitudeMode)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedByUserId).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();

            entity.HasIndex(e => new { e.MissionId, e.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasMany(e => e.Thrusters)
                .WithOne()
                .HasForeignKey(t => t.SpacecraftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.FuelTanks)
                .WithOne()
                .HasForeignKey(t => t.SpacecraftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.StateHistory)
                .WithOne()
                .HasForeignKey(s => s.SpacecraftId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SpacecraftState>(entity =>
        {
            entity.ToTable("SpacecraftStates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SpacecraftId).IsRequired();
            entity.Property(e => e.Epoch).IsRequired();
            entity.Property(e => e.X).IsRequired();
            entity.Property(e => e.Y).IsRequired();
            entity.Property(e => e.Z).IsRequired();
            entity.Property(e => e.Vx).IsRequired();
            entity.Property(e => e.Vy).IsRequired();
            entity.Property(e => e.Vz).IsRequired();
            entity.Property(e => e.FuelMassKg).IsRequired();
            entity.Property(e => e.CoordinateFrameId).IsRequired();
            entity.Property(e => e.RecordedAt).IsRequired();

            entity.HasIndex(e => new { e.SpacecraftId, e.Epoch });
        });

        modelBuilder.Entity<Thruster>(entity =>
        {
            entity.ToTable("Thrusters");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SpacecraftId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.ThrustN).IsRequired();
            entity.Property(e => e.IspSeconds).IsRequired();
            entity.Property(e => e.MassKg).IsRequired();
            entity.Property(e => e.FuelType)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.IsOperational).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<FuelTank>(entity =>
        {
            entity.ToTable("FuelTanks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SpacecraftId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FuelType)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.CapacityKg).IsRequired();
            entity.Property(e => e.CurrentMassKg).IsRequired();
            entity.Property(e => e.PressurePa).IsRequired();
            entity.Property(e => e.MassKg).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<SolarPanel>(entity =>
        {
            entity.ToTable("SolarPanels");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SpacecraftId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AreaM2).IsRequired();
            entity.Property(e => e.EfficiencyPercent).IsRequired();
            entity.Property(e => e.MaxPowerWatts).IsRequired();
            entity.Property(e => e.MassKg).IsRequired();
            entity.Property(e => e.IsDeployed).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Battery>(entity =>
        {
            entity.ToTable("Batteries");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SpacecraftId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.CapacityWattHours).IsRequired();
            entity.Property(e => e.CurrentChargeWattHours).IsRequired();
            entity.Property(e => e.MassKg).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
