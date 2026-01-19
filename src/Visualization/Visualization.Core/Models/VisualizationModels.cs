namespace Visualization.Core.Models;

/// <summary>
/// 3D orbit visualization data
/// </summary>
public sealed class OrbitPlotData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public int PointCount { get; init; }
    public List<OrbitPoint3D> Points { get; init; } = new();
    public CentralBodyData CentralBody { get; init; } = new();
    public OrbitMetadata Metadata { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public sealed class OrbitPoint3D
{
    public DateTime Epoch { get; init; }
    public double X { get; init; }  // km
    public double Y { get; init; }  // km
    public double Z { get; init; }  // km
    public double Vx { get; init; } // km/s
    public double Vy { get; init; } // km/s
    public double Vz { get; init; } // km/s
    public double Altitude { get; init; } // km
    public double Speed { get; init; }    // km/s
}

public sealed class CentralBodyData
{
    public string Name { get; init; } = "Earth";
    public double RadiusKm { get; init; } = 6378.137;
    public double FlatteningFactor { get; init; } = 1.0 / 298.257223563;
    public string TextureUrl { get; init; } = string.Empty;
}

public sealed class OrbitMetadata
{
    public double SemiMajorAxisKm { get; init; }
    public double Eccentricity { get; init; }
    public double InclinationDeg { get; init; }
    public double RaanDeg { get; init; }
    public double ArgPeriapsisDeg { get; init; }
    public double PeriodMinutes { get; init; }
    public double ApogeeKm { get; init; }
    public double PerigeeKm { get; init; }
}

/// <summary>
/// Ground track (lat/lon) visualization data
/// </summary>
public sealed class GroundTrackData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public int PointCount { get; init; }
    public List<GroundTrackPoint> Points { get; init; } = new();
    public List<GroundTrackSegment> Segments { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public sealed class GroundTrackPoint
{
    public DateTime Epoch { get; init; }
    public double LatitudeDeg { get; init; }
    public double LongitudeDeg { get; init; }
    public double AltitudeKm { get; init; }
    public bool IsAscending { get; init; }
    public bool IsDaylit { get; init; }
}

public sealed class GroundTrackSegment
{
    public int StartIndex { get; init; }
    public int EndIndex { get; init; }
    public bool IsAscending { get; init; }
    public bool CrossesAntimeridian { get; init; }
}

/// <summary>
/// Time-series plot data
/// </summary>
public sealed class TimeSeriesData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? SpacecraftId { get; init; }
    public string ParameterName { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public int PointCount { get; init; }
    public List<TimeSeriesPoint> Points { get; init; } = new();
    public TimeSeriesStatistics Statistics { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public sealed class TimeSeriesPoint
{
    public DateTime Epoch { get; init; }
    public double Value { get; init; }
}

public sealed class TimeSeriesStatistics
{
    public double Min { get; init; }
    public double Max { get; init; }
    public double Mean { get; init; }
    public double StandardDeviation { get; init; }
}

/// <summary>
/// Orbital elements plot data
/// </summary>
public sealed class OrbitalElementsData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public int PointCount { get; init; }
    public List<OrbitalElementsPoint> Points { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public sealed class OrbitalElementsPoint
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
/// Eclipse visualization data
/// </summary>
public sealed class EclipseData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public List<EclipseEvent> Events { get; init; } = new();
    public List<EclipsePoint> Points { get; init; } = new();
    public EclipseSummary Summary { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public sealed class EclipseEvent
{
    public EclipseType Type { get; init; }
    public DateTime EntryTime { get; init; }
    public DateTime ExitTime { get; init; }
    public TimeSpan Duration => ExitTime - EntryTime;
}

public enum EclipseType
{
    Umbra,
    Penumbra
}

public sealed class EclipsePoint
{
    public DateTime Epoch { get; init; }
    public double SunVisibilityPercent { get; init; }  // 0-100
    public bool InUmbra { get; init; }
    public bool InPenumbra { get; init; }
    public double SunAngleDeg { get; init; }  // Angle from spacecraft to sun
}

public sealed class EclipseSummary
{
    public int TotalEclipseEvents { get; init; }
    public TimeSpan TotalUmbraDuration { get; init; }
    public TimeSpan TotalPenumbraDuration { get; init; }
    public double SunlitPercentage { get; init; }
    public TimeSpan MaxEclipseDuration { get; init; }
}

/// <summary>
/// Attitude visualization data
/// </summary>
public sealed class AttitudeData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SpacecraftId { get; init; }
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public int PointCount { get; init; }
    public List<AttitudePoint> Points { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public sealed class AttitudePoint
{
    public DateTime Epoch { get; init; }
    // Quaternion (scalar last convention)
    public double Qx { get; init; }
    public double Qy { get; init; }
    public double Qz { get; init; }
    public double Qw { get; init; }
    // Euler angles (deg)
    public double RollDeg { get; init; }
    public double PitchDeg { get; init; }
    public double YawDeg { get; init; }
    // Body vectors
    public double SunVectorX { get; init; }
    public double SunVectorY { get; init; }
    public double SunVectorZ { get; init; }
    public double EarthVectorX { get; init; }
    public double EarthVectorY { get; init; }
    public double EarthVectorZ { get; init; }
}

/// <summary>
/// Conjunction analysis visualization data
/// </summary>
public sealed class ConjunctionData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PrimarySpacecraftId { get; init; }
    public Guid SecondarySpacecraftId { get; init; }
    public DateTime StartEpoch { get; init; }
    public DateTime EndEpoch { get; init; }
    public List<ConjunctionPoint> Points { get; init; } = new();
    public ConjunctionEvent? ClosestApproach { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public sealed class ConjunctionPoint
{
    public DateTime Epoch { get; init; }
    public double RelativeX { get; init; }  // km
    public double RelativeY { get; init; }  // km
    public double RelativeZ { get; init; }  // km
    public double DistanceKm { get; init; }
    public double RelativeSpeedKmps { get; init; }
}

public sealed class ConjunctionEvent
{
    public DateTime Epoch { get; init; }
    public double MinDistanceKm { get; init; }
    public double RelativeSpeedKmps { get; init; }
    public double? CollisionProbability { get; init; }  // If covariance available
    public double TimeToClosestApproachSeconds { get; init; }
}

/// <summary>
/// 3D export format options
/// </summary>
public enum ExportFormat
{
    Gltf,
    Obj,
    Json
}

/// <summary>
/// 3D scene export data
/// </summary>
public sealed class SceneExportData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public ExportFormat Format { get; init; }
    public byte[] Data { get; init; } = Array.Empty<byte>();
    public string MimeType { get; init; } = "application/json";
    public string FileName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}
