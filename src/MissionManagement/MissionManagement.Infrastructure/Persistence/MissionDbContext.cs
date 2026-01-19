using Microsoft.EntityFrameworkCore;
using MissionManagement.Core.Entities;

namespace MissionManagement.Infrastructure.Persistence;

public sealed class MissionDbContext : DbContext
{
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<MissionShare> MissionShares => Set<MissionShare>();
    public DbSet<MissionStatusHistory> MissionStatusHistory => Set<MissionStatusHistory>();

    public MissionDbContext(DbContextOptions<MissionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Mission>(entity =>
        {
            entity.ToTable("Missions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.StartEpoch).IsRequired();
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();

            entity.HasIndex(e => new { e.OwnerId, e.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsDeleted);

            entity.HasMany(e => e.Shares)
                .WithOne()
                .HasForeignKey(s => s.MissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.StatusHistory)
                .WithOne()
                .HasForeignKey(h => h.MissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MissionShare>(entity =>
        {
            entity.ToTable("MissionShares");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.MissionId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Permission)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(e => e.SharedAt).IsRequired();
            entity.Property(e => e.SharedByUserId).IsRequired();
            entity.Property(e => e.IsRevoked).IsRequired();

            entity.HasIndex(e => new { e.MissionId, e.UserId });
        });

        modelBuilder.Entity<MissionStatusHistory>(entity =>
        {
            entity.ToTable("MissionStatusHistory");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.MissionId).IsRequired();
            entity.Property(e => e.FromStatus)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.ToStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.ChangedAt).IsRequired();
            entity.Property(e => e.ChangedByUserId).IsRequired();

            entity.HasIndex(e => e.MissionId);
        });
    }
}
