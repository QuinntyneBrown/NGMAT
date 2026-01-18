using System.Diagnostics;

namespace Shared.Messaging.Abstractions.Observability;

/// <summary>
/// Activity source for distributed tracing of messaging operations.
/// </summary>
public static class MessagingActivitySource
{
    public const string SourceName = "NGMAT.Messaging";

    private static readonly ActivitySource Source = new(SourceName, "1.0.0");

    /// <summary>
    /// Starts a new activity for publishing an event.
    /// </summary>
    public static Activity? StartPublishActivity<TEvent>(string channel) where TEvent : IEvent
    {
        var activity = Source.StartActivity(
            $"publish {typeof(TEvent).Name}",
            ActivityKind.Producer);

        activity?.SetTag("messaging.system", "ngmat");
        activity?.SetTag("messaging.operation", "publish");
        activity?.SetTag("messaging.destination", channel);
        activity?.SetTag("messaging.message.type", typeof(TEvent).FullName);

        return activity;
    }

    /// <summary>
    /// Starts a new activity for receiving an event.
    /// </summary>
    public static Activity? StartReceiveActivity<TEvent>(string channel, string? parentId = null)
        where TEvent : IEvent
    {
        ActivityContext parentContext = default;
        if (!string.IsNullOrEmpty(parentId) && ActivityContext.TryParse(parentId, null, out var parsed))
        {
            parentContext = parsed;
        }

        var activity = Source.StartActivity(
            $"receive {typeof(TEvent).Name}",
            ActivityKind.Consumer,
            parentContext);

        activity?.SetTag("messaging.system", "ngmat");
        activity?.SetTag("messaging.operation", "receive");
        activity?.SetTag("messaging.destination", channel);
        activity?.SetTag("messaging.message.type", typeof(TEvent).FullName);

        return activity;
    }

    /// <summary>
    /// Starts a new activity for processing an event.
    /// </summary>
    public static Activity? StartProcessActivity<TEvent>(IEvent @event) where TEvent : IEvent
    {
        var activity = Source.StartActivity(
            $"process {typeof(TEvent).Name}",
            ActivityKind.Internal);

        activity?.SetTag("messaging.system", "ngmat");
        activity?.SetTag("messaging.operation", "process");
        activity?.SetTag("messaging.message.id", @event.EventId.ToString());
        activity?.SetTag("messaging.message.type", typeof(TEvent).FullName);

        if (@event.CorrelationId != null)
        {
            activity?.SetTag("messaging.correlation.id", @event.CorrelationId);
        }

        return activity;
    }

    /// <summary>
    /// Records an exception on the current activity.
    /// </summary>
    public static void RecordException(Activity? activity, Exception exception)
    {
        if (activity == null) return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("exception.type", exception.GetType().FullName);
        activity.SetTag("exception.message", exception.Message);
        activity.SetTag("exception.stacktrace", exception.StackTrace);
    }

    /// <summary>
    /// Marks the activity as successful.
    /// </summary>
    public static void SetSuccess(Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
