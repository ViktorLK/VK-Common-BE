using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Unified response container containing bound DTO, raw content, and governance metadata.
/// Complies with AP.01 (sealed record).
/// </summary>
/// <typeparam name="T">The bound DTO model type.</typeparam>
public sealed record VKAIEidosEnvelope<T> where T : class
{
    public T? Model { get; init; }
    public string? RawContent { get; init; }
    public VKAIEidosExpressionMode ExpressionMode { get; init; } = VKAIEidosExpressionMode.ToolCall;
    public VKAIEidosContractVersion ContractVersion { get; init; } = VKAIEidosContractVersion.V1;
    public IReadOnlyList<string> Issues { get; init; } = [];
}
