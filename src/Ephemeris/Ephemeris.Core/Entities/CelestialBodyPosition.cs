namespace Ephemeris.Core.Entities;

public sealed class CelestialBodyPosition
{
    public Guid Id { get; private set; }
    public Guid CelestialBodyId { get; private set; }
    public CelestialBody? CelestialBody { get; private set; }
    public DateTime Epoch { get; private set; }

    // Position in meters (ICRF/J2000 heliocentric or geocentric depending on context)
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Z { get; private set; }

    // Velocity in meters/second
    public double Vx { get; private set; }
    public double Vy { get; private set; }
    public double Vz { get; private set; }

    // Optional acceleration in meters/second^2
    public double? Ax { get; private set; }
    public double? Ay { get; private set; }
    public double? Az { get; private set; }

    // Reference frame identifier
    public string ReferenceFrame { get; private set; } = "ICRF";

    // Center body (e.g., Sun for heliocentric, Earth for geocentric)
    public int CenterNaifId { get; private set; }

    // Source of ephemeris data (e.g., "DE440", "DE441", "JPL_HORIZONS")
    public string Source { get; private set; } = string.Empty;

    public DateTime RecordedAt { get; private set; }

    private CelestialBodyPosition() { }

    public static CelestialBodyPosition Create(
        Guid celestialBodyId,
        DateTime epoch,
        double x, double y, double z,
        double vx, double vy, double vz,
        int centerNaifId,
        string source,
        string referenceFrame = "ICRF",
        double? ax = null, double? ay = null, double? az = null)
    {
        return new CelestialBodyPosition
        {
            Id = Guid.NewGuid(),
            CelestialBodyId = celestialBodyId,
            Epoch = epoch,
            X = x,
            Y = y,
            Z = z,
            Vx = vx,
            Vy = vy,
            Vz = vz,
            Ax = ax,
            Ay = ay,
            Az = az,
            CenterNaifId = centerNaifId,
            Source = source,
            ReferenceFrame = referenceFrame,
            RecordedAt = DateTime.UtcNow
        };
    }

    public Vector3 Position => new(X, Y, Z);
    public Vector3 Velocity => new(Vx, Vy, Vz);
    public Vector3? Acceleration => Ax.HasValue && Ay.HasValue && Az.HasValue
        ? new Vector3(Ax.Value, Ay.Value, Az.Value)
        : null;

    public double DistanceFromCenter => Math.Sqrt(X * X + Y * Y + Z * Z);
    public double Speed => Math.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);
}

public readonly struct Vector3
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector3 Normalize()
    {
        var mag = Magnitude;
        return mag > 0 ? new Vector3(X / mag, Y / mag, Z / mag) : this;
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(Vector3 v, double s) => new(v.X * s, v.Y * s, v.Z * s);
    public static Vector3 operator *(double s, Vector3 v) => v * s;
    public static Vector3 operator /(Vector3 v, double s) => new(v.X / s, v.Y / s, v.Z / s);

    public static double Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Cross(Vector3 a, Vector3 b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public override string ToString() => $"({X:E6}, {Y:E6}, {Z:E6})";
}
