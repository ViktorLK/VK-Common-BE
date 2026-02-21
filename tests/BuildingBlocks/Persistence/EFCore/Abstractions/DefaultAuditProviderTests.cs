using FluentAssertions;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.EFCore.Auditing;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.Tests.Abstractions;

public class DefaultAuditProviderTests
{
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
