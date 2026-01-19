namespace CoordinateSystem.Core.Entities;

/// <summary>
/// Keplerian orbital elements.
/// </summary>
public readonly struct KeplerianElements : IEquatable<KeplerianElements>
{
    /// <summary>Semi-major axis in km.</summary>
    public double SemiMajorAxis { get; init; }

    /// <summary>Eccentricity (dimensionless, 0 = circular, 0-1 = ellipse, 1 = parabola, >1 = hyperbola).</summary>
    public double Eccentricity { get; init; }

    /// <summary>Inclination in radians.</summary>
    public double Inclination { get; init; }

    /// <summary>Right ascension of the ascending node (RAAN) in radians.</summary>
    public double RAAN { get; init; }

    /// <summary>Argument of periapsis in radians.</summary>
    public double ArgumentOfPeriapsis { get; init; }

    /// <summary>True anomaly in radians.</summary>
    public double TrueAnomaly { get; init; }

    /// <summary>Gravitational parameter (mu) in km³/s².</summary>
    public double Mu { get; init; }

    /// <summary>Orbital period in seconds (elliptical orbits only).</summary>
    public double Period => IsElliptical
        ? 2 * Math.PI * Math.Sqrt(Math.Pow(SemiMajorAxis, 3) / Mu)
        : double.PositiveInfinity;

    /// <summary>Semi-latus rectum in km.</summary>
    public double SemiLatusRectum => SemiMajorAxis * (1 - Eccentricity * Eccentricity);

    /// <summary>Apoapsis radius in km (elliptical orbits only).</summary>
    public double ApoapsisRadius => IsElliptical
        ? SemiMajorAxis * (1 + Eccentricity)
        : double.PositiveInfinity;

    /// <summary>Periapsis radius in km.</summary>
    public double PeriapsisRadius => SemiMajorAxis * (1 - Eccentricity);

    /// <summary>Specific orbital energy in km²/s².</summary>
    public double SpecificEnergy => -Mu / (2 * SemiMajorAxis);

    /// <summary>Mean motion in radians/second (elliptical orbits only).</summary>
    public double MeanMotion => IsElliptical
        ? Math.Sqrt(Mu / Math.Pow(SemiMajorAxis, 3))
        : 0;

    /// <summary>True if orbit is circular (e ≈ 0).</summary>
    public bool IsCircular => Math.Abs(Eccentricity) < 1e-10;

    /// <summary>True if orbit is elliptical (0 ≤ e < 1).</summary>
    public bool IsElliptical => Eccentricity >= 0 && Eccentricity < 1;

    /// <summary>True if orbit is parabolic (e = 1).</summary>
    public bool IsParabolic => Math.Abs(Eccentricity - 1) < 1e-10;

    /// <summary>True if orbit is hyperbolic (e > 1).</summary>
    public bool IsHyperbolic => Eccentricity > 1;

    /// <summary>True if orbit is equatorial (i ≈ 0 or i ≈ π).</summary>
    public bool IsEquatorial => Math.Abs(Inclination) < 1e-10 || Math.Abs(Inclination - Math.PI) < 1e-10;

    public KeplerianElements(
        double semiMajorAxis,
        double eccentricity,
        double inclination,
        double raan,
        double argumentOfPeriapsis,
        double trueAnomaly,
        double mu)
    {
        SemiMajorAxis = semiMajorAxis;
        Eccentricity = eccentricity;
        Inclination = inclination;
        RAAN = raan;
        ArgumentOfPeriapsis = argumentOfPeriapsis;
        TrueAnomaly = trueAnomaly;
        Mu = mu;
    }

    /// <summary>
    /// Compute eccentric anomaly from true anomaly.
    /// </summary>
    public double GetEccentricAnomaly()
    {
        if (IsElliptical)
        {
            var cosNu = Math.Cos(TrueAnomaly);
            var sinNu = Math.Sin(TrueAnomaly);
            var cosE = (Eccentricity + cosNu) / (1 + Eccentricity * cosNu);
            var sinE = Math.Sqrt(1 - Eccentricity * Eccentricity) * sinNu / (1 + Eccentricity * cosNu);
            return Math.Atan2(sinE, cosE);
        }
        else if (IsHyperbolic)
        {
            // Hyperbolic eccentric anomaly
            var cosNu = Math.Cos(TrueAnomaly);
            return Math.Acosh((Eccentricity + cosNu) / (1 + Eccentricity * cosNu));
        }
        return TrueAnomaly;
    }

    /// <summary>
    /// Compute mean anomaly from eccentric anomaly.
    /// </summary>
    public double GetMeanAnomaly()
    {
        var E = GetEccentricAnomaly();
        if (IsElliptical)
        {
            return E - Eccentricity * Math.Sin(E);
        }
        else if (IsHyperbolic)
        {
            return Eccentricity * Math.Sinh(E) - E;
        }
        return TrueAnomaly;
    }

    public bool Equals(KeplerianElements other) =>
        SemiMajorAxis == other.SemiMajorAxis &&
        Eccentricity == other.Eccentricity &&
        Inclination == other.Inclination &&
        RAAN == other.RAAN &&
        ArgumentOfPeriapsis == other.ArgumentOfPeriapsis &&
        TrueAnomaly == other.TrueAnomaly &&
        Mu == other.Mu;

    public override bool Equals(object? obj) => obj is KeplerianElements other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(SemiMajorAxis, Eccentricity, Inclination, RAAN, ArgumentOfPeriapsis, TrueAnomaly);

    public static bool operator ==(KeplerianElements left, KeplerianElements right) => left.Equals(right);
    public static bool operator !=(KeplerianElements left, KeplerianElements right) => !left.Equals(right);

    public override string ToString() =>
        $"a={SemiMajorAxis:F3} km, e={Eccentricity:F6}, i={Inclination * 180 / Math.PI:F4}°, " +
        $"Ω={RAAN * 180 / Math.PI:F4}°, ω={ArgumentOfPeriapsis * 180 / Math.PI:F4}°, ν={TrueAnomaly * 180 / Math.PI:F4}°";
}
