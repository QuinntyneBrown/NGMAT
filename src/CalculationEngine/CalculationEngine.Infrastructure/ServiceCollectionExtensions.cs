using CalculationEngine.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CalculationEngine.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCalculationEngine(this IServiceCollection services)
    {
        // Register all calculation services as singletons (they are stateless)
        services.AddSingleton<MatrixService>();
        services.AddSingleton<OdeSolverService>();
        services.AddSingleton<RootFindingService>();
        services.AddSingleton<InterpolationService>();
        services.AddSingleton<QuaternionService>();
        services.AddSingleton<UnitConversionService>();
        services.AddSingleton<StatisticsService>();
        services.AddSingleton<DerivativeService>();

        return services;
    }
}
