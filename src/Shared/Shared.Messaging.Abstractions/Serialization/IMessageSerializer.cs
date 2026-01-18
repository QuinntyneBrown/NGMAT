namespace Shared.Messaging.Abstractions.Serialization;

/// <summary>
/// Interface for serializing and deserializing messages.
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Serializes an object to a byte array.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized byte array.</returns>
    byte[] Serialize<T>(T value);

    /// <summary>
    /// Deserializes a byte array to an object.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize to.</typeparam>
    /// <param name="data">The byte array to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    T Deserialize<T>(byte[] data);

    /// <summary>
    /// Deserializes a byte array to an object of the specified type.
    /// </summary>
    /// <param name="data">The byte array to deserialize.</param>
    /// <param name="type">The type to deserialize to.</param>
    /// <returns>The deserialized object.</returns>
    object? Deserialize(byte[] data, Type type);

    /// <summary>
    /// Gets the content type for this serializer (e.g., "application/x-msgpack").
    /// </summary>
    string ContentType { get; }
}
