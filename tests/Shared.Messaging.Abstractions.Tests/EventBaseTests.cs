using Shared.Messaging.Abstractions;

namespace Shared.Messaging.Abstractions.Tests;

[MessagePack.MessagePackObject]
public class TestEvent : EventBase
{
    [MessagePack.Key(10)]
    public string TestProperty { get; init; } = string.Empty;
}

public class EventBaseTests
{
    [Fact]
    public void EventBase_ShouldGenerateEventId()
    {
        // Act
        var @event = new TestEvent();

        // Assert
        Assert.NotEqual(Guid.Empty, @event.EventId);
    }

    [Fact]
    public void EventBase_ShouldSetTimestamp()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var @event = new TestEvent();
        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.InRange(@event.Timestamp, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public void EventBase_DefaultVersion_ShouldBe1()
    {
        // Act
        var @event = new TestEvent();

        // Assert
        Assert.Equal(1, @event.Version);
    }

    [Fact]
    public void EventBase_CanSetCorrelationId()
    {
        // Arrange
        var correlationId = "test-correlation-id";

        // Act
        var @event = new TestEvent 
        { 
            CorrelationId = correlationId 
        };

        // Assert
        Assert.Equal(correlationId, @event.CorrelationId);
    }

    [Fact]
    public void EventBase_CanSetCausationId()
    {
        // Arrange
        var causationId = Guid.NewGuid();

        // Act
        var @event = new TestEvent 
        { 
            CausationId = causationId 
        };

        // Assert
        Assert.Equal(causationId, @event.CausationId);
    }

    [Fact]
    public void EventBase_CanSetUserId()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var @event = new TestEvent 
        { 
            UserId = userId 
        };

        // Assert
        Assert.Equal(userId, @event.UserId);
    }

    [Fact]
    public void EventBase_CanSetSourceService()
    {
        // Arrange
        var sourceService = "TestService";

        // Act
        var @event = new TestEvent 
        { 
            SourceService = sourceService 
        };

        // Assert
        Assert.Equal(sourceService, @event.SourceService);
    }

    [Fact]
    public void EventBase_GetMetadata_ShouldReturnMetadata()
    {
        // Arrange
        var @event = new TestEvent 
        { 
            CorrelationId = "test-correlation",
            UserId = "test-user"
        };

        // Act
        var metadata = @event.GetMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(@event.EventId, metadata.EventId);
        Assert.Equal(@event.Timestamp, metadata.Timestamp);
        Assert.Equal(@event.CorrelationId, metadata.CorrelationId);
    }

    [Fact]
    public void EventBase_WithCausedByConstructor_ShouldInheritContext()
    {
        // Arrange
        var parentEvent = new TestEvent 
        { 
            CorrelationId = "parent-correlation",
            UserId = "parent-user"
        };

        // Act
        var childEvent = new TestEventWithParent(parentEvent);

        // Assert
        Assert.Equal(parentEvent.CorrelationId, childEvent.CorrelationId);
        Assert.Equal(parentEvent.EventId, childEvent.CausationId);
        Assert.Equal(parentEvent.UserId, childEvent.UserId);
    }
}

[MessagePack.MessagePackObject]
public class TestEventWithParent : EventBase
{
    [MessagePack.Key(10)]
    public string TestProperty { get; init; } = string.Empty;

    public TestEventWithParent() : base()
    {
    }

    public TestEventWithParent(IEvent causedBy) : base(causedBy)
    {
    }
}
