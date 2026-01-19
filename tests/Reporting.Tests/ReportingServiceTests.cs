using Reporting.Core.Services;
using Reporting.Core.Models;

namespace Reporting.Tests;

public class ReportingServiceTests
{
    private readonly ReportingService _service;

    public ReportingServiceTests()
    {
        _service = new ReportingService();
    }

    [Fact]
    public void GenerateMissionReport_ShouldReturnSuccessResult()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var missionName = "Test Mission";
        var format = ReportFormat.Json;

        // Act
        var result = _service.GenerateMissionReport(missionId, missionName, format);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(missionId, result.Value.MissionId);
        Assert.Equal(missionName, result.Value.MissionName);
        Assert.Equal(format, result.Value.Format);
        Assert.Equal(ReportStatus.Completed, result.Value.Status);
        Assert.NotNull(result.Value.Content);
    }

    [Fact]
    public void GenerateMissionReport_WithPdfFormat_ShouldGeneratePdfContent()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var missionName = "Test Mission";
        var format = ReportFormat.Pdf;

        // Act
        var result = _service.GenerateMissionReport(missionId, missionName, format);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value?.Content);
        Assert.True(result.Value.Content.Length > 0);
        Assert.Equal("application/pdf", result.Value.ContentType);
    }

    [Fact]
    public void ExportStateVectors_ShouldReturnSuccessResult()
    {
        // Arrange
        var spacecraftId = Guid.NewGuid();
        var startEpoch = DateTime.UtcNow;
        var endEpoch = startEpoch.AddHours(1);
        var sampleInterval = 60.0;

        // Act
        var result = _service.ExportStateVectors(spacecraftId, startEpoch, endEpoch, sampleInterval, ReportFormat.Csv);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(spacecraftId, result.Value.SpacecraftId);
        Assert.True(result.Value.Records.Count > 0);
    }

    [Fact]
    public void ExportStateVectors_WithInvalidEpochs_ShouldReturnFailure()
    {
        // Arrange
        var spacecraftId = Guid.NewGuid();
        var startEpoch = DateTime.UtcNow;
        var endEpoch = startEpoch.AddHours(-1); // Invalid: end before start

        // Act
        var result = _service.ExportStateVectors(spacecraftId, startEpoch, endEpoch, 60, ReportFormat.Csv);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void GenerateDeltaVBudget_ShouldReturnSuccessResult()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var missionName = "Test Mission";
        var initialFuel = 1000.0;
        var remainingFuel = 800.0;

        // Act
        var result = _service.GenerateDeltaVBudget(missionId, missionName, initialFuel, remainingFuel);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(missionId, result.Value.MissionId);
        Assert.Equal(missionName, result.Value.MissionName);
        Assert.Equal(initialFuel, result.Value.InitialFuelKg);
        Assert.Equal(remainingFuel, result.Value.RemainingFuelKg);
    }

    [Fact]
    public void GenerateDeltaVBudget_WithPdfFormat_ShouldGeneratePdfContent()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var missionName = "Test Mission";
        var initialFuel = 1000.0;
        var remainingFuel = 800.0;
        var format = ReportFormat.Pdf;

        // Act
        var result = _service.GenerateDeltaVBudget(missionId, missionName, initialFuel, remainingFuel, null, format);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var bytes = _service.ExportToBytes(result.Value);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void GenerateTle_ShouldReturnSuccessResult()
    {
        // Arrange
        var spacecraftId = Guid.NewGuid();
        var spacecraftName = "TestSat";
        var epoch = DateTime.UtcNow;

        // Act
        var result = _service.GenerateTle(spacecraftId, spacecraftName, epoch);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(spacecraftId, result.Value.SpacecraftId);
        Assert.NotEmpty(result.Value.Line1);
        Assert.NotEmpty(result.Value.Line2);
    }

    [Fact]
    public void ExportOrbitalElements_ShouldReturnSuccessResult()
    {
        // Arrange
        var spacecraftId = Guid.NewGuid();
        var startEpoch = DateTime.UtcNow;
        var endEpoch = startEpoch.AddHours(1);
        var sampleInterval = 300.0;

        // Act
        var result = _service.ExportOrbitalElements(spacecraftId, startEpoch, endEpoch, sampleInterval, ReportFormat.Csv);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(spacecraftId, result.Value.SpacecraftId);
        Assert.True(result.Value.Records.Count > 0);
    }

    [Fact]
    public void ExportOrbitalElements_WithInvalidEpochs_ShouldReturnFailure()
    {
        // Arrange
        var spacecraftId = Guid.NewGuid();
        var startEpoch = DateTime.UtcNow;
        var endEpoch = startEpoch.AddHours(-1);

        // Act
        var result = _service.ExportOrbitalElements(spacecraftId, startEpoch, endEpoch, 300, ReportFormat.Csv);

        // Assert
        Assert.True(result.IsFailure);
    }
}
