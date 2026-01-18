namespace Shared.Messaging.Abstractions;

/// <summary>
/// Interface for subscribing to events from the event bus.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The handler to invoke when an event is received.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A subscription that can be disposed to unsubscribe.</returns>
    Task<IAsyncDisposable> SubscribeAsync<TEvent>(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to events on a specific channel.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="channel">The channel to subscribe to.</param>
    /// <param name="handler">The handler to invoke when an event is received.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A subscription that can be disposed to unsubscribe.</returns>
    Task<IAsyncDisposable> SubscribeAsync<TEvent>(
        string channel,
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to events matching a pattern (e.g., "mission.*").
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="pattern">The pattern to match channels against.</param>
    /// <param name="handler">The handler to invoke when an event is received.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A subscription that can be disposed to unsubscribe.</returns>
    Task<IAsyncDisposable> SubscribePatternAsync<TEvent>(
        string pattern,
        Func<TEvent, string, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;
}
