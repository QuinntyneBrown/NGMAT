using MissionManagement.Core.Entities;

namespace MissionManagement.Core.Models;

/// <summary>
/// Represents a mission in export format.
/// </summary>
public sealed class MissionExportData
{
    public required int Version { get; init; } = 1;
    public required string ExportedAt { get; init; }
    public required string ExportedBy { get; init; }
    public required MissionData Mission { get; init; }
}

/// <summary>
/// Core mission data for export/import.
/// </summary>
public sealed class MissionData
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Type { get; init; }
    public required string Status { get; init; }
    public required DateTime StartEpoch { get; init; }
    public DateTime? EndEpoch { get; init; }
    public DateTime? OriginalCreatedAt { get; init; }
}

/// <summary>
/// Represents a batch export of multiple missions.
/// </summary>
public sealed class MissionBatchExportData
{
    public required int Version { get; init; } = 1;
    public required string ExportedAt { get; init; }
    public required string ExportedBy { get; init; }
    public required int MissionCount { get; init; }
    public required List<MissionData> Missions { get; init; }
}

/// <summary>
/// Request to import a mission.
/// </summary>
public sealed class MissionImportRequest
{
    public required MissionData Mission { get; init; }
    public bool OverwriteExisting { get; init; } = false;
}

/// <summary>
/// Request to import multiple missions.
/// </summary>
public sealed class MissionBatchImportRequest
{
    public required List<MissionData> Missions { get; init; }
    public bool OverwriteExisting { get; init; } = false;
    public bool StopOnError { get; init; } = true;
}

/// <summary>
/// Result of a mission import operation.
/// </summary>
public sealed class MissionImportResult
{
    public required Guid MissionId { get; init; }
    public required string MissionName { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool WasOverwritten { get; init; }
}

/// <summary>
/// Result of a batch import operation.
/// </summary>
public sealed class MissionBatchImportResult
{
    public required int TotalCount { get; init; }
    public required int SuccessCount { get; init; }
    public required int FailureCount { get; init; }
    public required List<MissionImportResult> Results { get; init; }
}

/// <summary>
/// Extension methods for mission export/import.
/// </summary>
public static class MissionExportExtensions
{
    public static MissionData ToExportData(this Mission mission)
    {
        return new MissionData
        {
            Name = mission.Name,
            Description = mission.Description,
            Type = mission.Type.ToString(),
            Status = mission.Status.ToString(),
            StartEpoch = mission.StartEpoch,
            EndEpoch = mission.EndEpoch,
            OriginalCreatedAt = mission.CreatedAt
        };
    }

    public static (MissionType Type, bool Success) ParseMissionType(string typeString)
    {
        if (Enum.TryParse<MissionType>(typeString, ignoreCase: true, out var type))
        {
            return (type, true);
        }
        return (MissionType.Other, false);
    }

    public static (MissionStatus Status, bool Success) ParseMissionStatus(string statusString)
    {
        if (Enum.TryParse<MissionStatus>(statusString, ignoreCase: true, out var status))
        {
            return (status, true);
        }
        return (MissionStatus.Draft, false);
    }
}
