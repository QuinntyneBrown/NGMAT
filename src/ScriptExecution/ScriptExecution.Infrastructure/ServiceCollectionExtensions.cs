using Microsoft.Extensions.DependencyInjection;
using ScriptExecution.Core.Interfaces;
using ScriptExecution.Core.Services;
using ScriptExecution.Infrastructure.Repositories;

namespace ScriptExecution.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScriptExecutionInfrastructure(this IServiceCollection services)
    {
        // Register repositories
        services.AddSingleton<IScriptRepository, ScriptRepository>();

        // Register services
        services.AddSingleton<ScriptParser>();
        services.AddScoped<ScriptExecutionService>();

        return services;
    }
}
