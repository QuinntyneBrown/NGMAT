namespace Identity.Core.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsMfaEnabled { get; private set; }
    public string? MfaSecret { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockoutEndAt { get; private set; }

    private readonly List<UserRole> _roles = new();
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { } // For EF Core

    public static User Create(string email, string passwordHash, string? displayName = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            DisplayName = displayName,
            IsEmailVerified = false,
            IsActive = true,
            IsMfaEnabled = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        // Invalidate all refresh tokens on password change
        _refreshTokens.Clear();
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
    }

    public void UpdateDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEndAt = null;
    }

    public void RecordFailedLogin(int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEndAt = DateTimeOffset.UtcNow.Add(lockoutDuration);
        }
    }

    public bool IsLockedOut => LockoutEndAt.HasValue && LockoutEndAt.Value > DateTimeOffset.UtcNow;

    public void Deactivate()
    {
        IsActive = false;
        _refreshTokens.Clear();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void EnableMfa(string secret)
    {
        MfaSecret = secret;
        IsMfaEnabled = true;
    }

    public void DisableMfa()
    {
        MfaSecret = null;
        IsMfaEnabled = false;
    }

    public void AddRole(Role role)
    {
        if (!_roles.Any(r => r.RoleId == role.Id))
        {
            _roles.Add(new UserRole { UserId = Id, RoleId = role.Id, Role = role });
        }
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (userRole != null)
        {
            _roles.Remove(userRole);
        }
    }

    public void AddRefreshToken(RefreshToken token)
    {
        // Remove expired tokens
        _refreshTokens.RemoveAll(t => t.ExpiresAt < DateTimeOffset.UtcNow);
        _refreshTokens.Add(token);
    }

    public void RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        if (refreshToken != null)
        {
            refreshToken.Revoke();
        }
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens)
        {
            token.Revoke();
        }
    }
}
