namespace CoordinateSystem.Core.Entities;

/// <summary>
/// Geodetic coordinates (latitude, longitude, altitude) on an ellipsoid.
/// </summary>
public readonly struct GeodeticCoordinates : IEquatable<GeodeticCoordinates>
{
    /// <summary>Geodetic latitude in radians (-π/2 to π/2).</summary>
    public double Latitude { get; init; }

    /// <summary>Longitude in radians (-π to π).</summary>
    public double Longitude { get; init; }

    /// <summary>Altitude above ellipsoid in km.</summary>
    public double Altitude { get; init; }

    /// <summary>Latitude in degrees.</summary>
    public double LatitudeDegrees => Latitude * 180.0 / Math.PI;

    /// <summary>Longitude in degrees.</summary>
    public double LongitudeDegrees => Longitude * 180.0 / Math.PI;

    /// <summary>Altitude in meters.</summary>
    public double AltitudeMeters => Altitude * 1000.0;

    public GeodeticCoordinates(double latitude, double longitude, double altitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
    }

    public static GeodeticCoordinates FromDegrees(double latitudeDeg, double longitudeDeg, double altitudeKm)
    {
        return new GeodeticCoordinates(
            latitudeDeg * Math.PI / 180.0,
            longitudeDeg * Math.PI / 180.0,
            altitudeKm);
    }

    public bool Equals(GeodeticCoordinates other) =>
        Latitude == other.Latitude && Longitude == other.Longitude && Altitude == other.Altitude;

    public override bool Equals(object? obj) => obj is GeodeticCoordinates other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude, Altitude);

    public static bool operator ==(GeodeticCoordinates left, GeodeticCoordinates right) => left.Equals(right);
    public static bool operator !=(GeodeticCoordinates left, GeodeticCoordinates right) => !left.Equals(right);

    public override string ToString() =>
        $"Lat: {LatitudeDegrees:F6}°, Lon: {LongitudeDegrees:F6}°, Alt: {Altitude:F3} km";
}

/// <summary>
/// Constants for the WGS-84 ellipsoid.
/// </summary>
public static class Wgs84
{
    /// <summary>Semi-major axis (equatorial radius) in km.</summary>
    public const double SemiMajorAxis = 6378.137;

    /// <summary>Semi-minor axis (polar radius) in km.</summary>
    public const double SemiMinorAxis = 6356.752314245;

    /// <summary>Flattening factor.</summary>
    public const double Flattening = 1.0 / 298.257223563;

    /// <summary>First eccentricity squared.</summary>
    public static double EccentricitySquared =>
        (SemiMajorAxis * SemiMajorAxis - SemiMinorAxis * SemiMinorAxis) / (SemiMajorAxis * SemiMajorAxis);

    /// <summary>Second eccentricity squared.</summary>
    public static double SecondEccentricitySquared =>
        (SemiMajorAxis * SemiMajorAxis - SemiMinorAxis * SemiMinorAxis) / (SemiMinorAxis * SemiMinorAxis);

    /// <summary>Earth gravitational parameter (GM) in km³/s².</summary>
    public const double GM = 398600.4418;

    /// <summary>Earth angular velocity in rad/s.</summary>
    public const double AngularVelocity = 7.292115e-5;

    /// <summary>J2 zonal harmonic coefficient.</summary>
    public const double J2 = 1.08262668e-3;
}
