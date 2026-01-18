using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MissionManagement.Core.Interfaces;
using MissionManagement.Infrastructure.Handlers;
using MissionManagement.Infrastructure.Persistence;
using MissionManagement.Infrastructure.Persistence.Repositories;

namespace MissionManagement.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mission Management infrastructure services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddMissionManagementInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // Add DbContext
        services.AddDbContext<MissionManagementDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add repositories
        services.AddScoped<IMissionRepository, MissionRepository>();

        // Add command handlers
        services.AddScoped<CreateMissionCommandHandler>();
        services.AddScoped<UpdateMissionCommandHandler>();
        services.AddScoped<DeleteMissionCommandHandler>();

        // Add query handlers
        services.AddScoped<GetMissionByIdQueryHandler>();
        services.AddScoped<ListMissionsQueryHandler>();

        return services;
    }
}
