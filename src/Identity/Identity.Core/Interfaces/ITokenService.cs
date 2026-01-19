using Identity.Core.Entities;

namespace Identity.Core.Interfaces;

/// <summary>
/// Interface for JWT token generation and validation.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT access token and returns the user ID.
    /// </summary>
    Guid? ValidateAccessToken(string token);

    /// <summary>
    /// Gets the access token lifetime.
    /// </summary>
    TimeSpan AccessTokenLifetime { get; }

    /// <summary>
    /// Gets the refresh token lifetime.
    /// </summary>
    TimeSpan RefreshTokenLifetime { get; }

    /// <summary>
    /// Generates an email verification token.
    /// </summary>
    string GenerateEmailVerificationToken(Guid userId);

    /// <summary>
    /// Validates an email verification token and returns the user ID.
    /// </summary>
    Guid? ValidateEmailVerificationToken(string token);

    /// <summary>
    /// Generates a password reset token.
    /// </summary>
    string GeneratePasswordResetToken(Guid userId);

    /// <summary>
    /// Validates a password reset token and returns the user ID.
    /// </summary>
    Guid? ValidatePasswordResetToken(string token);
}
