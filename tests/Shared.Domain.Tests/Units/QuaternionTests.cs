using Shared.Domain.Units;

namespace Shared.Domain.Tests.Units;

public class QuaternionTests
{
    private const double Epsilon = 1e-6;

    [Fact]
    public void Constructor_ShouldCreateQuaternionWithComponents()
    {
        // Act
        var quaternion = new Quaternion(1.0, 0.0, 0.0, 0.0);

        // Assert
        Assert.Equal(1.0, quaternion.W);
        Assert.Equal(0.0, quaternion.X);
        Assert.Equal(0.0, quaternion.Y);
        Assert.Equal(0.0, quaternion.Z);
    }

    [Fact]
    public void Identity_ShouldReturnIdentityQuaternion()
    {
        // Act
        var identity = Quaternion.Identity;

        // Assert
        Assert.Equal(1.0, identity.W);
        Assert.Equal(0.0, identity.X);
        Assert.Equal(0.0, identity.Y);
        Assert.Equal(0.0, identity.Z);
    }

    [Fact]
    public void Magnitude_ShouldCalculateCorrectly()
    {
        // Arrange
        var quaternion = new Quaternion(1.0, 0.0, 0.0, 0.0);

        // Act
        var magnitude = quaternion.Magnitude;

        // Assert
        Assert.Equal(1.0, magnitude, Epsilon);
    }

    [Fact]
    public void IsNormalized_ForNormalizedQuaternion_ShouldReturnTrue()
    {
        // Arrange
        var quaternion = new Quaternion(1.0, 0.0, 0.0, 0.0);

        // Act
        var isNormalized = quaternion.IsNormalized;

        // Assert
        Assert.True(isNormalized);
    }

    [Fact]
    public void IsNormalized_ForUnnormalizedQuaternion_ShouldReturnFalse()
    {
        // Arrange
        var quaternion = new Quaternion(2.0, 0.0, 0.0, 0.0);

        // Act
        var isNormalized = quaternion.IsNormalized;

        // Assert
        Assert.False(isNormalized);
    }

    [Fact]
    public void Normalize_ShouldReturnNormalizedQuaternion()
    {
        // Arrange
        var quaternion = new Quaternion(2.0, 0.0, 0.0, 0.0);

        // Act
        var normalized = quaternion.Normalize();

        // Assert
        Assert.True(normalized.IsNormalized);
        Assert.Equal(1.0, normalized.W, Epsilon);
        Assert.Equal(0.0, normalized.X, Epsilon);
    }

    [Fact]
    public void Normalize_ZeroQuaternion_ShouldReturnIdentity()
    {
        // Arrange
        var quaternion = new Quaternion(0.0, 0.0, 0.0, 0.0);

        // Act
        var normalized = quaternion.Normalize();

        // Assert
        Assert.Equal(Quaternion.Identity, normalized);
    }

    [Fact]
    public void Conjugate_ShouldNegateVectorComponents()
    {
        // Arrange
        var quaternion = new Quaternion(1.0, 2.0, 3.0, 4.0);

        // Act
        var conjugate = quaternion.Conjugate();

        // Assert
        Assert.Equal(1.0, conjugate.W);
        Assert.Equal(-2.0, conjugate.X);
        Assert.Equal(-3.0, conjugate.Y);
        Assert.Equal(-4.0, conjugate.Z);
    }

    [Fact]
    public void Inverse_ShouldReturnInverseQuaternion()
    {
        // Arrange
        var quaternion = new Quaternion(1.0, 0.0, 0.0, 0.0);

        // Act
        var inverse = quaternion.Inverse();

        // Assert
        Assert.Equal(1.0, inverse.W, Epsilon);
        Assert.Equal(0.0, inverse.X, Epsilon);
        Assert.Equal(0.0, inverse.Y, Epsilon);
        Assert.Equal(0.0, inverse.Z, Epsilon);
    }

    [Fact]
    public void Inverse_ZeroQuaternion_ShouldReturnIdentity()
    {
        // Arrange
        var quaternion = new Quaternion(0.0, 0.0, 0.0, 0.0);

        // Act
        var inverse = quaternion.Inverse();

        // Assert
        Assert.Equal(Quaternion.Identity, inverse);
    }

    [Fact]
    public void Multiplication_ShouldCombineRotations()
    {
        // Arrange
        var q1 = Quaternion.Identity;
        var q2 = Quaternion.Identity;

        // Act
        var result = q1 * q2;

        // Assert
        Assert.Equal(Quaternion.Identity, result);
    }

    [Fact]
    public void FromAxisAngle_ShouldCreateRotationQuaternion()
    {
        // Arrange
        var axis = Vector3D.UnitZ;
        var angle = Math.PI / 2; // 90 degrees

        // Act
        var quaternion = Quaternion.FromAxisAngle(axis, angle);

        // Assert
        Assert.True(quaternion.IsNormalized);
        Assert.Equal(Math.Cos(angle / 2), quaternion.W, Epsilon);
    }

