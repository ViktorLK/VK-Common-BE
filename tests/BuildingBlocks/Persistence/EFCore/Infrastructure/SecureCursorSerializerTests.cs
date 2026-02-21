using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Persistence.EFCore.Infrastructure;
using VK.Blocks.Persistence.EFCore.Options;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

public class SecureCursorSerializerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IOptions<CursorSerializerOptions>> _optionsMock;

    public SecureCursorSerializerTests()
    {
        _fixture = new Fixture();
        _optionsMock = new Mock<IOptions<CursorSerializerOptions>>();
    }

    private SecureCursorSerializer CreateSut(string signingKey = "s3cr3t_k3y_f0r_t3st1ng_purp0s3s_0nly!!", TimeSpan? expiry = null, TimeProvider? timeProvider = null)
    {
        var options = new CursorSerializerOptions
        {
            SigningKey = signingKey,
            DefaultExpiry = expiry
        };

        _optionsMock.Setup(x => x.Value).Returns(options);

        // If no time provider is supplied, use a fake one with a fixed time for stability,
        // unless specific tests behave otherwise. But default behavior was System.
        // Let's default to null to use System (via constructor default) or pass explicit.
        return new SecureCursorSerializer(_optionsMock.Object, timeProvider);
    }

    [Fact]
    public void Serialize_ValidInput_ReturnsTokenWithSignature()
    {
        // Arrange
        var sut = CreateSut();
        var cursor = 123;

        // Act
        var token = sut.Serialize(cursor);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain("."); // Payload . Signature
        var parts = token.Split('.');
        parts.Length.Should().Be(2);
    }

    [Fact]
    public void Deserialize_ValidToken_ReturnsOriginalValue()
    {
        // Arrange
        var sut = CreateSut();
        var originalCursor = _fixture.Create<int>();
        var token = sut.Serialize(originalCursor);

        // Act
        var result = sut.Deserialize<int>(token);

        // Assert
        result.Should().Be(originalCursor);
    }

    [Fact]
    public void Deserialize_TamperedPayload_ReturnsDefault()
    {
        // Arrange
        var sut = CreateSut();
        var token = sut.Serialize(123);
        var parts = token.Split('.');

        // Tamper with the payload (first part)
        // We decode, change data, and re-encode without updating signature
        var jsonBytes = Convert.FromBase64String(parts[0]);
        // Just flipping a bit or changing a char is enough, but let's replace the whole thing to be sure
        // We'll just append a char to the valid base64 string to make it invalid/different
        var tamperedPayload = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        var tamperedToken = $"{tamperedPayload}.{parts[1]}";

        // Act
        var result = sut.Deserialize<int>(tamperedToken);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public void Deserialize_TamperedSignature_ReturnsDefault()
    {
        // Arrange
        var sut = CreateSut();
        var token = sut.Serialize(123);
        var parts = token.Split('.');

        // Act
        // Flip the last character of the signature
        var originalSig = parts[1];
        var tamperedSig = originalSig.Substring(0, originalSig.Length - 1) + (originalSig.EndsWith('A') ? 'B' : 'A');
        var tamperedToken = $"{parts[0]}.{tamperedSig}";

        var result = sut.Deserialize<int>(tamperedToken);

        // Assert
        result.Should().Be(default);
    }


    [Fact]
    public void Deserialize_InvalidFormat_ReturnsDefault()
    {
        // Arrange
        var sut = CreateSut();
        var invalidToken = "not_a_valid_token";

        // Act
        var result = sut.Deserialize<int>(invalidToken);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new SecureCursorSerializer(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptySigningKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _optionsMock.Setup(x => x.Value).Returns(new CursorSerializerOptions { SigningKey = "" });

        // Act
        Action act = () => new SecureCursorSerializer(_optionsMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*SigningKey*");
    }
    [Fact]
    public void Deserialize_ExpiredToken_ReturnsDefault()
    {
        // Arrange
        var fakeTime = new FakeTimeProvider();
        fakeTime.SetUtcNow(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var expiry = TimeSpan.FromMinutes(5);
        var sut = CreateSut(expiry: expiry, timeProvider: fakeTime);

        var token = sut.Serialize(123);

        // Act
        // Advance time beyond expiry
        fakeTime.Advance(expiry.Add(TimeSpan.FromSeconds(1)));
        var result = sut.Deserialize<int>(token);

        // Assert
        result.Should().Be(default);
    }

    private class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;

        public override DateTimeOffset GetUtcNow() => _now;

        public void SetUtcNow(DateTimeOffset now) => _now = now;
        public void Advance(TimeSpan span) => _now = _now.Add(span);
    }
}
