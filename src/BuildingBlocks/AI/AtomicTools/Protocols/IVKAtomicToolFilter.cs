using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines an onion-style middleware filter that intercepts atomic tool execution (ARCH-11).
/// </summary>
public interface IVKAtomicToolFilter
{
    /// <summary>
    /// Gets the execution order of the filter. Lower values execute first.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Intercepts an atomic tool invocation.
    /// </summary>
    Task<VKResult<VKAtomicToolResult>> InvokeAsync(
        VKAtomicToolInvocationContext context,
        VKAtomicToolExecutionDelegate next,
        CancellationToken cancellationToken = default);
}
