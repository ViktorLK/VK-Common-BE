using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Resolves the appropriate response contract for a given scenario and request args.
/// </summary>
public interface IVKSchemaResolver
{
    /// <summary>
    /// Registers a response contract for named and versioned resolution.
    /// </summary>
    void RegisterContract(VKAIEidosResponseContract contract);

    Task<VKResult<VKAIEidosResponseContract>> ResolveForContextAsync(
        string contractName,
        string? version = null,
        CancellationToken cancellationToken = default);

    Task<VKAIEidosResponseContract?> ResolveFromArgsAsync(
        VKAIEidosRequestArgs? args,
        CancellationToken cancellationToken = default);
}
