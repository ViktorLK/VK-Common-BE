using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Authentication.Abstractions;

namespace VK.Blocks.Authentication.Security;

/// <summary>
/// Manages the revocation of tokens, ensuring users are fully logged out.
/// </summary>
public class TokenRevocationService
{
    #region Fields

    private readonly ITokenBlacklist _blacklist;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRevocationService"/> class.
    /// </summary>
    /// <param name="blacklist">The token blacklist provider.</param>
    public TokenRevocationService(ITokenBlacklist blacklist)
    {
        _blacklist = blacklist;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Revokes an access token asynchronously by its JWT ID (JTI).
    /// </summary>
    /// <param name="userId">The unique identifier of the user (currently unused directly in revocation, but provided for context).</param>
    /// <param name="jti">The JWT ID to revoke.</param>
    /// <param name="timeToLive">The optional time-to-live for the revocation entry. Defaults to 1 day if not provided.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    public async Task RevokeUserTokensAsync(string userId, string jti, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
    {
        // Add AccessToken's JTI to Blacklist
        if (!string.IsNullOrEmpty(jti))
        {
            var ttl = timeToLive ?? TimeSpan.FromDays(1);
            await _blacklist.RevokeAsync(jti, ttl, cancellationToken);
        }
    }

    #endregion
}
