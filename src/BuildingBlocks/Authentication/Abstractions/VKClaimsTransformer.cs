using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Authentication.Diagnostics;
using VK.Blocks.Core.Context;

namespace VK.Blocks.Authentication.Abstractions;

/// <summary>
/// Intercepts the ClaimsPrincipal after authentication to enrich it with permissions, tenant IDs, etc.
/// It uses <see cref="IVKClaimsProvider"/> if registered in the DI container.
/// </summary>
public sealed class VKClaimsTransformer(
    IServiceScopeFactory scopeFactory, 
    IHttpContextAccessor httpContextAccessor,
    ILogger<VKClaimsTransformer> logger) : IClaimsTransformation
{
    #region Public Methods

    /// <inheritdoc />
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return principal;
        }

        // 1. Check for the marker claim to ensure idempotency.
        if (principal.HasClaim(c => c.Type == VKClaimTypes.ClaimsTransformed))
        {
            return principal;
        }

        using var activity = AuthenticationDiagnostics.StartClaimsTransformation();
        var startTime = Stopwatch.GetTimestamp();

        var userId = principal.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            return principal;
        }

        using var scope = scopeFactory.CreateScope();
        var claimsProvider = scope.ServiceProvider.GetService<IVKClaimsProvider>();

        if (claimsProvider is not null)
        {
            try
            {
                // 2. Restore CancellationToken propagation via IHttpContextAccessor (Rule 3).
                var cancellationToken = httpContextAccessor.HttpContext?.RequestAborted ?? default;
                var dynamicClaims = await claimsProvider.GetUserClaimsAsync(userId, cancellationToken).ConfigureAwait(false);

                if (dynamicClaims is not null)
                {
                    // 3. Clone the principal to keep the original immutable as per best practices.
                    var clone = principal.Clone();
                    var newIdentity = (ClaimsIdentity)clone.Identity!;

                    newIdentity.AddClaims(dynamicClaims);

                    // 4. Mark the identity as transformed.
                    newIdentity.AddClaim(new Claim(VKClaimTypes.ClaimsTransformed, "true"));

                    var durationMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
                    AuthenticationDiagnostics.RecordClaimsTransformation(durationMs, applied: true);
                    activity?.SetTag(AuthenticationDiagnosticsConstants.TagClaimsTransformed, true);

                    return clone;
                }
            }
            catch (Exception ex)
            {
                // Resilience: Do not crash the entire request if claims enrichment fails.
                // Log and return the original principal (Rule 6).
                logger.LogClaimsTransformationError(ex, userId);
                
                AuthenticationDiagnostics.RecordClaimsTransformation(
                    Stopwatch.GetElapsedTime(startTime).TotalMilliseconds, applied: false);
            }
        }

        var finalDurationMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
        AuthenticationDiagnostics.RecordClaimsTransformation(finalDurationMs, applied: false);

        return principal;
    }

    #endregion
}
