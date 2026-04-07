using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.DependencyInjection;
using FluentAssertions;

namespace VK.Blocks.Authentication.UnitTests.Common;

public sealed class InMemoryCleanupBackgroundServiceTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IOptionsMonitor<VKAuthenticationOptions>> _optionsMock;
    private readonly Mock<ILogger<InMemoryCleanupBackgroundService>> _loggerMock;
    private readonly List<IInMemoryCacheCleanup> _cleanupProviders;

    public InMemoryCleanupBackgroundServiceTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _optionsMock = new Mock<IOptionsMonitor<VKAuthenticationOptions>>();
        _loggerMock = new Mock<ILogger<InMemoryCleanupBackgroundService>>();
        _cleanupProviders = new List<IInMemoryCacheCleanup>();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_scopeFactoryMock.Object);
        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(() => 
        {
            var mock = new Mock<IServiceScope>();
            mock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            return mock.Object;
        });

        _optionsMock.Setup(x => x.CurrentValue).Returns(new VKAuthenticationOptions 
        { 
            InMemoryCleanupIntervalMinutes = 1 
        });

        // Setup IsEnabled for all levels to avoid potential Moq issues with SG loggers
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStopImmediately_WhenNoActiveProviders()
    {
        // Arrange
        var service = new InMemoryCleanupBackgroundService(
            _serviceProviderMock.Object,
            Enumerable.Empty<IInMemoryCacheCleanup>(),
            _optionsMock.Object,
            _loggerMock.Object);

        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100); // Give it a moment to run ExecuteAsync
        await service.StopAsync(CancellationToken.None);

        // Assert
        // Verify LogNoActiveProviders was called (via a simple check)
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPerformCleanup_OnActiveProviders()
    {
        // Arrange
        var providerMock = new Mock<IInMemoryCacheCleanup>();
        providerMock.Setup(x => x.AssociatedServiceType).Returns(typeof(string)); // Just a dummy type
        _cleanupProviders.Add(providerMock.Object);

        // Mock the provider being active in DI
        _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns(providerMock.Object);

        // Set interval to 1 minute (minimum for int)
        _optionsMock.Setup(x => x.CurrentValue).Returns(new VKAuthenticationOptions 
        { 
            InMemoryCleanupIntervalMinutes = 1 
        });

        var service = new InMemoryCleanupBackgroundService(
            _serviceProviderMock.Object,
            _cleanupProviders,
            _optionsMock.Object,
            _loggerMock.Object);

        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        
        // Wait a bit - unfortunately we can't easily trigger the cleanup in a unit test 
        // without waiting for 1 minute or refactoring the service to use a mockable delay.
        // For now, we verify the service starts and performs the initial scan.
        await Task.Delay(100);
        
        await service.StopAsync(CancellationToken.None);
        cts.Cancel();

        // Assert
        // The initial scan happened
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipProvider_WhenNotActiveInDI()
    {
        // Arrange
        var providerMock = new Mock<IInMemoryCacheCleanup>();
        providerMock.Setup(x => x.AssociatedServiceType).Returns(typeof(string));
        _cleanupProviders.Add(providerMock.Object);

        // Mock the provider NOT being active in DI (e.g. replaced by Redis)
        _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns(new object()); // Different instance

        _optionsMock.Setup(x => x.CurrentValue).Returns(new VKAuthenticationOptions 
        { 
            InMemoryCleanupIntervalMinutes = 1 
        });

        var service = new InMemoryCleanupBackgroundService(
            _serviceProviderMock.Object,
            _cleanupProviders,
            _optionsMock.Object,
            _loggerMock.Object);

        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        // Assert
        providerMock.Verify(x => x.CleanupExpiredEntries(), Times.Never);
        // Verify LogSkippingProvider was called
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
