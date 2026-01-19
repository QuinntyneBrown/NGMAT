namespace CoordinateSystem.Core.Entities;

/// <summary>
/// Represents a position and velocity state in a reference frame.
/// </summary>
public readonly struct StateVector : IEquatable<StateVector>
{
    /// <summary>X position component in km.</summary>
    public double X { get; init; }

    /// <summary>Y position component in km.</summary>
    public double Y { get; init; }

    /// <summary>Z position component in km.</summary>
    public double Z { get; init; }

    /// <summary>X velocity component in km/s.</summary>
    public double Vx { get; init; }

    /// <summary>Y velocity component in km/s.</summary>
    public double Vy { get; init; }

    /// <summary>Z velocity component in km/s.</summary>
    public double Vz { get; init; }

    /// <summary>Position vector in km.</summary>
    public Vector3 Position => new(X, Y, Z);

    /// <summary>Velocity vector in km/s.</summary>
    public Vector3 Velocity => new(Vx, Vy, Vz);

    /// <summary>Position magnitude in km.</summary>
    public double PositionMagnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>Velocity magnitude in km/s.</summary>
    public double VelocityMagnitude => Math.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);

    public StateVector(double x, double y, double z, double vx, double vy, double vz)
    {
        X = x;
        Y = y;
        Z = z;
        Vx = vx;
        Vy = vy;
        Vz = vz;
    }

    public StateVector(Vector3 position, Vector3 velocity)
    {
        X = position.X;
        Y = position.Y;
        Z = position.Z;
        Vx = velocity.X;
        Vy = velocity.Y;
        Vz = velocity.Z;
    }

    public bool Equals(StateVector other) =>
        X == other.X && Y == other.Y && Z == other.Z &&
        Vx == other.Vx && Vy == other.Vy && Vz == other.Vz;

    public override bool Equals(object? obj) => obj is StateVector other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Z, Vx, Vy, Vz);

    public static bool operator ==(StateVector left, StateVector right) => left.Equals(right);
    public static bool operator !=(StateVector left, StateVector right) => !left.Equals(right);

    public override string ToString() =>
        $"Position: [{X:F3}, {Y:F3}, {Z:F3}] km, Velocity: [{Vx:F6}, {Vy:F6}, {Vz:F6}] km/s";
}

/// <summary>
/// Simple 3D vector for position/velocity components.
/// </summary>
public readonly struct Vector3 : IEquatable<Vector3>
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }

    public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3 Normalize()
    {
        var mag = Magnitude;
        return mag > 0 ? new Vector3(X / mag, Y / mag, Z / mag) : this;
    }

    public static Vector3 Cross(Vector3 a, Vector3 b) =>
        new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    public static double Dot(Vector3 a, Vector3 b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 operator +(Vector3 a, Vector3 b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3 operator -(Vector3 a, Vector3 b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3 operator *(Vector3 v, double scalar) =>
        new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3 operator *(double scalar, Vector3 v) =>
        new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3 operator /(Vector3 v, double scalar) =>
        new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public static Vector3 operator -(Vector3 v) =>
        new(-v.X, -v.Y, -v.Z);

    public bool Equals(Vector3 other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is Vector3 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
    public static bool operator !=(Vector3 left, Vector3 right) => !left.Equals(right);

    public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";
}
