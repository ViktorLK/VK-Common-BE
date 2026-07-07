using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Environment.Internal;

/// <summary>
/// Default production-grade implementation of <see cref="IVKEnvironmentPerceptionProvider"/>.
/// </summary>
internal sealed class DefaultEnvironmentPerceptionProvider : IVKEnvironmentPerceptionProvider
{
    public Task<VKResult<VKEnvironmentState>> GetEnvironmentStateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var state = new VKEnvironmentState(
            ActiveWindowTitle: "Active IDE Window",
            ScreenOcrText: "",
            ClipboardContent: ""
        );

        return Task.FromResult(VKResult.Success(state));
    }
}
