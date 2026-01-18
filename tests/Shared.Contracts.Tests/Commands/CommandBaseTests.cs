using Shared.Contracts.Commands;

namespace Shared.Contracts.Tests.Commands;

// Test command for validation
[MessagePack.MessagePackObject]
public class TestCommand : CommandBase
{
    [MessagePack.Key(10)]
    public string TestProperty { get; init; } = string.Empty;
}

public class CommandBaseTests
{
    [Fact]
    public void CommandBase_ShouldGenerateCommandId()
    {
        // Act
        var command = new TestCommand();

        // Assert
        Assert.NotEqual(Guid.Empty, command.CommandId);
    }

    [Fact]
    public void CommandBase_ShouldSetTimestamp()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var command = new TestCommand();
        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.InRange(command.Timestamp, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public void CommandBase_CanSetCorrelationId()
    {
        // Arrange
        var correlationId = "test-correlation-id";

        // Act
        var command = new TestCommand 
        { 
            CorrelationId = correlationId 
        };

        // Assert
        Assert.Equal(correlationId, command.CorrelationId);
    }

    [Fact]
    public void CommandBase_CanSetUserId()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var command = new TestCommand 
        { 
            UserId = userId 
        };

        // Assert
        Assert.Equal(userId, command.UserId);
    }

    [Fact]
    public void CommandBase_CanSetCustomTimestamp()
    {
        // Arrange
        var customTimestamp = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var command = new TestCommand 
        { 
            Timestamp = customTimestamp 
        };

        // Assert
        Assert.Equal(customTimestamp, command.Timestamp);
    }

    [Fact]
    public void CommandBase_CanSetCustomCommandId()
    {
        // Arrange
        var customId = Guid.NewGuid();

        // Act
        var command = new TestCommand 
        { 
            CommandId = customId 
        };

        // Assert
        Assert.Equal(customId, command.CommandId);
    }

    [Fact]
    public void CommandBase_WithAllProperties_ShouldWorkCorrectly()
    {
        // Arrange
        var commandId = Guid.NewGuid();
        var correlationId = "correlation-123";
        var userId = "user-456";
        var timestamp = DateTimeOffset.UtcNow;
        var testProperty = "test-value";

        // Act
        var command = new TestCommand
        {
            CommandId = commandId,
            CorrelationId = correlationId,
            UserId = userId,
            Timestamp = timestamp,
            TestProperty = testProperty
        };

        // Assert
        Assert.Equal(commandId, command.CommandId);
        Assert.Equal(correlationId, command.CorrelationId);
        Assert.Equal(userId, command.UserId);
        Assert.Equal(timestamp, command.Timestamp);
        Assert.Equal(testProperty, command.TestProperty);
    }
}
