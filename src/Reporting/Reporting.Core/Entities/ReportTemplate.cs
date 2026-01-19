namespace Reporting.Core.Entities;

/// <summary>
/// Custom report template entity
/// </summary>
public sealed class ReportTemplate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Format { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? Schema { get; private set; }
    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    
    // Metadata
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string CreatedByUserId { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    private ReportTemplate() { }

    public static ReportTemplate Create(
        string name,
        string type,
        string format,
        string content,
        string createdByUserId,
        string? description = null,
        string? schema = null)
    {
        return new ReportTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            Format = format,
            Content = content,
            Schema = schema,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public void Update(string name, string? description, string content, string? schema)
    {
        Name = name;
        Description = description;
        Content = content;
        Schema = schema;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
