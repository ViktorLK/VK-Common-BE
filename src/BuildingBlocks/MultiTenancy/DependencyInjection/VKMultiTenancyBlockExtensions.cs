using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy.DependencyInjection.Internal;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Public extension methods for the MultiTenancy core block.
/// </summary>
public static class VKMultiTenancyBlockExtensions
{
    /// <summary>
    /// Adds core multi-tenancy services (Context, Provider, Store) to the service collection.
    /// </summary>
    public static IVKMultiTenancyBuilder AddMultiTenancyBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKMultiTenancyOptions, VKMultiTenancyOptions>? transform = null)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        return MultiTenancyBlockRegistration.Register(services, configuration, transform);
    }
}
