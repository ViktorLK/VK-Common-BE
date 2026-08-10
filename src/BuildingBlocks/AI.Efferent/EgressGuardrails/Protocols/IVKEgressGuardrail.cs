using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

public interface IVKEgressGuardrail
{
    Task<VKResult<string>> ValidateOutputSafetyAsync(string text, CancellationToken cancellationToken = default);
}
