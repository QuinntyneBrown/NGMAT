using System.Security.Claims;
using ScriptExecution.Core.Interfaces;
using ScriptExecution.Core.Models;
using ScriptExecution.Core.Services;

namespace ScriptExecution.Api.Endpoints;

public static class ScriptEndpoints
{
    public static void MapScriptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scripts")
            .WithTags("Script Execution");

        // Parse script
        group.MapPost("/parse", ParseScript)
            .WithName("ParseScript")
            .WithOpenApi()
            .RequireAuthorization();

        // Validate script
        group.MapPost("/validate", ValidateScript)
            .WithName("ValidateScript")
            .WithOpenApi()
            .RequireAuthorization();

        // Execute script
        group.MapPost("/execute", ExecuteScript)
            .WithName("ExecuteScript")
            .WithOpenApi()
            .RequireAuthorization();

        // Execute saved script by ID
        group.MapPost("/{scriptId:guid}/execute", ExecuteSavedScript)
            .WithName("ExecuteSavedScript")
            .WithOpenApi()
            .RequireAuthorization();

        // Save script to library
        group.MapPost("/library", SaveScript)
            .WithName("SaveScript")
            .WithOpenApi()
            .RequireAuthorization();

        // Get user's scripts
        group.MapGet("/library/mine", GetMyScripts)
            .WithName("GetMyScripts")
            .WithOpenApi()
            .RequireAuthorization();

        // Get public scripts
        group.MapGet("/library/public", GetPublicScripts)
            .WithName("GetPublicScripts")
            .WithOpenApi()
            .RequireAuthorization();

        // Get script by ID
        group.MapGet("/library/{id:guid}", GetScriptById)
            .WithName("GetScriptById")
            .WithOpenApi()
            .RequireAuthorization();

        // Update script
        group.MapPut("/library/{id:guid}", UpdateScript)
            .WithName("UpdateScript")
            .WithOpenApi()
            .RequireAuthorization();

        // Delete script
        group.MapDelete("/library/{id:guid}", DeleteScript)
            .WithName("DeleteScript")
            .WithOpenApi()
            .RequireAuthorization();

        // Get job status
        group.MapGet("/jobs/{jobId:guid}", GetJobStatus)
            .WithName("GetScriptJobStatus")
            .WithOpenApi()
            .RequireAuthorization();

        // Cancel job
        group.MapPost("/jobs/{jobId:guid}/cancel", CancelJob)
            .WithName("CancelScriptJob")
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static IResult ParseScript(
        ParseScriptRequest request,
        ScriptParser parser)
    {
        if (string.IsNullOrWhiteSpace(request.Script))
        {
            return Results.BadRequest("Script content is required");
        }

        var result = parser.Parse(request.Script);
        return Results.Ok(new ParseScriptResponse
        {
            IsValid = result.IsValid,
            LineCount = result.LineCount,
            CommandCount = result.CommandCount,
            Errors = result.Errors.Select(e => new ScriptErrorDto
            {
                LineNumber = e.LineNumber,
                Column = e.Column,
                Message = e.Message,
                Code = e.Code
            }).ToList(),
            Warnings = result.Warnings.Select(w => new ScriptWarningDto
            {
                LineNumber = w.LineNumber,
                Message = w.Message,
                Suggestion = w.Suggestion
            }).ToList(),
            Ast = result.Ast.Select(MapAstNode).ToList()
        });
    }

    private static IResult ValidateScript(
        ValidateScriptRequest request,
        ScriptExecutionService service)
    {
        if (string.IsNullOrWhiteSpace(request.Script))
        {
            return Results.BadRequest("Script content is required");
        }

        var result = service.ValidateScript(request.Script);
        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }

