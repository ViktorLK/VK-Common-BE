using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Contracts;
using VK.Blocks.Authorization.Diagnostics;
using VK.Blocks.Authorization.Features.DynamicPolicies;
using VK.Blocks.Authorization.Features.InternalNetwork;
using VK.Blocks.Authorization.Features.MinimumRank;
using VK.Blocks.Authorization.Features.Permissions;
using VK.Blocks.Authorization.Features.Roles;
using VK.Blocks.Authorization.Features.TenantIsolation;
using VK.Blocks.Authorization.Features.WorkingHours;
using VK.Blocks.Authorization.Generated;
using VK.Blocks.Core.DependencyInjection;
using VK.Blocks.Core.Security;
using VK.Blocks.Core.Utilities.State;

namespace VK.Blocks.Authorization.DependencyInjection;

/// <summary>
/// Service collection extensions for VK.Blocks.Authorization module.
/// </summary>
public static class AuthorizationBlockExtensions
{
    /// <summary>
    /// Adds VK authorization services to the specified <see cref="IServiceCollection"/>.
    /// [WRAPPER] pattern for IConfiguration-based registration.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> AddVKAuthorizationBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddVKAuthorizationBlockInternal(() => services.AddVKBlockOptions<VKAuthorizationOptions>(configuration));
    }

    /// <summary>
    /// Adds VK authorization services to the specified <see cref="IServiceCollection"/> using a setup action.
    /// </summary>
    public static IVKBlockBuilder<AuthorizationBlock> AddVKAuthorizationBlock(
        this IServiceCollection services,
        Action<VKAuthorizationOptions>? configure = null)
    {
        return services.AddVKAuthorizationBlockInternal(() => services.AddVKBlockOptions(configure));
    }

    /// <summary>
    /// Template method for authorization registration lifecycle.
    /// Following Rule 13: Check-Self, Check-Prerequisite, Options, Mark-Self.
    /// </summary>
    private static IVKBlockBuilder<AuthorizationBlock> AddVKAuthorizationBlockInternal(
        this IServiceCollection services,
        Func<VKAuthorizationOptions> registerOptionsFunc)
    {
        // 1. Check for self-registration
        if (services.IsVKBlockRegistered<AuthorizationBlock>())
        {
            return new VKBlockBuilder<AuthorizationBlock>(services);
        }

        // 2. Validate prerequisites
        services.EnsureVKCoreBlockRegistered<AuthorizationBlock>();

        // 3. Register options
        var options = registerOptionsFunc();

        // 4. Initialize builder
        var builder = new VKBlockBuilder<AuthorizationBlock>(services);

        // 5. Register common infrastructure
        builder.TryAddEnumerableSingleton<AuthorizationBlock, IValidateOptions<VKAuthorizationOptions>, VKAuthorizationOptionsValidator>();
        services.TryAddEnumerableSingleton<ISecurityMetadataProvider, AuthorizationMetadataProvider>();

        // 6. Register self-marker (Commit)
        services.AddVKBlockMarker<AuthorizationBlock>();

        // 7. Check for feature-activation and register features
        if (!options.Enabled)
        {
            return builder;
        }

        return services.AddAuthorizationFeatures(builder);
    }


    private static IVKBlockBuilder<AuthorizationBlock> AddAuthorizationFeatures(
        this IServiceCollection services,
        IVKBlockBuilder<AuthorizationBlock> builder)
    {
        // Foundation & Prerequisites
        services.TryAddSingleton<ISyncStateStore, NoOpSyncStateStore>();

        // Features (Vertical Slices)
        services.AddDynamicPoliciesFeature();
        services.AddRolesFeature();
        services.AddTenantIsolationFeature();
        services.AddInternalNetworkFeature();
        services.AddPermissionsFeature();
        services.AddMinimumRankFeature();
        services.AddWorkingHoursFeature();

        // Source Generated Components
        services.AddGeneratedAuthorizationHandlers();

        return builder;
    }
}






