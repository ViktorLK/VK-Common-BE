using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core.UnitTests.DependencyInjection;

public class VKBlockQueryExtensionsTests
{
    private readonly ServiceCollection _services = new ServiceCollection();

    [Fact]
    public void IsVKBlockRegistered_ById_ShouldReturnTrue_WhenRegistered()
    {
        // Arrange
        _services.AddVKBlockMarker<TestMarker>();

        // Act
        var result = _services.IsVKBlockRegistered("Test");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsVKBlockRegistered_ById_ShouldReturnFalse_WhenNotRegistered()
    {
        // Act
        var result = _services.IsVKBlockRegistered("NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsVKServiceRegistered_ByType_ShouldReturnTrue_WhenRegistered()
    {
        // Arrange
        _services.AddSingleton<ITestService, TestService>();

        // Act
        var result = _services.IsVKServiceRegistered(typeof(ITestService));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsVKServiceRegistered_ByType_ShouldReturnFalse_WhenNotRegistered()
    {
        // Act
        var result = _services.IsVKServiceRegistered(typeof(ITestService));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsVKServiceRegistered_NonGeneric_ReturnsCorrectValue()
    {
        // Arrange
        _services.AddSingleton<ITestService, TestService>();

        // Act & Assert
        _services.IsVKServiceRegistered(typeof(ITestService)).Should().BeTrue();
        _services.IsVKServiceRegistered(typeof(Microsoft.Extensions.Configuration.IConfiguration)).Should().BeFalse();
    }

    [Fact]
    public void GetVKServiceInstance_ShouldReturnInstance_WhenRegistered()
    {
        // Arrange
        var instance = new TestService();
        _services.AddSingleton<ITestService>(instance);

        // Act
        var result = _services.GetVKServiceInstance<ITestService>();

        // Assert
        result.Should().Be(instance);
    }

    [Fact]
    public void GetVKServiceInstance_ShouldReturnNull_WhenNotRegistered()
    {
        // Act
        var result = _services.GetVKServiceInstance<ITestService>();

        // Assert
        result.Should().BeNull();
    }

    private interface ITestService;
    private sealed class TestService : ITestService;

    private sealed class TestMarker : IVKBlockMarker, IVKBlockMarkerProvider<TestMarker>
    {
        public static IVKBlockMarker Instance { get; } = new TestMarker();
        public string Name => "Test";
        public string Identifier => "Test";
        public string Version => "1.0.0";
        public IReadOnlyList<IVKBlockMarker> Dependencies => [];
        public string ActivitySourceName => "Test";
        public string MeterName => "Test";
    }
}
