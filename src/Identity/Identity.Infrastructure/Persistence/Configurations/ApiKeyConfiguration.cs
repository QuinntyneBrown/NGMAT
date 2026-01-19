using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

internal sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("ApiKeys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(k => k.KeyHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(k => k.KeyPrefix)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(k => k.KeyHash)
            .IsUnique();

        builder.HasOne(k => k.User)
            .WithMany()
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Store scopes as JSON
        builder.Property(k => k.Scopes)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly());

        // Ignore computed properties
        builder.Ignore(k => k.IsExpired);
        builder.Ignore(k => k.IsValid);
    }
}
