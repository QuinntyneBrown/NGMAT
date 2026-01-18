using System.ComponentModel.DataAnnotations;

namespace Shared.Messaging.UdpMulticast;

/// <summary>
/// Configuration options for UDP Multicast messaging.
/// </summary>
public sealed class UdpMulticastOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Messaging:UdpMulticast";

    /// <summary>
    /// The multicast group IP address. Default: 239.0.0.1
    /// Must be in the range 224.0.0.0 to 239.255.255.255.
    /// </summary>
    [Required]
    public string MulticastGroup { get; set; } = "239.0.0.1";

    /// <summary>
    /// The port to use for multicast communication. Default: 5000
    /// </summary>
    [Range(1024, 65535)]
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Time To Live for multicast packets. Default: 1 (local network only)
    /// Higher values allow packets to traverse routers.
    /// </summary>
    [Range(1, 255)]
    public int TimeToLive { get; set; } = 1;

    /// <summary>
    /// Whether to enable loopback (receive own messages). Default: true
    /// </summary>
    public bool EnableLoopback { get; set; } = true;

    /// <summary>
    /// Size of the receive buffer in bytes. Default: 65535
    /// </summary>
    [Range(1024, 1048576)]
    public int ReceiveBufferSize { get; set; } = 65535;

    /// <summary>
    /// Size of the send buffer in bytes. Default: 65535
    /// </summary>
    [Range(1024, 1048576)]
    public int SendBufferSize { get; set; } = 65535;

    /// <summary>
    /// Maximum message size in bytes. Default: 65000
    /// Messages larger than this will be fragmented.
    /// </summary>
    [Range(1024, 65000)]
    public int MaxMessageSize { get; set; } = 65000;

    /// <summary>
    /// Network interface to use. Null means all interfaces.
    /// </summary>
    public string? NetworkInterface { get; set; }

    /// <summary>
    /// Window size for message deduplication. Default: 1000
    /// </summary>
    [Range(100, 10000)]
    public int DeduplicationWindowSize { get; set; } = 1000;

    /// <summary>
    /// Timeout in milliseconds for deduplication entries. Default: 5000
    /// </summary>
    [Range(1000, 60000)]
    public int DeduplicationTimeoutMs { get; set; } = 5000;
}
