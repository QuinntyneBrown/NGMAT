using Shared.Domain.Units;

namespace Shared.Domain.Tests.Units;

public class Vector3DTests
{
    private const double Epsilon = 1e-10;

    [Fact]
    public void Constructor_ShouldCreateVectorWithComponents()
    {
        // Act
        var vector = new Vector3D(1.0, 2.0, 3.0);

        // Assert
        Assert.Equal(1.0, vector.X);
        Assert.Equal(2.0, vector.Y);
        Assert.Equal(3.0, vector.Z);
    }

    [Fact]
    public void Zero_ShouldReturnZeroVector()
    {
        // Act
        var zero = Vector3D.Zero;

        // Assert
        Assert.Equal(0.0, zero.X);
        Assert.Equal(0.0, zero.Y);
        Assert.Equal(0.0, zero.Z);
    }

    [Fact]
    public void UnitVectors_ShouldReturnCorrectValues()
    {
        // Act
        var unitX = Vector3D.UnitX;
        var unitY = Vector3D.UnitY;
        var unitZ = Vector3D.UnitZ;

        // Assert
        Assert.Equal(new Vector3D(1, 0, 0), unitX);
        Assert.Equal(new Vector3D(0, 1, 0), unitY);
        Assert.Equal(new Vector3D(0, 0, 1), unitZ);
    }

    [Fact]
    public void Magnitude_ShouldCalculateCorrectly()
    {
        // Arrange
        var vector = new Vector3D(3.0, 4.0, 0.0);

        // Act
        var magnitude = vector.Magnitude;

        // Assert
        Assert.Equal(5.0, magnitude, Epsilon);
    }

    [Fact]
    public void MagnitudeSquared_ShouldCalculateCorrectly()
    {
        // Arrange
        var vector = new Vector3D(3.0, 4.0, 0.0);

        // Act
        var magnitudeSquared = vector.MagnitudeSquared;

        // Assert
        Assert.Equal(25.0, magnitudeSquared, Epsilon);
    }

    [Fact]
    public void Normalize_ShouldReturnUnitVector()
    {
        // Arrange
        var vector = new Vector3D(3.0, 4.0, 0.0);

        // Act
        var normalized = vector.Normalize();

        // Assert
        Assert.Equal(1.0, normalized.Magnitude, Epsilon);
        Assert.Equal(0.6, normalized.X, Epsilon);
        Assert.Equal(0.8, normalized.Y, Epsilon);
        Assert.Equal(0.0, normalized.Z, Epsilon);
    }

    [Fact]
    public void Normalize_ZeroVector_ShouldReturnZero()
    {
        // Arrange
        var vector = Vector3D.Zero;

        // Act
        var normalized = vector.Normalize();

        // Assert
        Assert.Equal(Vector3D.Zero, normalized);
    }

    [Fact]
    public void Dot_ShouldCalculateCorrectly()
    {
        // Arrange
        var v1 = new Vector3D(1.0, 2.0, 3.0);
        var v2 = new Vector3D(4.0, 5.0, 6.0);

        // Act
        var dot = v1.Dot(v2);

        // Assert
        Assert.Equal(32.0, dot, Epsilon); // 1*4 + 2*5 + 3*6 = 4 + 10 + 18 = 32
    }

    [Fact]
    public void Cross_ShouldCalculateCorrectly()
    {
        // Arrange
        var v1 = new Vector3D(1.0, 0.0, 0.0);
        var v2 = new Vector3D(0.0, 1.0, 0.0);

        // Act
        var cross = v1.Cross(v2);

        // Assert
        Assert.Equal(0.0, cross.X, Epsilon);
        Assert.Equal(0.0, cross.Y, Epsilon);
        Assert.Equal(1.0, cross.Z, Epsilon);
    }

    [Fact]
    public void DistanceTo_ShouldCalculateCorrectly()
    {
        // Arrange
        var v1 = new Vector3D(0.0, 0.0, 0.0);
        var v2 = new Vector3D(3.0, 4.0, 0.0);

        // Act
        var distance = v1.DistanceTo(v2);

        // Assert
        Assert.Equal(5.0, distance, Epsilon);
    }

    [Fact]
    public void AngleTo_ShouldCalculateCorrectly()
    {
        // Arrange
        var v1 = new Vector3D(1.0, 0.0, 0.0);
        var v2 = new Vector3D(0.0, 1.0, 0.0);

        // Act
        var angle = v1.AngleTo(v2);

        // Assert
        Assert.Equal(Math.PI / 2, angle, Epsilon); // 90 degrees
    }

