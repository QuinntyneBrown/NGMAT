namespace Notification.Infrastructure.Services;

/// <summary>
/// Email service configuration options
/// </summary>
public sealed class EmailOptions
{
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "noreply@example.com";
    public string FromName { get; set; } = "NGMAT Notification System";
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 30;
}
