using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Abstractions.Serialization;
using StackExchange.Redis;

namespace Shared.Messaging.Redis;

/// <summary>
/// Redis Pub/Sub implementation of the event bus for production use.
/// </summary>
public sealed class RedisEventBus : IEventBus
{
    private readonly RedisOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger<RedisEventBus> _logger;

    private IConnectionMultiplexer? _connection;
    private ISubscriber? _subscriber;
    private readonly ConcurrentDictionary<string, List<ChannelMessageQueue>> _subscriptions = new();
    private readonly object _lock = new();
    private bool _isStarted;

    /// <inheritdoc />
    public string ProviderName => "Redis";

    /// <inheritdoc />
    public bool IsConnected => _isStarted && _connection?.IsConnected == true;

    public RedisEventBus(
        IOptions<RedisOptions> options,
        IMessageSerializer serializer,
        ILogger<RedisEventBus> logger)
    {
        _options = options.Value;
        _serializer = serializer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isStarted)
        {
            return;
        }

        lock (_lock)
        {
            if (_isStarted)
            {
                return;
            }

            var configOptions = ConfigurationOptions.Parse(_options.ConnectionString);
            configOptions.ConnectTimeout = _options.ConnectTimeout;
            configOptions.SyncTimeout = _options.SyncTimeout;
            configOptions.AbortOnConnectFail = _options.AbortOnConnectFail;
            configOptions.ConnectRetry = _options.ConnectRetry;
            configOptions.Ssl = _options.UseSsl;
            configOptions.ClientName = _options.ClientName;
            configOptions.AllowAdmin = _options.AllowAdmin;
            configOptions.DefaultDatabase = _options.DefaultDatabase;
            configOptions.KeepAlive = _options.KeepAlive;

            if (!string.IsNullOrEmpty(_options.Password))
            {
                configOptions.Password = _options.Password;
            }

            _connection = ConnectionMultiplexer.Connect(configOptions);
            _connection.ConnectionFailed += OnConnectionFailed;
            _connection.ConnectionRestored += OnConnectionRestored;
            _connection.ErrorMessage += OnErrorMessage;

            _subscriber = _connection.GetSubscriber();
            _isStarted = true;
        }

