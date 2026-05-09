using System.Text.Json.Serialization;

namespace VK.Blocks.Web;

/// <summary>
/// Represents a specific detail of an error, used to communicate multiple errors in a single response (e.g., validation failures).
/// </summary>
public sealed record VKWebErrorDetail
{
    /// <summary>
    /// Gets the specific error code.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// Gets the detailed human-readable explanation.
    /// </summary>
    [JsonPropertyName("detail")]
    public required string Detail { get; init; }
}
