using MissionManagement.Core.Enums;

namespace MissionManagement.Core.Entities;

/// <summary>
/// Represents a space mission with its configuration and lifecycle.
/// </summary>
public class Mission
{
    public Guid MissionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MissionType Type { get; private set; }
    public DateTimeOffset StartEpoch { get; private set; }
    public DateTimeOffset? EndEpoch { get; private set; }
    public MissionStatus Status { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    private Mission() { } // For EF Core

    /// <summary>
    /// Creates a new mission with the specified properties.
    /// </summary>
    public static Mission Create(
        string name,
        MissionType type,
        DateTimeOffset startEpoch,
        Guid ownerId,
        string? description = null,
        DateTimeOffset? endEpoch = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Mission name is required.", nameof(name));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID is required.", nameof(ownerId));

        return new Mission
        {
            MissionId = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            Status = MissionStatus.Draft,
            OwnerId = ownerId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Updates the mission metadata.
    /// </summary>
    public void Update(
        string name,
        MissionType type,
        DateTimeOffset startEpoch,
        string? description = null,
        DateTimeOffset? endEpoch = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Mission name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Type = type;
        StartEpoch = startEpoch;
        EndEpoch = endEpoch;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Changes the mission status.
    /// </summary>
    public void ChangeStatus(MissionStatus newStatus)
    {
        // No change needed
        if (Status == newStatus)
            return;

        // Validate status transitions: Draft → Active → Completed → Archived
        // Cannot change status of an archived mission
        if (Status == MissionStatus.Archived)
            throw new InvalidOperationException("Cannot change status of an archived mission.");

        // Validate allowed transitions
        var isValidTransition = Status switch
        {
            MissionStatus.Draft => newStatus is MissionStatus.Active or MissionStatus.Archived,
            MissionStatus.Active => newStatus is MissionStatus.Completed or MissionStatus.Archived,
            MissionStatus.Completed => newStatus == MissionStatus.Archived,
            _ => false
        };

        if (!isValidTransition)
        {
            throw new InvalidOperationException(
                $"Invalid status transition from {Status} to {newStatus}. " +
                "Valid transitions: Draft → Active/Archived, Active → Completed/Archived, Completed → Archived.");
        }

        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Soft deletes the mission.
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Mission is already deleted.");

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Checks if the specified user is the owner of this mission.
    /// </summary>
    public bool IsOwnedBy(Guid userId) => OwnerId == userId;
}
