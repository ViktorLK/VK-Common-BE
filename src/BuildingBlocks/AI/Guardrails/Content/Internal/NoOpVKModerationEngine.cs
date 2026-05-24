using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Content.Internal;

/// <summary>
/// A no-op implementation of <see cref="IVKModerationEngine"/> that approves all content.
/// </summary>
// [AP.03] Internal implementation is deep namespace and does not carry the VK prefix
internal sealed class NoOpVKModerationEngine : IVKModerationEngine
{
    private static readonly VKModerationResult _approvedResult = new VKModerationResult { IsFlagged = false };

    public Task<VKResult<VKModerationResult>> CheckContentAsync(string content, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(_approvedResult));
    }
}
