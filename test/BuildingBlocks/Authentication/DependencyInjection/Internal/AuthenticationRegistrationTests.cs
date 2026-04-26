using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Authentication.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.UnitTests.DependencyInjection.Internal;

public sealed class AuthenticationRegistrationTests
{
    private readonly ServiceCollection _services;
    private readonly IConfiguration _configuration;

    public AuthenticationRegistrationTests()
    {
        _services = new ServiceCollection();
        _services.AddLogging();

        var configData = new Dictionary<string, string?>
        {
            ["VKBlocks:Authentication:Enabled"] = "true",
            ["VKBlocks:Authentication:DefaultScheme"] = "Bearer",
            ["VKBlocks:Authentication:Jwt:Enabled"] = "true",
            ["VKBlocks:Authentication:Jwt:SecretKey"] = "SuperSecretKeyThatIsLongEnoughToPassValidation",
            ["VKBlocks:Authentication:Jwt:Issuer"] = "TestIssuer",
            ["VKBlocks:Authentication:Jwt:Audience"] = "TestAudience"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public void AddAuthenticationBlock_WhenCoreNotRegistered_ShouldThrow()
    {
        // Act
        var action = () => _services.AddVKAuthenticationBlock(_configuration);

        // Assert
        action.Should().Throw<VKDependencyException>();
    }

    [Fact]
    public void AddAuthenticationBlock_WhenDisabled_ShouldNotRegisterServices()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "false"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(config);
        using var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IClaimsTransformation>().Should().BeNull();
    }

    [Fact]
    public void AddAuthenticationBlock_WhenMissingSection_ShouldNotRegisterServices()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var config = new ConfigurationBuilder().Build();

        // Act
        _services.AddVKAuthenticationBlock(config);
        using var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IClaimsTransformation>().Should().BeNull();
    }

