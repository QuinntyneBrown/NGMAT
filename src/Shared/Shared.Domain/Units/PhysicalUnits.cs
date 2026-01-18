using MessagePack;

namespace Shared.Domain.Units;

/// <summary>
/// Represents a length/distance value with unit conversions.
/// </summary>
[MessagePackObject]
public readonly struct Length : IEquatable<Length>, IComparable<Length>
{
    [Key(0)]
    public double Meters { get; }

    [IgnoreMember]
    public double Kilometers => Meters / 1000.0;

    [IgnoreMember]
    public double AstronomicalUnits => Meters / 149597870700.0;

    [IgnoreMember]
    public double Miles => Meters / 1609.344;

    [IgnoreMember]
    public double Feet => Meters / 0.3048;

    [SerializationConstructor]
    public Length(double meters)
    {
        Meters = meters;
    }

    public static Length FromMeters(double value) => new(value);
    public static Length FromKilometers(double value) => new(value * 1000.0);
    public static Length FromAU(double value) => new(value * 149597870700.0);
    public static Length FromMiles(double value) => new(value * 1609.344);
    public static Length FromFeet(double value) => new(value * 0.3048);

    public static Length operator +(Length a, Length b) => new(a.Meters + b.Meters);
    public static Length operator -(Length a, Length b) => new(a.Meters - b.Meters);
    public static Length operator *(Length l, double scalar) => new(l.Meters * scalar);
    public static Length operator /(Length l, double scalar) => new(l.Meters / scalar);

    public bool Equals(Length other) => Meters.Equals(other.Meters);
    public int CompareTo(Length other) => Meters.CompareTo(other.Meters);
    public override bool Equals(object? obj) => obj is Length other && Equals(other);
    public override int GetHashCode() => Meters.GetHashCode();
    public static bool operator ==(Length left, Length right) => left.Equals(right);
    public static bool operator !=(Length left, Length right) => !left.Equals(right);
    public static bool operator <(Length left, Length right) => left.Meters < right.Meters;
    public static bool operator >(Length left, Length right) => left.Meters > right.Meters;
    public static bool operator <=(Length left, Length right) => left.Meters <= right.Meters;
    public static bool operator >=(Length left, Length right) => left.Meters >= right.Meters;
    public override string ToString() => $"{Meters:G6} m";
}

/// <summary>
/// Represents a duration/time value with unit conversions.
/// </summary>
[MessagePackObject]
public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>
{
    [Key(0)]
    public double Seconds { get; }

    [IgnoreMember]
    public double Minutes => Seconds / 60.0;

    [IgnoreMember]
    public double Hours => Seconds / 3600.0;

    [IgnoreMember]
    public double Days => Seconds / 86400.0;

    [IgnoreMember]
    public double JulianYears => Seconds / 31557600.0;

    [IgnoreMember]
    public TimeSpan ToTimeSpan => TimeSpan.FromSeconds(Seconds);

    [SerializationConstructor]
    public Duration(double seconds)
    {
        Seconds = seconds;
    }

    public static Duration FromSeconds(double value) => new(value);
    public static Duration FromMinutes(double value) => new(value * 60.0);
    public static Duration FromHours(double value) => new(value * 3600.0);
    public static Duration FromDays(double value) => new(value * 86400.0);
    public static Duration FromTimeSpan(TimeSpan ts) => new(ts.TotalSeconds);

    public static Duration operator +(Duration a, Duration b) => new(a.Seconds + b.Seconds);
    public static Duration operator -(Duration a, Duration b) => new(a.Seconds - b.Seconds);
    public static Duration operator *(Duration d, double scalar) => new(d.Seconds * scalar);
    public static Duration operator /(Duration d, double scalar) => new(d.Seconds / scalar);

    public bool Equals(Duration other) => Seconds.Equals(other.Seconds);
    public int CompareTo(Duration other) => Seconds.CompareTo(other.Seconds);
    public override bool Equals(object? obj) => obj is Duration other && Equals(other);
    public override int GetHashCode() => Seconds.GetHashCode();
    public static bool operator ==(Duration left, Duration right) => left.Equals(right);
    public static bool operator !=(Duration left, Duration right) => !left.Equals(right);
    public static bool operator <(Duration left, Duration right) => left.Seconds < right.Seconds;
    public static bool operator >(Duration left, Duration right) => left.Seconds > right.Seconds;
    public static bool operator <=(Duration left, Duration right) => left.Seconds <= right.Seconds;
    public static bool operator >=(Duration left, Duration right) => left.Seconds >= right.Seconds;
    public override string ToString() => $"{Seconds:G6} s";
}

