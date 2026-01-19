using System.Text;
using System.Text.Json;
using Shared.Domain.Results;
using Reporting.Core.Models;
using Reporting.Core.Services.Pdf;

namespace Reporting.Core.Services;

public sealed class ReportingService
{
    private const double EarthRadiusKm = 6378.137;
    private const double EarthGM = 398600.4418;
    private readonly PdfGenerator _pdfGenerator;

    public ReportingService()
    {
        _pdfGenerator = new PdfGenerator();
    }

    /// <summary>
    /// Generate a mission report
    /// </summary>
    public Result<MissionReport> GenerateMissionReport(
        Guid missionId,
        string missionName,
        ReportFormat format,
        string? description = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var report = new MissionReport
        {
            MissionId = missionId,
            MissionName = missionName,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Title = $"Mission Report: {missionName}",
            Format = format,
            Status = ReportStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            FileName = $"mission_report_{missionId:N}_{DateTime.UtcNow:yyyyMMddHHmmss}.{FormatToExtension(format)}",
            ContentType = FormatToContentType(format)
        };

        // Generate report content
        report.Content = format switch
        {
            ReportFormat.Pdf => _pdfGenerator.GenerateMissionReportPdf(report),
            ReportFormat.Json => GenerateJsonContent(report),
            ReportFormat.Csv => GenerateCsvMissionContent(report),
            ReportFormat.Html => GenerateHtmlMissionContent(report),
            ReportFormat.Markdown => GenerateMarkdownMissionContent(report),
            _ => GenerateJsonContent(report)
        };

        return Result<MissionReport>.Success(report);
    }

