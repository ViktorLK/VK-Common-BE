using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.Diagnostics.Internal;

/// <summary>
/// Centralized Diagnostics definition for the VK.Blocks.Authentication.OpenIdConnect building block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// Complies with OR.01 & BB.04 (Diagnostics Blueprint).
/// </summary>
[VKBlockDiagnostics<VKOidcBlock>]
internal static partial class OidcDiagnostics
{
    private static readonly Counter<long> AuthenticationRequests;
    private static readonly Histogram<double> ValidationDuration;

    static OidcDiagnostics()
    {
        AuthenticationRequests = Meter.CreateCounter<long>(
            VKOidcDiagnosticsConstants.AuthRequestCounterName,
            description: "Number of OIDC authentication attempts"
        );

        ValidationDuration = Meter.CreateHistogram<double>(
            VKOidcDiagnosticsConstants.AuthDurationHistogramName,
            unit: "ms",
            description: "Duration of OIDC authentication/validation"
        );
    }

    /// <summary>
    /// Records an OIDC authentication attempt result.
    /// </summary>
    /// <param name="provider">The authentication provider name.</param>
    /// <param name="isSuccess">Whether the attempt was successful.</param>
    /// <param name="failureReason">The reason for failure if not successful.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    public static void RecordAuthAttempt(string provider, bool isSuccess, string? failureReason = null, string? tenantId = null)
    {
        var tags = new TagList
        {
            { VKOidcDiagnosticsConstants.TagAuthProvider, provider },
            { VKOidcDiagnosticsConstants.TagAuthResult, isSuccess ? VKOidcDiagnosticsConstants.ValueSuccess : VKOidcDiagnosticsConstants.ValueFailure }
        };

        if (!string.IsNullOrEmpty(tenantId))
        {
            tags.Add(VKOidcDiagnosticsConstants.TagTenantId, tenantId);
        }

        if (!isSuccess && !string.IsNullOrEmpty(failureReason))
        {
            tags.Add(VKOidcDiagnosticsConstants.TagAuthFailureReason, failureReason);
        }

        AuthenticationRequests.Add(1, tags);
    }

    /// <summary>
    /// Records the duration of an OIDC validation attempt.
    /// </summary>
    /// <param name="provider">The authentication provider name.</param>
    /// <param name="durationMs">The duration in milliseconds.</param>
    /// <param name="isSuccess">Whether the validation was successful.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    public static void RecordDuration(string provider, double durationMs, bool isSuccess, string? tenantId = null)
    {
        var tags = new TagList
        {
            { VKOidcDiagnosticsConstants.TagAuthProvider, provider },
            { VKOidcDiagnosticsConstants.TagAuthResult, isSuccess ? VKOidcDiagnosticsConstants.ValueSuccess : VKOidcDiagnosticsConstants.ValueFailure }
        };

        if (!string.IsNullOrEmpty(tenantId))
        {
            tags.Add(VKOidcDiagnosticsConstants.TagTenantId, tenantId);
        }

        ValidationDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Starts a new activity for OIDC token validation.
    /// </summary>
    /// <param name="provider">The authentication provider name.</param>
    /// <returns>The started <see cref="Activity"/> or null if diagnostics are disabled.</returns>
    public static Activity? StartOidcValidation(string provider)
    {
        var activity = Source.StartActivity(VKOidcDiagnosticsConstants.ActivityAuthenticateOidc);
        activity?.SetTag(VKOidcDiagnosticsConstants.TagAuthProvider, provider);
        return activity;
    }
}


