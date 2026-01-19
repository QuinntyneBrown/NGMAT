namespace ScriptExecution.Core.Models;

/// <summary>
/// Script execution status
/// </summary>
public enum ScriptStatus
{
    Queued,
    Validating,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Script command types
/// </summary>
public enum CommandType
{
    Comment,
    Create,
    Set,
    Propagate,
    Maneuver,
    Report,
    Save,
    If,
    Else,
    EndIf,
    While,
    EndWhile,
    Variable,
    Function,
    Unknown
}

/// <summary>
/// Script stored in library
/// </summary>
public sealed class Script
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Content { get; init; } = string.Empty;
    public string CreatedByUserId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublic { get; init; }
    public List<string> Tags { get; init; } = new();
    public int Version { get; init; } = 1;
}

/// <summary>
/// Script execution job
/// </summary>
public sealed class ScriptJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? ScriptId { get; init; }
    public string? InlineScript { get; init; }
    public string CreatedByUserId { get; init; } = string.Empty;
    public ScriptStatus Status { get; set; }
    public double ProgressPercent { get; set; }
    public int CurrentLine { get; set; }
    public string? CurrentCommand { get; set; }
    public List<string> OutputLog { get; init; } = new();
    public List<ScriptError> Errors { get; init; } = new();
    public Dictionary<string, object> Variables { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Script parsing result
/// </summary>
public sealed class ParseResult
{
    public bool IsValid { get; init; }
    public List<AstNode> Ast { get; init; } = new();
    public List<ScriptError> Errors { get; init; } = new();
    public List<ScriptWarning> Warnings { get; init; } = new();
    public int LineCount { get; init; }
    public int CommandCount { get; init; }
}

/// <summary>
/// Validation result
/// </summary>
public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public List<ScriptError> Errors { get; init; } = new();
    public List<ScriptWarning> Warnings { get; init; } = new();
}

/// <summary>
/// Script error
/// </summary>
public sealed class ScriptError
{
    public int LineNumber { get; init; }
    public int Column { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Code { get; init; }
    public ScriptErrorSeverity Severity { get; init; } = ScriptErrorSeverity.Error;
}

public enum ScriptErrorSeverity
{
    Error,
    Warning,
    Info
}

/// <summary>
/// Script warning
/// </summary>
public sealed class ScriptWarning
{
    public int LineNumber { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Suggestion { get; init; }
}

/// <summary>
/// Abstract Syntax Tree node
/// </summary>
public class AstNode
{
    public CommandType Type { get; init; }
    public int LineNumber { get; init; }
    public string RawText { get; init; } = string.Empty;
    public Dictionary<string, object> Properties { get; init; } = new();
    public List<AstNode> Children { get; init; } = new();
}

/// <summary>
/// Create command node
/// </summary>
public sealed class CreateNode : AstNode
{
    public string ObjectType { get; init; } = string.Empty;
    public string ObjectName { get; init; } = string.Empty;

    public CreateNode()
    {
        Type = CommandType.Create;
    }
}

/// <summary>
/// Set property command node
/// </summary>
public sealed class SetNode : AstNode
{
    public string ObjectName { get; init; } = string.Empty;
    public string PropertyName { get; init; } = string.Empty;
    public object Value { get; init; } = null!;

    public SetNode()
    {
        Type = CommandType.Set;
    }
}

/// <summary>
/// Propagate command node
/// </summary>
public sealed class PropagateNode : AstNode
{
    public string PropagatorName { get; init; } = string.Empty;
    public string SpacecraftName { get; init; } = string.Empty;
    public string? StopCondition { get; init; }
    public double? StopValue { get; init; }

    public PropagateNode()
    {
        Type = CommandType.Propagate;
    }
}

/// <summary>
/// Report command node
/// </summary>
public sealed class ReportNode : AstNode
{
    public List<string> Parameters { get; init; } = new();

    public ReportNode()
    {
        Type = CommandType.Report;
    }
}

/// <summary>
/// Variable declaration node
/// </summary>
public sealed class VariableNode : AstNode
{
    public string VariableName { get; init; } = string.Empty;
    public object Value { get; init; } = null!;

    public VariableNode()
    {
        Type = CommandType.Variable;
    }
}

/// <summary>
/// Script execution context
/// </summary>
public sealed class ScriptExecutionContext
{
    public Dictionary<string, ScriptObject> Objects { get; } = new();
    public Dictionary<string, object> Variables { get; } = new();
    public List<string> OutputLog { get; } = new();
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// Generic script object
/// </summary>
public class ScriptObject
{
    public string Type { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Dictionary<string, object> Properties { get; } = new();
}

/// <summary>
/// Spacecraft script object
/// </summary>
public sealed class SpacecraftObject : ScriptObject
{
    public DateTime Epoch { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double VX { get; set; }
    public double VY { get; set; }
    public double VZ { get; set; }
    public double FuelMass { get; set; }
    public double DryMass { get; set; }

    public SpacecraftObject()
    {
        Type = "Spacecraft";
    }
}

/// <summary>
/// Propagator script object
/// </summary>
public sealed class PropagatorObject : ScriptObject
{
    public string PropagatorType { get; set; } = "RK4";
    public double StepSize { get; set; } = 60;
    public double MinStep { get; set; } = 1;
    public double MaxStep { get; set; } = 300;
    public double Accuracy { get; set; } = 1e-10;

    public PropagatorObject()
    {
        Type = "Propagator";
    }
}
