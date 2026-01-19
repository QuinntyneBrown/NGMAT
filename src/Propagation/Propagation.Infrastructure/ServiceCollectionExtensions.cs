using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Propagation.Core.Interfaces;
using Propagation.Core.Services;
using Propagation.Infrastructure.Persistence;
using Propagation.Infrastructure.Repositories;

namespace Propagation.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPropagation(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<PropagationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add in-memory result repository (singleton)
        services.AddSingleton<IPropagationResultRepository, InMemoryPropagationResultRepository>();

        // Add Unit of Work
        services.AddScoped<IPropagationUnitOfWork, PropagationUnitOfWork>();

        // Add services
        services.AddScoped<PropagationService>();

        return services;
    }
}
