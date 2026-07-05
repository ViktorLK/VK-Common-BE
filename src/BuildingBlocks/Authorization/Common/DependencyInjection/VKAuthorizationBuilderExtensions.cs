using System;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Fluent extensions for <see cref="AuthorizationBuilder"/> and <see cref="IVKAuthorizationBuilder"/>.
/// </summary>
public static partial class VKAuthorizationBuilderExtensions
{
    /// <summary>
    /// Adds a custom <see cref="IVKUserTenantProvider"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddUserTenantProvider<TProvider>(
        this IVKAuthorizationBuilder builder)
        where TProvider : class, IVKUserTenantProvider
    {
        builder.WithScoped<VKAuthorizationBlock, IVKUserTenantProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKIpAddressProvider"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddIpAddressProvider<TProvider>(
        this IVKAuthorizationBuilder builder)
        where TProvider : class, IVKIpAddressProvider
    {
        builder.WithScoped<VKAuthorizationBlock, IVKIpAddressProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKPermissionProvider"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddPermissionProvider<TProvider>(
        this IVKAuthorizationBuilder builder)
        where TProvider : class, IVKPermissionProvider
    {
        builder.WithScoped<VKAuthorizationBlock, IVKPermissionProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKRoleProvider"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddRoleProvider<TProvider>(
        this IVKAuthorizationBuilder builder)
        where TProvider : class, IVKRoleProvider
    {
        builder.WithScoped<VKAuthorizationBlock, IVKRoleProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKRankProvider"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddRankProvider<TProvider>(
        this IVKAuthorizationBuilder builder)
        where TProvider : class, IVKRankProvider
    {
        builder.WithScoped<VKAuthorizationBlock, IVKRankProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKWorkingHoursProvider"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddWorkingHoursProvider<TProvider>(
        this IVKAuthorizationBuilder builder)
        where TProvider : class, IVKWorkingHoursProvider
    {
        builder.WithScoped<VKAuthorizationBlock, IVKWorkingHoursProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="TimeProvider"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddTimeProvider<TProvider>(
        this IVKAuthorizationBuilder builder)
        where TProvider : TimeProvider
    {
        builder.WithSingleton<VKAuthorizationBlock, TimeProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKDynamicPoliciesProvider"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddDynamicPoliciesProvider<TProvider>(
        this IVKAuthorizationBuilder builder)
        where TProvider : class, IVKDynamicPoliciesProvider
    {
        builder.WithScoped<VKAuthorizationBlock, IVKDynamicPoliciesProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKPermissionEvaluator"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddPermissionEvaluator<TEvaluator>(
        this IVKAuthorizationBuilder builder)
        where TEvaluator : class, IVKPermissionEvaluator
    {
        builder.WithScoped<VKAuthorizationBlock, IVKPermissionEvaluator, TEvaluator>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKRoleEvaluator"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddRoleEvaluator<TEvaluator>(
        this IVKAuthorizationBuilder builder)
        where TEvaluator : class, IVKRoleEvaluator
    {
        builder.WithScoped<VKAuthorizationBlock, IVKRoleEvaluator, TEvaluator>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKTenantEvaluator"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddTenantEvaluator<TEvaluator>(
        this IVKAuthorizationBuilder builder)
        where TEvaluator : class, IVKTenantEvaluator
    {
        builder.WithScoped<VKAuthorizationBlock, IVKTenantEvaluator, TEvaluator>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKInternalNetworkEvaluator"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddInternalNetworkEvaluator<TEvaluator>(
        this IVKAuthorizationBuilder builder)
        where TEvaluator : class, IVKInternalNetworkEvaluator
    {
        builder.WithScoped<VKAuthorizationBlock, IVKInternalNetworkEvaluator, TEvaluator>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKMinimumRankEvaluator"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddMinimumRankEvaluator<TEvaluator>(
        this IVKAuthorizationBuilder builder)
        where TEvaluator : class, IVKMinimumRankEvaluator
    {
        builder.WithScoped<VKAuthorizationBlock, IVKMinimumRankEvaluator, TEvaluator>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKWorkingHoursEvaluator"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddWorkingHoursEvaluator<TEvaluator>(
        this IVKAuthorizationBuilder builder)
        where TEvaluator : class, IVKWorkingHoursEvaluator
    {
        builder.WithScoped<VKAuthorizationBlock, IVKWorkingHoursEvaluator, TEvaluator>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKDynamicPoliciesEvaluator"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddDynamicPoliciesEvaluator<TEvaluator>(
        this IVKAuthorizationBuilder builder)
        where TEvaluator : class, IVKDynamicPoliciesEvaluator
    {
        builder.WithScoped<VKAuthorizationBlock, IVKDynamicPoliciesEvaluator, TEvaluator>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKPermissionStore"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddPermissionStore<TImplementation>(
        this IVKAuthorizationBuilder builder)
        where TImplementation : class, IVKPermissionStore
    {
        builder.WithScoped<VKAuthorizationBlock, IVKPermissionStore, TImplementation>();
        return builder;
    }

    /// <summary>
    /// Adds a custom <see cref="IVKSyncStateStore"/> implementation.
    /// </summary>
    public static IVKAuthorizationBuilder AddSyncStateStore<TImplementation>(
        this IVKAuthorizationBuilder builder)
        where TImplementation : class, IVKSyncStateStore
    {
        builder.WithSingleton<VKAuthorizationBlock, IVKSyncStateStore, TImplementation>();
        return builder;
    }
}
