using CoordinateSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoordinateSystem.Infrastructure.Persistence;

public sealed class CoordinateSystemDbContext : DbContext
{
    public DbSet<ReferenceFrame> ReferenceFrames => Set<ReferenceFrame>();

    public CoordinateSystemDbContext(DbContextOptions<CoordinateSystemDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReferenceFrame>(entity =>
        {
            entity.ToTable("ReferenceFrames");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.CentralBody)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Axes)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Origin)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Epoch);

            entity.Property(e => e.IsInertial)
                .IsRequired();

            entity.Property(e => e.IsBuiltIn)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.CreatedByUserId);

            // Unique index on name
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        // Seed built-in reference frames
        SeedBuiltInFrames(modelBuilder);
    }

    private static void SeedBuiltInFrames(ModelBuilder modelBuilder)
    {
        // Note: Since ReferenceFrame uses private setters and factory methods,
        // we need to use raw data for seeding. The actual seeding happens
        // in the repository using the BuiltInFrames class.
    }
}
