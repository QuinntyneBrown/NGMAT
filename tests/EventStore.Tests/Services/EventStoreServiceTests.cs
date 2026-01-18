using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using EventStore.Core.Services;
using Moq;
using Shared.Messaging.Abstractions;

namespace EventStore.Tests.Services;

public class EventStoreServiceTests
{
    private readonly Mock<IEventStoreUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly EventStoreOptions _options;
    private readonly EventStoreService _service;

    public EventStoreServiceTests()
    {
        _unitOfWorkMock = new Mock<IEventStoreUnitOfWork>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _options = new EventStoreOptions
        {
            EnableAuditHashing = true,
            SnapshotInterval = 100
        };

        _unitOfWorkMock.Setup(x => x.Events).Returns(_eventRepositoryMock.Object);
        _service = new EventStoreService(_unitOfWorkMock.Object, _eventPublisherMock.Object, _options);
    }

    [Fact]
    public async Task AppendEventAsync_ShouldCreateAndSaveEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var eventType = "TestEvent";
        var aggregateType = "TestAggregate";
        var data = "{\"test\":\"data\"}";

        _eventRepositoryMock
            .Setup(x => x.GetLatestSequenceNumberAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);

        _eventRepositoryMock
            .Setup(x => x.AppendAsync(It.IsAny<StoredEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredEvent e, CancellationToken ct) => e);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.AppendEventAsync(
            eventType,
            aggregateId,
            aggregateType,
            data);

        // Assert
        Assert.True(result.IsSuccess);
        var storedEvent = result.Value;
        Assert.Equal(eventType, storedEvent.EventType);
        Assert.Equal(aggregateId, storedEvent.AggregateId);
        Assert.Equal(aggregateType, storedEvent.AggregateType);
        Assert.Equal(1L, storedEvent.SequenceNumber);
        Assert.Equal(data, storedEvent.Data);
        Assert.NotNull(storedEvent.Hash); // Hash should be set when enabled

        _eventRepositoryMock.Verify(x => x.AppendAsync(It.IsAny<StoredEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AppendEventAsync_WithAuditHashingDisabled_ShouldNotSetHash()
    {
        // Arrange
        var options = new EventStoreOptions { EnableAuditHashing = false };
        var service = new EventStoreService(_unitOfWorkMock.Object, _eventPublisherMock.Object, options);
        var aggregateId = Guid.NewGuid();

        _eventRepositoryMock
            .Setup(x => x.GetLatestSequenceNumberAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);

        _eventRepositoryMock
            .Setup(x => x.AppendAsync(It.IsAny<StoredEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredEvent e, CancellationToken ct) => e);

        // Act
        var result = await service.AppendEventAsync(
            "TestEvent",
            aggregateId,
            "TestAggregate",
            "{}");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Hash);
    }

    [Fact]
    public async Task AppendEventAsync_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var version = 2;
        var metadata = "{\"meta\":\"data\"}";

        _eventRepositoryMock
            .Setup(x => x.GetLatestSequenceNumberAsync(aggregateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5L);

        _eventRepositoryMock
            .Setup(x => x.AppendAsync(It.IsAny<StoredEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredEvent e, CancellationToken ct) => e);

        // Act
        var result = await _service.AppendEventAsync(
            "TestEvent",
            aggregateId,
            "TestAggregate",
            "{}",
            userId,
            correlationId,
            causationId,
            version,
            metadata);

        // Assert
        Assert.True(result.IsSuccess);
        var storedEvent = result.Value;
        Assert.Equal(6L, storedEvent.SequenceNumber);
        Assert.Equal(userId, storedEvent.UserId);
        Assert.Equal(correlationId, storedEvent.CorrelationId);
        Assert.Equal(causationId, storedEvent.CausationId);
        Assert.Equal(version, storedEvent.Version);
        Assert.Equal(metadata, storedEvent.Metadata);
    }

    [Fact]
    public async Task AppendEventsAsync_ShouldAppendMultipleEvents()
    {
        // Arrange
        var aggregateId1 = Guid.NewGuid();
        var aggregateId2 = Guid.NewGuid();

        var requests = new[]
        {
            new AppendEventRequest("Event1", aggregateId1, "Aggregate1", "{}"),
            new AppendEventRequest("Event2", aggregateId1, "Aggregate1", "{}"),
            new AppendEventRequest("Event3", aggregateId2, "Aggregate2", "{}")
        };

        _eventRepositoryMock
            .Setup(x => x.GetLatestSequenceNumberAsync(aggregateId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);

        _eventRepositoryMock
            .Setup(x => x.GetLatestSequenceNumberAsync(aggregateId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);

        _eventRepositoryMock
            .Setup(x => x.AppendBatchAsync(It.IsAny<IEnumerable<StoredEvent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<StoredEvent> events, CancellationToken ct) => events.ToList());

        // Act
        var result = await _service.AppendEventsAsync(requests);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        _eventRepositoryMock.Verify(x => x.AppendBatchAsync(It.IsAny<IEnumerable<StoredEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEventsAsync_WithAggregateId_ShouldQueryEvents()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var events = new List<StoredEvent>
        {
            StoredEvent.Create("Event1", aggregateId, "Aggregate", 1L, "{}"),
            StoredEvent.Create("Event2", aggregateId, "Aggregate", 2L, "{}")
        };

        _eventRepositoryMock
            .Setup(x => x.QueryAsync(
                aggregateId,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetEventsAsync(aggregateId: aggregateId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetEventsAsync_WithEventType_ShouldQueryByEventType()
    {
        // Arrange
        var eventType = "TestEvent";
        var events = new List<StoredEvent>
        {
            StoredEvent.Create(eventType, Guid.NewGuid(), "Aggregate", 1L, "{}")
        };

        _eventRepositoryMock
            .Setup(x => x.QueryAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                eventType,
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<Guid?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetEventsAsync(eventType: eventType);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task GetEventsByAggregateAsync_ShouldReturnEventsForAggregate()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var events = new List<StoredEvent>
        {
            StoredEvent.Create("Event1", aggregateId, "Aggregate", 1L, "{}"),
            StoredEvent.Create("Event2", aggregateId, "Aggregate", 2L, "{}"),
            StoredEvent.Create("Event3", aggregateId, "Aggregate", 3L, "{}")
        };

        _eventRepositoryMock
            .Setup(x => x.GetByAggregateIdAsync(
                aggregateId,
                It.IsAny<long?>(),
                It.IsAny<long?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetEventsByAggregateAsync(aggregateId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
    }

    [Fact]
    public async Task GetEventsByAggregateAsync_WithSequenceRange_ShouldFilterBySequence()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var fromSequence = 2L;
        var toSequence = 5L;
        var events = new List<StoredEvent>
        {
            StoredEvent.Create("Event2", aggregateId, "Aggregate", 2L, "{}"),
            StoredEvent.Create("Event3", aggregateId, "Aggregate", 3L, "{}")
        };

        _eventRepositoryMock
            .Setup(x => x.GetByAggregateIdAsync(
                aggregateId,
                fromSequence,
                toSequence,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetEventsByAggregateAsync(
            aggregateId,
            fromSequence,
            toSequence);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetEventCountAsync_ShouldReturnCount()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var expectedCount = 42;

        _eventRepositoryMock
            .Setup(x => x.GetCountAsync(
                aggregateId,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetEventCountAsync(aggregateId: aggregateId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCount, result.Value);
    }
}