    [Fact]
    public void AddAuthenticationBlock_WhenRegistered_ShouldProvideServices()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration);

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetService<IClaimsTransformation>().Should().NotBeNull().And.BeOfType<ClaimsTransformer>();
        serviceProvider.GetService<IOptions<VKAuthenticationOptions>>().Should().NotBeNull();
        serviceProvider.GetServices<IValidateOptions<VKAuthenticationOptions>>().Should().Contain(v => v is AuthenticationOptionsValidator);
    }

    [Fact]
    public void AddAuthenticationBlock_WithFunc_ShouldConfigureOptions()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration, options => options with
        {
            DefaultScheme = "Custom",
            InMemoryCleanupIntervalMinutes = 20
        })
        .AddVKJwt(j => j with { Enabled = true, Issuer = "ActionIssuer" })
        .AddVKApiKeys(a => a with { Enabled = true })
        .AddVKOAuth(o => o with { Enabled = true });

        using var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<VKAuthenticationOptions>>().Value;
        var jwtOptions = serviceProvider.GetRequiredService<IOptions<VKJwtOptions>>().Value;
        var apiKeyOptions = serviceProvider.GetRequiredService<IOptions<VKApiKeyOptions>>().Value;
        var oauthOptions = serviceProvider.GetRequiredService<IOptions<VKOAuthOptions>>().Value;

        // Assert
        options.DefaultScheme.Should().Be("Custom");
        options.InMemoryCleanupIntervalMinutes.Should().Be(20);
        jwtOptions.Enabled.Should().BeTrue();
        jwtOptions.Issuer.Should().Be("ActionIssuer");
        apiKeyOptions.Enabled.Should().BeTrue();
        oauthOptions.Enabled.Should().BeTrue();
    }

    [Fact]
    public void AddAuthenticationBlock_IsIdempotent()
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
    public void AddAuthenticationBlock_WhenDisabled_ShouldOnlyRegisterOptions()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var disabledConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "false"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(disabledConfig);

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<VKAuthenticationOptions>>().Value;

        options.Enabled.Should().BeFalse();
        serviceProvider.GetService<IClaimsTransformation>().Should().BeNull();

        // ASP.NET Core authentication services should NOT be registered
        _services.Any(d => d.ServiceType == typeof(IAuthenticationService)).Should().BeFalse();
    }

    [Fact]
    public void AddAuthenticationBlock_WhenDisabled_ChainedMethods_ShouldNotCrash()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var disabledConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "false",
                ["VKBlocks:Authentication:Jwt:Enabled"] = "true",
                ["VKBlocks:Authentication:ApiKey:Enabled"] = "true"
            })
            .Build();

        // Act
        var act = () => _services.AddVKAuthenticationBlock(disabledConfig)
                                .AddVKJwt()
                                .AddVKApiKeys()
                                .AddVKOAuth();

        // Assert
        act.Should().NotThrow();
        _services.IsVKBlockRegistered<VKAuthenticationBlock>().Should().BeTrue();
    }

    [Fact]
    public void AddVKDefaultFeatures_RegistersAllFeatures()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration)
                 .AddVKDefaultFeatures();

        // Assert
        _services.IsVKBlockRegistered<VK.Blocks.Authentication.Jwt.Internal.JwtFeature>().Should().BeTrue();
        _services.IsVKBlockRegistered<VK.Blocks.Authentication.ApiKeys.Internal.ApiKeyFeature>().Should().BeTrue();
        _services.IsVKBlockRegistered<VK.Blocks.Authentication.OAuth.Internal.OAuthFeature>().Should().BeTrue();
    }

    [Fact]
    public void AddVKJwt_IsIdempotent_ViaMarker()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var builder = _services.AddVKAuthenticationBlock(_configuration);
        builder.AddVKJwt();
        var countAfterFirst = _services.Count;

        // Act
        builder.AddVKJwt();

        // Assert
        _services.Count.Should().Be(countAfterFirst);
        _services.IsVKBlockRegistered<VK.Blocks.Authentication.Jwt.Internal.JwtFeature>().Should().BeTrue();
    }

    [Fact]
    public void AddAuthenticationBlock_WhenOnlyJwtEnabled_RegistersSemanticSchemeProviders()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var jwtOnlyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "true",
                ["VKBlocks:Authentication:DefaultScheme"] = "Bearer",
                ["VKBlocks:Authentication:Jwt:Enabled"] = "true",
                ["VKBlocks:Authentication:Jwt:Issuer"] = "test-issuer",
                ["VKBlocks:Authentication:Jwt:Audience"] = "test-audience",
                ["VKBlocks:Authentication:Jwt:SecretKey"] = "SuperSecretKeyThatIsLongEnoughToPassValidation",
                ["VKBlocks:Authentication:ApiKey:Enabled"] = "false"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(jwtOnlyConfig).AddVKJwt();
        using var serviceProvider = _services.BuildServiceProvider();

        var providers = serviceProvider.GetServices<IVKSemanticSchemeProvider>().ToList();

        // Assert
        providers.Should().NotBeEmpty();
        var jwtProvider = providers.Should().ContainSingle(p => p.GetType().Name.Contains("Jwt")).Subject;
        jwtProvider.GetUserSchemes().Should().Contain("Bearer");
        jwtProvider.GetServiceSchemes().Should().Contain("Bearer");
        jwtProvider.GetInternalSchemes().Should().BeEmpty();
    }

    [Fact]
    public void AddAuthenticationBlock_WhenOnlyApiKeyEnabled_RegistersSemanticSchemeProviders()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var apiKeyOnlyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "true",
                ["VKBlocks:Authentication:DefaultScheme"] = "ApiKey",
                ["VKBlocks:Authentication:Jwt:Enabled"] = "false",
                ["VKBlocks:Authentication:ApiKey:Enabled"] = "true"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(apiKeyOnlyConfig).AddVKApiKeys();
        using var serviceProvider = _services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IVKSemanticSchemeProvider>().ToList();

        // Assert
        providers.Should().NotBeEmpty();
        var apiProvider = providers.Should().ContainSingle(p => p.GetType().Name.Contains("ApiKey")).Subject;
        apiProvider.GetUserSchemes().Should().BeEmpty();
        apiProvider.GetServiceSchemes().Should().Contain("ApiKey");
        apiProvider.GetInternalSchemes().Should().Contain("ApiKey");
    }

    [Fact]
    public void AddAuthenticationBlock_WhenOAuthEnabled_RegistersSemanticSchemeProviders()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var oauthConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "true",
                ["VKBlocks:Authentication:DefaultScheme"] = "Bearer",
                ["VKBlocks:Authentication:OAuth:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:ClientId"] = "test-id",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:ClientSecret"] = "test-secret",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:Authority"] = "https://github.com",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:CallbackPath"] = "/cb"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(oauthConfig).AddVKOAuth();
        using var serviceProvider = _services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IVKSemanticSchemeProvider>().ToList();

        // Assert
        var oauthProvider = providers.Should().ContainSingle(p => p.GetType().Name.Contains("OAuth")).Subject;
        oauthProvider.GetUserSchemes().Should().Contain("GitHub");
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

        using var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IVKJwtRefreshValidator>().Should().BeOfType<TestJwtRefreshTokenValidator>();
        serviceProvider.GetService<IVKJwtRevocationProvider>().Should().BeOfType<TestJwtTokenRevocationProvider>();
        serviceProvider.GetService<IVKApiKeyRevocationProvider>().Should().BeOfType<TestApiKeyRevocationProvider>();
        serviceProvider.GetService<IVKApiKeyRateLimiter>().Should().BeOfType<TestApiKeyRateLimiter>();
    }

    [Fact]
    public void BuilderExtensions_TryAddOAuthMapper_ShouldRegisterKeyed()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration)
            .TryAddOAuthMapper<TestOAuthMapper>("TestProvider");

        using var serviceProvider = _services.BuildServiceProvider();
        var mapper = serviceProvider.GetKeyedService<IVKOAuthClaimsMapper>("TestProvider");

        // Assert
        mapper.Should().NotBeNull().And.BeOfType<TestOAuthMapper>();
    }

    [Fact]
    public void AddAuthenticationBlock_WithExplicitSchemeNames_ShouldUseThem()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Providers:MyProvider:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Providers:MyProvider:SchemeName"] = "CustomScheme",
                ["VKBlocks:Authentication:OAuth:Providers:MyProvider:ClientId"] = "id",
                ["VKBlocks:Authentication:OAuth:Providers:MyProvider:ClientSecret"] = "secret",
                ["VKBlocks:Authentication:OAuth:Providers:MyProvider:Authority"] = "auth",
                ["VKBlocks:Authentication:OAuth:Providers:MyProvider:CallbackPath"] = "/cb"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(config).AddVKOAuth();
        using var serviceProvider = _services.BuildServiceProvider();
        // Skip direct Policy check in Authentication tests as it's now an Authorization responsibility.
        var providers = serviceProvider.GetServices<IVKSemanticSchemeProvider>().ToList();

        // Assert
        providers.Should().NotBeEmpty();
        providers.Any(p => p.GetUserSchemes().Contains("CustomScheme")).Should().BeTrue();
    }

    [Fact]
    public void AddAuthenticationBlock_WhenMultipleFeaturesEnabled_RegistersCombinedPolicies()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var combinedConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "true",
                ["VKBlocks:Authentication:DefaultScheme"] = "Bearer",
                ["VKBlocks:Authentication:Jwt:Enabled"] = "true",
                ["VKBlocks:Authentication:Jwt:Issuer"] = "test-issuer",
                ["VKBlocks:Authentication:Jwt:Audience"] = "test-audience",
                ["VKBlocks:Authentication:Jwt:SecretKey"] = "SuperSecretKeyThatIsLongEnoughToPassValidation",
                ["VKBlocks:Authentication:ApiKey:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:ClientId"] = "test-id",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:ClientSecret"] = "test-secret",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:Authority"] = "https://github.com",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:CallbackPath"] = "/cb"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(combinedConfig)
            .AddVKJwt()
            .AddVKApiKeys()
            .AddVKOAuth();
        using var serviceProvider = _services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IVKSemanticSchemeProvider>().ToList();

        // Assert
        providers.Should().HaveCount(3); // Jwt, ApiKey, OAuth
    }

    [Fact]
    public void AddAuthenticationBlock_WithOAuthProviders_ShouldRegisterIndividualPolicies()
    {
        // Arrange
        _services.RegisterCoreBlock();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VKBlocks:Authentication:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:Enabled"] = "true",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:ClientId"] = "id",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:ClientSecret"] = "secret",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:Authority"] = "auth",
                ["VKBlocks:Authentication:OAuth:Providers:GitHub:CallbackPath"] = "/cb"
            })
            .Build();

        // Act
        _services.AddVKAuthenticationBlock(config).AddVKOAuth();
        using var serviceProvider = _services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IVKSemanticSchemeProvider>().ToList();

        // Assert
        var provider = providers.Should().ContainSingle(p => p.GetType().Name.Contains("OAuth")).Subject;
        provider.GetUserSchemes().Should().Contain("GitHub");
    }

    [Fact]
    public void BuilderExtensions_WithJwtTokenRevocationProvider_ShouldRegisterAsSingleton()
    {
        // Arrange
        _services.RegisterCoreBlock();

        // Act
        _services.AddVKAuthenticationBlock(_configuration)
            .WithJwtTokenRevocationProvider<TestJwtTokenRevocationProvider>();

        using var serviceProvider = _services.BuildServiceProvider();
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IVKJwtRevocationProvider));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationType.Should().Be<TestJwtTokenRevocationProvider>();
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
        // Minimal registration to satisfy IsVKBlockRegistered<VKCoreBlock>
        services.AddVKBlockMarker<VKCoreBlock>();
    }
}

