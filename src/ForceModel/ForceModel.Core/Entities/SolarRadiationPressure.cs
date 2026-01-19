namespace ForceModel.Core.Entities;

public static class SrpConstants
{
    // Solar flux at 1 AU (W/m^2)
    public const double SolarFlux = 1367.0;

    // Speed of light (m/s)
    public const double SpeedOfLight = 299792458.0;

    // Solar radiation pressure at 1 AU (N/m^2)
    public const double SrpPressure1AU = SolarFlux / SpeedOfLight; // ~4.56e-6 N/m^2

    // 1 AU in meters
    public const double AstronomicalUnit = 1.495978707e11;

    // Sun radius in meters
    public const double SunRadius = 6.96e8;

    // Earth radius in meters
    public const double EarthRadius = 6378137.0;

    // Moon radius in meters
    public const double MoonRadius = 1737400.0;
}

public enum EclipseType
{
    None,
    Penumbra,
    Umbra
}

public readonly struct SrpAcceleration
{
    public double Ax { get; init; }
    public double Ay { get; init; }
    public double Az { get; init; }
    public EclipseType Eclipse { get; init; }
    public double ShadowFactor { get; init; }

    public SrpAcceleration(double ax, double ay, double az, EclipseType eclipse = EclipseType.None, double shadowFactor = 1.0)
    {
        Ax = ax;
        Ay = ay;
        Az = az;
        Eclipse = eclipse;
        ShadowFactor = shadowFactor;
    }

    public double Magnitude => Math.Sqrt(Ax * Ax + Ay * Ay + Az * Az);

    public static SrpAcceleration Zero => new(0, 0, 0, EclipseType.None, 0);
}

public static class CannonBallSrp
{
    public static SrpAcceleration Calculate(
        SpacecraftState state,
        SpacecraftProperties props,
        double sunX, double sunY, double sunZ,
        bool checkEclipse = true)
    {
        // Vector from spacecraft to Sun
        var toSunX = sunX - state.X;
        var toSunY = sunY - state.Y;
        var toSunZ = sunZ - state.Z;

        var distToSun = Math.Sqrt(toSunX * toSunX + toSunY * toSunY + toSunZ * toSunZ);

        // Check for eclipse if enabled
        var eclipse = EclipseType.None;
        var shadowFactor = 1.0;

        if (checkEclipse)
        {
            (eclipse, shadowFactor) = CalculateEclipse(state, sunX, sunY, sunZ);
        }

        if (shadowFactor <= 0)
        {
            return SrpAcceleration.Zero;
        }

        // SRP at distance from Sun (varies with 1/r^2)
        var auFactor = SrpConstants.AstronomicalUnit / distToSun;
        var pressure = SrpConstants.SrpPressure1AU * auFactor * auFactor;

        // Cannon ball model: a = -P * Cr * A/m * r_hat
        // Cr = 1 + reflectivity (1 for absorbing, 2 for perfectly reflecting)
        var cr = 1.0 + props.ReflectivityCoefficient;
        var factor = -pressure * cr * props.SrpAreaM2 * shadowFactor / props.MassKg;

        // Unit vector from Sun to spacecraft (opposite direction of acceleration)
        var unitX = -toSunX / distToSun;
        var unitY = -toSunY / distToSun;
        var unitZ = -toSunZ / distToSun;

        return new SrpAcceleration(
            factor * unitX,
            factor * unitY,
            factor * unitZ,
            eclipse,
            shadowFactor);
    }

    public static (EclipseType type, double factor) CalculateEclipse(
        SpacecraftState state,
        double sunX, double sunY, double sunZ)
    {
        // Check Earth shadow using conical shadow model
        // Vector from Earth to Sun
        var earthSunX = sunX;
        var earthSunY = sunY;
        var earthSunZ = sunZ;
        var earthSunDist = Math.Sqrt(earthSunX * earthSunX + earthSunY * earthSunY + earthSunZ * earthSunZ);

        // Unit vector from Earth to Sun
        var sunHatX = earthSunX / earthSunDist;
        var sunHatY = earthSunY / earthSunDist;
        var sunHatZ = earthSunZ / earthSunDist;

        // Spacecraft position in shadow coordinates
        // Project spacecraft onto Sun-Earth line
        var scDotSun = state.X * sunHatX + state.Y * sunHatY + state.Z * sunHatZ;

        // If spacecraft is on Sun side of Earth, no eclipse
        if (scDotSun > 0)
        {
            return (EclipseType.None, 1.0);
        }

        // Perpendicular distance from Sun-Earth line
        var perpX = state.X - scDotSun * sunHatX;
        var perpY = state.Y - scDotSun * sunHatY;
        var perpZ = state.Z - scDotSun * sunHatZ;
        var perpDist = Math.Sqrt(perpX * perpX + perpY * perpY + perpZ * perpZ);

        // Distance behind Earth along Sun-Earth line
        var behindDist = -scDotSun;

        // Calculate umbra and penumbra cone angles
        var f1 = Math.Asin((SrpConstants.SunRadius + SrpConstants.EarthRadius) / earthSunDist); // Penumbra angle
        var f2 = Math.Asin((SrpConstants.SunRadius - SrpConstants.EarthRadius) / earthSunDist); // Umbra angle

        // Penumbra and umbra radii at spacecraft distance
        var penumbraRadius = SrpConstants.EarthRadius + behindDist * Math.Tan(f1);
        var umbraRadius = SrpConstants.EarthRadius - behindDist * Math.Tan(f2);

        if (perpDist > penumbraRadius)
        {
            // No eclipse
            return (EclipseType.None, 1.0);
        }

        if (umbraRadius > 0 && perpDist < umbraRadius)
        {
            // Total eclipse (umbra)
            return (EclipseType.Umbra, 0.0);
        }

        // Penumbra - partial shadow
        // Linear interpolation for shadow factor
        double factor;
        if (umbraRadius > 0)
        {
            factor = (perpDist - umbraRadius) / (penumbraRadius - umbraRadius);
        }
        else
        {
            // Past umbra vertex, antumbra region
            factor = perpDist / penumbraRadius;
        }

        return (EclipseType.Penumbra, Math.Max(0, Math.Min(1, factor)));
    }
}

public static class CylindricalShadow
{
    public static (EclipseType type, double factor) Calculate(
        SpacecraftState state,
        double sunX, double sunY, double sunZ)
    {
        // Simplified cylindrical shadow model
        // Vector from Earth to Sun
        var earthSunDist = Math.Sqrt(sunX * sunX + sunY * sunY + sunZ * sunZ);

        // Unit vector from Earth to Sun
        var sunHatX = sunX / earthSunDist;
        var sunHatY = sunY / earthSunDist;
        var sunHatZ = sunZ / earthSunDist;

        // Project spacecraft onto Sun-Earth line
        var scDotSun = state.X * sunHatX + state.Y * sunHatY + state.Z * sunHatZ;

        // If spacecraft is on Sun side of Earth, no eclipse
        if (scDotSun > 0)
        {
            return (EclipseType.None, 1.0);
        }

        // Perpendicular distance from Sun-Earth line
        var perpX = state.X - scDotSun * sunHatX;
        var perpY = state.Y - scDotSun * sunHatY;
        var perpZ = state.Z - scDotSun * sunHatZ;
        var perpDist = Math.Sqrt(perpX * perpX + perpY * perpY + perpZ * perpZ);

        if (perpDist < SrpConstants.EarthRadius)
        {
            return (EclipseType.Umbra, 0.0);
        }

        return (EclipseType.None, 1.0);
    }
}
