using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// A No-Op implementation of the chat engine used when the feature is disabled.
/// </summary>
internal sealed class NoOpAISKChatEngine : IVKChatEngine
{
    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<VKChatResponse>(VKChatErrors.FeatureDisabled));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return VKResult.Failure<VKChatStreamingResponse>(VKChatErrors.FeatureDisabled);
        await Task.CompletedTask;
    }
}
