using FluentAssertions;
using MissionManagement.Core.Entities;
using MissionManagement.Core.Enums;

namespace MissionManagement.Tests.Entities;

public class MissionTests
{
    [Fact]
    public void Create_ValidParameters_CreatesMission()
    {
        // Arrange
        var name = "Test Mission";
        var type = MissionType.LEO;
        var startEpoch = DateTimeOffset.UtcNow;
        var ownerId = Guid.NewGuid();
        var description = "Test Description";

        // Act
        var mission = Mission.Create(name, type, startEpoch, ownerId, description);

        // Assert
        mission.Should().NotBeNull();
        mission.MissionId.Should().NotBeEmpty();
        mission.Name.Should().Be(name);
        mission.Type.Should().Be(type);
        mission.StartEpoch.Should().Be(startEpoch);
        mission.OwnerId.Should().Be(ownerId);
        mission.Description.Should().Be(description);
        mission.Status.Should().Be(MissionStatus.Draft);
        mission.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var name = "";
        var type = MissionType.LEO;
        var startEpoch = DateTimeOffset.UtcNow;
        var ownerId = Guid.NewGuid();

        // Act
        var act = () => Mission.Create(name, type, startEpoch, ownerId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Mission name is required.*");
    }

    [Fact]
    public void Create_EmptyOwnerId_ThrowsArgumentException()
    {
        // Arrange
        var name = "Test Mission";
        var type = MissionType.LEO;
        var startEpoch = DateTimeOffset.UtcNow;
        var ownerId = Guid.Empty;

        // Act
        var act = () => Mission.Create(name, type, startEpoch, ownerId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Owner ID is required.*");
    }

    [Fact]
    public void Update_ValidParameters_UpdatesMission()
    {
        // Arrange
        var mission = Mission.Create("Original", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());
        var newName = "Updated";
        var newType = MissionType.GEO;
        var newStartEpoch = DateTimeOffset.UtcNow.AddDays(1);

        // Act
        mission.Update(newName, newType, newStartEpoch);

        // Assert
        mission.Name.Should().Be(newName);
        mission.Type.Should().Be(newType);
        mission.StartEpoch.Should().Be(newStartEpoch);
    }

    [Fact]
    public void ChangeStatus_ValidTransition_ChangesStatus()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());

        // Act
        mission.ChangeStatus(MissionStatus.Active);

        // Assert
        mission.Status.Should().Be(MissionStatus.Active);
    }

    [Fact]
    public void ChangeStatus_DraftToActive_IsValid()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());

        // Act
        mission.ChangeStatus(MissionStatus.Active);

        // Assert
        mission.Status.Should().Be(MissionStatus.Active);
    }

    [Fact]
    public void ChangeStatus_DraftToArchived_IsValid()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());

        // Act
        mission.ChangeStatus(MissionStatus.Archived);

        // Assert
        mission.Status.Should().Be(MissionStatus.Archived);
    }

    [Fact]
    public void ChangeStatus_DraftToCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());

        // Act
        var act = () => mission.ChangeStatus(MissionStatus.Completed);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid status transition*");
    }

    [Fact]
    public void ChangeStatus_ActiveToCompleted_IsValid()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());
        mission.ChangeStatus(MissionStatus.Active);

        // Act
        mission.ChangeStatus(MissionStatus.Completed);

        // Assert
        mission.Status.Should().Be(MissionStatus.Completed);
    }

    [Fact]
    public void ChangeStatus_ActiveToArchived_IsValid()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());
        mission.ChangeStatus(MissionStatus.Active);

        // Act
        mission.ChangeStatus(MissionStatus.Archived);

        // Assert
        mission.Status.Should().Be(MissionStatus.Archived);
    }

    [Fact]
    public void ChangeStatus_ActiveToDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());
        mission.ChangeStatus(MissionStatus.Active);

        // Act
        var act = () => mission.ChangeStatus(MissionStatus.Draft);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid status transition*");
    }

    [Fact]
    public void ChangeStatus_CompletedToArchived_IsValid()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());
        mission.ChangeStatus(MissionStatus.Active);
        mission.ChangeStatus(MissionStatus.Completed);

        // Act
        mission.ChangeStatus(MissionStatus.Archived);

        // Assert
        mission.Status.Should().Be(MissionStatus.Archived);
    }

    [Fact]
    public void ChangeStatus_CompletedToActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());
        mission.ChangeStatus(MissionStatus.Active);
        mission.ChangeStatus(MissionStatus.Completed);

        // Act
        var act = () => mission.ChangeStatus(MissionStatus.Active);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid status transition*");
    }

    [Fact]
    public void ChangeStatus_ArchivedMission_ThrowsInvalidOperationException()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());
        mission.ChangeStatus(MissionStatus.Archived);

        // Act
        var act = () => mission.ChangeStatus(MissionStatus.Active);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot change status of an archived mission.");
    }

    [Fact]
    public void Delete_ValidMission_SoftDeletesMission()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());

        // Act
        mission.Delete();

        // Assert
        mission.IsDeleted.Should().BeTrue();
        mission.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Delete_AlreadyDeletedMission_ThrowsInvalidOperationException()
    {
        // Arrange
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, Guid.NewGuid());
        mission.Delete();

        // Act
        var act = () => mission.Delete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Mission is already deleted.");
    }

    [Fact]
    public void IsOwnedBy_CorrectOwner_ReturnsTrue()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, ownerId);

        // Act
        var result = mission.IsOwnedBy(ownerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOwnedBy_DifferentUser_ReturnsFalse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var mission = Mission.Create("Test", MissionType.LEO, DateTimeOffset.UtcNow, ownerId);

        // Act
        var result = mission.IsOwnedBy(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
