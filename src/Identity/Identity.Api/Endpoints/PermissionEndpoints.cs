using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

/// <summary>
/// Permission management API endpoints.
/// </summary>
public static class PermissionEndpoints
{
    public static void MapPermissionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/permissions")
            .WithTags("Permissions")
            .RequireAuthorization("Admin");

        group.MapGet("/", GetPermissionsAsync)
            .WithName("GetPermissions")
            .WithDescription("Get all permissions")
            .Produces<List<PermissionResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetPermissionByIdAsync)
            .WithName("GetPermissionById")
            .WithDescription("Get permission by ID")
            .Produces<PermissionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/resources", GetResourcesAsync)
            .WithName("GetResources")
            .WithDescription("Get all permission resources")
            .Produces<List<string>>(StatusCodes.Status200OK);

        group.MapGet("/resources/{resource}", GetPermissionsByResourceAsync)
            .WithName("GetPermissionsByResource")
            .WithDescription("Get permissions by resource")
            .Produces<List<PermissionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetPermissionsAsync(
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var permissions = await unitOfWork.Permissions.GetAllAsync(cancellationToken);

        var response = permissions.Select(p => ToResponse(p)).ToList();
        return Results.Ok(response);
    }

    private static async Task<IResult> GetPermissionByIdAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var permission = await unitOfWork.Permissions.GetByIdAsync(id, cancellationToken);
        if (permission == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(permission));
    }

    private static async Task<IResult> GetResourcesAsync(
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var permissions = await unitOfWork.Permissions.GetAllAsync(cancellationToken);
        var resources = permissions.Select(p => p.Resource).Distinct().OrderBy(r => r).ToList();
        return Results.Ok(resources);
    }

    private static async Task<IResult> GetPermissionsByResourceAsync(
        string resource,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var permissions = await unitOfWork.Permissions.GetByResourceAsync(resource, cancellationToken);
        var response = permissions.Select(p => ToResponse(p)).ToList();
        return Results.Ok(response);
    }

    private static PermissionResponse ToResponse(Identity.Core.Entities.Permission permission)
    {
        return new PermissionResponse
        {
            Id = permission.Id,
            Name = permission.Name,
            Resource = permission.Resource,
            Action = permission.Action,
            Description = permission.Description,
            Roles = permission.Roles.Select(rp => new RoleSummaryResponse
            {
                Id = rp.Role.Id,
                Name = rp.Role.Name,
                IsSystem = rp.Role.IsSystem
            }).ToList()
        };
    }
}

// DTOs
public sealed class PermissionResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Resource { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<RoleSummaryResponse> Roles { get; init; } = new();
}

public sealed class RoleSummaryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsSystem { get; init; }
}
