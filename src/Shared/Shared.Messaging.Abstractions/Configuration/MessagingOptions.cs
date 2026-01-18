using Shared.Messaging.Abstractions.Resilience;

namespace Shared.Messaging.Abstractions.Configuration;

/// <summary>
/// Root configuration options for messaging.
/// </summary>
public sealed class MessagingOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// The messaging provider to use. Options: "UdpMulticast", "Redis"
    /// </summary>
    public string Provider { get; set; } = "UdpMulticast";

    /// <summary>
    /// Name of the service using messaging (for logging/tracing).
    /// </summary>
    public string ServiceName { get; set; } = "Unknown";

    /// <summary>
    /// Whether to enable distributed tracing.
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Whether to enable metrics collection.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Default retry options for event handlers.
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Default circuit breaker options for event handlers.
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Whether to use the dead letter queue for failed messages.
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;

    /// <summary>
    /// Maximum size of the dead letter queue per channel.
    /// </summary>
    public int DeadLetterQueueMaxSize { get; set; } = 10000;
}
