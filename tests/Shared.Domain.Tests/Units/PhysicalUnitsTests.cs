using Shared.Domain.Units;

namespace Shared.Domain.Tests.Units;

public class LengthTests
{
    private const double Epsilon = 1e-10;

    [Fact]
    public void Constructor_ShouldCreateLengthInMeters()
    {
        // Act
        var length = new Length(1000.0);

        // Assert
        Assert.Equal(1000.0, length.Meters, Epsilon);
    }

    [Fact]
    public void FromMeters_ShouldCreateLength()
    {
        // Act
        var length = Length.FromMeters(1000.0);

        // Assert
        Assert.Equal(1000.0, length.Meters, Epsilon);
    }

    [Fact]
    public void FromKilometers_ShouldConvertCorrectly()
    {
        // Act
        var length = Length.FromKilometers(1.0);

        // Assert
        Assert.Equal(1000.0, length.Meters, Epsilon);
        Assert.Equal(1.0, length.Kilometers, Epsilon);
    }

    [Fact]
    public void FromAU_ShouldConvertCorrectly()
    {
        // Act
        var length = Length.FromAU(1.0);

        // Assert
        Assert.Equal(149597870700.0, length.Meters, Epsilon);
        Assert.Equal(1.0, length.AstronomicalUnits, Epsilon);
    }

    [Fact]
    public void FromMiles_ShouldConvertCorrectly()
    {
        // Act
        var length = Length.FromMiles(1.0);

        // Assert
        Assert.Equal(1609.344, length.Meters, Epsilon);
        Assert.Equal(1.0, length.Miles, Epsilon);
    }

    [Fact]
    public void FromFeet_ShouldConvertCorrectly()
    {
        // Act
        var length = Length.FromFeet(1.0);

        // Assert
        Assert.Equal(0.3048, length.Meters, Epsilon);
        Assert.Equal(1.0, length.Feet, Epsilon);
    }

    [Fact]
    public void Addition_ShouldWorkCorrectly()
    {
        // Arrange
        var l1 = Length.FromMeters(100.0);
        var l2 = Length.FromMeters(200.0);

        // Act
        var result = l1 + l2;

        // Assert
        Assert.Equal(300.0, result.Meters, Epsilon);
    }

    [Fact]
    public void Subtraction_ShouldWorkCorrectly()
    {
        // Arrange
        var l1 = Length.FromMeters(300.0);
        var l2 = Length.FromMeters(100.0);

        // Act
        var result = l1 - l2;

        // Assert
        Assert.Equal(200.0, result.Meters, Epsilon);
    }

    [Fact]
    public void Multiplication_ShouldWorkCorrectly()
    {
        // Arrange
        var length = Length.FromMeters(100.0);

        // Act
        var result = length * 2.0;

        // Assert
        Assert.Equal(200.0, result.Meters, Epsilon);
    }

    [Fact]
    public void Division_ShouldWorkCorrectly()
    {
        // Arrange
        var length = Length.FromMeters(100.0);

        // Act
        var result = length / 2.0;

        // Assert
        Assert.Equal(50.0, result.Meters, Epsilon);
    }

    [Fact]
    public void CompareTo_ShouldWorkCorrectly()
    {
        // Arrange
        var l1 = Length.FromMeters(100.0);
        var l2 = Length.FromMeters(200.0);

        // Act & Assert
        Assert.True(l1 < l2);
        Assert.True(l2 > l1);
        Assert.True(l1 <= l2);
        Assert.True(l2 >= l1);
    }

    [Fact]
    public void Equals_ShouldWorkCorrectly()
    {
        // Arrange
        var l1 = Length.FromMeters(100.0);
        var l2 = Length.FromMeters(100.0);
        var l3 = Length.FromMeters(200.0);

        // Act & Assert
        Assert.True(l1 == l2);
        Assert.False(l1 == l3);
        Assert.False(l1 != l2);
        Assert.True(l1 != l3);
    }
}

public class DurationTests
{
    private const double Epsilon = 1e-10;

    [Fact]
    public void FromSeconds_ShouldCreateDuration()
    {
        // Act
        var duration = Duration.FromSeconds(3600.0);

        // Assert
        Assert.Equal(3600.0, duration.Seconds, Epsilon);
    }

    [Fact]
    public void FromMinutes_ShouldConvertCorrectly()
    {
        // Act
        var duration = Duration.FromMinutes(60.0);

        // Assert
        Assert.Equal(3600.0, duration.Seconds, Epsilon);
        Assert.Equal(60.0, duration.Minutes, Epsilon);
    }

    [Fact]
    public void FromHours_ShouldConvertCorrectly()
    {
        // Act
        var duration = Duration.FromHours(1.0);

        // Assert
        Assert.Equal(3600.0, duration.Seconds, Epsilon);
        Assert.Equal(1.0, duration.Hours, Epsilon);
    }

    [Fact]
    public void FromDays_ShouldConvertCorrectly()
    {
        // Act
        var duration = Duration.FromDays(1.0);

        // Assert
        Assert.Equal(86400.0, duration.Seconds, Epsilon);
        Assert.Equal(1.0, duration.Days, Epsilon);
    }

    [Fact]
    public void FromTimeSpan_ShouldConvertCorrectly()
    {
        // Arrange
        var timeSpan = TimeSpan.FromHours(2);

        // Act
        var duration = Duration.FromTimeSpan(timeSpan);

        // Assert
        Assert.Equal(7200.0, duration.Seconds, Epsilon);
    }

