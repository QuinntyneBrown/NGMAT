namespace MissionManagement.Core.Enums;

/// <summary>
/// Represents the type of mission based on orbital regime.
/// </summary>
public enum MissionType
{
    /// <summary>
    /// Low Earth Orbit mission.
    /// </summary>
    LEO = 0,

    /// <summary>
    /// Geostationary Earth Orbit mission.
    /// </summary>
    GEO = 1,

    /// <summary>
    /// Medium Earth Orbit mission.
    /// </summary>
    MEO = 2,

    /// <summary>
    /// Highly Elliptical Orbit mission.
    /// </summary>
    HEO = 3,

    /// <summary>
    /// Lunar mission.
    /// </summary>
    Lunar = 4,

    /// <summary>
    /// Interplanetary mission.
    /// </summary>
    Interplanetary = 5,

    /// <summary>
    /// Deep space mission.
    /// </summary>
    DeepSpace = 6,

    /// <summary>
    /// Custom or unspecified mission type.
    /// </summary>
    Other = 99
}
