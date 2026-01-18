namespace Identity.Core.Entities;

/// <summary>
/// Represents an API key for programmatic access.
/// </summary>
public class ApiKey
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public string KeyPrefix { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<string> _scopes = new();
    public IReadOnlyCollection<string> Scopes => _scopes.AsReadOnly();

    public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow >= ExpiresAt.Value;
    public bool IsValid => IsActive && !IsExpired;

    private ApiKey() { } // For EF Core

    public static ApiKey Create(Guid userId, string name, string keyHash, string keyPrefix,
        IEnumerable<string>? scopes = null, TimeSpan? lifetime = null)
    {
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = lifetime.HasValue ? DateTimeOffset.UtcNow.Add(lifetime.Value) : null,
            IsActive = true
        };

        if (scopes != null)
        {
            apiKey._scopes.AddRange(scopes);
        }

        return apiKey;
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void UpdateScopes(IEnumerable<string> scopes)
    {
        _scopes.Clear();
        _scopes.AddRange(scopes);
    }
}