        return Results.Ok(new ValidationResponse
        {
            IsValid = result.Value!.IsValid,
            Errors = result.Value.Errors.Select(e => new ScriptErrorDto
            {
                LineNumber = e.LineNumber,
                Column = e.Column,
                Message = e.Message,
                Code = e.Code
            }).ToList(),
            Warnings = result.Value.Warnings.Select(w => new ScriptWarningDto
            {
                LineNumber = w.LineNumber,
                Message = w.Message,
                Suggestion = w.Suggestion
            }).ToList()
        });
    }

    private static async Task<IResult> ExecuteScript(
        ExecuteScriptRequest request,
        ScriptExecutionService service,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Script))
        {
            return Results.BadRequest("Script content is required");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var result = await service.ExecuteScriptAsync(request.Script, userId, null, cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }

        return Results.Ok(new ExecuteScriptResponse
        {
            JobId = result.Value!.Id,
            Status = result.Value.Status.ToString()
        });
    }

    private static async Task<IResult> ExecuteSavedScript(
        Guid scriptId,
        ScriptExecutionService service,
        IScriptRepository repository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var script = await repository.GetByIdAsync(scriptId, cancellationToken);
        if (script == null)
        {
            return Results.NotFound($"Script {scriptId} not found");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var result = await service.ExecuteScriptAsync(script.Content, userId, scriptId, cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }

        return Results.Ok(new ExecuteScriptResponse
        {
            JobId = result.Value!.Id,
            Status = result.Value.Status.ToString()
        });
    }

    private static async Task<IResult> SaveScript(
        SaveScriptRequest request,
        IScriptRepository repository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Content))
        {
            return Results.BadRequest("Name and content are required");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

        var script = new Script
        {
            Name = request.Name,
            Description = request.Description,
            Content = request.Content,
            CreatedByUserId = userId,
            IsPublic = request.IsPublic,
            Tags = request.Tags ?? new List<string>()
        };

        await repository.AddAsync(script, cancellationToken);

        return Results.Created($"/api/scripts/library/{script.Id}", new ScriptDto
        {
            Id = script.Id,
            Name = script.Name,
            Description = script.Description,
            Content = script.Content,
            CreatedByUserId = script.CreatedByUserId,
            CreatedAt = script.CreatedAt,
            IsPublic = script.IsPublic,
            Tags = script.Tags,
            Version = script.Version
        });
    }

    private static async Task<IResult> GetMyScripts(
        IScriptRepository repository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var scripts = await repository.GetByUserAsync(userId, cancellationToken);

        return Results.Ok(scripts.Select(s => new ScriptSummaryDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            IsPublic = s.IsPublic,
            Tags = s.Tags,
            Version = s.Version
        }));
    }

    private static async Task<IResult> GetPublicScripts(
        IScriptRepository repository,
        CancellationToken cancellationToken)
    {
        var scripts = await repository.GetPublicAsync(cancellationToken);

        return Results.Ok(scripts.Select(s => new ScriptSummaryDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            IsPublic = s.IsPublic,
            Tags = s.Tags,
            Version = s.Version,
            CreatedByUserId = s.CreatedByUserId
        }));
    }

    private static async Task<IResult> GetScriptById(
        Guid id,
        IScriptRepository repository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var script = await repository.GetByIdAsync(id, cancellationToken);
        if (script == null)
        {
            return Results.NotFound($"Script {id} not found");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!script.IsPublic && script.CreatedByUserId != userId)
        {
            return Results.Forbid();
        }

        return Results.Ok(new ScriptDto
        {
            Id = script.Id,
            Name = script.Name,
            Description = script.Description,
            Content = script.Content,
            CreatedByUserId = script.CreatedByUserId,
            CreatedAt = script.CreatedAt,
            UpdatedAt = script.UpdatedAt,
            IsPublic = script.IsPublic,
            Tags = script.Tags,
            Version = script.Version
        });
    }

    private static async Task<IResult> UpdateScript(
        Guid id,
        UpdateScriptRequest request,
        IScriptRepository repository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return Results.NotFound($"Script {id} not found");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (existing.CreatedByUserId != userId)
        {
            return Results.Forbid();
        }

        var updated = new Script
        {
            Id = existing.Id,
            Name = request.Name ?? existing.Name,
            Description = request.Description ?? existing.Description,
            Content = request.Content ?? existing.Content,
            CreatedByUserId = existing.CreatedByUserId,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsPublic = request.IsPublic ?? existing.IsPublic,
            Tags = request.Tags ?? existing.Tags,
            Version = existing.Version + 1
        };

        await repository.UpdateAsync(updated, cancellationToken);

        return Results.Ok(new ScriptDto
        {
            Id = updated.Id,
            Name = updated.Name,
            Description = updated.Description,
            Content = updated.Content,
            CreatedByUserId = updated.CreatedByUserId,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt,
            IsPublic = updated.IsPublic,
            Tags = updated.Tags,
            Version = updated.Version
        });
    }

    private static async Task<IResult> DeleteScript(
        Guid id,
        IScriptRepository repository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return Results.NotFound($"Script {id} not found");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (existing.CreatedByUserId != userId)
        {
            return Results.Forbid();
        }

        await repository.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static IResult GetJobStatus(
        Guid jobId,
        ScriptExecutionService service)
    {
        var result = service.GetJobStatus(jobId);
        if (result.IsFailure)
        {
            return Results.NotFound($"Job {jobId} not found");
        }

        var job = result.Value!;
        return Results.Ok(new JobStatusResponse
        {
            JobId = job.Id,
            Status = job.Status.ToString(),
            ProgressPercent = job.ProgressPercent,
            CurrentLine = job.CurrentLine,
            CurrentCommand = job.CurrentCommand,
            OutputLog = job.OutputLog,
            Errors = job.Errors.Select(e => new ScriptErrorDto
            {
                LineNumber = e.LineNumber,
                Column = e.Column,
                Message = e.Message,
                Code = e.Code
            }).ToList(),
            Variables = job.Variables,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ExecutionTimeMs = job.ExecutionTimeMs
        });
    }

    private static IResult CancelJob(
        Guid jobId,
        ScriptExecutionService service)
    {
        var result = service.CancelJob(jobId);
        if (result.IsFailure)
        {
            return Results.NotFound($"Job {jobId} not found or cannot be cancelled");
        }

        return Results.Ok(new { Message = "Job cancellation requested" });
    }

    private static AstNodeDto MapAstNode(AstNode node)
    {
        return new AstNodeDto
        {
            Type = node.Type.ToString(),
            LineNumber = node.LineNumber,
            RawText = node.RawText,
            Properties = node.Properties.ToDictionary(p => p.Key, p => p.Value?.ToString() ?? "")
        };
    }
}

