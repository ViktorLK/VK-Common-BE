using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Options for Eidos Contract Feature slice.
/// </summary>
public sealed partial record VKContractOptions : IVKBlockOptions
{
    public bool AutoMigrationEnabled { get; init; } = true;
}
