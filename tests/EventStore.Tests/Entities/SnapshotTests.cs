using EventStore.Core.Entities;

namespace EventStore.Tests.Entities;

public class SnapshotTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var sequenceNumber = 100L;
        var data = "{\"state\":\"data\"}";
        var version = 2;

        // Act
        var snapshot = Snapshot.Create(
            aggregateId,
            aggregateType,
            sequenceNumber,
            data,
            version);

        // Assert
        Assert.NotEqual(Guid.Empty, snapshot.Id);
        Assert.Equal(aggregateId, snapshot.AggregateId);
        Assert.Equal(aggregateType, snapshot.AggregateType);
        Assert.Equal(sequenceNumber, snapshot.SequenceNumber);
        Assert.Equal(data, snapshot.Data);
        Assert.Equal(version, snapshot.Version);
        Assert.True(snapshot.Timestamp <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_WithDefaultVersion_ShouldUseVersionOne()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var sequenceNumber = 50L;
        var data = "{\"state\":\"data\"}";

        // Act
        var snapshot = Snapshot.Create(
            aggregateId,
            aggregateType,
            sequenceNumber,
            data);

        // Assert
        Assert.Equal(1, snapshot.Version);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var snapshot1 = Snapshot.Create(Guid.NewGuid(), "Aggregate", 1L, "{}");
        var snapshot2 = Snapshot.Create(Guid.NewGuid(), "Aggregate", 1L, "{}");

        // Assert
        Assert.NotEqual(snapshot1.Id, snapshot2.Id);
    }
}
