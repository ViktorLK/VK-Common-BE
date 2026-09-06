using System;
using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Runtime C# DTO type reflection specification.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKTypeContractSpec : IVKContractSpec
{
    public required Type TargetType { get; init; }
    public string? ContractName { get; init; }
    public string? Version { get; init; }

    public VKTypeContractSpec() { }

    [SetsRequiredMembers]
    public VKTypeContractSpec(Type targetType, string? contractName = null, string? version = null)
    {
        TargetType = VKGuard.NotNull(targetType);
        ContractName = contractName;
        Version = version;
    }
}
