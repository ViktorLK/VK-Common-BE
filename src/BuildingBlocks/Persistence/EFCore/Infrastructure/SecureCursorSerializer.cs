using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VK.Blocks.Persistence.Abstractions.Pagination;
using VK.Blocks.Persistence.EFCore.Options;

namespace VK.Blocks.Persistence.EFCore.Infrastructure;

/// <summary>
/// A production-grade cursor serializer that uses HMAC-SHA256 signing,
/// schema versioning, and optional expiry to protect cursor tokens.
/// </summary>
/// <remarks>
/// Token format: <c>Base64(json_payload) + "." + Base64(hmac_signature)</c>
/// <br/>
/// The JSON payload contains:
/// <list type="bullet">
///   <item><description><c>v</c> — schema version (for forward compatibility)</description></item>
///   <item><description><c>d</c> — the cursor data</description></item>
///   <item><description><c>exp</c> — optional expiry timestamp (Unix seconds)</description></item>
/// </list>
/// </remarks>
public sealed class SecureCursorSerializer : ICursorSerializer
{
    #region Fields

    private readonly byte[] _signingKey;
    private readonly TimeSpan? _defaultExpiry;
    private const int _currentVersion = 1;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="SecureCursorSerializer"/>.
    /// </summary>
    /// <param name="options">The serializer configuration options.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="CursorSerializerOptions.SigningKey"/> is null or empty.
    /// </exception>
    public SecureCursorSerializer(IOptions<CursorSerializerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var signingKey = options.Value.SigningKey;
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                $"A non-empty signing key must be configured under '{CursorSerializerOptions.SectionName}:{nameof(CursorSerializerOptions.SigningKey)}'.");
        }

        _signingKey = Encoding.UTF8.GetBytes(signingKey);
        _defaultExpiry = options.Value.DefaultExpiry;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public string Serialize<T>(T value)
    {
        long? expiresAt = _defaultExpiry.HasValue
            ? DateTimeOffset.UtcNow.Add(_defaultExpiry.Value).ToUnixTimeSeconds()
            : null;

        var payload = new CursorPayload<T>
        {
            Version = _currentVersion,
            Data = value,
            ExpiresAt = expiresAt
        };

        var json = JsonSerializer.Serialize(payload);
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var signature = ComputeHmac(jsonBytes);

        return $"{Convert.ToBase64String(jsonBytes)}.{Convert.ToBase64String(signature)}";
    }

    /// <inheritdoc />
    public T? Deserialize<T>(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return default;
        }

        try
        {
            var dotIndex = token.LastIndexOf('.');
            if (dotIndex < 0)
            {
                return default;
            }

            var jsonBytes = Convert.FromBase64String(token[..dotIndex]);
            var providedSignature = Convert.FromBase64String(token[(dotIndex + 1)..]);
            var expectedSignature = ComputeHmac(jsonBytes);

            // Timing-safe comparison to prevent timing attacks.
            if (!CryptographicOperations.FixedTimeEquals(providedSignature, expectedSignature))
            {
                return default;
            }

            var payload = JsonSerializer.Deserialize<CursorPayload<T>>(jsonBytes);
            if (payload is null)
            {
                return default;
            }

            // Reject tokens from a different schema version.
            if (payload.Version != _currentVersion)
            {
                return default;
            }

            // Reject expired tokens.
            if (payload.ExpiresAt.HasValue &&
                payload.ExpiresAt.Value < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return default;
            }

            return payload.Data;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            // Treat malformed tokens as absent cursors; do not propagate.
            return default;
        }
    }

    #endregion

    #region Private Methods

    private byte[] ComputeHmac(byte[] data)
    {
        using var hmac = new HMACSHA256(_signingKey);
        return hmac.ComputeHash(data);
    }

    #endregion

    #region Nested Types

    /// <summary>Internal payload structure embedded in the token.</summary>
    private sealed class CursorPayload<T>
    {
        /// <summary>Schema version for forward compatibility.</summary>
        public int Version { get; init; }

        /// <summary>The cursor value.</summary>
        public required T Data { get; init; }

        /// <summary>Optional expiry as Unix timestamp (seconds). Null means no expiry.</summary>
        public long? ExpiresAt { get; init; }
    }

    #endregion
}
