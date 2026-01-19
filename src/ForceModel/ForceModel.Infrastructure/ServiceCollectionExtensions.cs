using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ForceModel.Core.Interfaces;
using ForceModel.Core.Services;
using ForceModel.Infrastructure.Persistence;
using ForceModel.Infrastructure.Repositories;

namespace ForceModel.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddForceModel(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<ForceModelDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Unit of Work
        services.AddScoped<IForceModelUnitOfWork, ForceModelUnitOfWork>();

        // Add services
        services.AddScoped<ForceModelService>();
        services.AddScoped<ForceCalculationService>();

        return services;
    }
}
