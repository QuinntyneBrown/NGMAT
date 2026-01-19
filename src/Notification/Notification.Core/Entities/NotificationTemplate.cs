using Notification.Core.Models;

namespace Notification.Core.Entities;

/// <summary>
/// Notification template for reusable notification content
/// </summary>
public class NotificationTemplate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public NotificationChannel Channel { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsHtml { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private NotificationTemplate() { } // For EF Core

    public static NotificationTemplate Create(
        string name,
        string description,
        NotificationChannel channel,
        string subject,
        string body,
        bool isHtml = false)
    {
        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Channel = channel,
            Subject = subject,
            Body = body,
            IsHtml = isHtml,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string? name = null, string? description = null, string? subject = null, string? body = null, bool? isHtml = null)
    {
        if (name is not null) Name = name;
        if (description is not null) Description = description;
        if (subject is not null) Subject = subject;
        if (body is not null) Body = body;
        if (isHtml.HasValue) IsHtml = isHtml.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Renders the template with the provided variables
    /// </summary>
    public (string subject, string body) Render(Dictionary<string, string> variables)
    {
        var renderedSubject = Subject;
        var renderedBody = Body;

        foreach (var (key, value) in variables)
        {
            var placeholder = $"{{{{{key}}}}}";
            renderedSubject = renderedSubject.Replace(placeholder, value);
            renderedBody = renderedBody.Replace(placeholder, value);
        }

        return (renderedSubject, renderedBody);
    }
}
