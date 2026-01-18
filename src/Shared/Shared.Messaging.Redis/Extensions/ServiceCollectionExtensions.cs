using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Abstractions.Serialization;

namespace Shared.Messaging.Redis.Extensions;

/// <summary>
/// Extension methods for registering Redis event bus services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Redis event bus to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisEventBus(
        this IServiceCollection services,
        Action<RedisOptions>? configure = null)
    {
        // Register options
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<RedisOptions>();
        }

        // Register serializer
        services.TryAddSingleton<IMessageSerializer, MessagePackEventSerializer>();

        // Register event bus
        services.AddSingleton<RedisEventBus>();
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RedisEventBus>());
        services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<RedisEventBus>());
        services.AddSingleton<IEventSubscriber>(sp => sp.GetRequiredService<RedisEventBus>());

        return services;
    }

    /// <summary>
    /// Adds the Redis event bus with connection string.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">Redis connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisEventBus(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddRedisEventBus(options =>
        {
            options.ConnectionString = connectionString;
        });
    }

    /// <summary>
    /// Adds an event handler to the service collection.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : class, IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, THandler>();
        return services;
    }
}
