using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// Options for configuring request logging behavior.
/// </summary>
public sealed record VKRequestLoggingOptions : IVKBlockOptions
{
    /// <summary>
    /// The default configuration section name for Request Logging options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Web:RequestLogging";

    /// <summary>
    /// Gets or sets the list of paths that should be excluded from logging.
    /// Default includes health and metrics endpoints.
    /// </summary>
    public HashSet<string> ExcludedPaths { get; init; } = ["/health", "/metrics", "/favicon.ico"];

    /// <summary>
    /// Gets or sets the property name used in the logging scope for the Correlation ID.
    /// </summary>
    public string CorrelationIdPropertyName { get; init; } = "CorrelationId";

    /// <summary>
    /// Gets or sets a value indicating whether to log the request start.
    /// </summary>
    public bool LogRequestStart { get; init; } = true;
}
