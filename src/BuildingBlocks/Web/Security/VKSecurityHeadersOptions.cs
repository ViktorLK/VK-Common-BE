using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// Options for configuring standard security headers.
/// </summary>
public sealed record VKSecurityHeadersOptions : IVKBlockOptions
{
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Web:SecurityHeaders";

    /// <summary>
    /// Gets or sets a value indicating whether to enable security headers.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the X-Frame-Options header value.
    /// Default is "DENY".
    /// </summary>
    [Required]
    public string XFrameOptions { get; init; } = "DENY";

    /// <summary>
    /// Gets or sets the X-Content-Type-Options header value.
    /// Default is "nosniff".
    /// </summary>
    [Required]
    public string XContentTypeOptions { get; init; } = "nosniff";

    /// <summary>
    /// Gets or sets the X-XSS-Protection header value.
    /// Default is "1; mode=block".
    /// </summary>
    [Required]
    public string XXssProtection { get; init; } = "1; mode=block";

    /// <summary>
    /// Gets or sets the Referrer-Policy header value.
    /// Default is "strict-origin-when-cross-origin".
    /// </summary>
    [Required]
    public string ReferrerPolicy { get; init; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Gets or sets the Content-Security-Policy header value.
    /// Default is a restrictive policy.
    /// </summary>
    [Required]
    public string ContentSecurityPolicy { get; init; } = "default-src 'self'; object-src 'none';";

    /// <summary>
    /// Gets or sets the Strict-Transport-Security header value.
    /// Typical value is "max-age=31536000; includeSubDomains".
    /// If null or empty, the header will not be added.
    /// </summary>
    public string? StrictTransportSecurity { get; init; } = "max-age=31536000; includeSubDomains";
}
