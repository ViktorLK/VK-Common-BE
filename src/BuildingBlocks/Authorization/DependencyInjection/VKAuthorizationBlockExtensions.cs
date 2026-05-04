using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authorization.DependencyInjection.Internal;

namespace VK.Blocks.Authorization;

/// <summary>
/// Service collection extensions for VK.Blocks.Authorization module.
/// Public API Wrapper following Rule 18.1.
/// </summary>
public static class VKAuthorizationBlockExtensions
{
    /// <summary>
    /// Adds VK authorization services to the specified <see cref="IServiceCollection"/>.
    /// [WRAPPER] pattern for IConfiguration-based registration.
    /// </summary>
    public static IVKAuthorizationBuilder AddVKAuthorizationBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return AuthorizationBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds VK authorization services to the specified <see cref="IServiceCollection"/> using a functional transformation.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthorizationBuilder AddVKAuthorizationBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAuthorizationOptions, VKAuthorizationOptions> transform)
    {
        return AuthorizationBlockRegistration.Register(services, configuration, transform);
    }
}
