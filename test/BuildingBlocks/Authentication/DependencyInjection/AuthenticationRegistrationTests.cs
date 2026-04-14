using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Core.DependencyInjection;
using VK.Blocks.Core.Results;
using VK.Blocks.Authentication.Features.ApiKeys;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Features.Jwt.Persistence;
using VK.Blocks.Authentication.Features.ApiKeys.Persistence;
using Microsoft.AspNetCore.Authorization;
using FluentAssertions;

namespace VK.Blocks.Authentication.UnitTests.DependencyInjection;

public sealed class AuthenticationRegistrationTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public AuthenticationRegistrationTests()
    {
        _services = new ServiceCollection();
        _services.AddLogging();

        var configData = new Dictionary<string, string?>
        {
            ["Authentication:Enabled"] = "true",
            ["Authentication:DefaultScheme"] = "Bearer",
            ["Authentication:Jwt:Enabled"] = "true",
            ["Authentication:Jwt:SecretKey"] = "SuperSecretKeyThatIsLongEnoughToPassValidation",
            ["Authentication:Jwt:Issuer"] = "TestIssuer",
            ["Authentication:Jwt:Audience"] = "TestAudience"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public void AddVKAuthenticationBlock_WhenCoreNotRegistered_ShouldThrow()
    {
        // Act
        var action = () => _services.AddVKAuthenticationBlock(_configuration);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .And.Message.Should().Contain("VK.Blocks.Core");
    }

    [Fact]
    public void AddVKAuthenticationBlock_WhenDisabled_ShouldNotRegisterServices()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Enabled"] = "false"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(config);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<AuthenticatedUser>().Should().BeNull();
    }

    [Fact]
    public void AddVKAuthenticationBlock_WhenMissingSection_ShouldNotRegisterServices()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var config = new ConfigurationBuilder().Build();

        // Act
        _services.AddVKAuthenticationBlock(config);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<AuthenticatedUser>().Should().BeNull();
    }

    [Fact]
    public void AddVKAuthenticationBlock_WhenRegistered_ShouldProvideServices()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetService<IClaimsTransformation>().Should().NotBeNull().And.BeOfType<VKClaimsTransformer>();
        serviceProvider.GetService<IOptions<VKAuthenticationOptions>>().Should().NotBeNull();
        serviceProvider.GetServices<IValidateOptions<VKAuthenticationOptions>>().Should().Contain(v => v is VKAuthenticationOptionsValidator);
    }

    [Fact]
    public void AddVKAuthenticationBlock_IsIdempotent()
    {
        // Arrange
        _services.RegisterCoreBlock();
        _services.AddVKAuthenticationBlock(_configuration);
        var countBefore = _services.Count;

        // Act
        _services.AddVKAuthenticationBlock(_configuration);

        // Assert
        _services.Count.Should().Be(countBefore);
    }

    [Fact]
    public void AddVKAuthenticationBlock_WhenDisabled_ShouldOnlyRegisterOptions()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var disabledConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Enabled"] = "false"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(disabledConfig);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<VKAuthenticationOptions>>().Value;

        options.Enabled.Should().BeFalse();
        serviceProvider.GetService<IClaimsTransformation>().Should().BeNull();
    }

    [Fact]
    public void AddVKAuthenticationBlock_WhenOnlyJwtEnabled_RegistersUserAndServicePolicies()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var jwtOnlyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Enabled"] = "true",
                ["Authentication:DefaultScheme"] = "Bearer",
                ["Authentication:Jwt:Enabled"] = "true",
                ["Authentication:Jwt:Issuer"] = "test-issuer",
                ["Authentication:Jwt:Audience"] = "test-audience",
                ["Authentication:Jwt:SecretKey"] = "SuperSecretKeyThatIsLongEnoughToPassValidation",
                ["Authentication:ApiKey:Enabled"] = "false"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(jwtOnlyConfig);
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        // Assert
        options.GetPolicy(AuthenticationConstants.JwtPolicy).Should().NotBeNull();
        options.GetPolicy(AuthenticationConstants.ApiKeyPolicy).Should().BeNull();

        // Groups
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.User}").Should().NotBeNull();
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Service}").Should().NotBeNull();
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Internal}").Should().BeNull();
    }

    /// <summary>
    /// Updates the failing tests to include required configuration fields (DefaultScheme, etc) and ensure validators pass.
    /// </summary>
    [Fact]
    public void AddVKAuthenticationBlock_WhenOnlyApiKeyEnabled_RegistersServiceAndInternalPolicies()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var apiKeyOnlyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Enabled"] = "true",
                ["Authentication:DefaultScheme"] = "ApiKey",
                ["Authentication:Jwt:Enabled"] = "false",
                ["Authentication:ApiKey:Enabled"] = "true"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(apiKeyOnlyConfig);
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        // Assert
        options.GetPolicy(AuthenticationConstants.ApiKeyPolicy).Should().NotBeNull();
        options.GetPolicy(AuthenticationConstants.JwtPolicy).Should().BeNull();

        // Groups
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.User}").Should().BeNull();
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Service}").Should().NotBeNull();
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Internal}").Should().NotBeNull();
    }

    [Fact]
    public void AddVKAuthenticationBlock_WhenOAuthEnabled_RegistersUserGroupPolicy()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var oauthConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Enabled"] = "true",
                ["Authentication:DefaultScheme"] = "Bearer",
                ["Authentication:OAuth:Enabled"] = "true",
                ["Authentication:OAuth:Providers:GitHub:Enabled"] = "true",
                ["Authentication:OAuth:Providers:GitHub:ClientId"] = "test-id",
                ["Authentication:OAuth:Providers:GitHub:ClientSecret"] = "test-secret",
                ["Authentication:OAuth:Providers:GitHub:Authority"] = "https://github.com",
                ["Authentication:OAuth:Providers:GitHub:CallbackPath"] = "/cb"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(oauthConfig);
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        // Assert
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.User}").Should().NotBeNull();
    }

    [Fact]
    public void BuilderExtensions_WithCustomImplementations_ShouldOverrideDefaults()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration)
            .WithJwtRefreshTokenValidator<TestJwtRefreshTokenValidator>()
            .WithJwtTokenRevocationProvider<TestJwtTokenRevocationProvider>()
            .WithApiKeyRevocationProvider<TestApiKeyRevocationProvider>()
            .WithApiKeyRateLimiter<TestApiKeyRateLimiter>();

        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IJwtRefreshTokenValidator>().Should().BeOfType<TestJwtRefreshTokenValidator>();
        serviceProvider.GetService<IJwtTokenRevocationProvider>().Should().BeOfType<TestJwtTokenRevocationProvider>();
        serviceProvider.GetService<IApiKeyRevocationProvider>().Should().BeOfType<TestApiKeyRevocationProvider>();
        serviceProvider.GetService<IApiKeyRateLimiter>().Should().BeOfType<TestApiKeyRateLimiter>();
    }

    [Fact]
    public void BuilderExtensions_TryAddOAuthMapper_ShouldRegisterKeyed()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration)
            .TryAddOAuthMapper<TestOAuthMapper>("TestProvider");

        var serviceProvider = _services.BuildServiceProvider();
        var mapper = serviceProvider.GetKeyedService<IOAuthClaimsMapper>("TestProvider");

        // Assert
        mapper.Should().NotBeNull().And.BeOfType<TestOAuthMapper>();
    }

    [Fact]
    public void AddVKAuthenticationBlock_WithExplicitSchemeNames_ShouldUseThem()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Enabled"] = "true",
                ["Authentication:OAuth:Enabled"] = "true",
                ["Authentication:OAuth:Providers:MyProvider:Enabled"] = "true",
                ["Authentication:OAuth:Providers:MyProvider:SchemeName"] = "CustomScheme",
                ["Authentication:OAuth:Providers:MyProvider:ClientId"] = "id",
                ["Authentication:OAuth:Providers:MyProvider:ClientSecret"] = "secret",
                ["Authentication:OAuth:Providers:MyProvider:Authority"] = "auth",
                ["Authentication:OAuth:Providers:MyProvider:CallbackPath"] = "/cb"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(config);
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var policy = options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.User}");

        // Assert
        policy.Should().NotBeNull();
        policy!.AuthenticationSchemes.Should().Contain("CustomScheme");
    }

    [Fact]
    public void AddVKAuthenticationBlock_WhenMultipleFeaturesEnabled_RegistersCombinedPolicies()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var combinedConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Enabled"] = "true",
                ["Authentication:DefaultScheme"] = "Bearer",
                ["Authentication:Jwt:Enabled"] = "true",
                ["Authentication:Jwt:Issuer"] = "test-issuer",
                ["Authentication:Jwt:Audience"] = "test-audience",
                ["Authentication:Jwt:SecretKey"] = "SuperSecretKeyThatIsLongEnoughToPassValidation",
                ["Authentication:ApiKey:Enabled"] = "true",
                ["Authentication:OAuth:Enabled"] = "true",
                ["Authentication:OAuth:Providers:GitHub:Enabled"] = "true",
                ["Authentication:OAuth:Providers:GitHub:ClientId"] = "test-id",
                ["Authentication:OAuth:Providers:GitHub:ClientSecret"] = "test-secret",
                ["Authentication:OAuth:Providers:GitHub:Authority"] = "https://github.com",
                ["Authentication:OAuth:Providers:GitHub:CallbackPath"] = "/cb"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(combinedConfig);
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        // Assert
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.User}").Should().NotBeNull();
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Service}").Should().NotBeNull();
        options.GetPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Internal}").Should().NotBeNull();
    }

    [Fact]
    public void AddVKAuthenticationBlock_WithOAuthProviders_ShouldRegisterIndividualPolicies()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Enabled"] = "true",
                ["Authentication:OAuth:Enabled"] = "true",
                ["Authentication:OAuth:Providers:GitHub:Enabled"] = "true",
                ["Authentication:OAuth:Providers:GitHub:ClientId"] = "id",
                ["Authentication:OAuth:Providers:GitHub:ClientSecret"] = "secret",
                ["Authentication:OAuth:Providers:GitHub:Authority"] = "auth",
                ["Authentication:OAuth:Providers:GitHub:CallbackPath"] = "/cb"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(config);
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        // Assert
        options.GetPolicy("VK.Group.GitHub").Should().NotBeNull();
    }

    [Fact]
    public void BuilderExtensions_WithJwtTokenRevocationProvider_ShouldRegisterAsSingleton()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration)
            .WithJwtTokenRevocationProvider<TestJwtTokenRevocationProvider>();

        var serviceProvider = _services.BuildServiceProvider();
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IJwtTokenRevocationProvider));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationType.Should().Be(typeof(TestJwtTokenRevocationProvider));
    }

    [Fact]
    public void BuilderExtensions_TryAddClaimsProvider_CheckIdempotency()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var builder = _services.AddVKAuthenticationBlock(_configuration);

        // Act
        builder.TryAddClaimsProvider<TestClaimsProvider1>();
        var countMid = _services.Count;
        builder.TryAddClaimsProvider<TestClaimsProvider1>(); // Should be ignored

        // Assert
        _services.Count.Should().Be(countMid);
    }
}