// Request/Response DTOs
public record ParseScriptRequest(string Script);

public record ValidateScriptRequest(string Script);

public record ExecuteScriptRequest(string Script);

public record SaveScriptRequest(
    string Name,
    string? Description,
    string Content,
    bool IsPublic = false,
    List<string>? Tags = null);

public record UpdateScriptRequest(
    string? Name = null,
    string? Description = null,
    string? Content = null,
    bool? IsPublic = null,
    List<string>? Tags = null);

public record ParseScriptResponse
{
    public bool IsValid { get; init; }
    public int LineCount { get; init; }
    public int CommandCount { get; init; }
    public List<ScriptErrorDto> Errors { get; init; } = new();
    public List<ScriptWarningDto> Warnings { get; init; } = new();
    public List<AstNodeDto> Ast { get; init; } = new();
}

public record ValidationResponse
{
    public bool IsValid { get; init; }
    public List<ScriptErrorDto> Errors { get; init; } = new();
    public List<ScriptWarningDto> Warnings { get; init; } = new();
}

public record ExecuteScriptResponse
{
    public Guid JobId { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record JobStatusResponse
{
    public Guid JobId { get; init; }
    public string Status { get; init; } = string.Empty;
    public double ProgressPercent { get; init; }
    public int CurrentLine { get; init; }
    public string? CurrentCommand { get; init; }
    public List<string> OutputLog { get; init; } = new();
    public List<ScriptErrorDto> Errors { get; init; } = new();
    public Dictionary<string, object> Variables { get; init; } = new();
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public long ExecutionTimeMs { get; init; }
}

public record ScriptDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Content { get; init; } = string.Empty;
    public string CreatedByUserId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsPublic { get; init; }
    public List<string> Tags { get; init; } = new();
    public int Version { get; init; }
}

public record ScriptSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsPublic { get; init; }
    public List<string> Tags { get; init; } = new();
    public int Version { get; init; }
    public string? CreatedByUserId { get; init; }
}

public record ScriptErrorDto
{
    public int LineNumber { get; init; }
    public int Column { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Code { get; init; }
}

public record ScriptWarningDto
{
    public int LineNumber { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Suggestion { get; init; }
}

public record AstNodeDto
{
    public string Type { get; init; } = string.Empty;
    public int LineNumber { get; init; }
    public string RawText { get; init; } = string.Empty;
    public Dictionary<string, string> Properties { get; init; } = new();
}
