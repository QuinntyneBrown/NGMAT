using Identity.Core.Interfaces;
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

        group.MapGet("/{id:guid}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithDescription("Get user by ID")
            .RequireAuthorization("Admin")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", GetUsersAsync)
            .WithName("GetUsers")
            .WithDescription("Get paginated list of users")
            .RequireAuthorization("Admin")
            .Produces<PagedUsersResponse>(StatusCodes.Status200OK);
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

        return Results.Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            IsMfaEnabled = user.IsMfaEnabled,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Role.Name).ToList()
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

        return Results.Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            IsMfaEnabled = user.IsMfaEnabled,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Role.Name).ToList()
        });
    }

    private static async Task<IResult> GetUsersAsync(
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Users.GetAllAsync(skip, take, cancellationToken);
        var totalCount = await unitOfWork.Users.GetCountAsync(cancellationToken);

        return Results.Ok(new PagedUsersResponse
        {
            Items = users.Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                IsEmailVerified = u.IsEmailVerified,
                IsActive = u.IsActive,
                IsMfaEnabled = u.IsMfaEnabled,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                Roles = u.Roles.Select(r => r.Role.Name).ToList()
            }).ToList(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        });
    }
}

public sealed class UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsActive { get; init; }
    public bool IsMfaEnabled { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public List<string> Roles { get; init; } = new();
}

public sealed class PagedUsersResponse
{
    public List<UserResponse> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}
