namespace Maneuver.Core.Entities;

/// <summary>
/// Hohmann transfer orbit calculations between circular orbits
/// </summary>
public sealed class HohmannTransfer
{
    private const double EarthGM = 3.986004418e14; // m^3/s^2

    public double InitialRadiusM { get; }
    public double FinalRadiusM { get; }
    public double DeltaV1Mps { get; }
    public double DeltaV2Mps { get; }
    public double TotalDeltaVMps { get; }
    public double TransferTimeSeconds { get; }
    public double SemiMajorAxisM { get; }
    public double TransferPeriodSeconds { get; }

    public HohmannTransfer(double initialRadiusM, double finalRadiusM, double centralBodyGM = EarthGM)
    {
        InitialRadiusM = initialRadiusM;
        FinalRadiusM = finalRadiusM;

        // Transfer orbit semi-major axis
        SemiMajorAxisM = (initialRadiusM + finalRadiusM) / 2.0;

        // Velocities
        var v1Circular = Math.Sqrt(centralBodyGM / initialRadiusM);
        var v2Circular = Math.Sqrt(centralBodyGM / finalRadiusM);

        // Transfer orbit velocities at periapsis and apoapsis
        var vTransferPeriapsis = Math.Sqrt(centralBodyGM * (2.0 / initialRadiusM - 1.0 / SemiMajorAxisM));
        var vTransferApoapsis = Math.Sqrt(centralBodyGM * (2.0 / finalRadiusM - 1.0 / SemiMajorAxisM));

        // Delta-V calculations (accounts for direction)
        if (finalRadiusM > initialRadiusM)
        {
            // Raise orbit
            DeltaV1Mps = vTransferPeriapsis - v1Circular;
            DeltaV2Mps = v2Circular - vTransferApoapsis;
        }
        else
        {
            // Lower orbit
            DeltaV1Mps = v1Circular - vTransferPeriapsis;
            DeltaV2Mps = vTransferApoapsis - v2Circular;
        }

        TotalDeltaVMps = Math.Abs(DeltaV1Mps) + Math.Abs(DeltaV2Mps);

        // Transfer time is half the transfer orbit period
        TransferPeriodSeconds = 2.0 * Math.PI * Math.Sqrt(Math.Pow(SemiMajorAxisM, 3) / centralBodyGM);
        TransferTimeSeconds = TransferPeriodSeconds / 2.0;
    }

    public static HohmannTransfer Calculate(double initialRadiusM, double finalRadiusM, double centralBodyGM = EarthGM)
    {
        return new HohmannTransfer(initialRadiusM, finalRadiusM, centralBodyGM);
    }
}

/// <summary>
/// Bi-elliptic transfer orbit calculations (three-impulse transfer)
/// More efficient than Hohmann for large radius ratios (> 11.94)
/// </summary>
public sealed class BiEllipticTransfer
{
    private const double EarthGM = 3.986004418e14; // m^3/s^2

    public double InitialRadiusM { get; }
    public double FinalRadiusM { get; }
    public double IntermediateRadiusM { get; }
    public double DeltaV1Mps { get; }
    public double DeltaV2Mps { get; }
    public double DeltaV3Mps { get; }
    public double TotalDeltaVMps { get; }
    public double TransferTimeSeconds { get; }
    public double HohmannEquivalentDeltaVMps { get; }
    public bool IsMoreEfficientThanHohmann { get; }

    public BiEllipticTransfer(double initialRadiusM, double finalRadiusM, double intermediateRadiusM, double centralBodyGM = EarthGM)
    {
        if (intermediateRadiusM < Math.Max(initialRadiusM, finalRadiusM))
            throw new ArgumentException("Intermediate radius must be larger than both initial and final radii");

        InitialRadiusM = initialRadiusM;
        FinalRadiusM = finalRadiusM;
        IntermediateRadiusM = intermediateRadiusM;

        // First ellipse: initial to intermediate
        var a1 = (initialRadiusM + intermediateRadiusM) / 2.0;
        // Second ellipse: intermediate to final
        var a2 = (finalRadiusM + intermediateRadiusM) / 2.0;

        // Circular velocities
        var v1Circular = Math.Sqrt(centralBodyGM / initialRadiusM);
        var v2Circular = Math.Sqrt(centralBodyGM / finalRadiusM);

        // First transfer orbit velocities
        var v1TransferPeri = Math.Sqrt(centralBodyGM * (2.0 / initialRadiusM - 1.0 / a1));
        var v1TransferApo = Math.Sqrt(centralBodyGM * (2.0 / intermediateRadiusM - 1.0 / a1));

        // Second transfer orbit velocities
        var v2TransferApo = Math.Sqrt(centralBodyGM * (2.0 / intermediateRadiusM - 1.0 / a2));
        var v2TransferPeri = Math.Sqrt(centralBodyGM * (2.0 / finalRadiusM - 1.0 / a2));

        // Delta-V calculations
        DeltaV1Mps = v1TransferPeri - v1Circular;
        DeltaV2Mps = v2TransferApo - v1TransferApo;
        DeltaV3Mps = v2Circular - v2TransferPeri;

        TotalDeltaVMps = Math.Abs(DeltaV1Mps) + Math.Abs(DeltaV2Mps) + Math.Abs(DeltaV3Mps);

        // Transfer times
        var t1 = Math.PI * Math.Sqrt(Math.Pow(a1, 3) / centralBodyGM);
        var t2 = Math.PI * Math.Sqrt(Math.Pow(a2, 3) / centralBodyGM);
        TransferTimeSeconds = t1 + t2;

        // Compare to Hohmann
        var hohmann = new HohmannTransfer(initialRadiusM, finalRadiusM, centralBodyGM);
        HohmannEquivalentDeltaVMps = hohmann.TotalDeltaVMps;
        IsMoreEfficientThanHohmann = TotalDeltaVMps < HohmannEquivalentDeltaVMps;
    }

