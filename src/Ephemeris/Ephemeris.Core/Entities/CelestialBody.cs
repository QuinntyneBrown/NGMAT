namespace Ephemeris.Core.Entities;

public enum CelestialBodyType
{
    Star,
    Planet,
    Moon,
    Asteroid,
    Comet,
    DwarfPlanet,
    Barycenter
}

public sealed class CelestialBody
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int NaifId { get; private set; }
    public CelestialBodyType Type { get; private set; }
    public double GravitationalParameterM3S2 { get; private set; }
    public double MeanRadiusKm { get; private set; }
    public double? EquatorialRadiusKm { get; private set; }
    public double? PolarRadiusKm { get; private set; }
    public double? FlatteningCoefficient { get; private set; }
    public double? J2Coefficient { get; private set; }
    public double? RotationPeriodSeconds { get; private set; }
    public Guid? ParentBodyId { get; private set; }
    public CelestialBody? ParentBody { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CelestialBody() { }

    public static CelestialBody Create(
        string name,
        int naifId,
        CelestialBodyType type,
        double gravitationalParameterM3S2,
        double meanRadiusKm,
        double? equatorialRadiusKm = null,
        double? polarRadiusKm = null,
        double? flatteningCoefficient = null,
        double? j2Coefficient = null,
        double? rotationPeriodSeconds = null,
        Guid? parentBodyId = null)
    {
        return new CelestialBody
        {
            Id = Guid.NewGuid(),
            Name = name,
            NaifId = naifId,
            Type = type,
            GravitationalParameterM3S2 = gravitationalParameterM3S2,
            MeanRadiusKm = meanRadiusKm,
            EquatorialRadiusKm = equatorialRadiusKm,
            PolarRadiusKm = polarRadiusKm,
            FlatteningCoefficient = flatteningCoefficient,
            J2Coefficient = j2Coefficient,
            RotationPeriodSeconds = rotationPeriodSeconds,
            ParentBodyId = parentBodyId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        double? gravitationalParameterM3S2 = null,
        double? meanRadiusKm = null,
        double? equatorialRadiusKm = null,
        double? polarRadiusKm = null,
        double? flatteningCoefficient = null,
        double? j2Coefficient = null,
        double? rotationPeriodSeconds = null)
    {
        if (gravitationalParameterM3S2.HasValue)
            GravitationalParameterM3S2 = gravitationalParameterM3S2.Value;
        if (meanRadiusKm.HasValue)
            MeanRadiusKm = meanRadiusKm.Value;
        if (equatorialRadiusKm.HasValue)
            EquatorialRadiusKm = equatorialRadiusKm.Value;
        if (polarRadiusKm.HasValue)
            PolarRadiusKm = polarRadiusKm.Value;
        if (flatteningCoefficient.HasValue)
            FlatteningCoefficient = flatteningCoefficient.Value;
        if (j2Coefficient.HasValue)
            J2Coefficient = j2Coefficient.Value;
        if (rotationPeriodSeconds.HasValue)
            RotationPeriodSeconds = rotationPeriodSeconds.Value;

        UpdatedAt = DateTime.UtcNow;
    }
}

public static class StandardCelestialBodies
{
    // NAIF IDs for standard celestial bodies
    public const int SunNaifId = 10;
    public const int MercuryNaifId = 199;
    public const int VenusNaifId = 299;
    public const int EarthNaifId = 399;
    public const int MoonNaifId = 301;
    public const int MarsNaifId = 499;
    public const int JupiterNaifId = 599;
    public const int SaturnNaifId = 699;
    public const int UranusNaifId = 799;
    public const int NeptuneNaifId = 899;
    public const int PlutoNaifId = 999;
    public const int EarthMoonBarycenterNaifId = 3;

    // Standard gravitational parameters (GM) in m^3/s^2
    public static readonly double SunGM = 1.32712440041279419e20;
    public static readonly double MercuryGM = 2.2031868551e13;
    public static readonly double VenusGM = 3.24858592e14;
    public static readonly double EarthGM = 3.986004418e14;
    public static readonly double MoonGM = 4.9028000661e12;
    public static readonly double MarsGM = 4.282837362e13;
    public static readonly double JupiterGM = 1.26686531e17;
    public static readonly double SaturnGM = 3.79311879e16;
    public static readonly double UranusGM = 5.7939399e15;
    public static readonly double NeptuneGM = 6.8365299e15;

    // Mean radii in km
    public static readonly double SunRadius = 695700.0;
    public static readonly double MercuryRadius = 2439.7;
    public static readonly double VenusRadius = 6051.8;
    public static readonly double EarthEquatorialRadius = 6378.137;
    public static readonly double EarthPolarRadius = 6356.752;
    public static readonly double MoonRadius = 1737.4;
    public static readonly double MarsRadius = 3389.5;
    public static readonly double JupiterRadius = 69911.0;
    public static readonly double SaturnRadius = 58232.0;
    public static readonly double UranusRadius = 25362.0;
    public static readonly double NeptuneRadius = 24622.0;

    // Earth-specific constants
    public static readonly double EarthFlattening = 1.0 / 298.257223563;
    public static readonly double EarthJ2 = 1.08262668e-3;
    public static readonly double EarthRotationPeriod = 86164.0905; // Sidereal day in seconds

    // Moon-specific constants
    public static readonly double MoonJ2 = 2.0323e-4;

    public static IEnumerable<CelestialBody> GetStandardBodies()
    {
        yield return CelestialBody.Create("Sun", SunNaifId, CelestialBodyType.Star, SunGM, SunRadius);
        yield return CelestialBody.Create("Mercury", MercuryNaifId, CelestialBodyType.Planet, MercuryGM, MercuryRadius);
        yield return CelestialBody.Create("Venus", VenusNaifId, CelestialBodyType.Planet, VenusGM, VenusRadius);

        var earth = CelestialBody.Create("Earth", EarthNaifId, CelestialBodyType.Planet, EarthGM,
            (EarthEquatorialRadius + EarthPolarRadius) / 2, EarthEquatorialRadius, EarthPolarRadius,
            EarthFlattening, EarthJ2, EarthRotationPeriod);
        yield return earth;

        yield return CelestialBody.Create("Moon", MoonNaifId, CelestialBodyType.Moon, MoonGM, MoonRadius,
            null, null, null, MoonJ2, null, earth.Id);

        yield return CelestialBody.Create("Mars", MarsNaifId, CelestialBodyType.Planet, MarsGM, MarsRadius);
        yield return CelestialBody.Create("Jupiter", JupiterNaifId, CelestialBodyType.Planet, JupiterGM, JupiterRadius);
        yield return CelestialBody.Create("Saturn", SaturnNaifId, CelestialBodyType.Planet, SaturnGM, SaturnRadius);
        yield return CelestialBody.Create("Uranus", UranusNaifId, CelestialBodyType.Planet, UranusGM, UranusRadius);
        yield return CelestialBody.Create("Neptune", NeptuneNaifId, CelestialBodyType.Planet, NeptuneGM, NeptuneRadius);
    }
}
