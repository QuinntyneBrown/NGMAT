namespace Identity.Core.Entities;

/// <summary>
/// Represents a refresh token for JWT token renewal.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { } // For EF Core

    public static RefreshToken Create(Guid userId, string token, TimeSpan lifetime, string? ipAddress = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    public void Revoke(string? ipAddress = null, string? replacedByToken = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        RevokedByIp = ipAddress;
        ReplacedByToken = replacedByToken;
    }
}
