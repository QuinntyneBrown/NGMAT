using Microsoft.EntityFrameworkCore;
using MissionManagement.Core.Entities;

namespace MissionManagement.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for the Mission Management service.
/// </summary>
public sealed class MissionManagementDbContext : DbContext
{
    public MissionManagementDbContext(DbContextOptions<MissionManagementDbContext> options) : base(options)
    {
    }

    public DbSet<Mission> Missions => Set<Mission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MissionManagementDbContext).Assembly);
    }
}
