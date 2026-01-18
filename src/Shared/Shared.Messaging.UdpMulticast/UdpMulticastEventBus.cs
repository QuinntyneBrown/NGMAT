using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Abstractions.Serialization;

namespace Shared.Messaging.UdpMulticast;

/// <summary>
/// UDP Multicast implementation of the event bus for local development.
/// Zero external dependencies - works entirely over the local network.
/// </summary>
public sealed class UdpMulticastEventBus : IEventBus
{
    private readonly UdpMulticastOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger<UdpMulticastEventBus> _logger;

    private UdpClient? _sendClient;
    private UdpClient? _receiveClient;
    private IPEndPoint? _multicastEndPoint;
    private CancellationTokenSource? _receiveCts;
    private Task? _receiveTask;

    private readonly ConcurrentDictionary<string, List<Func<byte[], CancellationToken, Task>>> _handlers = new();
    private readonly ConcurrentDictionary<Guid, DateTime> _processedMessages = new();
    private readonly object _lock = new();
    private bool _isStarted;

    /// <inheritdoc />
    public string ProviderName => "UdpMulticast";

    /// <inheritdoc />
    public bool IsConnected => _isStarted && _sendClient != null && _receiveClient != null;

    public UdpMulticastEventBus(
        IOptions<UdpMulticastOptions> options,
        IMessageSerializer serializer,
        ILogger<UdpMulticastEventBus> logger)
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

            var multicastAddress = IPAddress.Parse(_options.MulticastGroup);
            _multicastEndPoint = new IPEndPoint(multicastAddress, _options.Port);

            // Create send client
            _sendClient = new UdpClient();
            _sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sendClient.Client.SendBufferSize = _options.SendBufferSize;
            _sendClient.Ttl = (short)_options.TimeToLive;
            _sendClient.MulticastLoopback = _options.EnableLoopback;

            // Create receive client
            _receiveClient = new UdpClient();
            _receiveClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _receiveClient.Client.ReceiveBufferSize = _options.ReceiveBufferSize;
            _receiveClient.Client.Bind(new IPEndPoint(IPAddress.Any, _options.Port));
            _receiveClient.JoinMulticastGroup(multicastAddress);

