using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Interaction contract container between AI and Host system.
/// </summary>
public sealed record VKAIEidosResponseContract
{
    public required string ContractId { get; init; }
    public required VKAIEidosContractVersion Version { get; init; }
    public required string Scenario { get; init; }
    public required string Description { get; init; }
    public required VKAIEidosSchema Schema { get; init; }
}
