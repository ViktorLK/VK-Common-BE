using FluentAssertions;
using VK.Blocks.Authentication.Features.OAuth;

namespace VK.Blocks.Authentication.UnitTests.Features.OAuth;

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
