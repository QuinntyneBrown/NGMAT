using System.Text;
using EventStore.Api.Endpoints;
using EventStore.Core.Services;
using EventStore.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Messaging.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("EventStoreDb")
    ?? "Server=localhost;Database=NGMAT_EventStore;Trusted_Connection=True;TrustServerCertificate=True";

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? "super-secret-key-that-should-be-at-least-32-chars!";

// Add services
builder.Services.AddEventStoreInfrastructure(connectionString);

// Add a null event publisher for now
builder.Services.AddSingleton<IEventPublisher, NullEventPublisher>();

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "NGMAT",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "NGMAT",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NGMAT Event Store API",
        Version = "v1",
        Description = "Event sourcing and audit trail API for NGMAT"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
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

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.Services.EnsureEventStoreDatabaseCreatedAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapEventEndpoints();
app.MapSubscriptionEndpoints();

// Health check
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "EventStore" }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.Run();

internal sealed class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent
        => Task.CompletedTask;

    public Task PublishAsync<TEvent>(string channel, TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent
        => Task.CompletedTask;

    public Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default) where TEvent : class, IEvent
        => Task.CompletedTask;
}
