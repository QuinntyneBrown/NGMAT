using Reporting.Core.Entities;

namespace Reporting.Tests;

public class ReportEntityTests
{
    [Fact]
    public void Create_ShouldInitializeReportWithCorrectValues()
    {
        // Arrange
        var name = "Test Report";
        var type = "MissionReport";
        var format = "PDF";
        var userId = "user123";
        var missionId = Guid.NewGuid();

        // Act
        var report = Report.Create(name, type, format, userId, missionId: missionId);

        // Assert
        Assert.NotEqual(Guid.Empty, report.Id);
        Assert.Equal(name, report.Name);
        Assert.Equal(type, report.Type);
        Assert.Equal(format, report.Format);
        Assert.Equal(userId, report.CreatedByUserId);
        Assert.Equal(missionId, report.MissionId);
        Assert.Equal("Pending", report.Status);
        Assert.False(report.IsDeleted);
    }

    [Fact]
    public void MarkAsGenerating_ShouldUpdateStatus()
    {
        // Arrange
        var report = Report.Create("Test", "Type", "PDF", "user");

        // Act
        report.MarkAsGenerating();

        // Assert
        Assert.Equal("Generating", report.Status);
    }

    [Fact]
    public void MarkAsCompleted_ShouldUpdateStatusAndFileInfo()
    {
        // Arrange
        var report = Report.Create("Test", "Type", "PDF", "user");
        var fileName = "test.pdf";
        var contentType = "application/pdf";
        var storagePath = "/reports/test.pdf";
        var fileSize = 1024L;

        // Act
        report.MarkAsCompleted(fileName, contentType, storagePath, fileSize);

        // Assert
        Assert.Equal("Completed", report.Status);
        Assert.Equal(fileName, report.FileName);
        Assert.Equal(contentType, report.ContentType);
        Assert.Equal(storagePath, report.StoragePath);
        Assert.Equal(fileSize, report.FileSizeBytes);
        Assert.NotNull(report.CompletedAt);
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusAndErrorMessage()
    {
        // Arrange
        var report = Report.Create("Test", "Type", "PDF", "user");
        var errorMessage = "Test error";

        // Act
        report.MarkAsFailed(errorMessage);

        // Assert
        Assert.Equal("Failed", report.Status);
        Assert.Equal(errorMessage, report.ErrorMessage);
        Assert.NotNull(report.CompletedAt);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        // Arrange
        var report = Report.Create("Test", "Type", "PDF", "user");

        // Act
        report.Delete();

        // Assert
        Assert.True(report.IsDeleted);
    }
}
