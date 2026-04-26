using FluentAssertions;

namespace VK.Blocks.Authentication.UnitTests.OAuth.Internal;

public sealed class VKOAuthErrorsTests
{
    [Fact]
    public void Errors_ShouldNotBeNull()
    {
        // Assert
        VKOAuthErrors.ProviderNotSupported.Should().NotBeNull();
        VKOAuthErrors.InvalidState.Should().NotBeNull();
        VKOAuthErrors.RemoteFailure.Should().NotBeNull();
        VKOAuthErrors.MissingRequiredClaim.Should().NotBeNull();
        VKOAuthErrors.MapperNotFound.Should().NotBeNull();
        VKOAuthErrors.UserInfoFailure.Should().NotBeNull();
    }
}
