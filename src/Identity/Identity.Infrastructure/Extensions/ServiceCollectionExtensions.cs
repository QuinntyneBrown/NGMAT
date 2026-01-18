using Identity.Core.Interfaces;
using Identity.Core.Services;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring Identity infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Identity infrastructure services to the service collection.
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString,
        JwtOptions jwtOptions,
        AuthenticationOptions? authOptions = null)
    {
        // Database
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // Repositories and Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IMfaService, TotpMfaService>();
        services.AddSingleton(jwtOptions);
        services.AddSingleton<ITokenService, JwtTokenService>();

        // Authentication service
        services.AddSingleton(authOptions ?? new AuthenticationOptions());
        services.AddScoped<AuthenticationService>();

        return services;
    }

    /// <summary>
    /// Applies pending migrations to the Identity database.
    /// </summary>
    public static async Task MigrateIdentityDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Ensures the Identity database is created (for development/testing).
    /// </summary>
    public static async Task EnsureIdentityDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
