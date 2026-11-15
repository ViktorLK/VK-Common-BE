using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the next step in the atomic tool execution pipeline.
/// </summary>
public delegate Task<VKResult<VKAtomicToolResult>> VKAtomicToolExecutionDelegate(
    VKAtomicToolInvocationContext context,
    CancellationToken cancellationToken = default);

