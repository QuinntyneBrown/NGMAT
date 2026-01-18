namespace Shared.Messaging.Abstractions.Resilience;

/// <summary>
/// Configuration options for circuit breaker.
/// </summary>
public sealed class CircuitBreakerOptions
{
    /// <summary>
    /// Number of failures before opening the circuit. Default: 5
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Time window for counting failures. Default: 30s
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Minimum number of calls before the circuit can trip. Default: 10
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Time the circuit stays open before allowing a test request. Default: 30s
    /// </summary>
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);
}
