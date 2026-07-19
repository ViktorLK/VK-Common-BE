using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Defines a provider for validating multi-factor authentication (MFA) challenges.
/// </summary>
public interface IVKMfaProvider
{
    /// <summary>
    /// Validates an MFA code challenge for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="code">The verification code submitted by the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="VKResult"/> representing the outcome of the validation.</returns>
    ValueTask<VKResult> ValidateMfaChallengeAsync(string userId, string code, CancellationToken cancellationToken = default);
}
