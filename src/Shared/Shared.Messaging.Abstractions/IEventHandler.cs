namespace Shared.Messaging.Abstractions;

/// <summary>
/// Interface for handling events of a specific type.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : class, IEvent
{
    /// <summary>
    /// Handles the event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for handlers that can handle multiple event types.
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Gets the event types this handler can process.
    /// </summary>
    IEnumerable<Type> HandledEventTypes { get; }

    /// <summary>
    /// Handles an event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(IEvent @event, CancellationToken cancellationToken = default);
}
