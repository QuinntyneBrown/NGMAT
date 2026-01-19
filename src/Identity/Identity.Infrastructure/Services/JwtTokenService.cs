using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services;

/// <summary>
/// JWT token generation and validation service.
/// </summary>
internal sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public TimeSpan AccessTokenLifetime => _options.AccessTokenLifetime;
    public TimeSpan RefreshTokenLifetime => _options.RefreshTokenLifetime;

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("display_name", user.DisplayName ?? string.Empty)
        };

        // Add roles
        foreach (var userRole in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));

            // Add permissions from role
            foreach (var rolePermission in userRole.Role.Permissions)
            {
                claims.Add(new Claim("permission", rolePermission.Permission.Name));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(_options.AccessTokenLifetime),
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Guid? ValidateAccessToken(string token)
    {
        return ValidateToken(token, "access");
    }

    public string GenerateEmailVerificationToken(Guid userId)
    {
        return GeneratePurposeToken(userId, "email_verification", _options.EmailVerificationTokenLifetime);
    }

    public Guid? ValidateEmailVerificationToken(string token)
    {
        return ValidateToken(token, "email_verification");
    }

    public string GeneratePasswordResetToken(Guid userId)
    {
        return GeneratePurposeToken(userId, "password_reset", _options.PasswordResetTokenLifetime);
    }

    public Guid? ValidatePasswordResetToken(string token)
    {
        return ValidateToken(token, "password_reset");
    }

    private string GeneratePurposeToken(Guid userId, string purpose, TimeSpan lifetime)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("purpose", purpose)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private Guid? ValidateToken(string token, string expectedPurpose)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            // For access tokens, there's no purpose claim
            if (expectedPurpose != "access")
            {
                var purposeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "purpose")?.Value;
                if (purposeClaim != expectedPurpose)
                {
                    return null;
                }
            }

            var userId = jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
            return Guid.Parse(userId);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// JWT configuration options.
/// </summary>
public sealed class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "NGMAT";
    public string Audience { get; set; } = "NGMAT";
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);
    public TimeSpan EmailVerificationTokenLifetime { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan PasswordResetTokenLifetime { get; set; } = TimeSpan.FromHours(1);
}
