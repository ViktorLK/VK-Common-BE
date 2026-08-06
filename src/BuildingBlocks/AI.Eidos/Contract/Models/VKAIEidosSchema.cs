using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Value object representing a JSON Schema structure for response contracts.
/// </summary>
public sealed record VKAIEidosSchema
{
    public required string SchemaName { get; init; }
    public required string RawJsonSchema { get; init; }
    public IReadOnlyList<string> RequiredProperties { get; init; } = [];
}
