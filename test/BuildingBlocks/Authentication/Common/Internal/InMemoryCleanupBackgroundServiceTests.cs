using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Authentication.Common.Internal;

namespace VK.Blocks.Authentication.UnitTests.Common.Internal;

public sealed class InMemoryCleanupBackgroundServiceTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IOptions<VKAuthenticationOptions>> _optionsMock;
    private readonly Mock<ILogger<InMemoryCleanupBackgroundService>> _loggerMock;
    private readonly List<IInMemoryCacheCleanup> _cleanupProviders;

    public InMemoryCleanupBackgroundServiceTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _optionsMock = new Mock<IOptions<VKAuthenticationOptions>>();
        _loggerMock = new Mock<ILogger<InMemoryCleanupBackgroundService>>();
        _cleanupProviders = new List<IInMemoryCacheCleanup>();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_scopeFactoryMock.Object);
        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(() =>
        {
            var mock = new Mock<IServiceScope>();
            mock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            return mock.Object;
        });

        _optionsMock.Setup(x => x.Value).Returns(new VKAuthenticationOptions
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

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100); // Give it a moment to run ExecuteAsync
        await service.StopAsync(CancellationToken.None);

        service.Dispose();

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
    public async Task ExecuteAsync_ShouldPerformCleanupLoop_WhenIntervalElapses()
    {
        // Arrange
        var providerMock = new Mock<IInMemoryCacheCleanup>();
        providerMock.Setup(x => x.AssociatedServiceType).Returns(typeof(string));
        _cleanupProviders.Add(providerMock.Object);

        _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns(providerMock.Object);

        // Setting interval to 0 to trigger immediately in tests
        _optionsMock.Setup(x => x.Value).Returns(new VKAuthenticationOptions
        {
            InMemoryCleanupIntervalMinutes = 0
        });

        var service = new InMemoryCleanupBackgroundService(
            _serviceProviderMock.Object,
            _cleanupProviders,
            _optionsMock.Object,
            _loggerMock.Object);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

        // Wait long enough for at least one loop iteration (Delay(0) is fast)
        await Task.Delay(50);

        await service.StopAsync(CancellationToken.None);
        cts.Cancel();

        service.Dispose();

        // Assert
        providerMock.Verify(x => x.CleanupExpiredEntries(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldContinue_WhenProviderThrows()
    {
        // Arrange
        var provider1Mock = new Mock<IInMemoryCacheCleanup>();
        provider1Mock.Setup(x => x.AssociatedServiceType).Returns(typeof(string));
        provider1Mock.Setup(x => x.CleanupExpiredEntries()).Throws(new Exception("Cleanup failed"));

        var provider2Mock = new Mock<IInMemoryCacheCleanup>();
        provider2Mock.Setup(x => x.AssociatedServiceType).Returns(typeof(int));

        _cleanupProviders.Add(provider1Mock.Object);
        _cleanupProviders.Add(provider2Mock.Object);

        _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns(provider1Mock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(int))).Returns(provider2Mock.Object);

        _optionsMock.Setup(x => x.Value).Returns(new VKAuthenticationOptions
        {
            InMemoryCleanupIntervalMinutes = 0
        });

        var service = new InMemoryCleanupBackgroundService(
            _serviceProviderMock.Object,
            _cleanupProviders,
            _optionsMock.Object,
            _loggerMock.Object);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(50);
        await service.StopAsync(CancellationToken.None);
        cts.Cancel();

        service.Dispose();

        // Assert
        provider1Mock.Verify(x => x.CleanupExpiredEntries(), Times.AtLeastOnce);
        provider2Mock.Verify(x => x.CleanupExpiredEntries(), Times.AtLeastOnce); // Should still call provider2

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipProvider_InLoop_WhenNotActive()
    {
        // Arrange
        var providerMock = new Mock<IInMemoryCacheCleanup>();
        providerMock.Setup(x => x.AssociatedServiceType).Returns(typeof(string));
        _cleanupProviders.Add(providerMock.Object);

        // Initial scan: active
        // Loop: inactive (different object)
        _serviceProviderMock.SetupSequence(x => x.GetService(typeof(string)))
            .Returns(providerMock.Object) // Used for initial scan
            .Returns(new object());       // Used inside while loop

        _optionsMock.Setup(x => x.Value).Returns(new VKAuthenticationOptions
        {
            InMemoryCleanupIntervalMinutes = 0
        });

        var service = new InMemoryCleanupBackgroundService(
            _serviceProviderMock.Object,
            _cleanupProviders,
            _optionsMock.Object,
            _loggerMock.Object);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(50);
        await service.StopAsync(CancellationToken.None);
        cts.Cancel();

        service.Dispose();

        // Assert
        // Should NOT have called cleanup in the loop
        providerMock.Verify(x => x.CleanupExpiredEntries(), Times.Never);
    }
}
