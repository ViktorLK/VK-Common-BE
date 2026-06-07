using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// A registry that coordinates loading, rendering, and caching of prompt templates.
/// </summary>
public interface IVKPromptRegistry
{
    /// <summary>
    /// Loads a prompt template, renders it with the given variables, and returns the final string.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <param name="variables">Optional variables to inject into the template.</param>
    /// <param name="version">The optional specific version to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fully rendered prompt string.</returns>
    Task<VKResult<string>> RenderPromptAsync(
        string promptId,
        IDictionary<string, object?>? variables = null,
        string? version = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a raw prompt template without rendering it.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <param name="version">The optional specific version to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The raw prompt template.</returns>
    Task<VKResult<VKPromptTemplate>> GetTemplateAsync(
        string promptId,
        string? version = null,
        CancellationToken cancellationToken = default);
}
