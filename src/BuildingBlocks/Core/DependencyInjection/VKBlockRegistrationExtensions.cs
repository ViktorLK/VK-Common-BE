using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Core extension methods for setting up building block services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class VKBlockRegistrationExtensions
{
    /// <summary>
    /// Adds and configures a building block's options using a specific configuration section.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>ARCHITECTURE NOTE (Dual-Registration Pattern):</b><br/>
    /// This method performs a "double registration" of the options:
    /// <list type="number">
    /// <item>Standard <c>IOptions&lt;T&gt;</c> registration for lazy-loading and ASP.NET Core compatibility.</item>
    /// <item>Direct <c>Singleton</c> registration of the same instance for eager-loading.</item>
    /// </list>
    /// <b>Why?</b><br/>
    /// 1. <b>Library Internal Access:</b> Building blocks often need synchronous access to their options during the
    /// <c>ConfigureServices</c> phase to make structural decisions (e.g., conditional interceptor registration).<br/>
    /// 2. <b>External Consumer Access:</b> Allows applications to inject the raw options class directly instead of
    /// <c>IOptions&lt;T&gt;</c> for cleaner code in the business layer.
    /// </para>
    /// </remarks>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section to bind from.</param>
    /// <returns>The eagerly-binded options instance for immediate use in the registration pipeline.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration section)
        where TOptions : class, new()
    {
        var options = new TOptions();
        section.Bind(options);

        // Standard Options registration
        services.AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Singleton registration for direct injection & library-internal synchronous access
        services.AddSingleton(options);

        return options;
    }

    /// <summary>
    /// Adds and configures a building block's options using a manual configuration delegate.
    /// </summary>
    /// <remarks>
    /// See the <see cref="AddVKBlockOptions{TOptions}(IServiceCollection, IConfiguration)"/>
    /// documentation for details on the dual-registration pattern used here.
    /// </remarks>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Delegate to configure the options.</param>
    /// <returns>The configured options instance.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        Action<TOptions> configure)
        where TOptions : class, new()
    {
        var options = new TOptions();
        configure(options);

        // Standard Options registration
        services.AddOptions<TOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Singleton registration
        services.AddSingleton(options);

        return options;
    }
}
