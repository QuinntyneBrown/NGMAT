namespace CoordinateSystem.Core.Entities;

/// <summary>
/// Predefined reference frames commonly used in astrodynamics.
/// </summary>
public static class BuiltInFrames
{
    /// <summary>
    /// Earth-Centered Inertial J2000 frame.
    /// Origin at Earth center, aligned with mean equator and equinox of J2000.0 epoch.
    /// </summary>
    public static readonly Guid EciJ2000Id = new("00000000-0000-0000-0001-000000000001");
    public static ReferenceFrame EciJ2000 => ReferenceFrame.CreateBuiltIn(
        EciJ2000Id,
        "ECI J2000",
        ReferenceFrameType.Inertial,
        CentralBody.Earth,
        AxesDefinition.MeanEquatorMeanEquinoxJ2000,
        OriginDefinition.CenterOfMass,
        new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        "Earth-Centered Inertial frame aligned with J2000.0 mean equator and equinox");

    /// <summary>
    /// Earth-Centered Earth-Fixed frame (ITRF).
    /// Origin at Earth center, rotates with Earth.
    /// </summary>
    public static readonly Guid EcefId = new("00000000-0000-0000-0001-000000000002");
    public static ReferenceFrame Ecef => ReferenceFrame.CreateBuiltIn(
        EcefId,
        "ECEF",
        ReferenceFrameType.BodyFixed,
        CentralBody.Earth,
        AxesDefinition.ITRF,
        OriginDefinition.CenterOfMass,
        null,
        "Earth-Centered Earth-Fixed frame (ITRF)");

    /// <summary>
    /// International Celestial Reference Frame (ICRF).
    /// Quasi-inertial frame defined by extragalactic radio sources.
    /// </summary>
    public static readonly Guid IcrfId = new("00000000-0000-0000-0001-000000000003");
    public static ReferenceFrame Icrf => ReferenceFrame.CreateBuiltIn(
        IcrfId,
        "ICRF",
        ReferenceFrameType.Inertial,
        CentralBody.SolarSystemBarycenter,
        AxesDefinition.ICRF,
        OriginDefinition.CenterOfMass,
        null,
        "International Celestial Reference Frame");

    /// <summary>
    /// Moon-Centered Inertial frame.
    /// Origin at Moon center, axes parallel to ECI J2000.
    /// </summary>
    public static readonly Guid MoonCenteredInertialId = new("00000000-0000-0000-0001-000000000004");
    public static ReferenceFrame MoonCenteredInertial => ReferenceFrame.CreateBuiltIn(
        MoonCenteredInertialId,
        "Moon-Centered Inertial",
        ReferenceFrameType.Inertial,
        CentralBody.Moon,
        AxesDefinition.MeanEquatorMeanEquinoxJ2000,
        OriginDefinition.CenterOfMass,
        new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        "Moon-Centered Inertial frame with axes parallel to ECI J2000");

    /// <summary>
    /// Sun-Centered Inertial frame (Heliocentric).
    /// Origin at Sun center, axes parallel to ECI J2000.
    /// </summary>
    public static readonly Guid SunCenteredInertialId = new("00000000-0000-0000-0001-000000000005");
    public static ReferenceFrame SunCenteredInertial => ReferenceFrame.CreateBuiltIn(
        SunCenteredInertialId,
        "Sun-Centered Inertial",
        ReferenceFrameType.Inertial,
        CentralBody.Sun,
        AxesDefinition.MeanEquatorMeanEquinoxJ2000,
        OriginDefinition.CenterOfMass,
        new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        "Sun-Centered Inertial frame (heliocentric)");

    /// <summary>
    /// Mars-Centered Inertial frame.
    /// </summary>
    public static readonly Guid MarsCenteredInertialId = new("00000000-0000-0000-0001-000000000006");
    public static ReferenceFrame MarsCenteredInertial => ReferenceFrame.CreateBuiltIn(
        MarsCenteredInertialId,
        "Mars-Centered Inertial",
        ReferenceFrameType.Inertial,
        CentralBody.Mars,
        AxesDefinition.MeanEquatorMeanEquinoxJ2000,
        OriginDefinition.CenterOfMass,
        new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        "Mars-Centered Inertial frame");

    /// <summary>
    /// Get all built-in reference frames.
    /// </summary>
    public static IReadOnlyList<ReferenceFrame> GetAll() => new[]
    {
        EciJ2000,
        Ecef,
        Icrf,
        MoonCenteredInertial,
        SunCenteredInertial,
        MarsCenteredInertial
    };
}

/// <summary>
/// Gravitational parameters for celestial bodies (GM in km³/s²).
/// </summary>
public static class GravitationalParameters
{
    public const double Sun = 132712440041.93938;
    public const double Mercury = 22031.78;
    public const double Venus = 324858.592;
    public const double Earth = 398600.4418;
    public const double Moon = 4902.800066;
    public const double Mars = 42828.375214;
    public const double Jupiter = 126686534.921800;
    public const double Saturn = 37931206.159;
    public const double Uranus = 5793951.256;
    public const double Neptune = 6835099.97;
    public const double Pluto = 871;

    public static double GetMu(CentralBody body) => body switch
    {
        CentralBody.Sun => Sun,
        CentralBody.Mercury => Mercury,
        CentralBody.Venus => Venus,
        CentralBody.Earth => Earth,
        CentralBody.Moon => Moon,
        CentralBody.Mars => Mars,
        CentralBody.Jupiter => Jupiter,
        CentralBody.Saturn => Saturn,
        CentralBody.Uranus => Uranus,
        CentralBody.Neptune => Neptune,
        CentralBody.Pluto => Pluto,
        _ => Earth
    };
}
