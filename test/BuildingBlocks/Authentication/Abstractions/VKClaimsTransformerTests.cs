using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Core;
namespace VK.Blocks.Authentication.UnitTests.Abstractions;

public sealed class VKClaimsTransformerTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<ClaimsTransformer>> _loggerMock;
    private readonly Mock<IVKClaimsProvider> _claimsProviderMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly ClaimsTransformer _transformer;

    public VKClaimsTransformerTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<ClaimsTransformer>>();
        _claimsProviderMock = new Mock<IVKClaimsProvider>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _transformer = new ClaimsTransformer(
            _scopeFactoryMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task TransformAsync_ShouldReturnPrincipalAsIs_WhenIdentityIsUnauthenticated()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Unauthenticated
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Never);
    }

    [Fact]
    public async Task TransformAsync_ShouldReturnPrincipalAsIs_WhenAlreadyTransformed()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(VKClaimConstants.ClaimsTransformed, "true"));
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Never);
    }

    [Fact]
    public async Task TransformAsync_ShouldReturnPrincipalAsIs_WhenUserIdIsMissing()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        // No Sub or UserId claim
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
    }

    [Fact]
    public async Task TransformAsync_ShouldReturnPrincipalAsIs_WhenProviderIsNotRegistered()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "user-123"));
        var principal = new ClaimsPrincipal(identity);

        _serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IVKClaimsProvider>))).Returns(Enumerable.Empty<IVKClaimsProvider>());

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public async Task TransformAsync_ShouldEnrichPrincipal_WhenProviderReturnsClaims()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "user-123"));
        var principal = new ClaimsPrincipal(identity);

        var extraClaims = new List<Claim> { new Claim("Permission", "Read") };
        _serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IVKClaimsProvider>)))
            .Returns(new[] { _claimsProviderMock.Object });
        _claimsProviderMock.Setup(x => x.GetUserClaimsAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(extraClaims);

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert
        result.Should().NotBeSameAs(principal);
        result.HasClaim("Permission", "Read").Should().BeTrue();
        result.HasClaim(VKClaimConstants.ClaimsTransformed, "true").Should().BeTrue();
    }

    [Fact]
    public async Task TransformAsync_ShouldPropagateCancellationToken_FromHttpContext()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "user-123"));
        var principal = new ClaimsPrincipal(identity);

        var cts = new CancellationTokenSource();
        var context = new DefaultHttpContext { RequestAborted = cts.Token };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IVKClaimsProvider>)))
            .Returns(new[] { _claimsProviderMock.Object });

        // Act
        await _transformer.TransformAsync(principal);

        // Assert
        _claimsProviderMock.Verify(x => x.GetUserClaimsAsync("user-123", cts.Token), Times.Once);
    }

    [Fact]
    public async Task TransformAsync_ShouldLogAndReturnOriginalPrincipal_WhenProviderThrows()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "user-123"));
        var principal = new ClaimsPrincipal(identity);

        _serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IVKClaimsProvider>)))
            .Returns(new[] { _claimsProviderMock.Object });
        _claimsProviderMock.Setup(x => x.GetUserClaimsAsync("user-123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB VKError"));

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);

        // Verify logger was called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
