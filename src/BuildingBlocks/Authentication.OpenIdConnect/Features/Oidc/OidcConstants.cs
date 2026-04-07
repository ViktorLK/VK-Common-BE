using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc;

/// <summary>
/// Constants specific to the OpenID Connect providers.
/// </summary>
internal static class OidcConstants
{
    /// <summary>
    /// Identifier for the Azure B2C provider.
    /// </summary>
    internal const string AzureB2C = "AzureB2C";

    /// <summary>
    /// Identifier for the Google provider.
    /// </summary>
    internal const string Google = "Google";

    /// <summary>
    /// Identifier for the standard OIDC provider fallback.
    /// </summary>
    internal const string StandardProvider = "Standard";

    /// <summary>
    /// Identifier for the Microsoft Entra External ID (CIAM) provider.
    /// </summary>
    internal const string EntraExternal = "EntraExternal";

    /// <summary>
    /// The federated authentication type used for identity creation.
    /// </summary>
    internal const string FederatedAuthType = "VK.Federated";

    /// <summary>
    /// Fallback value for unknown provider IDs.
    /// </summary>
    internal const string UnknownProviderId = "unknown";

    /// <summary>
    /// TraceId identifier for startup-level logging.
    /// </summary>
    internal const string StartupTraceId = "startup";

    /// <summary>
    /// Default OIDC response type.
    /// </summary>
    internal const string DefaultResponseType = OpenIdConnectParameterNames.Code;

    #region Error and Activity Messages

    internal const string MapperNotFoundMessage = "OIDC Claims Mapper was not found.";
    internal const string DependencyMissingMessage = "AddDiscoveryOAuth requires AddVKAuthenticationBlock to be called first.";
    internal const string MissingConfigErrorMessage = "OIDC Provider configuration missing for scheme: {0}";

    #endregion

    #region Standard OIDC Claims

    internal const string ClaimSub = JwtRegisteredClaimNames.Sub;
    internal const string ClaimEmail = JwtRegisteredClaimNames.Email;
    internal const string ClaimName = JwtRegisteredClaimNames.Name;

    #endregion
 
    #region Azure AD B2C Specific Claims
 
    internal const string ClaimTfp = "tfp";
    internal const string ClaimAcr = JwtRegisteredClaimNames.Acr;
    internal const string ClaimEmails = "emails";
 
    #endregion
}
