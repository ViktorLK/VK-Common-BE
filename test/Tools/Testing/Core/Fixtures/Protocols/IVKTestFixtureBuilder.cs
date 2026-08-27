using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Testing;

/// <summary>
/// Defines the builder contract for configuring VK test fixtures and container dependencies.
/// </summary>
public interface IVKTestFixtureBuilder
{
    /// <summary>
    /// Gets the service collection used to register test services and mocks.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the test configuration provider.
    /// </summary>
    IConfiguration Configuration { get; }
}
