using System.Numerics;
using MessagePack;

namespace Shared.Domain.Units;

/// <summary>
/// Represents a 3D vector with X, Y, Z components.
/// </summary>
[MessagePackObject]
public readonly struct Vector3D : IEquatable<Vector3D>
{
    [Key(0)]
    public double X { get; }

    [Key(1)]
    public double Y { get; }

    [Key(2)]
    public double Z { get; }

    [IgnoreMember]
    public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

    [IgnoreMember]
    public double MagnitudeSquared => X * X + Y * Y + Z * Z;

    [SerializationConstructor]
    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);

    public Vector3D Normalize()
    {
        var mag = Magnitude;
        return mag > 0 ? new Vector3D(X / mag, Y / mag, Z / mag) : Zero;
    }

    public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

    public Vector3D Cross(Vector3D other) =>
        new(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );

    public double DistanceTo(Vector3D other) => (this - other).Magnitude;

    public double AngleTo(Vector3D other)
    {
        var dot = Dot(other);
        var mag1 = Magnitude;
        var mag2 = other.Magnitude;

        if (mag1 == 0 || mag2 == 0)
        {
            return 0;
        }

        var cosAngle = Math.Clamp(dot / (mag1 * mag2), -1.0, 1.0);
        return Math.Acos(cosAngle);
    }

    public static Vector3D operator +(Vector3D a, Vector3D b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3D operator -(Vector3D a, Vector3D b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3D operator -(Vector3D v) =>
        new(-v.X, -v.Y, -v.Z);

    public static Vector3D operator *(Vector3D v, double scalar) =>
        new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3D operator *(double scalar, Vector3D v) =>
        new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3D operator /(Vector3D v, double scalar) =>
        new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public bool Equals(Vector3D other) =>
        X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    public override bool Equals(object? obj) => obj is Vector3D other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public static bool operator ==(Vector3D left, Vector3D right) => left.Equals(right);

    public static bool operator !=(Vector3D left, Vector3D right) => !left.Equals(right);

    public override string ToString() => $"({X:G6}, {Y:G6}, {Z:G6})";

    public double[] ToArray() => [X, Y, Z];

    public static Vector3D FromArray(double[] array)
    {
        if (array.Length != 3)
        {
            throw new ArgumentException("Array must have exactly 3 elements", nameof(array));
        }
        return new Vector3D(array[0], array[1], array[2]);
    }

    public static implicit operator Vector3(Vector3D v) =>
        new((float)v.X, (float)v.Y, (float)v.Z);

    public static implicit operator Vector3D(Vector3 v) =>
        new(v.X, v.Y, v.Z);
}
