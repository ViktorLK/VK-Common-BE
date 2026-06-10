using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Delegate representing the next middleware in the Psyche execution chain.
/// Returns VKResult of VKPromptTapestry to comply with CS.01.
/// </summary>
public delegate Task<VKResult<VKPsycheResponse>> VKPsycheMiddlewareDelegate(VKPsycheContext context, CancellationToken cancellationToken);