        _logger.LogInformation(
            "Redis event bus started, connected to {Endpoints}",
            string.Join(", ", _connection!.GetEndPoints().Select(e => e.ToString())));

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isStarted)
        {
            return;
        }

        lock (_lock)
        {
            if (!_isStarted)
            {
                return;
            }

            _isStarted = false;
        }

        // Unsubscribe all
        foreach (var (channel, queues) in _subscriptions)
        {
            foreach (var queue in queues)
            {
                await queue.UnsubscribeAsync();
            }
        }
        _subscriptions.Clear();

        if (_connection != null)
        {
            _connection.ConnectionFailed -= OnConnectionFailed;
            _connection.ConnectionRestored -= OnConnectionRestored;
            _connection.ErrorMessage -= OnErrorMessage;
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _connection = null;
        _subscriber = null;

        _logger.LogInformation("Redis event bus stopped");
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var channel = GetChannelName<TEvent>();
        await PublishAsync(channel, @event, cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(string channel, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        EnsureStarted();

        var envelope = EventEnvelope.Create(@event, _serializer);
        var data = _serializer.Serialize(envelope);

        var fullChannel = $"{_options.ChannelPrefix}{channel}";
        await _subscriber!.PublishAsync(RedisChannel.Literal(fullChannel), data);

        _logger.LogDebug("Published event {EventType} to channel {Channel}", typeof(TEvent).Name, fullChannel);
    }

    /// <inheritdoc />
    public async Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable> SubscribeAsync<TEvent>(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var channel = GetChannelName<TEvent>();
        return await SubscribeAsync(channel, handler, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable> SubscribeAsync<TEvent>(
        string channel,
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        EnsureStarted();

        var fullChannel = $"{_options.ChannelPrefix}{channel}";
        var queue = await _subscriber!.SubscribeAsync(RedisChannel.Literal(fullChannel));

        _ = ProcessMessagesAsync(queue, handler, cancellationToken);

        var queues = _subscriptions.GetOrAdd(fullChannel, _ => new List<ChannelMessageQueue>());
        lock (queues)
        {
            queues.Add(queue);
        }

        _logger.LogDebug("Subscribed to channel {Channel} for event type {EventType}", fullChannel, typeof(TEvent).Name);

        return new Subscription(async () =>
        {
            await queue.UnsubscribeAsync();
            lock (queues)
            {
                queues.Remove(queue);
            }
        });
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable> SubscribePatternAsync<TEvent>(
        string pattern,
        Func<TEvent, string, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        EnsureStarted();

        var fullPattern = $"{_options.ChannelPrefix}{pattern}";
        var queue = await _subscriber!.SubscribeAsync(RedisChannel.Pattern(fullPattern));

        _ = ProcessPatternMessagesAsync(queue, handler, cancellationToken);

        var queues = _subscriptions.GetOrAdd($"pattern:{fullPattern}", _ => new List<ChannelMessageQueue>());
        lock (queues)
        {
            queues.Add(queue);
        }

        _logger.LogDebug("Subscribed to pattern {Pattern} for event type {EventType}", fullPattern, typeof(TEvent).Name);

        return new Subscription(async () =>
        {
            await queue.UnsubscribeAsync();
            lock (queues)
            {
                queues.Remove(queue);
            }
        });
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private async Task ProcessMessagesAsync<TEvent>(
        ChannelMessageQueue queue,
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
        where TEvent : class, IEvent
    {
        await foreach (var message in queue.WithCancellation(cancellationToken))
        {
            try
            {
                var data = (byte[])message.Message!;
                var envelope = _serializer.Deserialize<EventEnvelope>(data);
                var @event = envelope.GetEvent<TEvent>(_serializer);

                if (@event != null)
                {
                    await handler(@event, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from channel {Channel}", message.Channel);
            }
        }
    }

    private async Task ProcessPatternMessagesAsync<TEvent>(
        ChannelMessageQueue queue,
        Func<TEvent, string, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
        where TEvent : class, IEvent
    {
        await foreach (var message in queue.WithCancellation(cancellationToken))
        {
            try
            {
                var data = (byte[])message.Message!;
                var envelope = _serializer.Deserialize<EventEnvelope>(data);
                var @event = envelope.GetEvent<TEvent>(_serializer);

                if (@event != null)
                {
                    var channel = message.Channel.ToString();
                    // Remove prefix to get the logical channel name
                    if (channel.StartsWith(_options.ChannelPrefix))
                    {
                        channel = channel[_options.ChannelPrefix.Length..];
                    }
                    await handler(@event, channel, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from channel {Channel}", message.Channel);
            }
        }
    }

    private void OnConnectionFailed(object? sender, ConnectionFailedEventArgs e)
    {
        _logger.LogError(e.Exception, "Redis connection failed to {EndPoint}: {FailureType}",
            e.EndPoint, e.FailureType);
    }

    private void OnConnectionRestored(object? sender, ConnectionFailedEventArgs e)
    {
        _logger.LogInformation("Redis connection restored to {EndPoint}", e.EndPoint);
    }

    private void OnErrorMessage(object? sender, RedisErrorEventArgs e)
    {
        _logger.LogError("Redis error from {EndPoint}: {Message}", e.EndPoint, e.Message);
    }

    private void EnsureStarted()
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException("Event bus has not been started. Call StartAsync first.");
        }
    }

    private string GetChannelName<TEvent>() where TEvent : class, IEvent
    {
        var type = typeof(TEvent);
        return $"{type.Namespace}.{type.Name}".ToLowerInvariant().Replace(".", ":");
    }

    private sealed class Subscription : IAsyncDisposable
    {
        private readonly Func<Task> _unsubscribe;
        private bool _disposed;

        public Subscription(Func<Task> unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                await _unsubscribe();
            }
        }
    }
}
