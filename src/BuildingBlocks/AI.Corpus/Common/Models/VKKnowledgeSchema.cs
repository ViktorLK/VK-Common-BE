using System;
using System.Collections.Generic;
using System.Text;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Represents a structured knowledge entity schema definition for alignment and LLM extraction.
/// </summary>
public sealed record VKKnowledgeSchema
{
    /// <summary>
    /// Gets the category or entity type name of the knowledge (e.g. "CharacterSetting", "WorldRule", "Location").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Gets the high-level description of what this knowledge category represents.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the list of attribute field schemas for this knowledge category.
    /// </summary>
    public IReadOnlyList<VKKnowledgeAttributeSchema> Attributes { get; init; } = [];

    /// <summary>
    /// Renders a structured description format suitable for inclusion in LLM extraction prompts.
    /// </summary>
    /// <returns>A formatted markdown/text representation of the schema.</returns>
    public string ToSystemPromptDescription()
    {
        Span<char> initialBuffer = stackalloc char[256];
        using var builder = new VKValueStringBuilder(initialBuffer);
        builder.AppendLine($"### Category: {Category}");
        if (!string.IsNullOrWhiteSpace(Description))
        {
            builder.AppendLine($"Description: {Description}");
        }

        if (Attributes.Count > 0)
        {
            builder.AppendLine("Attributes:");
            foreach (var attr in Attributes)
            {
                string req = attr.IsRequired ? " (Required)" : "";
                string desc = !string.IsNullOrWhiteSpace(attr.Description) ? $" - {attr.Description}" : "";
                builder.AppendLine($"  - {attr.Name} [{attr.DataType}]{req}{desc}");
            }
        }

        return builder.ToString();
    }
}
