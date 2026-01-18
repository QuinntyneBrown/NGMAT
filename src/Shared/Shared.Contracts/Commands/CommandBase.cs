using MessagePack;

namespace Shared.Contracts.Commands;

/// <summary>
/// Base interface for all commands.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Unique identifier for this command instance.
    /// </summary>
    Guid CommandId { get; }

    /// <summary>
    /// Correlation ID for request tracing.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// ID of the user issuing this command.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Timestamp when the command was created.
    /// </summary>
    DateTimeOffset Timestamp { get; }
}

/// <summary>
/// Abstract base class for all commands.
/// Derived classes should apply [MessagePackObject] and use [Key] starting from 10.
/// Keys 0-9 are reserved for base class properties.
/// </summary>
public abstract class CommandBase : ICommand
{
    /// <inheritdoc />
    [Key(0)]
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    [Key(1)]
    public string? CorrelationId { get; init; }

    /// <inheritdoc />
    [Key(2)]
    public string? UserId { get; init; }

    /// <inheritdoc />
    [Key(3)]
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