    /// <summary>
    /// Export state vectors
    /// </summary>
    public Result<StateVectorExport> ExportStateVectors(
        Guid spacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds,
        ReportFormat format,
        string coordinateSystem = "J2000",
        double[]? initialState = null)
    {
        if (endEpoch <= startEpoch)
        {
            return Result<StateVectorExport>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var records = new List<StateVectorRecord>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        var currentEpoch = startEpoch;
        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagatedState = PropagateKepler(state, elapsedSeconds);

            records.Add(new StateVectorRecord
            {
                Epoch = currentEpoch,
                X = propagatedState[0],
                Y = propagatedState[1],
                Z = propagatedState[2],
                Vx = propagatedState[3],
                Vy = propagatedState[4],
                Vz = propagatedState[5]
            });

            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        var export = new StateVectorExport
        {
            SpacecraftId = spacecraftId,
            CoordinateSystem = coordinateSystem,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            Format = format,
            Records = records
        };

        return Result<StateVectorExport>.Success(export);
    }

    /// <summary>
    /// Export orbital elements
    /// </summary>
    public Result<OrbitalElementsExport> ExportOrbitalElements(
        Guid spacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds,
        ReportFormat format,
        double[]? initialState = null)
    {
        if (endEpoch <= startEpoch)
        {
            return Result<OrbitalElementsExport>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var records = new List<OrbitalElementsRecord>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        var currentEpoch = startEpoch;
        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagatedState = PropagateKepler(state, elapsedSeconds);
            var elements = StateToElements(propagatedState);

            records.Add(new OrbitalElementsRecord
            {
                Epoch = currentEpoch,
                SemiMajorAxisKm = elements.a,
                Eccentricity = elements.e,
                InclinationDeg = elements.i * 180.0 / Math.PI,
                RaanDeg = elements.raan * 180.0 / Math.PI,
                ArgPeriapsisDeg = elements.omega * 180.0 / Math.PI,
                TrueAnomalyDeg = elements.nu * 180.0 / Math.PI,
                MeanAnomalyDeg = TrueToMeanAnomaly(elements.nu, elements.e) * 180.0 / Math.PI
            });

            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        var export = new OrbitalElementsExport
        {
            SpacecraftId = spacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            Format = format,
            Records = records
        };

        return Result<OrbitalElementsExport>.Success(export);
    }

    /// <summary>
    /// Generate Two-Line Element set
    /// </summary>
    public Result<TleData> GenerateTle(
        Guid spacecraftId,
        string spacecraftName,
        DateTime epoch,
        double[]? state = null,
        int? noradNumber = null)
    {
        var actualState = state ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };
        var elements = StateToElements(actualState);

        // Convert to TLE mean elements (simplified - using osculating)
        var meanMotion = Math.Sqrt(EarthGM / Math.Pow(elements.a, 3)) * 86400 / (2 * Math.PI); // rev/day

        var tle = new TleData
        {
            SpacecraftId = spacecraftId,
            SpacecraftName = spacecraftName.Length > 24 ? spacecraftName[..24] : spacecraftName,
            NoradCatalogNumber = noradNumber,
            Epoch = epoch,
            InclinationDeg = elements.i * 180.0 / Math.PI,
            RaanDeg = elements.raan * 180.0 / Math.PI,
            Eccentricity = elements.e,
            ArgPerigeeDeg = elements.omega * 180.0 / Math.PI,
            MeanAnomalyDeg = TrueToMeanAnomaly(elements.nu, elements.e) * 180.0 / Math.PI,
            MeanMotion = meanMotion,
            MeanMotionRevPerDay = meanMotion,
            MeanMotionDerivative = 0,
            MeanMotionSecondDerivative = 0,
            Bstar = 0.0001,
            RevolutionNumber = 1
        };

        // Generate actual TLE lines
        tle.Line1 = GenerateTleLine1(tle);
        tle.Line2 = GenerateTleLine2(tle);

        return Result<TleData>.Success(tle);
    }

    /// <summary>
    /// Generate delta-V budget report
    /// </summary>
    public Result<DeltaVBudget> GenerateDeltaVBudget(
        Guid missionId,
        string missionName,
        double initialFuelKg,
        double remainingFuelKg,
        List<DeltaVManeuver>? maneuvers = null,
        ReportFormat format = ReportFormat.Json)
    {
        var budget = new DeltaVBudget
        {
            MissionId = missionId,
            MissionName = missionName,
            InitialFuelKg = initialFuelKg,
            RemainingFuelKg = remainingFuelKg,
            Maneuvers = maneuvers ?? new List<DeltaVManeuver>(),
            Format = format
        };

        return Result<DeltaVBudget>.Success(budget);
    }

    /// <summary>
    /// Generate event timeline
    /// </summary>
    public Result<EventTimeline> GenerateEventTimeline(
        Guid missionId,
        string missionName,
        DateTime startEpoch,
        DateTime endEpoch,
        ReportFormat format,
        double[]? initialState = null)
    {
        var events = new List<TimelineEvent>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        // Generate sample events (apsis, node crossings, etc.)
        var elements = StateToElements(state);
        var period = 2 * Math.PI * Math.Sqrt(Math.Pow(elements.a, 3) / EarthGM);

        var currentEpoch = startEpoch;
        int orbitCount = 0;
        while (currentEpoch <= endEpoch && orbitCount < 100)
        {
            // Periapsis
            events.Add(new TimelineEvent
            {
                Epoch = currentEpoch,
                EventType = "Periapsis",
                Description = $"Orbit {orbitCount + 1} periapsis passage",
                Data = new Dictionary<string, object>
                {
                    ["AltitudeKm"] = elements.a * (1 - elements.e) - EarthRadiusKm
                }
            });

            // Apoapsis (half period later)
            var apoapsisTime = currentEpoch.AddSeconds(period / 2);
            if (apoapsisTime <= endEpoch)
            {
                events.Add(new TimelineEvent
                {
                    Epoch = apoapsisTime,
                    EventType = "Apoapsis",
                    Description = $"Orbit {orbitCount + 1} apoapsis passage",
                    Data = new Dictionary<string, object>
                    {
                        ["AltitudeKm"] = elements.a * (1 + elements.e) - EarthRadiusKm
                    }
                });
            }

            currentEpoch = currentEpoch.AddSeconds(period);
            orbitCount++;
        }

        // Sort events by epoch
        events = events.OrderBy(e => e.Epoch).ToList();

        var timeline = new EventTimeline
        {
            MissionId = missionId,
            MissionName = missionName,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            Format = format,
            Events = events
        };

        return Result<EventTimeline>.Success(timeline);
    }

    /// <summary>
    /// Generate conjunction report
    /// </summary>
    public Result<ConjunctionReport> GenerateConjunctionReport(
        Guid spacecraftId,
        string spacecraftName,
        DateTime startEpoch,
        DateTime endEpoch,
        ReportFormat format,
        double[]? primaryState = null,
        List<(Guid id, string name, double[] state)>? secondaryObjects = null)
    {
        var events = new List<ConjunctionEvent>();
        var state1 = primaryState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        // Use default secondary objects if none provided
        var secondaries = secondaryObjects ?? new List<(Guid, string, double[])>
        {
            (Guid.NewGuid(), "Debris-1", new double[] { 7050, 50, 0, 0, 7.5, 0.1 }),
            (Guid.NewGuid(), "Debris-2", new double[] { 6950, -30, 20, 0.1, 7.6, -0.05 })
        };

        // Check for close approaches
        foreach (var secondary in secondaries)
        {
            var (minDistance, tcaEpoch, relVel) = FindClosestApproach(
                state1, secondary.state, startEpoch, endEpoch);

            if (minDistance < 10.0) // Report if within 10 km
            {
                var riskLevel = minDistance switch
                {
                    < 0.1 => "Critical",
                    < 0.5 => "High",
                    < 2.0 => "Medium",
                    _ => "Low"
                };

                events.Add(new ConjunctionEvent
                {
                    TimeOfClosestApproach = tcaEpoch,
                    SecondaryObjectId = secondary.id,
                    SecondaryObjectName = secondary.name,
                    MissDistanceKm = minDistance,
                    RelativeVelocityKmps = relVel,
                    RiskLevel = riskLevel
                });
            }
        }

        // Sort by TCA
        events = events.OrderBy(e => e.TimeOfClosestApproach).ToList();

        var report = new ConjunctionReport
        {
            SpacecraftId = spacecraftId,
            SpacecraftName = spacecraftName,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            Format = format,
            Events = events
        };

        return Result<ConjunctionReport>.Success(report);
    }

    /// <summary>
    /// Convert state vector export to bytes
    /// </summary>
    public byte[] ExportToBytes(StateVectorExport export)
    {
        return export.Format switch
        {
            ReportFormat.Csv => GenerateCsvStateVectors(export),
            ReportFormat.Json => JsonSerializer.SerializeToUtf8Bytes(export, new JsonSerializerOptions { WriteIndented = true }),
            ReportFormat.Xml => GenerateXmlStateVectors(export),
            _ => JsonSerializer.SerializeToUtf8Bytes(export)
        };
    }

    /// <summary>
    /// Convert orbital elements export to bytes
    /// </summary>
    public byte[] ExportToBytes(OrbitalElementsExport export)
    {
        return export.Format switch
        {
            ReportFormat.Csv => GenerateCsvOrbitalElements(export),
            ReportFormat.Json => JsonSerializer.SerializeToUtf8Bytes(export, new JsonSerializerOptions { WriteIndented = true }),
            _ => JsonSerializer.SerializeToUtf8Bytes(export)
        };
    }

    /// <summary>
    /// Convert delta-V budget to bytes
    /// </summary>
    public byte[] ExportToBytes(DeltaVBudget budget)
    {
        return budget.Format switch
        {
            ReportFormat.Pdf => _pdfGenerator.GenerateDeltaVBudgetPdf(budget),
            ReportFormat.Json => JsonSerializer.SerializeToUtf8Bytes(budget, new JsonSerializerOptions { WriteIndented = true }),
            _ => JsonSerializer.SerializeToUtf8Bytes(budget)
        };
    }

    // Helper methods

    private string GenerateTleLine1(TleData tle)
    {
        var norad = tle.NoradCatalogNumber?.ToString("D5") ?? "99999";
        var classification = tle.Classification;
        var intlDesig = tle.InternationalDesignator.PadRight(8)[..8];

        // Epoch: year (2 digits) + day of year with fractional days
        var epochYear = tle.Epoch.Year % 100;
        var dayOfYear = tle.Epoch.DayOfYear +
            (tle.Epoch.Hour + tle.Epoch.Minute / 60.0 + tle.Epoch.Second / 3600.0) / 24.0;

        var meanMotionDeriv = tle.MeanMotionDerivative;
        var meanMotionSecDeriv = tle.MeanMotionSecondDerivative;
        var bstar = tle.Bstar;

        var line1 = $"1 {norad}{classification} {intlDesig} {epochYear:D2}{dayOfYear:000.00000000} " +
                    $"{FormatMeanMotionDeriv(meanMotionDeriv)} {FormatExponent(meanMotionSecDeriv)} " +
                    $"{FormatExponent(bstar)} 0 {tle.ElementSetNumber:D4}";

        // Add checksum
        var checksum = CalculateChecksum(line1);
        return line1.PadRight(68)[..68] + checksum;
    }

    private string GenerateTleLine2(TleData tle)
    {
        var norad = tle.NoradCatalogNumber?.ToString("D5") ?? "99999";

        var line2 = $"2 {norad} " +
                    $"{tle.InclinationDeg:000.0000} " +
                    $"{tle.RaanDeg:000.0000} " +
                    $"{(tle.Eccentricity * 10000000):0000000} " +
                    $"{tle.ArgPerigeeDeg:000.0000} " +
                    $"{tle.MeanAnomalyDeg:000.0000} " +
                    $"{tle.MeanMotion:00.00000000}" +
                    $"{tle.RevolutionNumber:D5}";

        var checksum = CalculateChecksum(line2);
        return line2.PadRight(68)[..68] + checksum;
    }

    private string FormatMeanMotionDeriv(double value)
    {
        var sign = value >= 0 ? " " : "-";
        var absValue = Math.Abs(value);
        return $"{sign}.{(absValue * 100000000):00000000}";
    }

    private string FormatExponent(double value)
    {
        if (Math.Abs(value) < 1e-10) return " 00000-0";
        var exp = (int)Math.Floor(Math.Log10(Math.Abs(value)));
        var mantissa = value / Math.Pow(10, exp);
        var sign = mantissa >= 0 ? " " : "-";
        return $"{sign}{Math.Abs(mantissa) * 10000:00000}{(exp >= 0 ? "+" : "-")}{Math.Abs(exp)}";
    }

    private int CalculateChecksum(string line)
    {
        int sum = 0;
        foreach (char c in line)
        {
            if (char.IsDigit(c)) sum += c - '0';
            else if (c == '-') sum += 1;
        }
        return sum % 10;
    }

    private string FormatToExtension(ReportFormat format) => format switch
    {
        ReportFormat.Pdf => "pdf",
        ReportFormat.Html => "html",
        ReportFormat.Markdown => "md",
        ReportFormat.Csv => "csv",
        ReportFormat.Json => "json",
        ReportFormat.Xml => "xml",
        _ => "txt"
    };

    private string FormatToContentType(ReportFormat format) => format switch
    {
        ReportFormat.Pdf => "application/pdf",
        ReportFormat.Html => "text/html",
        ReportFormat.Markdown => "text/markdown",
        ReportFormat.Csv => "text/csv",
        ReportFormat.Json => "application/json",
        ReportFormat.Xml => "application/xml",
        _ => "application/octet-stream"
    };

    private byte[] GenerateJsonContent(object report) =>
        JsonSerializer.SerializeToUtf8Bytes(report, new JsonSerializerOptions { WriteIndented = true });

    private byte[] GenerateCsvMissionContent(MissionReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Mission Report");
        sb.AppendLine($"ID,{report.MissionId}");
        sb.AppendLine($"Name,{report.MissionName}");
        sb.AppendLine($"Description,{report.Description}");
        sb.AppendLine($"Start Date,{report.StartDate}");
        sb.AppendLine($"End Date,{report.EndDate}");
        sb.AppendLine();
        sb.AppendLine("Spacecraft");
        sb.AppendLine("ID,Name,Type,Dry Mass (kg),Initial Fuel (kg),Current Fuel (kg)");
        foreach (var sc in report.Spacecraft)
        {
            sb.AppendLine($"{sc.Id},{sc.Name},{sc.Type},{sc.DryMassKg},{sc.InitialFuelMassKg},{sc.CurrentFuelMassKg}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private byte[] GenerateHtmlMissionContent(MissionReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><title>Mission Report</title>");
        sb.AppendLine("<style>body{font-family:Arial;margin:20px;}table{border-collapse:collapse;width:100%;}");
        sb.AppendLine("th,td{border:1px solid #ddd;padding:8px;text-align:left;}th{background:#f4f4f4;}</style></head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h1>Mission Report: {report.MissionName}</h1>");
        sb.AppendLine($"<p><strong>ID:</strong> {report.MissionId}</p>");
        sb.AppendLine($"<p><strong>Description:</strong> {report.Description ?? "N/A"}</p>");
        sb.AppendLine($"<p><strong>Period:</strong> {report.StartDate} to {report.EndDate}</p>");
        sb.AppendLine($"<p><strong>Generated:</strong> {report.CreatedAt}</p>");
        sb.AppendLine("</body></html>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private byte[] GenerateMarkdownMissionContent(MissionReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Mission Report: {report.MissionName}");
        sb.AppendLine();
        sb.AppendLine($"**ID:** {report.MissionId}");
        sb.AppendLine();
        sb.AppendLine($"**Description:** {report.Description ?? "N/A"}");
        sb.AppendLine();
        sb.AppendLine($"**Period:** {report.StartDate} to {report.EndDate}");
        sb.AppendLine();
        sb.AppendLine($"**Generated:** {report.CreatedAt}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private byte[] GenerateCsvStateVectors(StateVectorExport export)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Epoch,X (km),Y (km),Z (km),Vx (km/s),Vy (km/s),Vz (km/s)");
        foreach (var rec in export.Records)
        {
            sb.AppendLine($"{rec.Epoch:O},{rec.X:F6},{rec.Y:F6},{rec.Z:F6},{rec.Vx:F9},{rec.Vy:F9},{rec.Vz:F9}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private byte[] GenerateXmlStateVectors(StateVectorExport export)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<StateVectorExport SpacecraftId=\"{export.SpacecraftId}\" CoordinateSystem=\"{export.CoordinateSystem}\">");
        foreach (var rec in export.Records)
        {
            sb.AppendLine($"  <Record Epoch=\"{rec.Epoch:O}\">");
            sb.AppendLine($"    <Position X=\"{rec.X}\" Y=\"{rec.Y}\" Z=\"{rec.Z}\" unit=\"km\"/>");
            sb.AppendLine($"    <Velocity Vx=\"{rec.Vx}\" Vy=\"{rec.Vy}\" Vz=\"{rec.Vz}\" unit=\"km/s\"/>");
            sb.AppendLine("  </Record>");
        }
        sb.AppendLine("</StateVectorExport>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private byte[] GenerateCsvOrbitalElements(OrbitalElementsExport export)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Epoch,SMA (km),Eccentricity,Inclination (deg),RAAN (deg),AoP (deg),True Anomaly (deg),Mean Anomaly (deg)");
        foreach (var rec in export.Records)
        {
            sb.AppendLine($"{rec.Epoch:O},{rec.SemiMajorAxisKm:F3},{rec.Eccentricity:F9},{rec.InclinationDeg:F6},{rec.RaanDeg:F6},{rec.ArgPeriapsisDeg:F6},{rec.TrueAnomalyDeg:F6},{rec.MeanAnomalyDeg:F6}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private (double distance, DateTime epoch, double relVel) FindClosestApproach(
        double[] state1, double[] state2, DateTime start, DateTime end)
    {
        double minDist = double.MaxValue;
        DateTime minEpoch = start;
        double minRelVel = 0;

        var current = start;
        while (current <= end)
        {
            var elapsed = (current - start).TotalSeconds;
            var prop1 = PropagateKepler(state1, elapsed);
            var prop2 = PropagateKepler(state2, elapsed);

            var dx = prop2[0] - prop1[0];
            var dy = prop2[1] - prop1[1];
            var dz = prop2[2] - prop1[2];
            var dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (dist < minDist)
            {
                minDist = dist;
                minEpoch = current;

                var dvx = prop2[3] - prop1[3];
                var dvy = prop2[4] - prop1[4];
                var dvz = prop2[5] - prop1[5];
                minRelVel = Math.Sqrt(dvx * dvx + dvy * dvy + dvz * dvz);
            }

            current = current.AddSeconds(60); // 1-minute steps
        }

        return (minDist, minEpoch, minRelVel);
    }

    // Propagation helpers (simplified Kepler)

    private double[] PropagateKepler(double[] state, double dt)
    {
        var r = new double[] { state[0], state[1], state[2] };
        var v = new double[] { state[3], state[4], state[5] };

        var rMag = Math.Sqrt(r[0] * r[0] + r[1] * r[1] + r[2] * r[2]);
        var vMag = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);

        var energy = vMag * vMag / 2 - EarthGM / rMag;
        var a = -EarthGM / (2 * energy);

        var h = new double[]
        {
            r[1] * v[2] - r[2] * v[1],
            r[2] * v[0] - r[0] * v[2],
            r[0] * v[1] - r[1] * v[0]
        };

        var eVec = new double[]
        {
            (v[1] * h[2] - v[2] * h[1]) / EarthGM - r[0] / rMag,
            (v[2] * h[0] - v[0] * h[2]) / EarthGM - r[1] / rMag,
            (v[0] * h[1] - v[1] * h[0]) / EarthGM - r[2] / rMag
        };
        var e = Math.Sqrt(eVec[0] * eVec[0] + eVec[1] * eVec[1] + eVec[2] * eVec[2]);

        var n = Math.Sqrt(EarthGM / (a * a * a));

        var cosNu = (a * (1 - e * e) - rMag) / (e * rMag);
        cosNu = Math.Max(-1, Math.Min(1, cosNu));
        var nu0 = Math.Acos(cosNu);
        if (r[0] * v[0] + r[1] * v[1] + r[2] * v[2] < 0) nu0 = 2 * Math.PI - nu0;

        var E0 = 2 * Math.Atan(Math.Sqrt((1 - e) / (1 + e)) * Math.Tan(nu0 / 2));
        var M0 = E0 - e * Math.Sin(E0);

        var M = M0 + n * dt;
        M = M % (2 * Math.PI);
        if (M < 0) M += 2 * Math.PI;

        var E = SolveKepler(M, e);
        var nu = 2 * Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(E / 2));

        var angle = nu - nu0;
        var cosA = Math.Cos(angle);
        var sinA = Math.Sin(angle);
        var hSign = Math.Sign(h[2]);
        if (hSign == 0) hSign = 1;

        return new double[]
        {
            r[0] * cosA - r[1] * sinA * hSign,
            r[0] * sinA * hSign + r[1] * cosA,
            r[2],
            v[0] * cosA - v[1] * sinA * hSign,
            v[0] * sinA * hSign + v[1] * cosA,
            v[2]
        };
    }

    private double SolveKepler(double M, double e)
    {
        var E = M;
        for (int i = 0; i < 50; i++)
        {
            var f = E - e * Math.Sin(E) - M;
            var df = 1 - e * Math.Cos(E);
            var dE = -f / df;
            E += dE;
            if (Math.Abs(dE) < 1e-12) break;
        }
        return E;
    }

    private (double a, double e, double i, double raan, double omega, double nu) StateToElements(double[] state)
    {
        var r = new double[] { state[0], state[1], state[2] };
        var v = new double[] { state[3], state[4], state[5] };

        var rMag = Math.Sqrt(r[0] * r[0] + r[1] * r[1] + r[2] * r[2]);
        var vMag = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);

        var energy = vMag * vMag / 2 - EarthGM / rMag;
        var a = -EarthGM / (2 * energy);

        var h = new double[]
        {
            r[1] * v[2] - r[2] * v[1],
            r[2] * v[0] - r[0] * v[2],
            r[0] * v[1] - r[1] * v[0]
        };
        var hMag = Math.Sqrt(h[0] * h[0] + h[1] * h[1] + h[2] * h[2]);

        var i = Math.Acos(h[2] / hMag);

        var n = new double[] { -h[1], h[0], 0 };
        var nMag = Math.Sqrt(n[0] * n[0] + n[1] * n[1]);

        var raan = nMag > 1e-10 ? Math.Acos(n[0] / nMag) : 0;
        if (n[1] < 0) raan = 2 * Math.PI - raan;

        var eVec = new double[]
        {
            (v[1] * h[2] - v[2] * h[1]) / EarthGM - r[0] / rMag,
            (v[2] * h[0] - v[0] * h[2]) / EarthGM - r[1] / rMag,
            (v[0] * h[1] - v[1] * h[0]) / EarthGM - r[2] / rMag
        };
        var e = Math.Sqrt(eVec[0] * eVec[0] + eVec[1] * eVec[1] + eVec[2] * eVec[2]);

        var omega = 0.0;
        if (nMag > 1e-10 && e > 1e-10)
        {
            omega = Math.Acos((n[0] * eVec[0] + n[1] * eVec[1]) / (nMag * e));
            if (eVec[2] < 0) omega = 2 * Math.PI - omega;
        }

        var nu = 0.0;
        if (e > 1e-10)
        {
            nu = Math.Acos((eVec[0] * r[0] + eVec[1] * r[1] + eVec[2] * r[2]) / (e * rMag));
            if (r[0] * v[0] + r[1] * v[1] + r[2] * v[2] < 0) nu = 2 * Math.PI - nu;
        }

        return (a, e, i, raan, omega, nu);
    }

    private double TrueToMeanAnomaly(double nu, double e)
    {
        var E = 2 * Math.Atan(Math.Sqrt((1 - e) / (1 + e)) * Math.Tan(nu / 2));
        return E - e * Math.Sin(E);
    }
}
