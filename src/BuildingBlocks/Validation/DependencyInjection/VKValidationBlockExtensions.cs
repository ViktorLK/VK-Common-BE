using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Validation.DependencyInjection.Internal;

namespace VK.Blocks.Validation;

/// <summary>
/// Extension methods for setting up validation services in an <see cref="IServiceCollection" />.
/// </summary>
public static class VKValidationBlockExtensions
{
    /// <summary>
    /// Adds the VK validation block configuration to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IVKValidationBuilder AddVKValidationBlock(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return ValidationBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds the VK validation block to the specified <see cref="IServiceCollection"/> using a functional transformation setup.
    /// Following ADR-016: Use 'with' expression to modify immutable options.
    /// </summary>
    public static IVKValidationBuilder AddVKValidationBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKValidationOptions, VKValidationOptions> configure)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(configure);
        return ValidationBlockRegistration.Register(services, configuration, configure);
    }
}
