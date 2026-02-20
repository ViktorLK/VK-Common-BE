using System.Text;
using System.Text.Json;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Persistence.EFCore.Infrastructure;

/// <summary>
/// A lightweight cursor serializer for development and testing environments.
/// Encodes cursor values as Base64-encoded JSON without any signature or expiry.
/// </summary>
/// <remarks>
/// ⚠�E�E<b>Do not use in production.</b> This implementation provides no tamper protection.
/// Use <see cref="SecureCursorSerializer"/> for production deployments.
/// </remarks>
public sealed class SimpleCursorSerializer : ICursorSerializer
{
    #region Public Methods

    /// <inheritdoc />
    public string Serialize<T>(T cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
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
            var bytes = Convert.FromBase64String(token);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return default;
        }
    }

    #endregion
}
