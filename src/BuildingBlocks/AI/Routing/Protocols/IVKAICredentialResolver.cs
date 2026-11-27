using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Resolves a logical credential reference into a sensitive string secret.
/// </summary>
// [ARCH-02] [AP.03] [CS.01] [CS.03]
public interface IVKAICredentialResolver
{
    /// <summary>
    /// Resolves the specified credential reference for a given scope.
    /// </summary>
    ValueTask<VKResult<VKSensitiveString>> ResolveAsync(
        VKAIScopeKey scopeKey,
        string credentialRef,
        CancellationToken cancellationToken = default);
}
