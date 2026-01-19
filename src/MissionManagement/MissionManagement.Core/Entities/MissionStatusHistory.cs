namespace MissionManagement.Core.Entities;

public sealed class MissionStatusHistory
{
    public Guid Id { get; init; }
    public Guid MissionId { get; init; }
    public MissionStatus? FromStatus { get; init; }
    public MissionStatus ToStatus { get; init; }
    public DateTime ChangedAt { get; init; }
    public Guid ChangedByUserId { get; init; }
}
