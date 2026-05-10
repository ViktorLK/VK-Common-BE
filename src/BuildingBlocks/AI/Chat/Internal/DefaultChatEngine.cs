using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Default implementation of <see cref="IVKChatEngine"/>.
/// </summary>
internal sealed class DefaultChatEngine : IVKChatEngine
{
    /// <inheritdoc />
    public Task<VKResult<VKChatMessage>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        // This is a placeholder that should be overridden by specific provider implementations
        return Task.FromResult(VKResult.Failure<VKChatMessage>(VKChatErrors.NotImplemented));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return VKResult.Failure<VKChatStreamingResponse>(VKChatErrors.NotImplemented);
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
