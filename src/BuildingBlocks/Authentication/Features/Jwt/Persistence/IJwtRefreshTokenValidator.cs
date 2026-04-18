using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.Jwt.Persistence;

/// <summary>
/// Validates a JWT refresh token to detect replay attacks and enforce token rotation policies.
/// </summary>
public interface IJwtRefreshTokenValidator
{
    /// <summary>
    /// Validates whether a refresh token JTI has been subject to a replay attack within its family.
    /// If a previously consumed JTI is encountered again, this method should return a failure,
    /// indicating a token compromise event.
    /// </summary>
    /// <param name="tokenJti">The JWT ID of the specific refresh token instance being validated.</param>
    /// <param name="familyId">The ID representing the token chain/family this refresh token belongs to.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A result containing true if validation passes, or an error if a replay/compromise is detected.</returns>
    ValueTask<Result<bool>> ValidateTokenRotationAsync(string tokenJti, string familyId, CancellationToken ct = default);
}
