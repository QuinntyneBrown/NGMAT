using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Visualization.Api.Endpoints;
using Visualization.Infrastructure;
using Shared.Messaging.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NGMAT Visualization API",
        Version = "v1",
        Description = "API for 3D orbit visualization, ground tracks, time-series plots, and mission data rendering"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
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

// Add authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ngmat-development-key-at-least-32-characters-long";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ngmat";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ngmat";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add Visualization services
builder.Services.AddVisualizationInfrastructure();

// Add event publisher (null implementation for now)
builder.Services.AddSingleton<IEventPublisher, NullEventPublisher>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NGMAT Visualization API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapVisualizationEndpoints();
app.MapHealthChecks("/health");

app.Run();

// Null event publisher for standalone operation
internal sealed class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
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
