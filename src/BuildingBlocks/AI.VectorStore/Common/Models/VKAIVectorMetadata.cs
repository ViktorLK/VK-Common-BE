using System.Collections.Generic;
using System.Linq;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Represents strongly-typed metadata for a vector record.
/// Ensures TenantId is always tracked for Rule 7 compliance.
/// </summary>
public sealed record VKAIVectorMetadata
{
    /// <summary>
    /// Gets the unique identifier of the tenant. Mandatory for isolation.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Gets the source origin of the document (e.g., File Path, URL).
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Gets a collection of tags for filtering.
    /// </summary>
    public HashSet<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets raw additional metadata properties.
    /// </summary>
    public Dictionary<string, string> Properties { get; init; } = [];

    /// <summary>
    /// Converts a raw dictionary to strongly-typed metadata.
    /// </summary>
    public static VKAIVectorMetadata FromDictionary(IDictionary<string, string> dictionary)
    {
        return new VKAIVectorMetadata
        {
            TenantId = dictionary.TryGetValue("TenantId", out var t) ? t : "Default",
            Source = dictionary.TryGetValue("Source", out var s) ? s : null,
            Tags = dictionary.TryGetValue("Tags", out var tg) ? tg.Split(',').ToHashSet() : []
        };
    }

    /// <summary>
    /// Converts metadata back to a flattened dictionary for storage.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>(Properties)
        {
            ["TenantId"] = TenantId
        };

        if (!string.IsNullOrEmpty(Source))
            dict["Source"] = Source;
        if (Tags.Count > 0)
            dict["Tags"] = string.Join(",", Tags);

        return dict;
    }
}
