using Xunit;

namespace VK.Blocks.Testing;

/// <summary>
/// Core lifecycle contract for all VK test fixtures.
/// Implements xUnit's <see cref="IAsyncLifetime"/> for framework integration.
/// </summary>
public interface IVKTestFixture : IAsyncLifetime
{
    /// <summary>
    /// Gets the service provider scoped to this fixture.
    /// </summary>
    IServiceProvider Services { get; }
}
