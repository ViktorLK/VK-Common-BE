using FluentAssertions;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Features.OAuth.Internal;

namespace VK.Blocks.Authentication.UnitTests.Features.OAuth.Internal;

public sealed class OAuthErrorsTests
{
    [Fact]
    public void Errors_ShouldNotBeNull()
    {
        // Assert
        OAuthErrors.ProviderNotSupported.Should().NotBeNull();
        OAuthErrors.InvalidState.Should().NotBeNull();
        OAuthErrors.RemoteFailure.Should().NotBeNull();
        OAuthErrors.MissingRequiredClaim.Should().NotBeNull();
        OAuthErrors.MapperNotFound.Should().NotBeNull();
        OAuthErrors.UserInfoFailure.Should().NotBeNull();
    }
}
