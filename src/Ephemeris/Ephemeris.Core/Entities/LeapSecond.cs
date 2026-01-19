namespace Ephemeris.Core.Entities;

public sealed class LeapSecond
{
    public Guid Id { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public double TaiMinusUtcSeconds { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public DateTime RecordedAt { get; private set; }

    private LeapSecond() { }

    public static LeapSecond Create(DateTime effectiveDate, double taiMinusUtcSeconds, string source)
    {
        return new LeapSecond
        {
            Id = Guid.NewGuid(),
            EffectiveDate = effectiveDate.Date,
            TaiMinusUtcSeconds = taiMinusUtcSeconds,
            Source = source,
            RecordedAt = DateTime.UtcNow
        };
    }
}

public static class LeapSecondData
{
    // Historical leap seconds from IERS
    // TAI-UTC = cumulative leap seconds
    public static IEnumerable<(DateTime date, double taiMinusUtc)> GetHistoricalLeapSeconds()
    {
        yield return (new DateTime(1972, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10);
        yield return (new DateTime(1972, 7, 1, 0, 0, 0, DateTimeKind.Utc), 11);
        yield return (new DateTime(1973, 1, 1, 0, 0, 0, DateTimeKind.Utc), 12);
        yield return (new DateTime(1974, 1, 1, 0, 0, 0, DateTimeKind.Utc), 13);
        yield return (new DateTime(1975, 1, 1, 0, 0, 0, DateTimeKind.Utc), 14);
        yield return (new DateTime(1976, 1, 1, 0, 0, 0, DateTimeKind.Utc), 15);
        yield return (new DateTime(1977, 1, 1, 0, 0, 0, DateTimeKind.Utc), 16);
        yield return (new DateTime(1978, 1, 1, 0, 0, 0, DateTimeKind.Utc), 17);
        yield return (new DateTime(1979, 1, 1, 0, 0, 0, DateTimeKind.Utc), 18);
        yield return (new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc), 19);
        yield return (new DateTime(1981, 7, 1, 0, 0, 0, DateTimeKind.Utc), 20);
        yield return (new DateTime(1982, 7, 1, 0, 0, 0, DateTimeKind.Utc), 21);
        yield return (new DateTime(1983, 7, 1, 0, 0, 0, DateTimeKind.Utc), 22);
        yield return (new DateTime(1985, 7, 1, 0, 0, 0, DateTimeKind.Utc), 23);
        yield return (new DateTime(1988, 1, 1, 0, 0, 0, DateTimeKind.Utc), 24);
        yield return (new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), 25);
        yield return (new DateTime(1991, 1, 1, 0, 0, 0, DateTimeKind.Utc), 26);
        yield return (new DateTime(1992, 7, 1, 0, 0, 0, DateTimeKind.Utc), 27);
        yield return (new DateTime(1993, 7, 1, 0, 0, 0, DateTimeKind.Utc), 28);
        yield return (new DateTime(1994, 7, 1, 0, 0, 0, DateTimeKind.Utc), 29);
        yield return (new DateTime(1996, 1, 1, 0, 0, 0, DateTimeKind.Utc), 30);
        yield return (new DateTime(1997, 7, 1, 0, 0, 0, DateTimeKind.Utc), 31);
        yield return (new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc), 32);
        yield return (new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc), 33);
        yield return (new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc), 34);
        yield return (new DateTime(2012, 7, 1, 0, 0, 0, DateTimeKind.Utc), 35);
        yield return (new DateTime(2015, 7, 1, 0, 0, 0, DateTimeKind.Utc), 36);
        yield return (new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc), 37);
    }

    public static double GetTaiMinusUtc(DateTime utc)
    {
        var leapSeconds = GetHistoricalLeapSeconds().OrderByDescending(x => x.date).ToList();

        foreach (var (date, taiMinusUtc) in leapSeconds)
        {
            if (utc >= date)
            {
                return taiMinusUtc;
            }
        }

        // Before 1972, use approximation
        return 10.0;
    }

    public static DateTime UtcToTai(DateTime utc)
    {
        var leapSeconds = GetTaiMinusUtc(utc);
        return utc.AddSeconds(leapSeconds);
    }

    public static DateTime TaiToUtc(DateTime tai)
    {
        // Iterative approach to handle edge cases
        var utcEstimate = tai.AddSeconds(-37);
        var leapSeconds = GetTaiMinusUtc(utcEstimate);
        return tai.AddSeconds(-leapSeconds);
    }

    // TT = TAI + 32.184 seconds
    public const double TtMinusTaiSeconds = 32.184;

    public static DateTime UtcToTt(DateTime utc)
    {
        var tai = UtcToTai(utc);
        return tai.AddSeconds(TtMinusTaiSeconds);
    }

    public static DateTime TtToUtc(DateTime tt)
    {
        var tai = tt.AddSeconds(-TtMinusTaiSeconds);
        return TaiToUtc(tai);
    }
}
