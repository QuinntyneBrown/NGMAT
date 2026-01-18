namespace Shared.Messaging.Abstractions.Resilience;

/// <summary>
/// Configuration options for retry policies.
/// </summary>
public sealed class RetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts. Default: 3
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Initial delay between retries. Default: 100ms
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Maximum delay between retries. Default: 30s
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Backoff type for retry delays. Default: Exponential
    /// </summary>
    public BackoffType BackoffType { get; set; } = BackoffType.Exponential;

    /// <summary>
    /// Whether to add jitter to retry delays. Default: true
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Type of backoff strategy for retries.
/// </summary>
public enum BackoffType
{
    /// <summary>
    /// Constant delay between retries.
    /// </summary>
    Constant,

    /// <summary>
    /// Linear increase in delay.
    /// </summary>
    Linear,

    /// <summary>
    /// Exponential increase in delay.
    /// </summary>
    Exponential
}
