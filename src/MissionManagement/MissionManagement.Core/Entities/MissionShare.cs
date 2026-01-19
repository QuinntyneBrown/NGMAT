namespace MissionManagement.Core.Entities;

public sealed class MissionShare
{
    public Guid Id { get; init; }
    public Guid MissionId { get; init; }
    public Guid UserId { get; init; }
    public MissionPermission Permission { get; init; }
    public DateTime SharedAt { get; init; }
    public Guid SharedByUserId { get; init; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public static MissionShare Create(
        Guid missionId,
        Guid userId,
        MissionPermission permission,
        Guid sharedByUserId)
    {
        return new MissionShare
        {
            Id = Guid.NewGuid(),
            MissionId = missionId,
            UserId = userId,
            Permission = permission,
            SharedAt = DateTime.UtcNow,
            SharedByUserId = sharedByUserId,
            IsRevoked = false
        };
    }

    private MissionShare() { }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
