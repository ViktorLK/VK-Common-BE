using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKIngressAudioService
{
    Task<VKResult<string>> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default);
}
