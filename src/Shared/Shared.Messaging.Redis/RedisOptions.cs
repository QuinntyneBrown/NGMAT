using System.ComponentModel.DataAnnotations;

namespace Shared.Messaging.Redis;

/// <summary>
/// Configuration options for Redis Pub/Sub messaging.
/// </summary>
public sealed class RedisOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Messaging:Redis";

    /// <summary>
    /// Redis connection string. Default: localhost:6379
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Prefix for all channel names. Default: ngmat:
    /// </summary>
    public string ChannelPrefix { get; set; } = "ngmat:";

    /// <summary>
    /// Connection timeout in milliseconds. Default: 5000
    /// </summary>
    [Range(1000, 60000)]
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Sync timeout in milliseconds. Default: 5000
    /// </summary>
    [Range(1000, 60000)]
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Whether to abort on connect failure. Default: false
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Number of times to retry connection. Default: 3
    /// </summary>
    [Range(0, 10)]
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// Whether to use SSL/TLS. Default: false
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Password for Redis authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Client name for identification. Default: ngmat-client
    /// </summary>
    public string ClientName { get; set; } = "ngmat-client";

    /// <summary>
    /// Whether to allow admin operations. Default: false
    /// </summary>
    public bool AllowAdmin { get; set; } = false;

    /// <summary>
    /// Default database index. Default: 0
    /// </summary>
    [Range(0, 15)]
    public int DefaultDatabase { get; set; } = 0;

    /// <summary>
    /// Keep-alive interval in seconds. Default: 60
    /// </summary>
    [Range(0, 300)]
    public int KeepAlive { get; set; } = 60;

    /// <summary>
    /// Whether to use Redis Streams for message persistence. Default: false
    /// </summary>
    public bool UseStreamsForPersistence { get; set; } = false;

    /// <summary>
    /// Maximum length of Redis Streams (when enabled). Default: 10000
    /// </summary>
    [Range(1000, 1000000)]
    public int StreamMaxLength { get; set; } = 10000;
}
