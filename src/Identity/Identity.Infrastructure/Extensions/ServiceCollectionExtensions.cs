using Identity.Core.Interfaces;
using Identity.Core.Services;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring Identity infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Identity infrastructure services to the service collection.
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString,
        JwtOptions jwtOptions,
        AuthenticationOptions? authOptions = null,
        bool useInMemoryDatabase = false)
    {
        // Database
        if (useInMemoryDatabase)
        {
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase("NGMAT_Identity_InMemory"));
        }
        else
        {
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));
        }

        // Repositories and Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IMfaService, TotpMfaService>();
        services.AddSingleton(jwtOptions);
        services.AddSingleton<ITokenService, JwtTokenService>();

        // Authentication service
        services.AddSingleton(authOptions ?? new AuthenticationOptions());
        services.AddScoped<AuthenticationService>();

        return services;
    }

    /// <summary>
    /// Applies pending migrations to the Identity database.
    /// </summary>
    public static async Task MigrateIdentityDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Ensures the Identity database is created (for development/testing).
    /// </summary>
    public static async Task EnsureIdentityDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Seed default roles and permissions for in-memory database
        await SeedDefaultDataAsync(context);
    }

    private static async Task SeedDefaultDataAsync(IdentityDbContext context)
    {
        // Seed default permissions if they don't exist
        if (!context.Permissions.Any())
        {
            var permissions = new[]
            {
                Identity.Core.Entities.Permission.Create("users.read", "View users"),
                Identity.Core.Entities.Permission.Create("users.write", "Create and edit users"),
                Identity.Core.Entities.Permission.Create("users.delete", "Delete users"),
                Identity.Core.Entities.Permission.Create("roles.read", "View roles"),
                Identity.Core.Entities.Permission.Create("roles.write", "Create and edit roles"),
                Identity.Core.Entities.Permission.Create("roles.delete", "Delete roles"),
                Identity.Core.Entities.Permission.Create("missions.read", "View missions"),
                Identity.Core.Entities.Permission.Create("missions.write", "Create and edit missions"),
                Identity.Core.Entities.Permission.Create("missions.delete", "Delete missions"),
                Identity.Core.Entities.Permission.Create("apikeys.read", "View API keys"),
                Identity.Core.Entities.Permission.Create("apikeys.write", "Create and manage API keys"),
                Identity.Core.Entities.Permission.Create("apikeys.revoke", "Revoke API keys"),
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }

        // Seed default roles if they don't exist
        if (!context.Roles.Any())
        {
            var adminRole = Identity.Core.Entities.Role.Create(
                Identity.Core.Entities.Role.SystemRoles.Admin,
                "System administrator with full access",
                isSystem: true);

            var userRole = Identity.Core.Entities.Role.Create(
                Identity.Core.Entities.Role.SystemRoles.User,
                "Standard user role",
                isSystem: true);

            await context.Roles.AddRangeAsync(adminRole, userRole);
            await context.SaveChangesAsync();

            // Seed admin user if no users exist
            if (!context.Users.Any())
            {
                var passwordHasher = new BcryptPasswordHasher();
                var adminUser = Identity.Core.Entities.User.Create(
                    "admin@ngmat.local",
                    passwordHasher.Hash("Admin123!"),
                    "System Admin");
                adminUser.AddRole(adminRole);
                adminUser.AddRole(userRole);

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
