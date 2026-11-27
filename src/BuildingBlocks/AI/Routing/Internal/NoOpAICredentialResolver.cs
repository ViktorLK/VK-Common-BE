using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Routing.Internal;

/// <summary>
/// Default no-op implementation of <see cref="IVKAICredentialResolver"/> that fails closed when no provider is configured.
/// </summary>
// [ARCH-05] [AP.01] [CS.01]
internal sealed class NoOpAICredentialResolver : IVKAICredentialResolver
{
    /// <inheritdoc />
    public ValueTask<VKResult<VKSensitiveString>> ResolveAsync(
        VKAIScopeKey scopeKey,
        string credentialRef,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = scopeKey;
        _ = credentialRef;

        return ValueTask.FromResult(VKResult.Failure<VKSensitiveString>(VKAIErrors.AuthenticationFailed));
    }
}
