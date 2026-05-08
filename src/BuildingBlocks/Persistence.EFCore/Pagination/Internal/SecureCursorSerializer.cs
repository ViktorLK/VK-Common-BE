using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Pagination.Internal;

/// <summary>
/// A production-grade cursor serializer that uses HMAC-SHA256 signing,
/// schema versioning, and optional expiry to protect cursor tokens.
/// </summary>
/// <remarks>
/// Token format: Base64(json_payload) + "." + Base64(hmac_signature)
/// <br/>
/// The JSON payload contains:
/// <list type="bullet">
///   <item><description>v  Eschema version (for forward compatibility)</description></item>
///   <item><description>d  Ethe cursor data</description></item>
///   <item><description>exp  Eoptional expiry timestamp (Unix seconds)</description></item>
/// </list>
/// </remarks>
internal sealed class SecureCursorSerializer : IVKCursorSerializer
{

    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly byte[] _signingKey;
    private readonly TimeSpan? _defaultExpiry;
    private readonly TimeProvider _timeProvider;
    private const int _currentVersion = 1;



    /// <summary>
    /// Initializes a new instance of <see cref="SecureCursorSerializer"/>.
    /// </summary>
    /// <param name="options">The serializer configuration options.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="timeProvider">The time provider for expiry checks. Defaults to <see cref="TimeProvider.System"/>.</param>
    public SecureCursorSerializer(
        IOptions<VKCursorSerializerOptions> options,
        IVKJsonSerializer jsonSerializer,
        TimeProvider? timeProvider = null)
    {
        VKGuard.NotNull(options);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);

        var signingKey = options.Value.SigningKey;
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                $"A non-empty signing key must be configured under '{VKCursorSerializerOptions.SectionName}:{nameof(VKCursorSerializerOptions.SigningKey)}'.");
        }

        _signingKey = Encoding.UTF8.GetBytes(signingKey);
        _defaultExpiry = options.Value.DefaultExpiry;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }



    /// <inheritdoc />
    public string Serialize<T>(T value)
    {
        long? expiresAt = _defaultExpiry.HasValue
            ? _timeProvider.GetUtcNow().Add(_defaultExpiry.Value).ToUnixTimeSeconds()
            : null;

        var payload = new CursorPayload<T>
        {
            Version = _currentVersion,
            Data = value,
            ExpiresAt = expiresAt
        };

        var json = _jsonSerializer.Serialize(payload);
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

            var payload = _jsonSerializer.Deserialize<CursorPayload<T>>(jsonBytes);
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
                payload.ExpiresAt.Value < _timeProvider.GetUtcNow().ToUnixTimeSeconds())
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



    private byte[] ComputeHmac(byte[] data)
    {
        using var hmac = new HMACSHA256(_signingKey);
        return hmac.ComputeHash(data);
    }



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

}
