using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Core.Constants;

namespace VK.Blocks.Authentication.Features.ApiKeys;

/// <summary>
/// Custom authentication handler to validate ApiKeys from HTTP Headers.
/// </summary>
public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ApiKeyValidator validator)
        : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var rawApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var result = await validator.ValidateAsync(rawApiKey!, Context.RequestAborted).ConfigureAwait(false);

        // Check if the validation failed
        if (result.IsFailure)
        {
            return AuthenticateResult.Fail(result.FirstError.Description);
        }

        var context = result.Value;

        // SUGGEST: Use C# 12 collection expression []
        List<Claim> claims = [
            new(VKClaimConstants.UserId,   context.OwnerId),
            new(VKClaimConstants.KeyId,    context.KeyId.ToString()),
            new(VKClaimConstants.AuthType, Options.AuthType),
        ];

        if (context.TenantId is not null)
        {
            claims.Add(new Claim(VKClaimConstants.TenantId, context.TenantId));
        }

        foreach (var scope in context.Scopes)
        {
            claims.Add(new Claim(VKClaimConstants.Scope, scope));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    /// <inheritdoc />
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        return AuthenticationResponseHelper.WriteUnauthorizedResponseAsync(Context, "API key is missing or invalid");
    }
}

