using Microsoft.Extensions.DependencyInjection;
using Optimization.Core.Interfaces;
using Optimization.Core.Services;
using Optimization.Infrastructure.Repositories;

namespace Optimization.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOptimizationInfrastructure(this IServiceCollection services)
    {
        // Register repositories - singleton for in-memory storage
        services.AddSingleton<InMemoryOptimizationJobRepository>();
        services.AddSingleton<IOptimizationJobRepository>(sp => sp.GetRequiredService<InMemoryOptimizationJobRepository>());

        // Register unit of work
        services.AddSingleton<IOptimizationUnitOfWork, InMemoryOptimizationUnitOfWork>();

        // Register service
        services.AddScoped<OptimizationService>();

        return services;
    }
}
