using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authorization.Abstractions;

/// <summary>
/// Defines a custom authorization handler interface for VK policies.
/// </summary>
public interface IVKAuthorizationHandler
{
    /// <summary>
    /// Evaluates authorization for a requirement asynchronously.
    /// </summary>
    Task<bool> AuthorizeAsync(ClaimsPrincipal user, object? resource, string requirement, CancellationToken ct = default);
}