// Test Dummies
internal sealed class TestJwtRefreshTokenValidator : IVKJwtRefreshValidator
{
    public ValueTask<VKResult<bool>> ValidateTokenRotationAsync(string tokenJti, string familyId, CancellationToken ct = default)
        => ValueTask.FromResult(VKResult.Success(true));
}

internal sealed class TestApiKeyRateLimiter : IVKApiKeyRateLimiter
{
    public ValueTask<bool> IsAllowedAsync(Guid keyId, int limit, int windowSeconds, CancellationToken ct = default)
        => ValueTask.FromResult(true);
}

internal sealed class TestJwtTokenRevocationProvider : IVKJwtRevocationProvider
{
    public ValueTask<bool> IsRevokedAsync(string jti, CancellationToken ct = default) => ValueTask.FromResult(false);
    public ValueTask RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default) => ValueTask.CompletedTask;
    public ValueTask<bool> IsUserRevokedAsync(string userId, CancellationToken ct = default) => ValueTask.FromResult(false);
    public ValueTask RevokeUserAsync(string userId, TimeSpan ttl, CancellationToken ct = default) => ValueTask.CompletedTask;
}

internal sealed class TestApiKeyRevocationProvider : IVKApiKeyRevocationProvider
{
    public ValueTask<bool> IsRevokedAsync(string keyId, CancellationToken ct = default) => ValueTask.FromResult(false);
    public ValueTask RevokeAsync(string keyId, TimeSpan ttl, CancellationToken ct = default) => ValueTask.CompletedTask;
}

internal sealed class TestOAuthMapper : IVKOAuthClaimsMapper
{
    public IEnumerable<System.Security.Claims.Claim> MapToClaims(VKExternalIdentity userInfo) => Enumerable.Empty<System.Security.Claims.Claim>();
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

// Trigger Source Generator for GitHub provider policy
[VKOAuthProvider("GitHub")]
internal sealed class GitHubProviderMarker;
