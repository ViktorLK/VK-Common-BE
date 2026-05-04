using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Default implementation of <see cref="IVKDynamicPoliciesProvider"/> that reads from user claims.
/// </summary>
internal sealed class DefaultDynamicPoliciesProvider : IVKDynamicPoliciesProvider
{
    /// <inheritdoc />
    public ValueTask<VKResult<string?>> GetAttributeValueAsync(
        ClaimsPrincipal user,
        string attributeName,
        CancellationToken cancellationToken = default)
    {
        var claimValue = user.FindFirst(attributeName)?.Value;

        return claimValue is null
            ? ValueTask.FromResult(VKResult.Failure<string?>(VKAuthorizationErrors.AttributeNotFound))
            : ValueTask.FromResult(VKResult.Success<string?>(claimValue));
    }
}
