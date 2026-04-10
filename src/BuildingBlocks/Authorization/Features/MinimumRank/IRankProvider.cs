using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authorization.Features.MinimumRank;

/// <summary>
/// Provides access to the user's rank information.
/// </summary>
public interface IRankProvider
{
    /// <summary>
    /// Gets the user's rank as a string (can be numeric or enum name).
    /// </summary>
    /// <param name="user">The user to evaluate.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The raw rank string, or null if not found.</returns>
    ValueTask<string?> GetRankAsync(ClaimsPrincipal user, CancellationToken ct = default);
}
