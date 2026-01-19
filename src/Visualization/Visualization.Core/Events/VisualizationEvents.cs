using MessagePack;
using Shared.Messaging.Abstractions;

namespace Visualization.Core.Events;

[MessagePackObject]
public sealed class OrbitPlotGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid PlotId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public int PointCount { get; init; }

    [Key(13)]
    public DateTime StartEpoch { get; init; }

    [Key(14)]
    public DateTime EndEpoch { get; init; }

    public OrbitPlotGeneratedEvent() : base()
    {
        SourceService = "Visualization";
    }
}

[MessagePackObject]
public sealed class GroundTrackGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid TrackId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public int PointCount { get; init; }

    [Key(13)]
    public int SegmentCount { get; init; }

    public GroundTrackGeneratedEvent() : base()
    {
        SourceService = "Visualization";
    }
}

[MessagePackObject]
public sealed class TimeSeriesGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid SeriesId { get; init; }

    [Key(11)]
    public string ParameterName { get; init; } = string.Empty;

    [Key(12)]
    public int PointCount { get; init; }

    public TimeSeriesGeneratedEvent() : base()
    {
        SourceService = "Visualization";
    }
}

[MessagePackObject]
public sealed class EclipseDataGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid DataId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public int EventCount { get; init; }

    [Key(13)]
    public double SunlitPercentage { get; init; }

    public EclipseDataGeneratedEvent() : base()
    {
        SourceService = "Visualization";
    }
}

[MessagePackObject]
public sealed class ConjunctionAnalyzedEvent : EventBase
{
    [Key(10)]
    public Guid AnalysisId { get; init; }

    [Key(11)]
    public Guid PrimarySpacecraftId { get; init; }

    [Key(12)]
    public Guid SecondarySpacecraftId { get; init; }

    [Key(13)]
    public double MinDistanceKm { get; init; }

    [Key(14)]
    public DateTime ClosestApproachTime { get; init; }

    public ConjunctionAnalyzedEvent() : base()
    {
        SourceService = "Visualization";
    }
}

[MessagePackObject]
public sealed class SceneExportedEvent : EventBase
{
    [Key(10)]
    public Guid ExportId { get; init; }

    [Key(11)]
    public string Format { get; init; } = string.Empty;

    [Key(12)]
    public string FileName { get; init; } = string.Empty;

    [Key(13)]
    public long FileSizeBytes { get; init; }

    public SceneExportedEvent() : base()
    {
        SourceService = "Visualization";
    }
}
