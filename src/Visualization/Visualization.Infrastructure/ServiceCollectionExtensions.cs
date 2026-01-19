using Microsoft.Extensions.DependencyInjection;
using Visualization.Core.Services;

namespace Visualization.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVisualizationInfrastructure(this IServiceCollection services)
    {
        // Register visualization services
        services.AddScoped<VisualizationService>();
        services.AddScoped<SceneExportService>();

        return services;
    }
}
