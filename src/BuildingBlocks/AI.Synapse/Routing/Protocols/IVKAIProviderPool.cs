using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Registry provider for looking up configured and available AI provider connections.
/// </summary>
public interface IVKAIProviderPool
{
    /// <summary>
    /// Gets all registered AI provider connections for the current ambient tenant and global pools.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKAIConnection>>> GetAvailablePoolAsync(
        CancellationToken cancellationToken = default);
}
