using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Interaction contract container between AI and Host system.
/// </summary>
public sealed record VKAIEidosResponseContract
{
    public required string ContractId { get; init; }
    public string Version { get; init; } = "1.0";
    public required string ContractName { get; init; }
    public required string Description { get; init; }
    public required VKAIEidosSchema Schema { get; init; }
}
