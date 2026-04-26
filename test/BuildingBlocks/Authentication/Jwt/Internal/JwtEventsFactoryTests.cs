using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Moq;
using VK.Blocks.Authentication.Jwt.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.UnitTests.Jwt.Internal;

public sealed class JwtEventsFactoryTests
{
    private readonly Mock<IVKJwtAuthService> _authServiceMock;
    private readonly DefaultHttpContext _httpContext;

    public JwtEventsFactoryTests()
    {
        _authServiceMock = new Mock<IVKJwtAuthService>();
        _httpContext = new DefaultHttpContext();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IVKJwtAuthService)))
            .Returns(_authServiceMock.Object);

        _httpContext.Response.Body = new MemoryStream();
        _httpContext.RequestServices = serviceProviderMock.Object;
    }

    [Fact]
    public async Task OnTokenValidated_WhenRevoked_ShouldCallFail()
    {
        // Arrange
        var events = JwtEventsFactory.CreateEvents();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
        var context = new TokenValidatedContext(_httpContext, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), new JwtBearerOptions())
        {
            Principal = principal,
            Properties = new AuthenticationProperties()
        };

        _authServiceMock.Setup(x => x.ValidateRevocationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure(VKJwtErrors.Revoked));

        // Act
        await events.OnTokenValidated(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result!.Failure.Should().NotBeNull();
    }

    [Fact]
    public async Task OnTokenValidated_WhenNotRevoked_ShouldNotCallFail()
    {
        // Arrange
        var events = JwtEventsFactory.CreateEvents();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
        var context = new TokenValidatedContext(_httpContext, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), new JwtBearerOptions())
        {
            Principal = principal,
            Properties = new AuthenticationProperties()
        };

        _authServiceMock.Setup(x => x.ValidateRevocationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        // Act
        await events.OnTokenValidated(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnAuthenticationFailed_WhenExpired_ShouldAddHeader()
    {
        // Arrange
        var events = JwtEventsFactory.CreateEvents();
        var context = new AuthenticationFailedContext(_httpContext, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), new JwtBearerOptions())
        {
            Exception = new SecurityTokenExpiredException("Expired")
        };

        // Act
        await events.OnAuthenticationFailed(context);

        // Assert
        _httpContext.Response.Headers.ContainsKey(JwtConstants.TokenExpiredHeader).Should().BeTrue();
        _httpContext.Response.Headers[JwtConstants.TokenExpiredHeader].Should().BeEquivalentTo([JwtConstants.HeaderTrueValue]);
    }

    [Fact]
    public async Task OnChallenge_ShouldHandleResponseAndWriteProblemDetails()
    {
        // Arrange
        var events = JwtEventsFactory.CreateEvents();
        var context = new JwtBearerChallengeContext(_httpContext, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), new JwtBearerOptions(), new AuthenticationProperties());

        // Act
        await events.OnChallenge(context);

        // Assert
        context.Handled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        // Standard problem details content type or fallback to application/json
        _httpContext.Response.ContentType.Should().NotBeNull("ContentType should be set by the helper");
        _httpContext.Response.ContentType.Should().MatchRegex("application/.*json", $"Actual ContentType was: {_httpContext.Response.ContentType}");
    }
}
