namespace Identity.Core.Entities;

/// <summary>
/// Represents a role in the system.
/// </summary>
public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<RolePermission> _permissions = new();
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    private readonly List<UserRole> _users = new();
    public IReadOnlyCollection<UserRole> Users => _users.AsReadOnly();

    private Role() { } // For EF Core

    public static Role Create(string name, string? description = null, bool isSystem = false)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsSystem = isSystem,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, string? description)
    {
        if (!IsSystem)
        {
            Name = name;
            Description = description;
        }
    }

    public void AddPermission(Permission permission)
    {
        if (!_permissions.Any(p => p.PermissionId == permission.Id))
        {
            _permissions.Add(new RolePermission { RoleId = Id, PermissionId = permission.Id, Permission = permission });
        }
    }

    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _permissions.Remove(rolePermission);
        }
    }

    public static class SystemRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string ReadOnly = "ReadOnly";
    }
}

/// <summary>
/// Join entity for User-Role relationship.
/// </summary>
public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
}
