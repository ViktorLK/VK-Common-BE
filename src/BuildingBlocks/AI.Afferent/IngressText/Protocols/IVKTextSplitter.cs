using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKTextSplitter
{
    Task<VKResult<IReadOnlyList<string>>> SplitTextAsync(string text, CancellationToken cancellationToken = default);
}
