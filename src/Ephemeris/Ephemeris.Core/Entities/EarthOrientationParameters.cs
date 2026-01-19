namespace Ephemeris.Core.Entities;

public sealed class EarthOrientationParameters
{
    public Guid Id { get; private set; }

    // Modified Julian Date for the data point
    public double Mjd { get; private set; }

    // Corresponding UTC date
    public DateTime Date { get; private set; }

    // Polar motion coordinates in arcseconds
    public double XPoleArcsec { get; private set; }
    public double YPoleArcsec { get; private set; }

    // UT1-UTC difference in seconds
    public double Ut1MinusUtcSeconds { get; private set; }

    // Length of day offset in milliseconds
    public double LodMilliseconds { get; private set; }

    // Celestial pole offsets (nutation corrections) in arcseconds
    public double DPsiArcsec { get; private set; }
    public double DEpsilonArcsec { get; private set; }

    // Uncertainties (optional)
    public double? XPoleUncertainty { get; private set; }
    public double? YPoleUncertainty { get; private set; }
    public double? Ut1MinusUtcUncertainty { get; private set; }

    // Data source (e.g., "IERS_BULLETIN_A", "IERS_BULLETIN_B", "FINALS2000A")
    public string Source { get; private set; } = string.Empty;

    // Whether this is a prediction or final value
    public bool IsPrediction { get; private set; }

    public DateTime RecordedAt { get; private set; }

    private EarthOrientationParameters() { }

    public static EarthOrientationParameters Create(
        double mjd,
        DateTime date,
        double xPoleArcsec,
        double yPoleArcsec,
        double ut1MinusUtcSeconds,
        double lodMilliseconds,
        double dPsiArcsec,
        double dEpsilonArcsec,
        string source,
        bool isPrediction,
        double? xPoleUncertainty = null,
        double? yPoleUncertainty = null,
        double? ut1MinusUtcUncertainty = null)
    {
        return new EarthOrientationParameters
        {
            Id = Guid.NewGuid(),
            Mjd = mjd,
            Date = date,
            XPoleArcsec = xPoleArcsec,
            YPoleArcsec = yPoleArcsec,
            Ut1MinusUtcSeconds = ut1MinusUtcSeconds,
            LodMilliseconds = lodMilliseconds,
            DPsiArcsec = dPsiArcsec,
            DEpsilonArcsec = dEpsilonArcsec,
            Source = source,
            IsPrediction = isPrediction,
            XPoleUncertainty = xPoleUncertainty,
            YPoleUncertainty = yPoleUncertainty,
            Ut1MinusUtcUncertainty = ut1MinusUtcUncertainty,
            RecordedAt = DateTime.UtcNow
        };
    }

    // Convert polar motion to radians
    public double XPoleRadians => XPoleArcsec * Math.PI / (180.0 * 3600.0);
    public double YPoleRadians => YPoleArcsec * Math.PI / (180.0 * 3600.0);

    // Convert nutation corrections to radians
    public double DPsiRadians => DPsiArcsec * Math.PI / (180.0 * 3600.0);
    public double DEpsilonRadians => DEpsilonArcsec * Math.PI / (180.0 * 3600.0);
}

public static class TimeConversions
{
    // J2000.0 epoch as Modified Julian Date
    public const double J2000Mjd = 51544.5;

    // Julian Date of J2000.0
    public const double J2000Jd = 2451545.0;

    // Modified Julian Date offset
    public const double MjdOffset = 2400000.5;

    // Seconds per day
    public const double SecondsPerDay = 86400.0;

    public static double DateTimeToMjd(DateTime dt)
    {
        var jd = DateTimeToJd(dt);
        return jd - MjdOffset;
    }

    public static double DateTimeToJd(DateTime dt)
    {
        var year = dt.Year;
        var month = dt.Month;
        var day = dt.Day + (dt.Hour + dt.Minute / 60.0 + dt.Second / 3600.0 + dt.Millisecond / 3600000.0) / 24.0;

        if (month <= 2)
        {
            year--;
            month += 12;
        }

        var a = (int)(year / 100.0);
        var b = 2 - a + (int)(a / 4.0);

        return (int)(365.25 * (year + 4716)) + (int)(30.6001 * (month + 1)) + day + b - 1524.5;
    }

    public static DateTime MjdToDateTime(double mjd)
    {
        var jd = mjd + MjdOffset;
        return JdToDateTime(jd);
    }

    public static DateTime JdToDateTime(double jd)
    {
        var z = (int)(jd + 0.5);
        var f = jd + 0.5 - z;

        int a;
        if (z < 2299161)
        {
            a = z;
        }
        else
        {
            var alpha = (int)((z - 1867216.25) / 36524.25);
            a = z + 1 + alpha - (int)(alpha / 4.0);
        }

        var b = a + 1524;
        var c = (int)((b - 122.1) / 365.25);
        var d = (int)(365.25 * c);
        var e = (int)((b - d) / 30.6001);

        var day = b - d - (int)(30.6001 * e);
        var month = e < 14 ? e - 1 : e - 13;
        var year = month > 2 ? c - 4716 : c - 4715;

        var hours = f * 24.0;
        var hour = (int)hours;
        var minutes = (hours - hour) * 60.0;
        var minute = (int)minutes;
        var seconds = (minutes - minute) * 60.0;
        var second = (int)seconds;
        var millisecond = (int)((seconds - second) * 1000);

        return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
    }

    public static double JulianCenturiesFromJ2000(DateTime dt)
    {
        var jd = DateTimeToJd(dt);
        return (jd - J2000Jd) / 36525.0;
    }
}
