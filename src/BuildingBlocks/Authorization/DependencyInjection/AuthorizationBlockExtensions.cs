using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Diagnostics;
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
using VK.Blocks.Core.Diagnostics;
using VK.Blocks.Core.Internal;

namespace VK.Blocks.Authorization.DependencyInjection;

/// <summary>
/// Service collection extensions for VK.Blocks.Authorization module.
/// </summary>
public static class AuthorizationBlockExtensions
{
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

        // Initialize builder early to use registration helpers
        var builder = new VKBlockBuilder<AuthorizationBlock>(services);

        // NOTE: Must use TryAddEnumerableSingleton here because AddVKBlockOptions registers its own IValidateOptions.
        // TryAddSingleton would be blocked by the existing registration.
        builder.TryAddEnumerableSingleton<AuthorizationBlock, IValidateOptions<VKAuthorizationOptions>, VKAuthorizationOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        // 2. Foundation & Prerequisites
        services.TryAddSingleton<ISyncStateStore, NoOpSyncStateStore>();

        // 3. Feature: Dynamic Policies
        services.AddDynamicPoliciesFeature();

        // 4. Feature: Roles
        services.AddRolesFeature();

        // 5. Feature: Multi-Tenancy
        services.AddTenantIsolationFeature();

        // 6. Feature: Network & Infrastructure
        services.AddInternalNetworkFeature();

        // 7. Feature: Permissions
        services.AddPermissionsFeature();

        // 8. Feature: Ranks & Working Hours
        services.AddMinimumRankFeature();
        services.AddWorkingHoursFeature();

        // 9. Source Generated Components
        // Auto-register generated IAuthorizationHandler and IVKAuthorizationHandler implementations
        services.AddGeneratedAuthorizationHandlers();

        // Register the security metadata provider for web discovery
        services.TryAddEnumerableSingleton<ISecurityMetadataProvider, AuthorizationMetadataProvider>();

        // 10. Mark-Self (Success Commit)
        services.AddVKBlockMarker<AuthorizationBlock>();

        return new VKBlockBuilder<AuthorizationBlock>(services);
    }
}
