using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spacecraft.Core.Interfaces;
using Spacecraft.Core.Services;
using Spacecraft.Infrastructure.Persistence;
using Spacecraft.Infrastructure.Repositories;

namespace Spacecraft.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpacecraft(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<SpacecraftDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Unit of Work
        services.AddScoped<ISpacecraftUnitOfWork, SpacecraftUnitOfWork>();

        // Add services
        services.AddScoped<SpacecraftService>();

        return services;
    }
}
