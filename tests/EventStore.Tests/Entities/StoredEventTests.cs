using EventStore.Core.Entities;

namespace EventStore.Tests.Entities;

public class StoredEventTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var eventType = "TestEvent";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var sequenceNumber = 5L;
        var data = "{\"test\":\"data\"}";
        var userId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var version = 2;
        var metadata = "{\"meta\":\"data\"}";

        // Act
        var storedEvent = StoredEvent.Create(
            eventType,
            aggregateId,
            aggregateType,
            sequenceNumber,
            data,
            userId,
            correlationId,
            causationId,
            version,
            metadata);

        // Assert
        Assert.NotEqual(Guid.Empty, storedEvent.Id);
        Assert.Equal(eventType, storedEvent.EventType);
        Assert.Equal(aggregateId, storedEvent.AggregateId);
        Assert.Equal(aggregateType, storedEvent.AggregateType);
        Assert.Equal(sequenceNumber, storedEvent.SequenceNumber);
        Assert.Equal(data, storedEvent.Data);
        Assert.Equal(userId, storedEvent.UserId);
        Assert.Equal(correlationId, storedEvent.CorrelationId);
        Assert.Equal(causationId, storedEvent.CausationId);
        Assert.Equal(version, storedEvent.Version);
        Assert.Equal(metadata, storedEvent.Metadata);
        Assert.True(storedEvent.Timestamp <= DateTimeOffset.UtcNow);
        Assert.Null(storedEvent.Hash);
    }

    [Fact]
    public void Create_WithMinimalParameters_ShouldSetDefaults()
    {
        // Arrange
        var eventType = "TestEvent";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var sequenceNumber = 1L;
        var data = "{\"test\":\"data\"}";

        // Act
        var storedEvent = StoredEvent.Create(
            eventType,
            aggregateId,
            aggregateType,
            sequenceNumber,
            data);

        // Assert
        Assert.NotEqual(Guid.Empty, storedEvent.Id);
        Assert.Equal(eventType, storedEvent.EventType);
        Assert.Equal(aggregateId, storedEvent.AggregateId);
        Assert.Equal(aggregateType, storedEvent.AggregateType);
        Assert.Equal(sequenceNumber, storedEvent.SequenceNumber);
        Assert.Equal(data, storedEvent.Data);
        Assert.Null(storedEvent.UserId);
        Assert.Null(storedEvent.CorrelationId);
        Assert.Null(storedEvent.CausationId);
        Assert.Equal(1, storedEvent.Version);
        Assert.Null(storedEvent.Metadata);
        Assert.Null(storedEvent.Hash);
    }

    [Fact]
    public void SetHash_ShouldUpdateHashProperty()
    {
        // Arrange
        var storedEvent = StoredEvent.Create(
            "TestEvent",
            Guid.NewGuid(),
            "TestAggregate",
            1L,
            "{}");
        var hash = "test-hash-value";

        // Act
        storedEvent.SetHash(hash);

        // Assert
        Assert.Equal(hash, storedEvent.Hash);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var event1 = StoredEvent.Create("Event", Guid.NewGuid(), "Aggregate", 1L, "{}");
        var event2 = StoredEvent.Create("Event", Guid.NewGuid(), "Aggregate", 1L, "{}");

        // Assert
        Assert.NotEqual(event1.Id, event2.Id);
    }
}
