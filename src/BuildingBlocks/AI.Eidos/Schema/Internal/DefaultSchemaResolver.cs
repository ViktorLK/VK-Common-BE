using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Schema.Internal;

internal sealed class DefaultSchemaResolver(IVKSchemaFactory schemaFactory) : IVKSchemaResolver
{
    private readonly ConcurrentDictionary<string, VKAIEidosResponseContract> _registry = new();
    private readonly IVKSchemaFactory _schemaFactory = VKGuard.NotNull(schemaFactory);

    public void RegisterContract(VKAIEidosResponseContract contract)
    {
        VKGuard.NotNull(contract);
        var key = BuildKey(contract.ContractName, contract.Version);
        _registry[key] = contract;

        // Also register as default version for contract if no specific version requested
        var defaultKey = BuildKey(contract.ContractName, null);
        _registry[defaultKey] = contract;
    }

    public Task<VKResult<VKAIEidosResponseContract>> ResolveForContextAsync(
        string contractName,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(contractName);

        var keys = new[]
        {
            BuildKey(contractName, version),
            BuildKey(contractName, null)
        };

        foreach (var key in keys)
        {
            if (_registry.TryGetValue(key, out var contract))
            {
                return Task.FromResult(VKResult.Success(contract));
            }
        }

        return Task.FromResult(VKResult.Failure<VKAIEidosResponseContract>(
            VKError.NotFound("Eidos.ContractNotFound", $"Contract '{contractName}' was not found.")));
    }

    public async Task<VKAIEidosResponseContract?> ResolveFromArgsAsync(
        VKAIEidosRequestArgs? args,
        CancellationToken cancellationToken = default)
    {
        if (args?.ContractSpec is null)
        {
            return null;
        }

        switch (args.ContractSpec)
        {
            case VKExplicitContractSpec explicitSpec:
                return explicitSpec.Contract;

            case VKNamedContractSpec namedSpec:
                var contractRes = await ResolveForContextAsync(namedSpec.ContractName, namedSpec.Version, cancellationToken).ConfigureAwait(false);
                return contractRes.IsSuccess ? contractRes.Value : null;

            case VKTypeContractSpec typeSpec:
                return _schemaFactory.CreateContract(typeSpec.TargetType, typeSpec.ContractName ?? typeSpec.TargetType.Name, version: typeSpec.Version);

            default:
                return null;
        }
    }

    private static string BuildKey(string contractName, string? version)
        => $"{contractName}:{version ?? "*"}";
}
