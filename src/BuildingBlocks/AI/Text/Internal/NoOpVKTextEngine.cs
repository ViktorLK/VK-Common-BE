using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Text.Internal;

/// <summary>
/// A no-op implementation of <see cref="IVKTextEngine"/> used when the feature is disabled.
/// </summary>
public sealed class NoOpVKTextEngine : IVKTextEngine
{
    /// <inheritdoc />
    public Task<VKResult<string>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<string>(VKTextErrors.FeatureDisabled));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VKResult<string>> GenerateStreamingAsync(
        string prompt,
        IVKAIArgs? args = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return VKResult.Failure<string>(VKTextErrors.FeatureDisabled);
        await Task.CompletedTask;
    }
}