    [Fact]
    public void AngleTo_WithZeroVector_ShouldReturnZero()
    {
        // Arrange
        var v1 = new Vector3D(1.0, 0.0, 0.0);
        var v2 = Vector3D.Zero;

        // Act
        var angle = v1.AngleTo(v2);

        // Assert
        Assert.Equal(0.0, angle, Epsilon);
    }

    [Fact]
    public void Addition_ShouldWorkCorrectly()
    {
        // Arrange
        var v1 = new Vector3D(1.0, 2.0, 3.0);
        var v2 = new Vector3D(4.0, 5.0, 6.0);

        // Act
        var result = v1 + v2;

        // Assert
        Assert.Equal(5.0, result.X);
        Assert.Equal(7.0, result.Y);
        Assert.Equal(9.0, result.Z);
    }

    [Fact]
    public void Subtraction_ShouldWorkCorrectly()
    {
        // Arrange
        var v1 = new Vector3D(4.0, 5.0, 6.0);
        var v2 = new Vector3D(1.0, 2.0, 3.0);

        // Act
        var result = v1 - v2;

        // Assert
        Assert.Equal(3.0, result.X);
        Assert.Equal(3.0, result.Y);
        Assert.Equal(3.0, result.Z);
    }

    [Fact]
    public void Negation_ShouldWorkCorrectly()
    {
        // Arrange
        var vector = new Vector3D(1.0, 2.0, 3.0);

        // Act
        var result = -vector;

        // Assert
        Assert.Equal(-1.0, result.X);
        Assert.Equal(-2.0, result.Y);
        Assert.Equal(-3.0, result.Z);
    }

    [Fact]
    public void Multiplication_ByScalar_ShouldWorkCorrectly()
    {
        // Arrange
        var vector = new Vector3D(1.0, 2.0, 3.0);

        // Act
        var result = vector * 2.0;

        // Assert
        Assert.Equal(2.0, result.X);
        Assert.Equal(4.0, result.Y);
        Assert.Equal(6.0, result.Z);
    }

    [Fact]
    public void Multiplication_ScalarByVector_ShouldWorkCorrectly()
    {
        // Arrange
        var vector = new Vector3D(1.0, 2.0, 3.0);

        // Act
        var result = 2.0 * vector;

        // Assert
        Assert.Equal(2.0, result.X);
        Assert.Equal(4.0, result.Y);
        Assert.Equal(6.0, result.Z);
    }

    [Fact]
    public void Division_ByScalar_ShouldWorkCorrectly()
    {
        // Arrange
        var vector = new Vector3D(2.0, 4.0, 6.0);

        // Act
        var result = vector / 2.0;

        // Assert
        Assert.Equal(1.0, result.X);
        Assert.Equal(2.0, result.Y);
        Assert.Equal(3.0, result.Z);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var v1 = new Vector3D(1.0, 2.0, 3.0);
        var v2 = new Vector3D(1.0, 2.0, 3.0);

        // Act & Assert
        Assert.True(v1.Equals(v2));
        Assert.True(v1 == v2);
        Assert.False(v1 != v2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var v1 = new Vector3D(1.0, 2.0, 3.0);
        var v2 = new Vector3D(4.0, 5.0, 6.0);

        // Act & Assert
        Assert.False(v1.Equals(v2));
        Assert.False(v1 == v2);
        Assert.True(v1 != v2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var v1 = new Vector3D(1.0, 2.0, 3.0);
        var v2 = new Vector3D(1.0, 2.0, 3.0);

        // Act & Assert
        Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var vector = new Vector3D(1.0, 2.0, 3.0);

        // Act
        var result = vector.ToString();

        // Assert
        Assert.Contains("1", result);
        Assert.Contains("2", result);
        Assert.Contains("3", result);
    }

    [Fact]
    public void ToArray_ShouldReturnCorrectArray()
    {
        // Arrange
        var vector = new Vector3D(1.0, 2.0, 3.0);

        // Act
        var array = vector.ToArray();

        // Assert
        Assert.Equal(3, array.Length);
        Assert.Equal(1.0, array[0]);
        Assert.Equal(2.0, array[1]);
        Assert.Equal(3.0, array[2]);
    }

    [Fact]
    public void FromArray_ShouldCreateVector()
    {
        // Arrange
        var array = new[] { 1.0, 2.0, 3.0 };

        // Act
        var vector = Vector3D.FromArray(array);

        // Assert
        Assert.Equal(1.0, vector.X);
        Assert.Equal(2.0, vector.Y);
        Assert.Equal(3.0, vector.Z);
    }

    [Fact]
    public void FromArray_WithInvalidLength_ShouldThrowException()
    {
        // Arrange
        var array = new[] { 1.0, 2.0 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Vector3D.FromArray(array));
    }
}
