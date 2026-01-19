namespace CoordinateSystem.Core.Entities;

/// <summary>
/// Defines a coordinate reference frame for orbital mechanics calculations.
/// </summary>
public sealed class ReferenceFrame
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ReferenceFrameType Type { get; private set; }
    public CentralBody CentralBody { get; private set; }
    public AxesDefinition Axes { get; private set; }
    public OriginDefinition Origin { get; private set; }
    public DateTime? Epoch { get; private set; }
    public bool IsInertial { get; private set; }
    public bool IsBuiltIn { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    private ReferenceFrame() { }

    public static ReferenceFrame Create(
        string name,
        ReferenceFrameType type,
        CentralBody centralBody,
        AxesDefinition axes,
        OriginDefinition origin,
        DateTime? epoch = null,
        string? description = null,
        Guid? createdByUserId = null)
    {
        return new ReferenceFrame
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            CentralBody = centralBody,
            Axes = axes,
            Origin = origin,
            Epoch = epoch,
            Description = description,
            IsInertial = type == ReferenceFrameType.Inertial,
            IsBuiltIn = false,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public static ReferenceFrame CreateBuiltIn(
        Guid id,
        string name,
        ReferenceFrameType type,
        CentralBody centralBody,
        AxesDefinition axes,
        OriginDefinition origin,
        DateTime? epoch = null,
        string? description = null)
    {
        return new ReferenceFrame
        {
            Id = id,
            Name = name,
            Type = type,
            CentralBody = centralBody,
            Axes = axes,
            Origin = origin,
            Epoch = epoch,
            Description = description,
            IsInertial = type == ReferenceFrameType.Inertial,
            IsBuiltIn = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = null
        };
    }
}

public enum ReferenceFrameType
{
    Inertial,
    BodyFixed,
    Rotating,
    Topocentric
}

public enum CentralBody
{
    Sun,
    Mercury,
    Venus,
    Earth,
    Moon,
    Mars,
    Jupiter,
    Saturn,
    Uranus,
    Neptune,
    Pluto,
    SolarSystemBarycenter
}

public enum AxesDefinition
{
    MeanEquatorMeanEquinoxJ2000,  // ECI J2000
    TrueEquatorTrueEquinox,       // True of Date
    ITRF,                          // International Terrestrial Reference Frame
    ICRF,                          // International Celestial Reference Frame
    VNB,                           // Velocity-Normal-Binormal
    LVLH,                          // Local Vertical Local Horizontal
    RSW,                           // Radial-Transverse-Normal
    Custom
}

public enum OriginDefinition
{
    CenterOfMass,
    Topocentric,
    SpacecraftCentered,
    Custom
}
