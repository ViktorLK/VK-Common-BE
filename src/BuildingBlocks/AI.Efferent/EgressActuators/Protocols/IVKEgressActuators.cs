using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

public interface IVKEgressActuators
{
    Task<VKResult<IReadOnlyList<VKToolResult>>> DispatchActionsAsync(IReadOnlyList<VKToolCall> toolCalls, CancellationToken cancellationToken = default);
}
