using CoordinateSystem.Core.Entities;

namespace CoordinateSystem.Core.Services;

/// <summary>
/// Service for coordinate system transformations.
/// </summary>
public sealed class CoordinateTransformService
{
    /// <summary>
    /// Convert ECI (J2000) state vector to ECEF at a given epoch.
    /// </summary>
    public StateVector EciToEcef(StateVector eciState, DateTime epoch)
    {
        // Compute Greenwich Mean Sidereal Time (GMST)
        var gmst = ComputeGmst(epoch);

        // Create rotation matrix for GMST
        var rotationMatrix = TransformationMatrix.RotationZ(gmst);

        // Transform position
        var ecefPosition = rotationMatrix.Transform(eciState.Position);

        // Transform velocity (need to account for Earth's rotation)
        var omega = Wgs84.AngularVelocity;
        var ecefVelocity = rotationMatrix.Transform(eciState.Velocity);

        // Add effect of Earth's rotation: v_ecef = R * v_eci - ω × r_ecef
        ecefVelocity = new Vector3(
            ecefVelocity.X + omega * ecefPosition.Y,
            ecefVelocity.Y - omega * ecefPosition.X,
            ecefVelocity.Z);

        return new StateVector(ecefPosition, ecefVelocity);
    }

    /// <summary>
    /// Convert ECEF state vector to ECI (J2000) at a given epoch.
    /// </summary>
    public StateVector EcefToEci(StateVector ecefState, DateTime epoch)
    {
        var gmst = ComputeGmst(epoch);
        var rotationMatrix = TransformationMatrix.RotationZ(-gmst);

        // First, remove the effect of Earth's rotation from velocity
        var omega = Wgs84.AngularVelocity;
        var adjustedVelocity = new Vector3(
            ecefState.Velocity.X - omega * ecefState.Position.Y,
            ecefState.Velocity.Y + omega * ecefState.Position.X,
            ecefState.Velocity.Z);

        var eciPosition = rotationMatrix.Transform(ecefState.Position);
        var eciVelocity = rotationMatrix.Transform(adjustedVelocity);

        return new StateVector(eciPosition, eciVelocity);
    }

    /// <summary>
    /// Convert ECEF position to geodetic coordinates (WGS-84).
    /// Uses iterative algorithm for accurate altitude computation.
    /// </summary>
    public GeodeticCoordinates EcefToGeodetic(Vector3 ecefPosition)
    {
        var x = ecefPosition.X;
        var y = ecefPosition.Y;
        var z = ecefPosition.Z;

        var a = Wgs84.SemiMajorAxis;
        var b = Wgs84.SemiMinorAxis;
        var e2 = Wgs84.EccentricitySquared;
        var ep2 = Wgs84.SecondEccentricitySquared;

        // Longitude is straightforward
        var longitude = Math.Atan2(y, x);

        // Distance from Z-axis
        var p = Math.Sqrt(x * x + y * y);

        // Iterative solution for latitude and altitude
        // Initial estimate using spherical approximation
        var latitude = Math.Atan2(z, p * (1 - e2));
        double altitude;

        for (int i = 0; i < 10; i++)
        {
            var sinLat = Math.Sin(latitude);
            var N = a / Math.Sqrt(1 - e2 * sinLat * sinLat);
            altitude = p / Math.Cos(latitude) - N;
            var newLatitude = Math.Atan2(z, p * (1 - e2 * N / (N + altitude)));

            if (Math.Abs(newLatitude - latitude) < 1e-12)
            {
                latitude = newLatitude;
                break;
            }
            latitude = newLatitude;
        }

        // Final altitude calculation
        var sinLatFinal = Math.Sin(latitude);
        var N_final = a / Math.Sqrt(1 - e2 * sinLatFinal * sinLatFinal);
        altitude = p / Math.Cos(latitude) - N_final;

        return new GeodeticCoordinates(latitude, longitude, altitude);
    }

