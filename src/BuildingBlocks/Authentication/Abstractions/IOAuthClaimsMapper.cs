using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions.Contracts;

namespace VK.Blocks.Authentication.Abstractions;

/// <summary>
/// Defines a mapper for converting OAuth user information into claims.
/// </summary>
public interface IOAuthClaimsMapper
{
    /// <summary>
    /// Maps the provided OAuth user information to a collection of claims.
    /// </summary>
    /// <param name="userInfo">The OAuth user information to map.</param>
    /// <returns>An enumerable collection of mapped claims.</returns>
    IEnumerable<Claim> MapToClaims(OAuthUserInfo userInfo);
}
