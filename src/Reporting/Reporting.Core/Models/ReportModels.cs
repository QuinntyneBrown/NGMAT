namespace Reporting.Core.Models;

/// <summary>
/// Report output format
/// </summary>
public enum ReportFormat
{
    Pdf,
    Html,
    Markdown,
    Csv,
    Json,
    Xml
}

/// <summary>
/// Report generation status
/// </summary>
public enum ReportStatus
{
    Pending,
    Generating,
    Completed,
    Failed
}

/// <summary>
/// Base class for all reports
/// </summary>
public abstract class ReportBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; init; } = string.Empty;
    public ReportFormat Format { get; init; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public byte[]? Content { get; set; }
    public string ContentType { get; init; } = "application/octet-stream";
    public string FileName { get; init; } = string.Empty;
}

/// <summary>
/// Mission report
/// </summary>
public sealed class MissionReport : ReportBase
{
    public Guid MissionId { get; init; }
    public string MissionName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public List<SpacecraftSummary> Spacecraft { get; init; } = new();
    public List<ManeuverSummary> Maneuvers { get; init; } = new();
    public DeltaVBudget? DeltaVBudget { get; init; }
    public List<OrbitSummary> Orbits { get; init; } = new();
}

public sealed class SpacecraftSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public double DryMassKg { get; init; }
    public double InitialFuelMassKg { get; init; }
    public double CurrentFuelMassKg { get; init; }
}

public sealed class ManeuverSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public DateTime PlannedEpoch { get; init; }
    public DateTime? ExecutedEpoch { get; init; }
    public double DeltaVMps { get; init; }
    public double FuelUsedKg { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class OrbitSummary
{
    public Guid SpacecraftId { get; init; }
    public DateTime Epoch { get; init; }
    public double SemiMajorAxisKm { get; init; }
    public double Eccentricity { get; init; }
    public double InclinationDeg { get; init; }
    public double PeriodMinutes { get; init; }
    public double ApogeeKm { get; init; }
    public double PerigeeKm { get; init; }
}

/// <summary>
/// State vector export
/// </summary>
public sealed class StateVectorExport
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public string CoordinateSystem { get; init; } = "J2000";
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public ReportFormat Format { get; init; }
    public List<StateVectorRecord> Records { get; init; } = new();
}

public sealed class StateVectorRecord
{
    public DateTime Epoch { get; init; }
    public double X { get; init; }   // km
    public double Y { get; init; }   // km
    public double Z { get; init; }   // km
    public double Vx { get; init; }  // km/s
    public double Vy { get; init; }  // km/s
    public double Vz { get; init; }  // km/s
}

/// <summary>
/// Orbital elements export
/// </summary>
public sealed class OrbitalElementsExport
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public ReportFormat Format { get; init; }
    public List<OrbitalElementsRecord> Records { get; init; } = new();
}

public sealed class OrbitalElementsRecord
{
    public DateTime Epoch { get; init; }
    public double SemiMajorAxisKm { get; init; }
    public double Eccentricity { get; init; }
    public double InclinationDeg { get; init; }
    public double RaanDeg { get; init; }
    public double ArgPeriapsisDeg { get; init; }
    public double TrueAnomalyDeg { get; init; }
    public double MeanAnomalyDeg { get; init; }
}

/// <summary>
/// Two-Line Element set
/// </summary>
public sealed class TleData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public string SpacecraftName { get; init; } = string.Empty;
    public int? NoradCatalogNumber { get; init; }
    public string Classification { get; init; } = "U";  // U = Unclassified
    public string InternationalDesignator { get; init; } = string.Empty;
    public DateTime Epoch { get; init; }
    public double MeanMotionRevPerDay { get; init; }
    public double MeanMotionDerivative { get; init; }
    public double MeanMotionSecondDerivative { get; init; }
    public double Bstar { get; init; }
    public int ElementSetNumber { get; init; } = 999;
    public double InclinationDeg { get; init; }
    public double RaanDeg { get; init; }
    public double Eccentricity { get; init; }
    public double ArgPerigeeDeg { get; init; }
    public double MeanAnomalyDeg { get; init; }
    public double MeanMotion { get; init; }
    public int RevolutionNumber { get; init; }
    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
}

/// <summary>
/// Delta-V budget report
/// </summary>
public sealed class DeltaVBudget
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid MissionId { get; init; }
    public string MissionName { get; init; } = string.Empty;
    public ReportFormat Format { get; init; }
    public List<DeltaVManeuver> Maneuvers { get; init; } = new();
    public double TotalDeltaVMps => Maneuvers.Sum(m => m.DeltaVMps);
    public double TotalFuelUsedKg => Maneuvers.Sum(m => m.FuelUsedKg);
    public double RemainingFuelKg { get; init; }
    public double InitialFuelKg { get; init; }
    public double FuelMarginPercent => InitialFuelKg > 0 ? RemainingFuelKg / InitialFuelKg * 100 : 0;
}

public sealed class DeltaVManeuver
{
    public int Sequence { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime Epoch { get; init; }
    public string Type { get; init; } = string.Empty;
    public double DeltaVMps { get; init; }
    public double FuelUsedKg { get; init; }
    public double CumulativeDeltaVMps { get; init; }
    public double CumulativeFuelUsedKg { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Event timeline report
/// </summary>
public sealed class EventTimeline
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid MissionId { get; init; }
    public string MissionName { get; init; } = string.Empty;
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public ReportFormat Format { get; init; }
    public List<TimelineEvent> Events { get; init; } = new();
}

public sealed class TimelineEvent
{
    public DateTime Epoch { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? SpacecraftId { get; init; }
    public string? SpacecraftName { get; init; }
    public Dictionary<string, object> Data { get; init; } = new();
}

/// <summary>
/// Conjunction report
/// </summary>
public sealed class ConjunctionReport
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public string SpacecraftName { get; init; } = string.Empty;
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public ReportFormat Format { get; init; }
    public List<ConjunctionEvent> Events { get; init; } = new();
}

public sealed class ConjunctionEvent
{
    public DateTime TimeOfClosestApproach { get; init; }
    public Guid SecondaryObjectId { get; init; }
    public string SecondaryObjectName { get; init; } = string.Empty;
    public double MissDistanceKm { get; init; }
    public double RelativeVelocityKmps { get; init; }
    public double? CollisionProbability { get; init; }
    public string RiskLevel { get; init; } = "Low";  // Low, Medium, High, Critical
}

/// <summary>
/// Scheduled report configuration
/// </summary>
public sealed class ScheduledReport
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public string ReportType { get; init; } = string.Empty;  // Mission, DeltaV, Timeline, Conjunction
    public Dictionary<string, object> Parameters { get; init; } = new();
    public string CronExpression { get; init; } = string.Empty;  // e.g., "0 0 * * *" for daily
    public bool IsEnabled { get; init; } = true;
    public List<string> EmailRecipients { get; init; } = new();
    public string? WebhookUrl { get; init; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
