using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using EventStore.Core.Services;
using Moq;

namespace EventStore.Tests.Services;

public class SnapshotServiceTests
{
    private readonly Mock<IEventStoreUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<ISnapshotRepository> _snapshotRepositoryMock;
    private readonly EventStoreOptions _options;
    private readonly SnapshotService _service;

    public SnapshotServiceTests()
    {
        _unitOfWorkMock = new Mock<IEventStoreUnitOfWork>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _snapshotRepositoryMock = new Mock<ISnapshotRepository>();
        _options = new EventStoreOptions();

        _unitOfWorkMock.Setup(x => x.Events).Returns(_eventRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Snapshots).Returns(_snapshotRepositoryMock.Object);
        _service = new SnapshotService(_unitOfWorkMock.Object, _options);
    }

    [Fact]
    public async Task CreateSnapshotAsync_ShouldCreateAndSaveSnapshot()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var stateData = "{\"state\":\"data\"}";
        var version = 1;
        var latestSequence = 100L;

        _eventRepositoryMock
            .Setup(x => x.GetLatestSequenceNumberAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestSequence);

        _snapshotRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<Snapshot>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Snapshot s, CancellationToken ct) => s);

        // Act
        var result = await _service.CreateSnapshotAsync(
            aggregateId,
            aggregateType,
            stateData,
            version);

        // Assert
        Assert.True(result.IsSuccess);
        var snapshot = result.Value;
        Assert.Equal(aggregateId, snapshot.AggregateId);
        Assert.Equal(aggregateType, snapshot.AggregateType);
        Assert.Equal(latestSequence, snapshot.SequenceNumber);
        Assert.Equal(stateData, snapshot.Data);
        Assert.Equal(version, snapshot.Version);

        _snapshotRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Snapshot>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLatestSnapshotAsync_WhenSnapshotExists_ShouldReturnSnapshot()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var snapshot = Snapshot.Create(aggregateId, "Aggregate", 50L, "{}");

        _snapshotRepositoryMock
            .Setup(x => x.GetLatestAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        // Act
        var result = await _service.GetLatestSnapshotAsync(aggregateId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(aggregateId, result.Value.AggregateId);
    }

    [Fact]
    public async Task GetLatestSnapshotAsync_WhenNoSnapshot_ShouldReturnNull()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        _snapshotRepositoryMock
            .Setup(x => x.GetLatestAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Snapshot?)null);

        // Act
        var result = await _service.GetLatestSnapshotAsync(aggregateId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task ReplayEventsAsync_WithSnapshot_ShouldStartFromSnapshot()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var snapshot = Snapshot.Create(aggregateId, "Aggregate", 50L, "{\"count\":50}");
        var eventsAfterSnapshot = new List<StoredEvent>
        {
            StoredEvent.Create("Event", aggregateId, "Aggregate", 51L, "{\"increment\":1}"),
            StoredEvent.Create("Event", aggregateId, "Aggregate", 52L, "{\"increment\":1}")
        };

        _snapshotRepositoryMock
            .Setup(x => x.GetLatestAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _eventRepositoryMock
            .Setup(x => x.GetByAggregateIdAsync(aggregateId, 51L, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventsAfterSnapshot);

        // Act
        var result = await _service.ReplayEventsAsync(
            aggregateId,
            (state, eventData) => $"{state}+{eventData}");

        // Assert
        Assert.True(result.IsSuccess);
        var replayResult = result.Value;
        Assert.Equal(aggregateId, replayResult.AggregateId);
        Assert.Equal("{\"count\":50}+{\"increment\":1}+{\"increment\":1}", replayResult.CurrentState);
        Assert.Equal(52L, replayResult.SequenceNumber);
        Assert.Equal(2, replayResult.EventsApplied);
        Assert.True(replayResult.UsedSnapshot);
    }

    [Fact]
    public async Task ReplayEventsAsync_WithoutSnapshot_ShouldStartFromEmpty()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var allEvents = new List<StoredEvent>
        {
            StoredEvent.Create("Event1", aggregateId, "Aggregate", 1L, "data1"),
            StoredEvent.Create("Event2", aggregateId, "Aggregate", 2L, "data2")
        };

        _snapshotRepositoryMock
            .Setup(x => x.GetLatestAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Snapshot?)null);

        _eventRepositoryMock
            .Setup(x => x.GetByAggregateIdAsync(aggregateId, 1L, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allEvents);

        // Act
        var result = await _service.ReplayEventsAsync(
            aggregateId,
            (state, eventData) => $"{state}+{eventData}");

        // Assert
        Assert.True(result.IsSuccess);
        var replayResult = result.Value;
        Assert.Equal("{}+data1+data2", replayResult.CurrentState);
        Assert.Equal(2L, replayResult.SequenceNumber);
        Assert.Equal(2, replayResult.EventsApplied);
        Assert.False(replayResult.UsedSnapshot);
    }

    [Fact]
    public async Task ReplayEventsAsync_WithNoEvents_ShouldReturnSnapshotState()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var snapshot = Snapshot.Create(aggregateId, "Aggregate", 100L, "{\"final\":true}");

        _snapshotRepositoryMock
            .Setup(x => x.GetLatestAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _eventRepositoryMock
            .Setup(x => x.GetByAggregateIdAsync(aggregateId, 101L, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoredEvent>());

        // Act
        var result = await _service.ReplayEventsAsync(
            aggregateId,
            (state, eventData) => state);

        // Assert
        Assert.True(result.IsSuccess);
        var replayResult = result.Value;
        Assert.Equal("{\"final\":true}", replayResult.CurrentState);
        Assert.Equal(100L, replayResult.SequenceNumber);
        Assert.Equal(0, replayResult.EventsApplied);
        Assert.True(replayResult.UsedSnapshot);
    }

    [Fact]
    public async Task CleanupOldSnapshotsAsync_WhenLessThanKeepCount_ShouldNotDelete()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var snapshots = new List<Snapshot>
        {
            Snapshot.Create(aggregateId, "Aggregate", 10L, "{}"),
            Snapshot.Create(aggregateId, "Aggregate", 20L, "{}"),
            Snapshot.Create(aggregateId, "Aggregate", 30L, "{}")
        };

        _snapshotRepositoryMock
            .Setup(x => x.GetAllAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshots);

        // Act
        var result = await _service.CleanupOldSnapshotsAsync(aggregateId, keepCount: 5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        _snapshotRepositoryMock.Verify(
            x => x.DeleteOlderThanAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CleanupOldSnapshotsAsync_WhenMoreThanKeepCount_ShouldDeleteOldSnapshots()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var baseTime = DateTimeOffset.UtcNow.AddDays(-10);
        var snapshots = Enumerable.Range(1, 8)
            .Select(i => Snapshot.Create(aggregateId, "Aggregate", i * 10L, "{}"))
            .ToList();

        _snapshotRepositoryMock
            .Setup(x => x.GetAllAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshots);

        _snapshotRepositoryMock
            .Setup(x => x.DeleteOlderThanAsync(aggregateId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _service.CleanupOldSnapshotsAsync(aggregateId, keepCount: 5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value);
        _snapshotRepositoryMock.Verify(
            x => x.DeleteOlderThanAsync(aggregateId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
