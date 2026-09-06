using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Named contract and optional semantic version lookup specification.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKNamedContractSpec : IVKContractSpec
{
    /// <summary>
    /// Gets the unique logical contract name registered in the schema registry.
    /// </summary>
    public required string ContractName { get; init; }

    /// <summary>
    /// Gets the optional semantic version of the contract.
    /// </summary>
    public string? Version { get; init; }

    public VKNamedContractSpec() { }

    [SetsRequiredMembers]
    public VKNamedContractSpec(string contractName, string? version = null)
    {
        ContractName = VKGuard.NotNullOrWhiteSpace(contractName);
        Version = version;
    }
}
