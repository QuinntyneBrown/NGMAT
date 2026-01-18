using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MissionManagement.Core.Entities;

namespace MissionManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Mission entity.
/// </summary>
internal sealed class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.ToTable("Missions");

        builder.HasKey(m => m.MissionId);

        builder.Property(m => m.MissionId)
            .IsRequired();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.StartEpoch)
            .IsRequired();

        builder.Property(m => m.EndEpoch);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.OwnerId)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        builder.Property(m => m.IsDeleted)
            .IsRequired();

        builder.Property(m => m.DeletedAt);

        // Indexes
        builder.HasIndex(m => m.OwnerId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.IsDeleted);
        builder.HasIndex(m => new { m.OwnerId, m.Name });

        // Query filter for soft deletes
        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
