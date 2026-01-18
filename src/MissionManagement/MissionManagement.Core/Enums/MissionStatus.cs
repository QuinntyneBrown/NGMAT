namespace MissionManagement.Core.Enums;

/// <summary>
/// Represents the lifecycle status of a mission.
/// </summary>
public enum MissionStatus
{
    /// <summary>
    /// Initial state, being configured.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Mission is being worked on.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Mission analysis complete.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Mission archived for reference.
    /// </summary>
    Archived = 3
}
