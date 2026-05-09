using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VK.Blocks.Web;

/// <summary>
/// Represents a machine-readable format for specifying errors in HTTP API responses based on RFC 7807,
/// with VK.Blocks specific extensions.
/// <remarks>
/// This class is explicitly marked as sealed since it acts as a structured DTO for web API responses.
/// It is implemented as a class rather than a record to maintain compatibility with the base 
/// <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> class.
/// If an extension is required, consider using the <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails.Extensions"/> dictionary.
/// </remarks>
/// </summary>
public sealed class VKWebProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    /// <summary>
    /// Gets or sets a machine-readable error code.
    /// </summary>
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the unique trace identifier for the request.
    /// </summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the error occurred.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets diagnostic debug information.
    /// Warning: This should only be populated in non-production environments.
    /// </summary>
    [JsonPropertyName("debugInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VKWebDebugInfo? DebugInfo { get; set; }

    /// <summary>
    /// Gets or sets a collection of multiple errors (e.g., validation failures).
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<VKWebErrorDetail>? Errors { get; set; }
}
