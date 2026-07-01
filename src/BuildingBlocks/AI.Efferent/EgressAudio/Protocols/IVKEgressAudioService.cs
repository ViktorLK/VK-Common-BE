using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

public interface IVKEgressAudioService
{
    Task<VKResult<Stream>> SynthesizeAsync(string text, CancellationToken cancellationToken = default);
}