    [Fact]
    public void ToAxisAngle_ShouldExtractAxisAndAngle()
    {
        // Arrange
        var axis = Vector3D.UnitZ;
        var angle = Math.PI / 2;
        var quaternion = Quaternion.FromAxisAngle(axis, angle);

        // Act
        var (resultAxis, resultAngle) = quaternion.ToAxisAngle();

        // Assert
        Assert.Equal(angle, resultAngle, Epsilon);
        Assert.Equal(axis.X, resultAxis.X, Epsilon);
        Assert.Equal(axis.Y, resultAxis.Y, Epsilon);
        Assert.Equal(axis.Z, resultAxis.Z, Epsilon);
    }

    [Fact]
    public void FromEulerAngles_ShouldCreateQuaternion()
    {
        // Arrange
        var roll = 0.0;
        var pitch = 0.0;
        var yaw = 0.0;

        // Act
        var quaternion = Quaternion.FromEulerAngles(roll, pitch, yaw);

        // Assert
        Assert.True(quaternion.IsNormalized);
        Assert.Equal(Quaternion.Identity.W, quaternion.W, Epsilon);
    }

    [Fact]
    public void ToEulerAngles_ShouldExtractAngles()
    {
        // Arrange
        var roll = 0.1;
        var pitch = 0.2;
        var yaw = 0.3;
        var quaternion = Quaternion.FromEulerAngles(roll, pitch, yaw);

        // Act
        var (resultRoll, resultPitch, resultYaw) = quaternion.ToEulerAngles();

        // Assert
        Assert.Equal(roll, resultRoll, Epsilon);
        Assert.Equal(pitch, resultPitch, Epsilon);
        Assert.Equal(yaw, resultYaw, Epsilon);
    }

    [Fact]
    public void Rotate_ShouldRotateVector()
    {
        // Arrange
        var quaternion = Quaternion.FromAxisAngle(Vector3D.UnitZ, Math.PI / 2);
        var vector = Vector3D.UnitX;

        // Act
        var rotated = quaternion.Rotate(vector);

        // Assert
        Assert.Equal(0.0, rotated.X, Epsilon);
        Assert.Equal(1.0, rotated.Y, Epsilon);
        Assert.Equal(0.0, rotated.Z, Epsilon);
    }

    [Fact]
    public void Slerp_ShouldInterpolateQuaternions()
    {
        // Arrange
        var q1 = Quaternion.Identity;
        var q2 = Quaternion.FromAxisAngle(Vector3D.UnitZ, Math.PI / 2);

        // Act
        var result = Quaternion.Slerp(q1, q2, 0.5);

        // Assert
        Assert.True(result.IsNormalized);
    }

    [Fact]
    public void Slerp_AtT0_ShouldReturnFirstQuaternion()
    {
        // Arrange
        var q1 = Quaternion.Identity;
        var q2 = Quaternion.FromAxisAngle(Vector3D.UnitZ, Math.PI / 2);

        // Act
        var result = Quaternion.Slerp(q1, q2, 0.0);

        // Assert
        Assert.Equal(q1.W, result.W, Epsilon);
        Assert.Equal(q1.X, result.X, Epsilon);
        Assert.Equal(q1.Y, result.Y, Epsilon);
        Assert.Equal(q1.Z, result.Z, Epsilon);
    }

    [Fact]
    public void Slerp_AtT1_ShouldReturnSecondQuaternion()
    {
        // Arrange
        var q1 = Quaternion.Identity;
        var q2 = Quaternion.FromAxisAngle(Vector3D.UnitZ, Math.PI / 2);

        // Act
        var result = Quaternion.Slerp(q1, q2, 1.0);

        // Assert
        Assert.Equal(q2.W, result.W, Epsilon);
        Assert.Equal(q2.X, result.X, Epsilon);
        Assert.Equal(q2.Y, result.Y, Epsilon);
        Assert.Equal(q2.Z, result.Z, Epsilon);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var q1 = new Quaternion(1.0, 2.0, 3.0, 4.0);
        var q2 = new Quaternion(1.0, 2.0, 3.0, 4.0);

        // Act & Assert
        Assert.True(q1.Equals(q2));
        Assert.True(q1 == q2);
        Assert.False(q1 != q2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var q1 = new Quaternion(1.0, 2.0, 3.0, 4.0);
        var q2 = new Quaternion(5.0, 6.0, 7.0, 8.0);

        // Act & Assert
        Assert.False(q1.Equals(q2));
        Assert.False(q1 == q2);
        Assert.True(q1 != q2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var q1 = new Quaternion(1.0, 2.0, 3.0, 4.0);
        var q2 = new Quaternion(1.0, 2.0, 3.0, 4.0);

        // Act & Assert
        Assert.Equal(q1.GetHashCode(), q2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var quaternion = new Quaternion(1.0, 2.0, 3.0, 4.0);

        // Act
        var result = quaternion.ToString();

        // Assert
        Assert.Contains("1", result);
        Assert.Contains("2", result);
        Assert.Contains("3", result);
        Assert.Contains("4", result);
    }
}
