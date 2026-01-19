using Notification.Api.Endpoints;
using Notification.Api.Hubs;
using Notification.Infrastructure.Extensions;
using Notification.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("NotificationDb")
    ?? "Server=localhost;Database=NGMAT_Notification;Trusted_Connection=True;TrustServerCertificate=True";

var emailOptions = builder.Configuration.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions
{
    SmtpHost = builder.Configuration["Email:SmtpHost"] ?? "localhost",
    SmtpPort = int.TryParse(builder.Configuration["Email:SmtpPort"], out var port) ? port : 587,
    UseSsl = bool.TryParse(builder.Configuration["Email:UseSsl"], out var useSsl) && useSsl,
    Username = builder.Configuration["Email:Username"],
    Password = builder.Configuration["Email:Password"],
    FromAddress = builder.Configuration["Email:FromAddress"] ?? "noreply@ngmat.example.com",
    FromName = builder.Configuration["Email:FromName"] ?? "NGMAT Notification System"
};

var webhookOptions = builder.Configuration.GetSection("Webhook").Get<WebhookOptions>() ?? new WebhookOptions();

// Add services
builder.Services.AddNotificationInfrastructure(connectionString, emailOptions, webhookOptions);
builder.Services.AddNotificationSignalR();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "NGMAT Notification Service API",
        Version = "v1",
        Description = "API for managing notifications, email, webhooks, and real-time push notifications"
    });
});

// Add CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:5173" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service v1");
    });

    // Ensure database is created in development
    await app.Services.EnsureNotificationDatabaseCreatedAsync();
}

app.UseHttpsRedirection();
app.UseCors();

// Map SignalR hub
app.MapHub<NotificationHub>("/api/v1/notifications/stream");

// Map API endpoints
app.MapNotificationEndpoints();
app.MapEmailEndpoints();
app.MapWebhookEndpoints();
app.MapPreferencesEndpoints();
app.MapAlertRuleEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "Notification",
    Timestamp = DateTimeOffset.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health");

// Ready check endpoint
app.MapGet("/ready", async (Notification.Infrastructure.Persistence.NotificationDbContext dbContext) =>
{
    try
    {
        await dbContext.Database.CanConnectAsync();
        return Results.Ok(new { Status = "Ready" });
    }
    catch
    {
        return Results.StatusCode(503);
    }
})
.WithName("ReadyCheck")
.WithTags("Health");

app.Run();
