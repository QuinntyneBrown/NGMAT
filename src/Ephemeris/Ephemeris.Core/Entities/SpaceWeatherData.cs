namespace Ephemeris.Core.Entities;

public sealed class SpaceWeatherData
{
    public Guid Id { get; private set; }

    // Date of the space weather data
    public DateTime Date { get; private set; }

    // Solar radio flux at 10.7 cm wavelength (F10.7) in SFU (solar flux units)
    // Used for atmospheric density models
    public double F107Observed { get; private set; }

    // F10.7 adjusted to 1 AU
    public double F107Adjusted { get; private set; }

    // 81-day centered average of F10.7
    public double F107Average81Day { get; private set; }

    // Daily Ap index (0-400 scale)
    public double ApDaily { get; private set; }

    // 3-hourly Ap indices (8 values per day)
    public double[]? Ap3Hour { get; private set; }

    // Daily Kp sum
    public double KpSum { get; private set; }

    // 3-hourly Kp indices (8 values per day, 0-9 scale)
    public double[]? Kp3Hour { get; private set; }

    // Sunspot number
    public double? SunspotNumber { get; private set; }

    // Mg II core-to-wing ratio (proxy for EUV)
    public double? MgIiIndex { get; private set; }

    // S10.7 (EUV proxy)
    public double? S107 { get; private set; }

    // M10.7 (FUV proxy)
    public double? M107 { get; private set; }

    // Y10.7 (mixed proxy)
    public double? Y107 { get; private set; }

    // Dst index (ring current, nT)
    public double? DstIndex { get; private set; }

    // Data source
    public string Source { get; private set; } = string.Empty;

    // Whether this is predicted or observed
    public bool IsPrediction { get; private set; }

    public DateTime RecordedAt { get; private set; }

    private SpaceWeatherData() { }

    public static SpaceWeatherData Create(
        DateTime date,
        double f107Observed,
        double f107Adjusted,
        double f107Average81Day,
        double apDaily,
        double kpSum,
        string source,
        bool isPrediction,
        double[]? ap3Hour = null,
        double[]? kp3Hour = null,
        double? sunspotNumber = null,
        double? mgIiIndex = null,
        double? s107 = null,
        double? m107 = null,
        double? y107 = null,
        double? dstIndex = null)
    {
        return new SpaceWeatherData
        {
            Id = Guid.NewGuid(),
            Date = date.Date,
            F107Observed = f107Observed,
            F107Adjusted = f107Adjusted,
            F107Average81Day = f107Average81Day,
            ApDaily = apDaily,
            Ap3Hour = ap3Hour,
            KpSum = kpSum,
            Kp3Hour = kp3Hour,
            SunspotNumber = sunspotNumber,
            MgIiIndex = mgIiIndex,
            S107 = s107,
            M107 = m107,
            Y107 = y107,
            DstIndex = dstIndex,
            Source = source,
            IsPrediction = isPrediction,
            RecordedAt = DateTime.UtcNow
        };
    }

    public void Update(
        double? f107Observed = null,
        double? f107Adjusted = null,
        double? f107Average81Day = null,
        double? apDaily = null,
        double? kpSum = null,
        double[]? ap3Hour = null,
        double[]? kp3Hour = null,
        bool? isPrediction = null)
    {
        if (f107Observed.HasValue)
            F107Observed = f107Observed.Value;
        if (f107Adjusted.HasValue)
            F107Adjusted = f107Adjusted.Value;
        if (f107Average81Day.HasValue)
            F107Average81Day = f107Average81Day.Value;
        if (apDaily.HasValue)
            ApDaily = apDaily.Value;
        if (kpSum.HasValue)
            KpSum = kpSum.Value;
        if (ap3Hour != null)
            Ap3Hour = ap3Hour;
        if (kp3Hour != null)
            Kp3Hour = kp3Hour;
        if (isPrediction.HasValue)
            IsPrediction = isPrediction.Value;

        RecordedAt = DateTime.UtcNow;
    }
}

public static class SpaceWeatherConstants
{
    // Quiet Sun F10.7 value (solar flux units)
    public const double QuietSunF107 = 70.0;

    // Active Sun F10.7 value (solar flux units)
    public const double ActiveSunF107 = 250.0;

    // Average F10.7 value (solar flux units)
    public const double AverageF107 = 150.0;

    // Quiet Ap index
    public const double QuietAp = 4.0;

    // Active Ap index
    public const double ActiveAp = 100.0;

    // Average Ap index
    public const double AverageAp = 15.0;

    // Kp scale conversion to Ap
    public static readonly double[] KpToAp = { 0, 2, 3, 4, 5, 6, 7, 9, 12, 15, 18, 22, 27, 32, 39, 48, 56, 67, 80, 94, 111, 132, 154, 179, 207, 236, 300, 400 };

    public static double ConvertKpToAp(double kp)
    {
        var index = (int)(kp * 3);
        if (index < 0) index = 0;
        if (index >= KpToAp.Length - 1) index = KpToAp.Length - 2;

        var fraction = kp * 3 - index;
        return KpToAp[index] + fraction * (KpToAp[index + 1] - KpToAp[index]);
    }
}
