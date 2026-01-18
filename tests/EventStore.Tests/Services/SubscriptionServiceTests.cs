using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using EventStore.Core.Services;
using Moq;

namespace EventStore.Tests.Services;

public class SubscriptionServiceTests
{
    private readonly Mock<IEventStoreUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
    private readonly EventStoreOptions _options;
    private readonly SubscriptionService _service;

    public SubscriptionServiceTests()
    {
        _unitOfWorkMock = new Mock<IEventStoreUnitOfWork>();
        _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        _options = new EventStoreOptions
        {
            InitialRetryDelay = TimeSpan.FromSeconds(1),
            RetryBackoffMultiplier = 2.0
        };

        _unitOfWorkMock.Setup(x => x.Subscriptions).Returns(_subscriptionRepositoryMock.Object);
        _service = new SubscriptionService(_unitOfWorkMock.Object, _options);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_WithNewName_ShouldCreateSubscription()
    {
        // Arrange
        var name = "TestSubscription";
        var eventTypes = new[] { "Event1", "Event2" };
        var callbackUrl = "https://example.com/callback";

        _subscriptionRepositoryMock
            .Setup(x => x.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        _subscriptionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription s, CancellationToken ct) => s);

        // Act
        var result = await _service.CreateSubscriptionAsync(
            name,
            eventTypes,
            callbackUrl);

        // Assert
        Assert.True(result.IsSuccess);
        var subscription = result.Value;
        Assert.Equal(name, subscription.Name);
        Assert.Equal(callbackUrl, subscription.CallbackUrl);
        Assert.True(subscription.IsActive);

        _subscriptionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_WithExistingName_ShouldReturnConflict()
    {
        // Arrange
        var name = "ExistingSubscription";
        var existingSubscription = Subscription.Create(
            name,
            new[] { "Event1" },
            "https://example.com");

        _subscriptionRepositoryMock
            .Setup(x => x.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        // Act
        var result = await _service.CreateSubscriptionAsync(
            name,
            new[] { "Event2" },
            "https://example.com/new");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error.Message);

        _subscriptionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_WithAggregateTypes_ShouldSetAggregateTypes()
    {
        // Arrange
        var name = "TestSubscription";
        var eventTypes = new[] { "Event1" };
        var aggregateTypes = new[] { "Aggregate1", "Aggregate2" };
        var callbackUrl = "https://example.com/callback";

        _subscriptionRepositoryMock
            .Setup(x => x.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        _subscriptionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription s, CancellationToken ct) => s);

        // Act
        var result = await _service.CreateSubscriptionAsync(
            name,
            eventTypes,
            callbackUrl,
            aggregateTypes);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.AggregateTypes);
    }

    [Fact]
    public async Task GetSubscriptionAsync_WhenExists_ShouldReturnSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _service.GetSubscriptionAsync(subscriptionId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(subscription.Name, result.Value.Name);
    }

    [Fact]
    public async Task GetSubscriptionAsync_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _service.GetSubscriptionAsync(subscriptionId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_ShouldReturnActiveSubscriptions()
    {
        // Arrange
        var subscriptions = new List<Subscription>
        {
            Subscription.Create("Sub1", new[] { "Event1" }, "https://example.com"),
            Subscription.Create("Sub2", new[] { "Event2" }, "https://example.com")
        };

        _subscriptionRepositoryMock
            .Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _service.GetActiveSubscriptionsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetSubscriptionsForEventAsync_ShouldReturnMatchingSubscriptions()
    {
        // Arrange
        var eventType = "TestEvent";
        var subscriptions = new List<Subscription>
        {
            Subscription.Create("Sub1", new[] { eventType }, "https://example.com")
        };

        _subscriptionRepositoryMock
            .Setup(x => x.GetByEventTypeAsync(eventType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _service.GetSubscriptionsForEventAsync(eventType);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task ActivateSubscriptionAsync_WhenExists_ShouldActivate()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");
        subscription.Deactivate();

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _service.ActivateSubscriptionAsync(subscriptionId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(subscription.IsActive);

        _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateSubscriptionAsync_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _service.ActivateSubscriptionAsync(subscriptionId);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeactivateSubscriptionAsync_WhenExists_ShouldDeactivate()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _service.DeactivateSubscriptionAsync(subscriptionId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(subscription.IsActive);

        _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateSubscriptionAsync_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _service.DeactivateSubscriptionAsync(subscriptionId);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_WhenExists_ShouldDelete()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _service.DeleteSubscriptionAsync(subscriptionId);

        // Assert
        Assert.True(result.IsSuccess);

        _subscriptionRepositoryMock.Verify(x => x.DeleteAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _service.DeleteSubscriptionAsync(subscriptionId);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RecordDeliveryAsync_WhenExists_ShouldUpdateSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var sequenceNumber = 42L;
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _service.RecordDeliveryAsync(subscriptionId, sequenceNumber);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(sequenceNumber, subscription.LastDeliveredSequence);
        Assert.NotNull(subscription.LastDeliveryAt);

        _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordDeliveryAsync_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _service.RecordDeliveryAsync(subscriptionId, 1L);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RecordFailureAsync_WhenExists_ShouldIncrementFailureCount()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _service.RecordFailureAsync(subscriptionId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, subscription.FailureCount);
        Assert.NotNull(subscription.NextRetryAt);

        _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordFailureAsync_Multiple_ShouldUseExponentialBackoff()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act - First failure (backoff = 1 * 2^0 = 1 second)
        await _service.RecordFailureAsync(subscriptionId);
        var firstRetryAt = subscription.NextRetryAt;

        // Act - Second failure (backoff = 1 * 2^1 = 2 seconds)
        await _service.RecordFailureAsync(subscriptionId);
        var secondRetryAt = subscription.NextRetryAt;

        // Act - Third failure (backoff = 1 * 2^2 = 4 seconds)
        await _service.RecordFailureAsync(subscriptionId);
        var thirdRetryAt = subscription.NextRetryAt;

        // Assert
        Assert.Equal(3, subscription.FailureCount);
        Assert.NotNull(firstRetryAt);
        Assert.NotNull(secondRetryAt);
        Assert.NotNull(thirdRetryAt);
        // Each subsequent retry should be later than the previous
        Assert.True(secondRetryAt > firstRetryAt);
        Assert.True(thirdRetryAt > secondRetryAt);
    }

    [Fact]
    public async Task RecordFailureAsync_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _service.RecordFailureAsync(subscriptionId);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
