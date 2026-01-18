using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(512);

        builder.Property(p => p.Resource)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(p => p.Action)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(p => new { p.Resource, p.Action })
            .IsUnique();

        builder.HasMany(p => p.Roles)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
