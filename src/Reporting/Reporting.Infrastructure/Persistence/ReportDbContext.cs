using Microsoft.EntityFrameworkCore;
using Reporting.Core.Entities;

namespace Reporting.Infrastructure.Persistence;

public sealed class ReportDbContext : DbContext
{
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();

    public ReportDbContext(DbContextOptions<ReportDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("Reports");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Format).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            
            entity.Property(e => e.MissionId);
            entity.Property(e => e.SpacecraftId);
            
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.StoragePath).HasMaxLength(500);
            entity.Property(e => e.FileSizeBytes);
            
            entity.Property(e => e.Parameters);
            
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CompletedAt);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.IsDeleted).IsRequired();

            // Indexes
            entity.HasIndex(e => e.MissionId);
            entity.HasIndex(e => e.SpacecraftId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });

        modelBuilder.Entity<ReportTemplate>(entity =>
        {
            entity.ToTable("ReportTemplates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Format).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Schema);
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsDeleted).IsRequired();

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
        });
    }
}
