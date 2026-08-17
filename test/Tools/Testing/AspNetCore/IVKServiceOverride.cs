using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.Testing.AspNetCore;

/// <summary>
/// Encapsulates a service replacement strategy for test hosts.
/// </summary>
public interface IVKServiceOverride
{
    /// <summary>
    /// Applies service replacements to the DI container.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    void Apply(IServiceCollection services);
}

/// <summary>
/// Replaces a service registration with a singleton instance.
/// </summary>
/// <typeparam name="TService">The service type to override.</typeparam>
public sealed class VKSingletonOverride<TService> : IVKServiceOverride
    where TService : class
{
    private readonly TService _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKSingletonOverride{TService}"/> class.
    /// </summary>
    /// <param name="instance">The singleton instance.</param>
    public VKSingletonOverride(TService instance)
    {
        _instance = instance;
    }

    /// <inheritdoc />
    public void Apply(IServiceCollection services)
    {
        services.RemoveAll<TService>();
        services.AddSingleton(_instance);
    }
}

/// <summary>
/// Replaces a service registration with a factory delegate.
/// </summary>
/// <typeparam name="TService">The service type to override.</typeparam>
public sealed class VKFactoryOverride<TService> : IVKServiceOverride
    where TService : class
{
    private readonly Func<IServiceProvider, TService> _factory;
    private readonly ServiceLifetime _lifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKFactoryOverride{TService}"/> class.
    /// </summary>
    /// <param name="factory">The service factory delegate.</param>
    /// <param name="lifetime">The target lifetime (default Scoped).</param>
    public VKFactoryOverride(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        _factory = factory;
        _lifetime = lifetime;
    }

    /// <inheritdoc />
    public void Apply(IServiceCollection services)
    {
        services.RemoveAll<TService>();
        services.Add(new ServiceDescriptor(typeof(TService), _factory, _lifetime));
    }
}
