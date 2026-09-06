using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Static helper for constructing fluent <see cref="IVKContractSpec"/> instances.
/// </summary>
public static class VKContractSpecFactory
{
    public static VKExplicitContractSpec Explicit(VKAIEidosResponseContract contract)
    {
        VKGuard.NotNull(contract);
        return new VKExplicitContractSpec(contract);
    }

    public static VKNamedContractSpec Named(string contractName, string? version = null)
    {
        VKGuard.NotNullOrWhiteSpace(contractName);
        return new VKNamedContractSpec(contractName, version);
    }

    public static VKTypeContractSpec ForType<T>(string? contractName = null, string? version = null)
        => ForType(typeof(T), contractName, version);

    public static VKTypeContractSpec ForType(Type targetType, string? contractName = null, string? version = null)
    {
        VKGuard.NotNull(targetType);
        return new VKTypeContractSpec(targetType, contractName, version);
    }
}
