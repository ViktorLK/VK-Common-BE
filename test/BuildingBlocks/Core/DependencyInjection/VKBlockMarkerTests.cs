using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core.UnitTests.DependencyInjection;

public class VKBlockMarkerTests
{
    private readonly ServiceCollection _services = new ServiceCollection();

    [Fact]
    public void EnsureDependenciesRegistered_WhenDependencyMissing_ThrowsVKDependencyException()
    {
        // Arrange
        var dep = new MockMarker("Dep");
        var root = (IVKBlockMarker)new MockMarker("Root", [dep]);

        // Act & Assert
        var action = () => root.EnsureDependenciesRegistered(_services, "App");
        action.Should().Throw<VKDependencyException>()
            .WithMessage("*requires 'Dep' to be registered first*");
    }

    [Fact]
    public void EnsureDependenciesRegistered_WhenDependencyRegistered_Succeeds()
    {
        // Arrange
        var dep = new MockMarker("Dep");
        var root = (IVKBlockMarker)new MockMarker("Root", [dep]);

        // Register internal identity marker (Rule 13)
        _services.AddSingleton(new VK.Blocks.Core.DependencyInjection.Internal.BlockRuntimeMarker("Dep"));

        // Act & Assert
        var action = () => root.EnsureDependenciesRegistered(_services, "App");
        action.Should().NotThrow();
    }

    [Fact]
    public void EnsureDependenciesRegistered_CircularDependency_HandlesGracefully()
    {
        // Arrange
        var markerA = new MockMarker("A");
        var markerB = new MockMarker("B");

        markerA.SetDependencies([markerB]);
        markerB.SetDependencies([markerA]);

        // Register internal identity markers (Rule 13)
        _services.AddSingleton(new VK.Blocks.Core.DependencyInjection.Internal.BlockRuntimeMarker("A"));
        _services.AddSingleton(new VK.Blocks.Core.DependencyInjection.Internal.BlockRuntimeMarker("B"));

        // Act & Assert
        var action = () => ((IVKBlockMarker)markerA).EnsureDependenciesRegistered(_services, "App");
        action.Should().NotThrow();
    }

    private sealed class MockMarker : IVKBlockMarker
    {
        private List<IVKBlockMarker> _dependencies;

        public MockMarker(string id, IEnumerable<IVKBlockMarker>? dependencies = null)
        {
            Identifier = id;
            _dependencies = dependencies != null ? new List<IVKBlockMarker>(dependencies) : new List<IVKBlockMarker>();
        }

        public string Name => Identifier;
        public string Identifier { get; }
        public string Version => "1.0.0";
        public IReadOnlyList<IVKBlockMarker> Dependencies => _dependencies;
        public string ActivitySourceName => Identifier;
        public string MeterName => Identifier;

        public void SetDependencies(IEnumerable<IVKBlockMarker> dependencies)
        {
            _dependencies = new List<IVKBlockMarker>(dependencies);
        }
    }
}
