using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Contract.Internal;

internal sealed class DefaultContractRegistry : IVKContractRegistry
{
    private readonly ConcurrentDictionary<string, VKAIEidosResponseContract> _registry = new();

    public void RegisterContract(VKAIEidosResponseContract contract, string? tenantId = null, string? personaId = null)
    {
        VKGuard.NotNull(contract);
        var key = BuildKey(contract.Scenario, tenantId, personaId);
        _registry[key] = contract;
    }

    public Task<VKResult<VKAIEidosResponseContract>> ResolveCascadedContractAsync(
        string scenario,
        string? tenantId = null,
        string? personaId = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(scenario);

        var keys = new[]
        {
            BuildKey(scenario, tenantId, personaId),
            BuildKey(scenario, null, personaId),
            BuildKey(scenario, tenantId, null),
            BuildKey(scenario, null, null)
        };

        foreach (var key in keys)
        {
            if (_registry.TryGetValue(key, out var contract))
            {
                return Task.FromResult(VKResult.Success(contract));
            }
        }

        return Task.FromResult(VKResult.Failure<VKAIEidosResponseContract>(
            VKError.NotFound("Eidos.ContractNotFound", $"Contract for scenario '{scenario}' was not found.")));
    }

    private static string BuildKey(string scenario, string? tenantId, string? personaId)
        => $"{tenantId ?? "*"}:{personaId ?? "*"}:{scenario}";
}
