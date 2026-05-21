using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Predefined structured error constants for the Presence block.
/// Follows CS.01, AP.03, and AP.01.
/// </summary>
internal static class PresenceErrors
{
    /// <summary>
    /// Error indicating the tenant context is suspended or could not be found.
    /// </summary>
    public static readonly VKError TenantSuspendedOrNotFound = new(
        "Presence.Tenant.SuspendedOrNotFound",
        "The requested tenant context is invalid, suspended, or could not be verified in the cognitive sandbox.",
        VKErrorType.Failure);

    /// <summary>
    /// Error indicating high-frequency rate limiting thresholds have been hit.
    /// </summary>
    public static readonly VKError TooManyRequests = new(
        "Presence.RateLimit.TooManyRequests",
        "High-frequency request QPS threshold has been exceeded. Please back off and retry later.",
        VKErrorType.Failure);

    /// <summary>
    /// Error indicating that the user or tenant financial quota/balance has been exhausted.
    /// </summary>
    public static readonly VKError QuotaExhausted = new(
        "Presence.Quota.Exhausted",
        "The dynamic financial budget or prepaid Token balance has been fully exhausted for this tenant/user.",
        VKErrorType.Failure);

    /// <summary>
    /// Error indicating that the governance snapshot was missing in the context.
    /// </summary>
    public static readonly VKError GovernanceSnapshotMissing = new(
        "Presence.Governance.SnapshotMissing",
        "The PresenceGovernanceInterceptor must execute before PresenceContextInterceptor.",
        VKErrorType.Failure);
}
