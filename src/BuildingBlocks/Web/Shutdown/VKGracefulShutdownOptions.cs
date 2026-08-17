using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// Options for configuring graceful shutdown and request draining.
/// Complies with AP.04 and BB.07.
/// </summary>
public sealed record VKGracefulShutdownOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Web:GracefulShutdown";

    /// <summary>
    /// Gets or sets a value indicating whether graceful shutdown and request draining is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the host shutdown timeout in seconds.
    /// Default is 30 seconds.
    /// </summary>
    [Range(1, 3600)]
    public int ShutdownTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets or sets the initial drain period in seconds before the host initiates shutdown.
    /// During this time, the application continues to run but returns 503 for new requests.
    /// Default is 5 seconds.
    /// </summary>
    [Range(0, 300)]
    public int DrainSeconds { get; init; } = 5;
}
