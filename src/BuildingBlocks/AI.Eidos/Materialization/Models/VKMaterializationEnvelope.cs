using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Unified response container containing materialized DTO, raw content, and governance metadata.
/// Complies with [AP.01] (sealed record).
/// </summary>
/// <typeparam name="T">The bound DTO model type.</typeparam>
public sealed record VKMaterializationEnvelope<T> where T : class // [AP.01]
{
    public T? Model { get; init; }
    public string? RawContent { get; init; }
    public VKAIEidosExpressionMode ExpressionMode { get; init; } = VKAIEidosExpressionMode.StructuredOutput;
    public string ContractVersion { get; init; } = "1.0";
    public IReadOnlyList<string> Issues { get; init; } = [];
    public VKMaterializationConfidenceMetadata? Confidence { get; init; }
}
