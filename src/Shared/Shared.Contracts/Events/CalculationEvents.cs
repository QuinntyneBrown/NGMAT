using MessagePack;
using Shared.Messaging.Abstractions;

namespace Shared.Contracts.Events;

/// <summary>
/// Event raised when a calculation is requested.
/// </summary>
[MessagePackObject]
public sealed class CalculationRequested : EventBase
{
    [Key(10)]
    public Guid CalculationId { get; init; }

    [Key(11)]
    public string CalculationType { get; init; } = string.Empty;

    [Key(12)]
    public string InputParameters { get; init; } = string.Empty;

    [Key(13)]
    public string RequestedBy { get; init; } = string.Empty;

    [Key(14)]
    public int Priority { get; init; }
}

/// <summary>
/// Event raised when a calculation completes successfully.
/// </summary>
[MessagePackObject]
public sealed class CalculationCompleted : EventBase
{
    [Key(10)]
    public Guid CalculationId { get; init; }

    [Key(11)]
    public string CalculationType { get; init; } = string.Empty;

    [Key(12)]
    public string ResultSummary { get; init; } = string.Empty;

    [Key(13)]
    public string? ResultLocation { get; init; }

    [Key(14)]
    public double ElapsedMilliseconds { get; init; }
}

/// <summary>
/// Event raised when a calculation fails.
/// </summary>
[MessagePackObject]
public sealed class CalculationFailed : EventBase
{
    [Key(10)]
    public Guid CalculationId { get; init; }

    [Key(11)]
    public string CalculationType { get; init; } = string.Empty;

    [Key(12)]
    public string ErrorCode { get; init; } = string.Empty;

    [Key(13)]
    public string ErrorMessage { get; init; } = string.Empty;

    [Key(14)]
    public string? StackTrace { get; init; }
}
