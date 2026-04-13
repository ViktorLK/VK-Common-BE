using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.DynamicPolicies.Internal;

/// <summary>
/// Default implementation of <see cref="IDynamicPolicyProvider"/> that reads from user claims.
/// </summary>
public sealed class DefaultDynamicPolicyProvider : IDynamicPolicyProvider
{
    /// <inheritdoc />
    public ValueTask<Result<string?>> GetAttributeValueAsync(
        ClaimsPrincipal user, 
        string attributeName, 
        CancellationToken cancellationToken = default)
    {
        var claimValue = user.FindFirst(attributeName)?.Value;
        
        return claimValue is null 
            ? ValueTask.FromResult(Result.Failure<string?>(AuthorizationErrors.AttributeNotFound))
            : ValueTask.FromResult(Result.Success<string?>(claimValue));
    }
}
