using Shared.Domain.Results;
using Shared.Messaging.Abstractions;
using ScriptExecution.Core.Events;
using ScriptExecution.Core.Interfaces;
using ScriptExecution.Core.Models;

namespace ScriptExecution.Core.Services;

/// <summary>
/// Service for executing GMAT-compatible scripts
/// </summary>
public sealed class ScriptExecutionService
{
    private readonly ScriptParser _parser;
    private readonly IScriptRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly Dictionary<Guid, ScriptJob> _runningJobs = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _jobCancellation = new();

    public ScriptExecutionService(
        ScriptParser parser,
        IScriptRepository repository,
        IEventPublisher eventPublisher)
    {
        _parser = parser;
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Parse a script without executing
    /// </summary>
    public Result<ParseResult> ParseScript(string scriptContent)
    {
        try
        {
            var result = _parser.Parse(scriptContent);
            return Result<ParseResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<ParseResult>.Failure(Error.Internal($"Parse error: {ex.Message}", ex));
        }
    }

    /// <summary>
    /// Validate a script
    /// </summary>
    public Result<ValidationResult> ValidateScript(string scriptContent)
    {
        try
        {
            var parseResult = _parser.Parse(scriptContent);
            var errors = new List<ScriptError>(parseResult.Errors);
            var warnings = new List<ScriptWarning>(parseResult.Warnings);

            // Semantic validation
            var declaredObjects = new HashSet<string>();
            var declaredVariables = new HashSet<string>();

            foreach (var node in parseResult.Ast)
            {
                switch (node)
                {
                    case CreateNode createNode:
                        if (declaredObjects.Contains(createNode.ObjectName))
                        {
                            warnings.Add(new ScriptWarning
                            {
                                LineNumber = node.LineNumber,
                                Message = $"Object '{createNode.ObjectName}' is already declared",
                                Suggestion = "Consider using a different name or removing the duplicate declaration"
                            });
                        }
                        declaredObjects.Add(createNode.ObjectName);
                        break;

                    case SetNode setNode:
                        if (!declaredObjects.Contains(setNode.ObjectName) && !declaredVariables.Contains(setNode.ObjectName))
                        {
                            errors.Add(new ScriptError
                            {
                                LineNumber = node.LineNumber,
                                Column = 1,
                                Message = $"Object '{setNode.ObjectName}' is not declared",
                                Code = "SE002",
                                Severity = ScriptErrorSeverity.Error
                            });
                        }
                        break;

                    case PropagateNode propNode:
                        if (!declaredObjects.Contains(propNode.PropagatorName))
                        {
                            errors.Add(new ScriptError
                            {
                                LineNumber = node.LineNumber,
                                Column = 1,
                                Message = $"Propagator '{propNode.PropagatorName}' is not declared",
                                Code = "SE003",
                                Severity = ScriptErrorSeverity.Error
                            });
                        }
                        if (!declaredObjects.Contains(propNode.SpacecraftName))
                        {
                            errors.Add(new ScriptError
                            {
                                LineNumber = node.LineNumber,
                                Column = 1,
                                Message = $"Spacecraft '{propNode.SpacecraftName}' is not declared",
                                Code = "SE004",
                                Severity = ScriptErrorSeverity.Error
                            });
                        }
                        break;

                    case VariableNode varNode:
                        declaredVariables.Add(varNode.VariableName);
                        break;
                }
            }

            return Result<ValidationResult>.Success(new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            });
        }
        catch (Exception ex)
        {
            return Result<ValidationResult>.Failure(Error.Internal($"Validation error: {ex.Message}", ex));
        }
    }

