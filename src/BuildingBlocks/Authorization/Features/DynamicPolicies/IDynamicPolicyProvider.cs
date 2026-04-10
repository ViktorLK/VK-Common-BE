using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Defines a provider for fetching dynamic attributes or claims for policy evaluation.
/// </summary>
public interface IDynamicPolicyProvider
{
    /// <summary>
    /// Gets the value of a specific attribute for the given user.
    /// </summary>
    /// <param name="user">The user principal.</param>
    /// <param name="attributeName">The name of the attribute/claim to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the attribute value if found.</returns>
    ValueTask<Result<string?>> GetAttributeValueAsync(
        ClaimsPrincipal user, 
        string attributeName, 
        CancellationToken cancellationToken = default);
}
