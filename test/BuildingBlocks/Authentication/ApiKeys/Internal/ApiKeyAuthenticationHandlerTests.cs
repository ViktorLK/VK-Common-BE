using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Authentication.ApiKeys.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.UnitTests.ApiKeys.Internal;

public sealed class ApiKeyAuthenticationHandlerTests
{
    private readonly Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>> _optionsMonitorMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<ApiKeyAuthenticationHandler>> _loggerMock;
    private readonly Mock<UrlEncoder> _urlEncoderMock;
    private readonly Mock<IVKApiKeyStore> _storeMock;
    private readonly Mock<IVKApiKeyRevocationProvider> _revocationProviderMock;
    private readonly Mock<IVKApiKeyRateLimiter> _rateLimiterMock;
    private readonly Mock<IOptions<VKApiKeyOptions>> _VKApiKeyOptionsMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly ApiKeyValidator _validator;
    private readonly ApiKeyAuthenticationOptions _handlerOptions;
    private readonly Fixture _fixture;
    private static readonly string[] expected = new[] { "read", "write" };

    public ApiKeyAuthenticationHandlerTests()
    {
        _optionsMonitorMock = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerMock = new Mock<ILogger<ApiKeyAuthenticationHandler>>();
        _urlEncoderMock = new Mock<UrlEncoder>();
        _storeMock = new Mock<IVKApiKeyStore>();
        _revocationProviderMock = new Mock<IVKApiKeyRevocationProvider>();
        _rateLimiterMock = new Mock<IVKApiKeyRateLimiter>();
        _VKApiKeyOptionsMock = new Mock<IOptions<VKApiKeyOptions>>();
        _timeProviderMock = new Mock<TimeProvider>();
        _fixture = new Fixture();

        _handlerOptions = new ApiKeyAuthenticationOptions
        {
            HeaderName = "X-API-KEY",
            AuthType = "ApiKey"
        };
        _optionsMonitorMock.Setup(x => x.Get(It.IsAny<string>())).Returns(_handlerOptions);

        var VKApiKeyOptions = new VKApiKeyOptions { MinLength = 32, Enabled = true };
        _VKApiKeyOptionsMock.Setup(x => x.Value).Returns(VKApiKeyOptions);

        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        var validatorLogger = new Mock<ILogger<ApiKeyValidator>>();
        _validator = new ApiKeyValidator(
            _storeMock.Object,
            _revocationProviderMock.Object,
            _rateLimiterMock.Object,
            _VKApiKeyOptionsMock.Object,
            _timeProviderMock.Object,
            validatorLogger.Object);

        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
    }

    private async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(HttpContext context)
    {
        var handler = new ApiKeyAuthenticationHandler(
            _optionsMonitorMock.Object,
            _loggerFactoryMock.Object,
            _urlEncoderMock.Object,
            _validator);

        var scheme = new AuthenticationScheme("ApiKey", "ApiKey", typeof(ApiKeyAuthenticationHandler));
        await handler.InitializeAsync(scheme, context);
        return handler;
    }

    [Fact]
    public async Task HandleAuthenticateAsync_NoHeader_ReturnsNoResult()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var handler = await CreateHandlerAsync(context);

        // Act
        // Accessing the protected method via a trick or just using AuthenticateAsync
        var result = await handler.AuthenticateAsync();

        // Assert
        result.None.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_InvalidApiKey_ReturnsFailure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-KEY"] = "too-short";
        var handler = await CreateHandlerAsync(context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ValidApiKey_ReturnsSuccess()
    {
        // Arrange
        var rawKey = new string('a', 32);
        var record = _fixture.Build<VKApiKeyRecord>()
            .With(x => x.IsEnabled, true)
            .With(x => x.ExpiresAt, (DateTimeOffset?)null)
            .With(x => x.Scopes, new List<string> { "read", "write" })
            .Create();

        _storeMock.Setup(x => x.FindByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(record));
        _revocationProviderMock.Setup(x => x.IsRevokedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _rateLimiterMock.Setup(x => x.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-KEY"] = rawKey;
        var handler = await CreateHandlerAsync(context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Principal.Should().NotBeNull();
        result.Principal!.HasClaim(VKClaimConstants.UserId, record.OwnerId).Should().BeTrue();
        result.Principal.HasClaim(VKClaimConstants.KeyId, record.Id.ToString()).Should().BeTrue();
        result.Principal.FindAll(VKClaimConstants.Scope).Select(c => c.Value).Should().Contain(expected);
    }

    [Fact]
    public async Task HandleChallengeAsync_WritesStandardizedResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var handler = await CreateHandlerAsync(context);

        // Act
        await handler.ChallengeAsync(new AuthenticationProperties());

        // Assert
        context.Response.StatusCode.Should().Be(401);
        responseStream.Position = 0;
        using var reader = new StreamReader(responseStream);
        var body = await reader.ReadToEndAsync();
        body.Should().Contain("API key is missing or invalid");
    }
}
