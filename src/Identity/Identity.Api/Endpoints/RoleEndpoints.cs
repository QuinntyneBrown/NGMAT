using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

/// <summary>
/// Role management API endpoints.
/// </summary>
public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/roles")
            .WithTags("Roles")
            .RequireAuthorization("Admin");

        group.MapGet("/", GetRolesAsync)
            .WithName("GetRoles")
            .WithDescription("Get all roles")
            .Produces<List<RoleResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetRoleByIdAsync)
            .WithName("GetRoleById")
            .WithDescription("Get role by ID")
            .Produces<RoleDetailResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateRoleAsync)
            .WithName("CreateRole")
            .WithDescription("Create a new role")
            .Produces<RoleResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateRoleAsync)
            .WithName("UpdateRole")
            .WithDescription("Update an existing role")
            .Produces<RoleResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteRoleAsync)
            .WithName("DeleteRole")
            .WithDescription("Delete a role")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/permissions", UpdateRolePermissionsAsync)
            .WithName("UpdateRolePermissions")
            .WithDescription("Update role permissions")
            .Produces<RoleDetailResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/users", GetRoleUsersAsync)
            .WithName("GetRoleUsers")
            .WithDescription("Get users with this role")
            .Produces<List<UserSummaryResponse>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetRolesAsync(
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var roles = await unitOfWork.Roles.GetAllAsync(cancellationToken);

        var response = roles.Select(r => new RoleResponse
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsSystem = r.IsSystem,
            CreatedAt = r.CreatedAt,
            UserCount = r.Users.Count,
            PermissionCount = r.Permissions.Count
        }).ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> GetRoleByIdAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (role == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToDetailResponse(role));
    }

    private static async Task<IResult> CreateRoleAsync(
        [FromBody] CreateRoleRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var existingRole = await unitOfWork.Roles.GetByNameAsync(request.Name, cancellationToken);
        if (existingRole != null)
        {
            return Results.Problem(
                detail: "A role with this name already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var role = Role.Create(request.Name, request.Description, isSystem: false);

        if (request.PermissionIds?.Count > 0)
        {
            var permissions = await unitOfWork.Permissions.GetAllAsync(cancellationToken);
            foreach (var permissionId in request.PermissionIds)
            {
                var permission = permissions.FirstOrDefault(p => p.Id == permissionId);
                if (permission != null)
                {
                    role.AddPermission(permission);
                }
            }
        }

        await unitOfWork.Roles.AddAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/roles/{role.Id}", new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            CreatedAt = role.CreatedAt,
            UserCount = 0,
            PermissionCount = role.Permissions.Count
        });
    }

    private static async Task<IResult> UpdateRoleAsync(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (role == null)
        {
            return Results.NotFound();
        }

        if (role.IsSystem)
        {
            return Results.Problem(
                detail: "System roles cannot be modified.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        role.Update(request.Name, request.Description);
        await unitOfWork.Roles.UpdateAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            CreatedAt = role.CreatedAt,
            UserCount = role.Users.Count,
            PermissionCount = role.Permissions.Count
        });
    }

    private static async Task<IResult> DeleteRoleAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (role == null)
        {
            return Results.NotFound();
        }

        if (role.IsSystem)
        {
            return Results.Problem(
                detail: "System roles cannot be deleted.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (role.Users.Count > 0)
        {
            return Results.Problem(
                detail: "Cannot delete a role that has users assigned. Remove users first.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        await unitOfWork.Roles.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateRolePermissionsAsync(
        Guid id,
        [FromBody] UpdateRolePermissionsRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (role == null)
        {
            return Results.NotFound();
        }

        if (role.IsSystem)
        {
            return Results.Problem(
                detail: "System role permissions cannot be modified.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var allPermissions = await unitOfWork.Permissions.GetAllAsync(cancellationToken);

        // Remove current permissions
        var currentPermissionIds = role.Permissions.Select(p => p.PermissionId).ToList();
        foreach (var permissionId in currentPermissionIds)
        {
            role.RemovePermission(permissionId);
        }

        // Add new permissions
        foreach (var permissionId in request.PermissionIds)
        {
            var permission = allPermissions.FirstOrDefault(p => p.Id == permissionId);
            if (permission != null)
            {
                role.AddPermission(permission);
            }
        }

        await unitOfWork.Roles.UpdateAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Refetch to get updated data
        role = await unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        return Results.Ok(ToDetailResponse(role!));
    }

    private static async Task<IResult> GetRoleUsersAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (role == null)
        {
            return Results.NotFound();
        }

        var users = role.Users.Select(ur => new UserSummaryResponse
        {
            Id = ur.UserId,
            Email = ur.User.Email,
            DisplayName = ur.User.DisplayName,
            IsActive = ur.User.IsActive
        }).ToList();

        return Results.Ok(users);
    }

    private static RoleDetailResponse ToDetailResponse(Role role)
    {
        return new RoleDetailResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            CreatedAt = role.CreatedAt,
            UserCount = role.Users.Count,
            PermissionCount = role.Permissions.Count,
            Permissions = role.Permissions.Select(rp => new PermissionSummaryResponse
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Resource = rp.Permission.Resource,
                Action = rp.Permission.Action,
                Description = rp.Permission.Description
            }).ToList()
        };
    }
}

// DTOs
public sealed record CreateRoleRequest(string Name, string? Description, List<Guid>? PermissionIds);
public sealed record UpdateRoleRequest(string Name, string? Description);
public sealed record UpdateRolePermissionsRequest(List<Guid> PermissionIds);

public class RoleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsSystem { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public int UserCount { get; init; }
    public int PermissionCount { get; init; }
}

public sealed class RoleDetailResponse : RoleResponse
{
    public List<PermissionSummaryResponse> Permissions { get; init; } = new();
}

public sealed class PermissionSummaryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Resource { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class UserSummaryResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public bool IsActive { get; init; }
}
