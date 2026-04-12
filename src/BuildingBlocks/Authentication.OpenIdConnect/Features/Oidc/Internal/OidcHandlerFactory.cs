using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Authentication.Features.OAuth;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;

/// <summary>
/// A factory for creating standardized OIDC event handlers.
/// </summary>
internal static class OidcHandlerFactory
{
    #region Public Methods

    public static Func<TokenValidatedContext, Task> CreateOnTokenValidated(string providerName)
    {
        return context =>
        {
            var services = context.HttpContext.RequestServices;
            var logger = services.GetRequiredService<ILogger<AuthenticationBlock>>();
            var traceId = context.HttpContext.TraceIdentifier;

            using var activity = OidcDiagnostics.StartOidcValidation(providerName);
            var stopwatch = Stopwatch.StartNew();
            var isSuccess = false;

            try
            {
                var mapper = services.GetKeyedService<IOAuthClaimsMapper>(providerName)
                             ?? services.GetKeyedService<IOAuthClaimsMapper>(OidcConstants.StandardProvider);

                if (mapper == null)
                {
                    logger.LogOidcMappingError(providerName, traceId);
                    OidcDiagnostics.RecordAuthAttempt(providerName, false, OidcDiagnosticsConstants.ReasonMapperNotFound);
                    activity?.SetStatus(ActivityStatusCode.Error, OidcConstants.MapperNotFoundMessage);
                    return Task.CompletedTask;
                }

                var externalIdentity = ExtractExternalIdentity(context, providerName);
                var internalClaims = mapper.MapToClaims(externalIdentity);

                var identity = new ClaimsIdentity(internalClaims, OidcConstants.FederatedAuthType);
                context.Principal?.AddIdentity(identity);

                logger.LogOidcAuthenticationSuccess(providerName, externalIdentity.ProviderId, traceId);
                OidcDiagnostics.RecordAuthAttempt(providerName, true);
                activity?.SetStatus(ActivityStatusCode.Ok);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                logger.LogOidcAuthenticationError(ex, providerName, traceId);
                OidcDiagnostics.RecordAuthAttempt(providerName, false, ex.Message);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                OidcDiagnostics.RecordDuration(providerName, stopwatch.Elapsed.TotalMilliseconds, isSuccess);
            }

            return Task.CompletedTask;
        };
    }

    #endregion

    #region Internal Methods

    internal static ExternalIdentity ExtractExternalIdentity(TokenValidatedContext context, string providerName)
    {
        var principal = context.Principal;
        if (principal == null)
        {
            return new ExternalIdentity
            {
                Provider = providerName,
                ProviderId = OidcConstants.UnknownProviderId,
                Claims = new Dictionary<string, string>()
            };
        }

        var claims = principal.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => g.First().Value);

        return new ExternalIdentity
        {
            Provider = providerName,
            ProviderId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal.FindFirst(OidcConstants.ClaimSub)?.Value
                         ?? OidcConstants.UnknownProviderId,
            Email = principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst(OidcConstants.ClaimEmail)?.Value,
            Name = principal.FindFirst(ClaimTypes.Name)?.Value
                   ?? principal.FindFirst(OidcConstants.ClaimName)?.Value,
            Claims = claims
        };
    }

    #endregion
}
