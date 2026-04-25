using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core.UnitTests.DependencyInjection;

public sealed class VKBlockServiceExtensionsTests
{
    private readonly ServiceCollection _services = new ServiceCollection();

    [Fact]
    public void AddVKBlockMarker_ShouldRegisterMarkerType()
    {
        // Act
        _services.AddVKBlockMarker<TestMarker>();

        // Assert
        _services.Any(d => d.ServiceType == typeof(TestMarker)).Should().BeTrue();

        var provider = _services.BuildServiceProvider();
        provider.GetService<TestMarker>().Should().NotBeNull();
    }

    [Fact]
    public void IsVKBlockRegistered_ShouldReturnTrueIfMarkerExists()
    {
        // Arrange
        _services.AddVKBlockMarker<TestMarker>();

        // Act
        var result = _services.IsVKBlockRegistered<TestMarker>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsVKBlockRegistered_ShouldReturnFalseIfMarkerMissing()
    {
        // Act
        var result = _services.IsVKBlockRegistered<TestMarker>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsVKBlockRegistered_WhenMissing_ShouldNotThrow()
    {
        // Act
        Action act = () => _services.IsVKBlockRegistered<TestMarker>();

        // Assert
        act.Should().NotThrow();
        _services.IsVKBlockRegistered<TestMarker>().Should().BeFalse();
    }

    [Fact]
    public void EnsureVKBlockRegistered_WhenMissing_ShouldThrowDependencyException()
    {
        // Act
        Action act = () => _services.EnsureVKBlockRegistered<TestMarker, TestDependentMarker>();

        // Assert
        act.Should().Throw<VKDependencyException>()
            .WithMessage("*VKBlock 'TestDependent' requires 'Test' to be registered first*");
    }

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

    private sealed class TestDependentMarker : IVKBlockMarker, IVKBlockMarkerProvider<TestDependentMarker>
    {
        public static IVKBlockMarker Instance { get; } = new TestDependentMarker();
        public string Name => "TestDependent";
        public string Identifier => "TestDependent";
        public string Version => "1.0.0";
        public IReadOnlyList<IVKBlockMarker> Dependencies => [TestMarker.Instance];
        public string ActivitySourceName => "TestDependent";
        public string MeterName => "TestDependent";
    }
}
