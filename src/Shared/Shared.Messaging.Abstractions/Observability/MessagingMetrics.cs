using System.Diagnostics.Metrics;

namespace Shared.Messaging.Abstractions.Observability;

/// <summary>
/// Metrics for messaging operations.
/// </summary>
public sealed class MessagingMetrics : IDisposable
{
    public const string MeterName = "NGMAT.Messaging";

    private readonly Meter _meter;
    private readonly Counter<long> _messagesPublished;
    private readonly Counter<long> _messagesReceived;
    private readonly Counter<long> _messagesFailed;
    private readonly Histogram<double> _publishDuration;
    private readonly Histogram<double> _processDuration;
    private readonly UpDownCounter<int> _activeSubscriptions;

    public MessagingMetrics()
    {
        _meter = new Meter(MeterName, "1.0.0");

        _messagesPublished = _meter.CreateCounter<long>(
            "ngmat.messaging.messages.published",
            description: "Number of messages published");

        _messagesReceived = _meter.CreateCounter<long>(
            "ngmat.messaging.messages.received",
            description: "Number of messages received");

        _messagesFailed = _meter.CreateCounter<long>(
            "ngmat.messaging.messages.failed",
            description: "Number of messages that failed processing");

        _publishDuration = _meter.CreateHistogram<double>(
            "ngmat.messaging.publish.duration",
            unit: "ms",
            description: "Duration of publish operations");

        _processDuration = _meter.CreateHistogram<double>(
            "ngmat.messaging.process.duration",
            unit: "ms",
            description: "Duration of message processing");

        _activeSubscriptions = _meter.CreateUpDownCounter<int>(
            "ngmat.messaging.subscriptions.active",
            description: "Number of active subscriptions");
    }

    public void RecordPublished(string eventType, string channel)
    {
        _messagesPublished.Add(1,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("channel", channel));
    }

    public void RecordReceived(string eventType, string channel)
    {
        _messagesReceived.Add(1,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("channel", channel));
    }

    public void RecordFailed(string eventType, string channel, string errorType)
    {
        _messagesFailed.Add(1,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("channel", channel),
            new KeyValuePair<string, object?>("error.type", errorType));
    }

    public void RecordPublishDuration(string eventType, double durationMs)
    {
        _publishDuration.Record(durationMs,
            new KeyValuePair<string, object?>("event.type", eventType));
    }

    public void RecordProcessDuration(string eventType, double durationMs, bool success)
    {
        _processDuration.Record(durationMs,
            new KeyValuePair<string, object?>("event.type", eventType),
            new KeyValuePair<string, object?>("success", success));
    }

    public void IncrementSubscriptions(string channel)
    {
        _activeSubscriptions.Add(1,
            new KeyValuePair<string, object?>("channel", channel));
    }

    public void DecrementSubscriptions(string channel)
    {
        _activeSubscriptions.Add(-1,
            new KeyValuePair<string, object?>("channel", channel));
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
