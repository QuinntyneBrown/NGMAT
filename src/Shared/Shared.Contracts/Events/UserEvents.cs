using MessagePack;
using Shared.Messaging.Abstractions;

namespace Shared.Contracts.Events;

/// <summary>
/// Event raised when a user logs in.
/// </summary>
[MessagePackObject]
public sealed class UserLoggedIn : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string? Email { get; init; }

    [Key(12)]
    public string LoginMethod { get; init; } = string.Empty;

    [Key(13)]
    public string? IpAddress { get; init; }

    [Key(14)]
    public string? UserAgent { get; init; }
}

/// <summary>
/// Event raised when a user logs out.
/// </summary>
[MessagePackObject]
public sealed class UserLoggedOut : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string LogoutReason { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a token is refreshed.
/// </summary>
[MessagePackObject]
public sealed class TokenRefreshed : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public DateTimeOffset NewExpiration { get; init; }

    [Key(12)]
    public string? IpAddress { get; init; }
}

/// <summary>
/// Event raised when a user's permissions change.
/// </summary>
[MessagePackObject]
public sealed class PermissionChanged : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string[] AddedPermissions { get; init; } = Array.Empty<string>();

    [Key(12)]
    public string[] RemovedPermissions { get; init; } = Array.Empty<string>();

    [Key(13)]
    public string ChangedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a user is created.
/// </summary>
[MessagePackObject]
public sealed class UserCreated : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string? Email { get; init; }

    [Key(12)]
    public string? DisplayName { get; init; }

    [Key(13)]
    public string CreatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a user is deactivated.
/// </summary>
[MessagePackObject]
public sealed class UserDeactivated : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string Reason { get; init; } = string.Empty;

    [Key(12)]
    public string DeactivatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a user's email is verified.
/// </summary>
[MessagePackObject]
public sealed class UserEmailVerified : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string? Email { get; init; }
}

/// <summary>
/// Event raised when a user's password is changed.
/// </summary>
[MessagePackObject]
public sealed class UserPasswordChanged : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string? Email { get; init; }
}

/// <summary>
/// Event raised when MFA is enabled for a user.
/// </summary>
[MessagePackObject]
public sealed class MfaEnabled : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string? Email { get; init; }
}

/// <summary>
/// Event raised when MFA is disabled for a user.
/// </summary>
[MessagePackObject]
public sealed class MfaDisabled : EventBase
{
    [Key(10)]
    public string UserIdentifier { get; init; } = string.Empty;

    [Key(11)]
    public string? Email { get; init; }
}
