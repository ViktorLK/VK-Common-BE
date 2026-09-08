using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Resilience.Internal;
using VK.Blocks.Core;
using VK.Blocks.Resilience;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Resilience;

/// <summary>
/// Unit tests for <see cref="LocalAIResilienceProvider"/>.
/// Follows AP.01, CS.01, CS.03, and DL.01.
/// </summary>
public sealed class LocalAIResilienceProviderTests
{
    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan span) => _now = _now.Add(span);
    }

    private readonly Mock<IVKCircuitBreaker> _circuitBreakerMock = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly VKAIResilienceOptions _options = new()
    {
        ProviderCooldown = TimeSpan.FromSeconds(30),
        DefaultRateLimitRetryDelay = TimeSpan.FromMilliseconds(5)
    };
    private readonly LocalAIResilienceProvider _provider;

    public LocalAIResilienceProviderTests()
    {
        _provider = new LocalAIResilienceProvider(_circuitBreakerMock.Object, _options, _timeProvider);
    }

    [Fact]
    public async Task ExecuteWithProviderFallbackAsync_WhenPrimarySucceeds_ReturnsPrimaryResult()
    {
        // Arrange
        _circuitBreakerMock.Setup(c => c.IsAllowed("ai:provider:openai")).Returns(true);

        // Act
        var result = await _provider.ExecuteWithProviderFallbackAsync<string>(
            "openai",
            "gemini",
            (providerName, _) => Task.FromResult(VKResult.Success($"Response from {providerName}")));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Response from openai");
        _circuitBreakerMock.Verify(c => c.RecordSuccess("ai:provider:openai", null, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteWithProviderFallbackAsync_WhenPrimaryFails_FailsOverToFallbackProvider()
    {
        // Arrange
        _circuitBreakerMock.Setup(c => c.IsAllowed("ai:provider:openai")).Returns(true);
        _circuitBreakerMock.Setup(c => c.IsAllowed("ai:provider:gemini")).Returns(true);

        // Act
        var result = await _provider.ExecuteWithProviderFallbackAsync<string>(
            "openai",
            "gemini",
            (providerName, _) =>
            {
                if (providerName == "openai")
                {
                    return Task.FromResult(VKResult.Failure<string>(new VKError("OpenAI.500", "Server Error")));
                }
                return Task.FromResult(VKResult.Success("Response from gemini"));
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Response from gemini");
        _circuitBreakerMock.Verify(c => c.RecordFailure("ai:provider:openai", It.IsAny<Exception>(), _options.ProviderCooldown, null, null, null), Times.Once);
        _circuitBreakerMock.Verify(c => c.RecordSuccess("ai:provider:gemini", null, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteWithProviderFallbackAsync_WhenBothProvidersFail_ReturnsFailure()
    {
        // Arrange
        _circuitBreakerMock.Setup(c => c.IsAllowed("ai:provider:openai")).Returns(true);

        // Act
        var result = await _provider.ExecuteWithProviderFallbackAsync<string>(
            "openai",
            "gemini",
            (providerName, _) => Task.FromResult(VKResult.Failure<string>(new VKError($"{providerName}.Error", "Failed"))));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Code.Should().Be("gemini.Error");
    }

    [Fact]
    public async Task ExecuteWithProviderFallbackAsync_WhenPrimaryCircuitOpen_DirectlyInvokesFallback()
    {
        // Arrange
        _circuitBreakerMock.Setup(c => c.IsAllowed("ai:provider:openai")).Returns(false);

        // Act
        var result = await _provider.ExecuteWithProviderFallbackAsync<string>(
            "openai",
            "gemini",
            (providerName, _) => Task.FromResult(VKResult.Success($"Response from {providerName}")));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Response from gemini");
        _circuitBreakerMock.Verify(c => c.RecordSuccess("ai:provider:gemini", null, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteWithModelFallbackAsync_WhenPrimaryModelSucceeds_ReturnsPrimaryResult()
    {
        // Act
        var result = await _provider.ExecuteWithModelFallbackAsync<string>(
            "gpt-4",
            "gpt-4o-mini",
            (modelId, _) => Task.FromResult(VKResult.Success($"Generated with {modelId}")));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Generated with gpt-4");
    }

    [Fact]
    public async Task ExecuteWithModelFallbackAsync_WhenPrimaryModelFails_DowngradesToFallbackModel()
    {
        // Act
        var result = await _provider.ExecuteWithModelFallbackAsync<string>(
            "gpt-4",
            "gpt-4o-mini",
            (modelId, _) =>
            {
                if (modelId == "gpt-4")
                {
                    return Task.FromResult(VKResult.Failure<string>(new VKError("Model.Overload", "Overloaded")));
                }
                return Task.FromResult(VKResult.Success($"Generated with {modelId}"));
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Generated with gpt-4o-mini");
    }

    [Fact]
    public async Task ExecuteWithRateLimitRetryAsync_WhenFirstAttemptSucceeds_ReturnsSuccess()
    {
        // Act
        var result = await _provider.ExecuteWithRateLimitRetryAsync<string>(
            _ => Task.FromResult(VKResult.Success("Success")));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success");
    }

    [Fact]
    public async Task ExecuteWithRateLimitRetryAsync_WhenRateLimited_RetriesWithBackoffAndSucceeds()
    {
        // Arrange
        int callCount = 0;

        // Act
        var result = await _provider.ExecuteWithRateLimitRetryAsync<string>(_ =>
        {
            callCount++;
            if (callCount < 3)
            {
                return Task.FromResult(VKResult.Failure<string>(new VKError("AI.429", "Too Many Requests")));
            }
            return Task.FromResult(VKResult.Success("Success on attempt 3"));
        }, maxRetries: 3, defaultRetryAfter: TimeSpan.FromMilliseconds(5));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success on attempt 3");
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteWithRateLimitRetryAsync_WhenNonRateLimitError_DoesNotRetry()
    {
        // Arrange
        int callCount = 0;

        // Act
        var result = await _provider.ExecuteWithRateLimitRetryAsync<string>(_ =>
        {
            callCount++;
            return Task.FromResult(VKResult.Failure<string>(new VKError("AI.500", "Internal Server Error")));
        }, maxRetries: 3);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Code.Should().Be("AI.500");
        callCount.Should().Be(1);
    }
}
