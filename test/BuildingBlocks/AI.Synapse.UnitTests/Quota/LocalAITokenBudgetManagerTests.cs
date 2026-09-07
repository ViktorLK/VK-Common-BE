using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VK.Blocks.AI.Synapse;
using VK.Blocks.AI.Synapse.Internal;
using Xunit;

namespace VK.Blocks.AI.Synapse.UnitTests.Quota;

/// <summary>
/// Simple custom TimeProvider for unit testing sliding time windows.
/// </summary>
internal sealed class ManualTestTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public ManualTestTimeProvider(DateTimeOffset initialUtcNow)
    {
        _utcNow = initialUtcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Advance(TimeSpan duration)
    {
        _utcNow = _utcNow.Add(duration);
    }
}

/// <summary>
/// Unit tests for <see cref="LocalAITokenBudgetManager"/>.
/// Follows AP.01, CS.01, CS.03, CS.06, and DL.01.
/// </summary>
public sealed class LocalAITokenBudgetManagerTests
{
    [Fact]
    public async Task AcquireTokensAsync_WhenBudgetDisabled_ReturnsSuccess()
    {
        // Arrange
        var options = new VKQuotaOptions { EnableTokenBudget = false };
        var manager = new LocalAITokenBudgetManager(options);

        // Act
        var result = await manager.AcquireTokensAsync("tenant-1", 1_000_000, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AcquireTokensAsync_WhenTokensRequestedUnderLimit_ReturnsSuccess()
    {
        // Arrange
        var options = new VKQuotaOptions
        {
            EnableTokenBudget = true,
            DefaultTokensPerMinute = 10_000
        };
        var manager = new LocalAITokenBudgetManager(options);

        // Act
        var result = await manager.AcquireTokensAsync("tenant-1", 5_000, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AcquireTokensAsync_WhenTokensExceedLimit_ReturnsFailure()
    {
        // Arrange
        var options = new VKQuotaOptions
        {
            EnableTokenBudget = true,
            DefaultTokensPerMinute = 10_000
        };
        var manager = new LocalAITokenBudgetManager(options);

        // Act
        var firstAcquire = await manager.AcquireTokensAsync("tenant-1", 8_000, CancellationToken.None);
        var secondAcquire = await manager.AcquireTokensAsync("tenant-1", 3_000, CancellationToken.None);

        // Assert
        firstAcquire.IsSuccess.Should().BeTrue();
        secondAcquire.IsFailure.Should().BeTrue();
        secondAcquire.FirstError.Code.Should().Be(VKAISynapseErrors.RateLimitExceeded.Code);
    }

    [Fact]
    public async Task RecordUsageAsync_And_SlideWindow_ResetsMinuteWindow()
    {
        // Arrange
        var fakeTime = new ManualTestTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var options = new VKQuotaOptions
        {
            EnableTokenBudget = true,
            DefaultTokensPerMinute = 10_000
        };
        var manager = new LocalAITokenBudgetManager(options, fakeTime);

        // Act - record usage near limit
        var recResult = await manager.RecordUsageAsync("tenant-1", 9_000, CancellationToken.None);
        recResult.IsSuccess.Should().BeTrue();

        var overLimit = await manager.AcquireTokensAsync("tenant-1", 2_000, CancellationToken.None);
        overLimit.IsFailure.Should().BeTrue();

        // Advance time by 61 seconds into next window
        fakeTime.Advance(TimeSpan.FromSeconds(61));

        var nextMinuteAcquire = await manager.AcquireTokensAsync("tenant-1", 2_000, CancellationToken.None);

        // Assert
        nextMinuteAcquire.IsSuccess.Should().BeTrue();
    }
}
