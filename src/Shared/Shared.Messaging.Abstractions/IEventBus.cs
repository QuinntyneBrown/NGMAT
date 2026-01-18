namespace Shared.Messaging.Abstractions;

/// <summary>
/// Combined interface for full event bus functionality (publish and subscribe).
/// </summary>
public interface IEventBus : IEventPublisher, IEventSubscriber, IAsyncDisposable
{
    /// <summary>
    /// Gets the name of the event bus provider (e.g., "Redis", "UdpMulticast").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets whether the event bus is currently connected and operational.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Starts the event bus and establishes connections.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the event bus and closes connections gracefully.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
}
