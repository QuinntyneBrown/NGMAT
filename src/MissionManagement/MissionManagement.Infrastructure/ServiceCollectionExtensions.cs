using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MissionManagement.Core.Interfaces;
using MissionManagement.Core.Services;
using MissionManagement.Infrastructure.Persistence;
using MissionManagement.Infrastructure.Repositories;

namespace MissionManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMissionManagement(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<MissionDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IMissionRepository, MissionRepository>();
        services.AddScoped<IMissionUnitOfWork, MissionUnitOfWork>();
        services.AddScoped<MissionService>();

        return services;
    }
}