            _receiveCts = new CancellationTokenSource();
            _isStarted = true;
        }

        _receiveTask = ReceiveLoopAsync(_receiveCts.Token);

        _logger.LogInformation(
            "UDP Multicast event bus started on {MulticastGroup}:{Port}",
            _options.MulticastGroup,
            _options.Port);

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
            _receiveCts?.Cancel();
        }

        if (_receiveTask != null)
        {
            try
            {
                await _receiveTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Receive loop did not complete in time");
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        _receiveClient?.Close();
        _sendClient?.Close();
        _receiveCts?.Dispose();

        _receiveClient = null;
        _sendClient = null;
        _receiveCts = null;

        _logger.LogInformation("UDP Multicast event bus stopped");
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
        var message = new UdpMessage
        {
            MessageId = @event.EventId,
            Channel = channel,
            Payload = _serializer.Serialize(envelope)
        };

        var data = _serializer.Serialize(message);

        if (data.Length > _options.MaxMessageSize)
        {
            throw new InvalidOperationException(
                $"Message size {data.Length} exceeds maximum {_options.MaxMessageSize}");
        }

        await _sendClient!.SendAsync(data, data.Length, _multicastEndPoint!);

        _logger.LogDebug("Published event {EventType} to channel {Channel}", typeof(TEvent).Name, channel);
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
    public Task<IAsyncDisposable> SubscribeAsync<TEvent>(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var channel = GetChannelName<TEvent>();
        return SubscribeAsync(channel, handler, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IAsyncDisposable> SubscribeAsync<TEvent>(
        string channel,
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        EnsureStarted();

        async Task Handler(byte[] data, CancellationToken ct)
        {
            var envelope = _serializer.Deserialize<EventEnvelope>(data);
            var @event = envelope.GetEvent<TEvent>(_serializer);
            if (@event != null)
            {
                await handler(@event, ct);
            }
        }

        var handlers = _handlers.GetOrAdd(channel, _ => new List<Func<byte[], CancellationToken, Task>>());
        lock (handlers)
        {
            handlers.Add(Handler);
        }

        _logger.LogDebug("Subscribed to channel {Channel} for event type {EventType}", channel, typeof(TEvent).Name);

        return Task.FromResult<IAsyncDisposable>(new Subscription(() =>
        {
            lock (handlers)
            {
                handlers.Remove(Handler);
            }
        }));
    }

    /// <inheritdoc />
    public Task<IAsyncDisposable> SubscribePatternAsync<TEvent>(
        string pattern,
        Func<TEvent, string, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        EnsureStarted();

        // For UDP multicast, pattern subscriptions are handled by matching all channels
        // This is a simplified implementation - production would need more sophisticated routing
        async Task Handler(byte[] data, CancellationToken ct)
        {
            var envelope = _serializer.Deserialize<EventEnvelope>(data);
            var @event = envelope.GetEvent<TEvent>(_serializer);
            if (@event != null)
            {
                await handler(@event, pattern, ct);
            }
        }

        var handlers = _handlers.GetOrAdd($"pattern:{pattern}", _ => new List<Func<byte[], CancellationToken, Task>>());
        lock (handlers)
        {
            handlers.Add(Handler);
        }

        return Task.FromResult<IAsyncDisposable>(new Subscription(() =>
        {
            lock (handlers)
            {
                handlers.Remove(Handler);
            }
        }));
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _receiveClient != null)
        {
            try
            {
                var result = await _receiveClient.ReceiveAsync(cancellationToken);
                _ = ProcessMessageAsync(result.Buffer, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving UDP multicast message");
            }
        }
    }

    private async Task ProcessMessageAsync(byte[] data, CancellationToken cancellationToken)
    {
        try
        {
            var message = _serializer.Deserialize<UdpMessage>(data);

            // Deduplication
            if (!TryAddProcessedMessage(message.MessageId))
            {
                return;
            }

            // Find matching handlers
            var matchingHandlers = new List<Func<byte[], CancellationToken, Task>>();

            if (_handlers.TryGetValue(message.Channel, out var channelHandlers))
            {
                lock (channelHandlers)
                {
                    matchingHandlers.AddRange(channelHandlers);
                }
            }

            // Check pattern handlers
            foreach (var (key, patternHandlers) in _handlers)
            {
                if (key.StartsWith("pattern:"))
                {
                    var pattern = key[8..];
                    if (MatchesPattern(message.Channel, pattern))
                    {
                        lock (patternHandlers)
                        {
                            matchingHandlers.AddRange(patternHandlers);
                        }
                    }
                }
            }

            // Execute handlers
            foreach (var handler in matchingHandlers)
            {
                try
                {
                    await handler(message.Payload, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing handler for channel {Channel}", message.Channel);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UDP multicast message");
        }
    }

    private bool TryAddProcessedMessage(Guid messageId)
    {
        var now = DateTime.UtcNow;

        // Clean up old entries
        var expiry = now.AddMilliseconds(-_options.DeduplicationTimeoutMs);
        var toRemove = _processedMessages
            .Where(kvp => kvp.Value < expiry)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in toRemove)
        {
            _processedMessages.TryRemove(key, out _);
        }

        // Limit size
        while (_processedMessages.Count >= _options.DeduplicationWindowSize)
        {
            var oldest = _processedMessages.OrderBy(kvp => kvp.Value).First();
            _processedMessages.TryRemove(oldest.Key, out _);
        }

        return _processedMessages.TryAdd(messageId, now);
    }

    private static bool MatchesPattern(string channel, string pattern)
    {
        // Simple wildcard matching: * matches any sequence
        if (pattern == "*")
        {
            return true;
        }

        if (pattern.EndsWith(".*"))
        {
            var prefix = pattern[..^2];
            return channel.StartsWith(prefix + ".", StringComparison.Ordinal) ||
                   channel.Equals(prefix, StringComparison.Ordinal);
        }

        return channel.Equals(pattern, StringComparison.Ordinal);
    }

    private void EnsureStarted()
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException("Event bus has not been started. Call StartAsync first.");
        }
    }

    private static string GetChannelName<TEvent>() where TEvent : class, IEvent
    {
        var type = typeof(TEvent);
        return $"{type.Namespace}.{type.Name}".ToLowerInvariant().Replace(".", ":");
    }

    private sealed class Subscription : IAsyncDisposable
    {
        private readonly Action _unsubscribe;
        private bool _disposed;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                _unsubscribe();
            }
            return ValueTask.CompletedTask;
        }
    }
}
