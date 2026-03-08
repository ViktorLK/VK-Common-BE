using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authorization.Features.DynamicPolicies;
using VK.Blocks.Authorization.Features.InternalNetwork;
using VK.Blocks.Authorization.Features.MinimumRank;
using VK.Blocks.Authorization.Features.TenantIsolation;
using VK.Blocks.Authorization.Features.WorkingHours;

namespace VK.Blocks.Authorization.DependencyInjection;

/// <summary>
/// Fluent extensions for <see cref="AuthorizationBuilder"/> to register the standard
/// VK authorization policies in a single call.
/// </summary>
public static class AuthorizationBuilderExtensions
{
    /// <summary>
    /// Registers all built-in VK authorization policies:
    /// <list type="bullet">
    ///   <item><see cref="VKPolicies.WorkingHoursOnly"/> 窶・time-window guard</item>
    ///   <item><see cref="VKPolicies.InternalNetworkOnly"/> 窶・CIDR guard</item>
    ///   <item><see cref="VKPolicies.SeniorAndAbove"/> 窶・rank guard (Senior+)</item>
    ///   <item><see cref="VKPolicies.FinancialDataWrite"/> 窶・composite of all three</item>
    /// </list>
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationBuilder"/> from <c>services.AddAuthorizationBuilder()</c>.</param>
    /// <param name="workStart">Start of working hours window (local time). Defaults to 09:00.</param>
    /// <param name="workEnd">End of working hours window (local time). Defaults to 18:00.</param>
    /// <param name="internalCidrs">
    ///     Allowed CIDR ranges. Pass <c>null</c> to use the default RFC1918 private ranges.
    /// </param>
    public static AuthorizationBuilder AddVKPolicies(
        this AuthorizationBuilder builder,
        TimeOnly? workStart = null,
        TimeOnly? workEnd = null,
        IReadOnlyList<string>? internalCidrs = null)
    {
        var start = workStart ?? new TimeOnly(9, 0);
        var end = workEnd ?? new TimeOnly(18, 0);
        var cidrs = internalCidrs ?? _defaultInternalCidrs;

        builder.AddPolicy(VKPolicies.WorkingHoursOnly, p =>
            p.RequireAuthenticatedUser()
             .AddRequirements(new WorkingHoursRequirement(start, end)));

        builder.AddPolicy(VKPolicies.InternalNetworkOnly, p =>
            p.RequireAuthenticatedUser()
             .AddRequirements(new InternalNetworkRequirement(cidrs)));

        builder.AddPolicy(VKPolicies.SeniorAndAbove, p =>
            p.RequireAuthenticatedUser()
             .AddRequirements(new MinimumRankRequirement(EmployeeRank.Senior)));

        // FinancialDataWrite = working hours AND internal network AND senior rank
        builder.AddPolicy(VKPolicies.FinancialDataWrite, p =>
            p.RequireAuthenticatedUser()
             .AddRequirements(
                 new WorkingHoursRequirement(start, end),
                 new InternalNetworkRequirement(cidrs),
                 new MinimumRankRequirement(EmployeeRank.Senior)));

        return builder;
    }

    // RFC 1918 private ranges used as a safe default for on-premises deployments.
    private static readonly IReadOnlyList<string> _defaultInternalCidrs =
    [
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
        "127.0.0.1/32"
    ];
}



