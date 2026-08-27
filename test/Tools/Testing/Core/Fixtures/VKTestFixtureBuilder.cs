using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Testing;

/// <summary>
/// Default implementation of <see cref="IVKTestFixtureBuilder"/> providing unified access to
/// <see cref="IServiceCollection"/> and <see cref="IConfiguration"/> for test fixtures.
/// </summary>
public class VKTestFixtureBuilder : IVKTestFixtureBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKTestFixtureBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="configuration">The optional configuration instance.</param>
    public VKTestFixtureBuilder(IServiceCollection services, IConfiguration? configuration = null)
    {
        Services = services;
        Configuration = configuration ?? VKEmptyConfiguration.Instance;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IConfiguration Configuration { get; }
}
