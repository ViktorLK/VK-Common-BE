using VK.Blocks.Core.Identity.Internal;

namespace VK.Blocks.Core.UnitTests.Identity.Internal;

public class NullUserContextTests
{
    [Fact]
    public void NullUserContext_Properties_ShouldReturnDefaults()
    {
        // Arrange
        var context = new NullUserContext();

        // Assert
        context.UserId.Should().BeNull();
        context.UserName.Should().BeNull();
        context.TenantId.Should().BeNull();
        context.Roles.Should().BeEmpty();
        context.IsAuthenticated.Should().BeFalse();
    }
}
