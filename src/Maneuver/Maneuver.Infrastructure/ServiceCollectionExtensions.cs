using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Maneuver.Core.Interfaces;
using Maneuver.Core.Services;
using Maneuver.Infrastructure.Persistence;
using Maneuver.Infrastructure.Repositories;

namespace Maneuver.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManeuver(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<ManeuverDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Unit of Work
        services.AddScoped<IManeuverUnitOfWork, ManeuverUnitOfWork>();

        // Add services
        services.AddScoped<ManeuverService>();

        return services;
    }
}
