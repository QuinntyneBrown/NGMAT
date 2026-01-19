using System.Security.Cryptography;
using System.Text;
using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

/// <summary>
/// API Key management endpoints.
/// </summary>
public static class ApiKeyEndpoints
{
    public static void MapApiKeyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/apikeys")
            .WithTags("API Keys")
            .RequireAuthorization();

        group.MapGet("/", GetApiKeysAsync)
            .WithName("GetApiKeys")
            .WithDescription("Get current user's API keys")
            .Produces<PagedApiKeysResponse>(StatusCodes.Status200OK);

        group.MapGet("/all", GetAllApiKeysAsync)
            .WithName("GetAllApiKeys")
            .WithDescription("Get all API keys (admin only)")
            .RequireAuthorization("Admin")
            .Produces<PagedApiKeysResponse>(StatusCodes.Status200OK);

        group.MapGet("/stats", GetApiKeyStatsAsync)
            .WithName("GetApiKeyStats")
            .WithDescription("Get API key statistics")
            .RequireAuthorization("Admin")
            .Produces<ApiKeyStatsResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetApiKeyByIdAsync)
            .WithName("GetApiKeyById")
            .WithDescription("Get API key by ID")
            .Produces<ApiKeyResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateApiKeyAsync)
            .WithName("CreateApiKey")
            .WithDescription("Create a new API key")
            .Produces<CreateApiKeyResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateApiKeyAsync)
            .WithName("UpdateApiKey")
            .WithDescription("Update an API key")
            .Produces<ApiKeyResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/revoke", RevokeApiKeyAsync)
            .WithName("RevokeApiKey")
            .WithDescription("Revoke an API key")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteApiKeyAsync)
            .WithName("DeleteApiKey")
            .WithDescription("Delete an API key")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetApiKeysAsync(
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var apiKeys = await unitOfWork.ApiKeys.GetByUserIdAsync(userId, cancellationToken);

        return Results.Ok(new PagedApiKeysResponse
        {
            Items = apiKeys.Select(ToResponse).ToList(),
            TotalCount = apiKeys.Count,
            Skip = 0,
            Take = apiKeys.Count
        });
    }

    private static async Task<IResult> GetAllApiKeysAsync(
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var apiKeys = await unitOfWork.ApiKeys.GetAllAsync(skip, take, cancellationToken);
        var totalCount = await unitOfWork.ApiKeys.GetCountAsync(cancellationToken);

        return Results.Ok(new PagedApiKeysResponse
        {
            Items = apiKeys.Select(ToResponse).ToList(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        });
    }

    private static async Task<IResult> GetApiKeyStatsAsync(
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var totalCount = await unitOfWork.ApiKeys.GetCountAsync(cancellationToken);
        var activeCount = await unitOfWork.ApiKeys.GetActiveCountAsync(cancellationToken);
        var expiredOrRevokedCount = totalCount - activeCount;

        return Results.Ok(new ApiKeyStatsResponse
        {
            TotalKeys = totalCount,
            ActiveKeys = activeCount,
            ExpiredOrRevokedKeys = expiredOrRevokedCount
        });
    }

    private static async Task<IResult> GetApiKeyByIdAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var apiKey = await unitOfWork.ApiKeys.GetByIdAsync(id, cancellationToken);
        if (apiKey == null)
        {
            return Results.NotFound();
        }

        // Non-admin users can only see their own keys
        var isAdmin = httpContext.User.IsInRole("Admin");
        if (!isAdmin && apiKey.UserId != userId)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(apiKey));
    }

    private static async Task<IResult> CreateApiKeyAsync(
        [FromBody] CreateApiKeyRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        [FromServices] IPasswordHasher passwordHasher,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Generate a secure random key
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var keyString = $"ngmat_{request.Prefix}_{Convert.ToBase64String(keyBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
        var keyHash = HashKey(keyString);
        var keyPrefix = keyString[..Math.Min(24, keyString.Length)] + "...";

        TimeSpan? lifetime = request.ExpiresInDays.HasValue
            ? TimeSpan.FromDays(request.ExpiresInDays.Value)
            : null;

        var apiKey = ApiKey.Create(
            userId,
            request.Name,
            keyHash,
            keyPrefix,
            request.Scopes,
            lifetime);

        await unitOfWork.ApiKeys.AddAsync(apiKey, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/apikeys/{apiKey.Id}", new CreateApiKeyResponse
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            Key = keyString, // Only returned once during creation
            KeyPrefix = apiKey.KeyPrefix,
            Scopes = apiKey.Scopes.ToList(),
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt
        });
    }

    private static async Task<IResult> UpdateApiKeyAsync(
        Guid id,
        [FromBody] UpdateApiKeyRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var apiKey = await unitOfWork.ApiKeys.GetByIdAsync(id, cancellationToken);
        if (apiKey == null)
        {
            return Results.NotFound();
        }

        // Non-admin users can only update their own keys
        var isAdmin = httpContext.User.IsInRole("Admin");
        if (!isAdmin && apiKey.UserId != userId)
        {
            return Results.NotFound();
        }

        apiKey.UpdateName(request.Name);
        if (request.Scopes != null)
        {
            apiKey.UpdateScopes(request.Scopes);
        }

        await unitOfWork.ApiKeys.UpdateAsync(apiKey, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(apiKey));
    }

    private static async Task<IResult> RevokeApiKeyAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var apiKey = await unitOfWork.ApiKeys.GetByIdAsync(id, cancellationToken);
        if (apiKey == null)
        {
            return Results.NotFound();
        }

        // Non-admin users can only revoke their own keys
        var isAdmin = httpContext.User.IsInRole("Admin");
        if (!isAdmin && apiKey.UserId != userId)
        {
            return Results.NotFound();
        }

        apiKey.Deactivate();
        await unitOfWork.ApiKeys.UpdateAsync(apiKey, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteApiKeyAsync(
        Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var apiKey = await unitOfWork.ApiKeys.GetByIdAsync(id, cancellationToken);
        if (apiKey == null)
        {
            return Results.NotFound();
        }

        // Non-admin users can only delete their own keys
        var isAdmin = httpContext.User.IsInRole("Admin");
        if (!isAdmin && apiKey.UserId != userId)
        {
            return Results.NotFound();
        }

        await unitOfWork.ApiKeys.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static string HashKey(string key)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(hashBytes);
    }

    private static ApiKeyResponse ToResponse(ApiKey apiKey)
    {
        var status = "Active";
        if (!apiKey.IsActive)
        {
            status = "Revoked";
        }
        else if (apiKey.IsExpired)
        {
            status = "Expired";
        }

        return new ApiKeyResponse
        {
            Id = apiKey.Id,
            UserId = apiKey.UserId,
            UserEmail = apiKey.User?.Email,
            Name = apiKey.Name,
            KeyPrefix = apiKey.KeyPrefix,
            Scopes = apiKey.Scopes.ToList(),
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt,
            LastUsedAt = apiKey.LastUsedAt,
            IsActive = apiKey.IsActive,
            Status = status
        };
    }
}

// DTOs
public sealed record CreateApiKeyRequest(string Name, string Prefix, List<string>? Scopes, int? ExpiresInDays);
public sealed record UpdateApiKeyRequest(string Name, List<string>? Scopes);

public class ApiKeyResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? UserEmail { get; init; }
    public string Name { get; init; } = string.Empty;
    public string KeyPrefix { get; init; } = string.Empty;
    public List<string> Scopes { get; init; } = new();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset? LastUsedAt { get; init; }
    public bool IsActive { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class CreateApiKeyResponse : ApiKeyResponse
{
    public string Key { get; init; } = string.Empty;
}

public sealed class PagedApiKeysResponse
{
    public List<ApiKeyResponse> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}

public sealed class ApiKeyStatsResponse
{
    public int TotalKeys { get; init; }
    public int ActiveKeys { get; init; }
    public int ExpiredOrRevokedKeys { get; init; }
}
