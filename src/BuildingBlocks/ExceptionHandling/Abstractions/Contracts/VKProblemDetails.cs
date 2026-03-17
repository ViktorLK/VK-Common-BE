using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace VK.Blocks.ExceptionHandling.Abstractions.Contracts;

/// <summary>
/// Represents a machine-readable format for specifying errors in HTTP API responses based on RFC 7807,
/// with VK.Blocks specific extensions.
/// </summary>
public sealed class VKProblemDetails : ProblemDetails
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
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
