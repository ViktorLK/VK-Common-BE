using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Persistence.DependencyInjection.Internal;

namespace VK.Blocks.Persistence;

/// <summary>
/// Service collection extensions for the Persistence building block.
/// </summary>
public static class VKPersistenceBlockExtensions
{
    /// <summary>
    /// Adds the Persistence building block to the service collection using <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The persistence block builder.</returns>
    public static IVKPersistenceBuilder AddPersistenceBlock(this IServiceCollection services, IConfiguration configuration)
        => PersistenceBlockRegistration.Register(services, configuration);

    /// <summary>
    /// Adds the Persistence building block to the service collection using a functional transform.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="transform">Function to transform options.</param>
    /// <returns>The persistence block builder.</returns>
    public static IVKPersistenceBuilder AddPersistenceBlock(this IServiceCollection services, Func<VKPersistenceOptions, VKPersistenceOptions> transform)
        => PersistenceBlockRegistration.Register(services, null!, transform);
}
