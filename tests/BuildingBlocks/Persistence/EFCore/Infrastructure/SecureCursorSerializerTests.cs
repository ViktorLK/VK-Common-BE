using System;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Persistence.EFCore.Infrastructure;
using VK.Blocks.Persistence.EFCore.Options;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="SecureCursorSerializer"/>.
/// </summary>
public class SecureCursorSerializerTests
{
    private readonly IFixture _fixture;

    private readonly Mock<IOptions<CursorSerializerOptions>> _optionsMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureCursorSerializerTests"/> class.
    /// </summary>
    public SecureCursorSerializerTests()
    {
        _fixture = new Fixture();
        _optionsMock = new Mock<IOptions<CursorSerializerOptions>>();
    }

    /// <summary>
    /// Helper method to create the system under test with specific options.
    /// </summary>
    private SecureCursorSerializer CreateSut(string signingKey = "s3cr3t_k3y_f0r_t3st1ng_purp0s3s_0nly!!", TimeSpan? expiry = null, TimeProvider? timeProvider = null)
    {
        var options = new CursorSerializerOptions
        {
            SigningKey = signingKey,
            DefaultExpiry = expiry
        };

        _optionsMock.Setup(x => x.Value).Returns(options);

        // Rationale: If no time provider is supplied, the serializer defaults to System time.
        return new SecureCursorSerializer(_optionsMock.Object, timeProvider);
    }

    /// <summary>
    /// Verifies that <see cref="SecureCursorSerializer.Serialize{T}"/> returns a valid token with a signature.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="SecureCursorSerializer.Deserialize{T}"/> correctly recovers the original value from a valid token.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="SecureCursorSerializer.Deserialize{T}"/> returns default when the payload is tampered with.
    /// </summary>
    [Fact]
    public void Deserialize_TamperedPayload_ReturnsDefault()
    {
        // Arrange
        var sut = CreateSut();
        var token = sut.Serialize(123);
        var parts = token.Split('.');

        // Rationale: Tamper with the payload (first part) to invalidate the signature.
        var tamperedPayload = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        var tamperedToken = $"{tamperedPayload}.{parts[1]}";

        // Act
        var result = sut.Deserialize<int>(tamperedToken);

        // Assert
        result.Should().Be(default);
    }

    /// <summary>
    /// Verifies that <see cref="SecureCursorSerializer.Deserialize{T}"/> returns default when the signature is tampered with.
    /// </summary>
    [Fact]
    public void Deserialize_TamperedSignature_ReturnsDefault()
    {
        // Arrange
        var sut = CreateSut();
        var token = sut.Serialize(123);
        var parts = token.Split('.');

        // Act
        // Rationale: Flip the last character of the signature to simulate tampering.
        var originalSig = parts[1];
        var tamperedSig = originalSig.Substring(0, originalSig.Length - 1) + (originalSig.EndsWith('A') ? 'B' : 'A');
        var tamperedToken = $"{parts[0]}.{tamperedSig}";

        var result = sut.Deserialize<int>(tamperedToken);

        // Assert
        result.Should().Be(default);
    }

    /// <summary>
    /// Verifies that <see cref="SecureCursorSerializer.Deserialize{T}"/> returns default for tokens with an invalid format.
    /// </summary>
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

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when options are null.
    /// </summary>
    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new SecureCursorSerializer(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="InvalidOperationException"/> when the signing key is empty.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="SecureCursorSerializer.Deserialize{T}"/> returns default for expired tokens.
    /// </summary>
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
        // Rationale: Advance time beyond the expiry duration.
        fakeTime.Advance(expiry.Add(TimeSpan.FromSeconds(1)));
        var result = sut.Deserialize<int>(token);

        // Assert
        result.Should().Be(default);
    }

    /// <summary>
    /// A fake time provider used for testing expiration logic.
    /// </summary>
    private class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;

        /// <inheritdoc />
        public override DateTimeOffset GetUtcNow() => _now;

        /// <summary>
        /// Sets the current UTC time.
        /// </summary>
        public void SetUtcNow(DateTimeOffset now) => _now = now;

        /// <summary>
        /// Advances the current UTC time by the specified duration.
        /// </summary>
        public void Advance(TimeSpan span) => _now = _now.Add(span);
    }
}
