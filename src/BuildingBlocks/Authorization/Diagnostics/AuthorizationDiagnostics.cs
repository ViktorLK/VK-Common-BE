using System.Collections.Generic;
using System.Diagnostics.Metrics;
using VK.Blocks.Core.Attributes;

namespace VK.Blocks.Authorization.Diagnostics;

/// <summary>
/// Centralized Diagnostics definition for the VK.Blocks.Authorization building block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[VKBlockDiagnostics("VK.Blocks.Authorization")]
public static partial class AuthorizationDiagnostics
{
    #region Fields

    /// <summary>
    /// Counter tracking the number of authorization decisions.
    /// Includes tags for "authz.policy" (e.g., TenantIsolation, MinimumRank) and "authz.result" (Allowed, Denied).
    /// </summary>
    public static readonly Counter<long> AuthorizationDecisions;

    #endregion

    static AuthorizationDiagnostics()
    {
        AuthorizationDecisions = Meter.CreateCounter<long>(
            "authorization.decisions",
            description: "Number of authorization decisions processed"
        );
    }

    #region Public Methods

    /// <summary>
    /// Records an authorization decision result.
    /// </summary>
    /// <param name="policyName">The name of the policy or requirement evaluated (e.g., "TenantIsolation").</param>
    /// <param name="isAllowed">Whether the user was granted access.</param>
    public static void RecordDecision(string policyName, bool isAllowed)
    {
        AuthorizationDecisions.Add(1,
            new KeyValuePair<string, object?>("authz.policy", policyName),
            new KeyValuePair<string, object?>("authz.result", isAllowed ? "Allowed" : "Denied"));
    }

    #endregion
}
