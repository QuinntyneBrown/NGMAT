using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Shared.Messaging.Abstractions.Resilience;

/// <summary>
/// Wraps an event handler with retry and circuit breaker policies.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
public sealed class ResilientEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : class, IEvent
{
    private readonly IEventHandler<TEvent> _innerHandler;
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger _logger;

    public ResilientEventHandler(
        IEventHandler<TEvent> innerHandler,
        RetryOptions retryOptions,
        CircuitBreakerOptions circuitBreakerOptions,
        ILogger logger)
    {
        _innerHandler = innerHandler;
        _logger = logger;
        _pipeline = BuildPipeline(retryOptions, circuitBreakerOptions);
    }

    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        await _pipeline.ExecuteAsync(
            async ct => await _innerHandler.HandleAsync(@event, ct),
            cancellationToken);
    }

    private ResiliencePipeline BuildPipeline(RetryOptions retry, CircuitBreakerOptions circuitBreaker)
    {
        var builder = new ResiliencePipelineBuilder();

        // Add retry policy
        builder.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = retry.MaxRetryAttempts,
            Delay = retry.InitialDelay,
            MaxDelay = retry.MaxDelay,
            BackoffType = retry.BackoffType switch
            {
                BackoffType.Constant => DelayBackoffType.Constant,
                BackoffType.Linear => DelayBackoffType.Linear,
                BackoffType.Exponential => DelayBackoffType.Exponential,
                _ => DelayBackoffType.Exponential
            },
            UseJitter = retry.UseJitter,
            OnRetry = args =>
            {
                _logger.LogWarning(
                    args.Outcome.Exception,
                    "Retry {RetryAttempt} for event {EventType} after {Delay}ms",
                    args.AttemptNumber,
                    typeof(TEvent).Name,
                    args.RetryDelay.TotalMilliseconds);
                return ValueTask.CompletedTask;
            }
        });

        // Add circuit breaker
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = (double)circuitBreaker.FailureThreshold / circuitBreaker.MinimumThroughput,
            SamplingDuration = circuitBreaker.SamplingDuration,
            MinimumThroughput = circuitBreaker.MinimumThroughput,
            BreakDuration = circuitBreaker.BreakDuration,
            OnOpened = args =>
            {
                _logger.LogError(
                    args.Outcome.Exception,
                    "Circuit breaker opened for event {EventType}",
                    typeof(TEvent).Name);
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                _logger.LogInformation(
                    "Circuit breaker closed for event {EventType}",
                    typeof(TEvent).Name);
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                _logger.LogInformation(
                    "Circuit breaker half-opened for event {EventType}",
                    typeof(TEvent).Name);
                return ValueTask.CompletedTask;
            }
        });

        return builder.Build();
    }
}
