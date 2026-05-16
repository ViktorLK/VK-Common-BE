using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKChatEngine"/>.
/// Always returns a NotImplemented error.
/// </summary>
internal sealed class NoOpVKChatEngine : IVKChatEngine
{
    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        _ = messages;
        _ = args;
        _ = cancellationToken;

        return Task.FromResult(VKResult.Failure<VKChatResponse>(VKChatErrors.NotImplemented));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = messages;
        _ = args;
        _ = cancellationToken;

        yield return VKResult.Failure<VKChatStreamingResponse>(VKChatErrors.NotImplemented);
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