    [Fact]
    public void ToTimeSpan_ShouldConvertCorrectly()
    {
        // Arrange
        var duration = Duration.FromHours(2);

        // Act
        var timeSpan = duration.ToTimeSpan;

        // Assert
        Assert.Equal(TimeSpan.FromHours(2), timeSpan);
    }

    [Fact]
    public void Addition_ShouldWorkCorrectly()
    {
        // Arrange
        var d1 = Duration.FromSeconds(100.0);
        var d2 = Duration.FromSeconds(200.0);

        // Act
        var result = d1 + d2;

        // Assert
        Assert.Equal(300.0, result.Seconds, Epsilon);
    }

    [Fact]
    public void Subtraction_ShouldWorkCorrectly()
    {
        // Arrange
        var d1 = Duration.FromSeconds(300.0);
        var d2 = Duration.FromSeconds(100.0);

        // Act
        var result = d1 - d2;

        // Assert
        Assert.Equal(200.0, result.Seconds, Epsilon);
    }
}

public class AngleTests
{
    private const double Epsilon = 1e-10;

    [Fact]
    public void FromRadians_ShouldCreateAngle()
    {
        // Act
        var angle = Angle.FromRadians(Math.PI);

        // Assert
        Assert.Equal(Math.PI, angle.Radians, Epsilon);
    }

    [Fact]
    public void FromDegrees_ShouldConvertCorrectly()
    {
        // Act
        var angle = Angle.FromDegrees(180.0);

        // Assert
        Assert.Equal(Math.PI, angle.Radians, Epsilon);
        Assert.Equal(180.0, angle.Degrees, Epsilon);
    }

    [Fact]
    public void FromRevolutions_ShouldConvertCorrectly()
    {
        // Act
        var angle = Angle.FromRevolutions(1.0);

        // Assert
        Assert.Equal(2.0 * Math.PI, angle.Radians, Epsilon);
        Assert.Equal(1.0, angle.Revolutions, Epsilon);
    }

    [Fact]
    public void Normalize_ShouldNormalizeToPositiveRange()
    {
        // Arrange
        var angle = Angle.FromDegrees(370.0);

        // Act
        var normalized = angle.Normalize();

        // Assert
        Assert.Equal(10.0, normalized.Degrees, Epsilon);
    }

    [Fact]
    public void NormalizeSymmetric_ShouldNormalizeToSymmetricRange()
    {
        // Arrange
        var angle = Angle.FromDegrees(190.0);

        // Act
        var normalized = angle.NormalizeSymmetric();

        // Assert
        Assert.Equal(-170.0, normalized.Degrees, 1e-8);
    }

    [Fact]
    public void Addition_ShouldWorkCorrectly()
    {
        // Arrange
        var a1 = Angle.FromDegrees(45.0);
        var a2 = Angle.FromDegrees(45.0);

        // Act
        var result = a1 + a2;

        // Assert
        Assert.Equal(90.0, result.Degrees, Epsilon);
    }

    [Fact]
    public void Subtraction_ShouldWorkCorrectly()
    {
        // Arrange
        var a1 = Angle.FromDegrees(90.0);
        var a2 = Angle.FromDegrees(45.0);

        // Act
        var result = a1 - a2;

        // Assert
        Assert.Equal(45.0, result.Degrees, Epsilon);
    }

    [Fact]
    public void Negation_ShouldWorkCorrectly()
    {
        // Arrange
        var angle = Angle.FromDegrees(45.0);

        // Act
        var result = -angle;

        // Assert
        Assert.Equal(-45.0, result.Degrees, Epsilon);
    }
}

public class MassTests
{
    private const double Epsilon = 1e-10;

    [Fact]
    public void FromKilograms_ShouldCreateMass()
    {
        // Act
        var mass = Mass.FromKilograms(1000.0);

        // Assert
        Assert.Equal(1000.0, mass.Kilograms, Epsilon);
    }

    [Fact]
    public void FromGrams_ShouldConvertCorrectly()
    {
        // Act
        var mass = Mass.FromGrams(1000.0);

        // Assert
        Assert.Equal(1.0, mass.Kilograms, Epsilon);
        Assert.Equal(1000.0, mass.Grams, Epsilon);
    }

    [Fact]
    public void FromPounds_ShouldConvertCorrectly()
    {
        // Act
        var mass = Mass.FromPounds(2.20462);

        // Assert
        Assert.Equal(1.0, mass.Kilograms, Epsilon);
    }

    [Fact]
    public void FromMetricTons_ShouldConvertCorrectly()
    {
        // Act
        var mass = Mass.FromMetricTons(1.0);

        // Assert
        Assert.Equal(1000.0, mass.Kilograms, Epsilon);
        Assert.Equal(1.0, mass.MetricTons, Epsilon);
    }

    [Fact]
    public void Addition_ShouldWorkCorrectly()
    {
        // Arrange
        var m1 = Mass.FromKilograms(100.0);
        var m2 = Mass.FromKilograms(200.0);

        // Act
        var result = m1 + m2;

        // Assert
        Assert.Equal(300.0, result.Kilograms, Epsilon);
    }

    [Fact]
    public void Subtraction_ShouldWorkCorrectly()
    {
        // Arrange
        var m1 = Mass.FromKilograms(300.0);
        var m2 = Mass.FromKilograms(100.0);

        // Act
        var result = m1 - m2;

        // Assert
        Assert.Equal(200.0, result.Kilograms, Epsilon);
    }

    [Fact]
    public void CompareTo_ShouldWorkCorrectly()
    {
        // Arrange
        var m1 = Mass.FromKilograms(100.0);
        var m2 = Mass.FromKilograms(200.0);

        // Act & Assert
        Assert.True(m1 < m2);
        Assert.True(m2 > m1);
    }
}
