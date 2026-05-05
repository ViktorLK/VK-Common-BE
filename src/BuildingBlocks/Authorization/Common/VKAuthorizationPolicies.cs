using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Well-known VK authorization policy names provided by the Authorization building block.
/// </summary>
public static class VKAuthorizationPolicies
{
    /// <summary>
    /// Policy requiring the user to be a Super Administrator.
    /// </summary>
    public const string SuperAdmin = VKSecurityPolicies.SuperAdmin;

    /// <summary>
    /// Policy requiring the resource tenant to match the user tenant.
    /// </summary>
    public const string SameTenant = VKSecurityPolicies.SameTenant;

    /// <summary>
    /// Policy indicating access is restricted to working hours only.
    /// </summary>
    public const string WorkingHoursOnly = nameof(WorkingHoursOnly);

    /// <summary>
    /// Policy indicating access is restricted to internal network origins only.
    /// </summary>
    public const string InternalNetworkOnly = nameof(InternalNetworkOnly);

    /// <summary>
    /// Policy indicating access requires a specific employee rank.
    /// </summary>
    public const string RankRestricted = nameof(RankRestricted);

    /// <summary>
    /// Policy indicating access requires specific tenant entitlements (features).
    /// </summary>
    public const string EntitlementsCheck = nameof(EntitlementsCheck);

    /// <summary>
    /// Backwards compatibility or specific example (to be deprecated if too specific).
    /// </summary>
    public const string SeniorAndAbove = nameof(SeniorAndAbove);

    /// <summary>
    /// Sample composite policy for financial data write operations.
    /// </summary>
    public const string FinancialDataWrite = nameof(FinancialDataWrite);
}
