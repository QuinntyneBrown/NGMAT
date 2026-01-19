using MessagePack;
using Shared.Messaging.Abstractions;

namespace ScriptExecution.Core.Events;

[MessagePackObject]
public sealed class ScriptExecutionStartedEvent : EventBase
{
    [Key(10)]
    public Guid JobId { get; init; }

    [Key(11)]
    public Guid? ScriptId { get; init; }

    public ScriptExecutionStartedEvent() : base()
    {
        SourceService = "ScriptExecution";
    }
}

[MessagePackObject]
public sealed class ScriptExecutionCompletedEvent : EventBase
{
    [Key(10)]
    public Guid JobId { get; init; }

    [Key(11)]
    public bool Success { get; init; }

    [Key(12)]
    public long ExecutionTimeMs { get; init; }

    public ScriptExecutionCompletedEvent() : base()
    {
        SourceService = "ScriptExecution";
    }
}

[MessagePackObject]
public sealed class ScriptExecutionFailedEvent : EventBase
{
    [Key(10)]
    public Guid JobId { get; init; }

    [Key(11)]
    public string ErrorMessage { get; init; } = string.Empty;

    [Key(12)]
    public int LineNumber { get; init; }

    public ScriptExecutionFailedEvent() : base()
    {
        SourceService = "ScriptExecution";
    }
}

[MessagePackObject]
public sealed class ScriptSavedEvent : EventBase
{
    [Key(10)]
    public Guid ScriptId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string CreatedByUserId { get; init; } = string.Empty;

    public ScriptSavedEvent() : base()
    {
        SourceService = "ScriptExecution";
    }
}
