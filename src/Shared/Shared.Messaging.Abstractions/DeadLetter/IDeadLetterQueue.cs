namespace Shared.Messaging.Abstractions.DeadLetter;

/// <summary>
/// Interface for dead letter queue operations.
/// </summary>
public interface IDeadLetterQueue
{
    /// <summary>
    /// Adds a failed message to the dead letter queue.
    /// </summary>
    Task EnqueueAsync(DeadLetterMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves messages from the dead letter queue.
    /// </summary>
    Task<IReadOnlyList<DeadLetterMessage>> GetMessagesAsync(
        int count = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a dead letter message.
    /// </summary>
    Task<bool> RetryAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a message from the dead letter queue.
    /// </summary>
    Task<bool> RemoveAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of messages in the dead letter queue.
    /// </summary>
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a message in the dead letter queue.
/// </summary>
public sealed record DeadLetterMessage
{
    /// <summary>
    /// Unique ID for this dead letter entry.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The original event ID.
    /// </summary>
    public Guid OriginalEventId { get; init; }

    /// <summary>
    /// The event type name.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// The channel the event was published to.
    /// </summary>
    public string Channel { get; init; } = string.Empty;

    /// <summary>
    /// The serialized event payload.
    /// </summary>
    public byte[] Payload { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// The error that caused the message to be dead-lettered.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// The exception type.
    /// </summary>
    public string? ExceptionType { get; init; }

    /// <summary>
    /// Stack trace of the error.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// When the message was dead-lettered.
    /// </summary>
    public DateTimeOffset DeadLetteredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Number of retry attempts.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Maximum retry attempts allowed.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Whether the message can be retried.
    /// </summary>
    public bool CanRetry => RetryCount < MaxRetries;
}