    /// <summary>
    /// Execute a script
    /// </summary>
    public async Task<Result<ScriptJob>> ExecuteScriptAsync(
        string scriptContent,
        string userId,
        Guid? scriptId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate first
        var validation = ValidateScript(scriptContent);
        if (validation.IsFailure)
        {
            return Result<ScriptJob>.Failure(validation.Error);
        }

        if (!validation.Value!.IsValid)
        {
            var job = new ScriptJob
            {
                ScriptId = scriptId,
                InlineScript = scriptContent,
                CreatedByUserId = userId,
                Status = ScriptStatus.Failed,
                Errors = validation.Value.Errors
            };
            return Result<ScriptJob>.Failure(Error.Validation("Script validation failed"));
        }

        // Parse the script
        var parseResult = _parser.Parse(scriptContent);

        // Create job
        var execJob = new ScriptJob
        {
            ScriptId = scriptId,
            InlineScript = scriptContent,
            CreatedByUserId = userId,
            Status = ScriptStatus.Queued
        };

        // Create cancellation token
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _jobCancellation[execJob.Id] = cts;
        _runningJobs[execJob.Id] = execJob;

        // Start execution
        _ = Task.Run(() => ExecuteJobAsync(execJob, parseResult.Ast, cts.Token), cts.Token);

        return Result<ScriptJob>.Success(execJob);
    }

