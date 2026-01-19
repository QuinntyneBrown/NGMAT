namespace MissionManagement.Core.Entities;

public sealed class Mission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MissionType Type { get; private set; }
    public MissionStatus Status { get; private set; }
    public DateTime StartEpoch { get; private set; }
    public DateTime? EndEpoch { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private readonly List<MissionShare> _shares = new();
    public IReadOnlyCollection<MissionShare> Shares => _shares.AsReadOnly();

    private readonly List<MissionStatusHistory> _statusHistory = new();
    public IReadOnlyCollection<MissionStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private Mission() { }

    public static Mission Create(
        string name,
        MissionType type,
        DateTime startEpoch,
        Guid ownerId,
        string? description = null,
        DateTime? endEpoch = null)
    {
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            Status = MissionStatus.Draft,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        mission._statusHistory.Add(new MissionStatusHistory
        {
            Id = Guid.NewGuid(),
            MissionId = mission.Id,
            FromStatus = null,
            ToStatus = MissionStatus.Draft,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = ownerId
        });

        return mission;
    }

    public void Update(string? name, string? description, DateTime? startEpoch, DateTime? endEpoch)
    {
        if (name != null)
            Name = name;
        if (description != null)
            Description = description;
        if (startEpoch.HasValue)
            StartEpoch = startEpoch.Value;
        EndEpoch = endEpoch;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public bool CanTransitionTo(MissionStatus newStatus)
    {
        return Status switch
        {
            MissionStatus.Draft => newStatus == MissionStatus.Active,
            MissionStatus.Active => newStatus == MissionStatus.Completed,
            MissionStatus.Completed => newStatus == MissionStatus.Archived,
            MissionStatus.Archived => false,
            _ => false
        };
    }

    public void TransitionTo(MissionStatus newStatus, Guid changedByUserId)
    {
        if (!CanTransitionTo(newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");

        var history = new MissionStatusHistory
        {
            Id = Guid.NewGuid(),
            MissionId = Id,
            FromStatus = Status,
            ToStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId
        };

        _statusHistory.Add(history);
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddShare(Guid userId, MissionPermission permission, Guid sharedByUserId)
    {
        if (_shares.Any(s => s.UserId == userId && !s.IsRevoked))
            throw new InvalidOperationException("User already has access to this mission");

        var share = MissionShare.Create(Id, userId, permission, sharedByUserId);
        _shares.Add(share);
    }

    public void RevokeShare(Guid userId)
    {
        var share = _shares.FirstOrDefault(s => s.UserId == userId && !s.IsRevoked);
        if (share == null)
            throw new InvalidOperationException("User does not have access to this mission");

        share.Revoke();
    }

    public bool HasAccess(Guid userId) =>
        OwnerId == userId || _shares.Any(s => s.UserId == userId && !s.IsRevoked);

    public bool CanEdit(Guid userId) =>
        OwnerId == userId || _shares.Any(s => s.UserId == userId && !s.IsRevoked && s.Permission == MissionPermission.ReadWrite);

    public Mission Clone(Guid newOwnerId)
    {
        return new Mission
        {
            Id = Guid.NewGuid(),
            Name = $"{Name} (Copy)",
            Description = Description,
            Type = Type,
            Status = MissionStatus.Draft,
            StartEpoch = StartEpoch,
            EndEpoch = EndEpoch,
            OwnerId = newOwnerId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }
}

public enum MissionType
{
    LEO,
    MEO,
    GEO,
    HEO,
    Lunar,
    Interplanetary,
    Other
}

public enum MissionStatus
{
    Draft,
    Active,
    Completed,
    Archived
}

public enum MissionPermission
{
    ReadOnly,
    ReadWrite
}
