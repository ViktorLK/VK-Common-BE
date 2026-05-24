using System.Threading.Tasks;

namespace VK.Blocks.AI;

/// <summary>
/// Defines a filter that can intercept atomic tool execution.
/// </summary>
public interface IVKAtomicToolFilter
{
    /// <summary>
    /// Called before a tool is executed.
    /// </summary>
    Task OnToolExecutingAsync(VKAtomicToolExecutingContext context);

    /// <summary>
    /// Called after a tool has been executed.
    /// </summary>
    Task OnToolExecutedAsync(VKAtomicToolExecutedContext context);
}
