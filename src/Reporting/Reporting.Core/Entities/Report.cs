namespace Reporting.Core.Entities;

/// <summary>
/// Report entity for database persistence
/// </summary>
public sealed class Report
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Format { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    
    // Related entity IDs
    public Guid? MissionId { get; private set; }
    public Guid? SpacecraftId { get; private set; }
    
    // File information
    public string? FileName { get; private set; }
    public string? ContentType { get; private set; }
    public string? StoragePath { get; private set; }
    public long? FileSizeBytes { get; private set; }
    
    // Generation parameters (stored as JSON)
    public string? Parameters { get; private set; }
    
    // Metadata
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string CreatedByUserId { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public bool IsDeleted { get; private set; }

    private Report() { }

    public static Report Create(
        string name,
        string type,
        string format,
        string createdByUserId,
        string? description = null,
        Guid? missionId = null,
        Guid? spacecraftId = null,
        string? parameters = null)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            Format = format,
            Status = "Pending",
            MissionId = missionId,
            SpacecraftId = spacecraftId,
            Parameters = parameters,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public void MarkAsGenerating()
    {
        Status = "Generating";
    }

    public void MarkAsCompleted(string fileName, string contentType, string storagePath, long fileSizeBytes)
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
        FileName = fileName;
        ContentType = contentType;
        StoragePath = storagePath;
        FileSizeBytes = fileSizeBytes;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = "Failed";
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}
