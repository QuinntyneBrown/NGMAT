using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Abstractions.Serialization;

namespace Shared.Messaging.UdpMulticast.Extensions;

/// <summary>
/// Extension methods for registering UDP Multicast event bus services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the UDP Multicast event bus to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUdpMulticastEventBus(
        this IServiceCollection services,
        Action<UdpMulticastOptions>? configure = null)
    {
        // Register options
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<UdpMulticastOptions>();
        }

        // Register serializer
        services.TryAddSingleton<IMessageSerializer, MessagePackEventSerializer>();

        // Register event bus
        services.AddSingleton<UdpMulticastEventBus>();
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<UdpMulticastEventBus>());
        services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<UdpMulticastEventBus>());
        services.AddSingleton<IEventSubscriber>(sp => sp.GetRequiredService<UdpMulticastEventBus>());

        return services;
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
