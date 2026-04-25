using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core.UnitTests.DependencyInjection;

public class VKServiceCollectionExtensionsTests
{
    private interface ITestService;
    private sealed class TestService : ITestService;
    private sealed class OtherTestService : ITestService;

    [Fact]
    public void TryAddEnumerableScoped_ShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.TryAddEnumerableScoped<ITestService, TestService>();
        services.TryAddEnumerableScoped<ITestService, TestService>(); // Idempotent

        // Assert
        var descriptors = services.Where(d => d.ServiceType == typeof(ITestService)).ToList();
        descriptors.Should().ContainSingle();
        descriptors[0].Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptors[0].ImplementationType.Should().Be(typeof(TestService));
    }

    [Fact]
    public void TryAddEnumerableSingleton_ShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.TryAddEnumerableSingleton<ITestService, TestService>();

        // Assert
        services.Any(d => d.ServiceType == typeof(ITestService) && d.Lifetime == ServiceLifetime.Singleton).Should().BeTrue();
    }

    [Fact]
    public void TryAddEnumerableTransient_ShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.TryAddEnumerableTransient<ITestService, TestService>();

        // Assert
        services.Any(d => d.ServiceType == typeof(ITestService) && d.Lifetime == ServiceLifetime.Transient).Should().BeTrue();
    }

    [Fact]
    public void TryAddScopedForwarding_ShouldForwardToImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();

        // Act
        services.TryAddScopedForwarding<ITestService, TestService>();
        var sp = services.BuildServiceProvider();

        // Assert
        var service = sp.GetRequiredService<ITestService>();
        service.Should().BeOfType<TestService>();
        service.Should().BeSameAs(sp.GetRequiredService<TestService>());
    }

    [Fact]
    public void TryAddSingletonForwarding_ShouldForwardToImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TestService>();

        // Act
        services.TryAddSingletonForwarding<ITestService, TestService>();
        var sp = services.BuildServiceProvider();

        // Assert
        var service = sp.GetRequiredService<ITestService>();
        service.Should().BeSameAs(sp.GetRequiredService<TestService>());
    }

    [Fact]
    public void TryAddEnumerableScopedForwarding_ShouldForwardToImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();

        // Act
        services.TryAddEnumerableScopedForwarding<ITestService, TestService>();
        var sp = services.BuildServiceProvider();

        // Assert
        var service = sp.GetRequiredService<ITestService>();
        service.Should().BeSameAs(sp.GetRequiredService<TestService>());
    }

    [Fact]
    public void TryAddEnumerableSingletonForwarding_ShouldForwardToImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TestService>();

        // Act
        services.TryAddEnumerableSingletonForwarding<ITestService, TestService>();
        var sp = services.BuildServiceProvider();

        // Assert
        var service = sp.GetRequiredService<ITestService>();
        service.Should().BeSameAs(sp.GetRequiredService<TestService>());
    }
}
