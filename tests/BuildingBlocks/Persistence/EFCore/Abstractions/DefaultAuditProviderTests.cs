using System;
using FluentAssertions;
using VK.Blocks.Persistence.EFCore.Auditing;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.Tests.Abstractions;

/// <summary>
/// Unit tests for <see cref="DefaultAuditProvider"/>.
/// </summary>
public class DefaultAuditProviderTests
{
    /// <summary>
    /// Verifies that the constructor correctly initializes the default properties.
    /// </summary>
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange & Act
        var provider = new DefaultAuditProvider();

        // Assert
        provider.CurrentUserId.Should().Be("System");
        provider.CurrentUserName.Should().Be("System");
        provider.IsAuthenticated.Should().BeFalse();
        provider.UtcNow.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
