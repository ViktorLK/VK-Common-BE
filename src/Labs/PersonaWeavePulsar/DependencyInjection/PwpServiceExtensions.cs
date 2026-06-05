using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Labs.PersonaWeavePulsar.DependencyInjection;

/// <summary>
/// Extension methods for registering the PWP backend services.
/// </summary>
public static class PwpServiceExtensions
{
    /// <summary>
    /// Adds PWP (PersonaWeavePulsar) backend services.
    /// </summary>
    public static IPwpBuilder AddPwpServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<PwpOptions, PwpOptions>? configure = null)
        => PwpServiceRegistration.Register(services, configuration, configure);
}
