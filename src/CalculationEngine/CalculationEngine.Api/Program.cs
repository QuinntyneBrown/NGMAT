using CalculationEngine.Api.Endpoints;
using CalculationEngine.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Calculation Engine API",
        Version = "v1",
        Description = "Numerical computation services for NGMAT"
    });
});

// Add Calculation Engine services
builder.Services.AddCalculationEngine();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calculation Engine API v1");
    });
}

app.UseHttpsRedirection();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map all endpoints
app.MapMatrixEndpoints();
app.MapStatisticsEndpoints();
app.MapInterpolationEndpoints();
app.MapQuaternionEndpoints();
app.MapUnitConversionEndpoints();
app.MapNumericalEndpoints();

app.Run();
