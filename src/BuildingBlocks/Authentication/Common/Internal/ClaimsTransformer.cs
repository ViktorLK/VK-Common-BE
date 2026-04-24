using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authentication.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Common.Internal;

/// <summary>
/// Intercepts the ClaimsPrincipal after authentication to enrich it with permissions, tenant IDs, etc.
/// It uses <see cref="IVKClaimsProvider"/> if registered in the DI container.
/// </summary>
internal sealed class ClaimsTransformer(
    IServiceScopeFactory scopeFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ClaimsTransformer> logger) : IClaimsTransformation
{
    /// <inheritdoc />
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return principal;
        }

        // 1. Check for the marker claim to ensure idempotency.
        if (principal.HasClaim(c => c.Type == VKClaimConstants.ClaimsTransformed))
        {
            return principal;
        }

        using Activity? activity = AuthenticationDiagnostics.StartClaimsTransformation();
        long startTime = Stopwatch.GetTimestamp();

        string? userId = principal.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            return principal;
        }

        using IServiceScope scope = scopeFactory.CreateScope();
        List<IVKClaimsProvider> claimsProviders = [.. (scope.ServiceProvider.GetService<IEnumerable<IVKClaimsProvider>>() ?? [])];

        if (claimsProviders.Count > 0)
        {
            try
            {
                // 2. Restore CancellationToken propagation via IHttpContextAccessor (Rule 3).
                CancellationToken cancellationToken = httpContextAccessor.HttpContext?.RequestAborted ?? default;

                List<Claim> allDynamicClaims = [];
                foreach (IVKClaimsProvider provider in claimsProviders)
                {
                    IEnumerable<Claim> claims = await provider.GetUserClaimsAsync(userId, cancellationToken).ConfigureAwait(false);
                    if (claims is not null)
                    {
                        allDynamicClaims.AddRange(claims);
                    }
                }

                if (allDynamicClaims.Count > 0)
                {
                    // 3. Clone the principal to keep the original immutable as per best practices.
                    ClaimsPrincipal clone = principal.Clone();
                    var newIdentity = (ClaimsIdentity)clone.Identity!;

                    newIdentity.AddClaims(allDynamicClaims);

                    // 4. Mark the identity as transformed.
                    newIdentity.AddClaim(new Claim(VKClaimConstants.ClaimsTransformed, "true"));

                    double durationMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
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

        double finalDurationMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
        AuthenticationDiagnostics.RecordClaimsTransformation(finalDurationMs, applied: false);

        return principal;
    }
}
