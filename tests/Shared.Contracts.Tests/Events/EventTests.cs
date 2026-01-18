using Shared.Contracts.Events;

namespace Shared.Contracts.Tests.Events;

public class MissionEventsTests
{
    [Fact]
    public void MissionCreated_ShouldSetProperties()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var name = "Test Mission";
        var description = "Test Description";
        var startEpoch = DateTimeOffset.UtcNow;
        var endEpoch = startEpoch.AddDays(30);
        var createdBy = "test-user";

        // Act
        var @event = new MissionCreated
        {
            MissionId = missionId,
            Name = name,
            Description = description,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            CreatedBy = createdBy
        };

        // Assert
        Assert.Equal(missionId, @event.MissionId);
        Assert.Equal(name, @event.Name);
        Assert.Equal(description, @event.Description);
        Assert.Equal(startEpoch, @event.StartEpoch);
        Assert.Equal(endEpoch, @event.EndEpoch);
        Assert.Equal(createdBy, @event.CreatedBy);
        Assert.NotEqual(Guid.Empty, @event.EventId);
    }

    [Fact]
    public void MissionUpdated_ShouldSetProperties()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var name = "Updated Mission";
        var updatedBy = "test-user";

        // Act
        var @event = new MissionUpdated
        {
            MissionId = missionId,
            Name = name,
            UpdatedBy = updatedBy
        };

        // Assert
        Assert.Equal(missionId, @event.MissionId);
        Assert.Equal(name, @event.Name);
        Assert.Equal(updatedBy, @event.UpdatedBy);
    }

    [Fact]
    public void MissionDeleted_ShouldSetProperties()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var deletedBy = "test-user";

        // Act
        var @event = new MissionDeleted
        {
            MissionId = missionId,
            DeletedBy = deletedBy
        };

        // Assert
        Assert.Equal(missionId, @event.MissionId);
        Assert.Equal(deletedBy, @event.DeletedBy);
    }
}

public class SpacecraftEventsTests
{
    [Fact]
    public void SpacecraftCreated_ShouldSetProperties()
    {
        // Arrange
        var spacecraftId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var name = "Test Spacecraft";

        // Act
        var @event = new SpacecraftCreated
        {
            SpacecraftId = spacecraftId,
            MissionId = missionId,
            Name = name
        };

        // Assert
        Assert.Equal(spacecraftId, @event.SpacecraftId);
        Assert.Equal(missionId, @event.MissionId);
        Assert.Equal(name, @event.Name);
    }
}

public class UserEventsTests
{
    [Fact]
    public void UserLoggedIn_ShouldSetProperties()
    {
        // Arrange
        var userIdentifier = "testuser";
        var email = "test@example.com";
        var loginMethod = "password";

        // Act
        var @event = new UserLoggedIn
        {
            UserIdentifier = userIdentifier,
            Email = email,
            LoginMethod = loginMethod
        };

        // Assert
        Assert.Equal(userIdentifier, @event.UserIdentifier);
        Assert.Equal(email, @event.Email);
        Assert.Equal(loginMethod, @event.LoginMethod);
    }
}