    private async Task ExecuteJobAsync(ScriptJob job, List<AstNode> ast, CancellationToken cancellationToken)
    {
        job.Status = ScriptStatus.Running;
        job.StartedAt = DateTime.UtcNow;

        try
        {
            await _eventPublisher.PublishAsync(new ScriptExecutionStartedEvent
            {
                JobId = job.Id,
                ScriptId = job.ScriptId
            }, cancellationToken);

            var context = new ScriptExecutionContext { CancellationToken = cancellationToken };
            int totalCommands = ast.Count(n => n.Type != CommandType.Comment && n.Type != CommandType.Unknown);
            int executedCommands = 0;

            for (int i = 0; i < ast.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var node = ast[i];
                job.CurrentLine = node.LineNumber;
                job.CurrentCommand = node.RawText;

                if (node.Type == CommandType.Comment || node.Type == CommandType.Unknown)
                    continue;

                try
                {
                    ExecuteNode(node, context, job);
                    executedCommands++;
                    job.ProgressPercent = totalCommands > 0 ? (executedCommands * 100.0 / totalCommands) : 100;
                }
                catch (Exception ex)
                {
                    job.Errors.Add(new ScriptError
                    {
                        LineNumber = node.LineNumber,
                        Message = ex.Message,
                        Code = "SE100",
                        Severity = ScriptErrorSeverity.Error
                    });
                    throw;
                }
            }

            // Copy context variables and output to job
            foreach (var kvp in context.Variables)
            {
                job.Variables[kvp.Key] = kvp.Value;
            }
            job.OutputLog.AddRange(context.OutputLog);

            job.Status = ScriptStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            job.ExecutionTimeMs = (long)(job.CompletedAt.Value - job.StartedAt!.Value).TotalMilliseconds;
            job.ProgressPercent = 100;

            await _eventPublisher.PublishAsync(new ScriptExecutionCompletedEvent
            {
                JobId = job.Id,
                Success = true,
                ExecutionTimeMs = job.ExecutionTimeMs
            }, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            job.Status = ScriptStatus.Cancelled;
            job.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            job.Status = ScriptStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            job.ExecutionTimeMs = (long)(job.CompletedAt.Value - (job.StartedAt ?? job.CreatedAt)).TotalMilliseconds;
            job.OutputLog.Add($"Error: {ex.Message}");

            await _eventPublisher.PublishAsync(new ScriptExecutionFailedEvent
            {
                JobId = job.Id,
                ErrorMessage = ex.Message,
                LineNumber = job.CurrentLine
            }, CancellationToken.None);
        }
        finally
        {
            _runningJobs.Remove(job.Id);
            _jobCancellation.Remove(job.Id);
        }
    }

    private void ExecuteNode(AstNode node, ScriptExecutionContext context, ScriptJob job)
    {
        switch (node)
        {
            case CreateNode createNode:
                ExecuteCreate(createNode, context, job);
                break;

            case SetNode setNode:
                ExecuteSet(setNode, context, job);
                break;

            case PropagateNode propagateNode:
                ExecutePropagate(propagateNode, context, job);
                break;

            case ReportNode reportNode:
                ExecuteReport(reportNode, context, job);
                break;

            case VariableNode varNode:
                context.Variables[varNode.VariableName] = varNode.Value;
                job.OutputLog.Add($"Set variable {varNode.VariableName} = {varNode.Value}");
                break;

            case { Type: CommandType.Save }:
                var objName = node.Properties.GetValueOrDefault("ObjectName")?.ToString();
                job.OutputLog.Add($"Save {objName}");
                break;
        }
    }

    private void ExecuteCreate(CreateNode node, ScriptExecutionContext context, ScriptJob job)
    {
        ScriptObject obj = node.ObjectType.ToLower() switch
        {
            "spacecraft" => new SpacecraftObject { Name = node.ObjectName },
            "propagator" => new PropagatorObject { Name = node.ObjectName },
            _ => new ScriptObject { Type = node.ObjectType, Name = node.ObjectName }
        };

        context.Objects[node.ObjectName] = obj;
        job.OutputLog.Add($"Created {node.ObjectType} '{node.ObjectName}'");
    }

    private void ExecuteSet(SetNode node, ScriptExecutionContext context, ScriptJob job)
    {
        if (context.Objects.TryGetValue(node.ObjectName, out var obj))
        {
            obj.Properties[node.PropertyName] = node.Value;

            // Handle special properties for known types
            if (obj is SpacecraftObject sc)
            {
                switch (node.PropertyName.ToLower())
                {
                    case "epoch":
                        if (node.Value is string epochStr)
                            sc.Epoch = DateTime.TryParse(epochStr, out var dt) ? dt : DateTime.UtcNow;
                        break;
                    case "x": sc.X = Convert.ToDouble(node.Value); break;
                    case "y": sc.Y = Convert.ToDouble(node.Value); break;
                    case "z": sc.Z = Convert.ToDouble(node.Value); break;
                    case "vx": sc.VX = Convert.ToDouble(node.Value); break;
                    case "vy": sc.VY = Convert.ToDouble(node.Value); break;
                    case "vz": sc.VZ = Convert.ToDouble(node.Value); break;
                    case "fuelmass": sc.FuelMass = Convert.ToDouble(node.Value); break;
                    case "drymass": sc.DryMass = Convert.ToDouble(node.Value); break;
                }
            }
            else if (obj is PropagatorObject prop)
            {
                switch (node.PropertyName.ToLower())
                {
                    case "type": prop.PropagatorType = node.Value?.ToString() ?? "RK4"; break;
                    case "stepsize": prop.StepSize = Convert.ToDouble(node.Value); break;
                    case "minstep": prop.MinStep = Convert.ToDouble(node.Value); break;
                    case "maxstep": prop.MaxStep = Convert.ToDouble(node.Value); break;
                    case "accuracy": prop.Accuracy = Convert.ToDouble(node.Value); break;
                }
            }

            job.OutputLog.Add($"Set {node.ObjectName}.{node.PropertyName} = {node.Value}");
        }
        else
        {
            // Set as variable
            context.Variables[node.ObjectName] = node.Value;
        }
    }

    private void ExecutePropagate(PropagateNode node, ScriptExecutionContext context, ScriptJob job)
    {
        if (!context.Objects.TryGetValue(node.SpacecraftName, out var scObj) || scObj is not SpacecraftObject sc)
        {
            throw new InvalidOperationException($"Spacecraft '{node.SpacecraftName}' not found");
        }

        // Simplified propagation (just update epoch)
        var elapsedDays = node.StopValue ?? 1.0;
        sc.Epoch = sc.Epoch.AddDays(elapsedDays);

        // Simple Kepler motion simulation
        var period = 2 * Math.PI * Math.Sqrt(Math.Pow(7000, 3) / 398600.4418);
        var orbits = elapsedDays * 86400 / period;
        var angle = 2 * Math.PI * (orbits - Math.Floor(orbits));

        var x0 = sc.X;
        var y0 = sc.Y;
        sc.X = x0 * Math.Cos(angle) - y0 * Math.Sin(angle);
        sc.Y = x0 * Math.Sin(angle) + y0 * Math.Cos(angle);

        var vx0 = sc.VX;
        var vy0 = sc.VY;
        sc.VX = vx0 * Math.Cos(angle) - vy0 * Math.Sin(angle);
        sc.VY = vx0 * Math.Sin(angle) + vy0 * Math.Cos(angle);

        job.OutputLog.Add($"Propagated {node.SpacecraftName} for {elapsedDays} days using {node.PropagatorName}");
    }

    private void ExecuteReport(ReportNode node, ScriptExecutionContext context, ScriptJob job)
    {
        var values = new List<string>();

        foreach (var param in node.Parameters)
        {
            var parts = param.Split('.');
            if (parts.Length == 2)
            {
                if (context.Objects.TryGetValue(parts[0], out var obj))
                {
                    if (obj is SpacecraftObject sc)
                    {
                        var value = parts[1].ToLower() switch
                        {
                            "x" => sc.X.ToString("F6"),
                            "y" => sc.Y.ToString("F6"),
                            "z" => sc.Z.ToString("F6"),
                            "vx" => sc.VX.ToString("F9"),
                            "vy" => sc.VY.ToString("F9"),
                            "vz" => sc.VZ.ToString("F9"),
                            "epoch" => sc.Epoch.ToString("O"),
                            "fuelmass" => sc.FuelMass.ToString("F3"),
                            _ => obj.Properties.GetValueOrDefault(parts[1])?.ToString() ?? "N/A"
                        };
                        values.Add($"{param}={value}");
                    }
                    else
                    {
                        values.Add($"{param}={obj.Properties.GetValueOrDefault(parts[1]) ?? "N/A"}");
                    }
                }
            }
            else if (context.Variables.TryGetValue(param, out var varValue))
            {
                values.Add($"{param}={varValue}");
            }
            else
            {
                values.Add($"{param}=undefined");
            }
        }

        job.OutputLog.Add($"Report: {string.Join(", ", values)}");
    }

    /// <summary>
    /// Get job status
    /// </summary>
    public Result<ScriptJob> GetJobStatus(Guid jobId)
    {
        if (_runningJobs.TryGetValue(jobId, out var job))
        {
            return Result<ScriptJob>.Success(job);
        }
        return Result<ScriptJob>.Failure(Error.NotFound("ScriptJob", jobId.ToString()));
    }

    /// <summary>
    /// Cancel a running job
    /// </summary>
    public Result CancelJob(Guid jobId)
    {
        if (_jobCancellation.TryGetValue(jobId, out var cts))
        {
            cts.Cancel();
            return Result.Success();
        }
        return Result.Failure(Error.NotFound("ScriptJob", jobId.ToString()));
    }

    /// <summary>
    /// Save script to library
    /// </summary>
    public async Task<Result<Script>> SaveScriptAsync(
        string name,
        string content,
        string userId,
        string? description = null,
        bool isPublic = false,
        List<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var script = new Script
        {
            Name = name,
            Content = content,
            Description = description,
            CreatedByUserId = userId,
            IsPublic = isPublic,
            Tags = tags ?? new List<string>()
        };

        await _repository.AddAsync(script, cancellationToken);
        return Result<Script>.Success(script);
    }

    /// <summary>
    /// Get scripts from library
    /// </summary>
    public async Task<Result<IReadOnlyList<Script>>> GetScriptsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var scripts = await _repository.GetByUserAsync(userId, cancellationToken);
        return Result<IReadOnlyList<Script>>.Success(scripts);
    }

    /// <summary>
    /// Get a script by ID
    /// </summary>
    public async Task<Result<Script>> GetScriptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var script = await _repository.GetByIdAsync(id, cancellationToken);
        if (script == null)
        {
            return Result<Script>.Failure(Error.NotFound("Script", id.ToString()));
        }
        return Result<Script>.Success(script);
    }
}
