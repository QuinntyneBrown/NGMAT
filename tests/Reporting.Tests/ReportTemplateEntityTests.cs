using Reporting.Core.Entities;

namespace Reporting.Tests;

public class ReportTemplateEntityTests
{
    [Fact]
    public void Create_ShouldInitializeTemplateWithCorrectValues()
    {
        // Arrange
        var name = "Monthly Report Template";
        var type = "MissionReport";
        var format = "PDF";
        var content = "Template content here";
        var userId = "user123";

        // Act
        var template = ReportTemplate.Create(name, type, format, content, userId);

        // Assert
        Assert.NotEqual(Guid.Empty, template.Id);
        Assert.Equal(name, template.Name);
        Assert.Equal(type, template.Type);
        Assert.Equal(format, template.Format);
        Assert.Equal(content, template.Content);
        Assert.Equal(userId, template.CreatedByUserId);
        Assert.Equal(1, template.Version);
        Assert.True(template.IsActive);
        Assert.False(template.IsDeleted);
    }

    [Fact]
    public void Update_ShouldIncrementVersionAndUpdateFields()
    {
        // Arrange
        var template = ReportTemplate.Create("Test", "Type", "PDF", "Content", "user");
        var newName = "Updated Template";
        var newContent = "New content";

        // Act
        template.Update(newName, "Updated description", newContent, null);

        // Assert
        Assert.Equal(newName, template.Name);
        Assert.Equal(newContent, template.Content);
        Assert.Equal(2, template.Version);
        Assert.NotNull(template.UpdatedAt);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var template = ReportTemplate.Create("Test", "Type", "PDF", "Content", "user");
        template.Deactivate();

        // Act
        template.Activate();

        // Assert
        Assert.True(template.IsActive);
        Assert.NotNull(template.UpdatedAt);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var template = ReportTemplate.Create("Test", "Type", "PDF", "Content", "user");

        // Act
        template.Deactivate();

        // Assert
        Assert.False(template.IsActive);
        Assert.NotNull(template.UpdatedAt);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        // Arrange
        var template = ReportTemplate.Create("Test", "Type", "PDF", "Content", "user");

        // Act
        template.Delete();

        // Assert
        Assert.True(template.IsDeleted);
        Assert.NotNull(template.UpdatedAt);
    }
}