/// <summary>
/// Represents an angle value with unit conversions.
/// </summary>
[MessagePackObject]
public readonly struct Angle : IEquatable<Angle>, IComparable<Angle>
{
    [Key(0)]
    public double Radians { get; }

    [IgnoreMember]
    public double Degrees => Radians * (180.0 / Math.PI);

    [IgnoreMember]
    public double ArcMinutes => Degrees * 60.0;

    [IgnoreMember]
    public double ArcSeconds => Degrees * 3600.0;

    [IgnoreMember]
    public double Revolutions => Radians / (2.0 * Math.PI);

    [SerializationConstructor]
    public Angle(double radians)
    {
        Radians = radians;
    }

    public static Angle FromRadians(double value) => new(value);
    public static Angle FromDegrees(double value) => new(value * (Math.PI / 180.0));
    public static Angle FromArcMinutes(double value) => new(value / 60.0 * (Math.PI / 180.0));
    public static Angle FromArcSeconds(double value) => new(value / 3600.0 * (Math.PI / 180.0));
    public static Angle FromRevolutions(double value) => new(value * 2.0 * Math.PI);

    public Angle Normalize()
    {
        var normalized = Radians % (2.0 * Math.PI);
        if (normalized < 0) normalized += 2.0 * Math.PI;
        return new Angle(normalized);
    }

    public Angle NormalizeSymmetric()
    {
        var normalized = Radians % (2.0 * Math.PI);
        if (normalized > Math.PI) normalized -= 2.0 * Math.PI;
        if (normalized < -Math.PI) normalized += 2.0 * Math.PI;
        return new Angle(normalized);
    }

    public static Angle operator +(Angle a, Angle b) => new(a.Radians + b.Radians);
    public static Angle operator -(Angle a, Angle b) => new(a.Radians - b.Radians);
    public static Angle operator -(Angle a) => new(-a.Radians);
    public static Angle operator *(Angle a, double scalar) => new(a.Radians * scalar);
    public static Angle operator /(Angle a, double scalar) => new(a.Radians / scalar);

    public bool Equals(Angle other) => Radians.Equals(other.Radians);
    public int CompareTo(Angle other) => Radians.CompareTo(other.Radians);
    public override bool Equals(object? obj) => obj is Angle other && Equals(other);
    public override int GetHashCode() => Radians.GetHashCode();
    public static bool operator ==(Angle left, Angle right) => left.Equals(right);
    public static bool operator !=(Angle left, Angle right) => !left.Equals(right);
    public static bool operator <(Angle left, Angle right) => left.Radians < right.Radians;
    public static bool operator >(Angle left, Angle right) => left.Radians > right.Radians;
    public static bool operator <=(Angle left, Angle right) => left.Radians <= right.Radians;
    public static bool operator >=(Angle left, Angle right) => left.Radians >= right.Radians;
    public override string ToString() => $"{Degrees:G6}°";
}

/// <summary>
/// Represents a mass value with unit conversions.
/// </summary>
[MessagePackObject]
public readonly struct Mass : IEquatable<Mass>, IComparable<Mass>
{
    [Key(0)]
    public double Kilograms { get; }

    [IgnoreMember]
    public double Grams => Kilograms * 1000.0;

    [IgnoreMember]
    public double Pounds => Kilograms * 2.20462;

    [IgnoreMember]
    public double MetricTons => Kilograms / 1000.0;

    [SerializationConstructor]
    public Mass(double kilograms)
    {
        Kilograms = kilograms;
    }

    public static Mass FromKilograms(double value) => new(value);
    public static Mass FromGrams(double value) => new(value / 1000.0);
    public static Mass FromPounds(double value) => new(value / 2.20462);
    public static Mass FromMetricTons(double value) => new(value * 1000.0);

    public static Mass operator +(Mass a, Mass b) => new(a.Kilograms + b.Kilograms);
    public static Mass operator -(Mass a, Mass b) => new(a.Kilograms - b.Kilograms);
    public static Mass operator *(Mass m, double scalar) => new(m.Kilograms * scalar);
    public static Mass operator /(Mass m, double scalar) => new(m.Kilograms / scalar);

    public bool Equals(Mass other) => Kilograms.Equals(other.Kilograms);
    public int CompareTo(Mass other) => Kilograms.CompareTo(other.Kilograms);
    public override bool Equals(object? obj) => obj is Mass other && Equals(other);
    public override int GetHashCode() => Kilograms.GetHashCode();
    public static bool operator ==(Mass left, Mass right) => left.Equals(right);
    public static bool operator !=(Mass left, Mass right) => !left.Equals(right);
    public static bool operator <(Mass left, Mass right) => left.Kilograms < right.Kilograms;
    public static bool operator >(Mass left, Mass right) => left.Kilograms > right.Kilograms;
    public static bool operator <=(Mass left, Mass right) => left.Kilograms <= right.Kilograms;
    public static bool operator >=(Mass left, Mass right) => left.Kilograms >= right.Kilograms;
    public override string ToString() => $"{Kilograms:G6} kg";
}
