using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;

namespace Shared.Domain.Identifiers;

/// <summary>
/// Base interface for strongly-typed identifiers.
/// </summary>
public interface IStronglyTypedId
{
    Guid Value { get; }
}

/// <summary>
/// Strongly-typed identifier for missions.
/// </summary>
[MessagePackObject]
public readonly struct MissionId : IStronglyTypedId, IEquatable<MissionId>
{
    [Key(0)]
    public Guid Value { get; }

    [SerializationConstructor]
    public MissionId(Guid value) => Value = value;

    public static MissionId New() => new(Guid.NewGuid());
    public static MissionId Empty => new(Guid.Empty);
    public static MissionId Parse(string value) => new(Guid.Parse(value));

    public static implicit operator Guid(MissionId id) => id.Value;
    public static explicit operator MissionId(Guid id) => new(id);

    public bool Equals(MissionId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is MissionId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
    public static bool operator ==(MissionId left, MissionId right) => left.Equals(right);
    public static bool operator !=(MissionId left, MissionId right) => !left.Equals(right);
}

/// <summary>
/// Strongly-typed identifier for spacecraft.
/// </summary>
[MessagePackObject]
public readonly struct SpacecraftId : IStronglyTypedId, IEquatable<SpacecraftId>
{
    [Key(0)]
    public Guid Value { get; }

    [SerializationConstructor]
    public SpacecraftId(Guid value) => Value = value;

    public static SpacecraftId New() => new(Guid.NewGuid());
    public static SpacecraftId Empty => new(Guid.Empty);
    public static SpacecraftId Parse(string value) => new(Guid.Parse(value));

    public static implicit operator Guid(SpacecraftId id) => id.Value;
    public static explicit operator SpacecraftId(Guid id) => new(id);

    public bool Equals(SpacecraftId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is SpacecraftId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
    public static bool operator ==(SpacecraftId left, SpacecraftId right) => left.Equals(right);
    public static bool operator !=(SpacecraftId left, SpacecraftId right) => !left.Equals(right);
}

/// <summary>
/// Strongly-typed identifier for users.
/// </summary>
[MessagePackObject]
public readonly struct UserId : IStronglyTypedId, IEquatable<UserId>
{
    [Key(0)]
    public Guid Value { get; }

    [SerializationConstructor]
    public UserId(Guid value) => Value = value;

    public static UserId New() => new(Guid.NewGuid());
    public static UserId Empty => new(Guid.Empty);
    public static UserId Parse(string value) => new(Guid.Parse(value));

    public static implicit operator Guid(UserId id) => id.Value;
    public static explicit operator UserId(Guid id) => new(id);

    public bool Equals(UserId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is UserId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
    public static bool operator ==(UserId left, UserId right) => left.Equals(right);
    public static bool operator !=(UserId left, UserId right) => !left.Equals(right);
}

/// <summary>
/// Strongly-typed identifier for propagations.
/// </summary>
[MessagePackObject]
public readonly struct PropagationId : IStronglyTypedId, IEquatable<PropagationId>
{
    [Key(0)]
    public Guid Value { get; }

    [SerializationConstructor]
    public PropagationId(Guid value) => Value = value;

    public static PropagationId New() => new(Guid.NewGuid());
    public static PropagationId Empty => new(Guid.Empty);
    public static PropagationId Parse(string value) => new(Guid.Parse(value));

    public static implicit operator Guid(PropagationId id) => id.Value;
    public static explicit operator PropagationId(Guid id) => new(id);

    public bool Equals(PropagationId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is PropagationId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
    public static bool operator ==(PropagationId left, PropagationId right) => left.Equals(right);
    public static bool operator !=(PropagationId left, PropagationId right) => !left.Equals(right);
}

/// <summary>
/// Strongly-typed identifier for maneuvers.
/// </summary>
[MessagePackObject]
public readonly struct ManeuverId : IStronglyTypedId, IEquatable<ManeuverId>
{
    [Key(0)]
    public Guid Value { get; }

    [SerializationConstructor]
    public ManeuverId(Guid value) => Value = value;

    public static ManeuverId New() => new(Guid.NewGuid());
    public static ManeuverId Empty => new(Guid.Empty);
    public static ManeuverId Parse(string value) => new(Guid.Parse(value));

    public static implicit operator Guid(ManeuverId id) => id.Value;
    public static explicit operator ManeuverId(Guid id) => new(id);

    public bool Equals(ManeuverId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is ManeuverId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
    public static bool operator ==(ManeuverId left, ManeuverId right) => left.Equals(right);
    public static bool operator !=(ManeuverId left, ManeuverId right) => !left.Equals(right);
}

/// <summary>
/// JSON converter factory for strongly-typed IDs.
/// </summary>
public sealed class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(MissionId) ||
               typeToConvert == typeof(SpacecraftId) ||
               typeToConvert == typeof(UserId) ||
               typeToConvert == typeof(PropagationId) ||
               typeToConvert == typeof(ManeuverId);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(MissionId))
            return new StronglyTypedIdJsonConverter<MissionId>(v => new MissionId(v));
        if (typeToConvert == typeof(SpacecraftId))
            return new StronglyTypedIdJsonConverter<SpacecraftId>(v => new SpacecraftId(v));
        if (typeToConvert == typeof(UserId))
            return new StronglyTypedIdJsonConverter<UserId>(v => new UserId(v));
        if (typeToConvert == typeof(PropagationId))
            return new StronglyTypedIdJsonConverter<PropagationId>(v => new PropagationId(v));
        if (typeToConvert == typeof(ManeuverId))
            return new StronglyTypedIdJsonConverter<ManeuverId>(v => new ManeuverId(v));

        throw new NotSupportedException($"Type {typeToConvert} is not supported");
    }
}

/// <summary>
/// JSON converter for strongly-typed IDs.
/// </summary>
public sealed class StronglyTypedIdJsonConverter<T> : JsonConverter<T>
    where T : IStronglyTypedId
{
    private readonly Func<Guid, T> _factory;

    public StronglyTypedIdJsonConverter(Func<Guid, T> factory)
    {
        _factory = factory;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetGuid();
        return _factory(value);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
