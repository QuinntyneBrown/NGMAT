using Notification.Core.Models;

namespace Notification.Core.Entities;

/// <summary>
/// Alert rule entity for triggering notifications based on conditions
/// </summary>
public class AlertRule
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Condition { get; private set; } = string.Empty;
    public string ConditionOperator { get; private set; } = string.Empty;
    public string ConditionValue { get; private set; } = string.Empty;
    public NotificationChannel NotificationChannel { get; private set; }
    public NotificationType NotificationType { get; private set; } = NotificationType.Warning;
    public string NotificationTitle { get; private set; } = string.Empty;
    public string NotificationMessage { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public int EvaluationIntervalMinutes { get; private set; } = 5;
    public DateTimeOffset? LastEvaluatedAt { get; private set; }
    public DateTimeOffset? LastTriggeredAt { get; private set; }
    public int TriggerCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private AlertRule() { } // For EF Core

    public static AlertRule Create(
        string userId,
        string name,
        string description,
        string condition,
        string conditionOperator,
        string conditionValue,
        NotificationChannel channel,
        string notificationTitle,
        string notificationMessage,
        NotificationType notificationType = NotificationType.Warning,
        int evaluationIntervalMinutes = 5)
    {
        return new AlertRule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = description,
            Condition = condition,
            ConditionOperator = conditionOperator,
            ConditionValue = conditionValue,
            NotificationChannel = channel,
            NotificationType = notificationType,
            NotificationTitle = notificationTitle,
            NotificationMessage = notificationMessage,
            IsEnabled = true,
            EvaluationIntervalMinutes = evaluationIntervalMinutes,
            TriggerCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        string? name = null,
        string? description = null,
        string? condition = null,
        string? conditionOperator = null,
        string? conditionValue = null,
        NotificationChannel? channel = null,
        string? notificationTitle = null,
        string? notificationMessage = null,
        int? evaluationIntervalMinutes = null)
    {
        if (name is not null) Name = name;
        if (description is not null) Description = description;
        if (condition is not null) Condition = condition;
        if (conditionOperator is not null) ConditionOperator = conditionOperator;
        if (conditionValue is not null) ConditionValue = conditionValue;
        if (channel.HasValue) NotificationChannel = channel.Value;
        if (notificationTitle is not null) NotificationTitle = notificationTitle;
        if (notificationMessage is not null) NotificationMessage = notificationMessage;
        if (evaluationIntervalMinutes.HasValue) EvaluationIntervalMinutes = evaluationIntervalMinutes.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;

    public void RecordEvaluation()
    {
        LastEvaluatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordTrigger()
    {
        LastTriggeredAt = DateTimeOffset.UtcNow;
        TriggerCount++;
    }

    public bool ShouldEvaluate()
    {
        if (!IsEnabled) return false;
        if (!LastEvaluatedAt.HasValue) return true;

        var nextEvaluationTime = LastEvaluatedAt.Value.AddMinutes(EvaluationIntervalMinutes);
        return DateTimeOffset.UtcNow >= nextEvaluationTime;
    }

    public bool Evaluate(double actualValue)
    {
        if (!double.TryParse(ConditionValue, out var threshold))
            return false;

        return ConditionOperator.ToLowerInvariant() switch
        {
            "lt" or "<" => actualValue < threshold,
            "lte" or "<=" => actualValue <= threshold,
            "gt" or ">" => actualValue > threshold,
            "gte" or ">=" => actualValue >= threshold,
            "eq" or "=" or "==" => Math.Abs(actualValue - threshold) < 0.0001,
            "neq" or "!=" or "<>" => Math.Abs(actualValue - threshold) >= 0.0001,
            _ => false
        };
    }
}
