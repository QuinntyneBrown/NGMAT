using CoordinateSystem.Core.Entities;
using MessagePack;
using Shared.Messaging.Abstractions;

namespace CoordinateSystem.Core.Events;

[MessagePackObject]
public sealed class CoordinateSystemCreatedEvent : EventBase
{
    [Key(10)]
    public Guid ReferenceFrameId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public ReferenceFrameType Type { get; init; }

    [Key(13)]
    public CentralBody CentralBody { get; init; }

    [Key(14)]
    public AxesDefinition Axes { get; init; }

    [Key(15)]
    public Guid? CreatedByUserId { get; init; }
}

[MessagePackObject]
public sealed class StateVectorTransformedEvent : EventBase
{
    [Key(10)]
    public Guid SourceFrameId { get; init; }

    [Key(11)]
    public Guid TargetFrameId { get; init; }

    [Key(12)]
    public DateTime Epoch { get; init; }

    [Key(13)]
    public double[] InputState { get; init; } = Array.Empty<double>();

    [Key(14)]
    public double[] OutputState { get; init; } = Array.Empty<double>();

    [Key(15)]
    public Guid? RequestedByUserId { get; init; }
}

[MessagePackObject]
public sealed class KeplerianConversionEvent : EventBase
{
    [Key(10)]
    public string ConversionType { get; init; } = string.Empty;

    [Key(11)]
    public double[] InputData { get; init; } = Array.Empty<double>();

    [Key(12)]
    public double[] OutputData { get; init; } = Array.Empty<double>();

    [Key(13)]
    public double Mu { get; init; }

    [Key(14)]
    public Guid? RequestedByUserId { get; init; }
}

[MessagePackObject]
public sealed class GeodeticConversionEvent : EventBase
{
    [Key(10)]
    public string ConversionType { get; init; } = string.Empty;

    [Key(11)]
    public double[] InputData { get; init; } = Array.Empty<double>();

    [Key(12)]
    public double[] OutputData { get; init; } = Array.Empty<double>();

    [Key(13)]
    public Guid? RequestedByUserId { get; init; }
}
