using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines a contract for retrieving prompt templates from a storage source.
/// </summary>
public interface IVKPromptProvider
{
    /// <summary>
    /// Retrieves a prompt template by its identifier and optional version.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <param name="version">The optional specific version to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The retrieved prompt template, or a failure if not found.</returns>
    Task<VKResult<VKPromptTemplate>> GetPromptAsync(string promptId, string? version = null, CancellationToken cancellationToken = default);
}
