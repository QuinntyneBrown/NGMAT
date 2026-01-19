using System.Security.Claims;
using MissionManagement.Core.Entities;
using MissionManagement.Core.Models;
using MissionManagement.Core.Services;

namespace MissionManagement.Api.Endpoints;

public static class MissionEndpoints
{
    public static void MapMissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/missions")
            .WithTags("Missions")
            .RequireAuthorization();

        group.MapPost("/", CreateMission).WithName("CreateMission");
        group.MapGet("/", ListMissions).WithName("ListMissions");
        group.MapGet("/{id:guid}", GetMission).WithName("GetMission");
        group.MapPut("/{id:guid}", UpdateMission).WithName("UpdateMission");
        group.MapDelete("/{id:guid}", DeleteMission).WithName("DeleteMission");
        group.MapPatch("/{id:guid}/status", ChangeStatus).WithName("ChangeMissionStatus");
        group.MapPost("/{id:guid}/share", ShareMission).WithName("ShareMission");
        group.MapDelete("/{id:guid}/share/{userId:guid}", RevokeMissionShare).WithName("RevokeMissionShare");
        group.MapPost("/{id:guid}/clone", CloneMission).WithName("CloneMission");

        // Export/Import endpoints
        group.MapGet("/{id:guid}/export", ExportMission).WithName("ExportMission");
        group.MapPost("/export", ExportMissions).WithName("ExportMissions");
        group.MapPost("/import", ImportMission).WithName("ImportMission");
        group.MapPost("/import/batch", ImportMissions).WithName("ImportMissions");
    }

    private static async Task<IResult> CreateMission(
        CreateMissionRequest request,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.CreateMissionAsync(
            request.Name,
            request.Type,
            request.StartEpoch,
            userId.Value,
            request.Description,
            request.EndEpoch);

        return result.IsSuccess
            ? Results.Created($"/v1/missions/{result.Value.Id}", ToResponse(result.Value))
            : MapError(result.Error);
    }

    private static async Task<IResult> GetMission(
        Guid id,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.GetMissionAsync(id, userId.Value);

        return result.IsSuccess
            ? Results.Ok(ToResponse(result.Value))
            : MapError(result.Error);
    }

    private static async Task<IResult> ListMissions(
        MissionService service,
        HttpContext context,
        int page = 1,
        int size = 20,
        MissionStatus? status = null,
        string? search = null)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.ListMissionsAsync(userId.Value, page, size, status, search);

        return result.IsSuccess
            ? Results.Ok(new PagedMissionResponse
            {
                Items = result.Value.Items.Select(ToResponse).ToList(),
                Page = result.Value.Page,
                PageSize = result.Value.PageSize,
                TotalCount = result.Value.TotalCount,
                TotalPages = result.Value.TotalPages
            })
            : MapError(result.Error);
    }

    private static async Task<IResult> UpdateMission(
        Guid id,
        UpdateMissionRequest request,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.UpdateMissionAsync(
            id,
            userId.Value,
            request.Name,
            request.Description,
            request.StartEpoch,
            request.EndEpoch);

        return result.IsSuccess
            ? Results.Ok(ToResponse(result.Value))
            : MapError(result.Error);
    }

    private static async Task<IResult> DeleteMission(
        Guid id,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.DeleteMissionAsync(id, userId.Value);

        return result.IsSuccess
            ? Results.NoContent()
            : MapError(result.Error);
    }

    private static async Task<IResult> ChangeStatus(
        Guid id,
        ChangeStatusRequest request,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.ChangeStatusAsync(id, userId.Value, request.Status);

        return result.IsSuccess
            ? Results.Ok(ToResponse(result.Value))
            : MapError(result.Error);
    }

    private static async Task<IResult> ShareMission(
        Guid id,
        ShareMissionRequest request,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.ShareMissionAsync(
            id,
            userId.Value,
            request.UserId,
            request.Permission);

        return result.IsSuccess
            ? Results.Ok(new { Message = "Mission shared successfully" })
            : MapError(result.Error);
    }

    private static async Task<IResult> RevokeMissionShare(
        Guid id,
        Guid userId,
        MissionService service,
        HttpContext context)
    {
        var currentUserId = GetUserId(context);
        if (currentUserId == null)
            return Results.Unauthorized();

        var result = await service.RevokeMissionShareAsync(id, currentUserId.Value, userId);

        return result.IsSuccess
            ? Results.Ok(new { Message = "Share revoked successfully" })
            : MapError(result.Error);
    }

    private static async Task<IResult> CloneMission(
        Guid id,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.CloneMissionAsync(id, userId.Value);

        return result.IsSuccess
            ? Results.Created($"/v1/missions/{result.Value.Id}", ToResponse(result.Value))
            : MapError(result.Error);
    }

    private static async Task<IResult> ExportMission(
        Guid id,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.ExportMissionAsync(id, userId.Value);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> ExportMissions(
        ExportMissionsRequest? request,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.ExportMissionsAsync(
            userId.Value,
            request?.MissionIds,
            request?.Status);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> ImportMission(
        ImportMissionRequest request,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.ImportMissionAsync(
            userId.Value,
            request.Mission,
            request.OverwriteExisting);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> ImportMissions(
        ImportMissionsRequest request,
        MissionService service,
        HttpContext context)
    {
        var userId = GetUserId(context);
        if (userId == null)
            return Results.Unauthorized();

        var result = await service.ImportMissionsAsync(
            userId.Value,
            request.Missions,
            request.OverwriteExisting,
            request.StopOnError);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : MapError(result.Error);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static MissionResponse ToResponse(Mission mission) => new()
    {
        Id = mission.Id,
        Name = mission.Name,
        Description = mission.Description,
        Type = mission.Type,
        Status = mission.Status,
        StartEpoch = mission.StartEpoch,
        EndEpoch = mission.EndEpoch,
        OwnerId = mission.OwnerId,
        CreatedAt = mission.CreatedAt,
        UpdatedAt = mission.UpdatedAt
    };

    private static IResult MapError(Shared.Domain.Results.Error error)
    {
        return error.Code switch
        {
            "NotFound" => Results.NotFound(new { Error = error.Message }),
            "Forbidden" => Results.Forbid(),
            "Conflict" => Results.Conflict(new { Error = error.Message }),
            "Validation" => Results.BadRequest(new { Error = error.Message }),
            _ => Results.BadRequest(new { Error = error.Message })
        };
    }
}

public sealed class CreateMissionRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required MissionType Type { get; init; }
    public required DateTime StartEpoch { get; init; }
    public DateTime? EndEpoch { get; init; }
}

public sealed class UpdateMissionRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public DateTime? StartEpoch { get; init; }
    public DateTime? EndEpoch { get; init; }
}

public sealed class ChangeStatusRequest
{
    public required MissionStatus Status { get; init; }
}

public sealed class ShareMissionRequest
{
    public required Guid UserId { get; init; }
    public required MissionPermission Permission { get; init; }
}

public sealed class MissionResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public MissionType Type { get; init; }
    public MissionStatus Status { get; init; }
    public DateTime StartEpoch { get; init; }
    public DateTime? EndEpoch { get; init; }
    public Guid OwnerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class PagedMissionResponse
{
    public List<MissionResponse> Items { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public sealed class ExportMissionsRequest
{
    public List<Guid>? MissionIds { get; init; }
    public MissionStatus? Status { get; init; }
}

public sealed class ImportMissionRequest
{
    public required MissionData Mission { get; init; }
    public bool OverwriteExisting { get; init; } = false;
}

public sealed class ImportMissionsRequest
{
    public required List<MissionData> Missions { get; init; }
    public bool OverwriteExisting { get; init; } = false;
    public bool StopOnError { get; init; } = true;
}
