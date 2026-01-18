using MissionManagement.Infrastructure.Extensions;
using MissionManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Messaging.UdpMulticast.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Mission Management API", Version = "v1" });
});

// Add Mission Management infrastructure
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=MissionManagement;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddMissionManagementInfrastructure(connectionString);

// Add UDP Multicast messaging for local development
builder.Services.AddUdpMulticastEventBus(options =>
{
    builder.Configuration.GetSection("UdpMulticast").Bind(options);
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MissionManagementDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Ensure database is created (for development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MissionManagementDbContext>();
    dbContext.Database.EnsureCreated();
}

Log.Information("Mission Management API starting...");

app.Run();

