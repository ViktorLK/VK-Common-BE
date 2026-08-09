using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

public interface IVKEgressTextFormatter
{
    Task<VKResult<string>> FormatOutputAsync(string text, CancellationToken cancellationToken = default);
}
