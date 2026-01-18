namespace Identity.Core.Entities;

/// <summary>
/// Represents a permission in the system.
/// </summary>
public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private readonly List<RolePermission> _roles = new();
    public IReadOnlyCollection<RolePermission> Roles => _roles.AsReadOnly();

    private Permission() { } // For EF Core

    public static Permission Create(string resource, string action, string? description = null)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = $"{resource}:{action}",
            Resource = resource,
            Action = action,
            Description = description
        };
    }

    public static class Resources
    {
        public const string Missions = "missions";
        public const string Spacecraft = "spacecraft";
        public const string Propagation = "propagation";
        public const string Users = "users";
        public const string Roles = "roles";
        public const string Settings = "settings";
    }

    public static class Actions
    {
        public const string Create = "create";
        public const string Read = "read";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Execute = "execute";
        public const string Admin = "admin";
    }
}

/// <summary>
/// Join entity for Role-Permission relationship.
/// </summary>
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
