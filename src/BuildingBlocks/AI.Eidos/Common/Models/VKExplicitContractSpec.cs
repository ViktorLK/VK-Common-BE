using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Explicit, pre-built response contract specification.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKExplicitContractSpec : IVKContractSpec
{
    public required VKAIEidosResponseContract Contract { get; init; }

    public VKExplicitContractSpec() { }

    [SetsRequiredMembers]
    public VKExplicitContractSpec(VKAIEidosResponseContract contract)
    {
        Contract = VKGuard.NotNull(contract);
    }
}
