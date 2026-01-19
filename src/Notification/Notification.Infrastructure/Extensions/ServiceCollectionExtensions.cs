using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Core.Interfaces;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Services;

namespace Notification.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        string connectionString,
        EmailOptions? emailOptions = null,
        WebhookOptions? webhookOptions = null)
    {
        // Database
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Options
        services.AddSingleton(emailOptions ?? new EmailOptions());
        services.AddSingleton(webhookOptions ?? new WebhookOptions());

        // Email Service
        services.AddScoped<IEmailService, EmailService>();

        // Webhook Service with HttpClient
        services.AddHttpClient<IWebhookService, WebhookService>()
            .ConfigureHttpClient(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "NGMAT-Notification-Service/1.0");
            });

        // Notification Service
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }

    public static IServiceCollection AddNotificationSignalR(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();
        return services;
    }

    public static async Task MigrateNotificationDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await context.Database.MigrateAsync();
    }

    public static async Task EnsureNotificationDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
