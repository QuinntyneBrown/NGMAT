using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ScriptExecution.Api.Endpoints;
using ScriptExecution.Infrastructure;
using Shared.Messaging.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "NGMAT",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "NGMAT",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "NGMAT-Development-Key-Min-32-Chars!!"))
        };
    });

builder.Services.AddAuthorization();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NGMAT Script Execution Service",
        Version = "v1",
        Description = "GMAT-compatible script parsing and execution service"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Add ScriptExecution infrastructure
builder.Services.AddScriptExecutionInfrastructure();

// Add null event publisher for standalone operation
builder.Services.AddSingleton<IEventPublisher, NullEventPublisher>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapScriptEndpoints();

// Health check
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "ScriptExecution" }))
    .WithTags("Health");

app.Run();

/// <summary>
/// Null event publisher for standalone operation
/// </summary>
internal class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync<TEvent>(string channel, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        return Task.CompletedTask;
    }

    public Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        return Task.CompletedTask;
    }
}
