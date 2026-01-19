namespace Spacecraft.Core.Entities;

/// <summary>
/// Represents the state of a spacecraft at a specific epoch.
/// </summary>
public sealed class SpacecraftState
{
    public Guid Id { get; init; }
    public Guid SpacecraftId { get; init; }
    public DateTime Epoch { get; init; }

    // Position in km
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }

    // Velocity in km/s
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }

    // Mass at this epoch
    public double FuelMassKg { get; init; }

    // Reference frame
    public Guid CoordinateFrameId { get; init; }

    // Metadata
    public DateTime RecordedAt { get; init; }

    // Computed properties
    public double PositionMagnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
    public double VelocityMagnitude => Math.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);
    public double Altitude => PositionMagnitude - 6378.137; // WGS84 equatorial radius
}
