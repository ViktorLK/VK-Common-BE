namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;

/// <summary>
/// Centralized constants for OIDC diagnostics.
/// </summary>
internal static class OidcDiagnosticsConstants
{
    public const string SourceName = "VK.Blocks.Authentication.OpenIdConnect";
    public const string AuthRequestCounterName = "vk.auth.oidc.requests";
    public const string AuthDurationHistogramName = "vk.auth.oidc.duration";
    public const string ActivityAuthenticateOidc = "Authenticate.Oidc";

    public const string TagAuthProvider = "auth.provider";
    public const string TagAuthResult = "auth.result";
    public const string TagAuthFailureReason = "auth.failure_reason";
    public const string TagTenantId = "auth.tenant_id";

    public const string ValueSuccess = "Success";
    public const string ValueFailure = "Failure";
    public const string ReasonMapperNotFound = "MapperNotFound";
}
