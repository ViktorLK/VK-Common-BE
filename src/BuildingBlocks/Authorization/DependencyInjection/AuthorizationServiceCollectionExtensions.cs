using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Features.DynamicPolicies;
using VK.Blocks.Authorization.Features.InternalNetwork;
using VK.Blocks.Authorization.Features.MinimumRank;
using VK.Blocks.Authorization.Features.Permissions;
using VK.Blocks.Authorization.Features.Roles;
using VK.Blocks.Authorization.Features.TenantIsolation;
using VK.Blocks.Authorization.Features.WorkingHours;
using VK.Blocks.Authorization.Generated;
using VK.Blocks.Core.Abstractions;
using VK.Blocks.Core.DependencyInjection;
using VK.Blocks.Core.Internal;

namespace VK.Blocks.Authorization.DependencyInjection;

/// <summary>
/// Service collection extensions for VK.Blocks.Authorization module.
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    #region Main Registration

    /// <summary>
    /// Adds VK authorization services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">Current application configuration for binding authorization options.</param>
    /// <returns>A <see cref="IVKBlockBuilder{TMarker}"/> to chain further block configurations.</returns>
    public static IVKBlockBuilder<AuthorizationBlock> AddVKAuthorizationBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 0. Idempotency & Prerequisite Check
        if (services.IsVKBlockRegistered<AuthorizationBlock>())
        {
            return new VKBlockBuilder<AuthorizationBlock>(services);
        }

        if (!services.IsVKBlockRegistered<CoreBlock>())
        {
            throw new InvalidOperationException(
                string.Format(CoreConstants.MissingCoreRegistrationMessage, typeof(AuthorizationBlock).Assembly.GetName().Name));
        }

        // 1. Options Registration (Eager-bind, Singleton, DataAnnotations, ValidateOnStart)
        var options = services.AddVKBlockOptions<VKAuthorizationOptions>(
            configuration.GetSection(VKAuthorizationOptions.SectionName));

        services.TryAddSingleton<IValidateOptions<VKAuthorizationOptions>, VKAuthorizationOptionsValidator>();

        if (!options.Enabled)
        {
            return new VKBlockBuilder<AuthorizationBlock>(services);
        }

        // 2. Foundation & Prerequisites
        services.TryAddSingleton<ISyncStateStore, NoOpSyncStateStore>();

        // 3. Feature: Dynamic Policies
        services.TryAddScoped<IDynamicPolicyProvider, DefaultDynamicPolicyProvider>();
        services.TryAddScoped<IDynamicPolicyEvaluator, DefaultDynamicPolicyEvaluator>();

        // 4. Feature: Roles
        services.TryAddScoped<IRoleProvider, DefaultRoleProvider>();
        services.TryAddScoped<RoleHandler>();
        services.TryAddScoped<IRoleEvaluator>(sp => sp.GetRequiredService<RoleHandler>());

        // 5. Feature: Multi-Tenancy
        services.TryAddScoped<IUserTenantProvider, DefaultUserTenantProvider>();
        services.TryAddScoped<TenantAuthorizationHandler>();
        services.TryAddScoped<ITenantEvaluator>(sp => sp.GetRequiredService<TenantAuthorizationHandler>());

        // 6. Feature: Network & Infrastructure
        services.TryAddScoped<IIpAddressProvider, DefaultIpAddressProvider>();
        services.TryAddScoped<InternalNetworkAuthorizationHandler>();
        services.TryAddScoped<IInternalNetworkEvaluator>(sp => sp.GetRequiredService<InternalNetworkAuthorizationHandler>());

        // 7. Feature: Permissions
        services.TryAddScoped<IPermissionProvider, DefaultPermissionProvider>();
        services.TryAddScoped<PermissionHandler>();
        services.TryAddScoped<IPermissionEvaluator>(sp => sp.GetRequiredService<PermissionHandler>());

        // 8. Feature: Ranks & Working Hours
        services.TryAddScoped<IRankProvider, DefaultRankProvider>();
        services.TryAddScoped<MinimumRankAuthorizationHandler>();
        services.TryAddScoped<IMinimumRankEvaluator>(sp => sp.GetRequiredService<MinimumRankAuthorizationHandler>());
        
        services.TryAddScoped<IWorkingHoursProvider, DefaultWorkingHoursProvider>();
        services.TryAddScoped<WorkingHoursAuthorizationHandler>();
        services.TryAddScoped<IWorkingHoursEvaluator>(sp => sp.GetRequiredService<WorkingHoursAuthorizationHandler>());

        // 9. Source Generated Components
        // Auto-register generated IAuthorizationHandler and IVKAuthorizationHandler implementations
        services.AddGeneratedAuthorizationHandlers();

        // 10. Mark-Self (Success Commit)
        services.AddVKBlockMarker<AuthorizationBlock>();

        return new VKBlockBuilder<AuthorizationBlock>(services);
    }

    #endregion

}




