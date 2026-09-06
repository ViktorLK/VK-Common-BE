using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Reusable schema fragment that can be composed into parent contract schemas (e.g. Pagination, Audit, Metadata).
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKAIEidosSchemaFragment
{
    /// <summary>
    /// Fragment identifier name.
    /// </summary>
    public required string FragmentName { get; init; }

    /// <summary>
    /// Partial JSON schema defining the properties/types introduced by this fragment.
    /// </summary>
    public required string RawJsonSchema { get; init; }

    /// <summary>
    /// Required properties mandated by this fragment.
    /// </summary>
    public IReadOnlyList<string> RequiredProperties { get; init; } = [];

    /// <summary>
    /// Creates a schema fragment directly from a C# DTO type using an injected schema factory.
    /// </summary>
    public static VKAIEidosSchemaFragment FromType<T>(IVKSchemaFactory schemaFactory, string? fragmentName = null)
    {
        VKGuard.NotNull(schemaFactory);
        var schema = schemaFactory.FromType<T>(fragmentName);
        return new VKAIEidosSchemaFragment
        {
            FragmentName = schema.SchemaName,
            RawJsonSchema = schema.RawJsonSchema,
            RequiredProperties = schema.RequiredProperties
        };
    }
}
