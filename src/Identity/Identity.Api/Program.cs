using System.Text;
using Identity.Api.Endpoints;
using Identity.Infrastructure.Extensions;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Messaging.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("IdentityDb")
    ?? "Server=localhost;Database=NGMAT_Identity;Trusted_Connection=True;TrustServerCertificate=True";

var useInMemoryDb = builder.Configuration.GetValue<bool>("UseInMemoryDatabase")
    || Environment.GetEnvironmentVariable("USE_INMEMORY_DB") == "true";

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions
{
    SecretKey = builder.Configuration["Jwt:SecretKey"] ?? "super-secret-key-that-should-be-at-least-32-chars!",
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "NGMAT",
    Audience = builder.Configuration["Jwt:Audience"] ?? "NGMAT",
    AccessTokenLifetime = TimeSpan.FromMinutes(15),
    RefreshTokenLifetime = TimeSpan.FromDays(7)
};

// Add services
builder.Services.AddIdentityInfrastructure(connectionString, jwtOptions, useInMemoryDatabase: useInMemoryDb);

// Add a null event publisher for now (will be replaced with Redis later)
builder.Services.AddSingleton<IEventPublisher, NullEventPublisher>();

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User", "Admin"));
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NGMAT Identity API",
        Version = "v1",
        Description = "Authentication and user management API for NGMAT"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Ensure database is created in development
    await app.Services.EnsureIdentityDatabaseCreatedAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthenticationEndpoints();
app.MapUserEndpoints();
app.MapRoleEndpoints();
app.MapPermissionEndpoints();
app.MapApiKeyEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Identity" }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.Run();

/// <summary>
/// Null event publisher for development without messaging infrastructure.
/// </summary>
internal sealed class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        // No-op for now - events will be published when messaging infrastructure is configured
        return Task.CompletedTask;
    }

    public Task PublishAsync<TEvent>(string channel, TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        return Task.CompletedTask;
    }

    public Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        return Task.CompletedTask;
    }
}
