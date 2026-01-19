using Microsoft.Extensions.DependencyInjection;
using Reporting.Core.Services;

namespace Reporting.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services)
    {
        // Register reporting service
        services.AddScoped<ReportingService>();

        return services;
    }
}
