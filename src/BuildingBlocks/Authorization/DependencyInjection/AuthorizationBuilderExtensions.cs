using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.Features.DynamicPolicies;
using VK.Blocks.Authorization.Features.InternalNetwork;
using VK.Blocks.Authorization.Features.MinimumRank;
using VK.Blocks.Authorization.Features.MinimumRank.Metadata;
using VK.Blocks.Authorization.Features.Permissions;
using VK.Blocks.Authorization.Features.Permissions.Persistence;
using VK.Blocks.Authorization.Features.Roles;
using VK.Blocks.Authorization.Features.TenantIsolation;
using VK.Blocks.Authorization.Features.WorkingHours;
using VK.Blocks.Core.Abstractions;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authorization.DependencyInjection;

/// <summary>
/// Fluent extensions for <see cref="AuthorizationBuilder"/> to register the standard
/// VK authorization policies in a single call.
/// </summary>
public static class AuthorizationBuilderExtensions
{
    /// <summary>
    /// Registers built-in VK authorization policies selectively.
    /// </summary>
    /// <param name="builder">The authorization builder.</param>
    /// <param name="options">The authorization options. If null, default values are used.</param>
    /// <returns>The same builder for chaining.</returns>
    public static AuthorizationBuilder AddVKAuthorizationPolicies(this AuthorizationBuilder builder, VKAuthorizationOptions? options = null)
    {
        var opt = options ?? new VKAuthorizationOptions();
        var policies = opt.EnabledPolicies;
        var start = opt.WorkStart;
        var end = opt.WorkEnd;
        var cidrs = opt.InternalCidrs;

        if (policies.HasFlag(VKAuthorizationPolicyFlags.WorkingHours))
        {
            builder.AddPolicy(VKAuthorizationPolicies.WorkingHoursOnly, p =>
                p.RequireAuthenticatedUser()
                 .AddRequirements(new WorkingHoursRequirement(start, end)));
        }

        if (policies.HasFlag(VKAuthorizationPolicyFlags.InternalNetwork))
        {
            builder.AddPolicy(VKAuthorizationPolicies.InternalNetworkOnly, p =>
                p.RequireAuthenticatedUser()
                 .AddRequirements(new InternalNetworkRequirement(cidrs)));
        }

        if (policies.HasFlag(VKAuthorizationPolicyFlags.SeniorRank))
        {
            builder.AddPolicy(VKAuthorizationPolicies.SeniorAndAbove, p =>
                p.RequireAuthenticatedUser()
                 .AddRequirements(new MinimumRankRequirement((int)EmployeeRank.Senior, typeof(EmployeeRank))));
        }

        if (policies.HasFlag(VKAuthorizationPolicyFlags.FinancialWrite))
        {
            // FinancialDataWrite = working hours AND internal network AND senior rank
            builder.AddPolicy(VKAuthorizationPolicies.FinancialDataWrite, p =>
                p.RequireAuthenticatedUser()
                 .AddRequirements(
                     new WorkingHoursRequirement(start, end),
                     new InternalNetworkRequirement(cidrs),
                     new MinimumRankRequirement((int)EmployeeRank.Senior, typeof(EmployeeRank))));
        }

        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IUserTenantProvider"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithUserTenantProvider<TProvider>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TProvider : class, IUserTenantProvider
        => builder.WithScoped<AuthorizationBlock, IUserTenantProvider, TProvider>();

    /// <summary>
    /// Overrides the default <see cref="IIpAddressProvider"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithIpAddressProvider<TProvider>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TProvider : class, IIpAddressProvider
        => builder.WithScoped<AuthorizationBlock, IIpAddressProvider, TProvider>();

    /// <summary>
    /// Overrides the default <see cref="IPermissionProvider"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithPermissionProvider<TProvider>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TProvider : class, IPermissionProvider
        => builder.WithScoped<AuthorizationBlock, IPermissionProvider, TProvider>();

    /// <summary>
    /// Overrides the default <see cref="IRoleProvider"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithRoleProvider<TProvider>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TProvider : class, IRoleProvider
        => builder.WithScoped<AuthorizationBlock, IRoleProvider, TProvider>();

    /// <summary>
    /// Overrides the default <see cref="IRankProvider"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithRankProvider<TProvider>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TProvider : class, IRankProvider
        => builder.WithScoped<AuthorizationBlock, IRankProvider, TProvider>();

    /// <summary>
    /// Overrides the default <see cref="IWorkingHoursProvider"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithWorkingHoursProvider<TProvider>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TProvider : class, IWorkingHoursProvider
        => builder.WithScoped<AuthorizationBlock, IWorkingHoursProvider, TProvider>();

    /// <summary>
    /// Overrides the default <see cref="TimeProvider"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithTimeProvider<TProvider>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TProvider : TimeProvider
        => builder.WithSingleton<AuthorizationBlock, TimeProvider, TProvider>();

    /// <summary>
    /// Overrides the default <see cref="IDynamicPolicyProvider"/> logic.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithDynamicPolicyProvider<TProvider>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TProvider : class, IDynamicPolicyProvider
        => builder.WithScoped<AuthorizationBlock, IDynamicPolicyProvider, TProvider>();

    #region Evaluator Overrides (Logic Layer)

    /// <summary>
    /// Overrides the default <see cref="IPermissionEvaluator"/> logic.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithPermissionEvaluator<TEvaluator>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TEvaluator : class, IPermissionEvaluator
        => builder.WithScoped<AuthorizationBlock, IPermissionEvaluator, TEvaluator>();

    /// <summary>
    /// Overrides the default <see cref="IRoleEvaluator"/> logic.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithRoleEvaluator<TEvaluator>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TEvaluator : class, IRoleEvaluator
        => builder.WithScoped<AuthorizationBlock, IRoleEvaluator, TEvaluator>();

    /// <summary>
    /// Overrides the default <see cref="ITenantEvaluator"/> logic.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithTenantEvaluator<TEvaluator>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TEvaluator : class, ITenantEvaluator
        => builder.WithScoped<AuthorizationBlock, ITenantEvaluator, TEvaluator>();

    /// <summary>
    /// Overrides the default <see cref="IInternalNetworkEvaluator"/> logic.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithInternalNetworkEvaluator<TEvaluator>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TEvaluator : class, IInternalNetworkEvaluator
        => builder.WithScoped<AuthorizationBlock, IInternalNetworkEvaluator, TEvaluator>();

    /// <summary>
    /// Overrides the default <see cref="IMinimumRankEvaluator"/> logic.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithMinimumRankEvaluator<TEvaluator>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TEvaluator : class, IMinimumRankEvaluator
        => builder.WithScoped<AuthorizationBlock, IMinimumRankEvaluator, TEvaluator>();

    /// <summary>
    /// Overrides the default <see cref="IWorkingHoursEvaluator"/> logic.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithWorkingHoursEvaluator<TEvaluator>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TEvaluator : class, IWorkingHoursEvaluator
        => builder.WithScoped<AuthorizationBlock, IWorkingHoursEvaluator, TEvaluator>();

    /// <summary>
    /// Overrides the default <see cref="IDynamicPolicyEvaluator"/> logic.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithDynamicPolicyEvaluator<TEvaluator>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TEvaluator : class, IDynamicPolicyEvaluator
        => builder.WithScoped<AuthorizationBlock, IDynamicPolicyEvaluator, TEvaluator>();

    /// <summary>
    /// Overrides the default <see cref="IPermissionStore"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithPermissionStore<TImplementation>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TImplementation : class, IPermissionStore
        => builder.WithScoped<AuthorizationBlock, IPermissionStore, TImplementation>();

    /// <summary>
    /// Overrides the default <see cref="ISyncStateStore"/> with a custom implementation.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> WithSyncStateStore<TImplementation>(
        this IVKBlockBuilder<AuthorizationBlock> builder)
        where TImplementation : class, ISyncStateStore
        => builder.WithSingleton<AuthorizationBlock, ISyncStateStore, TImplementation>();

    #endregion
}
