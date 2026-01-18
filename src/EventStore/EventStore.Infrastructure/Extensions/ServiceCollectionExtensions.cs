using EventStore.Core.Interfaces;
using EventStore.Core.Services;
using EventStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring Event Store infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Event Store infrastructure services to the service collection.
    /// </summary>
    public static IServiceCollection AddEventStoreInfrastructure(
        this IServiceCollection services,
        string connectionString,
        EventStoreOptions? options = null)
    {
        // Database
        services.AddDbContext<EventStoreDbContext>(opts =>
            opts.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // Unit of Work
        services.AddScoped<IEventStoreUnitOfWork, EventStoreUnitOfWork>();

        // Options
        services.AddSingleton(options ?? new EventStoreOptions());

        // Services
        services.AddScoped<EventStoreService>();
        services.AddScoped<SnapshotService>();
        services.AddScoped<SubscriptionService>();

        return services;
    }

    /// <summary>
    /// Applies pending migrations to the Event Store database.
    /// </summary>
    public static async Task MigrateEventStoreDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Ensures the Event Store database is created (for development/testing).
    /// </summary>
    public static async Task EnsureEventStoreDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
