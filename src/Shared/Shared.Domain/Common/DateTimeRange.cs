namespace Shared.Domain.Common;

/// <summary>
/// Represents a range of time between two points.
/// </summary>
public readonly struct DateTimeRange : IEquatable<DateTimeRange>
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }
    public TimeSpan Duration => End - Start;

    public DateTimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end < start)
        {
            throw new ArgumentException("End must be greater than or equal to start", nameof(end));
        }

        Start = start;
        End = end;
    }

    public static DateTimeRange Create(DateTimeOffset start, DateTimeOffset end) => new(start, end);

    public static DateTimeRange FromDuration(DateTimeOffset start, TimeSpan duration) =>
        new(start, start.Add(duration));

    public bool Contains(DateTimeOffset dateTime) => dateTime >= Start && dateTime <= End;

    public bool Overlaps(DateTimeRange other) =>
        Start < other.End && End > other.Start;

    public bool Contains(DateTimeRange other) =>
        Start <= other.Start && End >= other.End;

    public DateTimeRange? Intersect(DateTimeRange other)
    {
        if (!Overlaps(other))
        {
            return null;
        }

        var start = Start > other.Start ? Start : other.Start;
        var end = End < other.End ? End : other.End;
        return new DateTimeRange(start, end);
    }

    public DateTimeRange Union(DateTimeRange other)
    {
        var start = Start < other.Start ? Start : other.Start;
        var end = End > other.End ? End : other.End;
        return new DateTimeRange(start, end);
    }

    public bool Equals(DateTimeRange other) => Start == other.Start && End == other.End;

    public override bool Equals(object? obj) => obj is DateTimeRange other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Start, End);

    public static bool operator ==(DateTimeRange left, DateTimeRange right) => left.Equals(right);

    public static bool operator !=(DateTimeRange left, DateTimeRange right) => !left.Equals(right);

    public override string ToString() => $"[{Start:O} - {End:O}]";
}
