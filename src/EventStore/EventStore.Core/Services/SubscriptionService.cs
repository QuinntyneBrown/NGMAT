using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using Shared.Domain.Results;

namespace EventStore.Core.Services;

/// <summary>
/// Service for managing event subscriptions.
/// </summary>
public sealed class SubscriptionService
{
    private readonly IEventStoreUnitOfWork _unitOfWork;
    private readonly EventStoreOptions _options;

    public SubscriptionService(IEventStoreUnitOfWork unitOfWork, EventStoreOptions options)
    {
        _unitOfWork = unitOfWork;
        _options = options;
    }

    public async Task<Result<Subscription>> CreateSubscriptionAsync(
        string name,
        IEnumerable<string> eventTypes,
        string callbackUrl,
        IEnumerable<string>? aggregateTypes = null,
        CancellationToken cancellationToken = default)
    {
        // Check if subscription with same name exists
        var existing = await _unitOfWork.Subscriptions.GetByNameAsync(name, cancellationToken);
        if (existing != null)
        {
            return Error.Conflict($"Subscription with name '{name}' already exists");
        }

        var subscription = Subscription.Create(name, eventTypes, callbackUrl, aggregateTypes);
        var saved = await _unitOfWork.Subscriptions.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saved;
    }

    public async Task<Result<Subscription>> GetSubscriptionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
        {
            return Error.NotFound("Subscription", id.ToString());
        }

        return subscription;
    }

    public async Task<Result<IReadOnlyList<Subscription>>> GetActiveSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _unitOfWork.Subscriptions.GetActiveAsync(cancellationToken);
        return subscriptions.ToList();
    }

    public async Task<Result<IReadOnlyList<Subscription>>> GetSubscriptionsForEventAsync(
        string eventType,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _unitOfWork.Subscriptions.GetByEventTypeAsync(eventType, cancellationToken);
        return subscriptions.ToList();
    }

    public async Task<Result> ActivateSubscriptionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
        {
            return Error.NotFound("Subscription", id.ToString());
        }

        subscription.Activate();
        await _unitOfWork.Subscriptions.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateSubscriptionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
        {
            return Error.NotFound("Subscription", id.ToString());
        }

        subscription.Deactivate();
        await _unitOfWork.Subscriptions.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteSubscriptionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
        {
            return Error.NotFound("Subscription", id.ToString());
        }

        await _unitOfWork.Subscriptions.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RecordDeliveryAsync(
        Guid subscriptionId,
        long sequenceNumber,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            return Error.NotFound("Subscription", subscriptionId.ToString());
        }

        subscription.RecordDelivery(sequenceNumber);
        await _unitOfWork.Subscriptions.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RecordFailureAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
        {
            return Error.NotFound("Subscription", subscriptionId.ToString());
        }

        var backoffDelay = CalculateBackoffDelay(subscription.FailureCount);
        subscription.RecordFailure(backoffDelay);
        await _unitOfWork.Subscriptions.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private TimeSpan CalculateBackoffDelay(int failureCount)
    {
        var delay = _options.InitialRetryDelay.TotalMilliseconds * Math.Pow(_options.RetryBackoffMultiplier, failureCount);
        var maxDelay = TimeSpan.FromMinutes(30).TotalMilliseconds;
        return TimeSpan.FromMilliseconds(Math.Min(delay, maxDelay));
    }
}
