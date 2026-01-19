using CoordinateSystem.Core.Interfaces;
using CoordinateSystem.Core.Services;
using CoordinateSystem.Infrastructure.Persistence;
using CoordinateSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoordinateSystem.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoordinateSystem(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<CoordinateSystemDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Unit of Work
        services.AddScoped<ICoordinateSystemUnitOfWork, CoordinateSystemUnitOfWork>();

        // Add services
        services.AddSingleton<CoordinateTransformService>();
        services.AddSingleton<OrbitalElementsService>();

        return services;
    }
}