internal static class CoreTestExtensions
{
    public static void RegisterCoreBlock(this IServiceCollection services)
    {
        // Minimal registration to satisfy IsVKBlockRegistered<CoreBlock>
        services.AddVKBlockMarker<CoreBlock>();
    }
}

// Test Dummies
internal sealed class TestJwtRefreshTokenValidator : IJwtRefreshTokenValidator
{
    public ValueTask<Result<bool>> ValidateTokenRotationAsync(string tokenJti, string familyId, CancellationToken ct = default)
        => ValueTask.FromResult(Result.Success(true));
}

internal sealed class TestApiKeyRateLimiter : IApiKeyRateLimiter
{
    public ValueTask<bool> IsAllowedAsync(Guid keyId, int limit, int windowSeconds, CancellationToken ct = default)
        => ValueTask.FromResult(true);
}

internal sealed class TestJwtTokenRevocationProvider : IJwtTokenRevocationProvider
{
    public ValueTask<bool> IsRevokedAsync(string jti, CancellationToken ct = default) => ValueTask.FromResult(false);
    public ValueTask RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default) => ValueTask.CompletedTask;
    public ValueTask<bool> IsUserRevokedAsync(string userId, CancellationToken ct = default) => ValueTask.FromResult(false);
    public ValueTask RevokeUserAsync(string userId, TimeSpan ttl, CancellationToken ct = default) => ValueTask.CompletedTask;
}

internal sealed class TestApiKeyRevocationProvider : IApiKeyRevocationProvider
{
    public ValueTask<bool> IsRevokedAsync(string keyId, CancellationToken ct = default) => ValueTask.FromResult(false);
    public ValueTask RevokeAsync(string keyId, TimeSpan ttl, CancellationToken ct = default) => ValueTask.CompletedTask;
}

internal sealed class TestOAuthMapper : IOAuthClaimsMapper
{
    public IEnumerable<System.Security.Claims.Claim> MapToClaims(ExternalIdentity userInfo) => Enumerable.Empty<System.Security.Claims.Claim>();
}

internal sealed class TestClaimsProvider1 : IVKClaimsProvider
{
    public ValueTask<IEnumerable<System.Security.Claims.Claim>> GetUserClaimsAsync(string userId, CancellationToken ct = default)
        => ValueTask.FromResult(Enumerable.Empty<System.Security.Claims.Claim>());
}

internal sealed class TestClaimsProvider2 : IVKClaimsProvider
{
    public ValueTask<IEnumerable<System.Security.Claims.Claim>> GetUserClaimsAsync(string userId, CancellationToken ct = default)
        => ValueTask.FromResult(Enumerable.Empty<System.Security.Claims.Claim>());
}

