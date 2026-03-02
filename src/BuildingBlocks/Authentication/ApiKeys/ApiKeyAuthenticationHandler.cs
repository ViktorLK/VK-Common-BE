using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Claims;

namespace VK.Blocks.Authentication.ApiKeys;

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
    #region Fields

    private readonly ApiKeyValidator _validator = validator;

    #endregion

    #region Protected Methods

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var rawKey))
        {
            return AuthenticateResult.NoResult();
        }

        var result = await _validator.ValidateAsync(rawKey!, Context.RequestAborted);

        // Check if the validation failed
        if (!result.IsSuccess)
        {
            return AuthenticateResult.Fail(result.FirstError.Description);
        }

        var context = result.Value!;
        var claims = new List<Claim>
        {
            new(VKClaimTypes.UserId,   context.OwnerId),
            new(VKClaimTypes.KeyId,    context.KeyId.ToString()),
            new(VKClaimTypes.AuthType, Options.AuthType),
        };

        if (context.TenantId is not null)
        {
            claims.Add(new Claim(VKClaimTypes.TenantId, context.TenantId));
        }

        foreach (var scope in context.Scopes)
        {
            claims.Add(new Claim(VKClaimTypes.Scope, scope));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    /// <inheritdoc />
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";
        return Response.WriteAsync("""{"error":"API key is missing or invalid"}""");
    }

    #endregion
}
