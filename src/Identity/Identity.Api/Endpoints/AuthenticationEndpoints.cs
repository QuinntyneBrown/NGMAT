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

        group.MapPost("/email/request-verification", RequestEmailVerificationAsync)
            .WithName("RequestEmailVerification")
            .WithDescription("Request email verification token")
            .RequireAuthorization()
            .Produces<EmailVerificationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/email/verify", VerifyEmailAsync)
            .WithName("VerifyEmail")
            .WithDescription("Verify email using token")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/password/forgot", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .WithDescription("Request password reset token")
            .Produces<ForgotPasswordResponse>(StatusCodes.Status200OK);

        group.MapPost("/password/reset", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithDescription("Reset password using token")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/password/change", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithDescription("Change password for authenticated user")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapGet("/mfa/setup", GetMfaSetupAsync)
            .WithName("GetMfaSetup")
            .WithDescription("Get MFA setup information including QR code URI")
            .RequireAuthorization()
            .Produces<MfaSetupResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/mfa/enable", EnableMfaAsync)
            .WithName("EnableMfa")
            .WithDescription("Enable MFA for the authenticated user")
            .RequireAuthorization()
            .Produces<MfaEnabledResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/mfa/disable", DisableMfaAsync)
            .WithName("DisableMfa")
            .WithDescription("Disable MFA for the authenticated user")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
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

    private static async Task<IResult> RequestEmailVerificationAsync(
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await authService.RequestEmailVerificationAsync(userId, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.Ok(new EmailVerificationResponse
        {
            Email = result.Value.Email,
            Token = result.Value.Token
        });
    }

    private static async Task<IResult> VerifyEmailAsync(
        [FromBody] VerifyEmailRequest request,
        [FromServices] AuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.VerifyEmailAsync(request.Token, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.NoContent();
    }

    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] AuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.ForgotPasswordAsync(request.Email, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        // Always return success to prevent email enumeration
        // In a real app, you'd send the token via email, not in the response
        return Results.Ok(new ForgotPasswordResponse
        {
            Email = result.Value.Email,
            Message = "If an account exists with this email, a password reset link has been sent."
        });
    }

    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        [FromServices] AuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.ResetPasswordAsync(request.Token, request.NewPassword, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.NoContent();
    }

    private static async Task<IResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await authService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetMfaSetupAsync(
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await authService.GetMfaSetupAsync(userId, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.Ok(new MfaSetupResponse
        {
            Secret = result.Value.Secret,
            QrCodeUri = result.Value.QrCodeUri
        });
    }

    private static async Task<IResult> EnableMfaAsync(
        [FromBody] EnableMfaRequest request,
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await authService.EnableMfaAsync(
            userId,
            request.Secret,
            request.VerificationCode,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: GetStatusCode(result.Error));
        }

        return Results.Ok(new MfaEnabledResponse
        {
            BackupCodes = result.Value.BackupCodes.ToList()
        });
    }

    private static async Task<IResult> DisableMfaAsync(
        [FromBody] DisableMfaRequest request,
        [FromServices] AuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await authService.DisableMfaAsync(userId, request.Code, cancellationToken);

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
public sealed record VerifyEmailRequest(string Token);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record EnableMfaRequest(string Secret, string VerificationCode);
public sealed record DisableMfaRequest(string Code);

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

public sealed class EmailVerificationResponse
{
    public string Email { get; init; } = string.Empty;
    public string? Token { get; init; }
}

public sealed class ForgotPasswordResponse
{
    public string Email { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed class MfaSetupResponse
{
    public string Secret { get; init; } = string.Empty;
    public string QrCodeUri { get; init; } = string.Empty;
}

public sealed class MfaEnabledResponse
{
    public List<string> BackupCodes { get; init; } = new();
}
