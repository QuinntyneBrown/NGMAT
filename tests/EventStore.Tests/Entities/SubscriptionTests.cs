using EventStore.Core.Entities;

namespace EventStore.Tests.Entities;

public class SubscriptionTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var name = "TestSubscription";
        var eventTypes = new[] { "Event1", "Event2", "Event3" };
        var callbackUrl = "https://example.com/callback";
        var aggregateTypes = new[] { "Aggregate1", "Aggregate2" };

        // Act
        var subscription = Subscription.Create(
            name,
            eventTypes,
            callbackUrl,
            aggregateTypes);

        // Assert
        Assert.NotEqual(Guid.Empty, subscription.Id);
        Assert.Equal(name, subscription.Name);
        Assert.Equal("Event1,Event2,Event3", subscription.EventTypes);
        Assert.Equal("Aggregate1,Aggregate2", subscription.AggregateTypes);
        Assert.Equal(callbackUrl, subscription.CallbackUrl);
        Assert.True(subscription.IsActive);
        Assert.True(subscription.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.Equal(0, subscription.LastDeliveredSequence);
        Assert.Equal(0, subscription.FailureCount);
        Assert.Null(subscription.LastDeliveryAt);
        Assert.Null(subscription.NextRetryAt);
    }

    [Fact]
    public void Create_WithoutAggregateTypes_ShouldSetNullAggregateTypes()
    {
        // Arrange
        var name = "TestSubscription";
        var eventTypes = new[] { "Event1" };
        var callbackUrl = "https://example.com/callback";

        // Act
        var subscription = Subscription.Create(
            name,
            eventTypes,
            callbackUrl);

        // Assert
        Assert.Null(subscription.AggregateTypes);
    }

    [Fact]
    public void GetEventTypes_ShouldReturnParsedEventTypes()
    {
        // Arrange
        var eventTypes = new[] { "Event1", "Event2", "Event3" };
        var subscription = Subscription.Create(
            "Test",
            eventTypes,
            "https://example.com");

        // Act
        var result = subscription.GetEventTypes().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("Event1", result);
        Assert.Contains("Event2", result);
        Assert.Contains("Event3", result);
    }

    [Fact]
    public void GetAggregateTypes_WithAggregateTypes_ShouldReturnParsedTypes()
    {
        // Arrange
        var aggregateTypes = new[] { "Aggregate1", "Aggregate2" };
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com",
            aggregateTypes);

        // Act
        var result = subscription.GetAggregateTypes()?.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("Aggregate1", result);
        Assert.Contains("Aggregate2", result);
    }

    [Fact]
    public void GetAggregateTypes_WithoutAggregateTypes_ShouldReturnNull()
    {
        // Arrange
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        // Act
        var result = subscription.GetAggregateTypes();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RecordDelivery_ShouldUpdateDeliveryProperties()
    {
        // Arrange
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");
        subscription.RecordFailure(TimeSpan.FromSeconds(10)); // Set failure first
        var sequenceNumber = 42L;

        // Act
        subscription.RecordDelivery(sequenceNumber);

        // Assert
        Assert.NotNull(subscription.LastDeliveryAt);
        Assert.Equal(sequenceNumber, subscription.LastDeliveredSequence);
        Assert.Equal(0, subscription.FailureCount);
        Assert.Null(subscription.NextRetryAt);
    }

    [Fact]
    public void RecordFailure_ShouldIncrementFailureCountAndSetRetryTime()
    {
        // Arrange
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");
        var backoffDelay = TimeSpan.FromSeconds(30);

        // Act
        var beforeFailure = DateTimeOffset.UtcNow;
        subscription.RecordFailure(backoffDelay);
        var afterFailure = DateTimeOffset.UtcNow;

        // Assert
        Assert.Equal(1, subscription.FailureCount);
        Assert.NotNull(subscription.NextRetryAt);
        Assert.True(subscription.NextRetryAt >= beforeFailure.Add(backoffDelay));
        Assert.True(subscription.NextRetryAt <= afterFailure.Add(backoffDelay).AddSeconds(1));
    }

    [Fact]
    public void RecordFailure_Multiple_ShouldIncrementFailureCount()
    {
        // Arrange
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        // Act
        subscription.RecordFailure(TimeSpan.FromSeconds(1));
        subscription.RecordFailure(TimeSpan.FromSeconds(2));
        subscription.RecordFailure(TimeSpan.FromSeconds(4));

        // Assert
        Assert.Equal(3, subscription.FailureCount);
    }

    [Fact]
    public void Activate_ShouldSetActiveAndResetFailures()
    {
        // Arrange
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");
        subscription.Deactivate();
        subscription.RecordFailure(TimeSpan.FromSeconds(10));

        // Act
        subscription.Activate();

        // Assert
        Assert.True(subscription.IsActive);
        Assert.Equal(0, subscription.FailureCount);
        Assert.Null(subscription.NextRetryAt);
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        // Arrange
        var subscription = Subscription.Create(
            "Test",
            new[] { "Event1" },
            "https://example.com");

        // Act
        subscription.Deactivate();

        // Assert
        Assert.False(subscription.IsActive);
    }
}
