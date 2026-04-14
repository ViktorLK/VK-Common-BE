using System.Collections.Generic;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.Models;

/// <summary>
/// A transport-neutral representation of an error response, providing a standard error envelope 
/// suitable for cross-layer and cross-service communication (REST, gRPC, SignalR, etc.).
/// Independent of any specific transport or web framework.
/// </summary>
public sealed record VKErrorResponse
{
    /// <summary>
    /// Gets or sets the high-level category of the error.
    /// </summary>
    public ErrorType Type { get; init; } = ErrorType.Failure;

    /// <summary>
    /// Gets or sets the unique error code defined by the domain (e.g., "User.NotFound").
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Gets or sets a human-readable explanation of the error.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the unique trace identifier for the request/occurrence.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets or sets extension members or additional metadata for the error.
    /// </summary>
    public IDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets or sets diagnostic debug information. 
    /// </summary>
    public VKErrorDebugInfo? DebugInfo { get; init; }

    /// <summary>
    /// Gets or sets a collection of multiple sub-errors (e.g., validation failures).
    /// </summary>
    public IReadOnlyList<VKErrorDetail>? Errors { get; init; }
}
