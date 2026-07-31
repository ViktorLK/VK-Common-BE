namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines an attribute property field within a knowledge entity schema.
/// </summary>
public sealed record VKKnowledgeAttributeSchema
{
    /// <summary>
    /// Gets the name of the attribute (e.g. "role", "location", "affinity").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the data type of the attribute (e.g. "string", "number", "boolean", "array").
    /// </summary>
    public string DataType { get; init; } = "string";

    /// <summary>
    /// Gets a value indicating whether this attribute is required during extraction.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets the human-readable description or hint for this attribute.
    /// </summary>
    public string? Description { get; init; }
}
