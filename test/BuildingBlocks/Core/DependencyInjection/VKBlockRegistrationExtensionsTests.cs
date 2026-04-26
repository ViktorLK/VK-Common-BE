using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace VK.Blocks.Core.UnitTests.DependencyInjection;

public sealed class VKBlockRegistrationExtensionsTests
{
    private readonly ServiceCollection _services = new ServiceCollection();

    [Fact]
    public void AddVKBlockOptions_FromConfiguration_ShouldBindAndRegister()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Test:Value"] = "Hello"
            })
            .Build();
        var section = config.GetSection("Test");

        // Act
        var options = _services.AddVKBlockOptions<TestOptions>(section);

        // Assert
        options.Value.Should().Be("Hello");

        var provider = _services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<TestOptions>>().Value.Value.Should().Be("Hello");
        provider.GetRequiredService<TestOptions>().Value.Should().Be("Hello");
    }

    [Fact]
    public void AddVKBlockOptions_FromFunc_ShouldConfigureAndRegister()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        var options = _services.AddVKBlockOptions<TestOptions>(config, o => o with { Value = "Manual" });

        // Assert
        options.Value.Should().Be("Manual");

        var provider = _services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<TestOptions>>().Value.Value.Should().Be("Manual");
        provider.GetRequiredService<TestOptions>().Value.Should().Be("Manual");
    }

    [Fact]
    public void AddVKBlockOptions_IsIdempotent()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        _services.AddVKBlockOptions<TestOptions>(config);
        var countBefore = _services.Count;
        _services.AddVKBlockOptions<TestOptions>(config);

        // Assert
        _services.Count.Should().Be(countBefore);
    }

    [Fact]
    public void WithScoped_ShouldReplaceRegistration()
    {
        // Arrange
        _services.AddScoped<ITestService, DefaultService>();
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.WithScoped<TestMarker, ITestService, CustomService>();

        // Assert
        var descriptor = _services.Single(d => d.ServiceType == typeof(ITestService));
        descriptor.ImplementationType.Should().Be(typeof(CustomService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void WithSingleton_ShouldReplaceRegistration()
    {
        // Arrange
        _services.AddSingleton<ITestService, DefaultService>();
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.WithSingleton<TestMarker, ITestService, CustomService>();

        // Assert
        var descriptor = _services.Single(d => d.ServiceType == typeof(ITestService));
        descriptor.ImplementationType.Should().Be(typeof(CustomService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void WithTransient_ShouldReplaceRegistration()
    {
        // Arrange
        _services.AddTransient<ITestService, DefaultService>();
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.WithTransient<TestMarker, ITestService, CustomService>();

        // Assert
        var descriptor = _services.Single(d => d.ServiceType == typeof(ITestService));
        descriptor.ImplementationType.Should().Be(typeof(CustomService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void TryAddEnumerableScoped_ShouldAddOnlyOnce()
    {
        // Act
        _services.TryAddEnumerableScoped<ITestService, CustomService>();
        _services.TryAddEnumerableScoped<ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
    }

    [Fact]
    public void Builder_TryAddEnumerableScoped_ShouldAddOnlyOnce()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.TryAddEnumerableScoped<TestMarker, ITestService, CustomService>();
        builder.TryAddEnumerableScoped<TestMarker, ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
    }

    [Fact]
    public void TryAddEnumerableSingleton_ShouldAddOnlyOnce()
    {
        // Act
        _services.TryAddEnumerableSingleton<ITestService, CustomService>();
        _services.TryAddEnumerableSingleton<ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
        _services.First(d => d.ServiceType == typeof(ITestService)).Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void TryAddEnumerableTransient_ShouldAddOnlyOnce()
    {
        // Act
        _services.TryAddEnumerableTransient<ITestService, CustomService>();
        _services.TryAddEnumerableTransient<ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
        _services.First(d => d.ServiceType == typeof(ITestService)).Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void Builder_TryAddEnumerableSingleton_ShouldAddOnlyOnce()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.TryAddEnumerableSingleton<TestMarker, ITestService, CustomService>();
        builder.TryAddEnumerableSingleton<TestMarker, ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
    }

    [Fact]
    public void Builder_TryAddEnumerableTransient_ShouldAddOnlyOnce()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.TryAddEnumerableTransient<TestMarker, ITestService, CustomService>();
        builder.TryAddEnumerableTransient<TestMarker, ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
    }

    private sealed record TestOptions : IVKBlockOptions
    {
        public static string SectionName => "Test";
        public string Value { get; init; } = "";
    }

    private interface ITestService;
    private sealed class DefaultService : ITestService;
    private sealed class CustomService : ITestService;
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

    [Fact]
    public void EnsureCoreBlockRegistered_WhenMissing_ShouldThrow()
    {
        // Act
        Action act = () => _services.EnsureCoreBlockRegistered<TestMarker>();

        // Assert
        act.Should().Throw<VKDependencyException>()
            .WithMessage("*requires 'VK.Blocks.Core' to be registered first*");
    }

    [Fact]
    public void EnsureCoreBlockRegistered_WhenPresent_ShouldNotThrow()
    {
        // Arrange
        _services.AddVKCoreBlock(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());

        // Act
        Action act = () => _services.EnsureCoreBlockRegistered<TestMarker>();

        // Assert
        act.Should().NotThrow();
    }
}
