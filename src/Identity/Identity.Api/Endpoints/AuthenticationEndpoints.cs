using Identity.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

/// <summary>
/// Authentication API endpoints.
/// </summary>
public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithDescription("Register a new user account")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithDescription("Login with email and password")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/mfa/verify", VerifyMfaAsync)
            .WithName("VerifyMfa")
            .WithDescription("Verify MFA code to complete login")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithDescription("Refresh access token using refresh token")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithDescription("Logout and revoke refresh token")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(
            request.Email,
            request.Password,
            request.DisplayName,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var result = await authService.LoginAsync(
            request.Email,
            request.Password,
            ipAddress,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> VerifyMfaAsync(
        [FromBody] VerifyMfaRequest request,
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var result = await authService.VerifyMfaAsync(
            request.UserId,
            request.Code,
            ipAddress,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var result = await authService.RefreshTokenAsync(
            request.RefreshToken,
            ipAddress,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await authService.LogoutAsync(userId, request.RefreshToken, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.NoContent();
    }

    private static AuthenticationResponse ToResponse(AuthenticationResult result)
    {
        return new AuthenticationResponse
        {
            UserId = result.UserId,
            Email = result.Email,
            DisplayName = result.DisplayName,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresIn = result.ExpiresIn,
            RequiresMfa = result.RequiresMfa
        };
    }

    private static int GetStatusCode(Shared.Domain.Results.Error error)
    {
        return error.Code switch
        {
            "Validation" => StatusCodes.Status400BadRequest,
            _ when error.Code.EndsWith(".NotFound") => StatusCodes.Status404NotFound,
            "Conflict" => StatusCodes.Status409Conflict,
            "Unauthorized" => StatusCodes.Status401Unauthorized,
            "Forbidden" => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

// Request DTOs
public sealed record RegisterRequest(string Email, string Password, string? DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record VerifyMfaRequest(Guid UserId, string Code);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);

// Response DTOs
public sealed class AuthenticationResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public int ExpiresIn { get; init; }
    public bool RequiresMfa { get; init; }
}
