using MessagePack;
using Shared.Messaging.Abstractions;

namespace Ephemeris.Core.Events;

[MessagePackObject]
public sealed class CelestialBodyCreatedEvent : EventBase
{
    [Key(10)]
    public Guid CelestialBodyId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public int NaifId { get; init; }

    [Key(13)]
    public string Type { get; init; } = string.Empty;

    public CelestialBodyCreatedEvent() : base()
    {
        SourceService = "Ephemeris";
    }
}

[MessagePackObject]
public sealed class CelestialBodyUpdatedEvent : EventBase
{
    [Key(10)]
    public Guid CelestialBodyId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    public CelestialBodyUpdatedEvent() : base()
    {
        SourceService = "Ephemeris";
    }
}

[MessagePackObject]
public sealed class CelestialBodyPositionRecordedEvent : EventBase
{
    [Key(10)]
    public Guid PositionId { get; init; }

    [Key(11)]
    public Guid CelestialBodyId { get; init; }

    [Key(12)]
    public DateTimeOffset Epoch { get; init; }

    [Key(13)]
    public string Source { get; init; } = string.Empty;

    public CelestialBodyPositionRecordedEvent() : base()
    {
        SourceService = "Ephemeris";
    }
}

[MessagePackObject]
public sealed class EarthOrientationParametersUpdatedEvent : EventBase
{
    [Key(10)]
    public DateTimeOffset Date { get; init; }

    [Key(11)]
    public string Source { get; init; } = string.Empty;

    [Key(12)]
    public int RecordCount { get; init; }

    public EarthOrientationParametersUpdatedEvent() : base()
    {
        SourceService = "Ephemeris";
    }
}

[MessagePackObject]
public sealed class SpaceWeatherDataUpdatedEvent : EventBase
{
    [Key(10)]
    public DateTimeOffset Date { get; init; }

    [Key(11)]
    public string Source { get; init; } = string.Empty;

    [Key(12)]
    public double F107 { get; init; }

    [Key(13)]
    public double Ap { get; init; }

    public SpaceWeatherDataUpdatedEvent() : base()
    {
        SourceService = "Ephemeris";
    }
}

[MessagePackObject]
public sealed class LeapSecondAddedEvent : EventBase
{
    [Key(10)]
    public DateTimeOffset EffectiveDate { get; init; }

    [Key(11)]
    public double TaiMinusUtcSeconds { get; init; }

    public LeapSecondAddedEvent() : base()
    {
        SourceService = "Ephemeris";
    }
}

[MessagePackObject]
public sealed class EphemerisDataImportedEvent : EventBase
{
    [Key(10)]
    public string DataType { get; init; } = string.Empty;

    [Key(11)]
    public string Source { get; init; } = string.Empty;

    [Key(12)]
    public int RecordCount { get; init; }

    [Key(13)]
    public DateTimeOffset StartDate { get; init; }

    [Key(14)]
    public DateTimeOffset EndDate { get; init; }

    public EphemerisDataImportedEvent() : base()
    {
        SourceService = "Ephemeris";
    }
}
