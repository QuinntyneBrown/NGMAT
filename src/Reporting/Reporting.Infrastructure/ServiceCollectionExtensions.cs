using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reporting.Core.Interfaces;
using Reporting.Core.Services;
using Reporting.Infrastructure.Persistence;
using Reporting.Infrastructure.Repositories;
using Reporting.Infrastructure.Storage;

namespace Reporting.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("ReportingDb");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ReportDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        // Register repositories
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>();
        services.AddScoped<IReportingUnitOfWork, ReportingUnitOfWork>();

        // Register storage service
        services.AddScoped<IReportStorageService, FileSystemReportStorage>();

        // Register reporting service
        services.AddScoped<ReportingService>();

        return services;
    }
}
