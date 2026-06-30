using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// The top-level weaving coordinator executing the sequential pipeline stages.
/// </summary>
public interface IVKWeavingTaskEngine
{
    Task<VKResult> WeavePromptAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken);
}
