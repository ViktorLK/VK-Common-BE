using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// Options for configuring request body size limits.
/// Complies with AP.04 and BB.07.
/// </summary>
public sealed record VKRequestBodyLimitOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Web:RequestBodyLimit";

    /// <summary>
    /// Gets or sets a value indicating whether request body limit enforcement is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum request body size in bytes.
    /// Default is 30MB (31,457,280 bytes).
    /// </summary>
    [Range(1, long.MaxValue)]
    public long MaxRequestBodySize { get; init; } = 30 * 1024 * 1024;
}