    public static BiEllipticTransfer Calculate(double initialRadiusM, double finalRadiusM, double intermediateRadiusM, double centralBodyGM = EarthGM)
    {
        return new BiEllipticTransfer(initialRadiusM, finalRadiusM, intermediateRadiusM, centralBodyGM);
    }

    /// <summary>
    /// Find the optimal intermediate radius for bi-elliptic transfer
    /// </summary>
    public static double FindOptimalIntermediateRadius(double initialRadiusM, double finalRadiusM)
    {
        // For bi-elliptic to be more efficient than Hohmann, radius ratio should be > 11.94
        // Optimal intermediate radius is typically a few times the larger of the two radii
        var largerRadius = Math.Max(initialRadiusM, finalRadiusM);
        return largerRadius * 3.0; // Rule of thumb starting point
    }
}

/// <summary>
/// Plane change maneuver calculations
/// </summary>
public sealed class PlaneChangeManeuver
{
    private const double EarthGM = 3.986004418e14; // m^3/s^2

    public double OrbitalVelocityMps { get; }
    public double InclinationChangeDeg { get; }
    public double InclinationChangeRad { get; }
    public double DeltaVMps { get; }
    public bool BurnAtAscendingNode { get; }

    public PlaneChangeManeuver(double orbitRadiusM, double inclinationChangeDeg, bool burnAtAscendingNode = true, double centralBodyGM = EarthGM)
    {
        InclinationChangeDeg = inclinationChangeDeg;
        InclinationChangeRad = inclinationChangeDeg * Math.PI / 180.0;
        BurnAtAscendingNode = burnAtAscendingNode;

        OrbitalVelocityMps = Math.Sqrt(centralBodyGM / orbitRadiusM);

        // Delta-V for pure plane change: 2 * v * sin(Δi/2)
        DeltaVMps = 2.0 * OrbitalVelocityMps * Math.Sin(InclinationChangeRad / 2.0);
    }

    public static PlaneChangeManeuver Calculate(double orbitRadiusM, double inclinationChangeDeg, bool burnAtAscendingNode = true, double centralBodyGM = EarthGM)
    {
        return new PlaneChangeManeuver(orbitRadiusM, inclinationChangeDeg, burnAtAscendingNode, centralBodyGM);
    }

    /// <summary>
    /// Combined plane change with altitude change (more efficient than separate maneuvers)
    /// </summary>
    public static double CalculateCombinedDeltaV(double initialRadiusM, double finalRadiusM, double inclinationChangeDeg, double centralBodyGM = EarthGM)
    {
        var inclinationChangeRad = inclinationChangeDeg * Math.PI / 180.0;

        var v1 = Math.Sqrt(centralBodyGM / initialRadiusM);
        var v2 = Math.Sqrt(centralBodyGM / finalRadiusM);

        // Combined maneuver using cosine rule
        var deltaV = Math.Sqrt(v1 * v1 + v2 * v2 - 2.0 * v1 * v2 * Math.Cos(inclinationChangeRad));

        return deltaV;
    }
}

/// <summary>
/// Result of transfer orbit calculations
/// </summary>
public sealed class TransferOrbitResult
{
    public Guid Id { get; }
    public string TransferType { get; }
    public double InitialRadiusM { get; }
    public double FinalRadiusM { get; }
    public double? IntermediateRadiusM { get; }
    public List<TransferBurn> Burns { get; }
    public double TotalDeltaVMps { get; }
    public double TransferTimeSeconds { get; }
    public DateTime CalculatedAt { get; }

    public TransferOrbitResult(
        string transferType,
        double initialRadiusM,
        double finalRadiusM,
        List<TransferBurn> burns,
        double totalDeltaVMps,
        double transferTimeSeconds,
        double? intermediateRadiusM = null)
    {
        Id = Guid.NewGuid();
        TransferType = transferType;
        InitialRadiusM = initialRadiusM;
        FinalRadiusM = finalRadiusM;
        IntermediateRadiusM = intermediateRadiusM;
        Burns = burns;
        TotalDeltaVMps = totalDeltaVMps;
        TransferTimeSeconds = transferTimeSeconds;
        CalculatedAt = DateTime.UtcNow;
    }
}

public sealed class TransferBurn
{
    public int BurnNumber { get; }
    public string Location { get; }
    public double DeltaVMps { get; }
    public double TimeFromStartSeconds { get; }

    public TransferBurn(int burnNumber, string location, double deltaVMps, double timeFromStartSeconds)
    {
        BurnNumber = burnNumber;
        Location = location;
        DeltaVMps = deltaVMps;
        TimeFromStartSeconds = timeFromStartSeconds;
    }
}
