using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKIngressGuardrail
{
    Task<VKResult<string>> ValidateSafetyAsync(string text, CancellationToken cancellationToken = default);
}
