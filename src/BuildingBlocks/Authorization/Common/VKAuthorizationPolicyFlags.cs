using System;

namespace VK.Blocks.Authorization.Common;

/// <summary>
/// Bitwise flags to selectively enable built-in VK authorization policies.
/// </summary>
[Flags]
public enum VKAuthorizationPolicyFlags
{
    /// <summary>
    /// No default policies are registered.
    /// </summary>
    None = 0,

    /// <summary>
    /// Register the working hours policy.
    /// </summary>
    WorkingHours = 1,

    /// <summary>
    /// Register the internal network origin policy.
    /// </summary>
    InternalNetwork = 2,

    /// <summary>
    /// Register the senior employee rank seniority policy.
    /// </summary>
    SeniorRank = 4,

    /// <summary>
    /// Register the financial write capability policy.
    /// </summary>
    FinancialWrite = 8,

    /// <summary>
    /// Register all default VK policies.
    /// </summary>
    All = WorkingHours | InternalNetwork | SeniorRank | FinancialWrite
}
