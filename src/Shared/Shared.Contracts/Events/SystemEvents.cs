using MessagePack;
using Shared.Messaging.Abstractions;

namespace Shared.Contracts.Events;

/// <summary>
/// Event raised when a service starts.
/// </summary>
[MessagePackObject]
public sealed class ServiceStarted : EventBase
{
    [Key(10)]
    public string ServiceName { get; init; } = string.Empty;

    [Key(11)]
    public string ServiceVersion { get; init; } = string.Empty;

    [Key(12)]
    public string InstanceId { get; init; } = string.Empty;

    [Key(13)]
    public string? HostName { get; init; }

    [Key(14)]
    public string Environment { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a service stops.
/// </summary>
[MessagePackObject]
public sealed class ServiceStopped : EventBase
{
    [Key(10)]
    public string ServiceName { get; init; } = string.Empty;

    [Key(11)]
    public string InstanceId { get; init; } = string.Empty;

    [Key(12)]
    public string StopReason { get; init; } = string.Empty;

    [Key(13)]
    public int ExitCode { get; init; }
}

/// <summary>
/// Event raised when a health check fails.
/// </summary>
[MessagePackObject]
public sealed class HealthCheckFailed : EventBase
{
    [Key(10)]
    public string ServiceName { get; init; } = string.Empty;

    [Key(11)]
    public string CheckName { get; init; } = string.Empty;

    [Key(12)]
    public string Status { get; init; } = string.Empty;

    [Key(13)]
    public string? ErrorMessage { get; init; }

    [Key(14)]
    public double DurationMs { get; init; }
}

/// <summary>
/// Event raised when configuration changes.
/// </summary>
[MessagePackObject]
public sealed class ConfigurationChanged : EventBase
{
    [Key(10)]
    public string ServiceName { get; init; } = string.Empty;

    [Key(11)]
    public string ConfigurationKey { get; init; } = string.Empty;

    [Key(12)]
    public string? PreviousValue { get; init; }

    [Key(13)]
    public string? NewValue { get; init; }

    [Key(14)]
    public string ChangedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when an error occurs that should be monitored.
/// </summary>
[MessagePackObject]
public sealed class ErrorOccurred : EventBase
{
    [Key(10)]
    public string ServiceName { get; init; } = string.Empty;

    [Key(11)]
    public string ErrorCode { get; init; } = string.Empty;

    [Key(12)]
    public string ErrorMessage { get; init; } = string.Empty;

    [Key(13)]
    public string? StackTrace { get; init; }

    [Key(14)]
    public string Severity { get; init; } = string.Empty;

    [Key(15)]
    public string? Context { get; init; }
}
