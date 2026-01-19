using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ephemeris.Core.Interfaces;
using Ephemeris.Core.Services;
using Ephemeris.Infrastructure.Persistence;
using Ephemeris.Infrastructure.Repositories;

namespace Ephemeris.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEphemeris(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<EphemerisDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Unit of Work
        services.AddScoped<IEphemerisUnitOfWork, EphemerisUnitOfWork>();

        // Add services
        services.AddScoped<EphemerisService>();
        services.AddScoped<EarthOrientationService>();
        services.AddScoped<SpaceWeatherService>();
        services.AddScoped<TimeConversionService>();

        return services;
    }
}
