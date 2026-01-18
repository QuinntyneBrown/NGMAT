using MessagePack;

namespace Shared.Domain.Units;

/// <summary>
/// Represents a quaternion for 3D rotations.
/// </summary>
[MessagePackObject]
public readonly struct Quaternion : IEquatable<Quaternion>
{
    [Key(0)]
    public double W { get; }

    [Key(1)]
    public double X { get; }

    [Key(2)]
    public double Y { get; }

    [Key(3)]
    public double Z { get; }

    [IgnoreMember]
    public double Magnitude => Math.Sqrt(W * W + X * X + Y * Y + Z * Z);

    [IgnoreMember]
    public bool IsNormalized => Math.Abs(Magnitude - 1.0) < 1e-10;

    [SerializationConstructor]
    public Quaternion(double w, double x, double y, double z)
    {
        W = w;
        X = x;
        Y = y;
        Z = z;
    }

    public static Quaternion Identity => new(1, 0, 0, 0);

    public Quaternion Normalize()
    {
        var mag = Magnitude;
        return mag > 0 ? new Quaternion(W / mag, X / mag, Y / mag, Z / mag) : Identity;
    }

    public Quaternion Conjugate() => new(W, -X, -Y, -Z);

    public Quaternion Inverse()
    {
        var magSquared = W * W + X * X + Y * Y + Z * Z;
        if (magSquared == 0)
        {
            return Identity;
        }
        return new Quaternion(W / magSquared, -X / magSquared, -Y / magSquared, -Z / magSquared);
    }

    public static Quaternion operator *(Quaternion a, Quaternion b) =>
        new(
            a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
            a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
            a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
            a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W
        );

    public Vector3D Rotate(Vector3D v)
    {
        var qv = new Quaternion(0, v.X, v.Y, v.Z);
        var result = this * qv * Conjugate();
        return new Vector3D(result.X, result.Y, result.Z);
    }

    public static Quaternion FromAxisAngle(Vector3D axis, double angle)
    {
        var normalizedAxis = axis.Normalize();
        var halfAngle = angle / 2.0;
        var sinHalf = Math.Sin(halfAngle);
        var cosHalf = Math.Cos(halfAngle);

        return new Quaternion(
            cosHalf,
            normalizedAxis.X * sinHalf,
            normalizedAxis.Y * sinHalf,
            normalizedAxis.Z * sinHalf
        );
    }

    public (Vector3D Axis, double Angle) ToAxisAngle()
    {
        var normalized = Normalize();
        var angle = 2.0 * Math.Acos(Math.Clamp(normalized.W, -1.0, 1.0));

        var sinHalf = Math.Sin(angle / 2.0);
        if (Math.Abs(sinHalf) < 1e-10)
        {
            return (Vector3D.UnitZ, 0);
        }

        var axis = new Vector3D(
            normalized.X / sinHalf,
            normalized.Y / sinHalf,
            normalized.Z / sinHalf
        );

        return (axis, angle);
    }

    public static Quaternion FromEulerAngles(double roll, double pitch, double yaw)
    {
        var cy = Math.Cos(yaw * 0.5);
        var sy = Math.Sin(yaw * 0.5);
        var cp = Math.Cos(pitch * 0.5);
        var sp = Math.Sin(pitch * 0.5);
        var cr = Math.Cos(roll * 0.5);
        var sr = Math.Sin(roll * 0.5);

        return new Quaternion(
            cr * cp * cy + sr * sp * sy,
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy
        );
    }

    public (double Roll, double Pitch, double Yaw) ToEulerAngles()
    {
        var sinrCosp = 2.0 * (W * X + Y * Z);
        var cosrCosp = 1.0 - 2.0 * (X * X + Y * Y);
        var roll = Math.Atan2(sinrCosp, cosrCosp);

        var sinp = 2.0 * (W * Y - Z * X);
        var pitch = Math.Abs(sinp) >= 1 ? Math.CopySign(Math.PI / 2, sinp) : Math.Asin(sinp);

        var sinyCosp = 2.0 * (W * Z + X * Y);
        var cosyCosp = 1.0 - 2.0 * (Y * Y + Z * Z);
        var yaw = Math.Atan2(sinyCosp, cosyCosp);

        return (roll, pitch, yaw);
    }

    public static Quaternion Slerp(Quaternion a, Quaternion b, double t)
    {
        var dot = a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        if (dot < 0)
        {
            b = new Quaternion(-b.W, -b.X, -b.Y, -b.Z);
            dot = -dot;
        }

        if (dot > 0.9995)
        {
            var result = new Quaternion(
                a.W + t * (b.W - a.W),
                a.X + t * (b.X - a.X),
                a.Y + t * (b.Y - a.Y),
                a.Z + t * (b.Z - a.Z)
            );
            return result.Normalize();
        }

        var theta0 = Math.Acos(dot);
        var theta = theta0 * t;
        var sinTheta = Math.Sin(theta);
        var sinTheta0 = Math.Sin(theta0);

        var s0 = Math.Cos(theta) - dot * sinTheta / sinTheta0;
        var s1 = sinTheta / sinTheta0;

        return new Quaternion(
            s0 * a.W + s1 * b.W,
            s0 * a.X + s1 * b.X,
            s0 * a.Y + s1 * b.Y,
            s0 * a.Z + s1 * b.Z
        );
    }

    public bool Equals(Quaternion other) =>
        W.Equals(other.W) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    public override bool Equals(object? obj) => obj is Quaternion other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(W, X, Y, Z);

    public static bool operator ==(Quaternion left, Quaternion right) => left.Equals(right);

    public static bool operator !=(Quaternion left, Quaternion right) => !left.Equals(right);

    public override string ToString() => $"({W:G6}, {X:G6}, {Y:G6}, {Z:G6})";
}
