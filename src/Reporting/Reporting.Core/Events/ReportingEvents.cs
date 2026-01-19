using MessagePack;
using Shared.Messaging.Abstractions;

namespace Reporting.Core.Events;

[MessagePackObject]
public sealed class ReportGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid ReportId { get; init; }

    [Key(11)]
    public string ReportType { get; init; } = string.Empty;

    [Key(12)]
    public string Format { get; init; } = string.Empty;

    [Key(13)]
    public string FileName { get; init; } = string.Empty;

    [Key(14)]
    public long FileSizeBytes { get; init; }

    public ReportGeneratedEvent() : base()
    {
        SourceService = "Reporting";
    }
}

[MessagePackObject]
public sealed class StateVectorsExportedEvent : EventBase
{
    [Key(10)]
    public Guid ExportId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public string Format { get; init; } = string.Empty;

    [Key(13)]
    public int RecordCount { get; init; }

    public StateVectorsExportedEvent() : base()
    {
        SourceService = "Reporting";
    }
}

[MessagePackObject]
public sealed class TleGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid TleId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public string SpacecraftName { get; init; } = string.Empty;

    [Key(13)]
    public int? NoradNumber { get; init; }

    public TleGeneratedEvent() : base()
    {
        SourceService = "Reporting";
    }
}

[MessagePackObject]
public sealed class DeltaVBudgetGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid ReportId { get; init; }

    [Key(11)]
    public Guid MissionId { get; init; }

    [Key(12)]
    public double TotalDeltaVMps { get; init; }

    [Key(13)]
    public double TotalFuelUsedKg { get; init; }

    [Key(14)]
    public double RemainingFuelKg { get; init; }

    public DeltaVBudgetGeneratedEvent() : base()
    {
        SourceService = "Reporting";
    }
}

[MessagePackObject]
public sealed class EventTimelineGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid TimelineId { get; init; }

    [Key(11)]
    public Guid MissionId { get; init; }

    [Key(12)]
    public int EventCount { get; init; }

    public EventTimelineGeneratedEvent() : base()
    {
        SourceService = "Reporting";
    }
}

[MessagePackObject]
public sealed class ConjunctionReportGeneratedEvent : EventBase
{
    [Key(10)]
    public Guid ReportId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public int ConjunctionEventCount { get; init; }

    [Key(13)]
    public double? MinMissDistanceKm { get; init; }

    public ConjunctionReportGeneratedEvent() : base()
    {
        SourceService = "Reporting";
    }
}

[MessagePackObject]
public sealed class ScheduledReportCreatedEvent : EventBase
{
    [Key(10)]
    public Guid ScheduleId { get; init; }

    [Key(11)]
    public string ReportType { get; init; } = string.Empty;

    [Key(12)]
    public string CronExpression { get; init; } = string.Empty;

    public ScheduledReportCreatedEvent() : base()
    {
        SourceService = "Reporting";
    }
}
