using System;
using FluentAssertions;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Security.Internal;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Security;

/// <summary>
/// Unit tests for <see cref="DefaultConnectionValidator"/>.
/// Follows AP.01, CS.01, and DL.01.
/// </summary>
public sealed class DefaultConnectionValidatorTests
{
    private readonly DefaultConnectionValidator _validator = new();
    private readonly VKTenantId _tenantId = new(Guid.NewGuid());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateEndpoint_WithNullOrEmpty_ReturnsSuccess(string? endpoint)
    {
        // Act
        var result = _validator.ValidateEndpoint(_tenantId, endpoint);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://api.openai.com/v1")]
    [InlineData("https://models.inference.ai.azure.com")]
    [InlineData("https://api.anthropic.com/v1")]
    public void ValidateEndpoint_WithPublicValidUrls_ReturnsSuccess(string endpoint)
    {
        // Act
        var result = _validator.ValidateEndpoint(_tenantId, endpoint);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("http://localhost:8080")]
    [InlineData("http://127.0.0.1:5000")]
    [InlineData("http://10.0.0.1/ai")]
    [InlineData("http://192.168.1.100/v1")]
    public void ValidateEndpoint_WithInternalOrLoopbackUrls_ReturnsUnauthorized(string endpoint)
    {
        // Act
        var result = _validator.ValidateEndpoint(_tenantId, endpoint);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(VKAISynapseErrors.UnauthorizedConnection.Code);
    }

    [Fact]
    public void ValidateEndpoint_WithInvalidUriFormat_ReturnsFailure()
    {
        // Act
        var result = _validator.ValidateEndpoint(_tenantId, "not-a-valid-uri");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be(VKAISynapseErrors.InvalidEndpoint.Code);
    }

    [Fact]
    public void ValidateApiKey_WithNullOrEmpty_ReturnsSuccess()
    {
        // Act & Assert
        _validator.ValidateApiKey(_tenantId, null).IsSuccess.Should().BeTrue();
        _validator.ValidateApiKey(_tenantId, new VKSensitiveString("")).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateApiKey_WithValidKey_ReturnsSuccess()
    {
        // Act
        var result = _validator.ValidateApiKey(_tenantId, new VKSensitiveString("sk-valid-key"));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
