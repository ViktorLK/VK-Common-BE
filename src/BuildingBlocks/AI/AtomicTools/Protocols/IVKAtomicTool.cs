using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the interface for an atomic tool/function that can be called by AI.
/// Renamed from IVKAgentTool to reflect its broader use beyond just agents.
/// </summary>
public interface IVKAtomicTool
{
    /// <summary>
    /// Gets the tool manifest (Definition/Schema).
    /// </summary>
    VKAtomicToolManifest Manifest { get; }

    /// <summary>
    /// Executes the tool with the given arguments.
    /// </summary>
    /// <param name="arguments">The tool arguments.</param>
    /// <param name="context">The execution context provided by the caller.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tool execution result.</returns>
    Task<VKResult<VKAtomicToolResult>> ExecuteAsync(
        IDictionary<string, object> arguments,
        VKAgentExecutionContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the tool and streams the results back.
    /// By default, this wraps the non-streaming <see cref="ExecuteAsync"/> method.
    /// Tools that inherently support streaming (e.g. long-running scripts) should override this.
    /// </summary>
    /// <param name="arguments">The tool arguments.</param>
    /// <param name="context">The execution context provided by the caller.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async enumerable streaming the tool execution result.</returns>
    public async IAsyncEnumerable<VKResult<string>> ExecuteStreamingAsync(
        IDictionary<string, object> arguments,
        VKAgentExecutionContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(arguments, context, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            yield return VKResult.Success(result.Value?.Content ?? string.Empty);
        }
        else
        {
            yield return VKResult.Failure<string>(result.Errors);
        }
    }
}
