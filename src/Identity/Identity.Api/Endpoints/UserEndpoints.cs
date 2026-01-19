using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Identity.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

/// <summary>
/// User management API endpoints.
/// </summary>
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .WithDescription("Get current authenticated user's profile")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapGet("/stats", GetUserStatsAsync)
            .WithName("GetUserStats")
            .WithDescription("Get user statistics")
            .RequireAuthorization("Admin")
            .Produces<UserStatsResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithDescription("Get user by ID")
            .RequireAuthorization("Admin")
            .Produces<UserDetailResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", GetUsersAsync)
            .WithName("GetUsers")
            .WithDescription("Get paginated list of users with optional filters")
            .RequireAuthorization("Admin")
            .Produces<PagedUsersResponse>(StatusCodes.Status200OK);

        group.MapPost("/", CreateUserAsync)
            .WithName("CreateUser")
            .WithDescription("Create a new user")
            .RequireAuthorization("Admin")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateUserAsync)
            .WithName("UpdateUser")
            .WithDescription("Update user details")
            .RequireAuthorization("Admin")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithDescription("Delete a user")
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/activate", ActivateUserAsync)
            .WithName("ActivateUser")
            .WithDescription("Activate a user account")
            .RequireAuthorization("Admin")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", DeactivateUserAsync)
            .WithName("DeactivateUser")
            .WithDescription("Deactivate a user account")
            .RequireAuthorization("Admin")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/unlock", UnlockUserAsync)
            .WithName("UnlockUser")
            .WithDescription("Unlock a locked user account")
            .RequireAuthorization("Admin")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/reset-password", ResetUserPasswordAsync)
            .WithName("ResetUserPassword")
            .WithDescription("Reset user password (admin)")
            .RequireAuthorization("Admin")
            .Produces<ResetPasswordResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/roles", UpdateUserRolesAsync)
            .WithName("UpdateUserRoles")
            .WithDescription("Update user roles")
            .RequireAuthorization("Admin")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetCurrentUserAsync(
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> GetUserStatsAsync(
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Users.GetAllAsync(0, int.MaxValue, cancellationToken);
        var totalCount = users.Count;
        var activeCount = users.Count(u => u.IsActive);
        var inactiveCount = users.Count(u => !u.IsActive);
        var lockedCount = users.Count(u => u.IsLockedOut);

        return Results.Ok(new UserStatsResponse
        {
            TotalUsers = totalCount,
            ActiveUsers = activeCount,
            InactiveUsers = inactiveCount,
            LockedUsers = lockedCount
        });
    }

    private static async Task<IResult> GetUserByIdAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToDetailResponse(user));
    }

    private static async Task<IResult> GetUsersAsync(
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? role,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Users.GetAllAsync(0, int.MaxValue, cancellationToken);

        // Apply filters
        IEnumerable<User> filteredUsers = users;

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLowerInvariant();
            filteredUsers = filteredUsers.Where(u =>
                u.Email.ToLowerInvariant().Contains(searchLower) ||
                (u.DisplayName?.ToLowerInvariant().Contains(searchLower) ?? false));
        }

        if (!string.IsNullOrEmpty(status))
        {
            filteredUsers = status.ToLowerInvariant() switch
            {
                "active" => filteredUsers.Where(u => u.IsActive && !u.IsLockedOut),
                "inactive" => filteredUsers.Where(u => !u.IsActive),
                "locked" => filteredUsers.Where(u => u.IsLockedOut),
                _ => filteredUsers
            };
        }

        if (!string.IsNullOrEmpty(role))
        {
            filteredUsers = filteredUsers.Where(u => u.Roles.Any(r => r.Role.Name == role));
        }

        var totalCount = filteredUsers.Count();
        var pagedUsers = filteredUsers.Skip(skip).Take(take > 0 ? take : 10).ToList();

        return Results.Ok(new PagedUsersResponse
        {
            Items = pagedUsers.Select(u => ToResponse(u)).ToList(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        });
    }

    private static async Task<IResult> CreateUserAsync(
        [FromBody] CreateUserRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        [FromServices] IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        if (await unitOfWork.Users.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Results.Problem(
                detail: "A user with this email already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash, request.DisplayName);

        if (request.RoleIds?.Count > 0)
        {
            var roles = await unitOfWork.Roles.GetAllAsync(cancellationToken);
            foreach (var roleId in request.RoleIds)
            {
                var role = roles.FirstOrDefault(r => r.Id == roleId);
                if (role != null)
                {
                    user.AddRole(role);
                }
            }
        }

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/users/{user.Id}", ToResponse(user));
    }

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        user.UpdateDisplayName(request.DisplayName ?? string.Empty);

        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (userIdClaim == id.ToString())
        {
            return Results.Problem(
                detail: "Cannot delete your own account.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        await unitOfWork.Users.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ActivateUserAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        user.Activate();
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> DeactivateUserAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (userIdClaim == id.ToString())
        {
            return Results.Problem(
                detail: "Cannot deactivate your own account.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        user.Deactivate();
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> UnlockUserAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        user.RecordLogin(); // This resets failed attempts and lockout
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> ResetUserPasswordAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        [FromServices] IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        // Generate a temporary password
        var tempPassword = GenerateTemporaryPassword();
        var passwordHash = passwordHasher.Hash(tempPassword);

        user.UpdatePassword(passwordHash);
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(new ResetPasswordResponse
        {
            UserId = user.Id,
            Email = user.Email,
            TemporaryPassword = tempPassword,
            Message = "Password has been reset. Please share this temporary password securely with the user."
        });
    }

    private static async Task<IResult> UpdateUserRolesAsync(
        Guid id,
        [FromBody] UpdateUserRolesRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (userIdClaim == id.ToString())
        {
            return Results.Problem(
                detail: "Cannot modify your own roles.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Results.NotFound();
        }

        var allRoles = await unitOfWork.Roles.GetAllAsync(cancellationToken);

        // Remove current roles
        var currentRoleIds = user.Roles.Select(r => r.RoleId).ToList();
        foreach (var roleId in currentRoleIds)
        {
            user.RemoveRole(roleId);
        }

        // Add new roles
        foreach (var roleId in request.RoleIds)
        {
            var role = allRoles.FirstOrDefault(r => r.Id == roleId);
            if (role != null)
            {
                user.AddRole(role);
            }
        }

        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Refetch to get updated role names
        user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        return Results.Ok(ToResponse(user!));
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            IsMfaEnabled = user.IsMfaEnabled,
            IsLockedOut = user.IsLockedOut,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Role.Name).ToList()
        };
    }

    private static UserDetailResponse ToDetailResponse(User user)
    {
        return new UserDetailResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            IsMfaEnabled = user.IsMfaEnabled,
            IsLockedOut = user.IsLockedOut,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockoutEndAt = user.LockoutEndAt,
            ActiveSessions = user.RefreshTokens.Count(t => !t.IsRevoked && t.ExpiresAt > DateTimeOffset.UtcNow),
            Roles = user.Roles.Select(r => new UserRoleResponse
            {
                Id = r.RoleId,
                Name = r.Role.Name,
                AssignedAt = r.AssignedAt
            }).ToList()
        };
    }
}

// Request DTOs
public sealed record CreateUserRequest(string Email, string Password, string? DisplayName, List<Guid>? RoleIds);
public sealed record UpdateUserRequest(string? DisplayName);
public sealed record UpdateUserRolesRequest(List<Guid> RoleIds);

// Response DTOs
public class UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsActive { get; init; }
    public bool IsMfaEnabled { get; init; }
    public bool IsLockedOut { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public List<string> Roles { get; init; } = new();
}

public sealed class UserDetailResponse : UserResponse
{
    public int FailedLoginAttempts { get; init; }
    public DateTimeOffset? LockoutEndAt { get; init; }
    public int ActiveSessions { get; init; }
    public new List<UserRoleResponse> Roles { get; init; } = new();
}

public sealed class UserRoleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset AssignedAt { get; init; }
}

public sealed class PagedUsersResponse
{
    public List<UserResponse> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}

public sealed class UserStatsResponse
{
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int InactiveUsers { get; init; }
    public int LockedUsers { get; init; }
}

public sealed class ResetPasswordResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string TemporaryPassword { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
