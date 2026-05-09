using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling.DependencyInjection.Internal;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Extension methods for registering exception mapping services.
/// </summary>
public static class VKExceptionHandlingBlockExtensions
{
    /// <summary>
    /// Adds exception mapping services to the specified <see cref="IServiceCollection"/> using configuration.
    /// [WRAPPER] pattern for IConfiguration-based registration.
    /// </summary>
    public static IVKExceptionHandlingBuilder AddExceptionHandlingBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return ExceptionHandlingBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds exception mapping services to the specified <see cref="IServiceCollection"/> using manual configuration.
    /// Following ADR-016: Supports immutable options via 'with' expressions.
    /// </summary>
    public static IVKExceptionHandlingBuilder AddExceptionHandlingBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKExceptionHandlingOptions, VKExceptionHandlingOptions> transform)
    {
        services.AddVKBlockOptions(configuration, transform);
        return ExceptionHandlingBlockRegistration.Register(services, configuration);
    }
}
