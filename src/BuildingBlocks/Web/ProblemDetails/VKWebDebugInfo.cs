using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VK.Blocks.Web;

/// <summary>
/// Contains diagnostic information about an exception. 
/// Warning: This should NOT be exposed in production environments to prevent information disclosure.
/// </summary>
public sealed record VKWebDebugInfo
{
    /// <summary>
    /// Gets the debug message or the exception message.
    /// </summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string Message { get; init; }

    /// <summary>
    /// Gets the fully qualified type name of the underlying error, exception, or event.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string Type { get; init; }

    /// <summary>
    /// Gets the stack trace of the exception.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; init; }

    /// <summary>
    /// Gets the details of the inner exception, if one exists.
    /// </summary>
    [JsonPropertyName("innerError")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VKWebDebugInfo? InnerError { get; init; }

    /// <summary>
    /// Gets additional diagnostic or context information.
    /// </summary>
    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}
