namespace CoordinateSystem.Core.Entities;

/// <summary>
/// 3x3 rotation matrix for coordinate transformations.
/// </summary>
public readonly struct TransformationMatrix : IEquatable<TransformationMatrix>
{
    private readonly double[] _elements;

    /// <summary>
    /// Matrix elements in row-major order.
    /// </summary>
    public ReadOnlySpan<double> Elements => _elements.AsSpan();

    public double M11 => _elements[0];
    public double M12 => _elements[1];
    public double M13 => _elements[2];
    public double M21 => _elements[3];
    public double M22 => _elements[4];
    public double M23 => _elements[5];
    public double M31 => _elements[6];
    public double M32 => _elements[7];
    public double M33 => _elements[8];

    public TransformationMatrix(
        double m11, double m12, double m13,
        double m21, double m22, double m23,
        double m31, double m32, double m33)
    {
        _elements = new double[] { m11, m12, m13, m21, m22, m23, m31, m32, m33 };
    }

    private TransformationMatrix(double[] elements)
    {
        _elements = elements;
    }

    /// <summary>
    /// Identity matrix.
    /// </summary>
    public static TransformationMatrix Identity => new(1, 0, 0, 0, 1, 0, 0, 0, 1);

    /// <summary>
    /// Rotation about X-axis by angle in radians.
    /// </summary>
    public static TransformationMatrix RotationX(double angle)
    {
        var c = Math.Cos(angle);
        var s = Math.Sin(angle);
        return new TransformationMatrix(
            1, 0, 0,
            0, c, s,
            0, -s, c);
    }

    /// <summary>
    /// Rotation about Y-axis by angle in radians.
    /// </summary>
    public static TransformationMatrix RotationY(double angle)
    {
        var c = Math.Cos(angle);
        var s = Math.Sin(angle);
        return new TransformationMatrix(
            c, 0, -s,
            0, 1, 0,
            s, 0, c);
    }

    /// <summary>
    /// Rotation about Z-axis by angle in radians.
    /// </summary>
    public static TransformationMatrix RotationZ(double angle)
    {
        var c = Math.Cos(angle);
        var s = Math.Sin(angle);
        return new TransformationMatrix(
            c, s, 0,
            -s, c, 0,
            0, 0, 1);
    }

    /// <summary>
    /// Transform a vector using this matrix.
    /// </summary>
    public Vector3 Transform(Vector3 v) =>
        new(
            M11 * v.X + M12 * v.Y + M13 * v.Z,
            M21 * v.X + M22 * v.Y + M23 * v.Z,
            M31 * v.X + M32 * v.Y + M33 * v.Z);

    /// <summary>
    /// Transform a state vector (both position and velocity).
    /// </summary>
    public StateVector Transform(StateVector state) =>
        new(Transform(state.Position), Transform(state.Velocity));

    /// <summary>
    /// Matrix transpose.
    /// </summary>
    public TransformationMatrix Transpose() =>
        new(
            M11, M21, M31,
            M12, M22, M32,
            M13, M23, M33);

    /// <summary>
    /// For orthogonal rotation matrices, the inverse equals the transpose.
    /// </summary>
    public TransformationMatrix Inverse() => Transpose();

    /// <summary>
    /// Matrix multiplication.
    /// </summary>
    public static TransformationMatrix operator *(TransformationMatrix a, TransformationMatrix b)
    {
        return new TransformationMatrix(
            a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
            a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
            a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
            a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
            a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
            a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
            a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
            a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
            a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33);
    }

    public bool Equals(TransformationMatrix other)
    {
        for (int i = 0; i < 9; i++)
        {
            if (_elements[i] != other._elements[i])
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is TransformationMatrix other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var e in _elements)
            hash.Add(e);
        return hash.ToHashCode();
    }

    public static bool operator ==(TransformationMatrix left, TransformationMatrix right) => left.Equals(right);
    public static bool operator !=(TransformationMatrix left, TransformationMatrix right) => !left.Equals(right);

    public override string ToString() =>
        $"[{M11:F6}, {M12:F6}, {M13:F6}]\n[{M21:F6}, {M22:F6}, {M23:F6}]\n[{M31:F6}, {M32:F6}, {M33:F6}]";
}
