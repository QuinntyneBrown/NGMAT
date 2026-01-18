using K4os.Compression.LZ4;
using MessagePack;
using MessagePack.Resolvers;

namespace Shared.Messaging.Abstractions.Serialization;

/// <summary>
/// MessagePack serializer with LZ4 compression for high-performance event serialization.
/// </summary>
public sealed class MessagePackEventSerializer : IMessageSerializer
{
    private readonly MessagePackSerializerOptions _options;
    private readonly bool _useCompression;
    private readonly int _compressionThreshold;

    /// <summary>
    /// Magic bytes to identify compressed messages.
    /// </summary>
    private static readonly byte[] CompressionMagic = [0x4C, 0x5A, 0x34, 0x00]; // "LZ4\0"

    /// <summary>
    /// Creates a new MessagePack serializer with optional LZ4 compression.
    /// </summary>
    /// <param name="useCompression">Whether to use LZ4 compression.</param>
    /// <param name="compressionThreshold">Minimum size in bytes before compression is applied.</param>
    public MessagePackEventSerializer(bool useCompression = true, int compressionThreshold = 256)
    {
        _useCompression = useCompression;
        _compressionThreshold = compressionThreshold;

        // Configure MessagePack with standard resolver and security settings
        _options = MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(
                StandardResolver.Instance,
                ContractlessStandardResolver.Instance))
            .WithSecurity(MessagePackSecurity.UntrustedData);
    }

    /// <inheritdoc />
    public string ContentType => "application/x-msgpack+lz4";

    /// <inheritdoc />
    public byte[] Serialize<T>(T value)
    {
        var msgpackData = MessagePackSerializer.Serialize(value, _options);

        if (!_useCompression || msgpackData.Length < _compressionThreshold)
        {
            return msgpackData;
        }

        // Compress with LZ4
        var maxCompressedSize = LZ4Codec.MaximumOutputSize(msgpackData.Length);
        var compressedBuffer = new byte[CompressionMagic.Length + 4 + maxCompressedSize];

        // Write magic bytes
        CompressionMagic.CopyTo(compressedBuffer, 0);

        // Write original size (4 bytes, little-endian)
        BitConverter.GetBytes(msgpackData.Length).CopyTo(compressedBuffer, CompressionMagic.Length);

        // Compress
        var compressedSize = LZ4Codec.Encode(
            msgpackData, 0, msgpackData.Length,
            compressedBuffer, CompressionMagic.Length + 4, maxCompressedSize);

        // Only use compression if it actually reduces size
        if (compressedSize + CompressionMagic.Length + 4 >= msgpackData.Length)
        {
            return msgpackData;
        }

        // Resize to actual compressed size
        var result = new byte[CompressionMagic.Length + 4 + compressedSize];
        Array.Copy(compressedBuffer, result, result.Length);
        return result;
    }

    /// <inheritdoc />
    public T Deserialize<T>(byte[] data)
    {
        var decompressedData = DecompressIfNeeded(data);
        return MessagePackSerializer.Deserialize<T>(decompressedData, _options);
    }

    /// <inheritdoc />
    public object? Deserialize(byte[] data, Type type)
    {
        var decompressedData = DecompressIfNeeded(data);
        return MessagePackSerializer.Deserialize(type, decompressedData, _options);
    }

    private byte[] DecompressIfNeeded(byte[] data)
    {
        if (data.Length < CompressionMagic.Length + 4)
        {
            return data;
        }

        // Check for compression magic bytes
        for (int i = 0; i < CompressionMagic.Length; i++)
        {
            if (data[i] != CompressionMagic[i])
            {
                return data; // Not compressed
            }
        }

        // Read original size
        var originalSize = BitConverter.ToInt32(data, CompressionMagic.Length);
        var decompressedBuffer = new byte[originalSize];

        // Decompress
        var decompressedSize = LZ4Codec.Decode(
            data, CompressionMagic.Length + 4, data.Length - CompressionMagic.Length - 4,
            decompressedBuffer, 0, originalSize);

        if (decompressedSize != originalSize)
        {
            throw new InvalidOperationException(
                $"Decompression size mismatch. Expected {originalSize}, got {decompressedSize}");
        }

        return decompressedBuffer;
    }
}
