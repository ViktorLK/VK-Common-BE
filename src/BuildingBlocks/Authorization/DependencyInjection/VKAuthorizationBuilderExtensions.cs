using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.DynamicPolicies;
using VK.Blocks.Authorization.DynamicPolicies.Internal;
using VK.Blocks.Authorization.Entitlements.Internal;
using VK.Blocks.Authorization.InternalNetwork.Internal;
using VK.Blocks.Authorization.MinimumRank.Internal;
using VK.Blocks.Authorization.Permissions.Internal;
using VK.Blocks.Authorization.Roles.Internal;
using VK.Blocks.Authorization.TenantIsolation.Internal;
using VK.Blocks.Authorization.WorkingHours.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Fluent extensions for <see cref="AuthorizationBuilder"/> and <see cref="IVKAuthorizationBuilder"/>.
/// </summary>
public static class VKAuthorizationBuilderExtensions
{
    /// <summary>
    /// Registers built-in VK authorization policies using the configured feature options.
    /// </summary>
    /// <param name="builder">The authorization builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static AuthorizationBuilder AddVKAuthorizationPolicies(this AuthorizationBuilder builder)
    {
        // Named policies are only added if the corresponding feature options are registered and enabled.
        builder.Services.AddOptions<AuthorizationOptions>()
            .Configure<IServiceProvider>((authOptions, sp) =>
            {
                // Working Hours
                var workingHoursOptions = sp.GetService<IOptions<VKWorkingHoursOptions>>()?.Value;
                if (workingHoursOptions?.Enabled == true)
                {
                    authOptions.AddPolicy(VKAuthorizationPolicies.WorkingHoursOnly, p =>
                        p.RequireAuthenticatedUser()
                         .AddRequirements(new VKWorkingHoursRequirement(workingHoursOptions.WorkStart, workingHoursOptions.WorkEnd)));
                }

                // Internal Network
                var networkOptions = sp.GetService<IOptions<VKInternalNetworkOptions>>()?.Value;
                if (networkOptions?.Enabled == true)
                {
                    authOptions.AddPolicy(VKAuthorizationPolicies.InternalNetworkOnly, p =>
                        p.RequireAuthenticatedUser()
                         .AddRequirements(new VKInternalNetworkRequirement(networkOptions.InternalCidrs)));
                }

                // Senior Rank
                var rankOptions = sp.GetService<IOptions<VKMinimumRankOptions>>()?.Value;
                if (rankOptions?.Enabled == true)
                {
                    authOptions.AddPolicy(VKAuthorizationPolicies.SeniorAndAbove, p =>
                        p.RequireAuthenticatedUser()
                         .AddRequirements(new VKMinimumRankRequirement((int)VKEmployeeRank.Senior, typeof(VKEmployeeRank))));
                }

                // Financial Write (Composite)
                if (workingHoursOptions?.Enabled == true && networkOptions?.Enabled == true && rankOptions?.Enabled == true)
                {
                    authOptions.AddPolicy(VKAuthorizationPolicies.FinancialDataWrite, p =>
                        p.RequireAuthenticatedUser()
                         .AddRequirements(
                             new VKWorkingHoursRequirement(workingHoursOptions.WorkStart, workingHoursOptions.WorkEnd),
                             new VKInternalNetworkRequirement(networkOptions.InternalCidrs),
                             new VKMinimumRankRequirement((int)VKEmployeeRank.Senior, typeof(VKEmployeeRank))));
                }
            });

        return builder;
    }

    /// <summary>
    /// Adds all default VK authorization features (Permissions, Roles, TenantIsolation, etc.)
    /// and registers the corresponding named policies.
    /// </summary>
    public static IVKAuthorizationBuilder AddDefaultFeatures(this IVKAuthorizationBuilder builder)
    {
        builder.AddPermissions()
               .AddRoles()
               .AddTenantIsolation()
               .AddEntitlements()
               .AddInternalNetwork()
               .AddMinimumRank()
               .AddWorkingHours()
               .AddDynamicPolicies();

        // Automatically register named policies for the enabled features
        builder.Services.AddAuthorizationBuilder().AddVKAuthorizationPolicies();

        return builder;
    }

    /// <summary>
    /// Adds the Permissions authorization feature.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddPermissions(
        this IVKAuthorizationBuilder builder,
        Func<VKPermissionOptions, VKPermissionOptions>? transform = null)
        => PermissionsRegistration.Register(builder, transform);

    /// <summary>
    /// Adds the Roles authorization feature.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddRoles(
        this IVKAuthorizationBuilder builder,
        Func<VKRoleOptions, VKRoleOptions>? transform = null)
        => RolesRegistration.Register(builder, transform);

    /// <summary>
    /// Adds the Tenant Isolation authorization feature.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddTenantIsolation(
        this IVKAuthorizationBuilder builder,
        Func<VKTenantIsolationOptions, VKTenantIsolationOptions>? transform = null)
        => TenantIsolationRegistration.Register(builder, transform);

    /// <summary>
    /// Adds the Entitlements (Tenant Feature) authorization feature.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddEntitlements(
        this IVKAuthorizationBuilder builder,
        Func<VKEntitlementsOptions, VKEntitlementsOptions>? transform = null)
        => EntitlementsRegistration.Register(builder, transform);

    /// <summary>
    /// Adds the Internal Network authorization feature.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddInternalNetwork(
        this IVKAuthorizationBuilder builder,
        Func<VKInternalNetworkOptions, VKInternalNetworkOptions>? transform = null)
        => InternalNetworkRegistration.Register(builder, transform);

    /// <summary>
    /// Adds the Minimum Rank authorization feature.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddMinimumRank(
        this IVKAuthorizationBuilder builder,
        Func<VKMinimumRankOptions, VKMinimumRankOptions>? transform = null)
        => MinimumRankRegistration.Register(builder, transform);

    /// <summary>
    /// Adds the Working Hours authorization feature.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddWorkingHours(
        this IVKAuthorizationBuilder builder,
        Func<VKWorkingHoursOptions, VKWorkingHoursOptions>? transform = null)
        => WorkingHoursRegistration.Register(builder, transform);

    /// <summary>
    /// Adds the Dynamic Policies authorization feature.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddDynamicPolicies(
        this IVKAuthorizationBuilder builder,
        Func<VKDynamicPoliciesOptions, VKDynamicPoliciesOptions>? transform = null)
        => DynamicPoliciesRegistration.Register(builder, transform);

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