    /// <summary>
    /// Convert geodetic coordinates to ECEF position (WGS-84).
    /// </summary>
    public Vector3 GeodeticToEcef(GeodeticCoordinates geodetic)
    {
        var lat = geodetic.Latitude;
        var lon = geodetic.Longitude;
        var h = geodetic.Altitude;

        var a = Wgs84.SemiMajorAxis;
        var e2 = Wgs84.EccentricitySquared;

        var sinLat = Math.Sin(lat);
        var cosLat = Math.Cos(lat);
        var sinLon = Math.Sin(lon);
        var cosLon = Math.Cos(lon);

        // Radius of curvature in prime vertical
        var N = a / Math.Sqrt(1 - e2 * sinLat * sinLat);

        var x = (N + h) * cosLat * cosLon;
        var y = (N + h) * cosLat * sinLon;
        var z = (N * (1 - e2) + h) * sinLat;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Compute transformation matrix from ECI to VNB (Velocity-Normal-Binormal) frame.
    /// </summary>
    public TransformationMatrix GetVnbMatrix(StateVector state)
    {
        var r = state.Position;
        var v = state.Velocity;

        // Velocity direction
        var vHat = v.Normalize();

        // Normal direction (cross product of r and v, normalized)
        var h = Vector3.Cross(r, v);
        var nHat = h.Normalize();

        // Binormal direction (cross product of v and n)
        var bHat = Vector3.Cross(vHat, nHat);

        // Build transformation matrix (rows are the unit vectors)
        return new TransformationMatrix(
            vHat.X, vHat.Y, vHat.Z,
            nHat.X, nHat.Y, nHat.Z,
            bHat.X, bHat.Y, bHat.Z);
    }

    /// <summary>
    /// Compute transformation matrix from ECI to LVLH (Local Vertical Local Horizontal) frame.
    /// Also known as RSW (Radial-Along track-Cross track).
    /// </summary>
    public TransformationMatrix GetLvlhMatrix(StateVector state)
    {
        var r = state.Position;
        var v = state.Velocity;

        // Radial direction (away from Earth center)
        var rHat = r.Normalize();

        // Cross-track direction (normal to orbital plane)
        var h = Vector3.Cross(r, v);
        var wHat = h.Normalize();

        // Along-track direction (completes right-hand system)
        var sHat = Vector3.Cross(wHat, rHat);

        // Build transformation matrix (rows are the unit vectors)
        // R = radial (positive outward)
        // S = along-track (positive in direction of motion)
        // W = cross-track (positive normal to orbital plane)
        return new TransformationMatrix(
            rHat.X, rHat.Y, rHat.Z,
            sHat.X, sHat.Y, sHat.Z,
            wHat.X, wHat.Y, wHat.Z);
    }

    /// <summary>
    /// Compute transformation matrix from ECI to RSW frame.
    /// R = Radial, S = Along-track, W = Cross-track.
    /// </summary>
    public TransformationMatrix GetRswMatrix(StateVector state) => GetLvlhMatrix(state);

    /// <summary>
    /// Transform a state vector from one body-fixed frame to another.
    /// </summary>
    public StateVector TransformFrame(
        StateVector state,
        TransformationMatrix fromFrameToEci,
        TransformationMatrix eciToTargetFrame)
    {
        // Convert to ECI
        var eciState = fromFrameToEci.Transform(state);

        // Convert from ECI to target frame
        return eciToTargetFrame.Transform(eciState);
    }

    /// <summary>
    /// Compute Greenwich Mean Sidereal Time (GMST) in radians.
    /// </summary>
    public double ComputeGmst(DateTime utc)
    {
        // Julian date
        var jd = DateTimeToJulianDate(utc);

        // Julian centuries from J2000.0
        var t = (jd - 2451545.0) / 36525.0;

        // GMST at 0h UT1 in seconds
        var gmst0 = 24110.54841 + 8640184.812866 * t + 0.093104 * t * t - 6.2e-6 * t * t * t;

        // Fraction of day
        var ut1Fraction = (utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0 + utc.Millisecond / 3600000.0) / 24.0;

        // GMST in seconds
        var gmstSeconds = gmst0 + 86400.0 * 1.00273790935 * ut1Fraction;

        // Convert to radians (normalize to 0-2π)
        var gmstRad = (gmstSeconds / 86400.0) * 2.0 * Math.PI;
        gmstRad = gmstRad % (2.0 * Math.PI);
        if (gmstRad < 0) gmstRad += 2.0 * Math.PI;

        return gmstRad;
    }

    /// <summary>
    /// Convert DateTime to Julian Date.
    /// </summary>
    public double DateTimeToJulianDate(DateTime dateTime)
    {
        var year = dateTime.Year;
        var month = dateTime.Month;
        var day = dateTime.Day +
            (dateTime.Hour + dateTime.Minute / 60.0 + dateTime.Second / 3600.0 + dateTime.Millisecond / 3600000.0) / 24.0;

        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        var A = Math.Floor(year / 100.0);
        var B = 2 - A + Math.Floor(A / 4.0);

        return Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + B - 1524.5;
    }

    /// <summary>
    /// Convert Julian Date to DateTime (UTC).
    /// </summary>
    public DateTime JulianDateToDateTime(double jd)
    {
        var z = Math.Floor(jd + 0.5);
        var f = jd + 0.5 - z;

        double a;
        if (z < 2299161)
        {
            a = z;
        }
        else
        {
            var alpha = Math.Floor((z - 1867216.25) / 36524.25);
            a = z + 1 + alpha - Math.Floor(alpha / 4);
        }

        var b = a + 1524;
        var c = Math.Floor((b - 122.1) / 365.25);
        var d = Math.Floor(365.25 * c);
        var e = Math.Floor((b - d) / 30.6001);

        var day = b - d - Math.Floor(30.6001 * e) + f;
        var month = (e < 14) ? e - 1 : e - 13;
        var year = (month > 2) ? c - 4716 : c - 4715;

        var dayInt = (int)Math.Floor(day);
        var dayFrac = day - dayInt;
        var hours = dayFrac * 24;
        var hourInt = (int)Math.Floor(hours);
        var minutes = (hours - hourInt) * 60;
        var minuteInt = (int)Math.Floor(minutes);
        var seconds = (minutes - minuteInt) * 60;
        var secondInt = (int)Math.Floor(seconds);
        var milliseconds = (int)Math.Round((seconds - secondInt) * 1000);

        return new DateTime((int)year, (int)month, dayInt, hourInt, minuteInt, secondInt, milliseconds, DateTimeKind.Utc);
    }
}
