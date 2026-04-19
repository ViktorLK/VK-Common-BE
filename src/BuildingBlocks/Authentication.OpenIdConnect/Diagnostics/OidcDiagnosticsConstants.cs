using VK.Blocks.Authentication.OpenIdConnect.Contracts;
using VK.Blocks.Core.Constants;

namespace VK.Blocks.Authentication.OpenIdConnect.Diagnostics;

/// <summary>
/// Centralized constants for OIDC diagnostics.
/// </summary>
internal static class OidcDiagnosticsConstants
{
    /// <summary>
    /// The diagnostic source name for the OIDC block.
    /// </summary>
    public static readonly string SourceName = OidcBlock.ActivitySourceName;

    /// <summary>The name of the authentication requests counter.</summary>
    public const string AuthRequestCounterName = "vk.auth.oidc.requests";

    /// <summary>The name of the authentication duration histogram.</summary>
    public const string AuthDurationHistogramName = "vk.auth.oidc.duration";

    /// <summary>The activity name for OIDC authentication.</summary>
    public const string ActivityAuthenticateOidc = "Authenticate.Oidc";

    /// <summary>Tag key for the authentication provider.</summary>
    public const string TagAuthProvider = "auth.provider";

    /// <summary>Tag key for the authentication result.</summary>
    public const string TagAuthResult = "auth.result";

    /// <summary>Tag key for the failure reason.</summary>
    public const string TagAuthFailureReason = "auth.failure_reason";

    /// <summary>Tag key for the tenant identifier.</summary>
    public const string TagTenantId = "auth.tenant_id";

    /// <summary>Result value for success.</summary>
    public const string ValueSuccess = "Success";

    /// <summary>Result value for failure.</summary>
    public const string ValueFailure = "Failure";

    /// <summary>Reason code when a mapper is not found for the provider.</summary>
    public const string ReasonMapperNotFound = "MapperNotFound";
}
