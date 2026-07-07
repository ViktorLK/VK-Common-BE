using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKSystemEventDispatcher
{
    Task<VKResult> PublishAsync(VKSystemEvent systemEvent, CancellationToken cancellationToken = default);
    Task<VKResult<IReadOnlyList<VKSystemEvent>>> ConsumeEventsAsync(CancellationToken cancellationToken = default);
}
